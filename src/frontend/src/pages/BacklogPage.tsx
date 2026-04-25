import { useMemo, useState } from 'react';
import { Filter, Pencil, Plus, Search, Sparkles, Trash2 } from 'lucide-react';
import { useParams } from 'react-router-dom';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { FormActionButtons } from '@/components/common/FormActionButtons';
import { Input } from '@/components/ui/input';
import { Separator } from '@/components/ui/separator';
import { getProjectById } from '@/features/projects/projectData';

type BacklogTicket = {
  id: string;
  title: string;
  description: string;
  priority: 'High' | 'Medium' | 'Low';
  status: string;
  estimate: string;
};

type Epic = {
  id: string;
  name: string;
  description: string;
  ticketIds: string[];
};

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

const initialEpics: Epic[] = [
  {
    id: 'epic-ui-foundation',
    name: 'UI Foundation Stabilization',
    description: 'Consolidate UX quality and consistency across project views.',
    ticketIds: ['MJR-141'],
  },
];

export function BacklogPage() {
  const { projectId } = useParams();
  const project = getProjectById(projectId);
  const [isCreateEpicOpen, setIsCreateEpicOpen] = useState(false);
  const [epics, setEpics] = useState<Epic[]>(initialEpics);
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
    if (!assignEpicId) {
      return undefined;
    }

    return epics.find((epic) => epic.id === assignEpicId);
  }, [assignEpicId, epics]);

  const activeEditEpic = useMemo(() => {
    if (!editEpicId) {
      return undefined;
    }

    return epics.find((epic) => epic.id === editEpicId);
  }, [editEpicId, epics]);

  const epicPendingDelete = useMemo(() => {
    if (!deleteConfirmEpicId) {
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

  const createEpic = () => {
    const name = newEpicName.trim();
    const description = newEpicDescription.trim();
    if (name.length < 3) {
      return;
    }

    const epic: Epic = {
      id: `epic-${Date.now()}`,
      name,
      description,
      ticketIds: newEpicTicketIds,
    };

    setEpics((current) => [epic, ...current]);
    setNewEpicName('');
    setNewEpicDescription('');
    setNewEpicTicketIds([]);
    setCreateEpicSearch('');
    setIsCreateEpicOpen(false);
  };

  const closeCreateEpicModal = () => {
    setIsCreateEpicOpen(false);
    setNewEpicName('');
    setNewEpicDescription('');
    setNewEpicTicketIds([]);
    setCreateEpicSearch('');
  };

  const openAssignTicketsModal = (epic: Epic) => {
    setAssignEpicId(epic.id);
    setAssignSearch('');
    setAssignTicketDraft([]);
  };

  const openEditEpicModal = (epic: Epic) => {
    setEditEpicId(epic.id);
    setEditEpicName(epic.name);
    setEditEpicDescription(epic.description);
  };

  const closeEditEpicModal = () => {
    setEditEpicId(null);
    setEditEpicName('');
    setEditEpicDescription('');
  };

  const saveEpicEdit = () => {
    const name = editEpicName.trim();
    const description = editEpicDescription.trim();
    if (!editEpicId || name.length < 3) {
      return;
    }

    setEpics((current) =>
      current.map((epic) => {
        if (epic.id !== editEpicId) {
          return epic;
        }

        return {
          ...epic,
          name,
          description,
        };
      }),
    );

    closeEditEpicModal();
  };

  const openDeleteConfirmModal = (epicId: string) => {
    setDeleteConfirmEpicId(epicId);
  };

  const closeDeleteConfirmModal = () => {
    setDeleteConfirmEpicId(null);
  };

  const confirmDeleteEpic = () => {
    if (!deleteConfirmEpicId) {
      return;
    }

    setEpics((current) => current.filter((epic) => epic.id !== deleteConfirmEpicId));
    closeDeleteConfirmModal();
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
    if (!assignEpicId) {
      return;
    }

    setEpics((current) =>
      current.map((epic) => {
        if (epic.id !== assignEpicId) {
          return epic;
        }

        return {
          ...epic,
          ticketIds: Array.from(new Set([...epic.ticketIds, ...assignTicketDraft])),
        };
      }),
    );

    closeAssignTicketsModal();
  };

  const removeTicketFromEpic = (epicId: string, ticketId: string) => {
    setEpics((current) =>
      current.map((epic) => {
        if (epic.id !== epicId) {
          return epic;
        }

        return {
          ...epic,
          ticketIds: epic.ticketIds.filter((id) => id !== ticketId),
        };
      }),
    );
  };

  return (
    <section className="space-y-6">
      <Card className="border-border/70 bg-card/80 shadow-sm backdrop-blur-sm">
        <CardContent className="p-6 sm:p-8">
          <div className="flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between">
            <div className="space-y-4">
              <Badge variant="outline" className="w-fit border-border/70 bg-background/70 text-muted-foreground">
                <Sparkles className="mr-1.5 h-3.5 w-3.5" />
                {project?.name ?? 'Backlog planning'}
              </Badge>

              <div className="space-y-2">
                <h2 className="text-3xl font-semibold tracking-tight text-foreground sm:text-4xl">
                  {project ? `${project.name} Backlog` : 'Backlog'}
                </h2>
                <p className="max-w-2xl text-sm leading-6 text-muted-foreground sm:text-base">
                  Group related work into epics, then assign tickets to each initiative.
                </p>
              </div>
            </div>

            <div className="flex flex-wrap gap-2">
              <Button variant="outline" className="border-border/70 bg-background/80 shadow-sm">
                <Filter className="mr-2 h-4 w-4" />
                Filters
              </Button>
              <Button className="shadow-sm" onClick={() => setIsCreateEpicOpen(true)}>
                <Plus className="mr-2 h-4 w-4" />
                Create epic
              </Button>
            </div>
          </div>
        </CardContent>
      </Card>

      <div>
        <Card className="border-border/70 bg-card/80 shadow-sm backdrop-blur-sm">
          <CardHeader className="space-y-3 pb-4">
            <CardTitle>Epic backlog</CardTitle>
            <CardDescription>Manage epics and keep ticket assignment focused per epic.</CardDescription>
          </CardHeader>

          <CardContent className="space-y-5">
            {epics.length === 0 ? (
              <div className="rounded-2xl border border-dashed border-border/70 bg-background/60 p-6 text-sm text-muted-foreground">
                No epics created yet. Start with Create epic and group tickets by initiative.
              </div>
            ) : null}

            {epics.map((epic, index) => {
              const assignedTickets = epic.ticketIds
                .map((ticketId) => ticketById.get(ticketId))
                .filter((ticket): ticket is BacklogTicket => Boolean(ticket));

              return (
                <div key={epic.id}>
                  {index > 0 ? <Separator className="mb-5" /> : null}
                  <div className="space-y-4 rounded-2xl border border-border/70 bg-background/80 p-4 shadow-sm">
                    <div className="space-y-2">
                      <div className="flex flex-wrap items-center justify-between gap-2">
                        <div className="flex flex-wrap items-center gap-2">
                          <Badge variant="outline" className="border-border/70 bg-background/70 text-[0.68rem] uppercase tracking-[0.18em] text-muted-foreground">
                            Epic
                          </Badge>
                          <Badge variant="secondary" className="border border-border/60 bg-background/80 text-foreground">
                            {assignedTickets.length} tickets
                          </Badge>
                        </div>
                        <div className="flex flex-wrap items-center gap-2">
                          <Button size="sm" variant="outline" onClick={() => openAssignTicketsModal(epic)}>
                            Assign tickets
                          </Button>
                          <Button size="sm" variant="outline" onClick={() => openEditEpicModal(epic)}>
                            <Pencil className="mr-2 h-4 w-4" />
                            Edit
                          </Button>
                          <Button
                            size="icon"
                            variant="outline"
                            aria-label={`Delete epic ${epic.name}`}
                            title={`Delete epic ${epic.name}`}
                            className="text-rose-700 hover:bg-rose-50 hover:text-rose-800"
                            onClick={() => openDeleteConfirmModal(epic.id)}
                          >
                            <Trash2 className="h-4 w-4" />
                          </Button>
                        </div>
                      </div>
                      <h3 className="text-base font-semibold text-foreground">{epic.name}</h3>
                      <p className="text-sm leading-6 text-muted-foreground">{epic.description || 'No description provided yet.'}</p>
                    </div>

                    <div className="space-y-3">
                      <p className="text-sm font-medium text-foreground">Assigned tickets</p>
                      {assignedTickets.length === 0 ? (
                        <p className="text-sm text-muted-foreground">No tickets assigned yet.</p>
                      ) : (
                        <div className="grid gap-2">
                          {assignedTickets.map((ticket) => (
                            <div key={ticket.id} className="flex flex-wrap items-center justify-between gap-3 rounded-xl border border-border/70 bg-muted/30 p-3 text-sm">
                              <div>
                                <p className="font-medium text-foreground">
                                  {ticket.id} - {ticket.title}
                                </p>
                                <p className="text-xs text-muted-foreground">{ticket.estimate}</p>
                              </div>
                              <Button variant="outline" size="sm" onClick={() => removeTicketFromEpic(epic.id, ticket.id)}>
                                Remove
                              </Button>
                            </div>
                          ))}
                        </div>
                      )}
                    </div>
                  </div>
                </div>
              );
            })}
          </CardContent>
        </Card>
      </div>

      {activeAssignEpic ? (
        <div className="fixed inset-0 z-50 grid place-items-center bg-black/60 px-4 py-6" role="dialog" aria-modal="true" onClick={closeAssignTicketsModal}>
          <Card className="w-full max-w-3xl border-border/70 bg-card shadow-2xl" onClick={(event) => event.stopPropagation()}>
            <CardHeader className="space-y-3">
              <div className="flex flex-wrap items-start justify-between gap-3">
                <div>
                  <CardTitle>Assign tickets to {activeAssignEpic.name}</CardTitle>
                  <CardDescription>Select tickets to keep this epic scope clear.</CardDescription>
                </div>
                <Badge variant="secondary" className="border border-border/60 bg-background/80 text-foreground">
                  {assignTicketDraft.length} selected
                </Badge>
              </div>
              <div className="relative">
                <Search className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
                <Input
                  className="pl-9"
                  placeholder="Search tickets by id, title, or description"
                  value={assignSearch}
                  onChange={(event) => setAssignSearch(event.target.value)}
                />
              </div>
            </CardHeader>

            <CardContent className="space-y-4">
              <div className="h-80 space-y-2 overflow-y-auto rounded-2xl border border-border/70 bg-background/70 p-4">
                {unassignedTickets.length === 0 ? (
                  <p className="text-sm text-muted-foreground">No assignable tickets available right now.</p>
                ) : assignFilteredTickets.length === 0 ? (
                  <p className="text-sm text-muted-foreground">No tickets found for this search.</p>
                ) : (
                  assignFilteredTickets.map((ticket) => {
                    return (
                      <label
                        key={`${activeAssignEpic.id}-${ticket.id}`}
                        className="flex items-start gap-3 rounded-xl border border-border/50 bg-background px-3 py-2 text-sm"
                      >
                        <input
                          type="checkbox"
                          className="mt-1"
                          checked={assignTicketDraft.includes(ticket.id)}
                          onChange={() => toggleAssignDraftTicket(ticket.id)}
                        />
                        <span className="space-y-1">
                          <span className="block font-medium text-foreground">
                            {ticket.id} - {ticket.title}
                          </span>
                          <span className="block text-xs text-muted-foreground">
                            {ticket.status} - {ticket.priority} - {ticket.estimate}
                          </span>
                        </span>
                      </label>
                    );
                  })
                )}
              </div>

              <FormActionButtons
                onCancel={closeAssignTicketsModal}
                confirmLabel="Save assignment"
                onConfirm={saveAssignedTickets}
              />
            </CardContent>
          </Card>
        </div>
      ) : null}

      {activeEditEpic ? (
        <div className="fixed inset-0 z-50 grid place-items-center bg-black/60 px-4 py-6" role="dialog" aria-modal="true" onClick={closeEditEpicModal}>
          <Card className="w-full max-w-2xl border-border/70 bg-card shadow-2xl" onClick={(event) => event.stopPropagation()}>
            <CardHeader>
              <CardTitle>Edit epic</CardTitle>
              <CardDescription>Update the epic details below.</CardDescription>
            </CardHeader>
            <CardContent className="space-y-5">
              <div className="space-y-2">
                <label className="text-sm font-medium text-foreground" htmlFor="edit-epic-name">
                  Epic name
                </label>
                <Input
                  id="edit-epic-name"
                  value={editEpicName}
                  onChange={(event) => setEditEpicName(event.target.value)}
                  placeholder="Enter epic name"
                  maxLength={100}
                />
                <p className="text-xs text-muted-foreground">Minimum 3, maximum 100 characters.</p>
              </div>

              <div className="space-y-2">
                <label className="text-sm font-medium text-foreground" htmlFor="edit-epic-description">
                  Epic description
                </label>
                <textarea
                  id="edit-epic-description"
                  value={editEpicDescription}
                  onChange={(event) => setEditEpicDescription(event.target.value)}
                  placeholder="Describe the scope and expected outcome."
                  maxLength={2000}
                  className="min-h-24 w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground shadow-sm outline-none transition-colors placeholder:text-muted-foreground focus:border-ring focus:ring-2 focus:ring-ring/30"
                />
              </div>

              <FormActionButtons
                onCancel={closeEditEpicModal}
                confirmLabel="Save changes"
                onConfirm={saveEpicEdit}
                confirmDisabled={editEpicName.trim().length < 3}
              />
            </CardContent>
          </Card>
        </div>
      ) : null}

      {epicPendingDelete ? (
        <div className="fixed inset-0 z-50 grid place-items-center bg-black/60 px-4 py-6" role="dialog" aria-modal="true" onClick={closeDeleteConfirmModal}>
          <Card className="w-full max-w-md border-border/70 bg-card shadow-2xl" onClick={(event) => event.stopPropagation()}>
            <CardHeader>
              <CardTitle>Delete epic?</CardTitle>
              <CardDescription>
                This will remove <span className="font-medium text-foreground">{epicPendingDelete.name}</span>. Assigned tickets will remain in backlog as unassigned.
              </CardDescription>
            </CardHeader>
            <CardContent>
              <FormActionButtons onCancel={closeDeleteConfirmModal} confirmLabel="Delete epic" onConfirm={confirmDeleteEpic} />
            </CardContent>
          </Card>
        </div>
      ) : null}

      {isCreateEpicOpen ? (
        <div className="fixed inset-0 z-50 grid place-items-center bg-black/60 px-4 py-6" role="dialog" aria-modal="true" onClick={closeCreateEpicModal}>
          <Card className="w-full max-w-3xl border-border/70 bg-card shadow-2xl" onClick={(event) => event.stopPropagation()}>
            <CardHeader>
              <CardTitle>New epic</CardTitle>
              <CardDescription>Create an epic and optionally attach unassigned tickets.</CardDescription>
            </CardHeader>
            <CardContent className="space-y-5">
              <div className="grid gap-4 lg:grid-cols-2">
                <div className="space-y-2">
                  <label className="text-sm font-medium text-foreground" htmlFor="epic-name">
                    Epic name
                  </label>
                  <Input
                    id="epic-name"
                    value={newEpicName}
                    onChange={(event) => setNewEpicName(event.target.value)}
                    placeholder="Growth initiative, Reliability wave, ..."
                    maxLength={100}
                  />
                  <p className="text-xs text-muted-foreground">Minimum 3, maximum 100 characters.</p>
                </div>
                <div className="space-y-2">
                  <label className="text-sm font-medium text-foreground" htmlFor="epic-description">
                    Epic description
                  </label>
                  <textarea
                    id="epic-description"
                    value={newEpicDescription}
                    onChange={(event) => setNewEpicDescription(event.target.value)}
                    placeholder="Describe the scope and expected outcome."
                    maxLength={2000}
                    className="min-h-24 w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground shadow-sm outline-none transition-colors placeholder:text-muted-foreground focus:border-ring focus:ring-2 focus:ring-ring/30"
                  />
                </div>
              </div>

              <div className="space-y-3">
                <p className="text-sm font-medium text-foreground">Attach unassigned tickets</p>
                <div className="relative">
                  <Search className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
                  <Input
                    className="pl-9"
                    placeholder="Search tickets by id, title, or description"
                    value={createEpicSearch}
                    onChange={(event) => setCreateEpicSearch(event.target.value)}
                  />
                </div>
                <div className="h-80 space-y-2 overflow-y-auto rounded-2xl border border-border/70 bg-background/70 p-4">
                  {unassignedTickets.length === 0 ? (
                    <p className="text-sm text-muted-foreground">No unassigned tickets available right now.</p>
                  ) : createEpicFilteredTickets.length === 0 ? (
                    <p className="text-sm text-muted-foreground">No tickets found for this search.</p>
                  ) : (
                    createEpicFilteredTickets.map((ticket) => (
                      <label key={ticket.id} className="flex items-start gap-3 rounded-xl border border-border/50 bg-background px-3 py-2 text-sm">
                        <input
                          type="checkbox"
                          className="mt-1"
                          checked={newEpicTicketIds.includes(ticket.id)}
                          onChange={() => toggleCreateEpicTicket(ticket.id)}
                        />
                        <span className="space-y-1">
                          <span className="block font-medium text-foreground">
                            {ticket.id} - {ticket.title}
                          </span>
                          <span className="block text-xs text-muted-foreground">{ticket.description}</span>
                        </span>
                      </label>
                    ))
                  )}
                </div>
              </div>

              <FormActionButtons
                onCancel={closeCreateEpicModal}
                confirmLabel="Create epic"
                onConfirm={createEpic}
                confirmDisabled={newEpicName.trim().length < 3}
              />
            </CardContent>
          </Card>
        </div>
      ) : null}
    </section>
  );
}
