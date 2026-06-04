import { useMemo, useState } from 'react';
import { useAtomValue, useSetAtom } from 'jotai';
import { ArrowRight, ChevronDown, Filter, FolderKanban, LayoutGrid, LogOut, Settings, ShieldCheck, Sparkles } from 'lucide-react';
import { Link, useNavigate } from 'react-router-dom';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger } from '@/components/ui/dropdown-menu';
import { ModeToggle } from '@/components/common/ModeToggle';
import { CreateProjectForm } from '@/components/projects/CreateProjectForm';
import { useProjectsQuery, type ProjectDto } from '@/features/projects';
import { useAdminUsersQuery } from '@/features/users';
import { authSessionAtom } from '@/store/authAtoms';

type ProjectSortOption = 'newest' | 'oldest' | 'name-asc' | 'name-desc';

export function ProjectsPage() {
  const session = useAtomValue(authSessionAtom);
  const setSession = useSetAtom(authSessionAtom);
  const navigate = useNavigate();
  const currentUserId = session?.userId?.toLowerCase();

  const userInitials = session?.displayName
    ?.split(' ')
    .map((w) => w[0])
    .slice(0, 2)
    .join('')
    .toUpperCase() ?? '?';

  const handleSignOut = () => {
    setSession(null);
    navigate('/', { replace: true });
  };
  const { data: allProjects = [], isError, isLoading, error, refetch } = useProjectsQuery();
  const { data: users = [] } = useAdminUsersQuery();

  const projects = useMemo(() => {
    if (!currentUserId) return [];
    if (session?.role === 'Admin') return allProjects;
    return allProjects.filter(
      (project) =>
        project.createdById?.toLowerCase() === currentUserId ||
        (project.memberIds ?? []).some((id) => id.toLowerCase() === currentUserId),
    );
  }, [allProjects, currentUserId, session?.role]);

  const [isCreateProjectOpen, setIsCreateProjectOpen] = useState(false);
  const [projectSort, setProjectSort] = useState<ProjectSortOption>('newest');

  const describedCount = useMemo(() => {
    return projects.filter((project) => project.description.trim().length > 0).length;
  }, [projects]);

  const sortedProjects = useMemo(() => {
    const next = [...projects];
    const compareByName = (left: ProjectDto, right: ProjectDto) =>
      left.name.localeCompare(right.name, undefined, { sensitivity: 'base' });
    const getCreatedAt = (project: ProjectDto) => {
      const value = Date.parse(project.createdAtUtc);
      return Number.isNaN(value) ? 0 : value;
    };

    switch (projectSort) {
      case 'name-asc':
        next.sort(compareByName);
        break;
      case 'name-desc':
        next.sort((left, right) => compareByName(right, left));
        break;
      case 'oldest':
        next.sort((left, right) => getCreatedAt(left) - getCreatedAt(right));
        break;
      case 'newest':
      default:
        next.sort((left, right) => getCreatedAt(right) - getCreatedAt(left));
        break;
    }

    return next;
  }, [projectSort, projects]);

  const projectSortLabel = useMemo(() => {
    switch (projectSort) {
      case 'name-asc':
        return 'Name A-Z';
      case 'name-desc':
        return 'Name Z-A';
      case 'oldest':
        return 'Oldest';
      case 'newest':
      default:
        return 'Newest';
    }
  }, [projectSort]);

  return (
    <section className="space-y-6">
      <div className="relative overflow-hidden rounded-b-3xl rounded-t-none border border-border/70 bg-gradient-to-br from-card via-card to-muted/45 p-6 shadow-sm sm:p-8">
        <div className="absolute right-4 top-4 z-10 flex items-center gap-2">
          <ModeToggle className="h-11 w-11" />
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button
                variant="outline"
                aria-label="User menu"
                className="flex h-11 items-center gap-2.5 rounded-xl border-border/70 bg-background/85 px-3 shadow-md backdrop-blur-sm hover:bg-background"
              >
                <div className="flex h-7 w-7 shrink-0 items-center justify-center rounded-full bg-primary/10 text-xs font-semibold text-primary">
                  {userInitials}
                </div>
                <div className="text-left">
                  <p className="text-xs font-semibold leading-none text-foreground">{session?.displayName}</p>
                  <p className="mt-0.5 text-[10px] leading-none text-muted-foreground">{session?.email}</p>
                </div>
                <ChevronDown className="h-3.5 w-3.5 shrink-0 text-muted-foreground" />
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end" className="w-44">
              <DropdownMenuItem disabled className="text-muted-foreground">
                <Settings className="mr-2 h-4 w-4" />
                Settings
              </DropdownMenuItem>
              {session?.role === 'Admin' && (
                <DropdownMenuItem disabled className="text-muted-foreground">
                  <ShieldCheck className="mr-2 h-4 w-4" />
                  Admin settings
                </DropdownMenuItem>
              )}
              <DropdownMenuItem onClick={handleSignOut} className="text-rose-600 focus:text-rose-600">
                <LogOut className="mr-2 h-4 w-4" />
                Sign out
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        </div>
        <div className="pointer-events-none absolute -right-20 -top-20 h-56 w-56 rounded-full bg-primary/10 blur-3xl" aria-hidden="true" />
        <div className="pointer-events-none absolute -bottom-24 left-1/3 h-64 w-64 rounded-full bg-sky-500/10 blur-3xl" aria-hidden="true" />

        <div className="relative grid gap-6 lg:grid-cols-[1.2fr_0.8fr] lg:items-end">
          <div className="space-y-4">
            <Badge variant="outline" className="w-fit border-border/70 bg-background/70 text-muted-foreground">
              <Sparkles className="mr-1.5 h-3.5 w-3.5" />
              Projects hub
            </Badge>

            <div className="space-y-3">
              <h1 className="text-3xl font-semibold tracking-tight text-foreground sm:text-4xl lg:text-5xl">
                Select or Create a Project
              </h1>
              <p className="max-w-2xl text-sm leading-6 text-muted-foreground sm:text-base">
                This is the entry layer before dashboard and board views. Pick a project to open its dedicated workspace context.
              </p>
              <div className="mt-2 flex flex-wrap gap-2">
                <DropdownMenu>
                  <DropdownMenuTrigger asChild>
                    <Button variant="outline" className="border-foreground/40 bg-background/95 text-foreground shadow-sm hover:bg-background">
                      <Filter className="mr-2 h-4 w-4" />
                      {projectSortLabel}
                    </Button>
                  </DropdownMenuTrigger>
                  <DropdownMenuContent align="start">
                    <DropdownMenuItem onClick={() => setProjectSort('newest')}>Newest</DropdownMenuItem>
                    <DropdownMenuItem onClick={() => setProjectSort('oldest')}>Oldest</DropdownMenuItem>
                    <DropdownMenuItem onClick={() => setProjectSort('name-asc')}>Name A-Z</DropdownMenuItem>
                    <DropdownMenuItem onClick={() => setProjectSort('name-desc')}>Name Z-A</DropdownMenuItem>
                  </DropdownMenuContent>
                </DropdownMenu>
                <Button
                  className="border-0 bg-sky-500 text-white shadow-sm hover:bg-sky-600"
                  onClick={() => setIsCreateProjectOpen(true)}
                >
                  Create project
                </Button>
              </div>
            </div>
          </div>

          <div className="grid gap-3 rounded-2xl border border-border/70 bg-background/75 p-4 sm:grid-cols-3 lg:grid-cols-1 xl:grid-cols-3">
            <div>
              <p className="text-xs uppercase tracking-[0.18em] text-muted-foreground">Projects</p>
              <p className="mt-2 text-2xl font-semibold tracking-tight text-foreground">{projects.length}</p>
            </div>
            <div>
              <p className="text-xs uppercase tracking-[0.18em] text-muted-foreground">Described</p>
              <p className="mt-2 text-2xl font-semibold tracking-tight text-foreground">{describedCount}</p>
            </div>
            <div>
              <p className="text-xs uppercase tracking-[0.18em] text-muted-foreground">Showing</p>
              <p className="mt-2 text-2xl font-semibold tracking-tight text-foreground">{sortedProjects.length}</p>
            </div>
          </div>
        </div>
      </div>

      <div className="mx-2 grid gap-4 lg:mx-4 lg:grid-cols-3">
        {isLoading ? (
          <Card className="border-border/70 bg-card/80 shadow-sm backdrop-blur-sm">
            <CardHeader>
              <CardTitle className="text-xl tracking-tight">Loading projects...</CardTitle>
              <CardDescription>Fetching your project list.</CardDescription>
            </CardHeader>
          </Card>
        ) : isError ? (
          <Card className="border-border/70 bg-card/80 shadow-sm backdrop-blur-sm">
            <CardHeader className="space-y-3">
              <div>
                <CardTitle className="text-xl tracking-tight">Unable to load projects</CardTitle>
                <CardDescription>{error instanceof Error ? error.message : 'Try again in a moment.'}</CardDescription>
              </div>
              <Button variant="outline" size="sm" onClick={() => refetch()}>
                Retry
              </Button>
            </CardHeader>
          </Card>
        ) : projects.length === 0 ? (
          <Card className="border-border/70 bg-card/80 shadow-sm backdrop-blur-sm">
            <CardHeader>
              <CardTitle className="text-xl tracking-tight">No projects yet</CardTitle>
              <CardDescription>Create your first project to get started.</CardDescription>
            </CardHeader>
          </Card>
        ) : (
          sortedProjects.map((project) => (
            <Card key={project.id} className="border-border/70 bg-card/80 shadow-sm backdrop-blur-sm">
              <CardHeader className="space-y-3">
                <div className="flex items-start justify-between gap-3">
                  <div className="space-y-2">
                    <Badge variant="secondary" className="w-fit border border-border/60 bg-background/80 text-foreground">
                      <LayoutGrid className="mr-1.5 h-3.5 w-3.5" />
                      Project
                    </Badge>
                    <CardTitle className="text-xl tracking-tight">{project.name}</CardTitle>
                  </div>
                  <FolderKanban className="h-5 w-5 text-muted-foreground" />
                </div>
                <CardDescription>{project.description}</CardDescription>
              </CardHeader>

              <CardContent className="space-y-3 text-sm text-muted-foreground">
                <Button asChild className="w-full shadow-sm">
                  <Link to={`/app/project/${project.id}/dashboard`}>
                    Open project
                    <ArrowRight className="ml-2 h-4 w-4" />
                  </Link>
                </Button>
              </CardContent>
            </Card>
          ))
        )}
      </div>

      <CreateProjectForm open={isCreateProjectOpen} onClose={() => setIsCreateProjectOpen(false)} members={users} currentUserId={currentUserId} />
    </section>
  );
}
