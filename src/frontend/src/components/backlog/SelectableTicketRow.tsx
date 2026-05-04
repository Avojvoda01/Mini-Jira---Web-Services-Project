type SelectableTicketRowProps = {
  id: string;
  title: string;
  secondaryText: string;
  checked: boolean;
  onChange: () => void;
};

export function SelectableTicketRow({ id, title, secondaryText, checked, onChange }: SelectableTicketRowProps) {
  return (
    <label className="flex items-start gap-3 rounded-xl border border-border/50 bg-background px-3 py-2 text-sm">
      <input type="checkbox" className="mt-1" checked={checked} onChange={onChange} />
      <span className="space-y-1">
        <span className="block font-medium text-foreground">
          {id} - {title}
        </span>
        <span className="block text-xs text-muted-foreground">{secondaryText}</span>
      </span>
    </label>
  );
}