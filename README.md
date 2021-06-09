# xiphos
ASP.NET Core MVC application for education purposes. Demonstrates various aspects of web application development and challenges.

## Development Setup

1. Update connection string in [appsettings.Development.json](Web/appsettings.Development.json)
2. Create databases
```powershell
Update-Database -context ProductDbContext
Update-Database -context ServiceDbContext
```
3. Run configuration: Xiphos - Kestrel


