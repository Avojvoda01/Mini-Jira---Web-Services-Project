import { useState, type FormEvent } from 'react';
import { Link, Navigate, useNavigate, useSearchParams } from 'react-router-dom';
import { useAtomValue, useSetAtom } from 'jotai';
import { LogIn, Sparkles } from 'lucide-react';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { BackToHomeButton } from '@/components/common/BackToHomeButton';
import { authSessionAtom } from '@/store/authAtoms';
import { loginUser } from '@/features/auth/authApi';
import { getSafeRedirectPath } from '@/utils/safeRedirect';

function getDisplayName(email: string) {
  const localPart = email.split('@')[0] ?? email;
  return localPart
    .replace(/[._-]+/g, ' ')
    .replace(/\b\w/g, (character) => character.toUpperCase())
    .trim();
}

export function LoginPage() {
  const session = useAtomValue(authSessionAtom);
  const setSession = useSetAtom(authSessionAtom);
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const redirectTo = getSafeRedirectPath(searchParams.get('redirect'));
  const registered = searchParams.get('registered') === '1';
  const initialEmail = searchParams.get('email') ?? '';

  const [email, setEmail] = useState(initialEmail);
  const [password, setPassword] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleDemoLogin = () => {
    setSession({
      token: 'demo-session-token',
      email: 'demo@mini-jira.local',
      displayName: 'Demo User',
    });
    navigate(redirectTo, { replace: true });
  };

  if (session) {
    return <Navigate to={redirectTo} replace />;
  }

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setIsSubmitting(true);
    setError(null);

    try {
      const response = await loginUser({ email, password });
      setSession({
        token: response.token,
        email,
        displayName: getDisplayName(email),
      });
      navigate(redirectTo, { replace: true });
    } catch (submitError) {
      setError(submitError instanceof Error ? submitError.message : 'Unable to sign in.');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <main className="relative grid min-h-screen place-items-center px-4 py-10 sm:px-6 lg:px-8">
      <BackToHomeButton />

      <div className="grid w-full max-w-5xl gap-6 xl:grid-cols-[0.9fr_1.1fr]">
        <section className="hidden rounded-[2rem] border border-border/70 bg-card/80 p-8 shadow-sm backdrop-blur-sm xl:flex xl:flex-col xl:justify-between">
          <div className="space-y-4">
            <Badge variant="outline" className="w-fit border-border/70 bg-background/70 text-muted-foreground">
              <Sparkles className="mr-1.5 h-3.5 w-3.5" />
              Welcome back
            </Badge>
            <div className="space-y-3">
              <h1 className="text-4xl font-semibold tracking-tight text-foreground">Sign in to your workspace</h1>
              <p className="max-w-xl text-sm leading-6 text-muted-foreground">
                Keep your project view protected and return to the same project context every time.
              </p>
            </div>
          </div>

          <div className="grid gap-3 sm:grid-cols-3">
            {[
              { label: 'Projects', value: '08' },
              { label: 'Boards', value: '12' },
              { label: 'Members', value: '24' },
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
              <LogIn className="mr-1.5 h-3.5 w-3.5" />
              Sign in
            </Badge>
            <CardTitle className="text-3xl font-semibold tracking-tight">Welcome back</CardTitle>
            <CardDescription className="max-w-lg text-sm leading-6">
              {registered ? 'Account created. Sign in to continue to your workspace.' : 'Enter your credentials to continue.'}
            </CardDescription>
          </CardHeader>

          <CardContent>
            <form className="space-y-4" onSubmit={handleSubmit}>
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

              <div className="grid gap-3 sm:grid-cols-2">
                <Button type="submit" className="w-full shadow-sm" disabled={isSubmitting}>
                  {isSubmitting ? 'Signing in...' : 'Sign in'}
                </Button>
                <Button type="button" variant="outline" className="w-full border-border/70 bg-background/80 shadow-sm" onClick={handleDemoLogin}>
                  Use demo login
                </Button>
              </div>
            </form>

            <p className="mt-4 text-sm text-muted-foreground">
              Need an account?{' '}
              <Link className="font-medium text-foreground underline-offset-4 hover:underline" to={`/register?redirect=${encodeURIComponent(redirectTo)}`}>
                Register
              </Link>
            </p>
          </CardContent>
        </Card>
      </div>
    </main>
  );
}
