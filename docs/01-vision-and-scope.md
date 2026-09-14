# VetCare — Visión y alcance

> **Estado:** Aprobado para la versión 1  
> **Versión del documento:** 1.0  
> **Última actualización:** 2026-09-13

## 1. Resumen del producto

**VetCare** es una API REST desarrollada con .NET 10 para gestionar usuarios, mascotas, servicios veterinarios y citas.

La primera versión permitirá que los propietarios de mascotas se registren, inicien sesión, administren sus mascotas, consulten los servicios disponibles y programen citas. Los administradores podrán mantener el catálogo de servicios y controlar el ciclo de vida de las citas.

La API se diseñará como un backend independiente, preparado para ser consumido posteriormente por una aplicación web, móvil o panel administrativo.

## 2. Problema que resuelve

Muchas clínicas veterinarias pequeñas administran sus citas mediante llamadas telefónicas, mensajes de mensajería instantánea, hojas de cálculo o registros manuales. Este proceso puede causar:

- Duplicidad o superposición de horarios.
- Pérdida o dispersión de información.
- Dificultad para identificar las mascotas de cada cliente.
- Falta de seguimiento del estado de las citas.
- Catálogos de servicios desactualizados.
- Dependencia de procesos manuales para confirmar o cancelar atenciones.

VetCare centralizará estas operaciones y expondrá una API segura y documentada que podrá integrarse con diferentes interfaces de usuario.

## 3. Visión

> Proporcionar una base tecnológica segura, mantenible y extensible para digitalizar la gestión de mascotas, servicios y citas de una clínica veterinaria.

## 4. Objetivo general

Construir una API backend segura y mantenible utilizando .NET 10, Minimal APIs, Entity Framework Core, ASP.NET Core Identity, JWT, SQL Server, pruebas automatizadas, Docker y Azure.

## 5. Objetivos específicos

1. Permitir el registro y autenticación de usuarios.
2. Garantizar que cada cliente administre únicamente sus propias mascotas.
3. Publicar un catálogo consultable de servicios veterinarios activos.
4. Permitir la creación, consulta, reprogramación y cancelación de citas.
5. Evitar reservas con horarios superpuestos.
6. Permitir que un administrador mantenga servicios y estados de citas.
7. Devolver errores consistentes mediante Problem Details.
8. Documentar la API con OpenAPI y Swagger UI.
9. Incorporar logging estructurado y un endpoint de salud.
10. Implementar pruebas unitarias y de integración.
11. Ejecutar la aplicación mediante Docker.
12. Publicar la solución en Azure.
13. Dejar el backend preparado para un frontend futuro.

## 6. Actores

### 6.1 Usuario anónimo

Persona que todavía no ha iniciado sesión.

Puede:

- Registrarse como cliente.
- Iniciar sesión.
- Consultar los servicios veterinarios activos.
- Consultar el estado general de la API mediante `/health`.

### 6.2 Cliente

Usuario autenticado con el rol `Customer`.

Puede:

- Consultar su información de usuario.
- Registrar mascotas.
- Consultar sus mascotas.
- Actualizar sus mascotas.
- Desactivar sus mascotas.
- Consultar servicios veterinarios activos.
- Crear citas para sus propias mascotas.
- Consultar sus citas.
- Reprogramar citas permitidas.
- Cancelar citas permitidas.

### 6.3 Administrador

Usuario autenticado con el rol `Admin`.

Puede:

- Crear servicios veterinarios.
- Actualizar servicios veterinarios.
- Desactivar servicios veterinarios.
- Consultar servicios activos e inactivos.
- Consultar todas las citas.
- Confirmar citas.
- Completar citas.
- Cancelar citas cuando corresponda.

El administrador no se considera automáticamente propietario de las mascotas. Su acceso administrativo en la versión 1 se concentra en los servicios y las citas.

## 7. Matriz resumida de permisos

