# Database Setup Guide

## Configuration

The app reads the database connection string in this order:

1. Environment variable `QLBH_CONNECTION_STRING`
2. Local SQL Server fallback:

   ```text
   Server=localhost;Database=QuanLyBanHang;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;
   ```

Do not commit SQL usernames or passwords. For SQL Server authentication, set the environment variable locally:

```powershell
$env:QLBH_CONNECTION_STRING="Server=your_server;Database=QuanLyBanHang;User Id=your_user;Password=your_password;Encrypt=True;TrustServerCertificate=True;"
```

## Schema

The EF model is defined in `Database/ApplicationDbContext.cs`. Current migrations:

- `InitialCreate`
- `DemoReadySchema`

On startup, `DatabaseConfiguration.InitializeDatabaseAsync` runs pending migrations and then calls `DatabaseSeeder.SeedAsync`.

## Seed Data

The seed script is idempotent and only inserts into empty tables. It creates:

- Roles: `Admin`, `Cashier`, `Storekeeper`
- Users: `admin`, `cashier`, `storekeeper`
- Demo categories/products
- Demo customers, including `Khach le`
- One sample order

## Developer Commands

Restore and build:

```powershell
dotnet restore
dotnet build
```

Run tests:

```powershell
dotnet test
```

Check package vulnerabilities:

```powershell
dotnet list package --vulnerable --include-transitive
```

Apply migrations manually when `dotnet-ef` is installed:

```powershell
dotnet ef database update
```

The app also applies migrations automatically at startup, so manual update is optional for normal demo use.
