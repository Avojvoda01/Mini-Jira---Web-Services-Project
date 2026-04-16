import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useAtomValue } from 'jotai';
import { authSessionAtom } from '@/store/authAtoms';

export function ProtectedRoute() {
  const session = useAtomValue(authSessionAtom);
  const location = useLocation();

  if (!session) {
    const redirectTo = `${location.pathname}${location.search}`;
    return <Navigate to={`/login?redirect=${encodeURIComponent(redirectTo)}`} replace />;
  }

  return <Outlet />;
}
