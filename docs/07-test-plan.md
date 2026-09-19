# VetCare — Plan de pruebas

> **Estado:** Aprobado para la versión 1  
> **Versión del documento:** 1.0  
> **Framework principal:** xUnit

## 1. Objetivo

Definir cómo se verificará que VetCare cumple sus requisitos funcionales, reglas de negocio, seguridad y contrato HTTP. La estrategia combina pruebas unitarias de servicios con pruebas de integración de la API completa.

## 2. Alcance

Se probarán:

- Servicios de autenticación, mascotas, servicios y citas.
- Reglas de propiedad.
- Reglas temporales y transiciones de estado.
- Paginación, filtros y ordenamiento.
- Códigos HTTP.
- Problem Details.
- Autenticación JWT y autorización por roles.
- Integración con Entity Framework Core y SQL Server.
- Endpoint `/health`.

No se probará directamente:

- El funcionamiento interno de .NET, Identity o EF Core.
- La interfaz de Swagger UI como prueba de navegador.
- Un frontend, porque está fuera del alcance v1.
- Servicios externos inexistentes en VetCare v1.

## 3. Estrategia

```text
Pruebas unitarias
    ↓ muchas, rápidas y aisladas
Pruebas de integración
    ↓ flujos HTTP y persistencia reales
Pruebas manuales de demostración
    ↓ Swagger y archivos .http
```

### 3.1 Pruebas unitarias

Validan un servicio o regla de forma aislada, sin iniciar la API ni conectarse a la base de datos real.

Dependencias sustituibles:

- Repositorios.
- `IUnitOfWork`.
- `TimeProvider`.
- Generador de token, cuando corresponda.
- Logger, mediante `NullLogger<T>` o un doble de prueba.

### PetService

Casos cubiertos:

- La creación utiliza el usuario autenticado como propietario.
- Un usuario no puede consultar mascotas de otro propietario.
- No se puede desactivar una mascota con citas futuras activas.
- Una mascota sin citas futuras puede desactivarse correctamente.
- `pageSize` mayor a 100 produce error de validación.

### VeterinaryServiceService

Casos cubiertos:

- Creación correcta de un servicio activo.
- Rechazo de nombres duplicados.
- Un servicio inactivo no es visible mediante consulta pública.
- Un servicio inactivo no puede modificarse.
- La desactivación lógica modifica `IsActive`.
- `pageSize` mayor que 100 produce error de validación.

### 3.2 Pruebas de integración

Inician VetCare con `WebApplicationFactory<Program>` y envían peticiones mediante `HttpClient`.

Incluirán:

- Pipeline HTTP real.
- Routing y binding.
- Validación.
- Autenticación y autorización.
- Servicios reales.
- Repositorios reales.
- EF Core real.
- SQL Server de pruebas aislado.

## 4. Herramientas previstas

| Herramienta | Uso |
|---|---|
| xUnit | Framework de pruebas. |
| `Microsoft.AspNetCore.Mvc.Testing` | Hospedar la API en pruebas de integración. |
| `WebApplicationFactory<Program>` | Crear el sistema bajo prueba. |
| `HttpClient` | Enviar peticiones HTTP. |
| SQL Server de pruebas | Validar el proveedor real de persistencia. |
| Docker o contenedor de pruebas | Aislar SQL Server durante integración/CI. |
| `dotnet test` | Ejecutar toda la suite. |

No se requiere una biblioteca de aserciones adicional para la primera versión; las aserciones nativas de xUnit son suficientes.

## 5. Convenciones de nombres

Formato recomendado:

```text
Metodo_Escenario_ResultadoEsperado
```

Ejemplos:

```text
CreateAsync_WhenStartIsInPast_ThrowsValidationException
CreateAsync_WhenScheduleOverlaps_ThrowsConflictException
GetPets_WhenAuthenticated_ReturnsOk
CreateService_WhenUserIsCustomer_ReturnsForbidden
```

## 6. Estructura Arrange–Act–Assert

Cada prueba seguirá:

```csharp
// Arrange
// Act
// Assert
```

Principios:

- Una prueba verifica un comportamiento principal.
- No depende del orden de ejecución.
- No comparte estado mutable con otras pruebas.
- Utiliza nombres y datos fáciles de entender.
- Evita fechas reales no controladas.

## 7. Control determinista del tiempo

