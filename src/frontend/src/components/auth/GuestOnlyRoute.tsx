import { Navigate, Outlet, useSearchParams } from 'react-router-dom';
import { useAtomValue } from 'jotai';
import { authSessionAtom, isTokenExpired } from '@/store/authAtoms';
import { getSafeRedirectPath } from '@/utils/safeRedirect';

export function GuestOnlyRoute() {
  const session = useAtomValue(authSessionAtom);
  const [searchParams] = useSearchParams();

  if (session && !isTokenExpired(session.token)) {
    return <Navigate to={getSafeRedirectPath(searchParams.get('redirect'))} replace />;
  }

  return <Outlet />;
}
