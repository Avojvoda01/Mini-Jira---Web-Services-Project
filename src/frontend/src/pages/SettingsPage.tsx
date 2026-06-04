import { useEffect, useMemo, useState } from 'react';
import { useAtomValue } from 'jotai';
import { ChevronDown, Pencil, Trash2, X } from 'lucide-react';
import { useNavigate, useParams } from 'react-router-dom';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger } from '@/components/ui/dropdown-menu';
import { Input } from '@/components/ui/input';
import { Separator } from '@/components/ui/separator';
import { usePageHeader } from '@/components/layout/PageHeaderContext';
import { DeleteProjectModal } from '@/components/projects/DeleteProjectModal';
import { ProjectMemberPicker } from '@/components/projects/ProjectMemberPicker';
import {
  useAddProjectMemberMutation,
  useChangeProjectOwnerMutation,
  useDeleteProjectMutation,
  useProjectQuery,
  useRemoveProjectMemberMutation,
  useUpdateProjectMutation,
} from '@/features/projects';
import { useAdminUsersQuery } from '@/features/users';
import { authSessionAtom } from '@/store/authAtoms';

const MAX_PROJECT_NAME_LENGTH = 100;
const MAX_DESCRIPTION_LENGTH = 2000;

const workspacePreferences = [
  { label: 'Default landing page', value: 'Dashboard' },
  { label: 'Board density', value: 'Comfortable' },
  { label: 'Date format', value: 'MMM d, yyyy' },
];

