# QuickBooks Online Mapping Schema (MVP)

- Items ⇄ Inventory Lots/Products (SKU, UoM)
- Vendors/Customers ⇄ Partners (IDs)
- Accounts ⇄ GL mapping for WIP/FG/COGS
- Location/Class ⇄ Site/Room (where applicable)
- Bills/POs/Invoices/Payments ⇄ Item-level flows
- GL Summary ⇄ Periodic JE for WIP→FG & COGS
- Idempotency: Request-ID semantics; reconciliation report per period
