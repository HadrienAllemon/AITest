import { useState } from 'react'
import type { FormEvent } from 'react'
import './App.css'

interface ChatMessage {
  role: 'user' | 'assistant'
  content: string
}

function App() {
  const [messages, setMessages] = useState<ChatMessage[]>([])
  const [input, setInput] = useState('')
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const sendMessage = async (event: FormEvent) => {
    event.preventDefault()
    const text = input.trim()
    if (!text || isLoading) return

    setMessages((prev) => [...prev, { role: 'user', content: text }])
    setInput('')
    setIsLoading(true)
    setError(null)

    try {
      const res = await fetch('/api/chat', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ message: text }),
      })

      if (!res.ok) {
        throw new Error(`Request failed with status ${res.status}`)
      }

      const data: { reply: string } = await res.json()
      setMessages((prev) => [...prev, { role: 'assistant', content: data.reply }])
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Something went wrong.')
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div id="chat">
      <header id="chat-header">
        <h1>Elara test console</h1>
        <p>Talk to the middleware directly, no console required.</p>
      </header>

      <div id="chat-log">
        {messages.length === 0 && !isLoading && (
          <p id="chat-empty">Send a message to start the conversation.</p>
        )}
        {messages.map((m, i) => (
          <div key={i} className={`bubble ${m.role}`}>
            <span className="bubble-role">{m.role === 'user' ? 'You' : 'Elara'}</span>
            <p>{m.content}</p>
          </div>
        ))}
        {isLoading && (
          <div className="bubble assistant">
            <span className="bubble-role">Elara</span>
            <p className="thinking">Thinking…</p>
          </div>
        )}
      </div>

      {error && <p id="chat-error">{error}</p>}

      <form id="chat-form" onSubmit={sendMessage}>
        <input
          type="text"
          value={input}
          onChange={(e) => setInput(e.target.value)}
          placeholder="Type a message…"
          disabled={isLoading}
          autoFocus
        />
        <button type="submit" disabled={isLoading || !input.trim()}>
          Send
        </button>
      </form>
    </div>
  )
}

export default App
