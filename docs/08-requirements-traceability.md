# VetCare — Matriz de trazabilidad de requisitos

> **Estado:** Aprobado para la versión 1  
> **Versión del documento:** 1.0

## 1. Propósito

Relacionar los lineamientos del bootcamp y los requisitos de VetCare con su implementación y evidencia de verificación. Esta matriz permitirá comprobar al final del proyecto que ningún requisito obligatorio fue omitido.

## 2. Trazabilidad de los lineamientos del profesor

| Lineamiento | Implementación prevista | Evidencia final |
|---|---|---|
| API backend con .NET 10 | `VetCare.Api` con `TargetFramework net10.0`. | `.csproj`, compilación y README. |
| Minimal APIs | Grupos con `MapGet`, `MapPost`, `MapPut`, `MapPatch` y `MapDelete`. | Archivos de endpoints y OpenAPI. |
| CRUD HTTP completo | CRUD de mascotas y servicios veterinarios. | Swagger, archivos `.http` y pruebas. |
| Inyección de dependencias | Servicios, repositorios y abstracciones registrados en DI. | `DependencyInjection.cs` y pruebas. |
| Lógica en servicios | `PetService`, `VeterinaryCatalogService`, `AppointmentService`, `AuthService`. | Código y pruebas unitarias. |
| Lógica fuera de endpoints | Los endpoints solo coordinan HTTP y servicios. | Revisión de código. |
| Logging estructurado | `ILogger<T>` en servicios con plantillas. | Logs durante la demostración. |
| Separación de capas | Api, Application, Domain e Infrastructure. | Solución y diagrama de arquitectura. |
| Al menos dos entidades | Pet, VeterinaryService y Appointment. | Modelo de dominio y migraciones. |
| Al menos una relación | User–Pet, Pet–Appointment, Service–Appointment. | Diagrama ER y claves foráneas. |
| Códigos HTTP correctos | 200, 201, 204, 400, 401, 403, 404, 409, 500 y 503. | Pruebas de integración. |
| Validación de inputs | Data Annotations/validación nativa y reglas de aplicación. | Problem Details y pruebas. |
| Problem Details | Manejador global de excepciones. | Respuestas de error automatizadas. |
| Archivos `.http` | Carpeta `http` por módulo. | Repositorio. |
| OpenAPI | Documento OpenAPI v1. | `/openapi/v1.json`. |
| Swagger UI | Interfaz para explorar la API. | `/swagger`. |
| Endpoint `/health` | Health check de aplicación y SQL Server. | `GET /health` y prueba. |
| Endpoint paginado | Mascotas, servicios y citas. | Respuestas con metadatos. |
| Registro con Identity | `UserManager<ApplicationUser>`. | Flujo de registro. |
| Login emite token | `JwtTokenGenerator`. | Respuesta de login. |
| JWT en peticiones | JwtBearer y header `Authorization`. | Endpoint protegido. |
| Endpoint protegido | Mascotas y citas requieren autenticación. | Prueba `401` sin token. |
| Roles | `Customer` y `Admin`. | Seed y claims. |
| Claims | `sub`, `email`, `role`, `jti`, `iat`, `exp`, `iss`, `aud`. | JWT decodificado durante demostración. |
| Políticas | `AdminOnly` y autenticación general. | Rutas administrativas. |
| Prueba unitaria | Servicios probados de forma aislada. | `VetCare.UnitTests`. |
| Prueba de integración | API probada de punta a punta. | `VetCare.IntegrationTests`. |
| EF Core | Proveedor SQL Server y migraciones. | `VetCareDbContext`. |
| Docker | Dockerfile multietapa y Compose. | Ejecución en contenedor. |
| Azure | API desplegada y Azure SQL Database. | URL pública. |
| GitHub público | Repositorio `vetcare-api`. | URL de GitHub. |
| README | Instalación, arquitectura y uso. | `README.md`. |
| Video | Demostración funcional y técnica. | Enlace de YouTube o Drive. |

## 3. Trazabilidad de autenticación

| Requisito | Endpoint / componente | Regla asociada | Pruebas previstas |
|---|---|---|---|
| `RF-AUTH-01` Registro | `POST /api/v1/auth/register`, `AuthService` | `RN-AUTH-01` a `RN-AUTH-04` | `PI-AUTH-01`, `PI-AUTH-03` |
| `RF-AUTH-02` Correo duplicado | `AuthService`, Identity | `RN-AUTH-01` | `PI-AUTH-02` |
| `RF-AUTH-03` Login | `POST /api/v1/auth/login` | `RN-AUTH-05`, `RN-AUTH-06` | `PI-AUTH-04`, `PI-AUTH-05` |
| `RF-AUTH-04` JWT | `JwtTokenGenerator` | `RN-AUTH-08` | `PI-AUTH-04`, `PI-AUTH-07` |
| `RF-AUTH-05` Usuario actual | `GET /api/v1/auth/me` | Propiedad del claim `sub` | `PI-AUTH-07` |
| `RF-AUTH-06` Rol Customer | `AuthService`, seed de roles | `RN-AUTH-02`, `RN-AUTH-03` | `PI-AUTH-01`, `PI-AUTH-08` |
| `RF-AUTH-07` Rechazo de credenciales | `AuthService` | `RN-AUTH-05`, `RN-AUTH-06` | `PI-AUTH-05` |

