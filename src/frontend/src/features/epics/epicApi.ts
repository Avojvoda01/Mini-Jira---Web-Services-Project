import { apiClient } from '@/lib/apiClient';
import type { CreateEpicInput, EpicDto, UpdateEpicInput } from './epicTypes';

export const epicQueryKeys = {
  all: ['epics'] as const,
  list: () => [...epicQueryKeys.all, 'list'] as const,
};

export async function fetchEpics(): Promise<EpicDto[]> {
  return apiClient.get<EpicDto[]>('/epics');
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
