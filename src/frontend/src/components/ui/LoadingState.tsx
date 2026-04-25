import { Loader2 } from 'lucide-react';
import { Card, CardContent } from '@/components/ui/card';

type LoadingStateProps = {
  label?: string;
};

export function LoadingState({ label = 'Loading workspace...' }: LoadingStateProps) {
  return (
    <Card className="border-border/70 bg-card/80 shadow-sm backdrop-blur-sm" role="status" aria-live="polite">
      <CardContent className="flex items-center gap-3 p-4">
        <Loader2 className="h-4 w-4 animate-spin text-muted-foreground" aria-hidden="true" />
        <p className="text-sm font-medium text-foreground">{label}</p>
      </CardContent>
    </Card>
  );
}
