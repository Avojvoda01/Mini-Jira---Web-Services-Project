import { Filter, Plus, Sparkles } from 'lucide-react';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';

type BacklogPageHeaderProps = {
  projectName?: string;
  title: string;
  description: string;
  onCreateEpic: () => void;
};

export function BacklogPageHeader({ projectName, title, description, onCreateEpic }: BacklogPageHeaderProps) {
  return (
    <Card className="border-border/70 bg-card/80 shadow-sm backdrop-blur-sm">
      <CardContent className="p-6 sm:p-8">
        <div className="flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between">
          <div className="space-y-4">
            <Badge variant="outline" className="w-fit border-border/70 bg-background/70 text-muted-foreground">
              <Sparkles className="mr-1.5 h-3.5 w-3.5" />
              {projectName ?? 'Backlog planning'}
            </Badge>

            <div className="space-y-2">
              <h2 className="text-3xl font-semibold tracking-tight text-foreground sm:text-4xl">{title}</h2>
              <p className="max-w-2xl text-sm leading-6 text-muted-foreground sm:text-base">{description}</p>
            </div>
          </div>

          <div className="flex flex-wrap gap-2">
            <Button variant="outline" className="border-border/70 bg-background/80 shadow-sm">
              <Filter className="mr-2 h-4 w-4" />
              Filters
            </Button>
            <Button className="shadow-sm" onClick={onCreateEpic}>
              <Plus className="mr-2 h-4 w-4" />
              Create epic
            </Button>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}