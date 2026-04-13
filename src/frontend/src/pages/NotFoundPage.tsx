import { Link } from 'react-router-dom';

export function NotFoundPage() {
  return (
    <section className="panel">
      <h2 className="panel-title">Page Not Found</h2>
      <p className="panel-description">The route does not exist yet in this frontend foundation.</p>
      <Link className="button button-primary" to="/dashboard">
        Go to Dashboard
      </Link>
    </section>
  );
}
