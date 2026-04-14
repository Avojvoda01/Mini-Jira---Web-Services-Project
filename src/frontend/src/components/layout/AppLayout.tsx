import { NavLink, Outlet, type NavLinkRenderProps } from 'react-router-dom';

const navigationItems = [
  { to: '/board', label: 'Board' },
  { to: '/backlog', label: 'Backlog' },
  { to: '/dashboard', label: 'Dashboard' },
  { to: '/settings', label: 'Settings' },
];

export function AppLayout() {
  return (
    <div className="app-shell">
      <aside className="app-sidebar" aria-label="Primary">
        <div className="brand-block">
          <p className="brand-kicker">Mini Jira</p>
          <h1 className="brand-title">Task Workspace</h1>
        </div>

        <nav className="nav-list" aria-label="Main navigation">
          {navigationItems.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              className={({ isActive }: NavLinkRenderProps) => `nav-item ${isActive ? 'active' : ''}`}
            >
              {item.label}
            </NavLink>
          ))}
        </nav>
      </aside>

      <div className="app-main">
        <header className="topbar">
          <p className="topbar-title">Frontend Foundation</p>
          <p className="topbar-meta">Vite + React + Jotai + TanStack Query</p>
        </header>

        <main className="content-area">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
