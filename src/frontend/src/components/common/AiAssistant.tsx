import { useEffect, useRef, useState } from 'react';
import { useAtom, useAtomValue } from 'jotai';
import { atomWithStorage, createJSONStorage } from 'jotai/utils';
import { Bot, RotateCcw, SendHorizontal, X } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { cn } from '@/lib/utils';
import { authSessionAtom } from '@/store/authAtoms';

type ChatMessage = {
  id: number;
  role: 'user' | 'assistant';
  text: string;
  timestamp: number;
};

const formatTime = (timestamp: number) =>
  new Date(timestamp).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });

const messagesStorage = createJSONStorage<ChatMessage[]>(() => sessionStorage);
const messagesAtom = atomWithStorage<ChatMessage[]>('mini-jira.assistant.messages', [], messagesStorage, {
  getOnInit: true,
});

const draftStorage = createJSONStorage<string>(() => sessionStorage);
const draftAtom = atomWithStorage<string>('mini-jira.assistant.draft', '', draftStorage, { getOnInit: true });

export function AiAssistant({ greeting, placeholder }: { greeting: string; placeholder: string }) {
  const [isOpen, setIsOpen] = useState(false);
  const [input, setInput] = useAtom(draftAtom);
  const [messages, setMessages] = useAtom(messagesAtom);
  const [isSending, setIsSending] = useState(false);
  const session = useAtomValue(authSessionAtom);
  const messagesRef = useRef<HTMLDivElement | null>(null);
  const containerRef = useRef<HTMLDivElement | null>(null);

  useEffect(() => {
    if (isOpen) {
      messagesRef.current?.scrollTo({ top: messagesRef.current.scrollHeight, behavior: 'smooth' });
    }
  }, [messages, isOpen]);

  useEffect(() => {
    if (!isOpen) {
      return;
    }

    const handlePointerDown = (event: PointerEvent) => {
      if (containerRef.current && !containerRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    };

    document.addEventListener('pointerdown', handlePointerDown);
    return () => document.removeEventListener('pointerdown', handlePointerDown);
  }, [isOpen]);

  const appendMessage = (role: ChatMessage['role'], text: string) => {
    setMessages((current) => {
      const nextId = current.reduce((max, message) => Math.max(max, message.id), 0) + 1;
      return [...current, { id: nextId, role, text, timestamp: Date.now() }];
    });
  };

  const sendMessage = async () => {
    const text = input.trim();
    if (!text || isSending) {
      return;
    }

    appendMessage('user', text);
    setInput('');
    setIsSending(true);

    try {
      const response = await fetch('/api/chats', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          ...(session?.token ? { Authorization: `Bearer ${session.token}` } : {}),
        },
        body: JSON.stringify({ message: text }),
      });

      const data = (await response.json()) as { answer?: string; message?: string };
      appendMessage('assistant', data.answer ?? data.message ?? 'No response text found.');
    } catch {
      appendMessage('assistant', 'I could not reach the assistant. Please make sure LM Studio is running and try again.');
    } finally {
      setIsSending(false);
    }
  };

  return (
    <div ref={containerRef} className="fixed bottom-5 right-5 z-30 flex flex-col items-end sm:bottom-6 sm:right-6">
      {isOpen ? (
        <div
          id="ai-assistant-window"
          className="mb-3 flex w-[min(92vw,440px)] flex-col overflow-hidden rounded-2xl border border-border bg-card shadow-2xl ring-1 ring-foreground/10"
        >
          <div className="flex items-center justify-between gap-3 border-b border-border bg-muted/60 px-4 py-3">
            <div className="flex items-center gap-2.5">
              <span className="flex h-9 w-9 items-center justify-center rounded-xl bg-primary text-primary-foreground shadow-sm">
                <Bot className="h-4.5 w-4.5" />
              </span>
              <div>
                <p className="text-sm font-semibold text-foreground">AI Assistant</p>
                <p className="flex items-center gap-1.5 text-xs text-muted-foreground">
                  <span className="h-1.5 w-1.5 rounded-full bg-emerald-500" />
                  Online
                </p>
              </div>
            </div>
            <div className="flex items-center gap-1">
              {messages.length > 0 ? (
                <Button
                  variant="ghost"
                  size="icon"
                  className="h-8 w-8 text-muted-foreground hover:text-foreground"
                  onClick={() => setMessages([])}
                  aria-label="Clear chat history"
                  title="Clear chat history"
                >
                  <RotateCcw className="h-4 w-4" />
                </Button>
              ) : null}
              <Button
                variant="ghost"
                size="icon"
                className="h-8 w-8 text-muted-foreground hover:text-foreground"
                onClick={() => setIsOpen(false)}
                aria-label="Close assistant"
              >
                <X className="h-4 w-4" />
              </Button>
            </div>
          </div>

          <div ref={messagesRef} className="h-[26rem] max-h-[55vh] space-y-3 overflow-y-auto p-4">
            <div className="mr-auto max-w-[85%] rounded-2xl rounded-bl-md border border-border/60 bg-muted px-3.5 py-2.5 text-sm leading-6 text-foreground shadow-sm">
              {greeting}
            </div>
            {messages.map((message) => (
              <div
                key={message.id}
                className={cn('flex max-w-[85%] flex-col gap-1', message.role === 'user' ? 'ml-auto items-end' : 'mr-auto items-start')}
              >
                <div
                  className={cn(
                    'rounded-2xl px-3.5 py-2.5 text-sm leading-6 shadow-sm',
                    message.role === 'user'
                      ? 'rounded-br-md bg-primary text-primary-foreground'
                      : 'rounded-bl-md border border-border/60 bg-muted text-foreground',
                  )}
                >
                  {message.text}
                </div>
                {message.timestamp ? <span className="px-1 text-[11px] text-muted-foreground">{formatTime(message.timestamp)}</span> : null}
              </div>
            ))}
          </div>

          <form
            className="flex items-center gap-2 border-t border-border bg-muted/40 p-3"
            onSubmit={(event) => {
              event.preventDefault();
              void sendMessage();
            }}
          >
            <Input
              value={input}
              placeholder={placeholder}
              onChange={(event) => setInput(event.target.value)}
              disabled={isSending}
              className="bg-background"
            />
            <Button
              type="submit"
              size="icon"
              className="shrink-0 shadow-sm"
              disabled={!input.trim() || isSending}
              aria-label="Send message"
            >
              <SendHorizontal className="h-4 w-4" />
            </Button>
          </form>
        </div>
      ) : null}

      <Button
        type="button"
        aria-expanded={isOpen}
        aria-controls="ai-assistant-window"
        className="rounded-full px-4 shadow-lg"
        onClick={() => setIsOpen((current) => !current)}
      >
        <Bot className="mr-2 h-4 w-4" />
        {isOpen ? 'Close assistant' : 'AI Assistant'}
      </Button>
    </div>
  );
}
