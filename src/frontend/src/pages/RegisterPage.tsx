import { useState, type FormEvent } from 'react';
import { Link, Navigate, useNavigate, useSearchParams } from 'react-router-dom';
import { useAtomValue } from 'jotai';
import { Sparkles, UserPlus } from 'lucide-react';
import { Badge } from '@/components/ui/badge';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import { BackToHomeButton } from '@/components/common/BackToHomeButton';
import { authSessionAtom } from '@/store/authAtoms';
import { registerUser } from '@/features/auth/authApi';

export function RegisterPage() {
  const session = useAtomValue(authSessionAtom);
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const redirectTo = searchParams.get('redirect') ?? '/app/projects';

  const [displayName, setDisplayName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  if (session) {
    return <Navigate to={redirectTo} replace />;
  }

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setIsSubmitting(true);
    setError(null);

    try {
      await registerUser({ displayName, email, password });
      navigate(`/login?registered=1&email=${encodeURIComponent(email)}&redirect=${encodeURIComponent(redirectTo)}`, {
        replace: true,
      });
    } catch (submitError) {
      setError(submitError instanceof Error ? submitError.message : 'Unable to create your account.');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <main className="relative grid min-h-screen place-items-center px-4 py-10 sm:px-6 lg:px-8">
      <BackToHomeButton />

      <div className="grid w-full max-w-5xl gap-6 xl:grid-cols-[1.1fr_0.9fr]">
        <section className="hidden rounded-[2rem] border border-border/70 bg-card/80 p-8 shadow-sm backdrop-blur-sm xl:flex xl:flex-col xl:justify-between">
          <div className="space-y-4">
            <Badge variant="outline" className="w-fit border-border/70 bg-background/70 text-muted-foreground">
              <Sparkles className="mr-1.5 h-3.5 w-3.5" />
              Create account
            </Badge>
            <div className="space-y-3">
              <h1 className="text-4xl font-semibold tracking-tight text-foreground">Set up access for your team</h1>
              <p className="max-w-xl text-sm leading-6 text-muted-foreground">
                Register once, then move straight into projects, boards, and delivery tracking.
              </p>
            </div>
          </div>

          <div className="grid gap-3 sm:grid-cols-3">
            {[
              { label: 'Fast setup', value: '1 min' },
              { label: 'Protected', value: 'Yes' },
              { label: 'Ready', value: 'Now' },
            ].map((item) => (
              <Card key={item.label} className="border-border/70 bg-background/80">
                <CardContent className="p-4">
                  <p className="text-xs font-medium uppercase tracking-[0.18em] text-muted-foreground">{item.label}</p>
                  <p className="mt-2 text-2xl font-semibold tracking-tight text-foreground">{item.value}</p>
                </CardContent>
              </Card>
            ))}
          </div>
        </section>

        <Card className="border-border/70 bg-card/85 shadow-sm backdrop-blur-sm">
          <CardHeader className="space-y-3">
            <Badge variant="secondary" className="w-fit border border-border/60 bg-background/80 text-foreground">
              <UserPlus className="mr-1.5 h-3.5 w-3.5" />
              Register
            </Badge>
            <CardTitle className="text-3xl font-semibold tracking-tight">Create your account</CardTitle>
            <CardDescription className="max-w-lg text-sm leading-6">
              Add your details once and sign in to continue to the workspace.
            </CardDescription>
          </CardHeader>

          <CardContent>
            <form className="space-y-4" onSubmit={handleSubmit}>
              <div className="space-y-2">
                <label className="text-sm font-medium text-foreground" htmlFor="displayName">
                  Display name
                </label>
                <Input
                  id="displayName"
                  value={displayName}
                  onChange={(event) => setDisplayName(event.target.value)}
                  placeholder="Alex Johnson"
                  required
                />
              </div>

              <div className="space-y-2">
                <label className="text-sm font-medium text-foreground" htmlFor="email">
                  Email
                </label>
                <Input id="email" type="email" value={email} onChange={(event) => setEmail(event.target.value)} required />
              </div>

              <div className="space-y-2">
                <label className="text-sm font-medium text-foreground" htmlFor="password">
                  Password
                </label>
                <Input
                  id="password"
                  type="password"
                  value={password}
                  onChange={(event) => setPassword(event.target.value)}
                  required
                />
              </div>

              {error ? (
                <p className="rounded-2xl border border-rose-200 bg-rose-50 px-3 py-2 text-sm text-rose-700">{error}</p>
              ) : null}

              <Button type="submit" className="w-full shadow-sm" disabled={isSubmitting}>
                {isSubmitting ? 'Creating account...' : 'Create account'}
              </Button>
            </form>

            <p className="mt-4 text-sm text-muted-foreground">
              Already have an account?{' '}
              <Link className="font-medium text-foreground underline-offset-4 hover:underline" to={`/login?redirect=${encodeURIComponent(redirectTo)}`}>
                Sign in
              </Link>
            </p>
          </CardContent>
        </Card>
      </div>
    </main>
  );
}
