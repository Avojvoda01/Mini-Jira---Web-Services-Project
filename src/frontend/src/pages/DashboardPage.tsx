import { AlertCircle, CheckCircle2, Clock3, Flame, ListTodo, Sparkles, Users, Zap } from 'lucide-react';
import { useMemo, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import { Badge } from '@/components/ui/badge';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { usePageHeader } from '@/components/layout/PageHeaderContext';
import { useTasksQuery } from '@/features/tasks';
import { useEpicsQuery } from '@/features/epics';
import { useProjectQuery } from '@/features/projects';
import { useAdminUsersQuery } from '@/features/users';

const RECENT_LIMIT = 5;

export function DashboardPage() {
  const { setContent } = usePageHeader();
  const { projectId } = useParams();

  const { data: tasks = [] } = useTasksQuery({ projectId: projectId ?? null });
  const { data: epics = [] } = useEpicsQuery({ projectId: projectId ?? null });
  const { data: project } = useProjectQuery(projectId ?? null);
  const { data: users = [] } = useAdminUsersQuery();

  const stats = useMemo(() => {
    const totalTasks = tasks.length;
    const openTasks = tasks.filter((t) => t.status === 'todo').length;
    const inProgressTasks = tasks.filter((t) => t.status === 'in-progress').length;
    const completedTasks = tasks.filter((t) => t.status === 'done').length;
    const totalEpics = epics.filter((e) => e.projectId === projectId).length;
    const mediumPriorityTasks = tasks.filter((t) => t.priority === 'medium').length;
    const highPriorityTasks = tasks.filter((t) => t.priority === 'high').length;
    const teamMembers = project?.memberIds?.length ?? 0;
    return { totalTasks, openTasks, inProgressTasks, completedTasks, totalEpics, mediumPriorityTasks, highPriorityTasks, teamMembers };
  }, [tasks, epics, project, projectId]);

  const usersById = useMemo(
    () => new Map(users.map((u) => [u.id.toLowerCase(), u])),
    [users],
  );

  const recentTasks = useMemo(
    () =>
      [...tasks]
        .sort((a, b) => Date.parse(b.createdAtUtc) - Date.parse(a.createdAtUtc))
        .slice(0, RECENT_LIMIT),
    [tasks],
  );

  const recentEpics = useMemo(
    () =>
      [...epics]
        .sort((a, b) => Date.parse(b.createdAtUtc) - Date.parse(a.createdAtUtc))
        .slice(0, RECENT_LIMIT),
    [epics],
  );

  const resolveUser = (id: string | null | undefined) => {
    if (!id) return null;
    return usersById.get(id.toLowerCase()) ?? null;
  };

  const statusBadgeClass = (status: string) => {
    if (status === 'done') return 'bg-emerald-500/10 text-emerald-700';
    if (status === 'in-progress') return 'bg-sky-500/10 text-sky-700';
    if (status === 'review') return 'bg-amber-500/10 text-amber-700';
    return 'bg-muted text-muted-foreground';
  };

  const statusLabel = (status: string) => {
    if (status === 'done') return 'Done';
    if (status === 'in-progress') return 'In Progress';
    if (status === 'review') return 'Review';
    return 'Open';
  };

  useEffect(() => {
    setContent({ title: 'Dashboard', description: 'Project overview and key metrics.' });
    return () => setContent({});
  }, [setContent]);

  return (
    <section className="space-y-6">
      {/* Stats grid */}
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <StatCard label="Total Tasks" value={stats.totalTasks} icon={<ListTodo className="h-8 w-8 text-muted-foreground" />} />
        <StatCard label="Open" value={stats.openTasks} icon={<Zap className="h-8 w-8 text-muted-foreground" />} />
        <StatCard label="In Progress" value={stats.inProgressTasks} icon={<Clock3 className="h-8 w-8 text-muted-foreground" />} />
        <StatCard label="Completed" value={stats.completedTasks} icon={<CheckCircle2 className="h-8 w-8 text-muted-foreground" />} />
        <StatCard label="Total Epics" value={stats.totalEpics} icon={<Sparkles className="h-8 w-8 text-muted-foreground" />} />
        <StatCard label="Medium Priority" value={stats.mediumPriorityTasks} icon={<AlertCircle className="h-8 w-8 text-muted-foreground" />} />
        <StatCard label="High Priority" value={stats.highPriorityTasks} icon={<Flame className="h-8 w-8 text-muted-foreground" />} />
        <StatCard label="Team Members" value={stats.teamMembers} icon={<Users className="h-8 w-8 text-muted-foreground" />} />
      </div>

      {/* Recent activity */}
      <div className="grid gap-4 lg:grid-cols-2">
        {/* Recent tasks */}
        <Card className="border-border/70 bg-card/80 shadow-sm backdrop-blur-sm">
          <CardHeader className="pb-3">
            <div className="flex items-center justify-between">
              <CardTitle className="text-base font-semibold">Recent Tasks</CardTitle>
              <span className="rounded-full bg-muted px-2.5 py-0.5 text-xs font-medium text-muted-foreground">
                {recentTasks.length}
              </span>
            </div>
          </CardHeader>
          <CardContent className="space-y-1 pb-4">
            {recentTasks.length === 0 ? (
              <p className="text-sm text-muted-foreground">No tasks yet.</p>
            ) : (
              recentTasks.map((task) => {
                const creator = resolveUser(task.createdById);
                const initials = creator?.displayName?.slice(0, 2).toUpperCase() ?? '?';
                return (
                  <div key={task.id} className="flex items-center gap-3 rounded-lg px-2 py-2 hover:bg-muted/50">
                    <div className="flex h-7 w-7 shrink-0 items-center justify-center rounded-full bg-primary/10 text-[10px] font-semibold text-primary">
                      {initials}
                    </div>
                    <div className="min-w-0 flex-1">
                      <p className="truncate text-sm font-medium text-foreground">{task.title}</p>
                      <p className="text-xs text-muted-foreground">{creator?.displayName ?? 'Unknown'}</p>
                    </div>
                    <Badge className={`shrink-0 text-[10px] font-medium hover:opacity-100 ${statusBadgeClass(task.status)}`}>
                      {statusLabel(task.status)}
                    </Badge>
                  </div>
                );
              })
            )}
          </CardContent>
        </Card>

        {/* Recent epics */}
        <Card className="border-border/70 bg-card/80 shadow-sm backdrop-blur-sm">
          <CardHeader className="pb-3">
            <div className="flex items-center justify-between">
              <CardTitle className="text-base font-semibold">Recent Epics</CardTitle>
              <span className="rounded-full bg-muted px-2.5 py-0.5 text-xs font-medium text-muted-foreground">
                {recentEpics.length}
              </span>
            </div>
          </CardHeader>
          <CardContent className="space-y-1 pb-4">
            {recentEpics.length === 0 ? (
              <p className="text-sm text-muted-foreground">No epics yet.</p>
            ) : (
              recentEpics.map((epic) => {
                const creator = resolveUser(epic.createdById);
                const initials = creator?.displayName?.slice(0, 2).toUpperCase() ?? '?';
                return (
                  <div key={epic.id} className="flex items-center gap-3 rounded-lg px-2 py-2 hover:bg-muted/50">
                    <div className="flex h-7 w-7 shrink-0 items-center justify-center rounded-full bg-primary/10 text-[10px] font-semibold text-primary">
                      {initials}
                    </div>
                    <div className="min-w-0 flex-1">
                      <p className="truncate text-sm font-medium text-foreground">{epic.name}</p>
                      <p className="text-xs text-muted-foreground">{creator?.displayName ?? 'Unknown'}</p>
                    </div>
                    {epic.description?.trim() && (
                      <p className="hidden max-w-[120px] truncate text-xs text-muted-foreground sm:block">
                        {epic.description}
                      </p>
                    )}
                  </div>
                );
              })
            )}
          </CardContent>
        </Card>
      </div>
    </section>
  );
}

type StatCardProps = { label: string; value: number; icon: React.ReactNode };

function StatCard({ label, value, icon }: StatCardProps) {
  return (
    <Card className="border-border/70 bg-card/80 shadow-sm backdrop-blur-sm transition-shadow hover:shadow-md">
      <CardContent className="p-6">
        <div className="flex items-start justify-between">
          <div className="space-y-3">
            <p className="text-sm font-medium uppercase tracking-[0.15em] text-muted-foreground">{label}</p>
            <p className="text-4xl font-bold tracking-tight text-foreground">{value}</p>
          </div>
          {icon}
        </div>
      </CardContent>
    </Card>
  );
}
