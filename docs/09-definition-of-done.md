# VetCare — Definición de terminado

> **Estado:** Aprobado para la versión 1  
> **Versión del documento:** 1.0

## 1. Propósito

Establecer las condiciones objetivas que debe cumplir una funcionalidad, una fase y la versión completa de VetCare para considerarse terminadas.

## 2. Definición de terminado por funcionalidad

Una funcionalidad se considera terminada cuando cumple todos los puntos aplicables.

### 2.1 Diseño y alcance

```text
[ ] Está asociada a un requisito identificado.
[ ] Sus reglas de negocio están documentadas.
[ ] Su contrato HTTP está definido.
[ ] No agrega funcionalidades fuera del alcance sin documentarlas.
```

### 2.2 Arquitectura

```text
[ ] El endpoint se encuentra en VetCare.Api.
[ ] La lógica de negocio se encuentra en VetCare.Application.
[ ] Las entidades y enums corresponden a VetCare.Domain.
[ ] La persistencia y servicios técnicos corresponden a VetCare.Infrastructure.
[ ] Las dependencias respetan la dirección de la arquitectura.
[ ] No hay consultas directas al DbContext dentro del endpoint.
```

### 2.3 Código

```text
[ ] Compila sin errores.
[ ] No introduce advertencias evitables.
[ ] Nullable reference types están respetados.
[ ] Usa nombres claros y consistentes.
[ ] Las operaciones de entrada/salida son asíncronas.
[ ] Acepta CancellationToken cuando corresponde.
[ ] No contiene código muerto o comentado innecesario.
[ ] No duplica lógica existente.
```

### 2.4 Inyección de dependencias

```text
[ ] La lógica se expone mediante una interfaz cuando corresponde.
[ ] La implementación está registrada en DI.
[ ] El ciclo de vida elegido es adecuado.
[ ] El servicio puede probarse de forma aislada.
```

### 2.5 API y DTOs

```text
[ ] Usa DTOs de entrada y salida.
[ ] No devuelve entidades de EF Core directamente.
[ ] Realiza binding correctamente.
[ ] Devuelve el código HTTP adecuado.
[ ] Usa TypedResults o resultados claramente definidos.
[ ] El recurso creado devuelve 201 y Location cuando corresponde.
[ ] Una operación sin cuerpo devuelve 204 cuando corresponde.
```

### 2.6 Validación y errores

```text
[ ] Valida el formato y campos obligatorios.
[ ] Valida reglas de negocio en el servicio.
[ ] Los errores usan Problem Details.
[ ] El error incluye traceId y errorCode cuando corresponde.
[ ] No expone stack trace, SQL ni detalles internos.
[ ] Los conflictos de negocio usan 409 cuando corresponde.
```

### 2.7 Seguridad

```text
[ ] Aplica autenticación cuando corresponde.
[ ] Aplica rol o política cuando corresponde.
[ ] Verifica la propiedad del recurso.
[ ] No confía en OwnerId enviado por el cliente.
[ ] Un recurso ajeno no revela su existencia.
[ ] No registra contraseñas, tokens ni secretos.
```

### 2.8 Persistencia

```text
[ ] La configuración EF Core está definida.
[ ] Las relaciones y restricciones son correctas.
[ ] Las consultas de lectura usan AsNoTracking cuando corresponde.
[ ] La consulta evita problemas N+1.
[ ] La paginación tiene un orden estable.
[ ] Existe una migración cuando cambia el esquema.
[ ] La migración fue aplicada y probada.
```

### 2.9 Logging

```text
[ ] Registra los eventos importantes con ILogger<T>.
[ ] Usa logging estructurado con placeholders.
[ ] Emplea el nivel adecuado.
[ ] No concatena información sensible.
[ ] Los identificadores necesarios son trazables.
```

### 2.10 Documentación

