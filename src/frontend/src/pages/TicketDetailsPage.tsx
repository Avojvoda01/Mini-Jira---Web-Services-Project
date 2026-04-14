import { useParams } from 'react-router-dom';

export function TicketDetailsPage() {
  const { ticketId } = useParams();

  return (
    <section className="panel">
      <h2 className="panel-title">Ticket Details</h2>
      <p className="panel-description">Detail page for task metadata, comments, and activity timeline.</p>
      <p className="ticket-id">Selected ticket: {ticketId}</p>
    </section>
  );
}
