import { useEffect, useMemo } from 'react';
import { ArrowLeft, LayoutDashboard, ListTodo, LogOut, Settings2, SquareKanban } from 'lucide-react';
import { NavLink, Outlet, type NavLinkRenderProps, useNavigate, useParams } from 'react-router-dom';
import { useAtomValue, useSetAtom } from 'jotai';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Separator } from '@/components/ui/separator';
import { ModeToggle } from '@/components/common/ModeToggle';
import { PageHeaderProvider, usePageHeader } from '@/components/layout/PageHeaderContext';
import { cn } from '@/lib/utils';
import { authSessionAtom } from '@/store/authAtoms';
import { useProjectsQuery } from '@/features/projects';

type NavigationItem = {
  to: string;
  label: string;
  icon: typeof LayoutDashboard;
};

const navigationItems: NavigationItem[] = [
  { to: 'dashboard', label: 'Dashboard', icon: LayoutDashboard },
  { to: 'board', label: 'Board', icon: SquareKanban },
  { to: 'backlog', label: 'Epic Management', icon: ListTodo },
  { to: 'settings', label: 'Settings', icon: Settings2 },
];

export function AppLayout() {
  const session = useAtomValue(authSessionAtom);
  const setSession = useSetAtom(authSessionAtom);
  const navigate = useNavigate();
  const { projectId } = useParams();
  const { data: allProjects = [], error, isError, isLoading: isLoadingProjects } = useProjectsQuery();
  const currentUserId = session?.userId?.toLowerCase();

  const accessibleProjects = useMemo(() => {
    if (!currentUserId) return [];
    if (session?.role === 'Admin') return allProjects;
    return allProjects.filter(
      (p) =>
        p.createdById?.toLowerCase() === currentUserId ||
        (p.memberIds ?? []).some((id) => id.toLowerCase() === currentUserId),
    );
  }, [allProjects, currentUserId, session?.role]);

  const project = accessibleProjects.find((p) => p.id === projectId);

  useEffect(() => {
    if (!isLoadingProjects && !isError && !project) {
      navigate('/app/projects', { replace: true });
    }
  }, [isLoadingProjects, isError, project, navigate]);

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
                {isLoadingProjects
                  ? 'Loading project...'
                  : isError
                    ? error instanceof Error
                      ? error.message
                      : 'Unable to load projects.'
                    : project
                      ? project.name
                      : 'Choose a project to open its workspace.'}
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

      <PageHeaderProvider>
        <AppMain />
      </PageHeaderProvider>
    </div>
  );
}

function AppMain() {
  const { content } = usePageHeader();

  return (
    <div className="app-main">
      <header className="topbar">
        <div className="space-y-2">
          <h2 className="text-lg font-semibold tracking-tight text-foreground">{content.title ?? 'Workspace'}</h2>
          {content.description ? <p className="max-w-2xl text-sm text-muted-foreground">{content.description}</p> : null}
          {content.meta ? <div className="flex flex-wrap gap-2">{content.meta}</div> : null}
        </div>

        <div className="flex flex-wrap items-center justify-end gap-2">
          {content.actions}
          <ModeToggle />
        </div>
      </header>

      <main className="content-area">
        <Outlet />
      </main>
    </div>
  );
}
