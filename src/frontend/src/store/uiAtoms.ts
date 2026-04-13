import { atom } from 'jotai';

export type BoardViewMode = 'board' | 'backlog';

export const boardViewModeAtom = atom<BoardViewMode>('board');
export const selectedProjectIdAtom = atom<string | null>(null);
export const searchTextAtom = atom('');
