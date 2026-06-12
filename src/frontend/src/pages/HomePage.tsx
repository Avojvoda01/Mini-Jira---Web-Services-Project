import {
  ArrowRight,
  GaugeCircle,
  KanbanSquare,
  Layers3,
  LockKeyhole,
  Sparkles,
  Users2,
} from 'lucide-react';
import { useAtomValue } from 'jotai';
import { Link } from 'react-router-dom';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { ModeToggle } from '@/components/common/ModeToggle';
import { isAuthenticatedAtom } from '@/store/authAtoms';

const capabilities = [
  {
    title: 'Live project dashboards',
    description: 'Open, in-progress, and completed work summarized the moment you enter a project.',
    icon: GaugeCircle,
  },
  {
    title: 'Flow-based board',
    description: 'Move tickets through Ready, In progress, Review, and Done with drag and drop.',
    icon: KanbanSquare,
  },
  {
    title: 'Epics and backlog',
    description: 'Group related tickets into epics with estimates, priorities, and assignees.',
    icon: Layers3,
  },
  {
    title: 'Teams and ownership',
    description: 'Manage members, transfer project ownership, and control access per project.',
    icon: Users2,
  },
];

function ScreenshotFrame({ src, alt }: { src: string; alt: string }) {
  return (
    <figure className="overflow-hidden rounded-2xl border border-border/70 bg-card/85 shadow-lg backdrop-blur-sm">
      <div className="flex items-center gap-1.5 border-b border-border/60 bg-muted/40 px-4 py-2.5">
        <span className="h-2.5 w-2.5 rounded-full bg-red-400/70" />
        <span className="h-2.5 w-2.5 rounded-full bg-amber-400/70" />
        <span className="h-2.5 w-2.5 rounded-full bg-emerald-400/70" />
      </div>
      <img src={src} alt={alt} loading="lazy" className="w-full" />
    </figure>
  );
}

function FeatureSection({
  badge,
  title,
  description,
  points,
  image,
  imageAlt,
  stacked = false,
}: {
  badge: string;
  title: string;
  description: string;
  points: string[];
  image: string;
  imageAlt: string;
  stacked?: boolean;
}) {
  if (stacked) {
    return (
      <div className="space-y-8">
        <div className="max-w-3xl space-y-4">
          <Badge variant="outline" className="border-border/70 bg-background/70 text-muted-foreground">
            {badge}
          </Badge>
          <h3 className="text-2xl font-semibold tracking-tight text-foreground sm:text-3xl">{title}</h3>
          <p className="text-base leading-7 text-muted-foreground">{description}</p>
          <ul className="flex flex-wrap gap-x-6 gap-y-2">
            {points.map((point) => (
              <li key={point} className="flex items-center gap-2 text-sm text-muted-foreground">
                <span className="h-1.5 w-1.5 shrink-0 rounded-full bg-primary/70" />
                {point}
              </li>
            ))}
          </ul>
        </div>
        <ScreenshotFrame src={image} alt={imageAlt} />
      </div>
    );
  }

  return (
    <div className="grid items-center gap-8 lg:grid-cols-[0.6fr_1.4fr] lg:gap-12">
      <div className="space-y-4">
        <Badge variant="outline" className="border-border/70 bg-background/70 text-muted-foreground">
          {badge}
        </Badge>
        <h3 className="text-2xl font-semibold tracking-tight text-foreground sm:text-3xl">{title}</h3>
        <p className="text-base leading-7 text-muted-foreground">{description}</p>
        <ul className="space-y-2.5">
          {points.map((point) => (
            <li key={point} className="flex items-start gap-2.5 text-sm leading-6 text-muted-foreground">
              <span className="mt-2 h-1.5 w-1.5 shrink-0 rounded-full bg-primary/70" />
              {point}
            </li>
          ))}
        </ul>
      </div>
      <ScreenshotFrame src={image} alt={imageAlt} />
    </div>
  );
}

