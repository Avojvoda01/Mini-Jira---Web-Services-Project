import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { EpicBacklogCard } from '@/components/backlog/EpicBacklogCard';
import { ErrorState } from '@/components/ui/ErrorState';

type BacklogTicket = {
  id: string;
  displayId: string;
  title: string;
  estimate: string;
  estimateMinutes: number | null;
};

type Epic = {
  id: string;
  name: string;
  description: string;
  createdAtUtc?: string;
  updatedAtUtc?: string | null;
  ticketIds: string[];
};

type EpicBacklogSectionProps = {
  isLoading: boolean;
  isError?: boolean;
  error?: Error | null;
  onRetry?: () => void;
  epics: Epic[];
  ticketById: Map<string, BacklogTicket>;
  onAssignTickets: (epic: Epic) => void;
  onEdit: (epic: Epic) => void;
  onDelete: (epicId: string) => void;
  onRemoveTicket: (epicId: string, ticketId: string) => void;
};

export function EpicBacklogSection({ isLoading, isError = false, error, onRetry, epics, ticketById, onAssignTickets, onEdit, onDelete, onRemoveTicket }: EpicBacklogSectionProps) {
  return (
    <Card className="border-border/70 bg-card/80 shadow-sm backdrop-blur-sm">
      <CardHeader className="space-y-3 pb-4">
        <CardTitle>Epic backlog</CardTitle>
        <CardDescription>Manage epics and keep ticket assignment focused per epic.</CardDescription>
      </CardHeader>

      <CardContent className="space-y-5">
        {isError ? (
          <ErrorState
            title="Unable to load epics"
            description={error?.message ?? 'Check your connection and try again.'}
            onRetry={onRetry}
          />
        ) : null}

        {isLoading ? (
          <div className="rounded-2xl border border-dashed border-border/70 bg-background/60 p-6 text-sm text-muted-foreground">
            Loading epics...
          </div>
        ) : null}

        {!isLoading && !isError && epics.length === 0 ? (
          <div className="rounded-2xl border border-dashed border-border/70 bg-background/60 p-6 text-sm text-muted-foreground">
            No epics created yet. Start with Create epic and group tickets by initiative.
          </div>
        ) : null}

        {!isError && epics.map((epic, index) => {
          const assignedTickets = epic.ticketIds
            .map((ticketId) => ticketById.get(ticketId))
            .filter((ticket): ticket is BacklogTicket => Boolean(ticket));

          return (
            <EpicBacklogCard
              key={epic.id}
              epic={epic}
              assignedTickets={assignedTickets}
              onAssignTickets={() => onAssignTickets(epic)}
              onEdit={() => onEdit(epic)}
              onDelete={() => onDelete(epic.id)}
              onRemoveTicket={(ticketId) => onRemoveTicket(epic.id, ticketId)}
              showSeparator={index > 0}
            />
          );
        })}
      </CardContent>
    </Card>
  );
}