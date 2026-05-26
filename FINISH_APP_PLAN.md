# Finish `feature/finish-app`

## Summary

Make the app demo-ready while closing the open GitHub issues:

- [#6 Configure AppDbContext and Run Migrations](https://github.com/thqhuong/quanlybanhang-nmcnpm/issues/6)
- [#7 Write Seed Data Script](https://github.com/thqhuong/quanlybanhang-nmcnpm/issues/7)
- [#9 Database - EF migrations, seed data and secure configuration](https://github.com/thqhuong/quanlybanhang-nmcnpm/issues/9)
- [#10 Logic - core domain CRUD, validation, and MVVM refactor](https://github.com/thqhuong/quanlybanhang-nmcnpm/issues/10)

The branch should focus on working database-backed Products, Customers, Orders/Sales, seed data, validation, basic MVVM separation, tests, and setup docs. Avoid a full rewrite beyond what is needed for a clean final demo.

## Key Changes

- Upgrade project packages to `10.0.8` where applicable: EF Core, EF Core SQL Server, EF Core Tools, DependencyInjection. Remove `System.Configuration.ConfigurationManager` if connection config no longer needs it.
- Add environment-based DB configuration:
  - Read `QLBH_CONNECTION_STRING` first.
  - Fall back to a safe local SQL Server/LocalDB trusted connection.
  - Do not document or commit SQL passwords in `App.config`.
- Complete EF setup:
  - Keep `ApplicationDbContext`, add missing indexes/constraints for unique product/customer/account fields.
  - Add a new migration for any schema changes.
  - Run migration automatically on startup, then seed if tables are empty.
- Add seed data:
  - Roles: Admin, Cashier, Storekeeper.
  - Users/accounts for each role.
  - Demo categories, products, customers, and at least one sample order.
- Add service layer:
  - `IProductService`, `ICustomerService`, `IOrderService`, `IInventoryService`, `IAccountService`.
  - Services handle CRUD, validation, stock changes, order totals, and database persistence.
- Add practical MVVM:
  - Create view models for Dashboard, Products, Customers, Sales, Import, Accounts, and Overview.
  - Code-behind should only initialize views, wire `DataContext`, and handle unavoidable window navigation.
  - Replace hard-coded `ObservableCollection` sample data in view code-behind with service-backed view models.
- Finish core workflows:
  - Products: list, search, add, edit, delete, reload, validate required fields and unique code/name.
  - Customers: list, search, add, edit, delete, validate phone/email and unique phone.
  - Sales/Orders: add product to cart, update quantity, compute subtotal/discount/VAT/total, create order, reduce stock.
  - Import: add receipt lines, save receipt, increase product stock.
  - Accounts: show seeded users and roles; basic add/edit/lock-delete behavior is enough for demo.
- Fix visible text quality:
  - Correct mojibake Vietnamese strings in code/docs.
  - Update `README.md` and `DATABASE_SETUP.md` with setup, migration, seeding, env var, and demo account instructions.

## Interfaces And Types

- Add input/result DTOs for service calls, for example `ProductInput`, `CustomerInput`, `CreateOrderInput`, `OrderLineInput`, and a reusable `ValidationResult`.
- Extend models only where the UI/workflows need real fields:
  - Product: display code, unit, sale price, stock, category/supplier relation.
  - Customer: phone/email uniqueness and optional points if used by UI.
  - User: username, role, active status, optional last-login timestamp.
- Use async service methods consistently, for example `GetAllAsync`, `SearchAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync`.

## Test Plan

- Add `quanlybanhang-nmcnpm.Tests` with xUnit, EF Core InMemory `10.0.8`, and current .NET test SDK packages.
- Unit test:
  - Product validation, duplicate prevention, create/update/delete.
  - Customer validation and duplicate phone behavior.
  - Order creation totals, VAT/discount handling, and stock reduction.
  - Import receipt creation and stock increase.
  - Seed data is idempotent.
- Verification commands:
  - `dotnet restore`
  - `dotnet build`
  - `dotnet test`
  - `dotnet list package --vulnerable --include-transitive`
- Manual acceptance:
  - Fresh database starts successfully.
  - Seeded data appears in all main screens.
  - Product/customer/order/import workflows persist after app restart.
  - Open issue requirements are satisfied, then the issues can be closed.

## Assumptions

- Target finish level is demo-ready, not a full architecture rewrite.
- Database policy is `QLBH_CONNECTION_STRING` plus a safe local fallback.
- Keep branch name as the current `feature/finish-app`.
- Printing/invoice export can remain a placeholder unless required later; the core sale must save correctly.
