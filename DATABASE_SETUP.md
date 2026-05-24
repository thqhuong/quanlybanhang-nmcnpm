# Database Setup Guide

## Project Structure

The project is organized with the following folder structure:

```
quanlybanhang-nmcnpm/
├── Models/                      # Entity classes
│   ├── User.cs
│   ├── Role.cs
│   ├── Product.cs
│   ├── Category.cs
│   ├── Customer.cs
│   ├── Order.cs
│   ├── OrderDetail.cs
│   ├── InventoryReceipt.cs
│   └── InventoryReceiptDetail.cs
├── Database/                    # Database context and configuration
│   ├── ApplicationDbContext.cs
│   └── DatabaseConfiguration.cs
├── Migrations/                  # EF Core migration files
├── Views/                       # UI Views
└── App.config                   # Connection string configuration
```

## Entity Relationships

### Core Relationships:
- **User** → **Role** (Many-to-One): Each employee has one role
- **Product** → **Category** (Many-to-One): Multiple products belong to a category/supplier
- **Customer** → **Order** (One-to-Many): Each customer can have multiple orders
- **Order** → **OrderDetail** (One-to-Many): Each order contains multiple order details
- **OrderDetail** → **Product** (Many-to-One): Order details reference products
- **InventoryReceipt** → **InventoryReceiptDetail** (One-to-Many): Each receipt has multiple details
- **InventoryReceiptDetail** → **Product** (Many-to-One): Receipt details reference products

## Database Configuration

### Connection String (Environment Variable)
Set the `QLBH_CONNECTION_STRING` environment variable. The app reads this value at startup.

**PowerShell (Windows):**
```powershell
$env:QLBH_CONNECTION_STRING="Server=localhost;Database=QuanLyBanHang;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;"
```

**Command Prompt (Windows):**
```cmd
set QLBH_CONNECTION_STRING=Server=localhost;Database=QuanLyBanHang;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;
```

**SQL Server authentication example:**
```powershell
$env:QLBH_CONNECTION_STRING="Server=your_server;Database=QuanLyBanHang;User Id=sa;******;Encrypt=True;TrustServerCertificate=True;"
```

`App.config` keeps the `DefaultConnection` entry empty so secrets are not stored in source control. If you prefer to use a local config file, you can set the value there instead of using an environment variable.


## Creating and Updating the Database

### First Time Setup
Migration files are already included in the `Migrations/` folder. Apply them to create the schema:
1. **Update Database (Package Manager Console):**
   ```powershell
   Update-Database
   ```

2. **Update Database (dotnet-ef):**
   ```bash
   dotnet ef database update
   ```


### Adding Changes to Database Schema

1. **Add New Migration:**
   ```powershell
   Add-Migration [MigrationName]
   ```
   Example: `Add-Migration AddUserRoleColumn`

2. **Update Database:**
   ```powershell
   Update-Database
   ```

3. **Rollback (if needed):**
   ```powershell
   Update-Database -Migration [PreviousMigrationName]
   ```

### Seed Data
Seed data for demo/development is configured in `ApplicationDbContext` and applied through migrations. After running `Update-Database`, the database includes:
- Roles (Admin, NhanVien)
- A sample employee user
- Categories, products, and customers
- A sample order and inventory receipt with details

If you want to re-seed, drop the database and run `Update-Database` again.

## Entity Classes

### User (NhanVien)
- **MaNhanVien** (int, PK): Employee ID
- **TenNV** (string): Employee name
- **NgaySinh** (DateTime): Date of birth
- **NgayDangKy** (DateTime): Registration date
- **MaKH** (int): Customer ID
- **TenKH** (string): Customer name
- **DiaChiKH** (string): Address
- **SoDienThoai** (string): Phone number
- **Email** (string): Email address
- **MaVaiTro** (int, FK): Role reference

### Role (VaiTro)
- **MaVaiTro** (int, PK): Role ID
- **TenVaiTro** (string): Role name

### Product (Hang)
- **MaHang** (int, PK): Product ID
- **TenHang** (string): Product name
- **SoLuongTon** (int): Stock quantity
- **MaNhaCungCap** (int, FK): Supplier/Category reference
- **MaLoai** (string): Category code

### Category (NhaCungCap)
- **MaNhaCungCap** (int, PK): Supplier ID
- **TenNCC** (string): Supplier name

### Customer (KhachHang)
- **MaKH** (int, PK): Customer ID
- **TenKH** (string): Customer name
- **NgaySinh** (DateTime): Date of birth
- **NgayDangKy** (DateTime): Registration date
- **DiaChiKH** (string): Address
- **SoDienThoai** (string): Phone number
- **Email** (string): Email address

### Order (HoaDon)
- **MaDonHang** (int, PK): Order ID
- **MaKH** (int, FK): Customer reference
- **MaNhanVien** (int): Employee ID
- **NgayLap** (DateTime): Order date
- **TongTien** (decimal): Total amount

### OrderDetail (ChiTietHoaDon)
- **MaDonHang** (int, PK, FK): Order reference
- **MaHang** (int, PK, FK): Product reference
- **SoLuong** (int): Quantity
- **DonGiaBan** (int): Unit price
- **ThanhTien** (int): Line total

### InventoryReceipt (PhieuNhapKho)
- **MaPhieuNhapKho** (int, PK): Receipt ID
- **NgayNhapKho** (DateTime): Receipt date
- **TongTien** (int): Total amount
- **MaNhanVien** (int): Employee ID
- **MaNhaCungCap** (int): Supplier reference

### InventoryReceiptDetail (ChiTietPhieuNhapKho)
- **MaPhieuNhapKho** (int, PK, FK): Receipt reference
- **MaHang** (int, PK, FK): Product reference
- **SoLuongNhap** (int): Quantity received
- **DonGiaNhap** (int): Unit price
- **ThanhTien** (int): Line total

## Notes

- All foreign key deletions are set to **Restrict** to prevent accidental data loss
- Exception: Order and InventoryReceipt details use **Cascade** deletion for consistency
- The database is automatically initialized when the application starts
- Migrations are applied automatically on startup
