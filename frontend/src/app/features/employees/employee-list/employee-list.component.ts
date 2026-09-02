import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { EmployeeService } from '../../../core/services/employee.service';
import { DepartmentService } from '../../../core/services/department.service';
import { EmployeeDto } from '../../../core/models/employee.models';
import { DepartmentDto } from '../../../core/models/department.models';

type SortField = 'name' | 'department' | 'designation' | 'leave';
type SortDir = 'asc' | 'desc';

@Component({
  selector: 'app-employee-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './employee-list.component.html',
  styleUrl: './employee-list.component.scss'
})
export class EmployeeListComponent implements OnInit {
  private readonly employeeService = inject(EmployeeService);
  private readonly departmentService = inject(DepartmentService);

  readonly employees = signal<EmployeeDto[]>([]);
  readonly departments = signal<DepartmentDto[]>([]);
  readonly loading = signal(true);
  readonly errorMessage = signal<string | null>(null);
  readonly deletingId = signal<number | null>(null);

  // Search / filter / sort / pagination — all client-side. The employee list
  // for a system like this stays small (dozens, not thousands), so there's
  // no real performance case for round-tripping to the server on every
  // keystroke. A future version with a much larger dataset would move this
  // filtering into the GET /api/employees query itself (search/page/sortBy
  // params, EF Core Where/OrderBy/Skip/Take) instead of filtering in memory.
  readonly searchTerm = signal('');
  readonly departmentFilter = signal<number | null>(null);
  readonly sortField = signal<SortField>('name');
  readonly sortDir = signal<SortDir>('asc');
  readonly page = signal(1);
  readonly pageSize = 10;

  readonly filteredSorted = computed(() => {
    const term = this.searchTerm().trim().toLowerCase();
    const deptId = this.departmentFilter();
    const field = this.sortField();
    const dir = this.sortDir();

    const filtered = this.employees().filter((e) => {
      const matchesTerm =
        !term ||
        `${e.firstName} ${e.lastName}`.toLowerCase().includes(term) ||
        e.email.toLowerCase().includes(term);
      const matchesDept = deptId === null || e.departmentId === deptId;
      return matchesTerm && matchesDept;
    });

    const sorted = [...filtered].sort((a, b) => {
      let cmp = 0;
      switch (field) {
        case 'name':
          cmp = `${a.firstName} ${a.lastName}`.localeCompare(`${b.firstName} ${b.lastName}`);
          break;
        case 'department':
          cmp = (a.departmentName ?? '').localeCompare(b.departmentName ?? '');
          break;
        case 'designation':
          cmp = a.designation.localeCompare(b.designation);
          break;
        case 'leave':
          cmp = a.usedLeaveDays - b.usedLeaveDays;
          break;
      }
      return dir === 'asc' ? cmp : -cmp;
    });

    return sorted;
  });

  readonly totalPages = computed(() => Math.max(1, Math.ceil(this.filteredSorted().length / this.pageSize)));

  readonly pagedEmployees = computed(() => {
    const start = (this.page() - 1) * this.pageSize;
    return this.filteredSorted().slice(start, start + this.pageSize);
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.errorMessage.set(null);

    this.employeeService.getAll().subscribe({
      next: (employees) => {
        this.employees.set(employees);
        this.loading.set(false);
      },
      error: () => {
        this.errorMessage.set('Could not load employees. Try refreshing the page.');
        this.loading.set(false);
      }
    });

    this.departmentService.getAll().subscribe((departments) => this.departments.set(departments));
  }

  onSearchChange(value: string): void {
    this.searchTerm.set(value);
    this.page.set(1);
  }

  onDepartmentFilterChange(value: string): void {
    this.departmentFilter.set(value ? Number(value) : null);
    this.page.set(1);
  }

  setSort(field: SortField): void {
    if (this.sortField() === field) {
      this.sortDir.set(this.sortDir() === 'asc' ? 'desc' : 'asc');
    } else {
      this.sortField.set(field);
      this.sortDir.set('asc');
    }
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages()) return;
    this.page.set(page);
  }

  remove(employee: EmployeeDto): void {
    const confirmed = confirm(`Remove ${employee.firstName} ${employee.lastName}? This can't be undone.`);
    if (!confirmed) return;

    this.deletingId.set(employee.id);

    this.employeeService.delete(employee.id).subscribe({
      next: () => {
        this.employees.update((list) => list.filter((e) => e.id !== employee.id));
        this.deletingId.set(null);
      },
      error: (err) => {
        this.deletingId.set(null);
        const message =
          err.status === 409
            ? (err.error?.message ?? 'This employee still manages a team. Reassign their reports first.')
            : 'Could not remove this employee.';
        alert(message);
      }
    });
  }
}
