import { z } from 'zod';

// How to set VITE_API_BASE_URL:
// 1. Create a .env file in src/frontend (or use .env.development for local dev).
// 2. Add VITE_API_BASE_URL=<your backend base URL>, for example:
//    VITE_API_BASE_URL=http://localhost:5000/api
// If not set, the frontend falls back to '/api'.
const envSchema = z.object({
  VITE_API_BASE_URL: z.string().optional(),
});

const parsedEnv = envSchema.safeParse(import.meta.env);

if (!parsedEnv.success) {
  throw new Error(`Invalid frontend env configuration: ${parsedEnv.error.message}`);
}

export const env = {
  apiBaseUrl: parsedEnv.data.VITE_API_BASE_URL ?? '/api',
} as const;
