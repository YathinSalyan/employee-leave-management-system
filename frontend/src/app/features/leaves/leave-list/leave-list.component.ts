import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { LeaveService } from '../../../core/services/leave.service';
import { AuthService } from '../../../core/services/auth.service';
import { LeaveRequestDto } from '../../../core/models/leave.models';
import { LeaveTableComponent } from '../leave-table/leave-table.component';

@Component({
  selector: 'app-leave-list',
  standalone: true,
  imports: [CommonModule, RouterLink, LeaveTableComponent],
  templateUrl: './leave-list.component.html',
  styleUrl: './leave-list.component.scss'
})
export class LeaveListComponent implements OnInit {
  private readonly leaveService = inject(LeaveService);
  authService = inject(AuthService);

  readonly myRequests = signal<LeaveRequestDto[]>([]);
  readonly teamRequests = signal<LeaveRequestDto[]>([]);
  readonly mainList = signal<LeaveRequestDto[]>([]);
  readonly loading = signal(true);
  readonly errorMessage = signal<string | null>(null);
  readonly actioningId = signal<number | null>(null);

  get role(): string | null {
    return this.authService.role();
  }

  get isManager(): boolean {
    return this.role === 'Manager';
  }

  get canApply(): boolean {
    return this.role === 'Employee' || this.role === 'Manager';
  }

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.errorMessage.set(null);

    const employeeId = this.authService.getEmployeeId();

    if (this.isManager && employeeId !== null) {
      // Managers need two separate views — getAll() only returns their
      // team's requests, not their own (see LeaveService for why).
      let remaining = 2;
      const done = () => {
        remaining -= 1;
        if (remaining === 0) this.loading.set(false);
      };

      this.leaveService.getByEmployee(employeeId).subscribe({
        next: (leaves) => {
          this.myRequests.set(leaves);
          done();
        },
        error: () => {
          this.errorMessage.set('Could not load your requests.');
          done();
        }
      });

      this.leaveService.getAll().subscribe({
        next: (leaves) => {
          this.teamRequests.set(leaves);
          done();
        },
        error: () => {
          this.errorMessage.set("Could not load your team's requests.");
          done();
        }
      });
    } else {
      this.leaveService.getAll().subscribe({
        next: (leaves) => {
          this.mainList.set(leaves);
          this.loading.set(false);
        },
        error: () => {
          this.errorMessage.set('Could not load leave requests.');
          this.loading.set(false);
        }
      });
    }
  }

  cancelRequest(leave: LeaveRequestDto): void {
    const confirmed = confirm(`Cancel your ${leave.leaveType} request?`);
    if (!confirmed) return;

    this.actioningId.set(leave.id);

    this.leaveService.cancel(leave.id).subscribe({
      next: () => {
        this.myRequests.update((list) => list.filter((l) => l.id !== leave.id));
        this.mainList.update((list) => list.filter((l) => l.id !== leave.id));
        this.actioningId.set(null);
      },
      error: (err) => {
        this.actioningId.set(null);
        alert(err.error?.message ?? 'Could not cancel this request.');
      }
    });
  }

  approveRequest(leave: LeaveRequestDto): void {
    this.actioningId.set(leave.id);

    this.leaveService.approve(leave.id).subscribe({
      next: (updated) => {
        this.updateInLists(updated);
        this.actioningId.set(null);
      },
      error: (err) => {
        this.actioningId.set(null);
        alert(err.error?.message ?? 'Could not approve this request.');
      }
    });
  }

  rejectRequest(leave: LeaveRequestDto): void {
    this.actioningId.set(leave.id);

    this.leaveService.reject(leave.id).subscribe({
      next: (updated) => {
        this.updateInLists(updated);
        this.actioningId.set(null);
      },
      error: (err) => {
        this.actioningId.set(null);
        alert(err.error?.message ?? 'Could not reject this request.');
      }
    });
  }

  private updateInLists(updated: LeaveRequestDto): void {
    const replace = (list: LeaveRequestDto[]) => list.map((l) => (l.id === updated.id ? updated : l));
    this.teamRequests.update(replace);
    this.mainList.update(replace);
  }
}
