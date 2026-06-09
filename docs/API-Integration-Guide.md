# API Integration Guide

**Base URL:** `https://{host}/api/v1`

All endpoints use JSON unless noted otherwise. Responses follow standard HTTP status codes.

---

## Table of Contents

1. [Pagination](#pagination)
2. [Categories](#categories)
3. [Products](#products)
4. [Orders](#orders)
5. [Materials](#materials)
6. [Print Types](#print-types)
7. [Error Handling](#error-handling)

---

## Pagination

All list endpoints support pagination via query parameters:

| Parameter  | Type | Default | Description          |
|------------|------|---------|----------------------|
| `page`     | int  | 1       | Page number (1-based)|
| `pageSize` | int  | 10      | Items per page       |

**Response envelope:**

```json
{
  "items": [ ... ],
  "totalCount": 42,
  "page": 1,
  "pageSize": 10
}
```

---

## Categories

### List categories

```
GET /api/v1/categories?page=1&pageSize=10
```

**Response:** `200 OK`

```json
{
  "items": [
    {
      "id": 1,
      "name": "T-Shirts",
      "description": "All types of t-shirts"
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 10
}
```

### Get category by ID

```
GET /api/v1/categories/{id}
```

**Response:** `200 OK`

```json
{
  "id": 1,
  "name": "T-Shirts",
  "description": "All types of t-shirts"
}
```

**Error:** `404 Not Found` if category does not exist.

### Create category

```
POST /api/v1/categories
Content-Type: application/json
```

**Request body:**

```json
{
  "name": "T-Shirts",
  "description": "All types of t-shirts"
}
```

| Field         | Type    | Required | Description            |
|---------------|---------|----------|------------------------|
| `name`        | string  | Yes      | Category name (unique) |
| `description` | string  | No       | Category description   |

**Response:** `201 Created`

### Update category

```
PUT /api/v1/categories/{id}
Content-Type: application/json
```

**Request body:**

```json
{
  "name": "Updated Name",
  "description": "Updated description"
}
```

**Response:** `200 OK` with updated category.

**Error:** `404 Not Found` if category does not exist.

### Delete category

```
DELETE /api/v1/categories/{id}
```

**Response:** `204 No Content`

**Errors:**
- `404 Not Found` — category does not exist
- `400 Bad Request` — category has linked products (must delete products first)

---

## Products

### List products

```
GET /api/v1/products?page=1&pageSize=10
```

**Response:** `200 OK`

```json
{
  "items": [
    {
      "id": 1,
      "name": "Basic White Tee",
      "description": "A plain white t-shirt",
      "price": 15.99,
      "stockQuantity": 100,
      "categoryId": 1,
      "categoryName": "T-Shirts",
      "avatarImageUrl": "https://res.cloudinary.com/.../avatar.webp",
      "galleryImages": [
        {
          "id": 10,
          "imageUrl": "https://res.cloudinary.com/.../img1.webp",
          "imageType": "gallery",
          "displayOrder": 1,
          "createdAtUtc": "2026-06-01T12:00:00Z"
        }
      ]
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 10
}
```

### Get product by ID

```
GET /api/v1/products/{id}
```

**Response:** `200 OK` — same shape as a single item above.

**Error:** `404 Not Found`

### Create product

```
POST /api/v1/products
Content-Type: multipart/form-data
```

**Form fields:**

| Field            | Type     | Required | Description                  |
|------------------|----------|----------|------------------------------|
| `name`           | string   | Yes      | Product name                 |
| `description`    | string   | No       | Product description          |
| `price`          | decimal  | Yes      | Unit price                   |
| `stockQuantity`  | int      | Yes      | Available stock              |
| `categoryId`     | int      | Yes      | ID of existing category      |
| `avatarImage`    | file     | No       | Main product image           |
| `galleryImages`  | file[]   | No       | Additional gallery images    |

**Example (JavaScript `FormData`):**

```javascript
const form = new FormData();
form.append("name", "Basic White Tee");
form.append("description", "A plain white t-shirt");
form.append("price", "15.99");
form.append("stockQuantity", "100");
form.append("categoryId", "1");
form.append("avatarImage", avatarFile);
form.append("galleryImages", galleryFile1);
form.append("galleryImages", galleryFile2);

const res = await fetch("/api/v1/products", {
  method: "POST",
  body: form,
});
```

**Response:** `201 Created`

**Errors:**
- `400 Bad Request` — invalid `categoryId` or image processing error

### Update product

```
PUT /api/v1/products/{id}
Content-Type: application/json
```

**Request body:**

```json
{
  "name": "Updated Tee",
  "description": "Updated description",
  "price": 19.99,
  "stockQuantity": 50,
  "categoryId": 1
}
```

**Response:** `200 OK` with updated product.

**Errors:**
- `404 Not Found` — product does not exist
- `400 Bad Request` — invalid `categoryId`

### Delete product

```
DELETE /api/v1/products/{id}
```

**Response:** `204 No Content`

**Error:** `404 Not Found`

---

## Orders

### List orders

```
GET /api/v1/orders?page=1&pageSize=10
```

**Response:** `200 OK`

```json
{
  "items": [
    {
      "id": 1,
      "customerName": "Nguyen Van A",
      "totalAmount": 159.90,
      "status": "draft",
      "createdAtUtc": "2026-06-09T10:00:00Z"
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 10
}
```

### Get order by ID

```
GET /api/v1/orders/{id}
```

**Response:** `200 OK`

```json
{
  "id": 1,
  "customerName": "Nguyen Van A",
  "customerPhone": "0901234567",
  "customerEmail": "a@example.com",
  "note": "Please deliver before 5pm",
  "totalAmount": 159.90,
  "status": "draft",
  "createdAtUtc": "2026-06-09T10:00:00Z",
  "items": [
    {
      "productId": 1,
      "productName": "Basic White Tee",
      "quantity": 10,
      "unitPrice": 15.99
    }
  ]
}
```

**Error:** `404 Not Found`

### Create order

```
POST /api/v1/orders
Content-Type: application/json
```

**Request body:**

```json
{
  "customerName": "Nguyen Van A",
  "customerPhone": "0901234567",
  "customerEmail": "a@example.com",
  "note": "Please deliver before 5pm",
  "items": [
    {
      "productId": 1,
      "quantity": 10,
      "unitPrice": 15.99
    }
  ]
}
```

| Field            | Type   | Required | Description               |
|------------------|--------|----------|---------------------------|
| `customerName`   | string | Yes      | Customer full name        |
| `customerPhone`  | string | No       | Phone number              |
| `customerEmail`  | string | No       | Email address             |
| `note`           | string | No       | Order notes               |
| `items`          | array  | Yes      | At least 1 item required  |
| `items[].productId`  | int     | Yes | Existing product ID  |
| `items[].quantity`    | int     | Yes | Quantity ordered     |
| `items[].unitPrice`   | decimal | Yes | Price per unit      |

**Response:** `201 Created`

**Errors:**
- `400 Bad Request` — empty items list, or invalid `productId`

### Update order status

```
PUT /api/v1/orders/{id}/status
Content-Type: application/json
```

**Request body:**

```json
{
  "status": "confirmed"
}
```

**Valid status transitions:**

```
draft ──→ confirmed ──→ shipping ──→ completed
  │
  └──→ cancelled
```

| From        | Allowed transitions         |
|-------------|-----------------------------|
| `draft`     | `confirmed`, `cancelled`    |
| `confirmed` | `shipping`                  |
| `shipping`  | `completed`                 |
| `completed` | _(terminal — no transitions)_ |
| `cancelled` | _(terminal — no transitions)_ |

**Response:** `200 OK`

```json
{
  "status": "confirmed"
}
```

**Errors:**
- `404 Not Found` — order does not exist
- `400 Bad Request` — invalid status transition

### Delete order

```
DELETE /api/v1/orders/{id}
```

Only orders with status `draft` can be deleted.

**Response:** `204 No Content`

**Errors:**
- `404 Not Found` — order does not exist
- `400 Bad Request` — order is not in `draft` status

---

## Materials

### List materials

```
GET /api/v1/materials?page=1&pageSize=10
```

**Response:** `200 OK`

```json
{
  "items": [
    {
      "id": 1,
      "name": "Cotton",
      "description": "100% organic cotton"
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 10
}
```

### Create material

```
POST /api/v1/materials
Content-Type: application/json
```

**Request body:**

```json
{
  "name": "Cotton",
  "description": "100% organic cotton"
}
```

| Field         | Type   | Required | Description             |
|---------------|--------|----------|-------------------------|
| `name`        | string | Yes      | Material name (unique)  |
| `description` | string | No       | Material description    |

**Response:** `201 Created`

---

## Print Types

### List print types

```
GET /api/v1/printtypes?page=1&pageSize=10
```

**Response:** `200 OK`

```json
{
  "items": [
    {
      "id": 1,
      "name": "Screen Print",
      "colorCount": 4,
      "description": "Traditional screen printing"
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 10
}
```

### Create print type

```
POST /api/v1/printtypes
Content-Type: application/json
```

**Request body:**

```json
{
  "name": "Screen Print",
  "colorCount": 4,
  "description": "Traditional screen printing"
}
```

| Field         | Type   | Required | Description                |
|---------------|--------|----------|----------------------------|
| `name`        | string | Yes      | Print type name (unique)   |
| `colorCount`  | int    | Yes      | Number of colors supported |
| `description` | string | No       | Print type description     |

**Response:** `201 Created`

---

## Error Handling

All error responses use standard HTTP status codes:

| Status | Meaning                                            |
|--------|----------------------------------------------------|
| `200`  | Success                                            |
| `201`  | Created — resource was successfully created        |
| `204`  | No Content — resource was successfully deleted     |
| `400`  | Bad Request — validation error (check response body for message) |
| `404`  | Not Found — resource does not exist                |

Error response body format:

```json
"Error message string"
```

Or for validation errors:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Error message"
}
```
