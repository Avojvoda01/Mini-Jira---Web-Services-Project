import { createContext, useContext, useMemo, useState, type ReactNode } from 'react';

export type PageHeaderContent = {
  title?: ReactNode;
  description?: ReactNode;
  actions?: ReactNode;
  meta?: ReactNode;
};

type PageHeaderContextValue = {
  content: PageHeaderContent;
  setContent: (content: PageHeaderContent) => void;
};

const PageHeaderContext = createContext<PageHeaderContextValue | undefined>(undefined);

export function PageHeaderProvider({ children }: { children: ReactNode }) {
  const [content, setContent] = useState<PageHeaderContent>({});
  const value = useMemo(() => ({ content, setContent }), [content]);

  return <PageHeaderContext.Provider value={value}>{children}</PageHeaderContext.Provider>;
}

export function usePageHeader() {
  const context = useContext(PageHeaderContext);
  if (!context) {
    throw new Error('usePageHeader must be used within PageHeaderProvider');
  }

  return context;
}
