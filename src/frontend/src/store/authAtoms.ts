import { atom } from 'jotai';
import { atomWithStorage, createJSONStorage } from 'jotai/utils';

export type AuthSession = {
  token: string;
  email: string;
  displayName: string;
};

const authStorage = createJSONStorage<AuthSession | null>(() => localStorage);

export const authSessionAtom = atomWithStorage<AuthSession | null>(
  'mini-jira.auth.session',
  null,
  authStorage,
  { getOnInit: true },
);

export const isAuthenticatedAtom = atom((get) => Boolean(get(authSessionAtom)?.token));
