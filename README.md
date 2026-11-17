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

## Customer web app

The React customer UI is in `stylecart-web`. During development, run it through
Vite after starting the microservices:

```powershell
cd stylecart-web
npm install
npm run dev
```

Open `http://localhost:5173`. The Vite proxy routes browser calls through the
API gateway. The same UI can be packaged with Podman using its `Containerfile`;
the `stylecart-web` service exposes port `5173` when run from `compose.yaml`.

The customer journey includes product search, customer registration and login,
variant selection, cart quantity management, checkout, order history, and the
local support FAQ. Checkout uses the current Order Service mock-payment flow;
it is not a real payment-provider integration.

## Verification

```powershell
dotnet test StyleCart.Microservices.sln --no-restore
cd stylecart-web
npm run build
```

The current automated suite contains API Gateway controller tests. Add service-
level and end-to-end tests before treating this as production-ready.

## Deliberately deferred

- Microsoft Foundry agent, RAG knowledge base, safety configuration, and
  telemetry need an Azure subscription and the appropriate organization roles.
- A dedicated Payment Service and real payment-provider integration are not
  implemented; checkout currently records a mock payment outcome in the Order
  Service.
- Kubernetes manifests and deployment are deferred by the team decision to use
  Podman rather than Docker/Kubernetes in the available environment.

## Security

Do not commit Azure Foundry keys, connection strings, or `.env` files. Configure production secrets through environment variables or a secret manager.
