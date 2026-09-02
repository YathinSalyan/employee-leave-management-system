import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { DepartmentService } from '../../../core/services/department.service';

@Component({
  selector: 'app-department-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './department-form.component.html',
  styleUrl: './department-form.component.scss'
})
export class DepartmentFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly departmentService = inject(DepartmentService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly submitted = signal(false);

  departmentId: number | null = null;

  get isEditMode(): boolean {
    return this.departmentId !== null;
  }

  form = this.fb.nonNullable.group({
    name: ['', Validators.required],
    description: ['']
  });

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    this.departmentId = idParam ? Number(idParam) : null;

    if (this.isEditMode && this.departmentId !== null) {
      this.departmentService.getById(this.departmentId).subscribe({
        next: (department) => {
          this.form.patchValue({
            name: department.name,
            description: department.description ?? ''
          });
          this.loading.set(false);
        },
        error: () => {
          this.errorMessage.set('Could not load this department.');
          this.loading.set(false);
        }
      });
    } else {
      this.loading.set(false);
    }
  }

  submit(): void {
    this.submitted.set(true);

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.errorMessage.set('Please fix the highlighted fields below.');
      return;
    }

    this.saving.set(true);
    this.errorMessage.set(null);

    const raw = this.form.getRawValue();
    const payload = { name: raw.name, description: raw.description || null };

    const request$ =
      this.isEditMode && this.departmentId !== null
        ? this.departmentService.update(this.departmentId, payload)
        : this.departmentService.create(payload);

    request$.subscribe({
      next: () => this.router.navigate(['/departments']),
      error: (err) => {
        this.saving.set(false);
        this.errorMessage.set(err.error?.message ?? 'Could not save this department. Check the form and try again.');
      }
    });
  }

  isInvalid(field: string): boolean {
    const control = this.form.get(field);
    return !!control && control.invalid && (control.touched || this.submitted());
  }

  errorFor(field: string): string {
    const control = this.form.get(field);
    if (!control || !control.errors) return '';
    if (control.errors['required']) return 'This field is required.';
    return 'This field is invalid.';
  }
}
