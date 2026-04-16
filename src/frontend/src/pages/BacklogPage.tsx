import { Filter, Search, Sparkles } from 'lucide-react';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Separator } from '@/components/ui/separator';

const backlogItems = [
  {
    ticket: 'MJR-141',
    title: 'Add project-level quick filters',
    description: 'Make backlog triage faster for product and engineering leads.',
    priority: 'High',
    status: 'Ready for refinement',
    estimate: '5 pts',
  },
  {
    ticket: 'MJR-138',
    title: 'Improve issue description formatting',
    description: 'Support cleaner acceptance criteria and richer task context.',
    priority: 'Medium',
    status: 'Needs design input',
    estimate: '3 pts',
  },
  {
    ticket: 'MJR-135',
    title: 'Add notification preference controls',
    description: 'Allow workspace users to tune updates without leaving the app.',
    priority: 'Low',
    status: 'Queued',
    estimate: '2 pts',
  },
];

export function BacklogPage() {
  return (
    <section className="space-y-6">
      <Card className="border-border/70 bg-card/80 shadow-sm backdrop-blur-sm">
        <CardContent className="p-6 sm:p-8">
          <div className="flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between">
            <div className="space-y-4">
              <Badge variant="outline" className="w-fit border-border/70 bg-background/70 text-muted-foreground">
                <Sparkles className="mr-1.5 h-3.5 w-3.5" />
                Backlog planning
              </Badge>

              <div className="space-y-2">
                <h2 className="text-3xl font-semibold tracking-tight text-foreground sm:text-4xl">Backlog</h2>
                <p className="max-w-2xl text-sm leading-6 text-muted-foreground sm:text-base">
                  A refinement queue for shaping work before it reaches the board.
                </p>
              </div>
            </div>

            <div className="flex flex-wrap gap-2">
              <Button variant="outline" className="border-border/70 bg-background/80 shadow-sm">
                <Filter className="mr-2 h-4 w-4" />
                Filters
              </Button>
              <Button className="shadow-sm">Create item</Button>
            </div>
          </div>
        </CardContent>
      </Card>

      <div className="grid gap-4 xl:grid-cols-[minmax(0,1fr)_320px]">
        <Card className="border-border/70 bg-card/80 shadow-sm backdrop-blur-sm">
          <CardHeader className="space-y-3 pb-4">
            <CardTitle>Refinement queue</CardTitle>
            <CardDescription>Prioritized work that is ready for review and sizing.</CardDescription>
            <div className="relative max-w-md">
              <Search className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
              <Input className="pl-9" placeholder="Search backlog items" />
            </div>
          </CardHeader>

          <CardContent className="space-y-2">
            {backlogItems.map((item, index) => (
              <div key={item.ticket}>
                {index > 0 ? <Separator className="mb-4" /> : null}
                <div className="grid gap-4 rounded-2xl border border-border/70 bg-background/80 p-4 shadow-sm lg:grid-cols-[minmax(0,1fr)_16rem] lg:items-start">
                  <div className="space-y-2">
                    <div className="flex flex-wrap items-center gap-2">
                      <Badge variant="outline" className="border-border/70 bg-background/70 text-[0.68rem] uppercase tracking-[0.18em] text-muted-foreground">
                        {item.ticket}
                      </Badge>
                      <Badge variant="secondary" className="border border-border/60 bg-background/80 text-foreground">
                        {item.status}
                      </Badge>
                    </div>
                    <h3 className="text-sm font-medium text-foreground">{item.title}</h3>
                    <p className="text-sm leading-6 text-muted-foreground">{item.description}</p>
                  </div>

                  <div className="grid gap-2 rounded-2xl border border-border/70 bg-muted/30 p-4 text-sm">
                    <div className="flex items-center justify-between gap-3">
                      <span className="text-muted-foreground">Priority</span>
                      <Badge
                        className={
                          item.priority === 'High'
                            ? 'bg-rose-500/10 text-rose-700 hover:bg-rose-500/10'
                            : item.priority === 'Medium'
                              ? 'bg-amber-500/10 text-amber-700 hover:bg-amber-500/10'
                              : 'bg-slate-500/10 text-slate-700 hover:bg-slate-500/10'
                        }
                      >
                        {item.priority}
                      </Badge>
                    </div>
                    <div className="flex items-center justify-between gap-3">
                      <span className="text-muted-foreground">Estimate</span>
                      <span className="font-medium text-foreground">{item.estimate}</span>
                    </div>
                  </div>
                </div>
              </div>
            ))}
          </CardContent>
        </Card>

        <Card className="border-border/70 bg-card/80 shadow-sm backdrop-blur-sm">
          <CardHeader>
            <CardTitle>Refinement rules</CardTitle>
            <CardDescription>Keep backlog items consistent before they reach the board.</CardDescription>
          </CardHeader>
          <CardContent className="space-y-4 text-sm text-muted-foreground">
            <p>1. Each item should have a clear owner and measurable scope.</p>
            <p>2. Estimates should be lightweight and updated before sprint planning.</p>
            <p>3. Blocked items stay visible until dependency owners confirm a path forward.</p>
          </CardContent>
        </Card>
      </div>
    </section>
  );
}
