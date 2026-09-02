import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { DepartmentService } from '../../../core/services/department.service';
import { DepartmentDto } from '../../../core/models/department.models';

@Component({
  selector: 'app-department-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './department-list.component.html',
  styleUrl: './department-list.component.scss'
})
export class DepartmentListComponent implements OnInit {
  private readonly departmentService = inject(DepartmentService);

  readonly departments = signal<DepartmentDto[]>([]);
  readonly loading = signal(true);
  readonly errorMessage = signal<string | null>(null);
  readonly deletingId = signal<number | null>(null);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.errorMessage.set(null);

    this.departmentService.getAll().subscribe({
      next: (departments) => {
        this.departments.set(departments);
        this.loading.set(false);
      },
      error: () => {
        this.errorMessage.set('Could not load departments. Try refreshing the page.');
        this.loading.set(false);
      }
    });
  }

  remove(department: DepartmentDto): void {
    const confirmed = confirm(`Remove ${department.name}? This can't be undone.`);
    if (!confirmed) return;

    this.deletingId.set(department.id);

    this.departmentService.delete(department.id).subscribe({
      next: () => {
        this.departments.update((list) => list.filter((d) => d.id !== department.id));
        this.deletingId.set(null);
      },
      error: (err) => {
        this.deletingId.set(null);
        // 409 = backend's "still has employees assigned" conflict rule.
        const message =
          err.status === 409
            ? (err.error?.message ?? 'This department still has employees assigned to it.')
            : 'Could not remove this department.';
        alert(message);
      }
    });
  }
}
