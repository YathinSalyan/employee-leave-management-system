import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { LeaveService } from '../../../core/services/leave.service';

@Component({
  selector: 'app-leave-apply',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './leave-apply.component.html',
  styleUrl: './leave-apply.component.scss'
})
export class LeaveApplyComponent {
  private readonly fb = inject(FormBuilder);
  private readonly leaveService = inject(LeaveService);
  private readonly router = inject(Router);

  readonly saving = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly submitted = signal(false);

  form = this.fb.nonNullable.group({
    leaveType: ['Casual', Validators.required],
    startDate: ['', Validators.required],
    endDate: ['', Validators.required],
    reason: ['']
  });

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

    this.leaveService
      .apply({
        leaveType: raw.leaveType,
        startDate: raw.startDate,
        endDate: raw.endDate,
        reason: raw.reason || null
      })
      .subscribe({
        next: () => this.router.navigate(['/leaves']),
        error: (err) => {
          this.saving.set(false);
          // Surfaces the backend's actual rule violation — e.g. "End date cannot
          // be before the start date" or "overlaps with an existing request" —
          // rather than a generic failure message.
          this.errorMessage.set(err.error?.message ?? 'Could not submit this request. Check the dates and try again.');
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