export function SettingsPage() {
  const { setContent } = usePageHeader();
  const { projectId } = useParams();
  const navigate = useNavigate();
  const session = useAtomValue(authSessionAtom);

  const { data: project } = useProjectQuery(projectId ?? null);
  const { data: users = [] } = useAdminUsersQuery();

  const updateProjectMutation = useUpdateProjectMutation();
  const deleteProjectMutation = useDeleteProjectMutation();
  const addProjectMemberMutation = useAddProjectMemberMutation();
  const removeProjectMemberMutation = useRemoveProjectMemberMutation();
  const changeOwnerMutation = useChangeProjectOwnerMutation();

  const [isEditing, setIsEditing] = useState(false);
  const [editName, setEditName] = useState('');
  const [editDescription, setEditDescription] = useState('');
  const [memberIds, setMemberIds] = useState<string[]>([]);
  const [selectedNewOwnerId, setSelectedNewOwnerId] = useState('');
  const [isDeleteOpen, setIsDeleteOpen] = useState(false);
  const [isSavingDetails, setIsSavingDetails] = useState(false);
  const [isSavingMembers, setIsSavingMembers] = useState(false);

  useEffect(() => {
    if (project) {
      setMemberIds(project.memberIds ?? []);
    }
  }, [project]);

  useEffect(() => {
    setContent({
      title: 'Settings',
      description: 'Manage project details, members, and workspace preferences.',
    });
    return () => setContent({});
  }, [setContent]);

  const canManage = useMemo(() => {
    if (!session || !project) return false;
    return (
      session.role === 'Admin' ||
      project.createdById?.toLowerCase() === session.userId?.toLowerCase()
    );
  }, [session, project]);

  const usersById = useMemo(
    () => new Map(users.map((u) => [u.id.toLowerCase(), u])),
    [users],
  );

  const startEditing = () => {
    if (!project) return;
    setEditName(project.name);
    setEditDescription(project.description);
    setIsEditing(true);
  };

  const cancelEditing = () => {
    setIsEditing(false);
    setEditName('');
    setEditDescription('');
  };

  const handleSaveDetails = async () => {
    if (!projectId) return;
    setIsSavingDetails(true);
    try {
      await updateProjectMutation.mutateAsync({
        id: projectId,
        name: editName.trim(),
        description: editDescription.trim(),
      });
      setIsEditing(false);
    } finally {
      setIsSavingDetails(false);
    }
  };

  const handleSaveMembers = async () => {
    if (!projectId || !project) return;
    setIsSavingMembers(true);
    try {
      const currentSet = new Set(project.memberIds ?? []);
      const desiredSet = new Set(memberIds);
      const toAdd = memberIds.filter((id) => !currentSet.has(id));
      const toRemove = (project.memberIds ?? []).filter((id) => !desiredSet.has(id));

      for (const userId of toAdd) {
        await addProjectMemberMutation.mutateAsync({ projectId, userId, role: 'Member' });
      }
      for (const userId of toRemove) {
        await removeProjectMemberMutation.mutateAsync({ projectId, userId });
      }
    } finally {
      setIsSavingMembers(false);
    }
  };

  const handleChangeOwner = async () => {
    if (!projectId || !selectedNewOwnerId || !project) return;

    // Remove the incoming owner from members (they become PO, not a member)
    await removeProjectMemberMutation.mutateAsync({ projectId, userId: selectedNewOwnerId });

    // Transfer ownership
    await changeOwnerMutation.mutateAsync({ projectId, newOwnerId: selectedNewOwnerId });

    // Add the previous owner as a regular member
    if (project.createdById) {
      await addProjectMemberMutation.mutateAsync({ projectId, userId: project.createdById, role: 'Member' });
    }

    setSelectedNewOwnerId('');
  };

  const handleDelete = async () => {
    if (!projectId) return;
    await deleteProjectMutation.mutateAsync(projectId);
    navigate('/app/projects');
  };

  const detailsSaveDisabled =
    editName.trim().length < 3 || !editDescription.trim() || isSavingDetails;

  return (
    <section className="space-y-6">
      {project && (
        <>
          {/* Project details */}
          <Card className="border-border/70 bg-card/80 shadow-sm backdrop-blur-sm">
            <CardHeader className="flex flex-row items-start justify-between gap-4 pb-4">
              <CardTitle className="text-base font-semibold">Project details</CardTitle>
              {canManage && !isEditing && (
                <Button variant="outline" size="sm" onClick={startEditing} className="h-8 gap-1.5 text-xs">
                  <Pencil className="h-3.5 w-3.5" />
                  Edit
                </Button>
              )}
              {canManage && isEditing && (
                <Button variant="ghost" size="sm" onClick={cancelEditing} className="h-8 gap-1.5 text-xs text-muted-foreground">
                  <X className="h-3.5 w-3.5" />
                  Cancel
                </Button>
              )}
            </CardHeader>

            <CardContent>
              {isEditing ? (
                <div className="space-y-4">
                  <div className="space-y-1.5">
                    <label className="text-xs font-medium uppercase tracking-wide text-muted-foreground" htmlFor="project-name">
                      Name
                    </label>
                    <Input
                      id="project-name"
                      value={editName}
                      onChange={(e) => setEditName(e.target.value)}
                      maxLength={MAX_PROJECT_NAME_LENGTH}
                      autoFocus
                    />
                  </div>
                  <div className="space-y-1.5">
                    <label className="text-xs font-medium uppercase tracking-wide text-muted-foreground" htmlFor="project-description">
                      Description
                    </label>
                    <textarea
                      id="project-description"
                      value={editDescription}
                      onChange={(e) => setEditDescription(e.target.value)}
                      maxLength={MAX_DESCRIPTION_LENGTH}
                      rows={4}
                      className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground shadow-sm outline-none transition-colors placeholder:text-muted-foreground focus:border-ring focus:ring-2 focus:ring-ring/30"
                    />
                  </div>
                  <Button size="sm" onClick={handleSaveDetails} disabled={detailsSaveDisabled}>
                    Save changes
                  </Button>
                </div>
              ) : (
                <div className="space-y-3">
                  <div>
                    <p className="text-xs font-medium uppercase tracking-wide text-muted-foreground">Name</p>
                    <p className="mt-1 text-sm font-medium text-foreground">{project.name}</p>
                  </div>
                  <Separator />
                  <div>
                    <p className="text-xs font-medium uppercase tracking-wide text-muted-foreground">Description</p>
                    <p className="mt-1 text-sm leading-relaxed text-foreground">
                      {project.description || <span className="italic text-muted-foreground">No description provided.</span>}
                    </p>
                  </div>
                </div>
              )}
            </CardContent>
          </Card>

          {/* Owner */}
          <Card className="border-border/70 bg-card/80 shadow-sm backdrop-blur-sm">
            <CardHeader className="pb-4">
              <CardTitle className="text-base font-semibold">Project owner</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              {(() => {
                const owner = usersById.get((project.createdById ?? '').toLowerCase());
                const initials = owner?.displayName?.slice(0, 2).toUpperCase() ?? '??';
                return (
                  <div className="flex items-center gap-3 rounded-lg border border-border/60 bg-background/70 px-3 py-2.5">
                    <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-primary/10 text-xs font-semibold text-primary">
                      {initials}
                    </div>
                    <div className="min-w-0 flex-1">
                      <p className="truncate text-sm font-medium text-foreground">
                        {owner?.displayName ?? 'Unknown'}
                      </p>
                      <p className="truncate text-xs text-muted-foreground">{owner?.email ?? project.createdById}</p>
                    </div>
                    <span className="shrink-0 rounded-full bg-primary/10 px-2 py-0.5 text-xs font-medium text-primary">
                      Owner
                    </span>
                  </div>
                );
              })()}

              {session?.role === 'Admin' && (
                <div className="space-y-2 border-t border-border/50 pt-4">
                  <p className="text-xs font-medium uppercase tracking-wide text-muted-foreground">Transfer ownership</p>
                  <div className="flex gap-2">
                    {(() => {
                      const eligibleMembers = users.filter(
                        (u) =>
                          u.id.toLowerCase() !== (project.createdById ?? '').toLowerCase() &&
                          (project.memberIds ?? []).some((id) => id.toLowerCase() === u.id.toLowerCase()),
                      );
                      const selected = eligibleMembers.find((u) => u.id === selectedNewOwnerId);
                      return (
                        <>
                          <DropdownMenu>
                            <DropdownMenuTrigger asChild>
                              <Button
                                variant="outline"
                                className="flex-1 justify-between font-normal"
                                disabled={eligibleMembers.length === 0}
                              >
                                <span className={selected ? 'text-foreground' : 'text-muted-foreground'}>
                                  {selected ? selected.displayName : eligibleMembers.length === 0 ? 'No members eligible' : 'Select new owner…'}
                                </span>
                                <ChevronDown className="ml-2 h-4 w-4 shrink-0 text-muted-foreground" />
                              </Button>
                            </DropdownMenuTrigger>
                            <DropdownMenuContent align="start" className="w-72">
                              {eligibleMembers.map((u) => (
                                <DropdownMenuItem
                                  key={u.id}
                                  onClick={() => setSelectedNewOwnerId(u.id)}
                                  className="flex flex-col items-start gap-0.5"
                                >
                                  <span className="font-medium">{u.displayName}</span>
                                  <span className="text-xs text-muted-foreground">{u.email}</span>
                                </DropdownMenuItem>
                              ))}
                            </DropdownMenuContent>
                          </DropdownMenu>
                          <Button
                            size="sm"
                            onClick={handleChangeOwner}
                            disabled={!selectedNewOwnerId || changeOwnerMutation.isPending}
                          >
                            Transfer
                          </Button>
                        </>
                      );
                    })()}
                  </div>
                </div>
              )}
            </CardContent>
          </Card>

          {/* Members */}
          <Card className="border-border/70 bg-card/80 shadow-sm backdrop-blur-sm">
            <CardHeader className="pb-4">
              <div className="flex items-center justify-between gap-4">
                <CardTitle className="text-base font-semibold">Members</CardTitle>
                <span className="rounded-full bg-muted px-2.5 py-0.5 text-xs font-medium text-muted-foreground">
                  {(project.memberIds ?? []).length}
                </span>
              </div>
            </CardHeader>

            <CardContent className="space-y-4">
              {canManage ? (
                <>
                  <ProjectMemberPicker
                    members={users.filter((u) => u.id.toLowerCase() !== (project.createdById ?? '').toLowerCase())}
                    selectedMemberIds={memberIds}
                    onAdd={(userId) => setMemberIds((prev) => [...prev, userId])}
                    onRemove={(userId) => setMemberIds((prev) => prev.filter((id) => id !== userId))}
                    searchInputId="settings-member-search"
                    isBusy={isSavingMembers}
                  />
                  <Button size="sm" onClick={handleSaveMembers} disabled={isSavingMembers}>
                    Save members
                  </Button>
                </>
              ) : (
                <div className="space-y-2">
                  {(project.memberIds ?? []).length === 0 ? (
                    <p className="text-sm text-muted-foreground">No members assigned to this project.</p>
                  ) : (
                    <ul className="space-y-1">
                      {(project.memberIds ?? []).map((id) => {
                        const user = usersById.get(id.toLowerCase());
                        const initials = user?.displayName?.slice(0, 2).toUpperCase() ?? '??';
                        return (
                          <li key={id} className="flex items-center gap-3 rounded-lg px-2 py-2 hover:bg-muted/50">
                            <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-primary/10 text-xs font-semibold text-primary">
                              {initials}
                            </div>
                            <div className="min-w-0">
                              <p className="truncate text-sm font-medium text-foreground">
                                {user?.displayName ?? `User ${id.slice(0, 6)}`}
                              </p>
                              <p className="truncate text-xs text-muted-foreground">{user?.email ?? id}</p>
                            </div>
                          </li>
                        );
                      })}
                    </ul>
                  )}
                </div>
              )}
            </CardContent>
          </Card>

          {/* Danger zone */}
          {canManage && (
            <Card className="border-destructive/40 bg-card/80 shadow-sm backdrop-blur-sm">
              <CardHeader className="pb-4">
                <CardTitle className="text-base font-semibold text-destructive">Danger zone</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="flex items-center justify-between gap-4 rounded-lg border border-destructive/25 bg-destructive/5 p-4">
                  <div>
                    <p className="text-sm font-medium text-foreground">Delete this project</p>
                    <p className="text-xs text-muted-foreground">Permanently removes the project and all its data.</p>
                  </div>
                  <Button variant="destructive" size="sm" onClick={() => setIsDeleteOpen(true)}>
                    <Trash2 className="mr-1.5 h-3.5 w-3.5" />
                    Delete
                  </Button>
                </div>
              </CardContent>
            </Card>
          )}

          <DeleteProjectModal
            isOpen={isDeleteOpen}
            projectName={project.name}
            onClose={() => setIsDeleteOpen(false)}
            onConfirm={handleDelete}
            isPending={deleteProjectMutation.isPending}
          />
        </>
      )}

      {/* Workspace preferences */}
      <div className="grid gap-4 lg:grid-cols-2">
        <Card className="border-border/70 bg-card/80 shadow-sm backdrop-blur-sm">
          <CardHeader className="pb-4">
            <CardTitle className="text-base font-semibold">Workspace preferences</CardTitle>
          </CardHeader>
          <CardContent className="space-y-0">
            {workspacePreferences.map((item, index) => (
              <div key={item.label}>
                {index > 0 && <Separator />}
                <div className="flex items-center justify-between gap-4 py-3">
                  <p className="text-sm text-foreground">{item.label}</p>
                  <Badge variant="secondary" className="border border-border/60 bg-background/80 text-foreground">
                    {item.value}
                  </Badge>
                </div>
              </div>
            ))}
          </CardContent>
        </Card>

        <Card className="border-border/70 bg-card/80 shadow-sm backdrop-blur-sm">
          <CardHeader className="pb-4">
            <CardTitle className="text-base font-semibold">Assistant preferences</CardTitle>
          </CardHeader>
          <CardContent className="space-y-0">
            <div className="flex items-center justify-between gap-4 py-3">
              <p className="text-sm text-foreground">Suggestion tone</p>
              <Button variant="outline" size="sm" className="h-7 border-border/70 bg-background/80 text-xs">
                Balanced
              </Button>
            </div>
            <Separator />
            <div className="flex items-center justify-between gap-4 py-3">
              <p className="text-sm text-foreground">Auto summaries</p>
              <Badge className="bg-emerald-500/10 text-emerald-700 hover:bg-emerald-500/10">Enabled</Badge>
            </div>
          </CardContent>
        </Card>
      </div>
    </section>
  );
}
