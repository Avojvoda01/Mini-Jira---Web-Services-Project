import { TriangleAlert } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';

type ErrorStateProps = {
  title?: string;
  description?: string;
  onRetry?: () => void;
};

export function ErrorState({
  title = 'Something went wrong',
  description = 'Try refreshing or retrying the request.',
  onRetry,
}: ErrorStateProps) {
  return (
    <Card className="border-rose-200/70 bg-rose-50/80 shadow-sm backdrop-blur-sm" role="alert">
      <CardHeader className="space-y-3">
        <div className="flex items-center gap-2 text-rose-700">
          <TriangleAlert className="h-4 w-4" />
          <CardTitle className="text-base text-rose-900">{title}</CardTitle>
        </div>
        <CardDescription className="text-rose-700/90">{description}</CardDescription>
      </CardHeader>
      {onRetry ? (
        <CardContent>
          <Button type="button" variant="outline" className="border-rose-200 bg-white text-rose-900 hover:bg-rose-100" onClick={onRetry}>
            Retry
          </Button>
        </CardContent>
      ) : null}
    </Card>
  );
}
