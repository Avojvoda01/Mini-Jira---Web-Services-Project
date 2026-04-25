import { ArrowRight, Layers3, LockKeyhole, Sparkles, Users2 } from 'lucide-react';
import { Link } from 'react-router-dom';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Separator } from '@/components/ui/separator';
import { ModeToggle } from '@/components/common/ModeToggle';

const highlights = [
  {
    title: 'Projects first',
    description: 'Start from a calm overview of active work, delivery health, and ownership.',
    icon: Layers3,
  },
  {
    title: 'Secure access',
    description: 'Protected app routes redirect unauthenticated users to sign in automatically.',
    icon: LockKeyhole,
  },
  {
    title: 'Team visibility',
    description: 'Keep work readable with a focused layout designed for execution and review.',
    icon: Users2,
  },
];

export function HomePage() {
  return (
    <main className="relative min-h-screen px-4 py-6 sm:px-6 lg:px-8">
      <div className="absolute right-4 top-4 z-10 sm:right-6 lg:right-8">
        <ModeToggle />
      </div>

      <div className="mx-auto grid min-h-[calc(100vh-3rem)] max-w-7xl items-center gap-6 xl:grid-cols-[1.1fr_0.9fr]">
        <section className="space-y-6">
          <Badge variant="outline" className="w-fit border-border/70 bg-background/70 text-muted-foreground">
            <Sparkles className="mr-1.5 h-3.5 w-3.5" />
            Mini Jira workspace
          </Badge>

          <div className="space-y-4">
            <h1 className="max-w-3xl text-4xl font-semibold tracking-tight text-foreground sm:text-5xl lg:text-6xl">
              A focused project workspace for planning, tracking, and delivery.
            </h1>
            <p className="max-w-2xl text-base leading-7 text-muted-foreground sm:text-lg">
              Start with projects, move into the board when you need execution detail, and keep the experience clean
              until workflow actions are introduced.
            </p>
          </div>

          <div className="flex flex-wrap gap-3">
            <Button asChild size="lg" className="shadow-sm">
              <Link to="/app/projects">
                View projects
                <ArrowRight className="ml-2 h-4 w-4" />
              </Link>
            </Button>
            <Button asChild size="lg" variant="outline" className="hero-action-button border-border/70 bg-background/80 shadow-sm">
              <Link to="/login">Sign in</Link>
            </Button>
            <Button asChild size="lg" variant="ghost" className="text-muted-foreground hover:text-foreground">
              <Link to="/register">Create account</Link>
            </Button>
          </div>

          <div className="grid gap-4 sm:grid-cols-3">
            {[
              { label: 'Projects', value: '08' },
              { label: 'Open tickets', value: '124' },
              { label: 'Teams', value: '04' },
            ].map((item) => (
              <Card key={item.label} className="border-border/70 bg-card/80 shadow-sm backdrop-blur-sm">
                <CardContent className="p-4">
                  <p className="text-xs font-medium uppercase tracking-[0.18em] text-muted-foreground">{item.label}</p>
                  <p className="mt-2 text-3xl font-semibold tracking-tight text-foreground">{item.value}</p>
                </CardContent>
              </Card>
            ))}
          </div>
        </section>

        <section className="space-y-4">
          <Card className="border-border/70 bg-card/85 shadow-sm backdrop-blur-sm">
            <CardHeader className="space-y-3">
              <Badge variant="secondary" className="w-fit border border-border/60 bg-background/80 text-foreground">
                Workspace snapshot
              </Badge>
              <CardTitle className="text-2xl tracking-tight">Ready for projects</CardTitle>
              <CardDescription>
                This shell keeps the first impression calm and leaves room for real workflows later.
              </CardDescription>
            </CardHeader>

            <CardContent className="grid gap-4">
              <div className="grid gap-3 sm:grid-cols-2">
                {highlights.map((item) => (
                  <div key={item.title} className="rounded-2xl border border-border/70 bg-background/80 p-4">
                    <item.icon className="h-5 w-5 text-muted-foreground" />
                    <h2 className="mt-3 text-sm font-medium text-foreground">{item.title}</h2>
                    <p className="mt-1 text-sm leading-6 text-muted-foreground">{item.description}</p>
                  </div>
                ))}
              </div>

              <Separator />

              <div className="rounded-2xl border border-border/70 bg-muted/40 p-4">
                <div className="flex items-center justify-between gap-3">
                  <div>
                    <p className="text-sm font-medium text-foreground">Typical flow</p>
                    <p className="text-sm text-muted-foreground">Sign in, view projects, then open boards and tickets.</p>
                  </div>
                  <Badge variant="outline" className="border-border/70 bg-background/70 text-muted-foreground">
                    Secure
                  </Badge>
                </div>
              </div>
            </CardContent>
          </Card>
        </section>
      </div>
    </main>
  );
}
