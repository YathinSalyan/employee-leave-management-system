#!/usr/bin/env bash
# Splits appsettings.json into a safe (placeholder) version that stays in git,
# and appsettings.Development.json holding your real local secrets, which is
# gitignored and never committed. ASP.NET Core automatically layers
# appsettings.Development.json OVER appsettings.json when
# ASPNETCORE_ENVIRONMENT=Development — which is exactly what you've been
# running locally this whole time — so nothing about your local dev workflow
# changes.
set -e

cd ~/Angular_Project/backend

if [ ! -f "appsettings.json" ]; then
  echo "Error: run this from where it can find backend/appsettings.json (expected at ~/Angular_Project/backend)."
  exit 1
fi

echo "==> Saving your current real settings to appsettings.Development.json..."
cp appsettings.json appsettings.Development.json

echo "==> Resetting appsettings.json to safe placeholders (this is the one that goes to GitHub)..."
cat > appsettings.json << 'JSONEOF'
{
  "ConnectionStrings": {
    "DefaultConnection": ""
  },
  "Jwt": {
    "Key": "",
    "Issuer": "EmployeeLeaveManagement",
    "Audience": "EmployeeLeaveManagementClient",
    "ExpiryMinutes": 120
  },
  "Cors": {
    "AllowedOrigins": [ "http://localhost:4200" ]
  },
  "Smtp": {
    "Host": "sandbox.smtp.mailtrap.io",
    "Port": "587",
    "Username": "",
    "Password": "",
    "FromEmail": "noreply@employeeleavemanagement.local",
    "FromName": "Employee Leave Management",
    "EnableSsl": "true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
JSONEOF

echo "==> Done."
echo "    Real secrets are now in:      backend/appsettings.Development.json (never committed)"
echo "    Safe placeholder version in:  backend/appsettings.json (this one goes to GitHub)"
