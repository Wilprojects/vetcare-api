# VetCare — Contrato preliminar de la API

> **Estado:** Aprobado para la versión 1  
> **Versión del documento:** 1.0  
> **Ruta base:** `/api/v1`

## 1. Propósito

Este documento define el contrato HTTP previsto para VetCare v1: convenciones, rutas, seguridad, parámetros, cuerpos, respuestas y errores. La especificación OpenAPI implementada deberá mantenerse alineada con este documento.

## 2. Convenciones generales

| Aspecto | Convención |
|---|---|
| Protocolo de producción | HTTPS |
| Formato principal | JSON |
| Content-Type de peticiones | `application/json` |
| Nombres JSON | `camelCase` |
| Enumeraciones | Texto |
| Identificadores | UUID / `Guid` |
| Fechas y horas | ISO 8601 en UTC |
| Fechas sin hora | `YYYY-MM-DD` |
| Autenticación | Bearer JWT |
| Errores | `application/problem+json` |
| Página inicial | 1 |
| Tamaño predeterminado | 10 |
| Tamaño máximo | 100 |

## 3. Autenticación Bearer

Los endpoints protegidos esperan:

```http
Authorization: Bearer <access-token>
```

Un token ausente, inválido o expirado produce `401 Unauthorized`. Un usuario autenticado sin el rol requerido produce `403 Forbidden`.

## 4. Respuesta paginada común

