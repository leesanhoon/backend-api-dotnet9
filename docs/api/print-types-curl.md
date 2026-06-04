# Print Types API CURL Reference

Base URL placeholder: `{{baseUrl}}`

## `GET /api/v1/PrintTypes`

```bash
curl -X GET "{{baseUrl}}/api/v1/PrintTypes" \
  -H "Accept: application/json"
```

## `POST /api/v1/PrintTypes`

```bash
curl -X POST "{{baseUrl}}/api/v1/PrintTypes" \
  -H "Content-Type: application/json" \
  -H "Accept: application/json" \
  -d '{
    "name": "In 1 mau",
    "colorCount": 1,
    "description": "Kieu in co 1 mau"
  }'
```

Notes:
- `201 Created` returns the created print type.
