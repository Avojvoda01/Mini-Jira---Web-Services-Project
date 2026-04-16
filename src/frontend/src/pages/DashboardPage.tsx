import { ArrowRight, CircleCheckBig, Clock3, Sparkles } from 'lucide-react';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Separator } from '@/components/ui/separator';

const kpis = [
  { label: 'Open tickets', value: '12', detail: '+2 this week' },
  { label: 'In progress', value: '6', detail: '3 due today' },
  { label: 'Released', value: '3', detail: '1 release candidate' },
];

const activityItems = [
  {
    title: 'Sprint planning refined the top backlog items',
    time: '10 min ago',
    tone: 'Planning',
  },
  {
    title: 'Payment flow validation moved to staging review',
    time: '1 hour ago',
    tone: 'Release',
  },
  {
    title: 'Two blockers were cleared from the board',
    time: 'Today',
    tone: 'Execution',
  },
];

export function DashboardPage() {
  return (
    <section className="space-y-6">
      <Card className="border-border/70 bg-card/80 shadow-sm backdrop-blur-sm">
        <CardContent className="p-6 sm:p-8">
          <div className="flex flex-col gap-6 lg:flex-row lg:items-end lg:justify-between">
            <div className="space-y-4">
              <Badge variant="outline" className="w-fit border-border/70 bg-background/70 text-muted-foreground">
                <Sparkles className="mr-1.5 h-3.5 w-3.5" />
                Operational overview
              </Badge>

              <div className="space-y-2">
                <h2 className="text-3xl font-semibold tracking-tight text-foreground sm:text-4xl">Dashboard</h2>
                <p className="max-w-2xl text-sm leading-6 text-muted-foreground sm:text-base">
                  A calm, high-signal view of workload, flow health, and delivery momentum.
                </p>
              </div>

              <div className="flex flex-wrap gap-2">
                <Badge variant="secondary" className="border border-border/60 bg-background/80 text-foreground">
                  12 open tickets
                </Badge>
                <Badge variant="outline" className="border-border/70 bg-background/60 text-muted-foreground">
                  6 in progress
                </Badge>
                <Badge variant="outline" className="border-border/70 bg-background/60 text-muted-foreground">
                  3 released
                </Badge>
              </div>
            </div>

            <div className="grid gap-3 sm:grid-cols-3 lg:min-w-[24rem] lg:max-w-[24rem]">
              {kpis.map((kpi) => (
                <Card key={kpi.label} size="sm" className="border-border/70 bg-background/80 shadow-sm">
                  <CardContent className="space-y-2 p-4">
                    <p className="text-xs font-medium uppercase tracking-[0.18em] text-muted-foreground">{kpi.label}</p>
                    <p className="text-3xl font-semibold tracking-tight text-foreground">{kpi.value}</p>
                    <p className="text-xs text-muted-foreground">{kpi.detail}</p>
                  </CardContent>
                </Card>
              ))}
            </div>
          </div>
        </CardContent>
      </Card>

      <div className="grid gap-4 xl:grid-cols-[1.35fr_0.65fr]">
        <Card className="border-border/70 bg-card/80 shadow-sm backdrop-blur-sm">
          <CardHeader className="flex flex-row items-start justify-between gap-4 pb-4">
            <div>
              <CardTitle>Delivery pulse</CardTitle>
              <CardDescription>Recent flow indicators from the active sprint.</CardDescription>
            </div>
            <Button variant="ghost" size="sm" className="text-muted-foreground hover:text-foreground">
              View board
              <ArrowRight className="ml-2 h-4 w-4" />
            </Button>
          </CardHeader>
          <CardContent className="grid gap-4 md:grid-cols-3">
            {[
              {
                title: 'Cycle time',
                value: '3.8d',
                detail: 'down from 4.6d last sprint',
                icon: Clock3,
              },
              {
                title: 'Blocked work',
                value: '2',
                detail: 'none older than 1 day',
                icon: CircleCheckBig,
              },
              {
                title: 'Release confidence',
                value: '94%',
                detail: 'stable across two builds',
                icon: Sparkles,
              },
            ].map((item) => (
              <div key={item.title} className="rounded-2xl border border-border/70 bg-background/80 p-4">
                <div className="flex items-center justify-between gap-3">
                  <p className="text-sm font-medium text-muted-foreground">{item.title}</p>
                  <item.icon className="h-4 w-4 text-muted-foreground" />
                </div>
                <p className="mt-3 text-3xl font-semibold tracking-tight text-foreground">{item.value}</p>
                <p className="mt-1 text-sm text-muted-foreground">{item.detail}</p>
              </div>
            ))}
          </CardContent>
        </Card>

        <Card className="border-border/70 bg-card/80 shadow-sm backdrop-blur-sm">
          <CardHeader>
            <CardTitle>Recent activity</CardTitle>
            <CardDescription>Short signals that show where the team is spending time.</CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            {activityItems.map((item, index) => (
              <div key={item.title}>
                {index > 0 ? <Separator className="mb-4" /> : null}
                <div className="space-y-2">
                  <div className="flex items-center justify-between gap-3">
                    <Badge variant="outline" className="border-border/70 bg-background/70 text-muted-foreground">
                      {item.tone}
                    </Badge>
                    <span className="text-xs text-muted-foreground">{item.time}</span>
                  </div>
                  <p className="text-sm leading-6 text-foreground">{item.title}</p>
                </div>
              </div>
            ))}
          </CardContent>
        </Card>
      </div>
    </section>
  );
}
