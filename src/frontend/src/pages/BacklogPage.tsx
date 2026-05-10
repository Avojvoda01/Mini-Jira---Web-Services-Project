import { useEffect, useMemo, useState } from 'react';
import { Filter, Plus } from 'lucide-react';
import { EpicBacklogSection } from '@/components/backlog/EpicBacklogSection';
import { CreateEpicModal } from '@/components/backlog/CreateEpicModal';
import { AssignTicketsModal } from '@/components/backlog/AssignTicketsModal';
import { EditEpicModal } from '@/components/backlog/EditEpicModal';
import { DeleteEpicModal } from '@/components/backlog/DeleteEpicModal';
import { Button } from '@/components/ui/button';
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger } from '@/components/ui/dropdown-menu';
import { usePageHeader } from '@/components/layout/PageHeaderContext';
import {
  useCreateEpicMutation,
  useDeleteEpicMutation,
  useEpicsQuery,
  useUpdateEpicMutation,
  type EpicDto,
} from '@/features/epics';

type BacklogTicket = {
  id: string;
  title: string;
  description: string;
  priority: 'High' | 'Medium' | 'Low';
  status: string;
  estimate: string;
};

type Epic = EpicDto & {
  ticketIds: string[];
};

type EpicSummary = Pick<Epic, 'id' | 'name' | 'description' | 'ticketIds'>;

type EpicSortOption = 'newest' | 'oldest' | 'recently-updated' | 'tickets-desc' | 'tickets-asc' | 'name-asc' | 'name-desc';

const backlogItems: BacklogTicket[] = [
  {
    id: 'MJR-141',
    title: 'Add project-level quick filters',
    description: 'Make backlog triage faster for product and engineering leads.',
    priority: 'High',
    status: 'Ready for refinement',
    estimate: '5 pts',
  },
  {
    id: 'MJR-138',
    title: 'Improve issue description formatting',
    description: 'Support cleaner acceptance criteria and richer task context.',
    priority: 'Medium',
    status: 'Needs design input',
    estimate: '3 pts',
  },
  {
    id: 'MJR-135',
    title: 'Add notification preference controls',
    description: 'Allow workspace users to tune updates without leaving the app.',
    priority: 'Low',
    status: 'Queued',
    estimate: '2 pts',
  },
  {
    id: 'MJR-147',
    title: 'Add audit trail to ticket transitions',
    description: 'Capture status history for compliance and timeline review.',
    priority: 'High',
    status: 'Ready for refinement',
    estimate: '8 pts',
  },
];

export function BacklogPage() {
  const { setContent } = usePageHeader();
  const { data: epicDtos = [], isLoading: isLoadingEpics } = useEpicsQuery();
  const createEpicMutation = useCreateEpicMutation();
  const updateEpicMutation = useUpdateEpicMutation();
  const deleteEpicMutation = useDeleteEpicMutation();
  const [isCreateEpicOpen, setIsCreateEpicOpen] = useState(false);
  const [ticketAssignmentsByEpic, setTicketAssignmentsByEpic] = useState<Record<string, string[]>>({});
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

  const epics = useMemo<Epic[]>(() => {
    return epicDtos.map((epic) => ({
      ...epic,
      ticketIds: ticketAssignmentsByEpic[epic.id] ?? [],
    }));
  }, [epicDtos, ticketAssignmentsByEpic]);

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

    switch (epicSort) {
      case 'tickets-desc':
        next.sort((left, right) => right.ticketIds.length - left.ticketIds.length);
        break;
      case 'tickets-asc':
        next.sort((left, right) => left.ticketIds.length - right.ticketIds.length);
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
  }, [epics, epicSort]);

  const ticketById = useMemo(() => {
    return new Map(backlogItems.map((ticket) => [ticket.id, ticket]));
  }, []);

  const ticketToEpicMap = useMemo(() => {
    const map = new Map<string, string>();
    epics.forEach((epic) => {
      epic.ticketIds.forEach((ticketId) => {
        map.set(ticketId, epic.id);
      });
    });
    return map;
  }, [epics]);

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
    return backlogItems.filter((ticket) => !ticketToEpicMap.has(ticket.id));
  }, [ticketToEpicMap]);

  const createEpicFilteredTickets = useMemo(() => {
    const normalizedSearch = createEpicSearch.trim().toLowerCase();
    if (!normalizedSearch) {
      return unassignedTickets;
    }

    return unassignedTickets.filter((ticket) => {
      return (
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
    if (name.length < 3) {
      return;
    }

    try {
      const newEpic = await createEpicMutation.mutateAsync({
        name,
        description: description || null,
      });

      if (newEpicTicketIds.length > 0) {
        setTicketAssignmentsByEpic((current) => ({
          ...current,
          [newEpic.id]: newEpicTicketIds,
        }));
      }

      setNewEpicName('');
      setNewEpicDescription('');
      setNewEpicTicketIds([]);
      setCreateEpicSearch('');
      setIsCreateEpicOpen(false);
    } catch (error) {
      console.error('Error creating epic:', error);
    }
  };

  const closeCreateEpicModal = () => {
    setIsCreateEpicOpen(false);
    setNewEpicName('');
    setNewEpicDescription('');
    setNewEpicTicketIds([]);
    setCreateEpicSearch('');
  };

  const openAssignTicketsModal = (epic: EpicSummary) => {
    setAssignEpicId(epic.id);
    setAssignSearch('');
    setAssignTicketDraft([]);
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
      setTicketAssignmentsByEpic((current) => {
        const next = { ...current };
        delete next[deleteConfirmEpicId];
        return next;
      });
      closeDeleteConfirmModal();
    } catch (error) {
      console.error('Error deleting epic:', error);
    }
  };

  const closeAssignTicketsModal = () => {
    setAssignEpicId(null);
    setAssignSearch('');
    setAssignTicketDraft([]);
  };

  const toggleAssignDraftTicket = (ticketId: string) => {
    setAssignTicketDraft((current) =>
      current.includes(ticketId) ? current.filter((id) => id !== ticketId) : [...current, ticketId],
    );
  };

  const saveAssignedTickets = () => {
    if (assignEpicId === null) {
      return;
    }

    setTicketAssignmentsByEpic((current) => {
      const previous = current[assignEpicId] ?? [];
      return {
        ...current,
        [assignEpicId]: Array.from(new Set([...previous, ...assignTicketDraft])),
      };
    });

    closeAssignTicketsModal();
  };

  const removeTicketFromEpic = (epicId: string, ticketId: string) => {
    setTicketAssignmentsByEpic((current) => ({
      ...current,
      [epicId]: (current[epicId] ?? []).filter((id) => id !== ticketId),
    }));
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
      case 'oldest':
        return 'Oldest';
      case 'newest':
      default:
        return 'Newest';
    }
  }, [epicSort]);

  useEffect(() => {
    setContent({
      title: 'Backlog',
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
            </DropdownMenuContent>
          </DropdownMenu>
          <Button className="shadow-sm" onClick={() => setIsCreateEpicOpen(true)}>
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
      <EpicBacklogSection
        isLoading={isLoadingEpics}
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
      />
    </section>
  );
}
