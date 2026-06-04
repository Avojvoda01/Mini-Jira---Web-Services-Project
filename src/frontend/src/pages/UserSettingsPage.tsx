import { ArrowLeft, Settings } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';

export function UserSettingsPage() {
  const navigate = useNavigate();

  return (
    <section className="space-y-6">
      <div className="relative overflow-hidden rounded-b-3xl rounded-t-none border border-border/70 bg-gradient-to-br from-card via-card to-muted/45 p-6 shadow-sm sm:p-8">
        <Button
          variant="outline"
          size="icon"
          onClick={() => navigate(-1)}
          aria-label="Go back"
          className="absolute right-4 top-4 z-10 h-11 w-11 border-border/70 bg-background/85 backdrop-blur-sm hover:bg-background"
        >
          <ArrowLeft className="h-5 w-5" />
        </Button>
        <div className="pointer-events-none absolute -right-20 -top-20 h-56 w-56 rounded-full bg-primary/10 blur-3xl" aria-hidden="true" />

        <div className="space-y-4">
          <Badge variant="outline" className="w-fit border-border/70 bg-background/70 text-muted-foreground">
            <Settings className="mr-1.5 h-3.5 w-3.5" />
            Settings
          </Badge>
          <div className="space-y-2">
            <h1 className="text-3xl font-semibold tracking-tight text-foreground sm:text-4xl">Settings</h1>
            <p className="max-w-2xl text-sm leading-6 text-muted-foreground">
              Manage your account preferences and personal configuration.
            </p>
          </div>
        </div>
      </div>

      <div className="mx-2 lg:mx-4">
        <Card className="border-border/70 bg-card/80 shadow-sm backdrop-blur-sm">
          <CardHeader>
            <CardTitle>Coming soon</CardTitle>
            <CardDescription>Account settings and personal preferences will be available here.</CardDescription>
          </CardHeader>
        </Card>
      </div>
    </section>
  );
}
