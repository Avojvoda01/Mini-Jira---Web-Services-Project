export type ProjectDto = {
  id: string;
  name: string;
  description: string;
  createdAtUtc: string;
  updatedAtUtc: string | null;
};

export type CreateProjectInput = {
  name: string;
  description: string;
};

export type UpdateProjectInput = {
  id: string;
  name: string;
  description: string;
};
