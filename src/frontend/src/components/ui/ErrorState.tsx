type ErrorStateProps = {
  title?: string;
  description?: string;
  onRetry?: () => void;
};

export function ErrorState({
  title = 'Something went wrong',
  description = 'Try refreshing or retrying the request.',
  onRetry,
}: ErrorStateProps) {
  return (
    <div className="state-card state-card-error" role="alert">
      <p className="state-title">{title}</p>
      <p className="state-description">{description}</p>
      {onRetry && (
        <button type="button" className="button button-secondary" onClick={onRetry}>
          Retry
        </button>
      )}
    </div>
  );
}
