import { useEffect, useMemo, useState } from 'react';
import { useAtomValue } from 'jotai';
import { Trash2 } from 'lucide-react';
import { useNavigate, useParams } from 'react-router-dom';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Separator } from '@/components/ui/separator';
import { usePageHeader } from '@/components/layout/PageHeaderContext';
import { DeleteProjectModal } from '@/components/projects/DeleteProjectModal';
import { ProjectMemberPicker } from '@/components/projects/ProjectMemberPicker';
import {
  useAddProjectMemberMutation,
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

  const [editName, setEditName] = useState('');
  const [editDescription, setEditDescription] = useState('');
  const [memberIds, setMemberIds] = useState<string[]>([]);
  const [isDeleteOpen, setIsDeleteOpen] = useState(false);
  const [isSaving, setIsSaving] = useState(false);

  useEffect(() => {
    if (project) {
      setEditName(project.name);
      setEditDescription(project.description);
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

  const handleSave = async () => {
    if (!projectId || !project) return;
    setIsSaving(true);
    try {
      await updateProjectMutation.mutateAsync({
        id: projectId,
        name: editName.trim(),
        description: editDescription.trim(),
      });

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
      setIsSaving(false);
    }
  };

  const handleDelete = async () => {
    if (!projectId) return;
    await deleteProjectMutation.mutateAsync(projectId);
    navigate('/app/projects');
  };

  const saveDisabled =
    editName.trim().length < 3 || !editDescription.trim() || isSaving;

  return (
    <section className="space-y-6">
      {project && (
        <>
          <Card className="border-border/70 bg-card/80 shadow-sm backdrop-blur-sm">
            <CardHeader>
              <CardTitle>Project details</CardTitle>
              <CardDescription>
                {canManage ? 'Update the project name and description.' : 'Project information.'}
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-2">
                <label className="text-sm font-medium text-foreground" htmlFor="project-name">
                  Project name
                </label>
                {canManage ? (
                  <Input
                    id="project-name"
                    value={editName}
                    onChange={(e) => setEditName(e.target.value)}
                    maxLength={MAX_PROJECT_NAME_LENGTH}
                  />
                ) : (
                  <p className="text-sm text-foreground">{project.name}</p>
                )}
              </div>

              <div className="space-y-2">
                <label className="text-sm font-medium text-foreground" htmlFor="project-description">
                  Description
                </label>
                {canManage ? (
                  <textarea
                    id="project-description"
                    value={editDescription}
                    onChange={(e) => setEditDescription(e.target.value)}
                    maxLength={MAX_DESCRIPTION_LENGTH}
                    className="min-h-24 w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground shadow-sm outline-none transition-colors placeholder:text-muted-foreground focus:border-ring focus:ring-2 focus:ring-ring/30"
                  />
                ) : (
                  <p className="text-sm text-muted-foreground">
                    {project.description || 'No description.'}
                  </p>
                )}
              </div>
            </CardContent>
          </Card>

          <Card className="border-border/70 bg-card/80 shadow-sm backdrop-blur-sm">
            <CardHeader>
              <CardTitle>Members</CardTitle>
              <CardDescription>
                {canManage ? 'Add or remove project members.' : 'Current project members.'}
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              {canManage ? (
                <>
                  <ProjectMemberPicker
                    members={users}
                    selectedMemberIds={memberIds}
                    onAdd={(userId) => setMemberIds((prev) => [...prev, userId])}
                    onRemove={(userId) =>
                      setMemberIds((prev) => prev.filter((id) => id !== userId))
                    }
                    searchInputId="settings-member-search"
                    isBusy={isSaving}
                  />
                  <Button onClick={handleSave} disabled={saveDisabled}>
                    Save changes
                  </Button>
                </>
              ) : (
                <div className="space-y-2">
                  {(project.memberIds ?? []).length === 0 ? (
                    <p className="text-sm text-muted-foreground">No members assigned.</p>
                  ) : (
                    <ul className="space-y-2">
                      {(project.memberIds ?? []).map((id) => {
                        const user = usersById.get(id.toLowerCase());
                        return (
                          <li
                            key={id}
                            className="flex items-center gap-3 rounded-md border border-border/60 bg-background/70 px-3 py-2"
                          >
                            <div>
                              <p className="text-sm font-medium text-foreground">
                                {user?.displayName ?? `User ${id.slice(0, 6)}`}
                              </p>
                              <p className="text-xs text-muted-foreground">
                                {user?.email ?? id}
                              </p>
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

          {canManage && (
            <Card className="border-destructive/50 bg-card/80 shadow-sm backdrop-blur-sm">
              <CardHeader>
                <CardTitle className="text-destructive">Danger zone</CardTitle>
                <CardDescription>Irreversible project actions.</CardDescription>
              </CardHeader>
              <CardContent>
                <div className="flex items-center justify-between gap-4 rounded-lg border border-destructive/30 bg-destructive/5 p-4">
                  <div>
                    <p className="text-sm font-medium text-foreground">Delete this project</p>
                    <p className="text-sm text-muted-foreground">
                      Permanently removes the project and all its data.
                    </p>
                  </div>
                  <Button
                    variant="destructive"
                    size="sm"
                    onClick={() => setIsDeleteOpen(true)}
                  >
                    <Trash2 className="mr-2 h-4 w-4" />
                    Delete project
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

      <div className="grid gap-4 lg:grid-cols-2">
        <Card className="border-border/70 bg-card/80 shadow-sm backdrop-blur-sm">
          <CardHeader>
            <CardTitle>Workspace preferences</CardTitle>
            <CardDescription>Core settings that shape the day-to-day experience.</CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            {workspacePreferences.map((item, index) => (
              <div key={item.label}>
                {index > 0 ? <Separator className="mb-4" /> : null}
                <div className="flex items-center justify-between gap-4">
                  <div>
                    <p className="text-sm font-medium text-foreground">{item.label}</p>
                    <p className="text-sm text-muted-foreground">Apply across the entire workspace.</p>
                  </div>
                  <Badge
                    variant="secondary"
                    className="border border-border/60 bg-background/80 text-foreground"
                  >
                    {item.value}
                  </Badge>
                </div>
              </div>
            ))}
          </CardContent>
        </Card>

        <Card className="border-border/70 bg-card/80 shadow-sm backdrop-blur-sm">
          <CardHeader>
            <CardTitle>Assistant preferences</CardTitle>
            <CardDescription>
              Keep future AI assistance helpful without becoming intrusive.
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="flex items-center justify-between gap-4 rounded-2xl border border-border/70 bg-background/80 p-4">
              <div>
                <p className="text-sm font-medium text-foreground">Suggestion tone</p>
                <p className="text-sm text-muted-foreground">Prefer concise task recommendations.</p>
              </div>
              <Button variant="outline" size="sm" className="border-border/70 bg-background/80">
                Balanced
              </Button>
            </div>

            <div className="flex items-center justify-between gap-4 rounded-2xl border border-border/70 bg-background/80 p-4">
              <div>
                <p className="text-sm font-medium text-foreground">Auto summaries</p>
                <p className="text-sm text-muted-foreground">
                  Prepare short board summaries for standups.
                </p>
              </div>
              <Badge className="bg-emerald-500/10 text-emerald-700 hover:bg-emerald-500/10">
                Enabled
              </Badge>
            </div>
          </CardContent>
        </Card>
      </div>
    </section>
  );
}
