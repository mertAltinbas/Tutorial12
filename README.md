## Add Client to Trip

**Endpoint:**  
`POST http://localhost:5000/api/trips/{tripId}/clients`

**Example:**  
`POST http://localhost:5000/api/trips/2/clients`

**Example Body:**

```json
{
  "firstName": "Bober",
  "lastName": "Bober",
  "pesel": "12345678902",
  "email": "bober.bober@example.com",
  "telephone": "+48123456789",
  "paymentDate": "2024-07-01T00:00:00"
}
