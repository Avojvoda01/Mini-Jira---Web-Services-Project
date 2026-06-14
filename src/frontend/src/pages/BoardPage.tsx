import { useEffect, useMemo, useState } from 'react';
import { createPortal } from 'react-dom';
import {
  DndContext,
  DragOverlay,
  PointerSensor,
  useSensor,
  useSensors,
  type DragEndEvent,
  type DragStartEvent,
} from '@dnd-kit/core';
import { useQueryClient } from '@tanstack/react-query';
import { useAtomValue } from 'jotai';
import { useParams } from 'react-router-dom';
import { Plus } from 'lucide-react';
import { AiAssistant } from '@/components/common/AiAssistant';
import { CreateTaskModal } from '@/components/board/CreateTaskModal';
import { DeleteTaskModal } from '@/components/board/DeleteTaskModal';
import { EditTaskModal } from '@/components/board/EditTaskModal';
import { BoardColumnCard } from '@/components/board/BoardColumnCard';
import { TaskCardBody } from '@/components/board/BoardTaskCard';
import { TaskDetailPanel } from '@/components/board/TaskDetailPanel';
import {
  columnConfig,
  columnStatusMap,
  columnTaskStatusMap,
  MAX_TASK_DESCRIPTION_LENGTH,
  priorityLabelMap,
  statusColumnId,
  taskCardClass,
  truncateText,
  type BoardColumn,
  type TaskCard,
} from '@/components/board/boardModel';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { ErrorState } from '@/components/ui/ErrorState';
import { usePageHeader } from '@/components/layout/PageHeaderContext';
import { useEpicsQuery } from '@/features/epics';
import { useProjectQuery } from '@/features/projects';
import { taskQueryKeys, useChangeTaskStatusMutation, useDeleteTaskMutation, useTasksQuery, type TaskItem } from '@/features/tasks';
import { useUsersQuery } from '@/features/users';
import { formatEstimate } from '@/lib/estimate';
import { cn } from '@/lib/utils';
import { authSessionAtom } from '@/store/authAtoms';

