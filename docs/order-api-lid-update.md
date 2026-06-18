# Order API Update — Add Lid Support to Order Items

**Date:** 2026-06-18  
**Version:** v1.1.0  
**Status:** Ready for client integration

---

## Summary

Order items now support an optional `lidId` field, allowing customers to select a specific lid when placing an order. Previously, the `order_items` table had no relationship to the `lids` table, causing errors when attempting to create orders that include lid selections.

---

## Database Changes

### Migration: `AddLidIdToOrderItem`

| Action | Table | Column | Type | Nullable |
|--------|-------|--------|------|----------|
| ADD COLUMN | `order_items` | `LidId` | `integer` | Yes |
| ADD INDEX | `order_items` | `IX_order_items_LidId` | — | — |
| ADD FK | `order_items` → `lids` | `FK_order_items_lids_LidId` | Restrict | — |

> **Note:** The foreign key uses `Restrict` delete behavior — a lid cannot be deleted if it is referenced by any order item.

---

## API Changes

### POST `/api/orders` — Create Order

#### Request Body (Updated)

```json
{
  "customerName": "string",
  "customerPhone": "string",
  "customerEmail": "string | null",
  "note": "string | null",
  "items": [
    {
      "productId": 1,
      "quantity": 2,
      "unitPrice": 50000,
      "materialId": 1,
      "printTypeId": 1,
      "lidId": 3          // <-- NEW (optional)
    }
  ]
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `lidId` | `integer \| null` | No | ID of the lid to include with this product. Must reference a valid record in the `lids` table. Pass `null` or omit if no lid is needed. |

#### Response Body (Updated)

All endpoints that return order details now include `lidId` and `lidName` in each order item:

```json
{
  "id": 1,
  "customerName": "Nguyen Van A",
  "customerPhone": "0901234567",
  "customerEmail": null,
  "note": null,
  "totalAmount": 100000,
  "status": "pendingconfirmation",
  "createdAtUtc": "2026-06-18T01:00:00Z",
  "items": [
    {
      "productId": 1,
      "productName": "Ly giấy 12oz",
      "materialId": 1,
      "materialName": null,
      "printTypeId": 1,
      "printTypeName": null,
      "lidId": 3,              // <-- NEW
      "lidName": "Nắp phẳng",  // <-- NEW
      "quantity": 2,
      "unitPrice": 50000
    }
  ]
}
```

| New Field | Type | Description |
|-----------|------|-------------|
| `lidId` | `integer \| null` | The lid ID associated with this order item, or `null` if none. |
| `lidName` | `string \| null` | Display name of the lid, or `null` if no lid is selected. |

### Affected Endpoints

| Method | Endpoint | Change |
|--------|----------|--------|
| `POST` | `/api/orders` | Request accepts `lidId`; response includes `lidId` + `lidName` |
| `GET` | `/api/orders/{id}/track?phone={phone}` | Response includes `lidId` + `lidName` |
| `GET` | `/api/orders` | No change (summary only, no item details) |
| `PUT` | `/api/orders/{id}/status` | Response includes `lidId` + `lidName` |

---

## Migration Command

Run the following command on the server to apply the database migration:

```bash
dotnet ef database update
```

---

## Breaking Changes

**Response schema change:** The `items` array in order detail responses now includes two new fields (`lidId`, `lidName`) between `printTypeName` and `quantity`. Clients that destructure the response by field name will not be affected. Clients that rely on positional parsing may need to update.

**No breaking change on request:** The `lidId` field is optional and defaults to `null` when omitted. Existing client code that does not send `lidId` will continue to work without modification.
