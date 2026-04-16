import { useParams } from 'react-router-dom';
import { Badge } from '@/components/ui/badge';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Separator } from '@/components/ui/separator';

export function TicketDetailsPage() {
  const { ticketId } = useParams();

  return (
    <section className="space-y-6">
      <Card className="border-border/70 bg-card/80 shadow-sm backdrop-blur-sm">
        <CardHeader>
          <Badge variant="outline" className="w-fit border-border/70 bg-background/70 text-muted-foreground">
            Ticket details
          </Badge>
          <CardTitle className="text-3xl font-semibold tracking-tight sm:text-4xl">{ticketId}</CardTitle>
          <CardDescription className="max-w-2xl text-sm leading-6 sm:text-base">
            A detail surface for task metadata, discussion, and the activity trail.
          </CardDescription>
        </CardHeader>
      </Card>

      <div className="grid gap-4 xl:grid-cols-[minmax(0,1fr)_320px]">
        <Card className="border-border/70 bg-card/80 shadow-sm backdrop-blur-sm">
          <CardHeader>
            <CardTitle>Work summary</CardTitle>
            <CardDescription>What this ticket is about and how it should be handled.</CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="grid gap-3 sm:grid-cols-3">
              {[
                { label: 'Status', value: 'In progress' },
                { label: 'Priority', value: 'High' },
                { label: 'Estimate', value: '8 points' },
              ].map((item) => (
                <div key={item.label} className="rounded-2xl border border-border/70 bg-background/80 p-4">
                  <p className="text-xs font-medium uppercase tracking-[0.18em] text-muted-foreground">{item.label}</p>
                  <p className="mt-2 text-sm font-medium text-foreground">{item.value}</p>
                </div>
              ))}
            </div>

            <Separator />

            <div className="space-y-2">
              <p className="text-sm font-medium text-foreground">Summary</p>
              <p className="text-sm leading-6 text-muted-foreground">
                Track the implementation details, review notes, and QA evidence required before this issue is moved to
                done.
              </p>
            </div>

            <div className="space-y-2">
              <p className="text-sm font-medium text-foreground">Acceptance criteria</p>
              <ul className="space-y-2 text-sm leading-6 text-muted-foreground">
                <li>• The issue has a clearly defined owner and scope.</li>
                <li>• Changes are validated in staging before release.</li>
                <li>• Any blockers are documented and visible to the team.</li>
              </ul>
            </div>
          </CardContent>
        </Card>

        <Card className="border-border/70 bg-card/80 shadow-sm backdrop-blur-sm">
          <CardHeader>
            <CardTitle>Activity</CardTitle>
            <CardDescription>Short notes from the latest updates.</CardDescription>
          </CardHeader>
          <CardContent className="space-y-4 text-sm text-muted-foreground">
            <div>
              <p className="font-medium text-foreground">Maya</p>
              <p>Opened QA validation and requested stakeholder review.</p>
            </div>
            <Separator />
            <div>
              <p className="font-medium text-foreground">Eli</p>
              <p>Added implementation notes and linked the staging build.</p>
            </div>
            <Separator />
            <div>
              <p className="font-medium text-foreground">System</p>
              <p>Ticket moved to in progress after triage completed.</p>
            </div>
          </CardContent>
        </Card>
      </div>
    </section>
  );
}
