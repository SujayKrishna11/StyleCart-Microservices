# StyleCart Microservices

StyleCart is a .NET 10 e-commerce backend split into User, Product, Order, and API Gateway services. Each business service owns a separate PostgreSQL database and services communicate through REST APIs.

## Capstone 4 AI features

- `GET /gateway/products?search={text}` provides validated, case-insensitive product search across name, brand, and description.
- `GET /gateway/recommendations/for-you` provides order-history recommendations for the authenticated user.
- `GET /gateway/recommendations/variants/{variantId}/also-bought` provides items frequently purchased in the same order.
- `POST /api/support/chat` is a safe local FAQ fallback for shipping, returns, refunds, and order status. It is deliberately isolated in the API Gateway so a Microsoft Foundry implementation can replace it when an Azure subscription is available.

## Run locally with Podman

```powershell
$podmanCompose = "$env:APPDATA\Python\Python314\Scripts\podman-compose.exe"
& $podmanCompose up --build --detach
podman ps
```

The API gateway is available at `http://localhost:5270`.

## Security

Do not commit Azure Foundry keys, connection strings, or `.env` files. Configure production secrets through environment variables or a secret manager.
