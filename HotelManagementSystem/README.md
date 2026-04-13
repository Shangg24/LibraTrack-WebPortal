# Hotel Management System - Scaffolded Starter

This repository contains a minimal ASP.NET Core MVC (.NET 6) scaffold for a hotel/transient lodging management system.

Features included in the scaffold:
- ASP.NET Core MVC with Razor views
- EF Core with SQL Server provider
- ASP.NET Core Identity (basic setup)
- Basic domain entities: Guest, Room, Reservation, Transaction

Getting started:
1. Ensure you have .NET 6 SDK installed: https://dotnet.microsoft.com/download/dotnet/6.0
2. Update the connection string in `appsettings.json` to point to your SQL Server Express instance (default uses `.\\SQLEXPRESS`).
3. From a developer PowerShell, run:

```powershell
cd "c:\Users\Administrator\source\repos\LibraTrackStudentPortal\HotelManagementSystem"
dotnet restore; dotnet build
```

4. To create the database and run migrations, install the EF tools if you haven't:

```powershell
dotnet tool install --global dotnet-ef
```

Then run:

```powershell
dotnet ef migrations add InitialCreate
dotnet ef database update
```

5. Run the app:

```powershell
dotnet run
```

Next steps I can do for you:
- Wire up CRUD pages for Guests, Rooms, Reservations and Payments
- Implement role-based authentication and sample data seeding
- Add reporting pages (occupancy, revenue) and basic unit tests

If you want me to continue, tell me which feature to implement next or say "continue with scaffold" to have me build core CRUD pages.