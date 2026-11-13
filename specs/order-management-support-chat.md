# Order Management Support Chat Change

## Goal

Expose a customer-support chat endpoint without changing checkout, cart, or order persistence behavior.

## Acceptance criteria

- Empty and oversized messages return a validation error.
- Shipping, return/refund, and order-status questions receive a clear FAQ response.
- The endpoint is isolated in the API Gateway so Microsoft Foundry can be integrated later without changing downstream service contracts.