Las reglas de fechas utilizarán `TimeProvider`.

En pruebas se configurará un reloj fijo, por ejemplo:

```text
Ahora de prueba: 2026-10-01T12:00:00Z
```

Esto permite probar:

- Citas pasadas.
- Citas futuras.
- Fechas de creación.
- Reprogramaciones.
- Fechas de nacimiento futuras.

## 8. Pruebas unitarias de `AppointmentService`

| ID | Requisito/regla | Escenario | Resultado esperado |
|---|---|---|---|
| `PU-APT-01` | `RN-APT-04` | Inicio en el pasado. | Excepción de validación. |
| `PU-APT-02` | `RN-APT-02` | Mascota perteneciente a otro usuario. | Recurso no encontrado. |
| `PU-APT-03` | `RN-APT-01` | Mascota inactiva. | Conflicto o regla de negocio rechazada. |
| `PU-APT-04` | `RN-APT-03` | Servicio inactivo. | Conflicto o regla de negocio rechazada. |
| `PU-APT-05` | `RN-APT-08` | Horario superpuesto. | Excepción `ConflictException`. |
| `PU-APT-06` | `RN-APT-05` | Servicio de 30 minutos. | Fin calculado 30 minutos después. |
| `PU-APT-07` | `RN-APT-11` | Servicio con precio 75.00. | La cita conserva precio 75.00. |
| `PU-APT-08` | `RN-APT-07` | Creación válida. | Estado inicial `Pending`. |
| `PU-APT-09` | `RN-APT-13` | Reprogramar cita completada. | Operación rechazada. |
| `PU-APT-10` | `RN-APT-16` | Reactivar cita cancelada. | Transición inválida. |
| `PU-APT-11` | Estados | `Pending → Confirmed` por Admin. | Transición aceptada. |
| `PU-APT-12` | Estados | `Confirmed → Completed` por Admin. | Transición aceptada. |
| `PU-APT-13` | Estados | `Pending → Completed`. | Transición rechazada. |
| `PU-APT-14` | Conflictos | Nueva cita empieza cuando termina otra. | No hay conflicto. |
| `PU-APT-15` | Conflictos | Nueva cita termina cuando empieza otra. | No hay conflicto. |
| `PU-APT-16` | Reprogramación | Conflicto incluye la propia cita. | La propia cita se excluye. |
| `PU-APT-17` | Paginación | Repositorio devuelve una página. | Metadatos mapeados correctamente. |

## 9. Pruebas unitarias de `PetService`

| ID | Requisito/regla | Escenario | Resultado esperado |
|---|---|---|---|
| `PU-PET-01` | `RN-PET-03` | Fecha de nacimiento futura. | Excepción de validación. |
| `PU-PET-02` | `RN-PET-04` | Peso igual o menor que cero. | Excepción de validación. |
| `PU-PET-03` | `RN-PET-07` | Actualizar mascota ajena. | `NotFoundException`. |
| `PU-PET-04` | `RN-PET-09` | Desactivar con cita futura activa. | `ConflictException`. |
| `PU-PET-05` | `RN-PET-10` | Desactivación válida. | `IsActive` pasa a `false`. |
| `PU-PET-06` | `RF-PET-03` | Página con 25 registros, tamaño 10. | Total de páginas igual a 3. |
| `PU-PET-07` | Auditoría | Crear mascota. | `CreatedAtUtc` usa el reloj inyectado. |
| `PU-PET-08` | Propiedad | Crear mascota. | `OwnerId` se toma del usuario actual. |

## 10. Pruebas unitarias de `VeterinaryCatalogService`

| ID | Requisito/regla | Escenario | Resultado esperado |
|---|---|---|---|
| `PU-SVC-01` | `RN-SVC-05` | Nombre duplicado. | `ConflictException`. |
| `PU-SVC-02` | `RN-SVC-07` | Precio negativo. | Excepción de validación. |
| `PU-SVC-03` | `RN-SVC-08` | Duración menor a 15. | Excepción de validación. |
| `PU-SVC-04` | `RN-SVC-08` | Duración mayor a 240. | Excepción de validación. |
| `PU-SVC-05` | `RN-SVC-10` | Desactivar servicio. | `IsActive` pasa a `false`. |
| `PU-SVC-06` | `RN-SVC-11` | Desactivar servicio con historial. | No elimina citas. |

