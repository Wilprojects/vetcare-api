# VetCare — Requisitos funcionales y no funcionales

> **Estado:** Aprobado para la versión 1  
> **Versión del documento:** 1.0

## 1. Propósito

Este documento enumera los requisitos verificables de VetCare v1. Cada requisito utiliza un identificador estable para relacionarlo con endpoints, reglas de negocio, pruebas y evidencias del proyecto.

## 2. Convenciones

- `RF`: requisito funcional.
- `RNF`: requisito no funcional.
- Prioridad `Must`: obligatorio para VetCare v1.
- Prioridad `Should`: recomendado si no compromete los requisitos obligatorios.

## 3. Requisitos funcionales de autenticación

| ID | Requisito | Prioridad | Criterio de aceptación resumido |
|---|---|---|---|
| `RF-AUTH-01` | El sistema debe permitir registrar un usuario cliente. | Must | Un registro válido crea el usuario y devuelve `201 Created`. |
| `RF-AUTH-02` | El sistema debe impedir el registro de correos duplicados. | Must | Un correo existente devuelve `409 Conflict`. |
| `RF-AUTH-03` | El sistema debe permitir iniciar sesión mediante correo y contraseña. | Must | Credenciales válidas devuelven `200 OK`. |
| `RF-AUTH-04` | El login debe emitir un token JWT estándar. | Must | La respuesta contiene token, tipo y expiración. |
| `RF-AUTH-05` | El sistema debe permitir consultar los datos del usuario autenticado. | Must | `GET /auth/me` devuelve los datos del usuario del token. |
| `RF-AUTH-06` | Todo registro público debe recibir únicamente el rol `Customer`. | Must | El usuario no puede seleccionar `Admin` en la petición. |
| `RF-AUTH-07` | Credenciales inválidas deben ser rechazadas. | Must | El login devuelve `401 Unauthorized` sin revelar qué dato falló. |

## 4. Requisitos funcionales de mascotas

| ID | Requisito | Prioridad | Criterio de aceptación resumido |
|---|---|---|---|
| `RF-PET-01` | El cliente debe poder registrar una mascota. | Must | Una petición válida devuelve `201 Created`. |
| `RF-PET-02` | El cliente debe poder consultar únicamente sus mascotas. | Must | El listado se filtra por el identificador del usuario autenticado. |
| `RF-PET-03` | El listado de mascotas debe permitir paginación. | Must | La respuesta incluye elementos y metadatos de página. |
| `RF-PET-04` | El cliente debe poder consultar una mascota propia por identificador. | Must | Devuelve `200 OK`; una mascota inexistente o ajena devuelve `404`. |
| `RF-PET-05` | El cliente debe poder actualizar una mascota propia. | Must | Una actualización válida devuelve `200 OK`. |
| `RF-PET-06` | El cliente debe poder desactivar una mascota propia. | Must | La operación establece `IsActive = false` y devuelve `204 No Content`. |
| `RF-PET-07` | Un usuario no debe acceder a mascotas de otro usuario. | Must | Consulta o modificación de un recurso ajeno devuelve `404 Not Found`. |
| `RF-PET-08` | El listado debe permitir búsqueda, filtros y ordenamiento. | Must | Los parámetros admitidos se aplican sin construir SQL dinámico inseguro. |
| `RF-PET-09` | Una mascota inactiva debe conservarse para fines históricos. | Must | El registro permanece en la base de datos. |


### Estado de implementación

La gestión de mascotas se encuentra implementada.

El usuario con rol `Customer` puede:

- Listar sus mascotas con paginación.
- Buscar mascotas por nombre o raza.
- Filtrar mascotas por especie.
- Ordenar por nombre, fecha de creación o fecha de nacimiento.
- Consultar una mascota propia.
- Crear una mascota.
- Actualizar una mascota activa.
- Desactivar lógicamente una mascota.

El `OwnerId` se obtiene exclusivamente del JWT y nunca es recibido
desde el cliente.

Las mascotas pertenecientes a otros usuarios se responden como
`404 Not Found`.

## 5. Requisitos funcionales de servicios veterinarios

