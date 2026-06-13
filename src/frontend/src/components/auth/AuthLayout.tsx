import type { ReactNode } from 'react';
import { GaugeCircle, Home, KanbanSquare, Layers3, LockKeyhole } from 'lucide-react';
import { Link } from 'react-router-dom';
import { Button } from '@/components/ui/button';
import { ModeToggle } from '@/components/common/ModeToggle';

const highlights = [
  {
    title: 'Flow-based board',
    description: 'Move tickets through Ready, In progress, Review, and Done.',
    icon: KanbanSquare,
  },
  {
    title: 'Epics and backlog',
    description: 'Group related tickets into larger initiatives with estimates.',
    icon: Layers3,
  },
  {
    title: 'Live dashboards',
    description: 'Project health summarized the moment you sign in.',
    icon: GaugeCircle,
  },
];

export function AuthLayout({
  title,
  description,
  children,
}: {
  title: string;
  description: string;
  children: ReactNode;
}) {
  return (
    <main className="flex min-h-screen flex-col">
      <header className="border-b border-border/60 bg-background/75 backdrop-blur-md">
        <div className="mx-auto flex h-16 w-full max-w-6xl items-center justify-between px-4 sm:px-6 lg:px-8">
          <Link to="/" className="flex items-center gap-2.5">
            <span className="flex h-8 w-8 items-center justify-center rounded-lg bg-primary text-primary-foreground">
              <KanbanSquare className="h-4.5 w-4.5" />
            </span>
            <span className="text-base font-semibold tracking-tight text-foreground">Mini Jira</span>
          </Link>
          <div className="flex items-center gap-2">
            <Button
              asChild
              variant="outline"
              size="icon"
              className="border-border/70 bg-background/85 text-foreground shadow-md backdrop-blur-sm hover:bg-background"
            >
              <Link to="/" aria-label="Back to homepage" title="Back to homepage">
                <Home className="h-[1.2rem] w-[1.2rem]" />
              </Link>
            </Button>
            <ModeToggle />
          </div>
        </div>
      </header>

      <div className="mx-auto grid w-full max-w-6xl flex-1 items-center gap-10 px-4 py-12 sm:px-6 lg:grid-cols-[1.05fr_0.95fr] lg:gap-16 lg:px-8">
        <section className="hidden space-y-8 lg:block">
          <div className="space-y-4">
            <h1 className="text-3xl font-semibold tracking-tight text-foreground sm:text-4xl">{title}</h1>
            <p className="max-w-md text-base leading-7 text-muted-foreground">{description}</p>
          </div>

          <ul className="space-y-5">
            {highlights.map((item) => (
              <li key={item.title} className="flex items-start gap-4">
                <span className="flex h-10 w-10 shrink-0 items-center justify-center rounded-xl border border-border/70 bg-background/80">
                  <item.icon className="h-5 w-5 text-muted-foreground" />
                </span>
                <div>
                  <p className="text-sm font-medium text-foreground">{item.title}</p>
                  <p className="text-sm leading-6 text-muted-foreground">{item.description}</p>
                </div>
              </li>
            ))}
          </ul>

          <p className="flex items-center gap-1.5 text-xs text-muted-foreground">
            <LockKeyhole className="h-3.5 w-3.5" />
            Secured with JWT authentication — your workspace stays private.
          </p>
        </section>

        <section className="mx-auto w-full max-w-md lg:max-w-none">{children}</section>
      </div>
    </main>
  );
}
