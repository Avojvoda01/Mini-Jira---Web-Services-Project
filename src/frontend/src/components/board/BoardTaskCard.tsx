import { useDraggable } from '@dnd-kit/core';
import { Badge } from '@/components/ui/badge';
import { cn } from '@/lib/utils';
import { priorityBadgeClass, type TaskCard, taskCardClass } from './boardModel';

export function TaskCardBody({ task }: { task: TaskCard }) {
  return (
    <>
      <div className="flex items-start justify-between gap-3">
        <div className="min-w-0 flex-1 space-y-2">
          <Badge
            variant="outline"
            className="border-border/70 bg-background/70 text-[0.68rem] uppercase tracking-[0.18em] text-muted-foreground"
          >
            {task.ticket}
          </Badge>
          <h3 className="text-sm font-medium leading-6 text-foreground">
            {task.title}
          </h3>
          {task.description ? (
            <p className="max-w-full text-xs leading-5 text-muted-foreground break-words">
              {task.description}
            </p>
          ) : null}
        </div>
        <Badge className={priorityBadgeClass(task.priority)}>
          {task.priority}
        </Badge>
      </div>

      <div className="mt-4 flex flex-wrap items-center justify-between gap-2 text-xs text-muted-foreground">
        <span>{task.owner}</span>
        <span>{task.estimate}</span>
      </div>
    </>
  );
}

export function DraggableTaskCard({
  task,
  onOpen,
}: {
  task: TaskCard;
  onOpen: () => void;
}) {
  const { attributes, listeners, setNodeRef, isDragging } = useDraggable({
    id: task.taskId,
  });

  return (
    <button
      ref={setNodeRef}
      type="button"
      className={cn(
        taskCardClass(task.priority),
        isDragging && 'opacity-40 hover:translate-y-0',
      )}
      onClick={onOpen}
      {...attributes}
      {...listeners}
    >
      <TaskCardBody task={task} />
    </button>
  );
}