| ID | Requisito | Prioridad | Criterio de aceptación resumido |
|---|---|---|---|
| `RF-SVC-01` | Cualquier usuario debe poder consultar servicios activos. | Must | El listado público no requiere token y excluye servicios inactivos. |
| `RF-SVC-02` | Un administrador debe poder consultar servicios activos e inactivos. | Must | La ruta administrativa requiere rol `Admin`. |
| `RF-SVC-03` | Un administrador debe poder crear servicios. | Must | Una petición válida devuelve `201 Created`. |
| `RF-SVC-04` | Un administrador debe poder actualizar servicios. | Must | Una actualización válida devuelve `200 OK`. |
| `RF-SVC-05` | Un administrador debe poder desactivar servicios. | Must | La operación establece `IsActive = false` y devuelve `204 No Content`. |
| `RF-SVC-06` | El sistema debe impedir nombres de servicios duplicados. | Must | Un nombre duplicado devuelve `409 Conflict`. |
| `RF-SVC-07` | Un usuario sin rol `Admin` no debe administrar servicios. | Must | El acceso devuelve `403 Forbidden`. |
| `RF-SVC-08` | Los servicios deben poder consultarse con paginación, filtros y ordenamiento. | Must | La respuesta contiene metadatos y usa campos permitidos. |

### Estado de implementación

La gestión de servicios veterinarios se encuentra implementada.

Los usuarios públicos pueden:

- Consultar servicios veterinarios activos.
- Consultar el detalle de un servicio activo.
- Buscar servicios por nombre o descripción.
- Utilizar paginación y ordenamiento.

El usuario con rol `Admin` puede:

- Consultar servicios activos e inactivos.
- Crear servicios veterinarios.
- Actualizar servicios activos.
- Desactivar servicios mediante eliminación lógica.

Los nombres de servicios son únicos.

Los servicios inactivos no se muestran en el catálogo público y no
pueden modificarse.

## 6. Requisitos funcionales de citas

| ID | Requisito | Prioridad | Criterio de aceptación resumido |
|---|---|---|---|
| `RF-APT-01` | Un cliente debe poder reservar una cita para una mascota propia. | Must | Una cita válida devuelve `201 Created`. |
| `RF-APT-02` | Un cliente debe poder consultar sus citas. | Must | El listado incluye solo citas de sus mascotas. |
| `RF-APT-03` | El listado de citas debe ser paginado. | Must | La respuesta incluye elementos y metadatos. |
| `RF-APT-04` | Un cliente debe poder consultar una cita propia por identificador. | Must | Devuelve `200`; una cita ajena o inexistente devuelve `404`. |
| `RF-APT-05` | Un cliente debe poder reprogramar una cita permitida. | Must | Solo estados editables pueden cambiar de horario. |
| `RF-APT-06` | Un cliente debe poder cancelar una cita propia permitida. | Must | La cita cambia a `Cancelled`. |
| `RF-APT-07` | Un administrador debe poder consultar todas las citas. | Must | La ruta administrativa permite filtros y paginación. |
| `RF-APT-08` | Un administrador debe poder confirmar una cita pendiente. | Must | La transición `Pending → Confirmed` es aceptada. |
| `RF-APT-09` | Un administrador debe poder completar una cita confirmada. | Must | La transición `Confirmed → Completed` es aceptada. |
| `RF-APT-10` | El sistema debe impedir citas con horarios superpuestos. | Must | Un conflicto devuelve `409 Conflict`. |
| `RF-APT-11` | La hora final debe calcularse con la duración del servicio. | Must | El cliente no envía `ScheduledEndUtc`. |
| `RF-APT-12` | El precio de la cita debe conservar el valor vigente al reservar. | Must | Cambiar el precio del servicio no altera citas existentes. |
| `RF-APT-13` | El listado debe permitir filtrar por mascota, estado y rango de fechas. | Must | Los filtros se combinan correctamente. |
| `RF-APT-14` | Una cita completada o cancelada debe conservarse como historial. | Must | No existe eliminación física de citas. |

## 7. Requisitos funcionales de operación y documentación

