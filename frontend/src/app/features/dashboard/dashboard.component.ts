import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../core/services/auth.service';
import { EmployeeService } from '../../core/services/employee.service';
import { DepartmentService } from '../../core/services/department.service';
import { LeaveService } from '../../core/services/leave.service';
import { EmployeeDto } from '../../core/models/employee.models';
import { LeaveRequestDto } from '../../core/models/leave.models';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  authService = inject(AuthService);
  private readonly employeeService = inject(EmployeeService);
  private readonly departmentService = inject(DepartmentService);
  private readonly leaveService = inject(LeaveService);

  readonly loading = signal(true);
  readonly errorMessage = signal<string | null>(null);

  readonly totalEmployees = signal(0);
  readonly totalDepartments = signal(0);

  readonly teamSize = signal(0);

  readonly profile = signal<EmployeeDto | null>(null);

  readonly totalCount = signal(0);
  readonly pendingCount = signal(0);
  readonly approvedCount = signal(0);
  readonly rejectedCount = signal(0);

  get role(): string | null {
    return this.authService.role();
  }

  logout(): void {
    this.authService.logout();
  }

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.errorMessage.set(null);

    switch (this.role) {
      case 'Admin':
        this.loadAdmin();
        break;
      case 'Manager':
        this.loadManager();
        break;
      case 'Employee':
        this.loadEmployee();
        break;
      default:
        this.loading.set(false);
    }
  }

  private tallyLeaves(leaves: LeaveRequestDto[]): void {
    this.totalCount.set(leaves.length);
    this.pendingCount.set(leaves.filter((l) => l.status === 'Pending').length);
    this.approvedCount.set(leaves.filter((l) => l.status === 'Approved').length);
    this.rejectedCount.set(leaves.filter((l) => l.status === 'Rejected').length);
  }

  private loadAdmin(): void {
    let remaining = 3;
    const done = () => {
      remaining -= 1;
      if (remaining === 0) this.loading.set(false);
    };

    this.employeeService.getAll().subscribe({
      next: (employees) => {
        this.totalEmployees.set(employees.length);
        done();
      },
      error: () => {
        this.errorMessage.set('Could not load some dashboard data.');
        done();
      }
    });

    this.departmentService.getAll().subscribe({
      next: (departments) => {
        this.totalDepartments.set(departments.length);
        done();
      },
      error: () => {
        this.errorMessage.set('Could not load some dashboard data.');
        done();
      }
    });

    this.leaveService.getAll().subscribe({
      next: (leaves) => {
        this.tallyLeaves(leaves);
        done();
      },
      error: () => {
        this.errorMessage.set('Could not load some dashboard data.');
        done();
      }
    });
  }

  private loadManager(): void {
    let remaining = 2;
    const done = () => {
      remaining -= 1;
      if (remaining === 0) this.loading.set(false);
    };

    this.employeeService.getMyTeam().subscribe({
      next: (team) => {
        this.teamSize.set(team.length);
        done();
      },
      error: () => {
        this.errorMessage.set('Could not load some dashboard data.');
        done();
      }
    });

    this.leaveService.getAll().subscribe({
      next: (leaves) => {
        this.tallyLeaves(leaves);
        done();
      },
      error: () => {
        this.errorMessage.set('Could not load some dashboard data.');
        done();
      }
    });
  }

  private loadEmployee(): void {
    let remaining = 2;
    const done = () => {
      remaining -= 1;
      if (remaining === 0) this.loading.set(false);
    };

    this.employeeService.getMyProfile().subscribe({
      next: (profile) => {
        this.profile.set(profile);
        done();
      },
      error: () => {
        this.errorMessage.set('Could not load your profile.');
        done();
      }
    });

    this.leaveService.getAll().subscribe({
      next: (leaves) => {
        this.tallyLeaves(leaves);
        done();
      },
      error: () => {
        this.errorMessage.set('Could not load your leave requests.');
        done();
      }
    });
  }
}
