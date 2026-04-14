const kpis = [
  { label: 'Open Tickets', value: '12' },
  { label: 'In Progress', value: '6' },
  { label: 'Done', value: '3' },
];

export function DashboardPage() {
  return (
    <section className="panel">
      <h2 className="panel-title">Dashboard</h2>
      <p className="panel-description">Foundation dashboard only. Live metrics will be connected in later feature branches.</p>

      <div className="kpi-grid">
        {kpis.map((kpi) => (
          <article key={kpi.label} className="kpi-card">
            <p className="kpi-label">{kpi.label}</p>
            <p className="kpi-value">{kpi.value}</p>
          </article>
        ))}
      </div>
    </section>
  );
}