```json
{
  "items": [],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 25,
  "totalPages": 3,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

## 5. Endpoints de autenticación

| Método | Ruta | Acceso | Resultado principal |
|---|---|---|---|
| `POST` | `/api/v1/auth/register` | Público | `201 Created` |
| `POST` | `/api/v1/auth/login` | Público | `200 OK` |
| `GET` | `/api/v1/auth/me` | Autenticado | `200 OK` |

### 5.1 Registrar cliente

```http
POST /api/v1/auth/register
Content-Type: application/json
```

#### Petición

```json
{
  "firstName": "Wilder",
  "lastName": "Moreno",
  "email": "wilder@example.com",
  "phoneNumber": "999999999",
  "password": "Password123!"
}
```

#### Respuesta `201 Created`

```json
{
  "id": "11111111-1111-1111-1111-111111111111",
  "firstName": "Wilder",
  "lastName": "Moreno",
  "email": "wilder@example.com",
  "phoneNumber": "999999999",
  "roles": ["Customer"],
  "createdAtUtc": "2026-09-13T20:00:00Z"
}
```

#### Errores previstos

- `400 Bad Request`: validación de entrada o contraseña inválida.
- `409 Conflict`: correo ya registrado.

### 5.2 Iniciar sesión

```http
POST /api/v1/auth/login
Content-Type: application/json
```

#### Petición

```json
{
  "email": "wilder@example.com",
  "password": "Password123!"
}
```

#### Respuesta `200 OK`

```json
{
  "accessToken": "eyJhbGciOi...",
  "tokenType": "Bearer",
  "expiresAtUtc": "2026-09-13T21:00:00Z",
  "user": {
    "id": "11111111-1111-1111-1111-111111111111",
    "firstName": "Wilder",
    "lastName": "Moreno",
    "email": "wilder@example.com",
    "roles": ["Customer"]
  }
}
```

#### Error previsto

- `401 Unauthorized`: credenciales inválidas.

### 5.3 Consultar usuario actual

```http
GET /api/v1/auth/me
Authorization: Bearer <token>
```

Devuelve `200 OK` con los datos públicos del usuario autenticado.

## 6. Endpoints de mascotas

| Método | Ruta | Acceso | Resultado principal |
|---|---|---|---|
| `GET` | `/api/v1/pets` | Customer | `200 OK` |
| `GET` | `/api/v1/pets/{id}` | Propietario | `200 OK` |
| `POST` | `/api/v1/pets` | Customer | `201 Created` |
| `PUT` | `/api/v1/pets/{id}` | Propietario | `200 OK` |
| `DELETE` | `/api/v1/pets/{id}` | Propietario | `204 No Content` |

Este recurso cumple el CRUD completo requerido. `DELETE` realiza una desactivación lógica.

### 6.1 Listar mascotas

```http
GET /api/v1/pets?search=luna&species=Dog&isActive=true&sortBy=name&sortDirection=asc&pageNumber=1&pageSize=10
Authorization: Bearer <token>
```

#### Parámetros

| Parámetro | Tipo | Predeterminado | Descripción |
|---|---|---|---|
| `search` | string | — | Busca por nombre o raza. |
| `species` | enum | — | Filtra por especie. |
| `isActive` | bool | `true` | Incluye activas o inactivas propias. |
| `sortBy` | string | `name` | `name`, `birthDate`, `createdAt`. |
| `sortDirection` | string | `asc` | `asc` o `desc`. |
| `pageNumber` | int | 1 | Mínimo 1. |
| `pageSize` | int | 10 | Entre 1 y 100. |

### 6.2 Consultar mascota

```http
GET /api/v1/pets/{id}
Authorization: Bearer <token>
```

#### Respuesta `200 OK`

```json
{
  "id": "22222222-2222-2222-2222-222222222222",
  "name": "Luna",
  "species": "Dog",
  "breed": "Labrador",
  "sex": "Female",
  "birthDate": "2022-05-15",
  "weightKg": 18.50,
  "isActive": true,
  "createdAtUtc": "2026-09-13T20:10:00Z",
  "updatedAtUtc": null
}
```

Una mascota inexistente o ajena devuelve `404 Not Found`.

### 6.3 Crear mascota

```http
POST /api/v1/pets
Authorization: Bearer <token>
Content-Type: application/json
```

#### Petición

```json
{
  "name": "Luna",
  "species": "Dog",
  "breed": "Labrador",
  "sex": "Female",
  "birthDate": "2022-05-15",
  "weightKg": 18.50
}
```

El cuerpo no acepta `ownerId`, `isActive`, `createdAtUtc` ni `updatedAtUtc`.

#### Respuesta

- `201 Created`.
- Header `Location: /api/v1/pets/{id}`.
- Cuerpo con `PetResponse`.

### 6.4 Actualizar mascota

```http
PUT /api/v1/pets/{id}
Authorization: Bearer <token>
Content-Type: application/json
```

```json
{
  "name": "Luna",
  "species": "Dog",
  "breed": "Labrador Retriever",
  "sex": "Female",
  "birthDate": "2022-05-15",
  "weightKg": 19.20
}
```

Respuesta: `200 OK` con el recurso actualizado.

### 6.5 Desactivar mascota

```http
DELETE /api/v1/pets/{id}
Authorization: Bearer <token>
```

Respuesta: `204 No Content`.

Errores relevantes:

- `404 Not Found`: inexistente o ajena.
- `409 Conflict`: tiene citas futuras en estado `Pending` o `Confirmed`.

## 7. Endpoints de servicios veterinarios

| Método | Ruta | Acceso | Resultado principal |
|---|---|---|---|
| `GET` | `/api/v1/veterinary-services` | Público | `200 OK` |
| `GET` | `/api/v1/veterinary-services/{id}` | Público | `200 OK` |
| `POST` | `/api/v1/veterinary-services` | Admin | `201 Created` |
| `PUT` | `/api/v1/veterinary-services/{id}` | Admin | `200 OK` |
| `DELETE` | `/api/v1/veterinary-services/{id}` | Admin | `204 No Content` |
| `GET` | `/api/v1/admin/veterinary-services` | Admin | `200 OK` |

Este recurso también cumple el CRUD completo requerido. `DELETE` realiza una desactivación lógica.

### 7.1 Listar servicios públicos

```http
GET /api/v1/veterinary-services?search=consulta&minPrice=20&maxPrice=150&sortBy=name&sortDirection=asc&pageNumber=1&pageSize=10
```

Solo devuelve servicios activos.

#### Parámetros

| Parámetro | Tipo | Campos / rango |
|---|---|---|
| `search` | string | Nombre o descripción. |
| `minPrice` | decimal | Mayor o igual que 0. |
| `maxPrice` | decimal | Mayor o igual que `minPrice`. |
| `sortBy` | string | `name`, `price`, `duration`, `createdAt`. |
| `sortDirection` | string | `asc`, `desc`. |
| `pageNumber` | int | Mínimo 1. |
| `pageSize` | int | 1 a 100. |

### 7.2 Respuesta de servicio

```json
{
  "id": "33333333-3333-3333-3333-333333333333",
  "name": "Consulta general",
  "description": "Evaluación veterinaria general de la mascota.",
  "durationMinutes": 30,
  "price": 75.00,
  "isActive": true,
  "createdAtUtc": "2026-09-13T20:20:00Z",
  "updatedAtUtc": null
}
```

### 7.3 Crear servicio

```http
POST /api/v1/veterinary-services
Authorization: Bearer <admin-token>
Content-Type: application/json
```

```json
{
  "name": "Consulta general",
  "description": "Evaluación veterinaria general de la mascota.",
  "durationMinutes": 30,
  "price": 75.00
}
```

Resultados:

- `201 Created`: servicio creado.
- `400 Bad Request`: datos inválidos.
- `403 Forbidden`: usuario sin rol `Admin`.
- `409 Conflict`: nombre duplicado.

### 7.4 Actualizar servicio

```http
PUT /api/v1/veterinary-services/{id}
Authorization: Bearer <admin-token>
Content-Type: application/json
```

Utiliza el mismo conjunto editable de propiedades que la creación. Devuelve `200 OK`.

### 7.5 Desactivar servicio

```http
DELETE /api/v1/veterinary-services/{id}
Authorization: Bearer <admin-token>
```

Devuelve `204 No Content`. Las citas históricas conservan la relación.

### 7.6 Listado administrativo

```http
GET /api/v1/admin/veterinary-services?isActive=false&pageNumber=1&pageSize=10
Authorization: Bearer <admin-token>
```

Permite consultar activos e inactivos.

## 8. Endpoints de citas

| Método | Ruta | Acceso | Resultado principal |
|---|---|---|---|
| `GET` | `/api/v1/appointments` | Customer | `200 OK` |
| `GET` | `/api/v1/appointments/{id}` | Propietario o Admin | `200 OK` |
| `POST` | `/api/v1/appointments` | Customer | `201 Created` |
| `PUT` | `/api/v1/appointments/{id}` | Propietario | `200 OK` |
| `PATCH` | `/api/v1/appointments/{id}/cancel` | Propietario o Admin | `204 No Content` |
| `GET` | `/api/v1/admin/appointments` | Admin | `200 OK` |
| `PATCH` | `/api/v1/admin/appointments/{id}/status` | Admin | `200 OK` |

### 8.1 Listar citas del cliente

```http
GET /api/v1/appointments?petId={petId}&status=Pending&from=2026-10-01T00:00:00Z&to=2026-10-31T23:59:59Z&sortDirection=asc&pageNumber=1&pageSize=10
Authorization: Bearer <token>
```

#### Parámetros

| Parámetro | Tipo | Descripción |
|---|---|---|
| `petId` | UUID | Mascota propia. |
| `status` | enum | `Pending`, `Confirmed`, `Completed`, `Cancelled`. |
| `from` | datetime UTC | Inicio del rango. |
| `to` | datetime UTC | Fin del rango. |
| `sortDirection` | string | `asc` o `desc` por inicio. |
| `pageNumber` | int | Mínimo 1. |
| `pageSize` | int | 1 a 100. |

Ordenamiento estable:

```text
ScheduledStartUtc, Id
```

### 8.2 Crear cita

```http
POST /api/v1/appointments
Authorization: Bearer <token>
Content-Type: application/json
```

#### Petición

```json
{
  "petId": "22222222-2222-2222-2222-222222222222",
  "veterinaryServiceId": "33333333-3333-3333-3333-333333333333",
  "scheduledStartUtc": "2026-10-05T15:00:00Z",
  "reason": "Control preventivo anual"
}
```

El cliente no envía:

- `scheduledEndUtc`.
- `price`.
- `status`.
- `ownerId`.

#### Respuesta `201 Created`

```json
{
  "id": "44444444-4444-4444-4444-444444444444",
  "pet": {
    "id": "22222222-2222-2222-2222-222222222222",
    "name": "Luna"
  },
  "veterinaryService": {
    "id": "33333333-3333-3333-3333-333333333333",
    "name": "Consulta general",
    "durationMinutes": 30
  },
  "scheduledStartUtc": "2026-10-05T15:00:00Z",
  "scheduledEndUtc": "2026-10-05T15:30:00Z",
  "price": 75.00,
  "status": "Pending",
  "reason": "Control preventivo anual",
  "cancellationReason": null,
  "createdAtUtc": "2026-09-13T20:30:00Z",
  "updatedAtUtc": null
}
```

#### Errores relevantes

- `400 Bad Request`: fecha no futura o formato inválido.
- `404 Not Found`: mascota/servicio inexistente o mascota ajena.
- `409 Conflict`: servicio inactivo, mascota inactiva o conflicto de horario.

### 8.3 Reprogramar cita

```http
PUT /api/v1/appointments/{id}
Authorization: Bearer <token>
Content-Type: application/json
```

```json
{
  "scheduledStartUtc": "2026-10-06T16:00:00Z",
  "reason": "Control preventivo anual reprogramado"
}
```

La aplicación recalcula el final. Solo se permite para citas `Pending` o `Confirmed`.

### 8.4 Cancelar cita

```http
PATCH /api/v1/appointments/{id}/cancel
Authorization: Bearer <token>
Content-Type: application/json
```

```json
{
  "cancellationReason": "No podré asistir en el horario reservado."
}
```

Respuesta: `204 No Content`.

### 8.5 Listar todas las citas como administrador

```http
GET /api/v1/admin/appointments?status=Pending&from=2026-10-01T00:00:00Z&to=2026-10-31T23:59:59Z&pageNumber=1&pageSize=10
Authorization: Bearer <admin-token>
```

### 8.6 Cambiar estado como administrador

```http
PATCH /api/v1/admin/appointments/{id}/status
Authorization: Bearer <admin-token>
Content-Type: application/json
```

```json
{
  "status": "Confirmed",
  "cancellationReason": null
}
```

Solo se aceptan transiciones definidas en las reglas de negocio.

## 9. Estado de la aplicación

| Método | Ruta | Acceso | Resultado |
|---|---|---|---|
| `GET` | `/health` | Público | `200 OK` si la aplicación y base de datos están saludables. |

La implementación podrá devolver `503 Service Unavailable` cuando una dependencia crítica no responda.

## 10. Problem Details

Los errores controlados seguirán una estructura consistente:

```json
{
  "type": "https://vetcare/errors/appointment-time-conflict",
  "title": "Horario no disponible",
  "status": 409,
  "detail": "Ya existe una cita que se superpone con el horario solicitado.",
  "instance": "/api/v1/appointments",
  "traceId": "00-abc123",
  "errorCode": "APPOINTMENT_TIME_CONFLICT"
}
```

### 10.1 Errores de validación

Además, una validación puede incluir:

```json
{
  "type": "https://vetcare/errors/validation",
  "title": "La solicitud contiene errores de validación.",
  "status": 400,
  "errors": {
    "name": ["El nombre es obligatorio."],
    "weightKg": ["El peso debe ser mayor que cero."]
  },
  "traceId": "00-def456",
  "errorCode": "VALIDATION_ERROR"
}
```

## 11. Códigos HTTP

| Escenario | Código |
|---|---:|
| Consulta correcta | `200 OK` |
| Recurso creado | `201 Created` |
| Desactivación/cancelación sin cuerpo | `204 No Content` |
| Datos inválidos | `400 Bad Request` |
| Token ausente o inválido | `401 Unauthorized` |
| Rol insuficiente | `403 Forbidden` |
| Recurso inexistente o ajeno | `404 Not Found` |
| Conflicto de estado o unicidad | `409 Conflict` |
| Error inesperado | `500 Internal Server Error` |
| Dependencia no saludable | `503 Service Unavailable` |

## 12. Códigos internos previstos

```text
VALIDATION_ERROR
INVALID_CREDENTIALS
EMAIL_ALREADY_EXISTS
PET_NOT_FOUND
PET_HAS_ACTIVE_APPOINTMENTS
SERVICE_NOT_FOUND
SERVICE_INACTIVE
SERVICE_NAME_ALREADY_EXISTS
APPOINTMENT_NOT_FOUND
APPOINTMENT_TIME_CONFLICT
APPOINTMENT_INVALID_STATUS_TRANSITION
APPOINTMENT_CANNOT_BE_MODIFIED
FORBIDDEN
INTERNAL_SERVER_ERROR
```

## 13. Archivos `.http` previstos

```text
http/
├── auth.http
├── pets.http
├── veterinary-services.http
├── appointments.http
└── health.http
```

Variables sugeridas:

```http
@baseUrl = https://localhost:7001
@accessToken = PEGAR_TOKEN_AQUI
@adminToken = PEGAR_TOKEN_ADMIN_AQUI
@petId = 00000000-0000-0000-0000-000000000000
@serviceId = 00000000-0000-0000-0000-000000000000
@appointmentId = 00000000-0000-0000-0000-000000000000
```

No se versionarán tokens reales.

## 14. Versionado

VetCare v1 utiliza versionado por URL:

```text
/api/v1
```

Un cambio incompatible futuro podrá introducir `/api/v2` sin romper clientes existentes.

## 15. Referencias relacionadas

- [Requisitos funcionales](02-functional-requirements.md)
- [Reglas de negocio](03-business-rules.md)
- [Modelo de seguridad](06-security-model.md)
- [Plan de pruebas](07-test-plan.md)
