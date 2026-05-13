import { apiClient } from '@/lib/apiClient';
import type { CreateEpicInput, EpicDto, EpicFilters, UpdateEpicInput } from './epicTypes';

export const epicQueryKeys = {
  all: ['epics'] as const,
  list: (filters: EpicFilters) => [...epicQueryKeys.all, 'list', filters] as const,
};

export async function fetchEpics(filters: EpicFilters): Promise<EpicDto[]> {
  const params = new URLSearchParams();

  if (filters.projectId) {
    params.set('projectId', filters.projectId);
  }

  const query = params.toString();
  return apiClient.get<EpicDto[]>(`/epics${query ? `?${query}` : ''}`);
}

export async function createEpic(input: CreateEpicInput): Promise<EpicDto> {
  return apiClient.post<EpicDto>('/epics', input);
}

export async function updateEpic(input: UpdateEpicInput): Promise<void> {
  const { id, ...request } = input;
  return apiClient.put<void>(`/epics/${id}`, request);
}

export async function deleteEpic(id: string): Promise<void> {
  return apiClient.delete<void>(`/epics/${id}`);
}
