export type EpicDto = {
  id: string;
  name: string;
  description: string;
  createdAtUtc: string;
  updatedAtUtc: string | null;
};

export type CreateEpicInput = {
  name: string;
  description: string | null;
};

export type UpdateEpicInput = {
  id: string;
  name: string;
  description: string | null;
};