## 4. Trazabilidad de mascotas

| Requisito | Endpoint / componente | Regla asociada | Pruebas previstas |
|---|---|---|---|
| `RF-PET-01` Crear | `POST /api/v1/pets`, `PetService` | `RN-PET-01` a `RN-PET-06` | `PU-PET-01`, `PU-PET-02`, `PU-PET-08`, `PI-PET-01` a `PI-PET-03` |
| `RF-PET-02` Listar propias | `GET /api/v1/pets` | `RN-PET-07` | `PI-PET-04` |
| `RF-PET-03` Paginar | `GET /api/v1/pets` | `RN-PAGE-01` a `RN-PAGE-06` | `PU-PET-06`, `PI-PET-09` |
| `RF-PET-04` Obtener por ID | `GET /api/v1/pets/{id}` | `RN-PET-07`, `RN-PET-11` | `PI-PET-05` |
| `RF-PET-05` Actualizar | `PUT /api/v1/pets/{id}` | `RN-PET-03`, `RN-PET-04`, `RN-PET-07` | `PU-PET-03`, `PI-PET-06` |
| `RF-PET-06` Desactivar | `DELETE /api/v1/pets/{id}` | `RN-PET-09`, `RN-PET-10` | `PU-PET-04`, `PU-PET-05`, `PI-PET-07`, `PI-PET-08` |
| `RF-PET-07` Aislamiento | Todos los endpoints de mascotas | `RN-PET-07`, `RN-PET-11` | `PU-PET-03`, `PI-PET-05` |
| `RF-PET-08` Buscar/filtrar/ordenar | `GET /api/v1/pets` | `RN-PAGE-05`, `RN-PAGE-06` | `PI-PET-10`, `PI-PET-11` |
| `RF-PET-09` Conservar historial | Persistencia | `RN-PET-10`, `RN-PET-12` | `PU-PET-05`, verificación integración |

## 5. Trazabilidad de servicios

| Requisito | Endpoint / componente | Regla asociada | Pruebas previstas |
|---|---|---|---|
| `RF-SVC-01` Consulta pública | `GET /api/v1/veterinary-services` | `RN-SVC-12` | `PI-SVC-01`, `PI-SVC-07` |
| `RF-SVC-02` Consulta Admin | `GET /api/v1/admin/veterinary-services` | `RN-SVC-01` a `RN-SVC-03` | `PI-SVC-08` |
| `RF-SVC-03` Crear | `POST /api/v1/veterinary-services` | `RN-SVC-01`, `RN-SVC-04` a `RN-SVC-08` | `PU-SVC-01` a `PU-SVC-04`, `PI-SVC-02`, `PI-SVC-03` |
| `RF-SVC-04` Actualizar | `PUT /api/v1/veterinary-services/{id}` | `RN-SVC-02` | `PI-SVC-05` |
| `RF-SVC-05` Desactivar | `DELETE /api/v1/veterinary-services/{id}` | `RN-SVC-03`, `RN-SVC-10`, `RN-SVC-11` | `PU-SVC-05`, `PU-SVC-06`, `PI-SVC-06` |
| `RF-SVC-06` Nombre único | Servicio y repositorio | `RN-SVC-05` | `PU-SVC-01`, `PI-SVC-04` |
| `RF-SVC-07` Solo Admin | Política `AdminOnly` | `RN-SVC-01` a `RN-SVC-03` | `PI-AUTH-08`, `PI-SVC-02` |
| `RF-SVC-08` Paginación/filtros | GET público y Admin | Reglas de página | Pruebas de consulta previstas |

## 6. Trazabilidad de citas

