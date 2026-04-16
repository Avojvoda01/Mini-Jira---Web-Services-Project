import { ArrowLeft, LayoutDashboard, ListTodo, LogOut, Settings2, SquareKanban } from 'lucide-react';
import { NavLink, Outlet, type NavLinkRenderProps, useNavigate, useParams } from 'react-router-dom';
import { useAtomValue, useSetAtom } from 'jotai';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Separator } from '@/components/ui/separator';
import { ModeToggle } from '@/components/common/ModeToggle';
import { cn } from '@/lib/utils';
import { authSessionAtom } from '@/store/authAtoms';
import { getProjectById } from '@/features/projects/projectData';

type NavigationItem = {
  to: string;
  label: string;
  icon: typeof LayoutDashboard;
};

const navigationItems: NavigationItem[] = [
  { to: 'dashboard', label: 'Dashboard', icon: LayoutDashboard },
  { to: 'board', label: 'Board', icon: SquareKanban },
  { to: 'backlog', label: 'Backlog', icon: ListTodo },
  { to: 'settings', label: 'Settings', icon: Settings2 },
];

export function AppLayout() {
  const session = useAtomValue(authSessionAtom);
  const setSession = useSetAtom(authSessionAtom);
  const navigate = useNavigate();
  const { projectId } = useParams();
  const project = getProjectById(projectId);

  const handleSignOut = () => {
    setSession(null);
    navigate('/', { replace: true });
  };

  return (
    <div className="app-shell">
      <aside className="app-sidebar" aria-label="Primary">
        <div className="space-y-5">
          <div className="space-y-4">
            <Badge variant="outline" className="w-fit border-border/70 bg-background/70 text-[0.68rem] uppercase tracking-[0.24em] text-muted-foreground">
              Mini Jira
            </Badge>

            <div className="space-y-2">
              <h1 className="text-2xl font-semibold tracking-tight text-foreground">Task Workspace</h1>
              <p className="max-w-[18rem] text-sm leading-6 text-muted-foreground">
                {project ? project.name : 'Choose a project to open its workspace.'}
              </p>
            </div>
          </div>

          <Separator className="bg-border/70" />

          <Button asChild variant="outline" className="sidebar-action-button w-full justify-start border-border/70 bg-background/80 shadow-sm">
            <NavLink to="/app/projects">
              <ArrowLeft className="mr-2 h-4 w-4" />
              Back to projects
            </NavLink>
          </Button>

          <nav className="grid gap-2" aria-label="Main navigation">
            {navigationItems.map((item) => (
              <NavLink
                key={item.to}
                to={item.to}
                className={({ isActive }: NavLinkRenderProps) =>
                  cn(
                    'group flex items-center gap-3 rounded-2xl border px-3 py-2.5 text-sm font-medium transition-all',
                    isActive
                      ? 'border-foreground bg-foreground text-background shadow-sm'
                      : 'border-transparent bg-transparent text-muted-foreground hover:border-border/70 hover:bg-background/70 hover:text-foreground',
                  )
                }
              >
                <span
                  className={cn(
                    'grid h-8 w-8 place-items-center rounded-xl border transition-colors',
                    'border-border/60 bg-background/80 text-foreground/80 group-hover:border-border group-hover:bg-background',
                  )}
                >
                  <item.icon className="h-4 w-4" />
                </span>
                <span>{item.label}</span>
              </NavLink>
            ))}
          </nav>
        </div>

        <Card className="border-border/70 bg-card/80 shadow-sm backdrop-blur-sm">
          <CardHeader className="pb-3">
            <CardTitle className="text-sm font-medium">Workspace state</CardTitle>
            <CardDescription>Current sprint and delivery health.</CardDescription>
          </CardHeader>
          <CardContent className="space-y-3">
            <div className="flex items-center justify-between gap-3">
              <div>
                <p className="text-sm font-medium text-foreground">Sprint 14</p>
                <p className="text-xs text-muted-foreground">Delivery on track</p>
              </div>
              <Badge className="bg-emerald-500/10 text-emerald-700 hover:bg-emerald-500/10">Stable</Badge>
            </div>

            <Separator />

            <div className="grid gap-2 text-sm">
              <div className="flex items-center justify-between text-muted-foreground">
                <span>Open tickets</span>
                <span className="font-medium text-foreground">12</span>
              </div>
              <div className="flex items-center justify-between text-muted-foreground">
                <span>In progress</span>
                <span className="font-medium text-foreground">6</span>
              </div>
              <div className="flex items-center justify-between text-muted-foreground">
                <span>Released</span>
                <span className="font-medium text-foreground">3</span>
              </div>
            </div>
          </CardContent>
        </Card>

        <div className="space-y-3">
          <div className="rounded-2xl border border-border/70 bg-background/70 p-4">
            <p className="text-xs uppercase tracking-[0.18em] text-muted-foreground">Signed in as</p>
            <p className="mt-2 text-sm font-medium text-foreground">{session?.displayName}</p>
            <p className="text-xs text-muted-foreground">{session?.email}</p>
          </div>

          <Button variant="outline" className="sidebar-action-button w-full justify-start border-border/70 bg-background/80 shadow-sm" onClick={handleSignOut}>
            <LogOut className="mr-2 h-4 w-4" />
            Sign out
          </Button>
        </div>
      </aside>

      <div className="app-main">
        <header className="topbar">
          <div className="space-y-1">
            <p className="text-[0.72rem] font-semibold uppercase tracking-[0.24em] text-muted-foreground">Operations console</p>
            <h2 className="text-lg font-semibold tracking-tight text-foreground">{project?.name ?? 'Project workspace'}</h2>
            <p className="text-sm text-muted-foreground">Vite + React + Jotai + TanStack Query</p>
          </div>

          <div className="flex flex-wrap items-center justify-end gap-2">
            <ModeToggle />
            <Badge variant="secondary" className="border border-border/60 bg-background/80 text-foreground">
              Sprint 14
            </Badge>
            {project ? (
              <Badge variant="outline" className="border-border/70 bg-background/70 text-muted-foreground">
                {project.status}
              </Badge>
            ) : null}
          </div>
        </header>

        <main className="content-area">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
