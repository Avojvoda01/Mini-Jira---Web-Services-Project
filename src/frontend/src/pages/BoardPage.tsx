import { useRef, useState } from 'react';

type ChatMessage = {
  id: number;
  role: 'user' | 'assistant';
  text: string;
};

export function BoardPage() {
  const [isChatOpen, setIsChatOpen] = useState(false);
  const [input, setInput] = useState('');
  const nextMessageIdRef = useRef(2);
  const [messages, setMessages] = useState<ChatMessage[]>([
    {
      id: 1,
      role: 'assistant',
      text: 'AI helper is ready. Later this will connect to a real assistant endpoint.',
    },
  ]);

  const sendMessage = () => {
    const text = input.trim();
    if (!text) {
      return;
    }

    const userMessageId = nextMessageIdRef.current;
    nextMessageIdRef.current += 1;
    const assistantMessageId = nextMessageIdRef.current;
    nextMessageIdRef.current += 1;

    setMessages((current) => [
      ...current,
      { id: userMessageId, role: 'user', text },
      {
        id: assistantMessageId,
        role: 'assistant',
        text: 'Thanks. This is a placeholder response from the board chat scaffold.',
      },
    ]);
    setInput('');
  };

  return (
    <section className="panel board-panel">
      <h2 className="panel-title">Board</h2>
      <p className="panel-description">
        Kanban columns and drag-drop task workflow will be implemented in this view.
      </p>

      <button
        type="button"
        className="ai-chat-toggle"
        aria-expanded={isChatOpen}
        aria-controls="ai-chat-panel"
        onClick={() => setIsChatOpen((open) => !open)}
      >
        {isChatOpen ? 'Close AI' : 'AI'}
      </button>

      {isChatOpen && (
        <aside id="ai-chat-panel" className="ai-chat-panel" aria-label="AI board chat">
          <div className="ai-chat-header">AI Assistant</div>

          <div className="ai-chat-messages">
            {messages.map((message) => (
              <p key={message.id} className={`ai-chat-message ${message.role === 'user' ? 'user' : 'assistant'}`}>
                {message.text}
              </p>
            ))}
          </div>

          <div className="ai-chat-input-row">
            <input
              className="ai-chat-input"
              type="text"
              value={input}
              placeholder="Ask for grouping/help..."
              onChange={(event) => setInput(event.target.value)}
              onKeyDown={(event) => {
                if (event.key === 'Enter') {
                  sendMessage();
                }
              }}
            />
            <button type="button" className="button button-primary" onClick={sendMessage}>
              Send
            </button>
          </div>
        </aside>
      )}
    </section>
  );
}