| Funcionalidad | Anónimo | Customer | Admin |
|---|:---:|:---:|:---:|
| Registrarse | Sí | No necesario | No necesario |
| Iniciar sesión | Sí | Sí | Sí |
| Consultar servicios activos | Sí | Sí | Sí |
| Consultar su perfil | No | Sí | Sí |
| Crear mascota | No | Sí | No como propietario |
| Consultar mascotas propias | No | Sí | No automáticamente |
| Actualizar mascotas propias | No | Sí | No automáticamente |
| Crear cita | No | Sí | No como cliente |
| Consultar citas propias | No | Sí | No aplica |
| Consultar todas las citas | No | No | Sí |
| Administrar servicios | No | No | Sí |
| Cambiar el estado de una cita | No | No | Sí |

## 8. Alcance de VetCare v1

La primera versión incluirá:

- Registro con ASP.NET Core Identity.
- Login y emisión de JWT estándar.
- Roles `Customer` y `Admin`.
- Consulta del usuario autenticado.
- CRUD completo de mascotas.
- CRUD administrativo de servicios veterinarios.
- Consulta pública de servicios activos.
- Creación de citas.
- Consulta paginada de mascotas, servicios y citas.
- Reprogramación de citas.
- Cancelación de citas.
- Gestión administrativa de estados.
- Validación de horarios superpuestos.
- Filtros y ordenamiento controlado.
- Problem Details para errores.
- Logging estructurado con `ILogger<T>`.
- OpenAPI y Swagger UI.
- Endpoint `/health`.
- Archivos `.http`.
- Pruebas unitarias.
- Pruebas de integración.
- Contenerización con Docker.
- Publicación en Azure.
- Repositorio público en GitHub.
- README y video demostrativo.

## 9. Fuera del alcance de VetCare v1

Las siguientes funcionalidades se consideran mejoras futuras:

- Frontend web.
- Aplicación móvil.
- Historias clínicas.
- Registro de vacunas y tratamientos.
- Gestión de veterinarios.
- Asignación de citas por veterinario.
- Múltiples sucursales.
- Pagos en línea.
- Facturación.
- Notificaciones por correo o WhatsApp.
- Recuperación de contraseña.
- Confirmación de correo.
- Refresh tokens.
- Carga de imágenes.
- Horarios configurables por sede.
- Integraciones con servicios externos.
- Reportes analíticos.

## 10. Supuestos y restricciones

- La versión 1 representa una sola clínica.
- La clínica atiende una sola cita a la vez; no existe todavía la entidad `Veterinarian`.
- Todas las fechas y horas se almacenan en UTC.
- El frontend será un proyecto independiente.
- SQL Server se utilizará en desarrollo y Azure SQL Database en producción.
- Los clientes no pueden elegir ni alterar su rol.
- Las citas, mascotas y servicios históricos no se eliminan físicamente.
- La API utilizará rutas versionadas con el prefijo `/api/v1`.
- La versión 1 no requiere refresh tokens.

## 11. Criterios de éxito

VetCare v1 se considerará exitosa cuando:

- Cumpla todos los lineamientos del proyecto final del bootcamp.
- Permita completar el flujo registro → login → mascota → cita.
- Impida acceder a recursos pertenecientes a otro cliente.
- Impida citas pasadas o superpuestas.
- Aplique correctamente roles y políticas.
- Devuelva códigos HTTP y Problem Details consistentes.
- Sus pruebas automatizadas se ejecuten correctamente.
- Pueda iniciarse localmente y mediante Docker.
- Esté publicada en Azure y conectada a Azure SQL Database.
- Una persona pueda ejecutar el proyecto siguiendo únicamente el README.

## 12. Evolución prevista

Una versión posterior podrá incorporar veterinarios, disponibilidad por profesional, historias clínicas, vacunas, notificaciones, pagos, múltiples sedes y un frontend web o móvil.

## 13. Glosario

| Término | Definición |
|---|---|
| Cliente | Usuario con rol `Customer` y propietario de mascotas. |
| Administrador | Usuario con rol `Admin` encargado de servicios y citas. |
| Mascota | Animal registrado por un cliente. |
| Servicio veterinario | Atención ofrecida por la clínica, con duración y precio. |
| Cita | Reserva de un servicio para una mascota en una fecha y hora. |
| Eliminación lógica | Desactivación del registro sin borrarlo físicamente. |
| JWT | Token firmado utilizado para autenticar peticiones. |
| Problem Details | Formato estándar de respuesta para errores HTTP. |
