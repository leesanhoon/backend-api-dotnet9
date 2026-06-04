# Materials API CURL Reference

Base URL placeholder: `{{baseUrl}}`

## `GET /api/v1/Materials`

```bash
curl -X GET "{{baseUrl}}/api/v1/Materials" \
  -H "Accept: application/json"
```

## `POST /api/v1/Materials`

```bash
curl -X POST "{{baseUrl}}/api/v1/Materials" \
  -H "Content-Type: application/json" \
  -H "Accept: application/json" \
  -d '{
    "name": "PP",
    "description": "Vat lieu PP"
  }'
```

Notes:
- `201 Created` returns the created material.
