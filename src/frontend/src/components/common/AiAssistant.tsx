import { useEffect, useRef, useState } from 'react';
import { useAtom } from 'jotai';
import { atomWithStorage, createJSONStorage } from 'jotai/utils';
import { Bot, RotateCcw, SendHorizontal, X } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { cn } from '@/lib/utils';

type ChatMessage = {
  id: number;
  role: 'user' | 'assistant';
  text: string;
};

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
  const messagesRef = useRef<HTMLDivElement | null>(null);

  useEffect(() => {
    if (isOpen) {
      messagesRef.current?.scrollTo({ top: messagesRef.current.scrollHeight, behavior: 'smooth' });
    }
  }, [messages, isOpen]);

  const sendMessage = () => {
    const text = input.trim();
    if (!text) {
      return;
    }

    setMessages((current) => {
      const nextId = current.reduce((max, message) => Math.max(max, message.id), 0) + 1;
      return [
        ...current,
        { id: nextId, role: 'user', text },
        {
          id: nextId + 1,
          role: 'assistant',
          text: 'Noted. This assistant will later connect to a real workflow endpoint.',
        },
      ];
    });
    setInput('');
  };

  return (
    <div className="fixed bottom-5 right-5 z-30 flex flex-col items-end sm:bottom-6 sm:right-6">
      {isOpen ? (
        <div
          id="ai-assistant-window"
          className="mb-3 flex w-[min(92vw,380px)] flex-col overflow-hidden rounded-2xl border border-border bg-card shadow-2xl ring-1 ring-foreground/10"
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

          <div ref={messagesRef} className="h-72 space-y-3 overflow-y-auto p-4">
            <div className="mr-auto max-w-[85%] rounded-2xl rounded-bl-md border border-border/60 bg-muted px-3.5 py-2.5 text-sm leading-6 text-foreground shadow-sm">
              {greeting}
            </div>
            {messages.map((message) => (
              <div
                key={message.id}
                className={cn(
                  'max-w-[85%] rounded-2xl px-3.5 py-2.5 text-sm leading-6 shadow-sm',
                  message.role === 'user'
                    ? 'ml-auto rounded-br-md bg-primary text-primary-foreground'
                    : 'mr-auto rounded-bl-md border border-border/60 bg-muted text-foreground',
                )}
              >
                {message.text}
              </div>
            ))}
          </div>

          <form
            className="flex items-center gap-2 border-t border-border bg-muted/40 p-3"
            onSubmit={(event) => {
              event.preventDefault();
              sendMessage();
            }}
          >
            <Input
              value={input}
              placeholder={placeholder}
              onChange={(event) => setInput(event.target.value)}
              className="bg-background"
            />
            <Button type="submit" size="icon" className="shrink-0 shadow-sm" disabled={!input.trim()} aria-label="Send message">
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
