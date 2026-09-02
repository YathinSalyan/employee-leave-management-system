#!/usr/bin/env bash
# Sets up ONE combined git repo at ~/Angular_Project covering both backend/
# and frontend/, with a clean, logically-grouped commit history instead of
# one giant dump. Safe to review before running — every step just stages
# specific files/folders and commits them.
set -e

if [ ! -d "backend" ] || [ ! -d "frontend" ]; then
  echo "Error: run this from inside ~/Angular_Project (this script expects to find backend/ and frontend/ right here)."
  echo "Example: cd ~/Angular_Project && bash setup-git.sh"
  exit 1
fi

echo "==> Removing frontend's separate git repo (created by 'ng new')..."
rm -rf frontend/.git

echo "==> Writing combined .gitignore..."
cat > .gitignore << 'EOF'
# Build artifacts
bin/
obj/
node_modules/
dist/
.angular/

# Editor / OS
*.user
.vs/
.DS_Store

# Logs
npm-debug.log*
EOF

echo "==> Initializing repo..."
git init
git branch -m main

echo "==> Commit 1/17: Initial ASP.NET Core Web API setup"
git add .gitignore backend/EmployeeLeaveManagement.csproj backend/Program.cs backend/appsettings.json backend/Models/ backend/Common/ backend/Middleware/
git commit -m "Initial ASP.NET Core Web API setup"

echo "==> Commit 2/17: Add SQL Server EF Core DbContext, migrations, and dev seeding"
git add backend/Data/ backend/Migrations/
git commit -m "Add SQL Server EF Core DbContext, migrations, and dev seeding"

echo "==> Commit 3/17: Add JWT authentication"
git add backend/Controllers/AuthController.cs backend/Services/AuthService.cs backend/DTOs/Auth/
git commit -m "Add JWT authentication"

echo "==> Commit 4/17: Implement employee management APIs"
git add backend/Controllers/EmployeeController.cs backend/Services/EmployeeService.cs backend/DTOs/Employee/
git commit -m "Implement employee management APIs"

echo "==> Commit 5/17: Implement department management APIs"
git add backend/Controllers/DepartmentController.cs backend/Services/DepartmentService.cs backend/DTOs/Department/
git commit -m "Implement department management APIs"

echo "==> Commit 6/17: Implement leave management APIs with business rule validation"
git add backend/Controllers/LeaveController.cs backend/Services/LeaveService.cs backend/DTOs/Leave/
git commit -m "Implement leave management APIs with business rule validation"

echo "==> Commit 7/17: Add automated email notifications for leave submission and decisions"
git add backend/Services/EmailService.cs
git commit -m "Add automated email notifications for leave submission and decisions"

echo "==> Commit 8/17: Add backend README with setup instructions and design decisions"
git add backend/README.md
git commit -m "Add backend README with setup instructions and design decisions"

echo "==> Commit 9/17: Initial Angular project setup"
git add frontend/angular.json frontend/package.json frontend/package-lock.json \
        frontend/tsconfig.json frontend/tsconfig.app.json frontend/tsconfig.spec.json \
        frontend/.editorconfig frontend/.gitignore frontend/.vscode frontend/public \
        frontend/README.md frontend/src/index.html frontend/src/main.ts \
        frontend/src/app/app.component.ts frontend/src/app/app.component.html \
        frontend/src/app/app.component.scss frontend/src/app/app.component.spec.ts
git commit -m "Initial Angular project setup"

echo "==> Commit 10/17: Add core config, models, and API services"
git add frontend/src/app/core/config frontend/src/app/core/models frontend/src/app/core/services
git commit -m "Add core config, models, and API services"

echo "==> Commit 11/17: Add authentication: login, JWT interceptor, and route guards"
git add frontend/src/app/features/login frontend/src/app/core/interceptors frontend/src/app/core/guards
git commit -m "Add authentication: login, JWT interceptor, and route guards"

echo "==> Commit 12/17: Add employee management UI"
git add frontend/src/app/features/employees
git commit -m "Add employee management UI"

echo "==> Commit 13/17: Add department management UI"
git add frontend/src/app/features/departments
git commit -m "Add department management UI"

echo "==> Commit 14/17: Add leave management UI with role-based views"
git add frontend/src/app/features/leaves
git commit -m "Add leave management UI with role-based views"

echo "==> Commit 15/17: Add role-specific dashboards"
git add frontend/src/app/features/dashboard
git commit -m "Add role-specific dashboards"

echo "==> Commit 16/17: Add persistent sidebar layout and shared design system"
git add frontend/src/app/core/layout frontend/src/styles.scss frontend/src/app/app.routes.ts frontend/src/app/app.config.ts
git commit -m "Add persistent sidebar layout and shared design system"

echo "==> Commit 17/17: Final polish and any remaining files"
git add -A
git commit -m "Final polish and cleanup" --allow-empty

echo ""
echo "==> Done. Commit history:"
git log --oneline
