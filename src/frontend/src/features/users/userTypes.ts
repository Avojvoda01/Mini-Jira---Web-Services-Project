export type UserDto = {
  id: string;
  email: string;
  displayName: string;
  role: string;
};

export type CreateAdminUserInput = {
  email: string;
  password: string;
  displayName: string;
};

export type DeleteAdminUserInput = {
  userId: string;
};

export type UpdateProfileInput = {
  displayName: string;
  email: string;
};

export type ChangePasswordInput = {
  currentPassword: string;
  newPassword: string;
};
