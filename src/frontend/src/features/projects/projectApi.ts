import { apiClient } from '@/lib/apiClient';
import type { CreateProjectInput, ProjectDto, UpdateProjectInput } from './projectTypes';

export const projectQueryKeys = {
  all: ['projects'] as const,
  list: () => [...projectQueryKeys.all, 'list'] as const,
};

export async function fetchProjects(): Promise<ProjectDto[]> {
  return apiClient.get<ProjectDto[]>('/projects');
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
