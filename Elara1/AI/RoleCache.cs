using Elara1.DataAccess;
using Elara1.DataAccess.History;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;

namespace Elara1.AI
{
    // Caches the handful of Role rows (Assistant/User/System/Tool/Unknown) for the life of the
    // process, loading -- and creating any missing ones -- only once, instead of looking one up
    // or inserting a fresh duplicate for every message.
    internal class RoleCache
    {
        private static readonly string[] KnownNames = ["Assistant", "User", "System", "Tool", "Unknown"];

        private readonly IDbContextFactory<ElaraDbContext> _dbContextFactory;
        private readonly SemaphoreSlim _lock = new(1, 1);
        private Dictionary<string, Role>? _rolesByName;

        public RoleCache(IDbContextFactory<ElaraDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<Role> ResolveAsync(ChatRole chatRole)
        {
            var roles = await GetRolesAsync();
            return roles[ToName(chatRole)];
        }

        private static string ToName(ChatRole chatRole)
        {
            if (chatRole == ChatRole.Assistant) return "Assistant";
            if (chatRole == ChatRole.User) return "User";
            if (chatRole == ChatRole.System) return "System";
            if (chatRole == ChatRole.Tool) return "Tool";
            return "Unknown";
        }

        private async Task<Dictionary<string, Role>> GetRolesAsync()
        {
            if (_rolesByName != null) return _rolesByName;

            await _lock.WaitAsync();
            try
            {
                _rolesByName ??= await LoadOrCreateAsync();
                return _rolesByName;
            }
            finally
            {
                _lock.Release();
            }
        }

        private async Task<Dictionary<string, Role>> LoadOrCreateAsync()
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            var existing = await db.Roles
                .Where(r => KnownNames.Contains(r.Name))
                .ToListAsync();

            // If earlier runs left duplicate rows for a name, just pin one -- this doesn't
            // clean the table up, but it stops adding to the problem.
            var byName = existing
                .GroupBy(r => r.Name!)
                .ToDictionary(g => g.Key, g => g.First());

            foreach (var name in KnownNames)
            {
                if (!byName.ContainsKey(name))
                {
                    var role = new Role { Name = name };
                    db.Roles.Add(role);
                    byName[name] = role;
                }
            }

            await db.SaveChangesAsync();
            return byName;
        }
    }
}
