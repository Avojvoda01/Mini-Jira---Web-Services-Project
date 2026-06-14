import { Pencil, X } from 'lucide-react';
import { useEffect, useState } from 'react';
import { DeleteCommentModal } from '@/components/board/DeleteCommentModal';
import { Button } from '@/components/ui/button';
import {
  useCommentsQuery,
  useCreateCommentMutation,
  useDeleteCommentMutation,
  useUpdateCommentMutation,
} from '@/features/comments';
import type { AuthSession } from '@/store/authAtoms';

type TaskCommentsProps = {
  taskId: string;
  session: AuthSession | null;
  resolveUserDisplayName: (userId: string | null) => string;
};

export function TaskComments({
  taskId,
  session,
  resolveUserDisplayName,
}: TaskCommentsProps) {
  const {
    data: comments = [],
    isLoading,
    isError,
    error,
  } = useCommentsQuery(taskId);
  const createCommentMutation = useCreateCommentMutation();
  const updateCommentMutation = useUpdateCommentMutation();
  const deleteCommentMutation = useDeleteCommentMutation();

  const [commentDraftByTask, setCommentDraftByTask] = useState<
    Record<string, string>
  >({});
  const [editingCommentId, setEditingCommentId] = useState<string | null>(null);
  const [commentEditDrafts, setCommentEditDrafts] = useState<
    Record<string, string>
  >({});
  const [deleteCommentId, setDeleteCommentId] = useState<string | null>(null);

  useEffect(() => {
    setEditingCommentId(null);
    setDeleteCommentId(null);
  }, [taskId]);

  const draft = commentDraftByTask[taskId] ?? '';
  const deleteComment = deleteCommentId
    ? (comments.find((comment) => comment.id === deleteCommentId) ?? null)
    : null;

  return (
    <div className="space-y-3">
      <DeleteCommentModal
        isOpen={deleteCommentId !== null}
        onClose={() => setDeleteCommentId(null)}
        onConfirm={async () => {
          if (!deleteComment) {
            return;
          }

          await deleteCommentMutation.mutateAsync({
            taskId: deleteComment.taskId,
            commentId: deleteComment.id,
          });
          setDeleteCommentId(null);
        }}
        isPending={deleteCommentMutation.isPending}
      />

      <div className="flex items-center justify-between">
        <p className="text-sm font-medium text-foreground">Comments</p>
        <span className="text-xs text-muted-foreground">{comments.length}</span>
      </div>
      <div className="space-y-3">
        {isLoading ? (
          <div className="rounded-2xl border border-dashed border-border/70 bg-background/70 p-4 text-xs text-muted-foreground">
            Loading comments...
          </div>
        ) : isError ? (
          <div className="rounded-2xl border border-dashed border-border/70 bg-background/70 p-4 text-xs text-muted-foreground">
            {error instanceof Error
              ? error.message
              : 'Unable to load comments.'}
          </div>
        ) : comments.length === 0 ? (
          <div className="rounded-2xl border border-dashed border-border/70 bg-background/70 p-4 text-xs text-muted-foreground">
            No comments yet.
          </div>
        ) : (
          comments.map((comment) => (
            <div
              key={comment.id}
              className="rounded-2xl border border-border/70 bg-background/80 p-3"
            >
              <div className="flex items-start justify-between gap-2">
                <div className="flex items-center gap-2">
                  <div className="flex h-6 w-6 shrink-0 items-center justify-center rounded-full bg-primary/10 text-[10px] font-semibold text-primary">
                    {resolveUserDisplayName(comment.userId)
                      .slice(0, 2)
                      .toUpperCase()}
                  </div>
                  <span className="text-xs font-medium text-foreground">
                    {resolveUserDisplayName(comment.userId)}
                  </span>
                </div>
                {(session?.role === 'Admin' ||
                  comment.userId?.toLowerCase() ===
                    session?.userId?.toLowerCase()) && (
                  <div className="flex items-center gap-2">
                    <Button
                      variant="ghost"
                      size="icon"
                      className="h-7 w-7"
                      onClick={() => {
                        setEditingCommentId(comment.id);
                        setCommentEditDrafts((current) => ({
                          ...current,
                          [comment.id]: comment.content,
                        }));
                      }}
                      aria-label="Edit comment"
                    >
                      <Pencil className="h-3.5 w-3.5" />
                    </Button>
                    <Button
                      variant="ghost"
                      size="icon"
                      className="h-7 w-7 text-rose-600 hover:text-rose-700"
                      onClick={() => setDeleteCommentId(comment.id)}
                      aria-label="Delete comment"
                    >
                      <X className="h-3.5 w-3.5" />
                    </Button>
                  </div>
                )}
              </div>
              {editingCommentId === comment.id ? (
                <div className="mt-2 space-y-2">
                  <textarea
                    value={commentEditDrafts[comment.id] ?? comment.content}
                    onChange={(event) =>
                      setCommentEditDrafts((current) => ({
                        ...current,
                        [comment.id]: event.target.value,
                      }))
                    }
                    className="min-h-20 w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground shadow-sm outline-none transition-colors placeholder:text-muted-foreground focus:border-ring focus:ring-2 focus:ring-ring/30"
                  />
                  <div className="flex justify-end gap-2">
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => setEditingCommentId(null)}
                    >
                      Cancel
                    </Button>
                    <Button
                      size="sm"
                      onClick={async () => {
                        const content = (
                          commentEditDrafts[comment.id] ?? ''
                        ).trim();
                        if (!content) {
                          return;
                        }

                        await updateCommentMutation.mutateAsync({
                          taskId: comment.taskId,
                          commentId: comment.id,
                          content,
                        });
                        setEditingCommentId(null);
                      }}
                      disabled={updateCommentMutation.isPending}
                    >
                      {updateCommentMutation.isPending ? 'Saving...' : 'Save'}
                    </Button>
                  </div>
                </div>
              ) : (
                <>
                  <p className="mt-1 whitespace-pre-wrap text-sm text-foreground break-words">
                    {comment.content}
                  </p>
                  <p className="mt-2 text-[0.7rem] text-muted-foreground">
                    {new Date(comment.createdAtUtc).toLocaleString()}
                  </p>
                </>
              )}
            </div>
          ))
        )}
      </div>
      <div className="space-y-2">
        <label
          className="text-xs font-medium uppercase tracking-[0.18em] text-muted-foreground"
          htmlFor="comment-input"
        >
          Add comment
        </label>
        <textarea
          id="comment-input"
          value={draft}
          onChange={(event) =>
            setCommentDraftByTask((current) => ({
              ...current,
              [taskId]: event.target.value,
            }))
          }
          placeholder="Write a comment..."
          maxLength={2000}
          className="min-h-20 w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground shadow-sm outline-none transition-colors placeholder:text-muted-foreground focus:border-ring focus:ring-2 focus:ring-ring/30"
        />
        <div className="flex justify-end">
          <Button
            variant="outline"
            className="border-border/70 bg-background/80 shadow-sm"
            disabled={!draft.trim() || createCommentMutation.isPending}
            onClick={async () => {
              const content = draft.trim();
              if (!content) {
                return;
              }

              await createCommentMutation.mutateAsync({ taskId, content });
              setCommentDraftByTask((current) => ({
                ...current,
                [taskId]: '',
              }));
            }}
          >
            {createCommentMutation.isPending ? 'Posting...' : 'Post comment'}
          </Button>
        </div>
      </div>
    </div>
  );
}
