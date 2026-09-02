import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api-config';
import { CreateLeaveRequest, LeaveRequestDto } from '../models/leave.models';

@Injectable({ providedIn: 'root' })
export class LeaveService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${API_BASE_URL}/leaves`;

  // Role-scoped by the backend: Admin gets every request, Manager gets their
  // TEAM's requests (not their own — see getByEmployee below), Employee gets
  // only their own.
  getAll(): Observable<LeaveRequestDto[]> {
    return this.http.get<LeaveRequestDto[]>(this.baseUrl);
  }

  // A Manager's own applied leave never appears in getAll() for them, since
  // that endpoint returns their team's requests. This hits the per-employee
  // history endpoint instead, which always includes the caller's own record.
  getByEmployee(employeeId: number): Observable<LeaveRequestDto[]> {
    return this.http.get<LeaveRequestDto[]>(`${API_BASE_URL}/employees/${employeeId}/leaves`);
  }

  apply(dto: CreateLeaveRequest): Observable<LeaveRequestDto> {
    return this.http.post<LeaveRequestDto>(this.baseUrl, dto);
  }

  approve(id: number): Observable<LeaveRequestDto> {
    return this.http.put<LeaveRequestDto>(`${this.baseUrl}/${id}/approve`, {});
  }

  reject(id: number): Observable<LeaveRequestDto> {
    return this.http.put<LeaveRequestDto>(`${this.baseUrl}/${id}/reject`, {});
  }

  cancel(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
