import { Routes } from '@angular/router';
import { LoginComponent } from './features/login/login.component';
import { DashboardComponent } from './features/dashboard/dashboard.component';
import { EmployeeListComponent } from './features/employees/employee-list/employee-list.component';
import { EmployeeFormComponent } from './features/employees/employee-form/employee-form.component';
import { DepartmentListComponent } from './features/departments/department-list/department-list.component';
import { DepartmentFormComponent } from './features/departments/department-form/department-form.component';
import { LeaveListComponent } from './features/leaves/leave-list/leave-list.component';
import { LeaveApplyComponent } from './features/leaves/leave-apply/leave-apply.component';
import { AppShellComponent } from './core/layout/app-shell/app-shell.component';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  {
    path: '',
    component: AppShellComponent,
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: DashboardComponent },
      {
        path: 'employees',
        component: EmployeeListComponent,
        canActivate: [roleGuard(['Admin'])]
      },
      {
        path: 'employees/add',
        component: EmployeeFormComponent,
        canActivate: [roleGuard(['Admin'])]
      },
      {
        path: 'employees/edit/:id',
        component: EmployeeFormComponent,
        canActivate: [roleGuard(['Admin'])]
      },
      {
        path: 'departments',
        component: DepartmentListComponent,
        canActivate: [roleGuard(['Admin'])]
      },
      {
        path: 'departments/add',
        component: DepartmentFormComponent,
        canActivate: [roleGuard(['Admin'])]
      },
      {
        path: 'departments/edit/:id',
        component: DepartmentFormComponent,
        canActivate: [roleGuard(['Admin'])]
      },
      { path: 'leaves', component: LeaveListComponent },
      {
        path: 'leaves/apply',
        component: LeaveApplyComponent,
        canActivate: [roleGuard(['Employee', 'Manager'])]
      }
    ]
  },
  { path: '**', redirectTo: 'login' }
];
