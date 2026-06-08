import { useMemo, useState } from 'react';
import { Minus, UserPlus } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import type { UserDto } from '@/features/users';

type ProjectMemberPickerProps = {
  members: UserDto[];
  selectedMemberIds: string[];
  onAdd: (userId: string) => void;
  onRemove: (userId: string) => void;
  searchInputId: string;
  isBusy?: boolean;
};

export function ProjectMemberPicker({
  members,
  selectedMemberIds,
  onAdd,
  onRemove,
  searchInputId,
  isBusy = false,
}: ProjectMemberPickerProps) {
  const [search, setSearch] = useState('');
  const normalizedSearch = search.trim().toLowerCase();

  const selectedMemberIdSet = useMemo(() => new Set(selectedMemberIds), [selectedMemberIds]);

  const selectedMembers = useMemo(
    () => members.filter((member) => selectedMemberIdSet.has(member.id)),
    [members, selectedMemberIdSet],
  );

  const availableMembers = useMemo(() => {
    return members.filter((member) => {
      if (selectedMemberIdSet.has(member.id)) {
        return false;
      }

      if (!normalizedSearch) {
        return true;
      }

      return member.displayName.toLowerCase().includes(normalizedSearch) || member.email.toLowerCase().includes(normalizedSearch);
    });
  }, [members, normalizedSearch, selectedMemberIdSet]);

  return (
    <div className="space-y-4">
      <div className="space-y-2">
        {selectedMembers.length === 0 ? (
          <p className="rounded-md border border-dashed border-border/70 bg-background/60 px-3 py-3 text-xs text-muted-foreground">
            No members assigned yet.
          </p>
        ) : (
          <div className="max-h-44 overflow-y-auto rounded-md border border-border/70 bg-background/60">
            <table className="w-full text-sm">
              <tbody>
                {selectedMembers.map((member) => (
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
                        onClick={() => onRemove(member.id)}
                        disabled={isBusy}
                        aria-label={`Remove ${member.displayName}`}
                      >
                        <Minus className="h-4 w-4" />
                      </Button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      <Input
        id={searchInputId}
        value={search}
        onChange={(event) => setSearch(event.target.value)}
        placeholder="Search available users by name or email..."
      />

      <div className="max-h-52 overflow-y-auto rounded-md border border-border/70 bg-background/60">
        {availableMembers.length === 0 ? (
          <p className="px-3 py-4 text-xs text-muted-foreground">
            {normalizedSearch ? 'No matching members found.' : 'No users left to assign.'}
          </p>
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
                      onClick={() => onAdd(member.id)}
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
