export type CommentItem = {
  id: string;
  taskId: string;
  userId: string | null;
  content: string;
  createdAtUtc: string;
  updatedAtUtc: string | null;
};

export type CreateCommentInput = {
  taskId: string;
  content: string;
};

export type UpdateCommentInput = {
  taskId: string;
  commentId: string;
  content: string;
};

export type DeleteCommentInput = {
  taskId: string;
  commentId: string;
};
