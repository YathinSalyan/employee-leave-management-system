export type LeaveStatus = 'Pending' | 'Approved' | 'Rejected';

export interface LeaveRequestDto {
  id: number;
  employeeId: number;
  employeeName: string | null;
  leaveType: string;
  startDate: string;
  endDate: string;
  durationInDays: number;
  reason: string | null;
  status: LeaveStatus;
  appliedDate: string;
  approvedByName: string | null;
  approvedDate: string | null;
}

export interface CreateLeaveRequest {
  leaveType: string;
  startDate: string;
  endDate: string;
  reason: string | null;
}
