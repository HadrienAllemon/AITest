using Elara1.DataAccess.History;
using Elara1.DataAccess.Memory;
using Microsoft.EntityFrameworkCore;

namespace Elara1.DataAccess
{
    public class ElaraDbContext : DbContext
    {
        public ElaraDbContext(DbContextOptions<ElaraDbContext> options) : base(options)
        {
        }

        public DbSet<MemoryFact> MemoryFacts => Set<MemoryFact>();
        public DbSet<Conversation> Conversations => Set<Conversation>();
        public DbSet<Message> Messages => Set<Message>();
        public DbSet<Role> Roles => Set<Role>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Seeds the same facts MockMemoryStore used to hardcode, so the table has data
            // to query right after the first migration is applied.
            // HasData values are baked into the migration snapshot at "migrations add" time,
            // so they must be static literals -- DateTime.UtcNow here would look "changed"
            // on every future migration and generate spurious UpdateData ops forever.
            var seedCreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            modelBuilder.Entity<MemoryFact>().HasData(
                new MemoryFact { Id = 1, Content = "User works as a software engineer at a tech firm.", CreatedAt = seedCreatedAt },
                new MemoryFact { Id = 2, Content = "User's partner is named Alex.", CreatedAt = seedCreatedAt },
                new MemoryFact { Id = 3, Content = "User tends to feel overwhelmed when deadlines pile up on Fridays.", CreatedAt = seedCreatedAt },
                new MemoryFact { Id = 4, Content = "User's favorite way to relax is going for evening walks.", CreatedAt = seedCreatedAt }
            );

            // Conversation, Message, and Role map to tables that already existed in the DB
            // before EF was involved (see the schema-fix migration), so their shape here has
            // to mirror what's actually in SQL Server rather than what EF's defaults would pick.
            modelBuilder.Entity<Conversation>(entity =>
            {
                entity.ToTable("Conversation");
                entity.Property(e => e.Title).HasMaxLength(50);
            });

            modelBuilder.Entity<Message>(entity =>
            {
                entity.HasOne(m => m.Conversation)
                    .WithMany(c => c.Messages)
                    .HasForeignKey(m => m.ConversationId)
                    .HasConstraintName("FK_Messages_Conversation");

                entity.HasOne(m => m.Role)
                    .WithMany(r => r.Messages)
                    .HasForeignKey(m => m.RoleId)
                    .HasConstraintName("FK_Messages_Roles");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(50);
            });
        }
    }
}
