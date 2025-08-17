# ReshamBazaar API (ASP.NET Core 8 Web API)

E-commerce backend for silk sarees with Clean Architecture-style folders, ASP.NET Core Identity (JWT), EF Core InMemory, Swagger, CORS, Products/Users CRUD, Cart, and Orders.

## Prerequisites
- .NET 8 SDK
- Visual Studio 2022 17.8+ (with ASP.NET and web workload)

## How to Run (VS 2022)
1. Open `ReshamBazaar.Api.csproj` in VS 2022.
2. Set `ReshamBazaar.Api` as Startup project.
3. Run (F5). It opens Swagger at `https://localhost:7255/swagger`.

On first run, seed data includes 8 silk sarees and one demo user:
- Email: `demo@reshambazaar.com`
- Password: `Password1!`

## How to Run (CLI)
```bash
# From project directory
 dotnet restore
 dotnet run
# Swagger will be on the URL printed in the console (e.g., https://localhost:7255/swagger)
```

## CORS
Configured for React and Angular:
- http://localhost:3000
- http://localhost:5173
- http://localhost:4200

Update in `Program.cs` if needed.

## Auth (JWT)
- Register: `POST /api/users/register`
- Login: `POST /api/users/login`
- Copy the `token` from response, click Swagger "Authorize" (top-right), and paste as: `Bearer <token>`

## Endpoints Overview
- Products
  - `GET /api/products` (public)
  - `GET /api/products/{id}` (public)
  - `POST /api/products` (auth)
  - `PUT /api/products/{id}` (auth)
  - `DELETE /api/products/{id}` (auth)
- Users
  - `POST /api/users/register` (public)
  - `POST /api/users/login` (public)
  - `GET /api/users/me` (auth)
  - `PUT /api/users/me` (auth) — body: full name (string)
  - `DELETE /api/users/me` (auth)
- Cart (auth)
  - `GET /api/cart`
  - `POST /api/cart/add` — body: `{ "productId": 1, "quantity": 2 }`
  - `PUT /api/cart/update` — body: `{ "productId": 1, "quantity": 3 }`
  - `DELETE /api/cart/{productId}`
  - `DELETE /api/cart/clear`
- Orders (auth)
  - `POST /api/orders/create-from-cart`
  - `GET /api/orders/my`

## DTOs
Located in `DTOs/` folder: product, auth, cart, and order read/write models.

## InMemory DB
- EF Core InMemory used for rapid development. Replace with real database by switching provider in `Program.cs` and adding migrations.

## Configuration
`appsettings.json` contains JWT settings. Change the demo secret key for production.

## Notes
- Proper HTTP status codes are used: 200/201/204/400/401/404/409.
- Identity passwords relaxed for demo. Harden in production.
