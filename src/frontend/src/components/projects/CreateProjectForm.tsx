import { useEffect, useState, type FormEvent } from 'react';
import { useAtomValue } from 'jotai';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { authSessionAtom } from '@/store/authAtoms';

type CreateProjectProps = {
  open: boolean;
  onClose: () => void;
};

type CreateProjectState = {
  name: string;
  owner: string;
  description: string;
};

const MAX_PROJECT_NAME_LENGTH = 20;
const MAX_DESCRIPTION_LENGTH = 1000;

export function CreateProjectForm({ open, onClose }: CreateProjectProps) {
  const session = useAtomValue(authSessionAtom);
  const [form, setForm] = useState<CreateProjectState>({
    name: '',
    owner: '',
    description: '',
  });
  const [errors, setErrors] = useState<Partial<Record<keyof CreateProjectState, string>>>({});

  useEffect(() => {
    if (!open) {
      setForm({
        name: '',
        owner: '',
        description: '',
      });
      setErrors({});
      return;
    }

    setForm((current) => ({
      ...current,
      owner: session?.displayName ?? '',
    }));
  }, [open, session?.displayName]);

  if (!open) {
    return null;
  }

  const updateField = (field: keyof CreateProjectState, value: string) => {
    setForm((current) => ({ ...current, [field]: value }));
    setErrors((current) => ({ ...current, [field]: undefined }));
  };

  const validate = () => {
    const nextErrors: Partial<Record<keyof CreateProjectState, string>> = {};

    if (!form.name.trim()) {
      nextErrors.name = 'Project name is required.';
    } else if (form.name.trim().length > MAX_PROJECT_NAME_LENGTH) {
      nextErrors.name = `Project name must be ${MAX_PROJECT_NAME_LENGTH} characters or less.`;
    }

    if (!form.description.trim()) {
      nextErrors.description = 'Description is required.';
    } else if (form.description.trim().length > MAX_DESCRIPTION_LENGTH) {
      nextErrors.description = `Description must be ${MAX_DESCRIPTION_LENGTH} characters or less.`;
    }

    setErrors(nextErrors);
    return Object.keys(nextErrors).length === 0;
  };

  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    if (!validate()) {
      return;
    }

    onClose();
  };

  return (
    <div
      className="fixed inset-0 z-50 grid place-items-center bg-black/60 px-4 py-6"
      role="dialog"
      aria-modal="true"
      aria-labelledby="create-project-title"
      onClick={onClose}
    >
      <Card className="w-full max-w-2xl border-border/70 bg-card shadow-2xl" onClick={(event) => event.stopPropagation()}>
        <CardHeader>
          <CardTitle id="create-project-title" className="text-2xl tracking-tight">
            Create project
          </CardTitle>
          <CardDescription>Fill out the project details below. This form is ready for backend wiring later.</CardDescription>
        </CardHeader>

        <CardContent>
          <form className="space-y-5" onSubmit={handleSubmit}>
            <div className="space-y-2">
              <label className="text-sm font-medium text-foreground" htmlFor="name">
                Project name
              </label>
              <Input
                id="name"
                value={form.name}
                onChange={(event) => updateField('name', event.target.value)}
                placeholder="Enter a project name"
                  maxLength={MAX_PROJECT_NAME_LENGTH}
                aria-invalid={Boolean(errors.name)}
              />
              <p className="text-xs text-muted-foreground">Up to {MAX_PROJECT_NAME_LENGTH} characters.</p>
              {errors.name ? <p className="text-xs text-rose-700">{errors.name}</p> : null}
            </div>

            <div className="space-y-2">
              <label className="text-sm font-medium text-foreground" htmlFor="owner">
                Owner
              </label>
              <Input
                id="owner"
                value={form.owner}
                readOnly
                placeholder="Logged-in user"
                aria-readonly="true"
              />
              <p className="text-xs text-muted-foreground">Automatically set to your account name.</p>
            </div>

            <div className="space-y-2">
              <label className="text-sm font-medium text-foreground" htmlFor="description">
                Description
              </label>
              <textarea
                id="description"
                value={form.description}
                onChange={(event) => updateField('description', event.target.value)}
                placeholder="Describe the project goal and scope."
                maxLength={MAX_DESCRIPTION_LENGTH}
                aria-invalid={Boolean(errors.description)}
                className="min-h-24 w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground shadow-sm outline-none transition-colors placeholder:text-muted-foreground focus:border-ring focus:ring-2 focus:ring-ring/30"
              />
              <p className="text-xs text-muted-foreground">Up to {MAX_DESCRIPTION_LENGTH} characters.</p>
              {errors.description ? <p className="text-xs text-rose-700">{errors.description}</p> : null}
            </div>

            <div className="flex justify-end gap-2">
              <Button
                variant="outline"
                className="border-rose-300 bg-rose-50 text-rose-700 hover:bg-rose-100 hover:text-rose-800"
                onClick={onClose}
                type="button"
              >
                Cancel
              </Button>
              <Button className="border-0 bg-sky-500 text-white hover:bg-sky-600" type="submit">
                Create
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}