export function HomePage() {
  const isAuthenticated = useAtomValue(isAuthenticatedAtom);

  return (
    <main className="relative min-h-screen">
      <header className="sticky top-0 z-20 border-b border-border/60 bg-background/75 backdrop-blur-md">
        <div className="mx-auto flex h-16 max-w-7xl items-center justify-between gap-4 px-4 sm:px-6 lg:px-8">
          <Link to="/" className="flex items-center gap-2.5">
            <span className="flex h-8 w-8 items-center justify-center rounded-lg bg-primary text-primary-foreground">
              <KanbanSquare className="h-4.5 w-4.5" />
            </span>
            <span className="text-base font-semibold tracking-tight text-foreground">Mini Jira</span>
          </Link>

          <nav className="hidden items-center gap-6 text-sm text-muted-foreground md:flex">
            <a href="#product" className="transition-colors hover:text-foreground">
              Product
            </a>
            <a href="#workflow" className="transition-colors hover:text-foreground">
              Workflow
            </a>
            <a href="#teams" className="transition-colors hover:text-foreground">
              Teams
            </a>
          </nav>

          <div className="flex items-center gap-2">
            <ModeToggle />
            {isAuthenticated ? (
              <Button asChild size="sm" className="shadow-sm">
                <Link to="/app/projects">
                  View projects
                  <ArrowRight className="ml-1.5 h-4 w-4" />
                </Link>
              </Button>
            ) : (
              <>
                <Button
                  asChild
                  size="sm"
                  variant="outline"
                  className="hero-action-button border-border/70 bg-background/80"
                >
                  <Link to="/login">Sign in</Link>
                </Button>
                <Button asChild size="sm" className="shadow-sm">
                  <Link to="/register">Create account</Link>
                </Button>
              </>
            )}
          </div>
        </div>
      </header>

      <section className="mx-auto max-w-7xl px-4 pb-20 pt-16 sm:px-6 sm:pt-20 lg:px-8">
        <div className="mx-auto flex max-w-3xl flex-col items-center text-center">
          <Badge variant="outline" className="border-border/70 bg-background/70 text-muted-foreground">
            <Sparkles className="mr-1.5 h-3.5 w-3.5" />
            Project tracking without the overhead
          </Badge>
          <h1 className="mt-6 text-4xl font-semibold tracking-tight text-foreground sm:text-5xl lg:text-6xl">
            Plan, track, and ship work in one focused workspace.
          </h1>
          <p className="mt-5 max-w-2xl text-base leading-7 text-muted-foreground sm:text-lg">
            Mini Jira gives your team projects, boards, epics, and dashboards — everything you need to move
            tickets from idea to done, and nothing you don&apos;t.
          </p>
          <div className="mt-8 flex flex-wrap justify-center gap-3">
            {isAuthenticated ? (
              <Button asChild size="lg" className="shadow-sm">
                <Link to="/app/projects">
                  View your projects
                  <ArrowRight className="ml-2 h-4 w-4" />
                </Link>
              </Button>
            ) : (
              <>
                <Button asChild size="lg" className="shadow-sm">
                  <Link to="/register">
                    Get started free
                    <ArrowRight className="ml-2 h-4 w-4" />
                  </Link>
                </Button>
                <Button
                  asChild
                  size="lg"
                  variant="outline"
                  className="hero-action-button border-border/70 bg-background/80 shadow-sm"
                >
                  <Link to="/login">Sign in</Link>
                </Button>
              </>
            )}
          </div>
          <p className="mt-4 flex items-center gap-1.5 text-xs text-muted-foreground">
            <LockKeyhole className="h-3.5 w-3.5" />
            Secured with JWT authentication — your workspace stays private.
          </p>
        </div>

        <div className="relative mx-auto mt-14 max-w-6xl">
          <div
            aria-hidden
            className="absolute -inset-x-8 -top-10 -z-10 h-64 rounded-full bg-primary/10 blur-3xl"
          />
          <ScreenshotFrame
            src="/dashboard.png"
            alt="Mini Jira project dashboard with task metrics, recent tasks, and recent epics"
          />
        </div>
      </section>

      <section id="product" className="border-y border-border/60 bg-card/50 backdrop-blur-sm">
        <div className="mx-auto grid max-w-7xl gap-8 px-4 py-16 sm:grid-cols-2 sm:px-6 lg:grid-cols-4 lg:px-8">
          {capabilities.map((item) => (
            <div key={item.title} className="space-y-3">
              <span className="flex h-10 w-10 items-center justify-center rounded-xl border border-border/70 bg-background/80">
                <item.icon className="h-5 w-5 text-muted-foreground" />
              </span>
              <h2 className="text-sm font-medium text-foreground">{item.title}</h2>
              <p className="text-sm leading-6 text-muted-foreground">{item.description}</p>
            </div>
          ))}
        </div>
      </section>

      <section id="workflow" className="mx-auto max-w-[90rem] space-y-24 px-4 py-24 sm:px-6 lg:px-8">
        <FeatureSection
          badge="Board"
          title="A board that mirrors how your team actually works"
          description="Tickets flow through Ready, In progress, Review, and Done. Every card shows its priority, estimate, and assignee, so standups take minutes instead of meetings."
          points={[
            'Drag and drop tickets between workflow stages',
            'Priority and assignee visible on every card',
            'Filter by search, status, priority, epic, or assignee',
          ]}
          image="/board_page.png"
          imageAlt="Mini Jira kanban board with Ready, In progress, Review, and Done columns"
        />

        <FeatureSection
          badge="Epics"
          title="Group related work into epics, then track them to completion"
          description="Bundle tickets into larger initiatives and watch each epic's list shrink as the team delivers."
          points={['Assign tickets per epic', 'Rolled-up counts and estimates', 'Edit epics inline']}
          image="/epic_management.png"
          imageAlt="Mini Jira epic management view with epics and their assigned tickets"
          stacked
        />

        <FeatureSection
          badge="Projects"
          title="One hub for every project you own or joined"
          description="Separate the work you lead from the work you contribute to, and land straight on a project's dashboard."
          points={['Owned and joined projects side by side', 'Create and invite in seconds']}
          image="/project_page.png"
          imageAlt="Mini Jira project hub listing owned and joined projects"
          stacked
        />

        <div id="teams">
          <FeatureSection
            badge="Teams"
            title="Built for teams, with ownership and access under control"
            description="Every project has a clear owner, a managed member list, and role-based access."
            points={['Add and remove members', 'Transfer ownership in one click', 'Admin-only actions stay admin-only']}
            image="/settingspage.png"
            imageAlt="Mini Jira project settings with details, owner transfer, and member management"
            stacked
          />
        </div>
      </section>

      <section className="border-t border-border/60 bg-card/50 backdrop-blur-sm">
        <div className="mx-auto flex max-w-3xl flex-col items-center px-4 py-20 text-center sm:px-6 lg:px-8">
          <h2 className="text-3xl font-semibold tracking-tight text-foreground sm:text-4xl">
            Ready to plan your next sprint?
          </h2>
          <p className="mt-4 max-w-xl text-base leading-7 text-muted-foreground">
            Create an account, set up your first project, and have your team moving tickets in under five
            minutes.
          </p>
          <div className="mt-8 flex flex-wrap justify-center gap-3">
            {isAuthenticated ? (
              <Button asChild size="lg" className="shadow-sm">
                <Link to="/app/projects">
                  View your projects
                  <ArrowRight className="ml-2 h-4 w-4" />
                </Link>
              </Button>
            ) : (
              <>
                <Button asChild size="lg" className="shadow-sm">
                  <Link to="/register">
                    Create account
                    <ArrowRight className="ml-2 h-4 w-4" />
                  </Link>
                </Button>
                <Button
                  asChild
                  size="lg"
                  variant="outline"
                  className="hero-action-button border-border/70 bg-background/80 shadow-sm"
                >
                  <Link to="/login">Sign in</Link>
                </Button>
              </>
            )}
          </div>
        </div>
      </section>

      <footer className="border-t border-border/60">
        <div className="mx-auto flex max-w-7xl flex-col items-center justify-between gap-4 px-4 py-8 text-sm text-muted-foreground sm:flex-row sm:px-6 lg:px-8">
          <div className="flex items-center gap-2">
            <KanbanSquare className="h-4 w-4" />
            <span>Mini Jira — a focused project workspace.</span>
          </div>
          <nav className="flex items-center gap-5">
            <Link to="/login" className="transition-colors hover:text-foreground">
              Sign in
            </Link>
            <Link to="/register" className="transition-colors hover:text-foreground">
              Create account
            </Link>
            <Link to="/app/projects" className="transition-colors hover:text-foreground">
              View projects
            </Link>
          </nav>
        </div>
      </footer>
    </main>
  );
}