export function BoardPage() {
  const session = useAtomValue(authSessionAtom);
  const { setContent } = usePageHeader();
  const { projectId } = useParams();
  const { data: project } = useProjectQuery(projectId ?? null);
  const { data: users = [] } = useUsersQuery();
  const { data: tasks = [], isLoading, isError, error, refetch } = useTasksQuery({
    projectId: projectId ?? null,
  });
  const { data: epics = [] } = useEpicsQuery({ projectId: projectId ?? null });
  const deleteTaskMutation = useDeleteTaskMutation();
  const changeStatusMutation = useChangeTaskStatusMutation();
  const queryClient = useQueryClient();
  const dragSensors = useSensors(useSensor(PointerSensor, { activationConstraint: { distance: 6 } }));
  const [activeDragTask, setActiveDragTask] = useState<TaskCard | null>(null);
  const [createColumnId, setCreateColumnId] = useState<BoardColumn['id'] | null>(null);
  const [editTaskId, setEditTaskId] = useState<string | null>(null);
  const [detailTaskId, setDetailTaskId] = useState<string | null>(null);
  const [deleteTaskId, setDeleteTaskId] = useState<string | null>(null);

  const taskDisplayIds = useMemo(() => {
    const sorted = [...tasks].sort((left, right) => {
      const leftDate = Date.parse(left.createdAtUtc);
      const rightDate = Date.parse(right.createdAtUtc);
      return leftDate - rightDate;
    });

    const map = new Map<string, string>();
    sorted.forEach((task, index) => {
      map.set(task.id, `TASK-${String(index + 1).padStart(3, '0')}`);
    });

    return map;
  }, [tasks]);

  const taskById = useMemo(() => new Map(tasks.map((task) => [task.id, task])), [tasks]);

  const usersById = useMemo(
    () => new Map(users.map((user) => [user.id.toLowerCase(), user.displayName])),
    [users],
  );

  const assignableUsers = useMemo(() => {
    if (!projectId) {
      return [];
    }

    const participantIds = new Set([
      ...(project?.memberIds ?? []).map((id) => id.toLowerCase()),
      ...(project?.createdById ? [project.createdById.toLowerCase()] : []),
    ]);

    return users.filter((user) => participantIds.has(user.id.toLowerCase()));
  }, [project?.memberIds, project?.createdById, projectId, users]);

  const resolveUserDisplayName = (userId: string | null) => {
    if (!userId) {
      return 'Unassigned';
    }

    return usersById.get(userId.toLowerCase()) ?? `User ${userId.slice(0, 6)}`;
  };

  const ticketId = (task: TaskItem) =>
    taskDisplayIds.get(task.id) ?? `TASK-${task.id.slice(0, 6).toUpperCase()}`;

  const boardColumns = useMemo<BoardColumn[]>(() => {
    const toCard = (task: TaskItem): TaskCard => ({
      taskId: task.id,
      ticket: taskDisplayIds.get(task.id) ?? `TASK-${task.id.slice(0, 6).toUpperCase()}`,
      title: task.title,
      description: truncateText(task.description ?? '', MAX_TASK_DESCRIPTION_LENGTH),
      owner: resolveUserDisplayName(task.assigneeId),
      priority: priorityLabelMap[task.priority],
      estimate: formatEstimate(task.estimateMinutes),
    });

    const byColumn = new Map<BoardColumn['id'], TaskCard[]>([
      ['ready', []],
      ['in-progress', []],
      ['review', []],
      ['done', []],
    ]);

    tasks.forEach((task) => {
      byColumn.get(statusColumnId(task.status))?.push(toCard(task));
    });

    return columnConfig.map((column) => ({
      ...column,
      tasks: byColumn.get(column.id) ?? [],
    }));
  }, [taskDisplayIds, tasks, usersById]);

  const activeEditTask = editTaskId ? taskById.get(editTaskId) ?? null : null;
  const activeDetailTask = detailTaskId ? taskById.get(detailTaskId) ?? null : null;
  const activeDeleteTask = deleteTaskId ? taskById.get(deleteTaskId) ?? null : null;

  const handleDragStart = (event: DragStartEvent) => {
    const card = boardColumns.flatMap((column) => column.tasks).find((task) => task.taskId === event.active.id);
    setActiveDragTask(card ?? null);
  };

  const handleDragEnd = (event: DragEndEvent) => {
    setActiveDragTask(null);

    const targetColumnId = event.over?.id as BoardColumn['id'] | undefined;
    const task = taskById.get(String(event.active.id));
    if (!targetColumnId || !task || statusColumnId(task.status) === targetColumnId) {
      return;
    }

    queryClient.setQueriesData<TaskItem[]>({ queryKey: taskQueryKeys.all }, (current) =>
      current?.map((item) => (item.id === task.id ? { ...item, status: columnTaskStatusMap[targetColumnId] } : item)),
    );
    changeStatusMutation.mutate(
      { taskId: task.id, status: columnStatusMap[targetColumnId] },
      { onError: () => queryClient.invalidateQueries({ queryKey: taskQueryKeys.all }) },
    );
  };

  const totalTasks = boardColumns.reduce((total, column) => total + column.tasks.length, 0);
  const inProgressCount = boardColumns.find((column) => column.id === 'in-progress')?.tasks.length ?? 0;
  const doneCount = boardColumns.find((column) => column.id === 'done')?.tasks.length ?? 0;

  useEffect(() => {
    setContent({
      title: 'Board',
      description: 'Move tickets through your workflow and keep the team aligned.',
      meta: (
        <>
          <Badge variant="secondary" className="border border-border/60 bg-background/80 text-foreground">
            {totalTasks} tickets active
          </Badge>
          <Badge variant="outline" className="border-border/70 bg-background/60 text-muted-foreground">
            {inProgressCount} in progress
          </Badge>
          <Badge variant="outline" className="border-border/70 bg-background/60 text-muted-foreground">
            {doneCount} done
          </Badge>
        </>
      ),
      actions: (
        <div className="flex flex-wrap gap-2">
          <Button
            variant="outline"
            className="border-border/70 bg-background/80 shadow-sm"
            onClick={() => setCreateColumnId('ready')}
          >
            <Plus className="mr-2 h-4 w-4" />
            Add ticket
          </Button>
        </div>
      ),
    });

    return () => setContent({});
  }, [doneCount, inProgressCount, setContent, totalTasks]);

  useEffect(() => {
    document.documentElement.classList.add('board-no-scroll');
    return () => document.documentElement.classList.remove('board-no-scroll');
  }, []);

  return (
    <section className="relative space-y-6">
      <CreateTaskModal
        isOpen={createColumnId !== null}
        onClose={() => setCreateColumnId(null)}
        projectId={projectId ?? null}
        defaultStatus={createColumnId ? columnStatusMap[createColumnId] : 'Ready'}
        columnLabel={createColumnId ? columnConfig.find((column) => column.id === createColumnId)?.title ?? 'Ready' : 'Ready'}
        assignableUsers={assignableUsers}
        epics={epics}
      />

      <EditTaskModal
        isOpen={editTaskId !== null}
        onClose={() => setEditTaskId(null)}
        onSave={() => {
          setEditTaskId(null);
          setDetailTaskId(null);
        }}
        task={activeEditTask}
        assignableUsers={assignableUsers}
        epics={epics}
      />

      <DeleteTaskModal
        isOpen={deleteTaskId !== null}
        taskTitle={activeDeleteTask?.title ?? 'this ticket'}
        onClose={() => setDeleteTaskId(null)}
        onConfirm={async () => {
          if (!activeDeleteTask) {
            return;
          }

          await deleteTaskMutation.mutateAsync({ taskId: activeDeleteTask.id });
          setDeleteTaskId(null);
          setDetailTaskId(null);
        }}
        isPending={deleteTaskMutation.isPending}
      />

      {activeDetailTask ? (
        <TaskDetailPanel
          task={activeDetailTask}
          ticketId={ticketId(activeDetailTask)}
          assignableUsers={assignableUsers}
          epics={epics}
          session={session}
          isEditModalOpen={editTaskId !== null}
          resolveUserDisplayName={resolveUserDisplayName}
          onClose={() => setDetailTaskId(null)}
          onEdit={() => setEditTaskId(activeDetailTask.id)}
          onRequestDelete={() => setDeleteTaskId(activeDetailTask.id)}
        />
      ) : null}

      {isError ? (
        <ErrorState
          title="Unable to load tasks"
          description={error instanceof Error ? error.message : 'Check your connection and try again.'}
          onRetry={() => refetch()}
        />
      ) : null}

      <DndContext
        sensors={dragSensors}
        onDragStart={handleDragStart}
        onDragEnd={handleDragEnd}
        onDragCancel={() => setActiveDragTask(null)}
      >
        <div className="mt-2 grid gap-4 lg:grid-cols-2 xl:grid-cols-4 xl:h-[calc(100dvh-11.5rem)]">
          {boardColumns.map((column) => (
            <BoardColumnCard
              key={column.id}
              column={column}
              isLoading={isLoading}
              isError={isError}
              onAddTicket={() => setCreateColumnId(column.id)}
              onOpenTask={(taskId) => setDetailTaskId(taskId)}
            />
          ))}
        </div>

        {createPortal(
          <DragOverlay>
            {activeDragTask ? (
              <div className={cn(taskCardClass(activeDragTask.priority), 'cursor-grabbing shadow-xl')}>
                <TaskCardBody task={activeDragTask} />
              </div>
            ) : null}
          </DragOverlay>,
          document.body,
        )}
      </DndContext>

      <AiAssistant
        greeting="I can help sort tasks, spot blockers, or summarize this board once the data is connected."
        placeholder="Ask about your tasks, priorities, or project..."
      />
    </section>
  );
}
