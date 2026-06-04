# Product API CURL Reference

Base URL placeholder: `{{baseUrl}}`

## `GET /api/v1/Products`

```bash
curl -X GET "{{baseUrl}}/api/v1/Products" \
  -H "Accept: application/json"
```

## `GET /api/v1/Products/{id}`

```bash
curl -X GET "{{baseUrl}}/api/v1/Products/{{id}}" \
  -H "Accept: application/json"
```

## `POST /api/v1/Products`

```bash
curl -X POST "{{baseUrl}}/api/v1/Products" \
  -H "Content-Type: application/json" \
  -H "Accept: application/json" \
  -d '{
    "name": "Ly nhua 500ml",
    "description": "Ly dùng cho đồ uống lạnh",
    "price": 1500,
    "stockQuantity": 1000,
    "categoryId": 1
  }'
```

Notes:
- `400 Bad Request` if `categoryId` does not exist.
- `201 Created` returns the created product.

## `PUT /api/v1/Products/{id}`

```bash
curl -X PUT "{{baseUrl}}/api/v1/Products/{{id}}" \
  -H "Content-Type: application/json" \
  -H "Accept: application/json" \
  -d '{
    "name": "Ly nhua 500ml",
    "description": "Ly dùng cho đồ uống lạnh",
    "price": 1700,
    "stockQuantity": 1200,
    "categoryId": 1
  }'
```

Notes:
- `404 Not Found` if product does not exist.
- `400 Bad Request` if `categoryId` does not exist.

## `DELETE /api/v1/Products/{id}`

```bash
curl -X DELETE "{{baseUrl}}/api/v1/Products/{{id}}" \
  -H "Accept: application/json"
```

Notes:
- `404 Not Found` if product does not exist.
- `204 No Content` on success.

## `GET /api/v1/products/{productId}/configurations`

```bash
curl -X GET "{{baseUrl}}/api/v1/products/{{productId}}/configurations" \
  -H "Accept: application/json"
```

Notes:
- `404 Not Found` if product does not exist.

## `POST /api/v1/products/{productId}/configurations/materials`

```bash
curl -X POST "{{baseUrl}}/api/v1/products/{{productId}}/configurations/materials" \
  -H "Content-Type: application/json" \
  -H "Accept: application/json" \
  -d '{
    "materialId": 1,
    "extraPrice": 500
  }'
```

Notes:
- `404 Not Found` if product does not exist.
- `400 Bad Request` if material does not exist.

## `POST /api/v1/products/{productId}/configurations/print-options`

```bash
curl -X POST "{{baseUrl}}/api/v1/products/{{productId}}/configurations/print-options" \
  -H "Content-Type: application/json" \
  -H "Accept: application/json" \
  -d '{
    "printTypeId": 1,
    "extraPrice": 1000
  }'
```

Notes:
- `404 Not Found` if product does not exist.
- `400 Bad Request` if print type does not exist.

