import { Navigate, Outlet, useSearchParams } from 'react-router-dom';
import { useAtomValue } from 'jotai';
import { authSessionAtom } from '@/store/authAtoms';

export function GuestOnlyRoute() {
  const session = useAtomValue(authSessionAtom);
  const [searchParams] = useSearchParams();

  if (session) {
    return <Navigate to={searchParams.get('redirect') ?? '/app/projects'} replace />;
  }

  return <Outlet />;
}