## 11. Pruebas de integración de operación

| ID | Escenario | Petición | Resultado esperado |
|---|---|---|---|
| `PI-OPS-01` | Aplicación y DB saludables. | `GET /health` | `200 OK`. |
| `PI-OPS-02` | Error controlado. | Petición inválida | `application/problem+json`. |
| `PI-OPS-03` | Endpoint inexistente. | Ruta no registrada | `404 Not Found`. |

## 12. Pruebas de integración de autenticación

| ID | Escenario | Resultado esperado |
|---|---|---|
| `PI-AUTH-01` | Registro válido. | `201 Created`; rol `Customer`. |
| `PI-AUTH-02` | Registro con correo duplicado. | `409 Conflict`. |
| `PI-AUTH-03` | Registro con contraseña débil. | `400 Bad Request` con errores. |
| `PI-AUTH-04` | Login válido. | `200 OK` y JWT. |
| `PI-AUTH-05` | Login inválido. | `401 Unauthorized`. |
| `PI-AUTH-06` | Endpoint protegido sin token. | `401 Unauthorized`. |
| `PI-AUTH-07` | Token válido en `/auth/me`. | `200 OK` y usuario correcto. |
| `PI-AUTH-08` | Customer intenta ruta Admin. | `403 Forbidden`. |

## 13. Pruebas de integración de mascotas

| ID | Escenario | Resultado esperado |
|---|---|---|
| `PI-PET-01` | Customer crea mascota válida. | `201 Created` y `Location`. |
| `PI-PET-02` | Nombre vacío. | `400 Bad Request` con Problem Details. |
| `PI-PET-03` | Fecha futura. | `400 Bad Request`. |
| `PI-PET-04` | Customer lista sus mascotas. | `200 OK` y solo recursos propios. |
| `PI-PET-05` | Customer consulta mascota ajena. | `404 Not Found`. |
| `PI-PET-06` | Customer actualiza mascota propia. | `200 OK`. |
| `PI-PET-07` | Customer desactiva mascota propia. | `204 No Content`. |
| `PI-PET-08` | Desactivar mascota con cita futura. | `409 Conflict`. |
| `PI-PET-09` | Paginación. | Metadatos y cantidad correctos. |
| `PI-PET-10` | Filtro por especie. | Solo especies solicitadas. |
| `PI-PET-11` | Ordenamiento por nombre. | Orden estable. |

## 14. Pruebas de integración de servicios

| ID | Escenario | Resultado esperado |
|---|---|---|
| `PI-SVC-01` | Anónimo consulta servicios activos. | `200 OK`. |
| `PI-SVC-02` | Customer crea servicio. | `403 Forbidden`. |
| `PI-SVC-03` | Admin crea servicio. | `201 Created`. |
| `PI-SVC-04` | Admin crea nombre duplicado. | `409 Conflict`. |
| `PI-SVC-05` | Admin actualiza servicio. | `200 OK`. |
| `PI-SVC-06` | Admin desactiva servicio. | `204 No Content`. |
| `PI-SVC-07` | Servicio inactivo en catálogo público. | No aparece. |
| `PI-SVC-08` | Admin lista servicios inactivos. | Aparece en ruta administrativa. |

## 15. Pruebas de integración de citas

| ID | Escenario | Resultado esperado |
|---|---|---|
| `PI-APT-01` | Customer crea cita válida. | `201 Created`; estado `Pending`. |
| `PI-APT-02` | Cita en el pasado. | `400 Bad Request`. |
| `PI-APT-03` | Mascota ajena. | `404 Not Found`. |
| `PI-APT-04` | Servicio inactivo. | `409 Conflict`. |
| `PI-APT-05` | Horario superpuesto. | `409 Conflict`. |
| `PI-APT-06` | Horario contiguo sin superposición. | `201 Created`. |
| `PI-APT-07` | Customer lista citas propias. | `200 OK`; no incluye ajenas. |
| `PI-APT-08` | Customer reprograma cita pendiente. | `200 OK`. |
| `PI-APT-09` | Customer cancela cita. | `204 No Content`; estado cancelado. |
| `PI-APT-10` | Customer completa cita. | `403` o ruta no disponible para Customer. |
| `PI-APT-11` | Admin confirma cita pendiente. | `200 OK`; estado `Confirmed`. |
| `PI-APT-12` | Admin completa cita confirmada. | `200 OK`; estado `Completed`. |
| `PI-APT-13` | Transición `Pending → Completed`. | `409 Conflict`. |
| `PI-APT-14` | Listado paginado administrativo. | Metadatos correctos. |
| `PI-APT-15` | Filtro por estado y fechas. | Solo resultados coincidentes. |

