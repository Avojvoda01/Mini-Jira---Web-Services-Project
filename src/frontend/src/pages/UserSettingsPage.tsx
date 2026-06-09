import { useState } from 'react';
import { useAtom } from 'jotai';
import { ArrowLeft, KeyRound, Loader2, Settings, User } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Separator } from '@/components/ui/separator';
import { useChangePasswordMutation, useUpdateProfileMutation } from '@/features/users';
import { ApiError } from '@/lib/apiClient';
import { authSessionAtom } from '@/store/authAtoms';

export function UserSettingsPage() {
  const navigate = useNavigate();
  const [session, setSession] = useAtom(authSessionAtom);

  const [displayName, setDisplayName] = useState(session?.displayName ?? '');
  const [email, setEmail] = useState(session?.email ?? '');
  const [profileError, setProfileError] = useState('');
  const [profileSuccess, setProfileSuccess] = useState(false);

  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [passwordError, setPasswordError] = useState('');
  const [passwordSuccess, setPasswordSuccess] = useState(false);

  const updateProfile = useUpdateProfileMutation();
  const changePassword = useChangePasswordMutation();

  const profileDirty =
    displayName.trim() !== session?.displayName || email.trim() !== session?.email;

  const handleSaveProfile = async () => {
    setProfileError('');
    setProfileSuccess(false);
    if (!session) return;

    try {
      const updated = await updateProfile.mutateAsync({
        userId: session.userId,
        input: { displayName: displayName.trim(), email: email.trim() },
      });
      setSession({ ...session, displayName: updated.displayName, email: updated.email });
      setProfileSuccess(true);
    } catch (err) {
      setProfileError(err instanceof ApiError ? err.message : 'Something went wrong.');
    }
  };

  const handleChangePassword = async () => {
    setPasswordError('');
    setPasswordSuccess(false);
    if (!session) return;

    try {
      await changePassword.mutateAsync({
        userId: session.userId,
        input: { currentPassword, newPassword },
      });
      setCurrentPassword('');
      setNewPassword('');
      setConfirmPassword('');
      setPasswordSuccess(true);
    } catch (err) {
      setPasswordError(err instanceof ApiError ? err.message : 'Something went wrong.');
    }
  };

  const profileSaveDisabled =
    !profileDirty ||
    displayName.trim().length < 2 ||
    displayName.trim().length > 100 ||
    !email.trim() ||
    updateProfile.isPending;

  const passwordMismatch = confirmPassword.length > 0 && newPassword !== confirmPassword;

  const passwordSaveDisabled =
    !currentPassword ||
    newPassword.length < 8 ||
    !confirmPassword ||
    passwordMismatch ||
    changePassword.isPending;

  return (
    <section className="space-y-6">
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
            <Settings className="mr-1.5 h-3.5 w-3.5" />
            Settings
          </Badge>
          <div className="space-y-2">
            <h1 className="text-3xl font-semibold tracking-tight text-foreground sm:text-4xl">Settings</h1>
            <p className="max-w-2xl text-sm leading-6 text-muted-foreground">
              Manage your account preferences and personal configuration.
            </p>
          </div>
        </div>
      </div>

      <div className="mx-2 space-y-4 lg:mx-4">
        {/* Profile section */}
        <Card className="border-border/70 bg-card/80 shadow-sm backdrop-blur-sm">
          <CardHeader className="pb-4">
            <div className="flex items-center gap-2">
              <User className="h-4 w-4 text-muted-foreground" />
              <CardTitle className="text-base font-semibold">Profile</CardTitle>
            </div>
            <CardDescription>Update your display name and email address.</CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="space-y-1.5">
              <label className="text-xs font-medium uppercase tracking-wide text-muted-foreground" htmlFor="display-name">
                Display name
              </label>
              <Input
                id="display-name"
                value={displayName}
                onChange={(e) => {
                  setDisplayName(e.target.value);
                  setProfileSuccess(false);
                }}
                maxLength={100}
                placeholder="Your display name"
              />
            </div>

            <div className="space-y-1.5">
              <label className="text-xs font-medium uppercase tracking-wide text-muted-foreground" htmlFor="email">
                Email
              </label>
              <Input
                id="email"
                type="email"
                value={email}
                onChange={(e) => {
                  setEmail(e.target.value);
                  setProfileSuccess(false);
                }}
                placeholder="your@email.com"
              />
            </div>

            {profileError && (
              <p className="text-sm text-destructive">{profileError}</p>
            )}
            {profileSuccess && (
              <p className="text-sm text-green-600 dark:text-green-400">Profile updated successfully.</p>
            )}

            <Separator />

            <div className="flex justify-end">
              <Button
                onClick={handleSaveProfile}
                disabled={profileSaveDisabled}
                size="sm"
                className="gap-1.5"
              >
                {updateProfile.isPending && <Loader2 className="h-3.5 w-3.5 animate-spin" />}
                Save profile
              </Button>
            </div>
          </CardContent>
        </Card>

        {/* Password section */}
        <Card className="border-border/70 bg-card/80 shadow-sm backdrop-blur-sm">
          <CardHeader className="pb-4">
            <div className="flex items-center gap-2">
              <KeyRound className="h-4 w-4 text-muted-foreground" />
              <CardTitle className="text-base font-semibold">Password</CardTitle>
            </div>
            <CardDescription>Change your account password.</CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="space-y-1.5">
              <label className="text-xs font-medium uppercase tracking-wide text-muted-foreground" htmlFor="current-password">
                Current password
              </label>
              <Input
                id="current-password"
                type="password"
                value={currentPassword}
                onChange={(e) => {
                  setCurrentPassword(e.target.value);
                  setPasswordSuccess(false);
                }}
                placeholder="••••••••"
              />
            </div>

            <div className="space-y-1.5">
              <label className="text-xs font-medium uppercase tracking-wide text-muted-foreground" htmlFor="new-password">
                New password
              </label>
              <Input
                id="new-password"
                type="password"
                value={newPassword}
                onChange={(e) => {
                  setNewPassword(e.target.value);
                  setPasswordSuccess(false);
                }}
                placeholder="••••••••"
              />
              <p className="text-xs text-muted-foreground">
                At least 8 characters with one letter and one number.
              </p>
            </div>

            <div className="space-y-1.5">
              <label className="text-xs font-medium uppercase tracking-wide text-muted-foreground" htmlFor="confirm-password">
                Confirm new password
              </label>
              <Input
                id="confirm-password"
                type="password"
                value={confirmPassword}
                onChange={(e) => {
                  setConfirmPassword(e.target.value);
                  setPasswordSuccess(false);
                }}
                placeholder="••••••••"
              />
              {passwordMismatch && (
                <p className="text-sm text-destructive">Passwords do not match.</p>
              )}
            </div>

            {passwordError && (
              <p className="text-sm text-destructive">{passwordError}</p>
            )}
            {passwordSuccess && (
              <p className="text-sm text-green-600 dark:text-green-400">Password changed successfully.</p>
            )}

            <Separator />

            <div className="flex justify-end">
              <Button
                onClick={handleChangePassword}
                disabled={passwordSaveDisabled}
                size="sm"
                className="gap-1.5"
              >
                {changePassword.isPending && <Loader2 className="h-3.5 w-3.5 animate-spin" />}
                Change password
              </Button>
            </div>
          </CardContent>
        </Card>
      </div>
    </section>
  );
}
