import { QueryClientProvider } from '@tanstack/react-query';
import { Provider as JotaiProvider } from 'jotai';
import { RouterProvider } from 'react-router-dom';
import { queryClient } from './lib/queryClient';
import { router } from './router/routes';
import './App.css';

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <JotaiProvider>
        <RouterProvider router={router} />
      </JotaiProvider>
    </QueryClientProvider>
  );
}

export default App;
