import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { EmployeeService } from '../../../core/services/employee.service';
import { DepartmentService } from '../../../core/services/department.service';
import { DepartmentDto } from '../../../core/models/department.models';
import { EmployeeDto } from '../../../core/models/employee.models';

@Component({
  selector: 'app-employee-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './employee-form.component.html',
  styleUrl: './employee-form.component.scss'
})
export class EmployeeFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly employeeService = inject(EmployeeService);
  private readonly departmentService = inject(DepartmentService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly departments = signal<DepartmentDto[]>([]);
  readonly potentialManagers = signal<EmployeeDto[]>([]);
  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly submitted = signal(false);

  employeeId: number | null = null;

  get isEditMode(): boolean {
    return this.employeeId !== null;
  }

  // All fields live in one form for both modes. In edit mode we clear the
  // validators on username/password/role and hide them in the template —
  // the backend has no endpoint to change those after creation, so an edit
  // form must not offer to.
  form = this.fb.nonNullable.group({
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    phone: [''],
    dateOfBirth: ['', Validators.required],
    dateOfJoining: ['', Validators.required],
    designation: ['', Validators.required],
    salary: [0, [Validators.required, Validators.min(0)]],
    departmentId: [null as number | null, Validators.required],
    managerId: [null as number | null],
    annualLeaveEntitlement: [20, [Validators.required, Validators.min(0)]],
    username: ['', Validators.required],
    password: ['', [Validators.required, Validators.minLength(6)]],
    role: ['Employee', Validators.required]
  });

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    this.employeeId = idParam ? Number(idParam) : null;

    if (this.isEditMode) {
      for (const field of ['username', 'password', 'role'] as const) {
        const control = this.form.get(field);
        control?.clearValidators();
        control?.updateValueAndValidity();
      }
    }

    this.departmentService.getAll().subscribe((departments) => this.departments.set(departments));

    this.employeeService.getAll().subscribe((employees) => {
      // Exclude self from the manager dropdown — the backend also rejects
      // this, but there's no reason to let the form offer an invalid choice.
      this.potentialManagers.set(employees.filter((e) => e.id !== this.employeeId));
    });

    if (this.isEditMode && this.employeeId !== null) {
      this.employeeService.getByIdForAdmin(this.employeeId).subscribe({
        next: (employee) => {
          this.form.patchValue({
            firstName: employee.firstName,
            lastName: employee.lastName,
            email: employee.email,
            phone: employee.phone ?? '',
            dateOfBirth: employee.dateOfBirth.substring(0, 10),
            dateOfJoining: employee.dateOfJoining.substring(0, 10),
            designation: employee.designation,
            salary: employee.salary,
            departmentId: employee.departmentId,
            managerId: employee.managerId,
            annualLeaveEntitlement: employee.annualLeaveEntitlement
          });
          this.loading.set(false);
        },
        error: () => {
          this.errorMessage.set('Could not load this employee.');
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

    const sharedFields = {
      firstName: raw.firstName,
      lastName: raw.lastName,
      email: raw.email,
      phone: raw.phone || null,
      dateOfBirth: raw.dateOfBirth,
      dateOfJoining: raw.dateOfJoining,
      designation: raw.designation,
      salary: raw.salary,
      departmentId: raw.departmentId as number,
      managerId: raw.managerId,
      annualLeaveEntitlement: raw.annualLeaveEntitlement
    };

    if (this.isEditMode && this.employeeId !== null) {
      this.employeeService.update(this.employeeId, sharedFields).subscribe({
        next: () => this.router.navigate(['/employees']),
        error: (err) => this.handleSaveError(err)
      });
    } else {
      this.employeeService
        .create({
          ...sharedFields,
          username: raw.username,
          password: raw.password,
          role: raw.role as 'Admin' | 'Manager' | 'Employee'
        })
        .subscribe({
          next: () => this.router.navigate(['/employees']),
          error: (err) => this.handleSaveError(err)
        });
    }
  }

  private handleSaveError(err: any): void {
    this.saving.set(false);
    this.errorMessage.set(err.error?.message ?? 'Could not save this employee. Check the form and try again.');
  }

  // Used by the template to decide whether to show a field's error state.
  isInvalid(field: string): boolean {
    const control = this.form.get(field);
    return !!control && control.invalid && (control.touched || this.submitted());
  }

  errorFor(field: string): string {
    const control = this.form.get(field);
    if (!control || !control.errors) return '';

    if (control.errors['required']) return 'This field is required.';
    if (control.errors['email']) return 'Enter a valid email address, like name@example.com.';
    if (control.errors['minlength']) {
      return `Must be at least ${control.errors['minlength'].requiredLength} characters.`;
    }
    if (control.errors['min']) return `Must be ${control.errors['min'].min} or greater.`;

    return 'This field is invalid.';
  }
}
