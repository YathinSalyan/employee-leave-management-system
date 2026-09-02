import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api-config';
import { AdminEmployeeDto, CreateEmployeeRequest, EmployeeDto, UpdateEmployeeRequest } from '../models/employee.models';

@Injectable({ providedIn: 'root' })
export class EmployeeService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${API_BASE_URL}/employees`;

  getAll(): Observable<EmployeeDto[]> {
    return this.http.get<EmployeeDto[]>(this.baseUrl);
  }

  // Works for any role with a linked employee profile (Employee or Manager).
  getMyProfile(): Observable<EmployeeDto> {
    return this.http.get<EmployeeDto>(`${this.baseUrl}/me`);
  }

  // Manager-only on the backend — direct reports of the logged-in manager.
  getMyTeam(): Observable<EmployeeDto[]> {
    return this.http.get<EmployeeDto[]>(`${this.baseUrl}/me/team`);
  }

  // Admin-only view — includes salary. Backend returns this shape for
  // GET /api/employees/{id} specifically when the caller's role is Admin.
  getByIdForAdmin(id: number): Observable<AdminEmployeeDto> {
    return this.http.get<AdminEmployeeDto>(`${this.baseUrl}/${id}`);
  }

  create(dto: CreateEmployeeRequest): Observable<AdminEmployeeDto> {
    return this.http.post<AdminEmployeeDto>(this.baseUrl, dto);
  }

  update(id: number, dto: UpdateEmployeeRequest): Observable<EmployeeDto> {
    return this.http.put<EmployeeDto>(`${this.baseUrl}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
