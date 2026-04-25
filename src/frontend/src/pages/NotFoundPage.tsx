import { Link } from 'react-router-dom';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';

export function NotFoundPage() {
  return (
    <section className="flex min-h-[60vh] items-center justify-center px-4 py-10">
      <Card className="w-full max-w-xl border-border/70 bg-card/80 text-center shadow-sm backdrop-blur-sm">
        <CardHeader className="space-y-4">
          <Badge variant="outline" className="mx-auto w-fit border-border/70 bg-background/70 text-muted-foreground">
            404
          </Badge>
          <CardTitle className="text-3xl font-semibold tracking-tight sm:text-4xl">Page not found</CardTitle>
          <CardDescription className="mx-auto max-w-lg text-sm leading-6 sm:text-base">
            The route you opened does not exist in this workspace yet.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <Button asChild className="shadow-sm">
            <Link to="/">Go to homepage</Link>
          </Button>
        </CardContent>
      </Card>
    </section>
  );
}
