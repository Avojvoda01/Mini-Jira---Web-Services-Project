import { AlertCircle, CheckCircle2, Clock3, Flame, ListTodo, Sparkles, Users, Zap } from 'lucide-react';
import { useMemo, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import { Card, CardContent } from '@/components/ui/card';
import { usePageHeader } from '@/components/layout/PageHeaderContext';
import { useTasksQuery } from '@/features/tasks';
import { useEpicsQuery } from '@/features/epics';
import { useProjectQuery } from '@/features/projects';

export function DashboardPage() {
  const { setContent } = usePageHeader();
  const { projectId } = useParams();
  
  const { data: tasks = [] } = useTasksQuery({ projectId: projectId ?? null });
  const { data: epics = [] } = useEpicsQuery({ projectId: projectId ?? null });
  const { data: project } = useProjectQuery(projectId ?? null);

  const stats = useMemo(() => {
    const totalTasks = tasks.length;
    const openTasks = tasks.filter((t) => t.status === 'todo').length;
    const inProgressTasks = tasks.filter((t) => t.status === 'in-progress').length;
    const completedTasks = tasks.filter((t) => t.status === 'done').length;
    const totalEpics = epics.filter((e) => e.projectId === projectId).length;
    const mediumPriorityTasks = tasks.filter((t) => t.priority === 'medium').length;
    const highPriorityTasks = tasks.filter((t) => t.priority === 'high').length;
    const teamMembers = project?.memberIds?.length ?? 0;

    return {
      totalTasks,
      openTasks,
      inProgressTasks,
      completedTasks,
      totalEpics,
      mediumPriorityTasks,
      highPriorityTasks,
      teamMembers,
    };
  }, [tasks, epics, project, projectId]);

  useEffect(() => {
    setContent({
      title: 'Dashboard',
      description: 'Project overview and key metrics.',
    });

    return () => setContent({});
  }, [setContent]);

  return (
    <section className="space-y-6">
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        {/* Total Tasks */}
        <Card className="border-border/70 bg-card/80 shadow-sm backdrop-blur-sm hover:shadow-md transition-shadow">
          <CardContent className="p-6">
            <div className="flex items-start justify-between">
              <div className="space-y-3">
                <p className="text-sm font-medium uppercase tracking-[0.15em] text-muted-foreground">Total Tasks</p>
                <p className="text-4xl font-bold tracking-tight text-foreground">{stats.totalTasks}</p>
              </div>
              <ListTodo className="h-8 w-8 text-muted-foreground" />
            </div>
          </CardContent>
        </Card>

        {/* Open Tasks */}
        <Card className="border-border/70 bg-card/80 shadow-sm backdrop-blur-sm hover:shadow-md transition-shadow">
          <CardContent className="p-6">
            <div className="flex items-start justify-between">
              <div className="space-y-3">
                <p className="text-sm font-medium uppercase tracking-[0.15em] text-muted-foreground">Open</p>
                <p className="text-4xl font-bold tracking-tight text-foreground">{stats.openTasks}</p>
              </div>
              <Zap className="h-8 w-8 text-muted-foreground" />
            </div>
          </CardContent>
        </Card>

        {/* In Progress */}
        <Card className="border-border/70 bg-card/80 shadow-sm backdrop-blur-sm hover:shadow-md transition-shadow">
          <CardContent className="p-6">
            <div className="flex items-start justify-between">
              <div className="space-y-3">
                <p className="text-sm font-medium uppercase tracking-[0.15em] text-muted-foreground">In Progress</p>
                <p className="text-4xl font-bold tracking-tight text-foreground">{stats.inProgressTasks}</p>
              </div>
              <Clock3 className="h-8 w-8 text-muted-foreground" />
            </div>
          </CardContent>
        </Card>

        {/* Completed */}
        <Card className="border-border/70 bg-card/80 shadow-sm backdrop-blur-sm hover:shadow-md transition-shadow">
          <CardContent className="p-6">
            <div className="flex items-start justify-between">
              <div className="space-y-3">
                <p className="text-sm font-medium uppercase tracking-[0.15em] text-muted-foreground">Completed</p>
                <p className="text-4xl font-bold tracking-tight text-foreground">{stats.completedTasks}</p>
              </div>
              <CheckCircle2 className="h-8 w-8 text-muted-foreground" />
            </div>
          </CardContent>
        </Card>

        {/* Total Epics */}
        <Card className="border-border/70 bg-card/80 shadow-sm backdrop-blur-sm hover:shadow-md transition-shadow">
          <CardContent className="p-6">
            <div className="flex items-start justify-between">
              <div className="space-y-3">
                <p className="text-sm font-medium uppercase tracking-[0.15em] text-muted-foreground">Total Epics</p>
                <p className="text-4xl font-bold tracking-tight text-foreground">{stats.totalEpics}</p>
              </div>
              <Sparkles className="h-8 w-8 text-muted-foreground" />
            </div>
          </CardContent>
        </Card>

        {/* Medium Priority Tasks */}
        <Card className="border-border/70 bg-card/80 shadow-sm backdrop-blur-sm hover:shadow-md transition-shadow">
          <CardContent className="p-6">
            <div className="flex items-start justify-between">
              <div className="space-y-3">
                <p className="text-sm font-medium uppercase tracking-[0.15em] text-muted-foreground">Medium Priority</p>
                <p className="text-4xl font-bold tracking-tight text-foreground">{stats.mediumPriorityTasks}</p>
              </div>
              <AlertCircle className="h-8 w-8 text-muted-foreground" />
            </div>
          </CardContent>
        </Card>

        {/* High Priority Tasks */}
        <Card className="border-border/70 bg-card/80 shadow-sm backdrop-blur-sm hover:shadow-md transition-shadow">
          <CardContent className="p-6">
            <div className="flex items-start justify-between">
              <div className="space-y-3">
                <p className="text-sm font-medium uppercase tracking-[0.15em] text-muted-foreground">High Priority</p>
                <p className="text-4xl font-bold tracking-tight text-foreground">{stats.highPriorityTasks}</p>
              </div>
              <Flame className="h-8 w-8 text-muted-foreground" />
            </div>
          </CardContent>
        </Card>

        {/* Team Members */}
        <Card className="border-border/70 bg-card/80 shadow-sm backdrop-blur-sm hover:shadow-md transition-shadow">
          <CardContent className="p-6">
            <div className="flex items-start justify-between">
              <div className="space-y-3">
                <p className="text-sm font-medium uppercase tracking-[0.15em] text-muted-foreground">Team Members</p>
                <p className="text-4xl font-bold tracking-tight text-foreground">{stats.teamMembers}</p>
              </div>
              <Users className="h-8 w-8 text-muted-foreground" />
            </div>
          </CardContent>
        </Card>
      </div>
    </section>
  );
}
