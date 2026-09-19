# VetCare API

API REST desarrollada con .NET 10 para gestionar usuarios, mascotas,
servicios y citas veterinarias.

## Estado del proyecto

En desarrollo.

Fase actual: gestión de citas veterinarias completada.

### Aplicar migraciones

```bash
dotnet ef database update \
  --project src/VetCare.Infrastructure \
  --startup-project src/VetCare.Api
```

### Ejecutar datos iniciales

```bash
dotnet run \
  --project src/VetCare.Api \
  -- --SeedDatabase=true
```

## Modelo de dominio

La versión inicial contiene las siguientes entidades:

- `Pet`
- `VeterinaryService`
- `Appointment`

Y las enumeraciones:

- `PetSpecies`
- `PetSex`
- `AppointmentStatus`

## Tecnologías previstas

- .NET 10
- ASP.NET Core Minimal APIs
- Entity Framework Core
- SQL Server
- ASP.NET Core Identity
- JWT Bearer
- OpenAPI y Swagger UI
- xUnit
- Docker
- Azure App Service
- Azure SQL Database

## Arquitectura

La solución utiliza una arquitectura por capas:

- `VetCare.Api`: presentación y endpoints HTTP.
- `VetCare.Application`: casos de uso y lógica de aplicación.
- `VetCare.Domain`: entidades y reglas del dominio.
- `VetCare.Infrastructure`: persistencia, Identity e infraestructura.
- `VetCare.UnitTests`: pruebas unitarias.
- `VetCare.IntegrationTests`: pruebas de integración.

## Base de datos local

VetCare utiliza SQL Server.

La cadena de conexión y las credenciales del administrador se
configuran mediante ASP.NET Core User Secrets.

### Seguridad

VetCare utiliza:

- ASP.NET Core Identity para usuarios y contraseñas.
- JWT Bearer para autenticación de la API.
- Roles `Customer` y `Admin`.
- Políticas de autorización.
- User Secrets para la clave JWT.

## Endpoints

### Mascotas

| Método | Endpoint |
|---|---|
| GET | `/api/v1/pets` |
| GET | `/api/v1/pets/{id}` |
| POST | `/api/v1/pets` |
| PUT | `/api/v1/pets/{id}` |
| DELETE | `/api/v1/pets/{id}` |

Los endpoints requieren autenticación JWT y rol `Customer`.

### Citas

| Método | Endpoint | Acceso |
|---|---|---|
| GET | `/api/v1/appointments` | Customer |
| GET | `/api/v1/appointments/{id}` | Customer |
| POST | `/api/v1/appointments` | Customer |
| PUT | `/api/v1/appointments/{id}` | Customer |
| PATCH | `/api/v1/appointments/{id}/cancel` | Customer |
| GET | `/api/v1/admin/appointments` | Admin |
| PATCH | `/api/v1/admin/appointments/{id}/status` | Admin |

## Documentación

La documentación funcional y técnica se encuentra en la carpeta
[`docs`](docs/01-vision-and-scope.md).

## Estado de compilación

Para compilar la solución:

```bash
dotnet build
```

Para ejecutar las pruebas:

```bash
dotnet test
```

## Autor

Wilder Moreno

