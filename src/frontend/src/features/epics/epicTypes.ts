export type EpicDto = {
  id: string;
  name: string;
  description: string;
  projectId: string;
  createdAtUtc: string;
  updatedAtUtc: string | null;
};

export type CreateEpicInput = {
  name: string;
  description: string | null;
  projectId: string;
};

export type UpdateEpicInput = {
  id: string;
  name: string;
  description: string | null;
};

export type EpicFilters = {
  projectId: string | null;
};
