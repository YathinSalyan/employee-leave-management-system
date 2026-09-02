export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  expiresAt: string;
  username: string;
  role: 'Admin' | 'Manager' | 'Employee';
  employeeId: number | null;
}
