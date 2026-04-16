import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Separator } from '@/components/ui/separator';

const preferences = [
  { label: 'Default landing page', value: 'Dashboard' },
  { label: 'Board density', value: 'Comfortable' },
  { label: 'Date format', value: 'MMM d, yyyy' },
];

export function SettingsPage() {
  return (
    <section className="space-y-6">
      <Card className="border-border/70 bg-card/80 shadow-sm backdrop-blur-sm">
        <CardHeader>
          <Badge variant="outline" className="w-fit border-border/70 bg-background/70 text-muted-foreground">
            Workspace controls
          </Badge>
          <CardTitle className="text-3xl font-semibold tracking-tight sm:text-4xl">Settings</CardTitle>
          <CardDescription className="max-w-2xl text-sm leading-6 sm:text-base">
            Configure workspace defaults, communication preferences, and assistant behavior.
          </CardDescription>
        </CardHeader>
      </Card>

      <div className="grid gap-4 lg:grid-cols-2">
        <Card className="border-border/70 bg-card/80 shadow-sm backdrop-blur-sm">
          <CardHeader>
            <CardTitle>Workspace preferences</CardTitle>
            <CardDescription>Core settings that shape the day-to-day experience.</CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            {preferences.map((item, index) => (
              <div key={item.label}>
                {index > 0 ? <Separator className="mb-4" /> : null}
                <div className="flex items-center justify-between gap-4">
                  <div>
                    <p className="text-sm font-medium text-foreground">{item.label}</p>
                    <p className="text-sm text-muted-foreground">Apply across the entire workspace.</p>
                  </div>
                  <Badge variant="secondary" className="border border-border/60 bg-background/80 text-foreground">
                    {item.value}
                  </Badge>
                </div>
              </div>
            ))}
          </CardContent>
        </Card>

        <Card className="border-border/70 bg-card/80 shadow-sm backdrop-blur-sm">
          <CardHeader>
            <CardTitle>Assistant preferences</CardTitle>
            <CardDescription>Keep future AI assistance helpful without becoming intrusive.</CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="flex items-center justify-between gap-4 rounded-2xl border border-border/70 bg-background/80 p-4">
              <div>
                <p className="text-sm font-medium text-foreground">Suggestion tone</p>
                <p className="text-sm text-muted-foreground">Prefer concise task recommendations.</p>
              </div>
              <Button variant="outline" size="sm" className="border-border/70 bg-background/80">
                Balanced
              </Button>
            </div>

            <div className="flex items-center justify-between gap-4 rounded-2xl border border-border/70 bg-background/80 p-4">
              <div>
                <p className="text-sm font-medium text-foreground">Auto summaries</p>
                <p className="text-sm text-muted-foreground">Prepare short board summaries for standups.</p>
              </div>
              <Badge className="bg-emerald-500/10 text-emerald-700 hover:bg-emerald-500/10">Enabled</Badge>
            </div>
          </CardContent>
        </Card>
      </div>
    </section>
  );
}
