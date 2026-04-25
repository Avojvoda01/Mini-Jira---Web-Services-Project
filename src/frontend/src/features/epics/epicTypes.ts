export type EpicDto = {
  id: number;
  name: string;
  description: string;
};

export type CreateEpicInput = {
  name: string;
  description: string | null;
};

export type UpdateEpicInput = {
  id: number;
  name: string;
  description: string | null;
};
