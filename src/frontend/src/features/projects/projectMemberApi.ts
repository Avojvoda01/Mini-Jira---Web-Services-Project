import { apiClient } from '@/lib/apiClient';

export type AddProjectMemberInput = {
  projectId: string;
  userId: string;
  role?: string;
};

export type RemoveProjectMemberInput = {
  projectId: string;
  userId: string;
};

export async function addProjectMember(input: AddProjectMemberInput): Promise<void> {
  const { projectId, userId, role = 'Member' } = input;
  await apiClient.post<void>(`/projects/${projectId}/members`, {
    projectId,
    userId,
    role,
  });
}

export async function removeProjectMember(input: RemoveProjectMemberInput): Promise<void> {
  const { projectId, userId } = input;
  await apiClient.delete<void>(`/projects/${projectId}/members/${userId}`);
}
