import { ArrowRight, FolderKanban, LayoutGrid, Sparkles } from 'lucide-react';
import { Link } from 'react-router-dom';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { projectCatalog } from '@/features/projects/projectData';
import { BackToHomeButton } from '@/components/common/BackToHomeButton';

export function ProjectsPage() {
  return (
    <section className="space-y-6">
      <div className="relative overflow-hidden rounded-3xl border border-border/70 bg-gradient-to-br from-card via-card to-muted/45 p-6 shadow-sm sm:p-8">
        <div>
          <BackToHomeButton align="right" />
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
              <h1 className="text-3xl font-semibold tracking-tight text-foreground sm:text-4xl lg:text-5xl">Select a Project</h1>
              <p className="max-w-2xl text-sm leading-6 text-muted-foreground sm:text-base">
                This is your entry layer before dashboard and board views. Pick a project to open its dedicated workspace context.
              </p>
            </div>
          </div>

          <div className="grid gap-3 rounded-2xl border border-border/70 bg-background/75 p-4 sm:grid-cols-3 lg:grid-cols-1 xl:grid-cols-3">
            <div>
              <p className="text-xs uppercase tracking-[0.18em] text-muted-foreground">Projects</p>
              <p className="mt-2 text-2xl font-semibold tracking-tight text-foreground">{projectCatalog.length}</p>
            </div>
            <div>
              <p className="text-xs uppercase tracking-[0.18em] text-muted-foreground">Active</p>
              <p className="mt-2 text-2xl font-semibold tracking-tight text-foreground">
                {projectCatalog.filter((project) => project.status === 'Active').length}
              </p>
            </div>
            <div>
              <p className="text-xs uppercase tracking-[0.18em] text-muted-foreground">Ready</p>
              <p className="mt-2 text-2xl font-semibold tracking-tight text-foreground">
                {projectCatalog.filter((project) => project.status === 'Ready').length}
              </p>
            </div>
          </div>
        </div>
      </div>

      <div className="grid gap-4 lg:grid-cols-3">
        {projectCatalog.map((project) => (
          <Card key={project.name} className="border-border/70 bg-card/80 shadow-sm backdrop-blur-sm">
            <CardHeader className="space-y-3">
              <div className="flex items-start justify-between gap-3">
                <div className="space-y-2">
                  <Badge variant="secondary" className="w-fit border border-border/60 bg-background/80 text-foreground">
                    <LayoutGrid className="mr-1.5 h-3.5 w-3.5" />
                    {project.status}
                  </Badge>
                  <CardTitle className="text-xl tracking-tight">{project.name}</CardTitle>
                </div>
                <FolderKanban className="h-5 w-5 text-muted-foreground" />
              </div>
              <CardDescription>{project.description}</CardDescription>
            </CardHeader>

            <CardContent className="space-y-3 text-sm text-muted-foreground">
              <div className="flex items-center justify-between gap-3">
                <span>Owner</span>
                <span className="font-medium text-foreground">{project.owner}</span>
              </div>
              <div className="flex items-center justify-between gap-3">
                <span>Scope</span>
                <span className="font-medium text-foreground">{project.tickets}</span>
              </div>

              <Button asChild className="mt-2 w-full shadow-sm">
                <Link to={`/app/project/${project.id}/dashboard`}>
                  Open project
                  <ArrowRight className="ml-2 h-4 w-4" />
                </Link>
              </Button>
            </CardContent>
          </Card>
        ))}
      </div>
    </section>
  );
}
