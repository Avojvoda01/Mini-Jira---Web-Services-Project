import { Navigate, Outlet } from 'react-router-dom';
import { useAtomValue } from 'jotai';
import { authSessionAtom } from '@/store/authAtoms';

export function AdminRoute() {
  const session = useAtomValue(authSessionAtom);

  if (session?.role !== 'Admin') {
    return <Navigate to="/app/projects" replace />;
  }

  return <Outlet />;
}
