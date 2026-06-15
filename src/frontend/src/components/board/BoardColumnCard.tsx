import { useDroppable } from '@dnd-kit/core';
import { Plus } from 'lucide-react';
import { type ReactNode, useEffect, useRef, useState } from 'react';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import { Separator } from '@/components/ui/separator';
import { cn } from '@/lib/utils';
import { DraggableTaskCard } from './BoardTaskCard';
import type { BoardColumn } from './boardModel';

function ColumnContent({
  columnId,
  children,
}: {
  columnId: BoardColumn['id'];
  children: ReactNode;
}) {
  const [scrolling, setScrolling] = useState(false);
  const timerRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const { setNodeRef, isOver } = useDroppable({ id: columnId });

  useEffect(
    () => () => {
      if (timerRef.current) clearTimeout(timerRef.current);
    },
    [],
  );

  const handleScroll = () => {
    setScrolling(true);
    if (timerRef.current) clearTimeout(timerRef.current);
    timerRef.current = setTimeout(() => setScrolling(false), 2500);
  };

  return (
    <CardContent
      ref={setNodeRef}
      className={cn(
        'space-y-3 pb-3 xl:flex-1 xl:overflow-y-auto xl:min-h-0 board-col-scroll',
        scrolling && 'is-scrolling',
        isOver && 'bg-primary/5',
      )}
      onScroll={handleScroll}
    >
      {children}
    </CardContent>
  );
}

type BoardColumnCardProps = {
  column: BoardColumn;
  isLoading: boolean;
  isError: boolean;
  onAddTicket: () => void;
  onOpenTask: (taskId: string) => void;
};

export function BoardColumnCard({
  column,
  isLoading,
  isError,
  onAddTicket,
  onOpenTask,
}: BoardColumnCardProps) {
  return (
    <Card className="border-border/70 bg-muted/20 shadow-sm xl:flex xl:flex-col xl:h-full xl:overflow-hidden">
      <CardHeader className="space-y-3 pb-4 xl:shrink-0">
        <div className="flex items-start justify-between gap-3">
          <div>
            <CardTitle>{column.title}</CardTitle>
            <CardDescription className="mt-1">
              {column.description}
            </CardDescription>
          </div>
          <div className="flex items-center gap-2">
            <Button
              variant="outline"
              size="icon"
              className="h-8 w-8 border-border/70 bg-background/70 text-muted-foreground"
              onClick={onAddTicket}
              aria-label={`Add ticket to ${column.title}`}
            >
              <Plus className="h-4 w-4" />
            </Button>
            <Badge
              variant="outline"
              className="border-border/70 bg-background/70 text-muted-foreground"
            >
              {column.tasks.length}
            </Badge>
          </div>
        </div>
        <Separator />
      </CardHeader>

      <ColumnContent columnId={column.id}>
        {isLoading ? (
          <div
            className="flex items-center justify-center rounded-2xl border border-dashed border-border/70 bg-background/60 p-6"
            role="status"
            aria-live="polite"
          >
            <div
              className="h-9 w-9 animate-spin rounded-full border-2 border-border/60 border-t-foreground/70"
              aria-hidden="true"
            />
            <span className="sr-only">Loading tasks</span>
          </div>
        ) : isError ? (
          <div className="rounded-2xl border border-dashed border-border/70 bg-background/60 p-4 text-sm text-muted-foreground">
            Unable to load tasks.
          </div>
        ) : column.tasks.length === 0 ? (
          <div className="rounded-2xl border border-dashed border-border/70 bg-background/60 p-4 text-sm text-muted-foreground">
            No tasks here yet.
          </div>
        ) : (
          column.tasks.map((task) => (
            <DraggableTaskCard
              key={task.taskId}
              task={task}
              onOpen={() => onOpenTask(task.taskId)}
            />
          ))
        )}
      </ColumnContent>
    </Card>
  );
}
