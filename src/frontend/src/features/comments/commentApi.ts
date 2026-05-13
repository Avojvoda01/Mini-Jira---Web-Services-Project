import { apiClient } from '@/lib/apiClient';
import type { CommentItem, CreateCommentInput, DeleteCommentInput, UpdateCommentInput } from './commentTypes';

type CommentResponse = {
  id: string;
  taskId: string;
  userId: string | null;
  content: string;
  createdAtUtc: string;
  updatedAtUtc: string | null;
};

const mapComment = (comment: CommentResponse): CommentItem => ({
  ...comment,
});

export async function fetchComments(taskId: string): Promise<CommentItem[]> {
  const result = await apiClient.get<CommentResponse[]>(`/tasks/${taskId}/comments`);
  return result.map(mapComment);
}

export async function createComment(input: CreateCommentInput): Promise<CommentItem> {
  const { taskId, ...payload } = input;
  const result = await apiClient.post<CommentResponse>(`/tasks/${taskId}/comments`, payload);
  return mapComment(result);
}

export async function updateComment(input: UpdateCommentInput): Promise<void> {
  const { taskId, commentId, content } = input;
  await apiClient.put<void>(`/tasks/${taskId}/comments/${commentId}`, { content });
}

export async function deleteComment(input: DeleteCommentInput): Promise<void> {
  const { taskId, commentId } = input;
  await apiClient.delete<void>(`/tasks/${taskId}/comments/${commentId}`);
}