```text
[ ] El endpoint aparece en OpenAPI.
[ ] Swagger muestra request, response y códigos relevantes.
[ ] Existe una petición equivalente en un archivo .http.
[ ] La documentación funcional se actualizó si hubo cambios.
[ ] Los ejemplos no contienen secretos reales.
```

### 2.11 Pruebas

```text
[ ] Tiene prueba unitaria cuando contiene reglas de negocio.
[ ] Tiene prueba de integración del flujo HTTP principal.
[ ] Incluye al menos un caso negativo relevante.
[ ] Las pruebas son deterministas e independientes.
[ ] Todas las pruebas pasan con dotnet test.
```

### 2.12 Control de versiones

```text
[ ] Los cambios están incluidos en commits claros.
[ ] No se versionan bin, obj, secretos ni archivos locales.
[ ] El nombre del commit describe la funcionalidad.
[ ] El repositorio puede clonarse y compilarse limpiamente.
```

## 3. Definición de terminado por módulo

### 3.1 Autenticación

```text
[ ] Registro crea usuarios Customer.
[ ] Registro rechaza correos duplicados.
[ ] Identity aplica la política de contraseña.
[ ] Login válido emite JWT.
[ ] Login inválido devuelve 401.
[ ] JWT incluye claims y expiración correctos.
[ ] /auth/me devuelve el usuario autenticado.
[ ] Customer no accede a rutas Admin.
[ ] Swagger permite enviar Bearer token.
[ ] Existen pruebas unitarias/integración aplicables.
```

### 3.2 Mascotas

```text
[ ] GET listado implementado y paginado.
[ ] GET por ID implementado.
[ ] POST implementado.
[ ] PUT implementado.
[ ] DELETE lógico implementado.
[ ] Búsqueda, filtros y ordenamiento funcionan.
[ ] Un cliente solo accede a sus mascotas.
[ ] No se desactiva una mascota con citas futuras activas.
[ ] Todos los resultados y errores están documentados.
[ ] Pruebas unitarias e integración pasan.
```

### 3.3 Servicios veterinarios

```text
[ ] Catálogo público muestra solo activos.
[ ] Admin puede consultar activos e inactivos.
[ ] POST, PUT y DELETE lógico requieren Admin.
[ ] Nombre duplicado devuelve 409.
[ ] Precio y duración se validan.
[ ] Desactivar no altera citas históricas.
[ ] Paginación, filtros y ordenamiento funcionan.
[ ] Pruebas unitarias e integración pasan.
```

### 3.4 Citas

```text
[ ] Customer crea cita para mascota propia.
[ ] Servicio y mascota deben estar activos.
[ ] Inicio debe estar en el futuro.
[ ] Fin se calcula con la duración.
[ ] Precio se copia como valor histórico.
[ ] Se detectan superposiciones.
[ ] Customer lista solo sus citas.
[ ] Reprogramación respeta estados y conflictos.
[ ] Cancelación respeta propiedad y estados.
[ ] Admin lista todas las citas.
[ ] Admin confirma y completa mediante transiciones válidas.
[ ] Citas completadas/canceladas conservan historial.
[ ] Pruebas unitarias e integración pasan.
```

## 4. Definición de terminado de la base de datos

```text
[ ] SQL Server está configurado mediante EF Core.
[ ] VetCareDbContext incluye Identity y entidades funcionales.
[ ] Relaciones y DeleteBehavior están configurados.
[ ] Índices previstos están creados.
[ ] Decimales tienen precisión explícita.
[ ] Enums se almacenan de forma consistente.
[ ] Existe la migración inicial.
[ ] La base se crea desde cero aplicando migraciones.
[ ] El seed crea roles sin duplicarlos.
[ ] El seed crea el administrador usando secretos.
[ ] El seed de servicios es idempotente.
```

## 5. Definición de terminado de pruebas

