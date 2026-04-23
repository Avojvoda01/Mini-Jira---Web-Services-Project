import { Navigate, Outlet, useSearchParams } from 'react-router-dom';
import { useAtomValue } from 'jotai';
import { authSessionAtom } from '@/store/authAtoms';

function getSafeRedirectPath(redirect: string | null) {
  if (!redirect || !redirect.startsWith('/')) {
    return '/app/projects';
  }

  if (redirect.startsWith('//') || /^[a-zA-Z][a-zA-Z\d+\-.]*:/.test(redirect)) {
    return '/app/projects';
  }

  return redirect;
}

export function GuestOnlyRoute() {
  const session = useAtomValue(authSessionAtom);
  const [searchParams] = useSearchParams();

  if (session) {
    return <Navigate to={getSafeRedirectPath(searchParams.get('redirect'))} replace />;
  }

  return <Outlet />;
}
