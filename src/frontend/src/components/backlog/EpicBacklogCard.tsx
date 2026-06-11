import { Pencil, Trash2, User } from 'lucide-react';
import { Badge } from '@/components/ui/badge';
import { cn } from '@/lib/utils';
import { formatEstimate } from '@/lib/estimate';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';

type BacklogTicket = {
  id: string;
  displayId: string;
  title: string;
  estimate: string;
  estimateMinutes: number | null;
  priority: 'High' | 'Medium' | 'Low';
  assigneeName: string | null;
};

type EpicBacklogCardProps = {
  epic: {
    id: string;
    name: string;
    description: string;
    createdAtUtc?: string;
    updatedAtUtc?: string | null;
  };
  assignedTickets: BacklogTicket[];
  onAssignTickets: () => void;
  onEdit: () => void;
  onDelete: () => void;
  onRemoveTicket: (ticketId: string) => void;
};

const totalEstimate = (tickets: BacklogTicket[]): string => {
  const total = tickets.reduce((sum, t) => sum + (t.estimateMinutes ?? 0), 0);
  return total > 0 ? formatEstimate(total) : 'n/a';
};

export function EpicBacklogCard({
  epic,
  assignedTickets,
  onAssignTickets,
  onEdit,
  onDelete,
  onRemoveTicket,
}: EpicBacklogCardProps) {
  return (
    <Card className="space-y-4 rounded-2xl border border-border/70 bg-background/80 p-4 shadow-sm">
        <div className="space-y-2">
          <div className="flex flex-wrap items-center justify-between gap-2">
            <div className="flex flex-wrap items-center gap-2">
              <Badge variant="outline" className="border-border/70 bg-background/70 text-[0.68rem] uppercase tracking-[0.18em] text-muted-foreground">
                Epic
              </Badge>
              <Badge variant="secondary" className="border border-border/60 bg-background/80 text-foreground">
                {assignedTickets.length} tickets
              </Badge>
              <Badge variant="secondary" className="border border-border/60 bg-background/80 text-foreground">
                {totalEstimate(assignedTickets)} total
              </Badge>
            </div>
            <div className="flex flex-wrap items-center gap-2">
              <Button size="sm" variant="outline" onClick={onAssignTickets}>
                Assign tickets
              </Button>
              <Button size="sm" variant="outline" onClick={onEdit}>
                <Pencil className="mr-2 h-4 w-4" />
                Edit
              </Button>
              <Button
                size="icon"
                variant="outline"
                aria-label={`Delete epic ${epic.name}`}
                title={`Delete epic ${epic.name}`}
                className="text-rose-700 hover:bg-rose-50 hover:text-rose-800"
                onClick={onDelete}
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
                <div key={ticket.id} className="flex flex-wrap items-center justify-between gap-3 rounded-xl border border-border bg-muted p-3 text-sm">
                  <div>
                    <p className="font-medium text-foreground">
                      {ticket.displayId} - {ticket.title}
                    </p>
                    <p className="text-xs text-muted-foreground">{ticket.estimate}</p>
                  </div>
                  <div className="flex flex-wrap items-center gap-2">
                    <Badge className={cn('text-xs font-medium', ticket.priority === 'High' ? 'bg-rose-500/10 text-rose-700' : ticket.priority === 'Medium' ? 'bg-amber-500/10 text-amber-700' : 'bg-blue-500/10 text-blue-700')}>
                      {ticket.priority}
                    </Badge>
                    {ticket.assigneeName ? (
                      <span className="flex items-center gap-1 text-xs text-muted-foreground">
                        <User className="h-3 w-3" />
                        {ticket.assigneeName}
                      </span>
                    ) : null}
                    <Button variant="outline" size="sm" onClick={() => onRemoveTicket(ticket.id)}>
                      Remove
                    </Button>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </Card>
  );
}