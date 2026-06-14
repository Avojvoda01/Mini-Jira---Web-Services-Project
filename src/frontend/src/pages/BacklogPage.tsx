import { useEffect, useMemo, useState } from 'react';
import { useParams } from 'react-router-dom';
import { Filter, Plus } from 'lucide-react';
import { AiAssistant } from '@/components/common/AiAssistant';
import { EpicBacklogSection } from '@/components/backlog/EpicBacklogSection';
import { CreateEpicModal } from '@/components/backlog/CreateEpicModal';
import { AssignTicketsModal } from '@/components/backlog/AssignTicketsModal';
import { EditEpicModal } from '@/components/backlog/EditEpicModal';
import { DeleteEpicModal } from '@/components/backlog/DeleteEpicModal';
import { Button } from '@/components/ui/button';
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger } from '@/components/ui/dropdown-menu';
import { ErrorState } from '@/components/ui/ErrorState';
import { usePageHeader } from '@/components/layout/PageHeaderContext';
import {
  useCreateEpicMutation,
  useDeleteEpicMutation,
  useEpicsQuery,
  useUpdateEpicMutation,
  type EpicDto,
} from '@/features/epics';
import { useAssignEpicMutation, useTasksQuery, type TaskItem, type TaskPriority, type TaskStatus } from '@/features/tasks';
import { useUsersQuery } from '@/features/users';
import { formatEstimate } from '@/lib/estimate';

type BacklogTicket = {
  id: string;
  displayId: string;
  title: string;
  description: string;
  priority: 'High' | 'Medium' | 'Low';
  status: string;
  estimate: string;
  estimateMinutes: number | null;
  assigneeName: string | null;
};

type Epic = EpicDto & {
  ticketIds: string[];
};

type EpicSummary = Pick<Epic, 'id' | 'name' | 'description' | 'ticketIds'>;

type EpicSortOption = 'newest' | 'oldest' | 'recently-updated' | 'tickets-desc' | 'tickets-asc' | 'name-asc' | 'name-desc' | 'workload-desc' | 'workload-asc';

const priorityLabelMap: Record<TaskPriority, BacklogTicket['priority']> = {
  high: 'High',
  medium: 'Medium',
  low: 'Low',
  unknown: 'Low',
};

const statusLabelMap: Record<TaskStatus, string> = {
  todo: 'Ready',
  'in-progress': 'In progress',
  review: 'Review',
  done: 'Done',
  unknown: 'Ready',
};

const mapTaskToTicket = (task: TaskItem, displayId: string, userById: Map<string, string>): BacklogTicket => ({
  id: task.id,
  displayId,
  title: task.title,
  description: task.description ?? '',
  priority: priorityLabelMap[task.priority],
  status: statusLabelMap[task.status],
  estimate: formatEstimate(task.estimateMinutes),
  estimateMinutes: task.estimateMinutes,
  assigneeName: task.assigneeId ? (userById.get(task.assigneeId) ?? null) : null,
});

