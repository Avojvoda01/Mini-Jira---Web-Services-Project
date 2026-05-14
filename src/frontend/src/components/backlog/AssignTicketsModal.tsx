import { Search } from 'lucide-react';
import { Link } from 'react-router-dom';
import { BacklogModal } from '@/components/backlog/BacklogModal';
import { FormActionButtons } from '@/components/common/FormActionButtons';
import { Badge } from '@/components/ui/badge';
import { CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { ErrorState } from '@/components/ui/ErrorState';
import { Input } from '@/components/ui/input';
import { SelectableTicketRow } from '@/components/backlog/SelectableTicketRow';

type BacklogTicket = {
  id: string;
  displayId: string;
  title: string;
  description: string;
  priority: 'High' | 'Medium' | 'Low';
  status: string;
  estimate: string;
};

type AssignTicketsModalProps = {
  isOpen: boolean;
  epicName: string;
  onClose: () => void;
  assignSearch: string;
  setAssignSearch: (value: string) => void;
  assignTicketDraft: string[];
  toggleAssignDraftTicket: (ticketId: string) => void;
  assignFilteredTickets: BacklogTicket[];
  unassignedTickets: BacklogTicket[];
  onSave: () => void;
  isLoading?: boolean;
  isError?: boolean;
  error?: Error | null;
  onRetry?: () => void;
  isPending?: boolean;
  submitError?: string | null;
};

export function AssignTicketsModal({
  isOpen,
  epicName,
  onClose,
  assignSearch,
  setAssignSearch,
  assignTicketDraft,
  toggleAssignDraftTicket,
  assignFilteredTickets,
  unassignedTickets,
  onSave,
  isLoading = false,
  isError = false,
  error,
  onRetry,
  isPending = false,
  submitError,
}: AssignTicketsModalProps) {
  if (!isOpen) {
    return null;
  }

  return (
    <BacklogModal onClose={onClose} cardClassName="w-full max-w-3xl border-border/70 bg-card shadow-2xl">
      <CardHeader className="space-y-3">
        <div className="flex flex-wrap items-start justify-between gap-3">
          <div>
            <CardTitle>Assign tickets to {epicName}</CardTitle>
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
        {isError ? (
          <ErrorState
            title="Unable to load tickets"
            description={error?.message ?? 'Check your connection and try again.'}
            onRetry={onRetry}
          />
        ) : null}

        <div className="h-80 space-y-2 overflow-y-auto rounded-2xl border border-border/70 bg-background/70 p-4">
          {isLoading ? (
            <p className="text-sm text-muted-foreground">Loading tickets...</p>
          ) : unassignedTickets.length === 0 ? (
            <div className="space-y-2 text-sm text-muted-foreground">
              <p>No assignable tickets available right now.</p>
              <Link to="../board" className="text-sm font-medium text-sky-600 hover:text-sky-700">
                Create a ticket on the board.
              </Link>
            </div>
          ) : assignFilteredTickets.length === 0 ? (
            <p className="text-sm text-muted-foreground">No tickets found for this search.</p>
          ) : (
            assignFilteredTickets.map((ticket) => {
              return (
                <SelectableTicketRow
                  key={ticket.id}
                  id={ticket.displayId}
                  title={ticket.title}
                  secondaryText={`${ticket.status} - ${ticket.priority} - ${ticket.estimate}`}
                  checked={assignTicketDraft.includes(ticket.id)}
                  onChange={() => toggleAssignDraftTicket(ticket.id)}
                />
              );
            })
          )}
        </div>

        {submitError ? <p className="text-sm text-rose-700">{submitError}</p> : null}

        <FormActionButtons
          onCancel={onClose}
          confirmLabel={isPending ? 'Saving...' : 'Save assignment'}
          onConfirm={onSave}
          confirmDisabled={isPending || isLoading || isError}
        />
      </CardContent>
    </BacklogModal>
  );
}