| ID | Requisito | Prioridad | Criterio de aceptación resumido |
|---|---|---|---|
| `RF-OPS-01` | La API debe publicar el endpoint `/health`. | Must | Una aplicación y base de datos saludables devuelven `200 OK`. |
| `RF-OPS-02` | La API debe generar un documento OpenAPI. | Must | El documento describe rutas, modelos y códigos relevantes. |
| `RF-OPS-03` | La API debe mostrar Swagger UI. | Must | Los endpoints pueden explorarse y probarse desde la interfaz. |
| `RF-OPS-04` | Deben existir archivos `.http` para probar los endpoints. | Must | Hay archivos separados por módulo y sin secretos reales. |
| `RF-OPS-05` | Las operaciones importantes deben producir logs estructurados. | Must | Los logs usan plantillas y propiedades identificables. |
| `RF-OPS-06` | La API debe devolver errores mediante Problem Details. | Must | Errores controlados incluyen `status`, `title`, `detail`, `traceId` y `errorCode`. |
| `RF-OPS-07` | La aplicación debe crear los roles y datos iniciales de forma idempotente. | Must | Ejecutar el inicializador varias veces no duplica datos. |

## 8. Requisitos no funcionales

| ID | Requisito | Prioridad |
|---|---|---|
| `RNF-01` | La API debe desarrollarse con .NET 10. | Must |
| `RNF-02` | Los endpoints deben implementarse mediante Minimal APIs. | Must |
| `RNF-03` | La lógica de negocio debe permanecer fuera de los endpoints. | Must |
| `RNF-04` | Los servicios y repositorios deben registrarse mediante inyección de dependencias. | Must |
| `RNF-05` | El acceso a datos debe utilizar Entity Framework Core. | Must |
| `RNF-06` | La base de datos de desarrollo debe utilizar SQL Server. | Must |
| `RNF-07` | Producción debe utilizar Azure SQL Database. | Must |
| `RNF-08` | Las operaciones de entrada/salida deben ser asíncronas. | Must |
| `RNF-09` | Los métodos asíncronos deben aceptar `CancellationToken` cuando corresponda. | Must |
| `RNF-10` | La API no debe devolver directamente entidades de Entity Framework Core. | Must |
| `RNF-11` | El JSON debe utilizar propiedades en `camelCase`. | Must |
| `RNF-12` | Las enumeraciones deben exponerse como texto. | Must |
| `RNF-13` | Las fechas y horas deben almacenarse en UTC. | Must |
| `RNF-14` | Las contraseñas, tokens, secretos y cadenas de conexión no deben registrarse en logs. | Must |
| `RNF-15` | El tamaño máximo de página debe ser 100. | Must |
| `RNF-16` | La API debe poder ejecutarse mediante Docker. | Must |
| `RNF-17` | Los secretos no deben almacenarse en el repositorio Git. | Must |
| `RNF-18` | El frontend debe poder desplegarse independientemente del backend. | Must |
| `RNF-19` | Las consultas de lectura deben usar proyección y `AsNoTracking` cuando corresponda. | Should |
| `RNF-20` | La paginación debe utilizar un ordenamiento estable y completamente definido. | Must |
| `RNF-21` | La API debe utilizar HTTPS en producción. | Must |
| `RNF-22` | CORS debe permitir solamente orígenes configurados. | Must |
| `RNF-23` | Los errores inesperados no deben exponer stack traces al cliente. | Must |
| `RNF-24` | La solución debe contar con pruebas unitarias y de integración ejecutables con `dotnet test`. | Must |

## 9. Dependencias entre requisitos

- `RF-APT-01` depende de `RF-AUTH-04`, `RF-PET-01` y `RF-SVC-01`.
- `RF-APT-05` y `RF-APT-06` dependen de las reglas de estados de cita.
- `RF-SVC-03`, `RF-SVC-04` y `RF-SVC-05` dependen de la autenticación y del rol `Admin`.
- `RF-PET-02` a `RF-PET-07` dependen de que el JWT identifique al usuario mediante el claim `sub`.
- `RF-OPS-06` aplica transversalmente a todos los módulos.

## 10. Referencias relacionadas

- [Visión y alcance](01-vision-and-scope.md)
- [Reglas de negocio](03-business-rules.md)
- [Contrato de API](05-api-contract.md)
- [Plan de pruebas](07-test-plan.md)
- [Matriz de trazabilidad](08-requirements-traceability.md)
