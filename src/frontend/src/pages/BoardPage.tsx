import { useRef, useState } from 'react';
import { Bot, Plus, SendHorizontal, Sparkles } from 'lucide-react';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Separator } from '@/components/ui/separator';
import { cn } from '@/lib/utils';

type ChatMessage = {
  id: number;
  role: 'user' | 'assistant';
  text: string;
};

type TaskCard = {
  ticket: string;
  title: string;
  owner: string;
  priority: 'High' | 'Medium' | 'Low';
  estimate: string;
};

const boardColumns: Array<{ title: string; count: number; description: string; tasks: TaskCard[] }> = [
  {
    title: 'Backlog',
    count: 5,
    description: 'Ready for triage and sizing.',
    tasks: [
      { ticket: 'MJR-124', title: 'Refine ticket permissions', owner: 'Nina', priority: 'High', estimate: '5 pts' },
      { ticket: 'MJR-131', title: 'Standardize comment composer', owner: 'Ava', priority: 'Medium', estimate: '3 pts' },
    ],
  },
  {
    title: 'In progress',
    count: 4,
    description: 'Actively being implemented.',
    tasks: [
      { ticket: 'MJR-118', title: 'Board drag handles', owner: 'Eli', priority: 'High', estimate: '8 pts' },
      { ticket: 'MJR-127', title: 'Project overview metrics', owner: 'Maya', priority: 'Medium', estimate: '5 pts' },
    ],
  },
  {
    title: 'Done',
    count: 3,
    description: 'Shipped and ready to verify.',
    tasks: [
      { ticket: 'MJR-102', title: 'Auth layout cleanup', owner: 'Leo', priority: 'Low', estimate: '2 pts' },
      { ticket: 'MJR-109', title: 'Responsive nav polish', owner: 'Ava', priority: 'Low', estimate: '1 pt' },
    ],
  },
];

