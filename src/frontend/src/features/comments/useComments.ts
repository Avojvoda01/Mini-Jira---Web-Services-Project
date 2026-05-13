import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { createComment, deleteComment, fetchComments, updateComment } from './commentApi';
import type { CommentItem, CreateCommentInput, DeleteCommentInput, UpdateCommentInput } from './commentTypes';

export const commentQueryKeys = {
  all: ['comments'] as const,
  list: (taskId: string) => [...commentQueryKeys.all, 'task', taskId] as const,
};

export function useCommentsQuery(taskId: string | null) {
  return useQuery({
    queryKey: taskId ? commentQueryKeys.list(taskId) : commentQueryKeys.all,
    queryFn: () => fetchComments(taskId ?? ''),
    enabled: Boolean(taskId),
  });
}

export function useCreateCommentMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: CreateCommentInput) => createComment(input),
    onSuccess: (comment: CommentItem) => {
      return queryClient.invalidateQueries({ queryKey: commentQueryKeys.list(comment.taskId) });
    },
  });
}

export function useUpdateCommentMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: UpdateCommentInput) => updateComment(input),
    onSuccess: (_, input) => {
      return queryClient.invalidateQueries({ queryKey: commentQueryKeys.list(input.taskId) });
    },
  });
}

export function useDeleteCommentMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: DeleteCommentInput) => deleteComment(input),
    onSuccess: (_, input) => {
      return queryClient.invalidateQueries({ queryKey: commentQueryKeys.list(input.taskId) });
    },
  });
}