export function BacklogPage() {
  const { setContent } = usePageHeader();
  const { projectId } = useParams();
  const { data: epicDtos = [], isLoading: isLoadingEpics, isError: isEpicsError, error: epicsError, refetch: refetchEpics } = useEpicsQuery({
    projectId: projectId ?? null,
  });
  const {
    data: tasks = [],
    isLoading: isLoadingTickets,
    isError: isTicketsError,
    error: ticketsError,
    refetch: refetchTickets,
  } = useTasksQuery({
    projectId: projectId ?? null,
  });
  const { data: users = [] } = useUsersQuery();
  const userById = useMemo(() => new Map(users.map((u) => [u.id, u.displayName])), [users]);

  const createEpicMutation = useCreateEpicMutation();
  const updateEpicMutation = useUpdateEpicMutation();
  const deleteEpicMutation = useDeleteEpicMutation();
  const assignEpicMutation = useAssignEpicMutation();
  const [isCreateEpicOpen, setIsCreateEpicOpen] = useState(false);
  const [newEpicName, setNewEpicName] = useState('');
  const [newEpicDescription, setNewEpicDescription] = useState('');
  const [newEpicTicketIds, setNewEpicTicketIds] = useState<string[]>([]);
  const [createEpicSearch, setCreateEpicSearch] = useState('');
  const [assignEpicId, setAssignEpicId] = useState<string | null>(null);
  const [assignSearch, setAssignSearch] = useState('');
  const [assignTicketDraft, setAssignTicketDraft] = useState<string[]>([]);
  const [editEpicId, setEditEpicId] = useState<string | null>(null);
  const [editEpicName, setEditEpicName] = useState('');
  const [editEpicDescription, setEditEpicDescription] = useState('');
  const [deleteConfirmEpicId, setDeleteConfirmEpicId] = useState<string | null>(null);
  const [epicSort, setEpicSort] = useState<EpicSortOption>('newest');
  const [createEpicError, setCreateEpicError] = useState<string | null>(null);
  const [assignTicketsError, setAssignTicketsError] = useState<string | null>(null);
  const [ticketActionError, setTicketActionError] = useState<string | null>(null);
  const [isAssigningTickets, setIsAssigningTickets] = useState(false);
  const [isCreatingEpicTickets, setIsCreatingEpicTickets] = useState(false);

  const scopedEpicDtos = useMemo(() => {
    if (!projectId) {
      return epicDtos;
    }

    return epicDtos.filter((epic) => epic.projectId === projectId);
  }, [epicDtos, projectId]);

  const scopedTasks = useMemo(() => {
    if (!projectId) {
      return tasks;
    }

    return tasks.filter((task) => task.projectId === projectId);
  }, [projectId, tasks]);

  const taskDisplayIds = useMemo(() => {
    const sorted = [...scopedTasks].sort((left, right) => {
      const leftDate = Date.parse(left.createdAtUtc);
      const rightDate = Date.parse(right.createdAtUtc);
      return leftDate - rightDate;
    });

    const map = new Map<string, string>();
    sorted.forEach((task, index) => {
      map.set(task.id, `TASK-${String(index + 1).padStart(3, '0')}`);
    });

    return map;
  }, [scopedTasks]);

  const ticketsByEpicId = useMemo(() => {
    const map = new Map<string, string[]>();

    scopedTasks.forEach((task) => {
      if (!task.epicId) {
        return;
      }

      const current = map.get(task.epicId) ?? [];
      current.push(task.id);
      map.set(task.epicId, current);
    });

    return map;
  }, [scopedTasks]);

  const epics = useMemo<Epic[]>(() => {
    return scopedEpicDtos.map((epic) => ({
      ...epic,
      ticketIds: ticketsByEpicId.get(epic.id) ?? [],
    }));
  }, [scopedEpicDtos, ticketsByEpicId]);

  const backlogTickets = useMemo(() => {
    return scopedTasks.map((task) => mapTaskToTicket(task, taskDisplayIds.get(task.id) ?? `TASK-${task.id.slice(0, 6).toUpperCase()}`, userById));
  }, [scopedTasks, taskDisplayIds, userById]);

  const ticketById = useMemo(() => {
    return new Map(backlogTickets.map((ticket) => [ticket.id, ticket]));
  }, [backlogTickets]);

  const sortedEpics = useMemo(() => {
    const next = [...epics];

    const compareByName = (left: Epic, right: Epic) => left.name.localeCompare(right.name, undefined, { sensitivity: 'base' });
    const getCreatedAt = (epic: Epic) => {
      const value = Date.parse(epic.createdAtUtc);
      return Number.isNaN(value) ? 0 : value;
    };
    const getUpdatedAt = (epic: Epic) => {
      const value = Date.parse(epic.updatedAtUtc ?? epic.createdAtUtc);
      return Number.isNaN(value) ? 0 : value;
    };
    const getWorkload = (epic: Epic) =>
      epic.ticketIds.reduce((sum, id) => sum + (ticketById.get(id)?.estimateMinutes ?? 0), 0);

    switch (epicSort) {
      case 'tickets-desc':
        next.sort((left, right) => right.ticketIds.length - left.ticketIds.length);
        break;
      case 'tickets-asc':
        next.sort((left, right) => left.ticketIds.length - right.ticketIds.length);
        break;
      case 'workload-desc':
        next.sort((left, right) => getWorkload(right) - getWorkload(left));
        break;
      case 'workload-asc':
        next.sort((left, right) => getWorkload(left) - getWorkload(right));
        break;
      case 'name-asc':
        next.sort(compareByName);
        break;
      case 'name-desc':
        next.sort((left, right) => compareByName(right, left));
        break;
      case 'recently-updated':
        next.sort((left, right) => getUpdatedAt(right) - getUpdatedAt(left));
        break;
      case 'oldest':
        next.sort((left, right) => getCreatedAt(left) - getCreatedAt(right));
        break;
      case 'newest':
      default:
        next.sort((left, right) => getCreatedAt(right) - getCreatedAt(left));
        break;
    }

    return next;
  }, [epics, epicSort, ticketById]);

  const activeAssignEpic = useMemo(() => {
    if (assignEpicId === null) {
      return undefined;
    }

    return epics.find((epic) => epic.id === assignEpicId);
  }, [assignEpicId, epics]);

  const activeEditEpic = useMemo(() => {
    if (editEpicId === null) {
      return undefined;
    }

    return epics.find((epic) => epic.id === editEpicId);
  }, [editEpicId, epics]);

  const epicPendingDelete = useMemo(() => {
    if (deleteConfirmEpicId === null) {
      return undefined;
    }

    return epics.find((epic) => epic.id === deleteConfirmEpicId);
  }, [deleteConfirmEpicId, epics]);

  const unassignedTickets = useMemo(() => {
    return scopedTasks
      .filter((task) => !task.epicId)
      .map((task) => mapTaskToTicket(task, taskDisplayIds.get(task.id) ?? `TASK-${task.id.slice(0, 6).toUpperCase()}`, userById));
  }, [scopedTasks, taskDisplayIds, userById]);

  const createEpicFilteredTickets = useMemo(() => {
    const normalizedSearch = createEpicSearch.trim().toLowerCase();
    if (!normalizedSearch) {
      return unassignedTickets;
    }

    return unassignedTickets.filter((ticket) => {
      return (
        ticket.displayId.toLowerCase().includes(normalizedSearch) ||
        ticket.id.toLowerCase().includes(normalizedSearch) ||
        ticket.title.toLowerCase().includes(normalizedSearch) ||
        ticket.description.toLowerCase().includes(normalizedSearch)
      );
    });
  }, [createEpicSearch, unassignedTickets]);

  const assignFilteredTickets = useMemo(() => {
    const normalizedSearch = assignSearch.trim().toLowerCase();
    const source = unassignedTickets;

    if (!normalizedSearch) {
      return source;
    }

    return source.filter((ticket) => {
      return (
        ticket.displayId.toLowerCase().includes(normalizedSearch) ||
        ticket.id.toLowerCase().includes(normalizedSearch) ||
        ticket.title.toLowerCase().includes(normalizedSearch) ||
        ticket.description.toLowerCase().includes(normalizedSearch)
      );
    });
  }, [assignSearch, unassignedTickets]);

  const toggleCreateEpicTicket = (ticketId: string) => {
    setNewEpicTicketIds((current) =>
      current.includes(ticketId) ? current.filter((id) => id !== ticketId) : [...current, ticketId],
    );
  };

  const createEpic = async () => {
    const name = newEpicName.trim();
    const description = newEpicDescription.trim();
    if (!projectId) {
      setCreateEpicError('Select a project before creating an epic.');
      return;
    }
    if (name.length < 3) {
      return;
    }

    try {
      setCreateEpicError(null);
      setIsCreatingEpicTickets(true);
      const newEpic = await createEpicMutation.mutateAsync({
        name,
        description: description || null,
        projectId,
      });

      if (newEpicTicketIds.length > 0) {
        await Promise.all(
          newEpicTicketIds.map((ticketId) =>
            assignEpicMutation.mutateAsync({
              taskId: ticketId,
              epicId: newEpic.id,
            }),
          ),
        );
      }

      setNewEpicName('');
      setNewEpicDescription('');
      setNewEpicTicketIds([]);
      setCreateEpicSearch('');
      setIsCreateEpicOpen(false);
    } catch (error) {
      setCreateEpicError(error instanceof Error ? error.message : 'Unable to create epic.');
    } finally {
      setIsCreatingEpicTickets(false);
    }
  };

  const closeCreateEpicModal = () => {
    setIsCreateEpicOpen(false);
    setNewEpicName('');
    setNewEpicDescription('');
    setNewEpicTicketIds([]);
    setCreateEpicSearch('');
    setCreateEpicError(null);
  };

  const openAssignTicketsModal = (epic: EpicSummary) => {
    setAssignEpicId(epic.id);
    setAssignSearch('');
    setAssignTicketDraft([]);
    setAssignTicketsError(null);
  };

  const openEditEpicModal = (epic: EpicSummary) => {
    setEditEpicId(epic.id);
    setEditEpicName(epic.name);
    setEditEpicDescription(epic.description);
  };

  const closeEditEpicModal = () => {
    setEditEpicId(null);
    setEditEpicName('');
    setEditEpicDescription('');
  };

  const saveEpicEdit = async () => {
    const name = editEpicName.trim();
    const description = editEpicDescription.trim();
    if (editEpicId === null || name.length < 3) {
      return;
    }

    try {
      await updateEpicMutation.mutateAsync({
        id: editEpicId,
        name,
        description: description || null,
      });

      closeEditEpicModal();
    } catch (error) {
      console.error('Error updating epic:', error);
    }
  };

  const openDeleteConfirmModal = (epicId: string) => {
    setDeleteConfirmEpicId(epicId);
  };

  const closeDeleteConfirmModal = () => {
    setDeleteConfirmEpicId(null);
  };

  const confirmDeleteEpic = async () => {
    if (deleteConfirmEpicId === null) {
      return;
    }

    try {
      await deleteEpicMutation.mutateAsync(deleteConfirmEpicId);
      await refetchTickets();
      closeDeleteConfirmModal();
    } catch (error) {
      console.error('Error deleting epic:', error);
    }
  };

  const closeAssignTicketsModal = () => {
    setAssignEpicId(null);
    setAssignSearch('');
    setAssignTicketDraft([]);
    setAssignTicketsError(null);
  };

  const toggleAssignDraftTicket = (ticketId: string) => {
    setAssignTicketDraft((current) =>
      current.includes(ticketId) ? current.filter((id) => id !== ticketId) : [...current, ticketId],
    );
  };

  const saveAssignedTickets = async () => {
    if (assignEpicId === null) {
      return;
    }

    try {
      setAssignTicketsError(null);
      setIsAssigningTickets(true);
      await Promise.all(
        assignTicketDraft.map((ticketId) =>
          assignEpicMutation.mutateAsync({
            taskId: ticketId,
            epicId: assignEpicId,
          }),
        ),
      );
      closeAssignTicketsModal();
    } catch (error) {
      setAssignTicketsError(error instanceof Error ? error.message : 'Unable to assign tickets.');
    } finally {
      setIsAssigningTickets(false);
    }
  };

  const removeTicketFromEpic = async (_epicId: string, ticketId: string) => {
    try {
      setTicketActionError(null);
      await assignEpicMutation.mutateAsync({
        taskId: ticketId,
        epicId: null,
      });
    } catch (error) {
      setTicketActionError(error instanceof Error ? error.message : 'Unable to remove ticket from epic.');
    }
  };

  const epicSortLabel = useMemo(() => {
    switch (epicSort) {
      case 'recently-updated':
        return 'Recently updated';
      case 'tickets-desc':
        return 'Most tickets';
      case 'tickets-asc':
        return 'Fewest tickets';
      case 'name-asc':
        return 'Name A-Z';
      case 'name-desc':
        return 'Name Z-A';
      case 'workload-desc':
        return 'Most workload';
      case 'workload-asc':
        return 'Least workload';
      case 'oldest':
        return 'Oldest';
      case 'newest':
      default:
        return 'Newest';
    }
  }, [epicSort]);

  useEffect(() => {
    setContent({
      title: 'Epic Management',
      description: 'Group related work into epics, then assign tickets to each initiative.',
      actions: (
        <div className="flex flex-wrap gap-2">
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="outline" className="border-border/70 bg-background/80 shadow-sm">
                <Filter className="mr-2 h-4 w-4" />
                {epicSortLabel}
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              <DropdownMenuItem onClick={() => setEpicSort('newest')}>Newest</DropdownMenuItem>
              <DropdownMenuItem onClick={() => setEpicSort('oldest')}>Oldest</DropdownMenuItem>
              <DropdownMenuItem onClick={() => setEpicSort('recently-updated')}>Recently updated</DropdownMenuItem>
              <DropdownMenuItem onClick={() => setEpicSort('tickets-desc')}>Most tickets</DropdownMenuItem>
              <DropdownMenuItem onClick={() => setEpicSort('tickets-asc')}>Fewest tickets</DropdownMenuItem>
              <DropdownMenuItem onClick={() => setEpicSort('name-asc')}>Name A-Z</DropdownMenuItem>
              <DropdownMenuItem onClick={() => setEpicSort('name-desc')}>Name Z-A</DropdownMenuItem>
              <DropdownMenuItem onClick={() => setEpicSort('workload-desc')}>Most workload</DropdownMenuItem>
              <DropdownMenuItem onClick={() => setEpicSort('workload-asc')}>Least workload</DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
          <Button className="shadow-sm" onClick={() => {
            setCreateEpicError(null);
            setIsCreateEpicOpen(true);
          }}>
            <Plus className="mr-2 h-4 w-4" />
            Create epic
          </Button>
        </div>
      ),
    });

    return () => setContent({});
  }, [epicSortLabel, setContent]);

  return (
    <section className="space-y-6">
      {ticketActionError ? (
        <ErrorState title="Unable to update tickets" description={ticketActionError} />
      ) : null}

      {isTicketsError ? (
        <ErrorState
          title="Unable to load tickets"
          description={ticketsError instanceof Error ? ticketsError.message : 'Check your connection and try again.'}
          onRetry={refetchTickets}
        />
      ) : null}

      <EpicBacklogSection
        isLoading={isLoadingEpics}
        isError={isEpicsError}
        error={epicsError}
        onRetry={refetchEpics}
        epics={sortedEpics}
        ticketById={ticketById}
        onAssignTickets={openAssignTicketsModal}
        onEdit={openEditEpicModal}
        onDelete={openDeleteConfirmModal}
        onRemoveTicket={removeTicketFromEpic}
      />

      <AssignTicketsModal
        isOpen={Boolean(activeAssignEpic)}
        epicName={activeAssignEpic?.name ?? ''}
        onClose={closeAssignTicketsModal}
        assignSearch={assignSearch}
        setAssignSearch={setAssignSearch}
        assignTicketDraft={assignTicketDraft}
        toggleAssignDraftTicket={toggleAssignDraftTicket}
        assignFilteredTickets={assignFilteredTickets}
        unassignedTickets={unassignedTickets}
        onSave={saveAssignedTickets}
        isLoading={isLoadingTickets}
        isError={isTicketsError}
        error={ticketsError instanceof Error ? ticketsError : null}
        onRetry={refetchTickets}
        isPending={isAssigningTickets}
        submitError={assignTicketsError}
      />

      <EditEpicModal
        isOpen={Boolean(activeEditEpic)}
        epicName={editEpicName}
        epicDescription={editEpicDescription}
        onClose={closeEditEpicModal}
        onChangeName={setEditEpicName}
        onChangeDescription={setEditEpicDescription}
        onSave={saveEpicEdit}
        isPending={updateEpicMutation.isPending}
      />

      <DeleteEpicModal
        isOpen={Boolean(epicPendingDelete)}
        epicName={epicPendingDelete?.name ?? ''}
        onClose={closeDeleteConfirmModal}
        onConfirm={confirmDeleteEpic}
        isPending={deleteEpicMutation.isPending}
      />

      <CreateEpicModal
        isOpen={isCreateEpicOpen}
        onClose={closeCreateEpicModal}
        newEpicName={newEpicName}
        setNewEpicName={setNewEpicName}
        newEpicDescription={newEpicDescription}
        setNewEpicDescription={setNewEpicDescription}
        createEpicSearch={createEpicSearch}
        setCreateEpicSearch={setCreateEpicSearch}
        newEpicTicketIds={newEpicTicketIds}
        toggleCreateEpicTicket={toggleCreateEpicTicket}
        createEpicFilteredTickets={createEpicFilteredTickets}
        unassignedTickets={unassignedTickets}
        onCreate={createEpic}
        isPending={createEpicMutation.isPending}
        isTicketsLoading={isLoadingTickets}
        isTicketsError={isTicketsError}
        ticketsError={ticketsError instanceof Error ? ticketsError : null}
        onRetryTickets={refetchTickets}
        submitError={createEpicError}
        isAssigning={isCreatingEpicTickets}
      />

      <AiAssistant
        greeting="I can help group tickets into epics, balance workload, or summarize progress once the data is connected."
        placeholder="Ask about epics or workload..."
      />
    </section>
  );
}
