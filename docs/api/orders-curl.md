# Orders API CURL Reference

Base URL placeholder: `{{baseUrl}}`

## `GET /api/v1/Orders`

```bash
curl -X GET "{{baseUrl}}/api/v1/Orders" \
  -H "Accept: application/json"
```

## `GET /api/v1/Orders/{id}`

```bash
curl -X GET "{{baseUrl}}/api/v1/Orders/{{id}}" \
  -H "Accept: application/json"
```

## `POST /api/v1/Orders`

```bash
curl -X POST "{{baseUrl}}/api/v1/Orders" \
  -H "Content-Type: application/json" \
  -H "Accept: application/json" \
  -d '{
    "customerName": "Nguyen Van A",
    "customerPhone": "0900000000",
    "customerEmail": "a@example.com",
    "note": "Don hang mau",
    "items": [
      {
        "productId": 1,
        "materialId": 1,
        "printTypeId": 1,
        "quantity": 100,
        "unitPrice": 1500
      }
    ]
  }'
```

Notes:
- `400 Bad Request` if request validation/business rules fail.
- `201 Created` returns the created order.
