type LoadingStateProps = {
  label?: string;
};

export function LoadingState({ label = 'Loading workspace...' }: LoadingStateProps) {
  return (
    <div className="state-card" role="status" aria-live="polite">
      <span className="state-spinner" aria-hidden="true" />
      <p className="state-title">{label}</p>
    </div>
  );
}
