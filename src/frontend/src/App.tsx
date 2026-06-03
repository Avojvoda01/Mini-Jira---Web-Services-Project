import { QueryClientProvider } from '@tanstack/react-query';
import { Provider as JotaiProvider } from 'jotai';
import { RouterProvider } from 'react-router-dom';
import { ThemeProvider } from '@/components/theme-provider';
import { jotaiStore } from './lib/jotaiStore';
import { queryClient } from './lib/queryClient';
import { router } from './router/routes';
import './App.css';

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <JotaiProvider store={jotaiStore}>
        <ThemeProvider defaultTheme="system" storageKey="mini-jira.theme">
          <RouterProvider router={router} />
        </ThemeProvider>
      </JotaiProvider>
    </QueryClientProvider>
  );
}

export default App;
