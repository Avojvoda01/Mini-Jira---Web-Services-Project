import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { defineConfig, loadEnv } from 'vite';
import tailwindcss from '@tailwindcss/vite';
import react from '@vitejs/plugin-react';

// https://vite.dev/config/
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '');
  const apiTarget = env.SERVER_HTTPS || env.SERVER_HTTP || 'http://localhost:5413';

  return {
    plugins: [react(), tailwindcss()],
    resolve: {
      alias: {
        '@': path.resolve(fileURLToPath(new URL('.', import.meta.url)), './src')
      }
    },
    server: {
      proxy: {
        // Proxy API calls to the app service
        '/api': {
          target: apiTarget,
          changeOrigin: true
        }
      }
    }
  };
});
