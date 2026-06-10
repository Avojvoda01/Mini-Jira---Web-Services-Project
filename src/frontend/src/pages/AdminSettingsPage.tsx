import { useState } from 'react';
import { useAtomValue } from 'jotai';
import { ArrowLeft, ChevronDown, Loader2, Plus, ShieldCheck, Trash2, Users } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { BacklogModal } from '@/components/backlog/BacklogModal';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger } from '@/components/ui/dropdown-menu';
import { Input } from '@/components/ui/input';
import { LoadingState } from '@/components/ui/LoadingState';
import { Separator } from '@/components/ui/separator';
import {
  useAdminUsersQuery,
  useChangeUserRoleMutation,
  useCreateAdminUserMutation,
  useDeleteAdminUserMutation,
} from '@/features/users';
import { ApiError } from '@/lib/apiClient';
import { authSessionAtom } from '@/store/authAtoms';

export function AdminSettingsPage() {
  const navigate = useNavigate();
  const session = useAtomValue(authSessionAtom);

  const { data: users = [], isLoading } = useAdminUsersQuery();
  const changeRoleMutation = useChangeUserRoleMutation();
  const deleteMutation = useDeleteAdminUserMutation();
  const createMutation = useCreateAdminUserMutation();

  const [confirmDeleteId, setConfirmDeleteId] = useState<string | null>(null);

  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [createName, setCreateName] = useState('');
  const [createEmail, setCreateEmail] = useState('');
  const [createPassword, setCreatePassword] = useState('');
  const [createError, setCreateError] = useState('');

  const adminCount = users.filter((u) => u.role === 'Admin').length;
  const userCount = users.filter((u) => u.role === 'User').length;

  const handleRoleChange = (userId: string, newRole: string) => {
    changeRoleMutation.mutate({ userId, role: newRole });
  };

  const handleDelete = (userId: string) => {
    deleteMutation.mutate(
      { userId },
      { onSuccess: () => setConfirmDeleteId(null) },
    );
  };

  const handleCreateSubmit = async () => {
    setCreateError('');
    try {
      await createMutation.mutateAsync({
        displayName: createName.trim(),
        email: createEmail.trim(),
        password: createPassword,
      });
      setIsCreateOpen(false);
      setCreateName('');
      setCreateEmail('');
      setCreatePassword('');
    } catch (err) {
      setCreateError(err instanceof ApiError ? err.message : 'Something went wrong.');
    }
  };

  const handleCreateClose = () => {
    setIsCreateOpen(false);
    setCreateName('');
    setCreateEmail('');
    setCreatePassword('');
    setCreateError('');
  };

  const createDisabled =
    createName.trim().length < 2 ||
    !createEmail.trim() ||
    createPassword.length < 8 ||
    createMutation.isPending;

  const formatDate = (iso: string) =>
    new Intl.DateTimeFormat(undefined, { month: 'short', day: 'numeric', year: 'numeric' }).format(
      new Date(iso),
    );

  return (
    <section className="space-y-6">
      {/* Page header */}
      <div className="relative overflow-hidden rounded-b-3xl rounded-t-none border border-border/70 bg-gradient-to-br from-card via-card to-muted/45 p-6 shadow-sm sm:p-8">
        <Button
          variant="outline"
          size="icon"
          onClick={() => navigate(-1)}
          aria-label="Go back"
          className="absolute right-4 top-4 z-10 h-11 w-11 border-border/70 bg-background/85 backdrop-blur-sm hover:bg-background"
        >
          <ArrowLeft className="h-5 w-5" />
        </Button>
        <div className="pointer-events-none absolute -right-20 -top-20 h-56 w-56 rounded-full bg-primary/10 blur-3xl" aria-hidden="true" />

        <div className="space-y-4">
          <Badge variant="outline" className="w-fit border-border/70 bg-background/70 text-muted-foreground">
            <ShieldCheck className="mr-1.5 h-3.5 w-3.5" />
            Admin
          </Badge>
          <div className="space-y-2">
            <h1 className="text-3xl font-semibold tracking-tight text-foreground sm:text-4xl">Admin Settings</h1>
            <p className="max-w-2xl text-sm leading-6 text-muted-foreground">
              Manage users and system-wide configuration.
            </p>
          </div>
        </div>
      </div>

      <div className="mx-2 space-y-4 lg:mx-4">
        {/* Stats strip */}
        <div className="grid gap-4 sm:grid-cols-3">
          <StatCard label="Total users" value={users.length} icon={<Users className="h-7 w-7 text-muted-foreground" />} />
          <StatCard label="Admins" value={adminCount} icon={<ShieldCheck className="h-7 w-7 text-muted-foreground" />} />
          <StatCard label="Regular users" value={userCount} icon={<Users className="h-7 w-7 text-muted-foreground" />} />
        </div>

        {/* User management */}
        <Card className="border-border/70 bg-card/80 shadow-sm backdrop-blur-sm">
          <CardHeader className="flex flex-row items-center justify-between gap-4 pb-4">
            <CardTitle className="text-base font-semibold">Users</CardTitle>
            <Button size="sm" className="h-8 gap-1.5 text-xs" onClick={() => setIsCreateOpen(true)}>
              <Plus className="h-3.5 w-3.5" />
              Create user
            </Button>
          </CardHeader>

          <CardContent className="pb-2">
            {isLoading ? (
              <LoadingState label="Loading users…" />
            ) : users.length === 0 ? (
              <p className="py-4 text-center text-sm text-muted-foreground">No users found.</p>
            ) : (
              <div className="divide-y divide-border/50">
                {users.map((user) => {
                  const isSelf = user.id.toLowerCase() === session?.userId?.toLowerCase();
                  const isConfirmingDelete = confirmDeleteId === user.id;
                  const initials = user.displayName.slice(0, 2).toUpperCase();

                  return (
                    <div key={user.id} className="flex items-center gap-3 py-3">
                      {/* Avatar */}
                      <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-primary/10 text-xs font-semibold text-primary">
                        {initials}
                      </div>

                      {/* Name + email */}
                      <div className="min-w-0 flex-1">
                        <p className="truncate text-sm font-medium text-foreground">
                          {user.displayName}
                          {isSelf && (
                            <span className="ml-2 text-xs font-normal text-muted-foreground">(you)</span>
                          )}
                        </p>
                        <p className="truncate text-xs text-muted-foreground">{user.email}</p>
                      </div>

                      {/* Joined date */}
                      <p className="hidden shrink-0 text-xs text-muted-foreground sm:block">
                        {user.createdAtUtc ? formatDate(user.createdAtUtc) : '—'}
                      </p>

                      {/* Role dropdown */}
                      <DropdownMenu>
                        <DropdownMenuTrigger asChild>
                          <Button
                            variant="outline"
                            size="sm"
                            className="h-7 gap-1 px-2 text-xs"
                            disabled={changeRoleMutation.isPending && changeRoleMutation.variables?.userId === user.id}
                          >
                            <RoleBadge role={user.role} />
                            <ChevronDown className="h-3 w-3 text-muted-foreground" />
                          </Button>
                        </DropdownMenuTrigger>
                        <DropdownMenuContent align="end">
                          <DropdownMenuItem
                            onClick={() => handleRoleChange(user.id, 'Admin')}
                            className={user.role === 'Admin' ? 'font-medium' : ''}
                          >
                            Admin
                          </DropdownMenuItem>
                          <DropdownMenuItem
                            onClick={() => handleRoleChange(user.id, 'User')}
                            className={user.role === 'User' ? 'font-medium' : ''}
                          >
                            User
                          </DropdownMenuItem>
                        </DropdownMenuContent>
                      </DropdownMenu>

                      {/* Delete / confirm */}
                      {!isSelf && (
                        isConfirmingDelete ? (
                          <div className="flex shrink-0 items-center gap-1.5">
                            <span className="text-xs text-muted-foreground">Delete?</span>
                            <Button
                              size="sm"
                              variant="destructive"
                              className="h-7 px-2 text-xs"
                              onClick={() => handleDelete(user.id)}
                              disabled={deleteMutation.isPending}
                            >
                              {deleteMutation.isPending ? <Loader2 className="h-3 w-3 animate-spin" /> : 'Yes'}
                            </Button>
                            <Button
                              size="sm"
                              variant="outline"
                              className="h-7 px-2 text-xs"
                              onClick={() => setConfirmDeleteId(null)}
                              disabled={deleteMutation.isPending}
                            >
                              No
                            </Button>
                          </div>
                        ) : (
                          <Button
                            size="icon"
                            variant="ghost"
                            className="h-7 w-7 shrink-0 text-muted-foreground hover:text-destructive"
                            onClick={() => setConfirmDeleteId(user.id)}
                          >
                            <Trash2 className="h-3.5 w-3.5" />
                          </Button>
                        )
                      )}

                      {/* Spacer so self rows align with delete-button rows */}
                      {isSelf && <div className="h-7 w-7 shrink-0" />}
                    </div>
                  );
                })}
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Create user modal */}
      {isCreateOpen && (
        <BacklogModal onClose={handleCreateClose} cardClassName="w-full max-w-md border-border/70 bg-card shadow-2xl">
          <CardHeader className="pb-2">
            <CardTitle className="text-base font-semibold">Create user</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="space-y-1.5">
              <label className="text-xs font-medium uppercase tracking-wide text-muted-foreground" htmlFor="create-name">
                Display name
              </label>
              <Input
                id="create-name"
                value={createName}
                onChange={(e) => setCreateName(e.target.value)}
                placeholder="Alice Smith"
                maxLength={100}
                autoFocus
              />
            </div>

            <div className="space-y-1.5">
              <label className="text-xs font-medium uppercase tracking-wide text-muted-foreground" htmlFor="create-email">
                Email
              </label>
              <Input
                id="create-email"
                type="email"
                value={createEmail}
                onChange={(e) => setCreateEmail(e.target.value)}
                placeholder="alice@example.com"
              />
            </div>

            <div className="space-y-1.5">
              <label className="text-xs font-medium uppercase tracking-wide text-muted-foreground" htmlFor="create-password">
                Password
              </label>
              <Input
                id="create-password"
                type="password"
                value={createPassword}
                onChange={(e) => setCreatePassword(e.target.value)}
                placeholder="••••••••"
              />
              <p className="text-xs text-muted-foreground">At least 8 characters with one letter and one number.</p>
            </div>

            {createError && <p className="text-sm text-destructive">{createError}</p>}

            <Separator />

            <div className="flex justify-end gap-2">
              <Button variant="outline" size="sm" onClick={handleCreateClose} disabled={createMutation.isPending}>
                Cancel
              </Button>
              <Button size="sm" onClick={handleCreateSubmit} disabled={createDisabled} className="gap-1.5">
                {createMutation.isPending && <Loader2 className="h-3.5 w-3.5 animate-spin" />}
                Create
              </Button>
            </div>
          </CardContent>
        </BacklogModal>
      )}
    </section>
  );
}

function RoleBadge({ role }: { role: string }) {
  if (role === 'Admin') {
    return <span className="text-amber-600 dark:text-amber-400">Admin</span>;
  }
  return <span>User</span>;
}

type StatCardProps = { label: string; value: number; icon: React.ReactNode };

function StatCard({ label, value, icon }: StatCardProps) {
  return (
    <Card className="border-border/70 bg-card/80 shadow-sm backdrop-blur-sm">
      <CardContent className="p-5">
        <div className="flex items-start justify-between">
          <div className="space-y-2">
            <p className="text-xs font-medium uppercase tracking-[0.15em] text-muted-foreground">{label}</p>
            <p className="text-3xl font-bold tracking-tight text-foreground">{value}</p>
          </div>
          {icon}
        </div>
      </CardContent>
    </Card>
  );
}
