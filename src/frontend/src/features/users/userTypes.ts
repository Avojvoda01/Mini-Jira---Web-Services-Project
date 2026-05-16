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
