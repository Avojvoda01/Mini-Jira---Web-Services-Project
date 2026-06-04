import { useMemo, useState } from 'react';
import { useAtomValue } from 'jotai';
import { UserPlus, X } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import type { UserDto } from '@/features/users';
import { authSessionAtom } from '@/store/authAtoms';

type MemberAssigneePickerProps = {
  members: UserDto[];
  selectedAssigneeId: string;
  onAssign: (userId: string) => void;
  onRemove: () => void;
  searchInputId: string;
  isBusy?: boolean;
};

export function MemberAssigneePicker({
  members,
  selectedAssigneeId,
  onAssign,
  onRemove,
  searchInputId,
  isBusy = false,
}: MemberAssigneePickerProps) {
  const session = useAtomValue(authSessionAtom);
  const currentUserId = session?.userId?.toLowerCase();

  const [search, setSearch] = useState('');

  const selectedMember = useMemo(
    () => members.find((member) => member.id === selectedAssigneeId) ?? null,
    [members, selectedAssigneeId],
  );

  const currentUser = useMemo(
    () => (currentUserId ? members.find((m) => m.id.toLowerCase() === currentUserId) : null),
    [members, currentUserId],
  );

  const isCurrentUserAssigned = selectedAssigneeId.toLowerCase() === (currentUserId ?? '');

  const availableMembers = useMemo(() => {
    const normalized = search.trim().toLowerCase();
    return members.filter((member) => {
      if (member.id === selectedAssigneeId) return false;
      if (member.id.toLowerCase() === currentUserId) return false;
      if (!normalized) return true;
      return member.displayName.toLowerCase().includes(normalized) || member.email.toLowerCase().includes(normalized);
    });
  }, [members, search, selectedAssigneeId, currentUserId]);

  return (
    <div className="space-y-3">
      <div className="flex items-center justify-between gap-2 text-xs text-muted-foreground">
        <span>
          {selectedMember ? `Assigned: ${selectedMember.displayName}` : 'Assigned: Unassigned'}
        </span>
        {selectedMember && (
          <Button
            type="button"
            variant="outline"
            size="icon"
            className="h-7 w-7 border-rose-300/80 text-rose-600 hover:bg-rose-50 hover:text-rose-700"
            onClick={onRemove}
            disabled={isBusy}
            aria-label={`Remove ${selectedMember.displayName} as assignee`}
          >
            <X className="h-3.5 w-3.5" />
          </Button>
        )}
      </div>

      {currentUser && !isCurrentUserAssigned && (
        <Button
          type="button"
          variant="outline"
          size="sm"
          className="w-full justify-start gap-2 border-primary/30 bg-primary/5 text-primary hover:bg-primary/10 hover:text-primary"
          onClick={() => onAssign(currentUser.id)}
          disabled={isBusy}
        >
          <UserPlus className="h-3.5 w-3.5" />
          Assign yourself
        </Button>
      )}

      <Input
        id={searchInputId}
        value={search}
        onChange={(event) => setSearch(event.target.value)}
        placeholder="Search members by name or email..."
      />

      <div className="max-h-52 overflow-y-auto rounded-md border border-border/70 bg-background/60">
        {availableMembers.length === 0 ? (
          <p className="px-3 py-4 text-xs text-muted-foreground">No matching members found.</p>
        ) : (
          <table className="w-full text-sm">
            <tbody>
              {availableMembers.map((member) => (
                <tr key={member.id} className="border-b border-border/50 last:border-b-0">
                  <td className="px-3 py-2 align-middle">
                    <p className="font-medium text-foreground">{member.displayName}</p>
                    <p className="text-xs text-muted-foreground">{member.email}</p>
                  </td>
                  <td className="px-3 py-2 text-right align-middle">
                    <Button
                      type="button"
                      variant="outline"
                      size="icon"
                      className="h-8 w-8"
                      aria-label={`Assign ${member.displayName}`}
                      onClick={() => onAssign(member.id)}
                      disabled={isBusy}
                    >
                      <UserPlus className="h-4 w-4" />
                    </Button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}
