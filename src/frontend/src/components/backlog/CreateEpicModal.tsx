import { Search } from 'lucide-react';
import { BacklogModal } from '@/components/backlog/BacklogModal';
import { FormActionButtons } from '@/components/common/FormActionButtons';
import { CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { SelectableTicketRow } from '@/components/backlog/SelectableTicketRow';

type BacklogTicket = {
  id: string;
  title: string;
  description: string;
};

type CreateEpicModalProps = {
  isOpen: boolean;
  onClose: () => void;
  newEpicName: string;
  setNewEpicName: (value: string) => void;
  newEpicDescription: string;
  setNewEpicDescription: (value: string) => void;
  createEpicSearch: string;
  setCreateEpicSearch: (value: string) => void;
  newEpicTicketIds: string[];
  toggleCreateEpicTicket: (ticketId: string) => void;
  createEpicFilteredTickets: BacklogTicket[];
  unassignedTickets: BacklogTicket[];
  onCreate: () => void;
  isPending: boolean;
};

export function CreateEpicModal({
  isOpen,
  onClose,
  newEpicName,
  setNewEpicName,
  newEpicDescription,
  setNewEpicDescription,
  createEpicSearch,
  setCreateEpicSearch,
  newEpicTicketIds,
  toggleCreateEpicTicket,
  createEpicFilteredTickets,
  unassignedTickets,
  onCreate,
  isPending,
}: CreateEpicModalProps) {
  if (!isOpen) {
    return null;
  }

  return (
    <BacklogModal onClose={onClose} cardClassName="w-full max-w-3xl border-border/70 bg-card shadow-2xl">
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
                <SelectableTicketRow
                  key={ticket.id}
                  id={ticket.id}
                  title={ticket.title}
                  secondaryText={ticket.description}
                  checked={newEpicTicketIds.includes(ticket.id)}
                  onChange={() => toggleCreateEpicTicket(ticket.id)}
                />
              ))
            )}
          </div>
        </div>

        <FormActionButtons
          onCancel={onClose}
          confirmLabel="Create epic"
          onConfirm={onCreate}
          confirmDisabled={newEpicName.trim().length < 3 || isPending}
        />
      </CardContent>
    </BacklogModal>
  );
}