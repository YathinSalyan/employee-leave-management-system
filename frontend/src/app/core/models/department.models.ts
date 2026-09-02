export interface DepartmentDto {
  id: number;
  name: string;
  description: string | null;
  employeeCount: number;
}

// Used for both create and update — the backend's PUT endpoint accepts the
// same shape as POST (there's no separate UpdateDepartmentDto on the API side).
export interface DepartmentRequest {
  name: string;
  description: string | null;
}
