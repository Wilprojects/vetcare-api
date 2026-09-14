# ADR-003: ASP.NET Core Identity y JWT Bearer estándar

- **Estado:** Aceptada
- **Fecha:** 2026-09-13
- **Responsable:** Proyecto VetCare
- **Decisión relacionada:** Autenticación y autorización

## 1. Contexto

El proyecto final exige:

- Registro con ASP.NET Core Identity.
- Login que emita un token.
- JWT en las peticiones.
- Al menos un endpoint protegido.
- Roles, claims y políticas.

VetCare necesita autenticar clientes y administradores, proteger recursos propios y restringir operaciones administrativas.

## 2. Decisión

VetCare utilizará:

```text
ASP.NET Core Identity
    → usuarios, contraseñas y roles

Servicio propio de generación JWT
    → access token JWT estándar

Autenticación JwtBearer
    → validación del token en cada petición protegida
```

El login se implementará mediante un caso de uso propio que valida credenciales con Identity y delega la creación del token a `IJwtTokenGenerator`.

## 3. Roles

```text
Customer
Admin
```

Reglas:

- El registro público asigna `Customer`.
- El request de registro no contiene una propiedad `role`.
- El administrador inicial se crea mediante seed seguro.
- Las operaciones administrativas utilizan la política `AdminOnly`.

## 4. Claims del token

| Claim | Uso |
|---|---|
| `sub` | Identificar al usuario y aplicar propiedad. |
| `email` | Identificación básica. |
| `role` | Autorizar operaciones por rol. |
| `jti` | Identificador único del token. |
| `iat` | Fecha de emisión. |
| `exp` | Expiración. |
| `iss` | Validación de emisor. |
| `aud` | Validación de audiencia. |

## 5. Validación

JwtBearer validará:

- Firma.
- Clave.
- Emisor.
- Audiencia.
- Expiración.
- Formato.

Duración inicial:

```text
60 minutos
```

Los refresh tokens no se implementarán en VetCare v1.

## 6. Políticas

```text
AuthenticatedUser
AdminOnly
```

Las rutas de mascotas y citas de clientes requieren autenticación. Las rutas administrativas requieren `AdminOnly`.

La propiedad de una mascota o cita se verifica adicionalmente dentro de los casos de uso; estar autenticado no concede acceso a todos los recursos.

## 7. Motivos

- Cumple todos los requisitos del bootcamp.
- Identity evita implementar almacenamiento y hash de contraseñas manualmente.
- JWT permite que un frontend independiente consuma la API.
- Los roles y claims se integran con ASP.NET Core Authorization.
- La generación detrás de una interfaz facilita pruebas y mantenimiento.
- Un JWT estándar puede ser utilizado por clientes web o móviles.

## 8. Alternativas consideradas

### 8.1 Cookies de autenticación

**Ventaja:** adecuadas para aplicaciones web tradicionales servidas desde el mismo backend.

**Rechazada como mecanismo principal porque:** el proyecto exige JWT y el frontend será independiente.

### 8.2 Tokens propietarios de endpoints de Identity

**Ventaja:** menor implementación manual.

**No seleccionados como contrato principal porque:** el lineamiento solicita explícitamente JWT y se desea controlar claims, emisor, audiencia y expiración mediante un token JWT estándar.

### 8.3 Servicio externo de identidad

Ejemplos: Microsoft Entra ID, Auth0 u otro proveedor.

**Ventaja:** funciones avanzadas administradas.

**Rechazada para v1 porque:** el bootcamp exige practicar ASP.NET Core Identity y aumentaría dependencias externas.

### 8.4 Implementar usuarios y contraseñas manualmente

**Rechazada porque:** es insegura, innecesaria y contradice el uso solicitado de Identity.

## 9. Consecuencias positivas

- Contraseñas administradas por una solución probada.
- Roles y claims integrados.
- Frontend desacoplado.
- Endpoints protegidos con middleware estándar.
- Pruebas de autorización claras.

## 10. Consecuencias negativas

- La revocación inmediata de un access token ya emitido no está incluida en v1.
- Un token robado puede usarse hasta expirar.
- La gestión de claves de firma requiere disciplina.
- La implementación futura de refresh tokens agregará persistencia y rotación.

## 11. Mitigaciones

- Expiración corta de 60 minutos.
- HTTPS obligatorio.
- Clave JWT fuera del repositorio.
- No registrar tokens.
- Validar todos los parámetros del token.
- No incluir datos sensibles.
- Incorporar refresh tokens y revocación en una versión futura si el producto lo requiere.

## 12. Criterios de verificación

- Un usuario puede registrarse con rol `Customer`.
- Un login válido produce un JWT legible como JWT estándar.
- El token contiene los claims previstos.
- Sin token, una ruta protegida devuelve `401`.
- Customer en una ruta Admin recibe `403`.
- Un cliente no puede acceder a recursos de otro cliente.
- Un token expirado o mal firmado es rechazado.
- La clave no aparece en Git ni en logs.

## 13. Referencias

- [Modelo de seguridad](../06-security-model.md)
- [Contrato de API](../05-api-contract.md)
- [Plan de pruebas](../07-test-plan.md)