export function BoardPage() {
  const [isAssistantOpen, setIsAssistantOpen] = useState(false);
  const [input, setInput] = useState('');
  const nextMessageIdRef = useRef(2);
  const [messages, setMessages] = useState<ChatMessage[]>([
    {
      id: 1,
      role: 'assistant',
      text: 'I can help sort tasks, spot blockers, or summarize this board once the data is connected.',
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
        text: 'Noted. This board assistant will later connect to a real workflow endpoint.',
      },
    ]);
    setInput('');
  };

  return (
    <section className="relative space-y-6">
      <Card className="border-border/70 bg-card/80 shadow-sm backdrop-blur-sm">
        <CardContent className="p-6 sm:p-8">
          <div className="flex flex-col gap-5 lg:flex-row lg:items-end lg:justify-between">
            <div className="space-y-4">
              <Badge variant="outline" className="w-fit border-border/70 bg-background/70 text-muted-foreground">
                <Sparkles className="mr-1.5 h-3.5 w-3.5" />
                Board view
              </Badge>

              <div className="space-y-2">
                <h2 className="text-3xl font-semibold tracking-tight text-foreground sm:text-4xl">Board</h2>
                <p className="max-w-2xl text-sm leading-6 text-muted-foreground sm:text-base">
                  A structured Kanban surface for prioritization, flow management, and delivery review.
                </p>
              </div>

              <div className="flex flex-wrap gap-2">
                <Badge variant="secondary" className="border border-border/60 bg-background/80 text-foreground">
                  12 tickets active
                </Badge>
                <Badge variant="outline" className="border-border/70 bg-background/60 text-muted-foreground">
                  2 blockers
                </Badge>
                <Badge variant="outline" className="border-border/70 bg-background/60 text-muted-foreground">
                  3 columns
                </Badge>
              </div>
            </div>

            <div className="flex flex-wrap gap-2">
              <Button variant="outline" className="border-border/70 bg-background/80 shadow-sm">
                <Plus className="mr-2 h-4 w-4" />
                Add ticket
              </Button>
              <Button className="shadow-sm">Review sprint</Button>
            </div>
          </div>
        </CardContent>
      </Card>

      <div className="grid gap-4 lg:grid-cols-3">
        {boardColumns.map((column) => (
          <Card key={column.title} className="border-border/70 bg-card/80 shadow-sm backdrop-blur-sm">
            <CardHeader className="space-y-3 pb-4">
              <div className="flex items-start justify-between gap-3">
                <div>
                  <CardTitle>{column.title}</CardTitle>
                  <CardDescription className="mt-1">{column.description}</CardDescription>
                </div>
                <Badge variant="outline" className="border-border/70 bg-background/70 text-muted-foreground">
                  {column.count}
                </Badge>
              </div>
              <Separator />
            </CardHeader>

            <CardContent className="space-y-3">
              {column.tasks.map((task, index) => (
                <div key={task.ticket}>
                  {index > 0 ? <Separator className="mb-3" /> : null}
                  <article className="rounded-2xl border border-border/70 bg-background/80 p-4 shadow-sm transition-shadow hover:shadow-md">
                    <div className="flex items-start justify-between gap-3">
                      <div className="space-y-2">
                        <Badge variant="outline" className="border-border/70 bg-background/70 text-[0.68rem] uppercase tracking-[0.18em] text-muted-foreground">
                          {task.ticket}
                        </Badge>
                        <h3 className="text-sm font-medium leading-6 text-foreground">{task.title}</h3>
                      </div>
                      <Badge
                        className={
                          task.priority === 'High'
                            ? 'bg-rose-500/10 text-rose-700 hover:bg-rose-500/10'
                            : task.priority === 'Medium'
                              ? 'bg-amber-500/10 text-amber-700 hover:bg-amber-500/10'
                              : 'bg-slate-500/10 text-slate-700 hover:bg-slate-500/10'
                        }
                      >
                        {task.priority}
                      </Badge>
                    </div>

                    <div className="mt-4 flex flex-wrap items-center justify-between gap-2 text-xs text-muted-foreground">
                      <span>{task.owner}</span>
                      <span>{task.estimate}</span>
                    </div>
                  </article>
                </div>
              ))}
            </CardContent>
          </Card>
        ))}
      </div>

      <div className="fixed bottom-5 right-5 z-30 sm:bottom-6 sm:right-6">
        {isAssistantOpen ? (
          <Card id="board-ai-chat" className="mb-3 w-[min(92vw,360px)] border-border/70 bg-card/95 shadow-xl backdrop-blur-sm">
            <CardHeader className="space-y-3 pb-4">
              <div className="flex items-start justify-between gap-3">
                <div>
                  <CardTitle className="flex items-center gap-2">
                    <Bot className="h-4 w-4 text-muted-foreground" />
                    AI Assistant
                  </CardTitle>
                  <CardDescription className="mt-1">Useful for summaries, grouping, and quick board questions.</CardDescription>
                </div>
                <Badge variant="secondary" className="border border-border/60 bg-background/80 text-foreground">
                  Ready
                </Badge>
              </div>
              <Separator />
            </CardHeader>

            <CardContent className="space-y-4">
              <div className="max-h-56 space-y-3 overflow-y-auto pr-1">
                {messages.map((message) => (
                  <div
                    key={message.id}
                    className={cn(
                      'max-w-[92%] rounded-2xl border px-3 py-2.5 text-sm leading-6 shadow-sm',
                      message.role === 'user'
                        ? 'ml-auto border-primary/20 bg-primary/8 text-foreground'
                        : 'border-border/70 bg-muted/40 text-foreground',
                    )}
                  >
                    {message.text}
                  </div>
                ))}
              </div>

              <Separator />

              <div className="space-y-3">
                <Input
                  value={input}
                  placeholder="Ask about blockers, priorities, or grouping..."
                  onChange={(event) => setInput(event.target.value)}
                  onKeyDown={(event) => {
                    if (event.key === 'Enter') {
                      sendMessage();
                    }
                  }}
                />
                <Button className="w-full shadow-sm" onClick={sendMessage}>
                  <SendHorizontal className="mr-2 h-4 w-4" />
                  Send
                </Button>
              </div>
            </CardContent>
          </Card>
        ) : null}

        <Button
          type="button"
          aria-expanded={isAssistantOpen}
          aria-controls="board-ai-chat"
          className="rounded-full px-4 shadow-lg"
          onClick={() => setIsAssistantOpen((current) => !current)}
        >
          <Bot className="mr-2 h-4 w-4" />
          {isAssistantOpen ? 'Close assistant' : 'AI Assistant'}
        </Button>
      </div>
    </section>
  );
}
