import { apiClient } from '@/lib/apiClient';
import type { CreateProjectInput, ProjectDto, UpdateProjectInput } from './projectTypes';

type ProjectApiResponse = ProjectDto & {
  membersId?: string[];
};

export const projectQueryKeys = {
  all: ['projects'] as const,
  list: () => [...projectQueryKeys.all, 'list'] as const,
  detail: (id: string) => [...projectQueryKeys.all, 'detail', id] as const,
};

const mapProjectDto = (project: ProjectApiResponse): ProjectDto => ({
  ...project,
  memberIds: project.memberIds ?? project.membersId ?? [],
});

export async function fetchProjects(): Promise<ProjectDto[]> {
  const projects = await apiClient.get<ProjectApiResponse[]>('/projects');
  return projects.map(mapProjectDto);
}

export async function fetchProjectById(id: string): Promise<ProjectDto> {
  const project = await apiClient.get<ProjectApiResponse>(`/projects/${id}`);
  return mapProjectDto(project);
}

export async function createProject(input: CreateProjectInput): Promise<ProjectDto> {
  return apiClient.post<ProjectDto>('/projects', input);
}

export async function updateProject(input: UpdateProjectInput): Promise<void> {
  const { id, ...request } = input;
  return apiClient.put<void>(`/projects/${id}`, request);
}

export async function deleteProject(id: string): Promise<void> {
  return apiClient.delete<void>(`/projects/${id}`);
}
