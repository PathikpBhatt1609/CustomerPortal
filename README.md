# Customer Creation Module (CCM)
Full-stack enterprise app: ASP.NET Core 8 + MS SQL Server + HTML/JS Frontend

## Default Users
| Username  | Password | Role       |
|-----------|----------|------------|
| requestor | pass123  | Requestor  |
| teamlead  | pass123  | Team Lead  |
| manager   | pass123  | Manager    |
| admin     | pass123  | Admin      |

## Quick Start
cd CCM.API
dotnet restore
dotnet ef migrations add InitialCreate --output-dir Data/Migrations
dotnet ef database update
dotnet run