## 16. Mínimos exigidos por el profesor

La suite debe incluir como mínimo:

### Prueba unitaria obligatoria

```text
AppointmentService detecta una cita superpuesta.
```

### Prueba de integración obligatoria

```text
GET /api/v1/pets devuelve 200 con un usuario autenticado.
```

VetCare implementará pruebas adicionales para todos los flujos críticos.

## 17. Entorno de integración

### 17.1 Base de datos

La opción preferida es una instancia SQL Server aislada para las pruebas, iniciada mediante contenedor. Esto evita diferencias entre el proveedor de pruebas y el proveedor productivo.

No se utilizará el proveedor EF Core InMemory para validar comportamiento relacional, restricciones o consultas SQL.

### 17.2 Inicialización

Cada ejecución debe:

1. Crear una base de datos limpia o un esquema aislado.
2. Aplicar migraciones.
3. Crear roles.
4. Insertar únicamente los datos necesarios.
5. Ejecutar pruebas.
6. Eliminar o reiniciar el estado al finalizar.

### 17.3 Usuarios de prueba

Se crearán mediante helpers:

- `customer1@vetcare.test`.
- `customer2@vetcare.test`.
- `admin@vetcare.test`.

Las contraseñas se utilizarán solamente dentro del entorno de prueba.

## 18. `VetCareWebApplicationFactory`

La factoría de pruebas será responsable de:

- Hospedar `Program`.
- Sobrescribir la cadena de conexión.
- Usar configuración de ambiente `Testing`.
- Asegurar migraciones y seed mínimo.
- Crear clientes HTTP.
- Facilitar autenticación de Customer y Admin.
- Evitar el uso de secretos reales.

## 19. Aislamiento y paralelismo

- Las pruebas no dependen del orden.
- Cada clase o colección usa datos identificables y aislados.
- Las pruebas que compartan una base se agrupan para controlar paralelismo.
- La limpieza se realiza mediante transacciones, recreación o estrategia equivalente.
- Los identificadores se generan por prueba para evitar colisiones.

## 20. Validación de Problem Details

Las pruebas de error verificarán al menos:

```text
Content-Type
status
title
detail o errors
traceId
errorCode
```

No deben aparecer:

- Stack traces.
- SQL.
- Cadenas de conexión.
- Contraseñas.
- Tokens.

## 21. Pruebas manuales

Antes de cada entrega importante se verificará desde Swagger y archivos `.http`:

1. Registro.
2. Login.
3. Autorización Bearer.
4. CRUD de mascotas.
5. CRUD de servicios como Admin.
6. Creación y reprogramación de cita.
7. Conflicto de horario.
8. Cambio de estado administrativo.
9. `/health`.

Las pruebas manuales complementan, pero no sustituyen, las automatizadas.

## 22. Ejecución

Desde la raíz de la solución:

```bash
dotnet test
```

Ejecución de un proyecto:

```bash
dotnet test tests/VetCare.UnitTests
dotnet test tests/VetCare.IntegrationTests
```

## 23. Integración continua

El pipeline debe ejecutar:

```text
dotnet restore
dotnet build --no-restore
dotnet test --no-build
```

La compilación no debe desplegar si una prueba falla.

## 24. Criterios de salida

La fase de pruebas se considera completa cuando:

- Todas las pruebas obligatorias pasan.
- Todos los flujos críticos tienen cobertura automatizada.
- Los resultados son repetibles.
- No dependen de secretos locales.
- Los errores verifican Problem Details.
- La autorización se prueba con anónimo, Customer y Admin.
- `dotnet test` termina con código de salida 0.
- El pipeline de CI ejecuta la suite correctamente.

## 25. Referencias relacionadas

- [Requisitos](02-functional-requirements.md)
- [Reglas de negocio](03-business-rules.md)
- [Contrato de API](05-api-contract.md)
- [Matriz de trazabilidad](08-requirements-traceability.md)
