# VetCare API

API REST desarrollada con .NET 10 para gestionar usuarios, mascotas,
servicios y citas veterinarias.

## Estado del proyecto

En desarrollo.

Fase actual: modelo de dominio completado.

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

