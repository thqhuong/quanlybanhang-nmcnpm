# Quan Ly Ban Hang

Ung dung WPF demo cho quan ly ban hang: hang hoa, khach hang, ban hang tai quay, nhap kho, tai khoan va tong quan he thong.

## Yeu cau

- Windows
- .NET SDK 10
- SQL Server

## Chay nhanh

1. Khoi phuc package:

   ```powershell
   dotnet restore
   ```

2. Mac dinh ung dung dung SQL Server default instance tren may local:

   ```text
   Server=localhost;Database=QuanLyBanHang;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;
   ```

3. Neu dung instance khac, vi du SQLEXPRESS, dat bien moi truong:

   ```powershell
   $env:QLBH_CONNECTION_STRING="Server=localhost\SQLEXPRESS;Database=QuanLyBanHang;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;"
   ```

4. Build va chay:

   ```powershell
   dotnet build
   dotnet run --project .\quanlybanhang-nmcnpm.csproj
   ```

Lan dau khoi dong, ung dung tu dong chay EF migrations va seed du lieu demo neu database dang trong.

## Tai khoan demo

Man hinh dau tien cho phep chon vai tro de vao dashboard:

- `Admin`: quan tri vien
- `Cashier`: thu ngan
- `Storekeeper`: thu kho

Seed data tao cac tai khoan noi bo tuong ung: `admin`, `cashier`, `storekeeper`.

## Kiem thu

```powershell
dotnet test
dotnet list package --vulnerable --include-transitive
```

Test hien tai bao phu validation, CRUD co ban, seed data idempotent, tao don hang tru ton kho, va tao phieu nhap tang ton kho.
