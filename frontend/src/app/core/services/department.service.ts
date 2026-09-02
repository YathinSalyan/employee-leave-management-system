import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api-config';
import { DepartmentDto, DepartmentRequest } from '../models/department.models';

@Injectable({ providedIn: 'root' })
export class DepartmentService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${API_BASE_URL}/departments`;

  getAll(): Observable<DepartmentDto[]> {
    return this.http.get<DepartmentDto[]>(this.baseUrl);
  }

  getById(id: number): Observable<DepartmentDto> {
    return this.http.get<DepartmentDto>(`${this.baseUrl}/${id}`);
  }

  create(dto: DepartmentRequest): Observable<DepartmentDto> {
    return this.http.post<DepartmentDto>(this.baseUrl, dto);
  }

  update(id: number, dto: DepartmentRequest): Observable<DepartmentDto> {
    return this.http.put<DepartmentDto>(`${this.baseUrl}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
