import { useEffect } from 'react';
import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useAtomValue, useSetAtom } from 'jotai';
import { authSessionAtom, isTokenExpired } from '@/store/authAtoms';

export function ProtectedRoute() {
  const session = useAtomValue(authSessionAtom);
  const setSession = useSetAtom(authSessionAtom);
  const location = useLocation();

  useEffect(() => {
    if (session && isTokenExpired(session.token)) {
      setSession(null);
    }
  }, [session, setSession]);

  if (!session || isTokenExpired(session.token)) {
    const isProjectPage = location.pathname.startsWith('/app/project/');
    const redirectTo = isProjectPage ? '/app/projects' : `${location.pathname}${location.search}`;
    return <Navigate to={`/login?redirect=${encodeURIComponent(redirectTo)}`} replace />;
  }

  return <Outlet />;
}
