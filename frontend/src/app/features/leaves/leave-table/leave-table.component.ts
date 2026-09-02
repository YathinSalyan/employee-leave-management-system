import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LeaveRequestDto } from '../../../core/models/leave.models';

@Component({
  selector: 'app-leave-table',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './leave-table.component.html',
  styleUrl: './leave-table.component.scss'
})
export class LeaveTableComponent {
  @Input({ required: true }) leaves: LeaveRequestDto[] = [];
  @Input() showEmployeeColumn = false;
  @Input() mode: 'cancel' | 'approveReject' | 'none' = 'none';
  @Input() actioningId: number | null = null;
  @Input() emptyMessage = 'No leave requests.';

  @Output() cancel = new EventEmitter<LeaveRequestDto>();
  @Output() approve = new EventEmitter<LeaveRequestDto>();
  @Output() reject = new EventEmitter<LeaveRequestDto>();
}