```text
[ ] Existe VetCare.UnitTests.
[ ] Existe VetCare.IntegrationTests.
[ ] La superposición de citas tiene prueba unitaria.
[ ] GET /api/v1/pets autenticado tiene prueba de integración.
[ ] Hay casos 400, 401, 403, 404 y 409.
[ ] Las pruebas de integración usan SQL Server aislado.
[ ] No dependen de secretos del desarrollador.
[ ] dotnet test finaliza con código 0.
[ ] El pipeline ejecuta las pruebas.
```

## 6. Definición de terminado de Docker

```text
[ ] Existe Dockerfile multietapa.
[ ] Existe .dockerignore.
[ ] Existe docker-compose.yml.
[ ] API y SQL Server pueden iniciarse juntos.
[ ] La base de datos usa un volumen persistente.
[ ] La API espera la disponibilidad de la base cuando sea necesario.
[ ] Los secretos se pasan mediante variables de entorno.
[ ] /health funciona dentro del contenedor.
[ ] Swagger y endpoints funcionan desde el host.
[ ] La imagen no contiene archivos innecesarios ni secretos.
```

## 7. Definición de terminado de Azure

```text
[ ] Existe un grupo de recursos definido para VetCare.
[ ] La API está desplegada en Azure.
[ ] Azure SQL Database está creada y accesible por la API.
[ ] La cadena de conexión está en Application Settings.
[ ] La clave JWT está en configuración segura.
[ ] HTTPS está habilitado.
[ ] /health responde desde la URL pública.
[ ] Registro y login funcionan en Azure.
[ ] Los endpoints protegidos validan JWT.
[ ] Los logs pueden consultarse.
[ ] Swagger está habilitado solo según configuración.
```

## 8. Definición de terminado de documentación y entrega

```text
[ ] README describe el problema y las funciones.
[ ] README incluye arquitectura y tecnologías.
[ ] README explica requisitos previos.
[ ] README explica secretos y configuración.
[ ] README explica migraciones y ejecución local.
[ ] README explica Docker.
[ ] README explica pruebas.
[ ] README incluye URL de Azure.
[ ] README incluye enlace al video.
[ ] La carpeta docs contiene documentación actualizada.
[ ] Los diagramas se renderizan correctamente.
[ ] Los archivos .http están completos.
[ ] El repositorio es público.
[ ] No existen secretos en el historial visible del repositorio.
[ ] Se crea un tag o release v1.0.0.
```

## 9. Definición de terminado de VetCare v1

VetCare v1 estará terminado cuando:

1. La solución compile con .NET 10.
2. Las migraciones creen correctamente la base de datos.
3. Identity permita registrar usuarios.
4. El login emita un JWT válido.
5. Los endpoints protegidos requieran autenticación.
6. Los roles `Customer` y `Admin` funcionen.
7. Mascotas tenga CRUD completo.
8. Servicios tenga CRUD completo.
9. Las citas validen fechas, propiedad, estado y conflictos.
10. Los errores utilicen Problem Details.
11. Exista paginación, filtrado y ordenamiento.
12. Swagger documente todos los endpoints.
13. Los archivos `.http` permitan probar la API.
14. `/health` responda correctamente.
15. Los logs sean estructurados.
16. Las pruebas unitarias pasen.
17. Las pruebas de integración pasen.
18. Docker ejecute la aplicación y SQL Server.
19. La API esté desplegada en Azure con Azure SQL Database.
20. El repositorio GitHub sea público.
21. El README permita reproducir el proyecto.
22. El video demuestre las funciones principales.
23. No existan secretos versionados.
24. La matriz de trazabilidad no tenga requisitos obligatorios pendientes.

## 10. Aprobación de cierre

Antes de declarar la versión completa se realizará una revisión final de:

- Funcionalidad.
- Arquitectura.
- Seguridad.
- Persistencia.
- Pruebas.
- Docker.
- Azure.
- Documentación.
- Repositorio.
- Video.

Cualquier incumplimiento obligatorio devuelve el elemento al estado **En progreso**.
