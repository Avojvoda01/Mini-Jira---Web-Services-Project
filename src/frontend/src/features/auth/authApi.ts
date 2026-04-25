import { apiClient } from '@/lib/apiClient';

type LoginResponse = {
  token: string;
};

type RegisterResponse = {
  message: string;
};

export type LoginInput = {
  email: string;
  password: string;
};

export type RegisterInput = {
  email: string;
  password: string;
  displayName: string;
};

export async function loginUser(input: LoginInput): Promise<LoginResponse> {
  return apiClient.post<LoginResponse>('/auth/login', input);
}

export async function registerUser(input: RegisterInput): Promise<RegisterResponse> {
  return apiClient.post<RegisterResponse>('/auth/register', input);
}
