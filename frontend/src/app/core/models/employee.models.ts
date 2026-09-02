export interface EmployeeDto {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  phone: string | null;
  dateOfBirth: string; // ISO date string from the API
  dateOfJoining: string;
  designation: string;
  departmentId: number;
  departmentName: string | null;
  managerId: number | null;
  managerName: string | null;
  annualLeaveEntitlement: number;
  usedLeaveDays: number;
  remainingLeaveDays: number;
}

// Only ever returned to Admin — includes salary, which EmployeeDto deliberately omits.
export interface AdminEmployeeDto extends EmployeeDto {
  salary: number;
}

export interface CreateEmployeeRequest {
  firstName: string;
  lastName: string;
  email: string;
  phone: string | null;
  dateOfBirth: string;
  dateOfJoining: string;
  designation: string;
  salary: number;
  departmentId: number;
  managerId: number | null;
  annualLeaveEntitlement: number;
  username: string;
  password: string;
  role: 'Admin' | 'Manager' | 'Employee';
}

export interface UpdateEmployeeRequest {
  firstName: string;
  lastName: string;
  email: string;
  phone: string | null;
  dateOfBirth: string;
  dateOfJoining: string;
  designation: string;
  salary: number;
  departmentId: number;
  managerId: number | null;
  annualLeaveEntitlement: number;
}
