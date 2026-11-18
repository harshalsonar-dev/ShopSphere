# 🛍️ ShopSphere – Enterprise E-Commerce Platform

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Clean Architecture](https://img.shields.io/badge/Architecture-Clean%20%2B%20CQRS-blue)](https://github.com/jasontaylordev/CleanArchitecture)
[![Entity Framework Core](https://img.shields.io/badge/ORM-EF%20Core%208-512BD4)](https://learn.microsoft.com/ef/core/)
[![Database](https://img.shields.io/badge/Database-SQL%20Server-CC292B?logo=microsoftsqlserver&logoColor=white)](https://www.microsoft.com/sql-server)
[![Distributed Cache](https://img.shields.io/badge/Cache-Redis-DC382D?logo=redis&logoColor=white)](https://redis.io/)
[![UI](https://img.shields.io/badge/Frontend-Bootstrap%205-7952B3?logo=bootstrap&logoColor=white)](https://getbootstrap.com/)
[![Tests](https://img.shields.io/badge/Tests-14%20Passing-success)](https://xunit.net/)

**ShopSphere** is a high-performance, full-stack E-Commerce platform built from scratch with **.NET 8**, **ASP.NET Core Web API / MVC**, **Entity Framework Core**, **SQL Server**, and **Redis Distributed Caching**. It strictly adheres to **Clean Architecture**, **SOLID principles**, and the **CQRS (Command Query Responsibility Segregation)** pattern with **MediatR**.

---

## 🏛️ Clean Architecture Structure

```
ShopSphere/
├── src/
│   ├── ShopSphere.Domain/              # Enterprise Domain (Entities, Invariants, Repository Contracts)
│   ├── ShopSphere.Application/         # CQRS Commands, Queries, MediatR Behaviors, FluentValidation
│   ├── ShopSphere.Infrastructure/      # EF Core, SQL Server, Redis Cache, JWT, BCrypt, Repositories
│   └── ShopSphere.Web/                 # Web API Controllers, Bootstrap 5 MVC Views, Middleware
└── tests/
    └── ShopSphere.UnitTests/           # Automated Tests (xUnit, Moq, FluentAssertions)
```

---

## ✨ Core Features

1. **User Identity & Role-Based Access Control (RBAC):**
   - Cryptographically salted **BCrypt** password hashing (work factor 11).
   - Secure **JWT Token generation (HMAC-SHA256)** supporting both `Authorization: Bearer <token>` API headers and HTTP-only cookies for MVC sessions.
   - Roles: `Admin` (full product/order management) vs `Customer` (browsing, cart, checkout, order tracking).

2. **Product Catalog & Dynamic Multi-Attribute Filtering:**
   - Real-time product search by keyword.
   - Filtering by **Category** and **Price Range** (Min-Max bounds).
   - Highly optimized database indexes: single indexes on `CategoryId`, `Price`, and composite index on `(CategoryId, Price)`.
   - Paginated catalog with `PagedResult<T>` metadata.

3. **High-Speed Shopping Cart (Redis):**
   - Cart data cached in **Redis** (`ICacheService`) with 7-day sliding expiration.
   - Resilient in-memory distributed cache fallback when running offline or in development.
   - Real-time stock threshold validation during item addition and quantity adjustments.

4. **Atomic Checkout & Inventory Guard:**
   - Atomically executed within an `IUnitOfWork` database transaction.
   - Rich Domain Model stock deduction (`product.DeductStock(quantity)`) preventing negative inventory.
   - Automatic transaction rollback on stock conflict or concurrency issues.
   - Automatic cart invalidation upon successful order creation.

5. **Order Tracking & Fulfillment:**
   - Visual status progression bar: `Pending` $\rightarrow$ `Processing` $\rightarrow$ `Shipped` $\rightarrow$ `Delivered`.
   - Cancellation workflow with automatic product restocking.
   - Admin order portal to manage customer order states.

6. **Admin Dashboard:**
   - Key metrics: Total Catalog Items, Platform Revenue, and Critical Low-Stock Alerts ($< 10$ units).
   - Product management: Create, Edit, Restock, and Delete.

7. **MediatR Pipeline Behaviors:**
   - `ValidationBehavior`: Automatically runs **FluentValidation** rules on every request.
   - `CachingBehavior`: Intercepts `ICacheableQuery` requests, checking Redis before executing database queries.

---

## 🗄️ Database Schema (Normalized Relational Entities)

- **Users:** `Id`, `FullName`, `Email` (Unique Index), `PasswordHash`, `RoleId` (FK to Roles)
- **Roles:** `Id`, `Name` (Admin, Customer)
- **Categories:** `Id`, `Name` (Unique Index)
- **Products:** `Id`, `Name`, `Description`, `Price` (decimal 18,2), `StockQuantity`, `CategoryId` (FK to Categories), `CreatedAt`
  - *Indexes:* `IX_Products_CategoryId`, `IX_Products_Price`, `IX_Products_CategoryId_Price`, `IX_Products_Name`
- **Orders:** `Id`, `UserId` (FK to Users), `TotalAmount` (decimal 18,2), `OrderStatus`, `CreatedAt`
  - *Indexes:* `IX_Orders_UserId`, `IX_Orders_CreatedAt`, `IX_Orders_OrderStatus`
- **OrderItems:** `Id`, `OrderId` (FK to Orders), `ProductId` (FK to Products), `Quantity`, `UnitPrice` (decimal 18,2)

---

## 🚀 Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/sql-server) or LocalDB (default: `(localdb)\mssqllocaldb`)
- *(Optional)* [Redis](https://redis.io/) on port `6379` (In-memory fallback is automatically enabled if Redis is offline)

### Run the Application
```bash
git clone https://github.com/harshalsonar-dev/ShopSphere.git
cd ShopSphere
dotnet run --project src/ShopSphere.Web/ShopSphere.Web.csproj
```

Open your browser:
- **Storefront & Admin Web App:** [http://localhost:5000](http://localhost:5000)
- **Swagger Interactive API Documentation:** [http://localhost:5000/swagger](http://localhost:5000/swagger)

---

## 🔑 Pre-Seeded Demo Credentials

| Role | Email | Password | Permissions |
| :--- | :--- | :--- | :--- |
| **Admin** | `admin@shopsphere.com` | `Admin@123` | Full access to Admin Portal, Product CRUD, and Order Management |
| **Customer** | `customer@shopsphere.com` | `Customer@123` | Product browsing, cart management, checkout, order tracking |

*(Note: The login page includes quick-click auto-fill buttons for both roles.)*

---

## 🧪 Running Unit Tests

Execute the automated test suite covering rich domain models, CQRS handlers, real-time inventory deduction, and cart caching:
```bash
dotnet test tests/ShopSphere.UnitTests/ShopSphere.UnitTests.csproj
```
**Test Result:** `Passed: 14, Failed: 0, Skipped: 0 (100% Pass Rate)`

---

## 📡 REST API Endpoints Overview

| Method | Endpoint | Description | Access |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/auth/register` | Register new account & return JWT | Public |
| `POST` | `/api/v1/auth/login` | Authenticate & return JWT | Public |
| `GET` | `/api/v1/auth/me` | Current authenticated user context | Authorized |
| `GET` | `/api/v1/products` | Multi-attribute search & pagination | Public |
| `GET` | `/api/v1/products/{id}` | Get product details | Public |
| `POST` | `/api/v1/products` | Create new product | Admin |
| `PUT` | `/api/v1/products/{id}` | Update product details | Admin |
| `DELETE` | `/api/v1/products/{id}` | Delete product | Admin |
| `GET` | `/api/v1/categories` | Get cached categories | Public |
| `GET` | `/api/v1/cart` | Get current shopping cart | Public/User |
| `POST` | `/api/v1/cart/items` | Add product to cart | Public/User |
| `PUT` | `/api/v1/cart/items` | Update item quantity | Public/User |
| `DELETE` | `/api/v1/cart/items/{id}` | Remove item from cart | Public/User |
| `POST` | `/api/v1/orders/checkout` | Checkout cart & deduct stock | Authorized |
| `GET` | `/api/v1/orders/my-orders` | Get user order history | Authorized |
| `GET` | `/api/v1/orders/{id}` | Get order details | Authorized |
| `GET` | `/api/v1/orders` | View all customer orders | Admin |
| `PUT` | `/api/v1/orders/{id}/status` | Update fulfillment status | Admin |
| `POST` | `/api/v1/orders/{id}/cancel` | Cancel order & restock inventory | Authorized |

---

## 📄 License
This project is licensed under the [MIT License](LICENSE).