| Requisito | Endpoint / componente | Regla asociada | Pruebas previstas |
|---|---|---|---|
| `RF-APT-01` Crear | `POST /api/v1/appointments` | `RN-APT-01` a `RN-APT-12` | `PU-APT-01` a `PU-APT-08`, `PI-APT-01` a `PI-APT-06` |
| `RF-APT-02` Listar propias | `GET /api/v1/appointments` | `RN-APT-21` | `PI-APT-07` |
| `RF-APT-03` Paginar | Listados de citas | Reglas de página | `PU-APT-17`, `PI-APT-14` |
| `RF-APT-04` Obtener por ID | `GET /api/v1/appointments/{id}` | `RN-APT-18`, `RN-APT-21` | Prueba de propiedad prevista |
| `RF-APT-05` Reprogramar | `PUT /api/v1/appointments/{id}` | `RN-APT-13`, `RN-APT-15`, `RN-APT-16` | `PU-APT-09`, `PU-APT-16`, `PI-APT-08` |
| `RF-APT-06` Cancelar | `PATCH /api/v1/appointments/{id}/cancel` | `RN-APT-14`, `RN-APT-18`, `RN-APT-20` | `PI-APT-09` |
| `RF-APT-07` Listado Admin | `GET /api/v1/admin/appointments` | `RN-APT-21` | `PI-APT-14`, `PI-APT-15` |
| `RF-APT-08` Confirmar | PATCH estado Admin | Transición `Pending → Confirmed` | `PU-APT-11`, `PI-APT-11` |
| `RF-APT-09` Completar | PATCH estado Admin | Transición `Confirmed → Completed` | `PU-APT-12`, `PI-APT-12` |
| `RF-APT-10` Evitar superposición | `AppointmentService`, repositorio | `RN-APT-08` a `RN-APT-10` | `PU-APT-05`, `PU-APT-14` a `PU-APT-16`, `PI-APT-05`, `PI-APT-06` |
| `RF-APT-11` Calcular fin | `AppointmentService` | `RN-APT-05` | `PU-APT-06` |
| `RF-APT-12` Precio histórico | `AppointmentService` | `RN-APT-11`, `RN-APT-12` | `PU-APT-07` |
| `RF-APT-13` Filtros | GET cliente y Admin | Reglas de consulta | `PI-APT-15` |
| `RF-APT-14` Conservar historial | Persistencia | `RN-APT-19` | Verificación de cancelación e historial |

## 7. Trazabilidad de operación

| Requisito | Implementación | Prueba/evidencia |
|---|---|---|
| `RF-OPS-01` `/health` | Health Checks + SQL Server | `PI-OPS-01` |
| `RF-OPS-02` OpenAPI | `AddOpenApi` / `MapOpenApi` | Documento generado |
| `RF-OPS-03` Swagger UI | Configuración Swagger UI | Demostración `/swagger` |
| `RF-OPS-04` `.http` | Carpeta `http` | Repositorio |
| `RF-OPS-05` Logging | `ILogger<T>` en servicios | Consola/stream de Azure |
| `RF-OPS-06` Problem Details | `AddProblemDetails` + handler global | `PI-OPS-02` y pruebas de error |
| `RF-OPS-07` Seed idempotente | `DatabaseInitializer` | Ejecución repetida sin duplicados |

## 8. Trazabilidad de requisitos no funcionales

| Requisito | Evidencia prevista |
|---|---|
| `RNF-01` | Archivos `.csproj` con `net10.0`. |
| `RNF-02` | Clases de endpoints Minimal API. |
| `RNF-03` | Servicios de aplicación y revisión de endpoints. |
| `RNF-04` | Registro de DI y pruebas con sustitución de dependencias. |
| `RNF-05` | `VetCareDbContext`, migraciones y repositorios. |
| `RNF-06` | `UseSqlServer` y Compose local. |
| `RNF-07` | Cadena de Azure SQL Database en App Settings. |
| `RNF-08` | Métodos `async` y consultas EF asíncronas. |
| `RNF-09` | Parámetros `CancellationToken`. |
| `RNF-10` | DTOs/contracts separados. |
| `RNF-11` | Configuración JSON y ejemplos OpenAPI. |
| `RNF-12` | Convertidor de enums a texto. |
| `RNF-13` | Propiedades `*Utc`, `TimeProvider` y pruebas. |
| `RNF-14` | Revisión de logs y secretos. |
| `RNF-15` | Validación común de paginación. |
| `RNF-16` | Dockerfile y Docker Compose. |
| `RNF-17` | `.gitignore`, Secret Manager y revisión del repositorio. |
| `RNF-18` | Repositorio exclusivo del backend y CORS configurable. |
| `RNF-19` | Repositorios con `AsNoTracking` y proyección. |
| `RNF-20` | `ThenBy(Id)` en consultas paginadas. |
| `RNF-21` | Configuración HTTPS de Azure. |
| `RNF-22` | Política CORS nombrada. |
| `RNF-23` | Handler global y pruebas Problem Details. |
| `RNF-24` | Proyectos UnitTests e IntegrationTests. |

## 9. Estado de cumplimiento

Durante el desarrollo se utilizarán los estados:

```text
Pendiente
En progreso
Implementado
Verificado
No aplica
```

La matriz podrá ampliarse con una columna `Estado` al comenzar la implementación.

## 10. Criterio de cierre

La versión 1 no se considerará terminada mientras exista un lineamiento obligatorio sin:

1. Implementación identificable.
2. Evidencia verificable.
3. Documentación suficiente.
4. Prueba automatizada cuando corresponda.
