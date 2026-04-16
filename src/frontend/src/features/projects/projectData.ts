export type ProjectSummary = {
  id: string;
  name: string;
  description: string;
  owner: string;
  status: string;
  tickets: string;
};

export const projectCatalog: ProjectSummary[] = [
  {
    id: 'mini-jira-core',
    name: 'Mini Jira Core',
    description: 'Primary delivery stream for board, backlog, and ticket flow polish.',
    owner: 'Product Team',
    status: 'Active',
    tickets: '24 tickets',
  },
  {
    id: 'auth-foundation',
    name: 'Authentication Foundation',
    description: 'Login, registration, and route protection for the workspace shell.',
    owner: 'Platform',
    status: 'Ready',
    tickets: '10 tickets',
  },
  {
    id: 'analytics-expansion',
    name: 'Analytics Expansion',
    description: 'Dashboard metrics and delivery indicators for team health reporting.',
    owner: 'Insights',
    status: 'Planned',
    tickets: '08 tickets',
  },
];

export function getProjectById(projectId: string | undefined): ProjectSummary | undefined {
  return projectCatalog.find((project) => project.id === projectId);
}
