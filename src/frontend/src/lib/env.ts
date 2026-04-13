import { z } from 'zod';

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
