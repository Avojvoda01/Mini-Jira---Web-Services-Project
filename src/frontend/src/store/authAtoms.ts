import { atom } from 'jotai';
import { atomWithStorage, createJSONStorage } from 'jotai/utils';

export type AuthSession = {
  token: string;
  userId: string;
  email: string;
  displayName: string;
  role: string;
};

const authStorage = createJSONStorage<AuthSession | null>(() => localStorage);

export const authSessionAtom = atomWithStorage<AuthSession | null>(
  'mini-jira.auth.session',
  null,
  authStorage,
  { getOnInit: true },
);

export const isAuthenticatedAtom = atom((get) => Boolean(get(authSessionAtom)?.token));

export function isTokenExpired(token: string): boolean {
  try {
    const payload = JSON.parse(atob(token.split('.')[1]!)) as { exp?: number };
    return typeof payload.exp === 'number' && Date.now() >= payload.exp * 1000;
  } catch {
    return true;
  }
}
