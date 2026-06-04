# Product Configurations API CURL Reference

Base URL placeholder: `{{baseUrl}}`

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
