# ADR-001: Arquitectura por capas para VetCare

- **Estado:** Aceptada
- **Fecha:** 2026-09-13
- **Responsable:** Proyecto VetCare
- **Decisión relacionada:** Estructura general de la solución

## 1. Contexto

VetCare debe implementar una API con .NET 10 y Minimal APIs que incluya endpoints HTTP, reglas de negocio, persistencia, autenticación, logging y pruebas.

Los lineamientos del bootcamp exigen una separación clara entre:

- Endpoints.
- Servicios o lógica de negocio.
- Acceso a datos.

Además, el proyecto debe ser suficientemente mantenible para incorporar un frontend y nuevas funciones en iteraciones posteriores, sin introducir la complejidad de microservicios, CQRS o patrones innecesarios para su tamaño inicial.

## 2. Decisión

VetCare utilizará un **monolito modular con arquitectura por capas**, inspirado en Clean Architecture y dividido en cuatro proyectos principales:

```text
VetCare.Api
VetCare.Application
VetCare.Domain
VetCare.Infrastructure
```

También tendrá dos proyectos de pruebas:

```text
VetCare.UnitTests
VetCare.IntegrationTests
```

### 2.1 Responsabilidades

#### `VetCare.Api`

- Minimal API endpoints.
- Binding HTTP.
- Contratos de entrada y salida HTTP.
- Configuración del pipeline.
- Problem Details.
- OpenAPI y Swagger UI.
- Autenticación/autorización del pipeline.
- Health checks.
- Composition root.

#### `VetCare.Application`

- Casos de uso.
- Servicios de aplicación.
- Reglas de negocio coordinadas.
- Interfaces de repositorios y servicios técnicos.
- Excepciones de aplicación.
- Paginación y modelos comunes.

#### `VetCare.Domain`

- Entidades.
- Enumeraciones.
- Constantes y reglas puras del dominio.
- Sin dependencias de ASP.NET Core, Identity o Entity Framework Core.

#### `VetCare.Infrastructure`

- Entity Framework Core.
- SQL Server.
- `VetCareDbContext`.
- Repositorios.
- ASP.NET Core Identity.
- Generación de JWT.
- Migraciones y seed.
- Integraciones técnicas futuras.

### 2.2 Dirección de dependencias

```text
VetCare.Api
    ├── VetCare.Application
    └── VetCare.Infrastructure

VetCare.Infrastructure
    ├── VetCare.Application
    └── VetCare.Domain

VetCare.Application
    └── VetCare.Domain

VetCare.Domain
    └── sin dependencias internas
```

La referencia de `VetCare.Api` a `VetCare.Infrastructure` se utiliza para registrar las implementaciones en el composition root. La lógica de negocio no depende directamente de infraestructura.

## 3. Flujo esperado

```text
PetEndpoints
    ↓
IPetService
    ↓
PetService
    ↓
IPetRepository
    ↓
PetRepository
    ↓
VetCareDbContext
    ↓
SQL Server
```

El endpoint no consulta directamente el `DbContext` ni contiene reglas de negocio.

## 4. Motivos

- Cumple explícitamente los lineamientos del profesor.
- Facilita pruebas unitarias de los servicios.
- Mantiene los endpoints pequeños.
- Permite sustituir infraestructura sin reescribir reglas de negocio.
- Evita acoplar el dominio a HTTP, EF Core o Identity.
- Es apropiada para una API de tamaño pequeño o medio.
- Permite evolucionar a módulos adicionales sin distribuir prematuramente el sistema.

## 5. Alternativas consideradas

### 5.1 Un único proyecto API

**Ventaja:** menor cantidad de proyectos y configuración inicial.

**Rechazada porque:** aumenta el riesgo de mezclar endpoints, entidades, acceso a datos y reglas de negocio; dificulta demostrar la separación exigida.

### 5.2 Microservicios

**Ventaja:** despliegue y escalamiento independientes por dominio.

**Rechazada porque:** introduce comunicación distribuida, observabilidad, despliegues múltiples, consistencia eventual y costos innecesarios para VetCare v1.

### 5.3 CQRS con MediatR

**Ventaja:** separa comandos y consultas.

**Rechazada para v1 porque:** no es un requisito y agregaría abstracciones antes de que el tamaño del sistema las justifique.

### 5.4 Arquitectura vertical por features en un solo proyecto

**Ventaja:** alta cohesión por funcionalidad.

**No seleccionada como estructura principal porque:** el bootcamp solicita demostrar explícitamente capas de endpoints, servicios y datos. Se podrá mantener organización interna por feature dentro de cada capa.

## 6. Consecuencias positivas

- Límites de responsabilidad claros.
- Servicios probables de forma aislada.
- Infraestructura reemplazable.
- Código organizado para la presentación final.
- Menor acoplamiento entre HTTP y persistencia.
- Facilita un frontend independiente.

## 7. Consecuencias negativas

- Más proyectos y referencias que una API simple.
- Requiere mapeos entre entidades, modelos de aplicación y DTOs HTTP.
- Puede producir archivos repetitivos si se agregan abstracciones sin necesidad.
- Exige disciplina para no filtrar dependencias hacia `Domain`.

## 8. Reglas de aplicación de la decisión

1. Ningún endpoint accede directamente al `DbContext`.
2. Ningún endpoint implementa reglas de negocio.
3. `Domain` no referencia ASP.NET Core, Identity ni EF Core.
4. `Application` define las abstracciones que necesita.
5. `Infrastructure` implementa las abstracciones técnicas.
6. Los DTOs HTTP no se utilizan como entidades persistentes.
7. No se añadirá un repositorio genérico si no aporta valor real.
8. No se incorporarán microservicios, CQRS o MediatR en VetCare v1.

## 9. Criterios de verificación

- La solución respeta el grafo de dependencias definido.
- Los servicios pueden instanciarse con dependencias sustituidas en pruebas.
- Los endpoints son delgados y legibles.
- `dotnet build` compila todos los proyectos.
- Las reglas principales se prueban sin iniciar la API.

## 10. Referencias

- [Visión y alcance](../01-vision-and-scope.md)
- [Requisitos](../02-functional-requirements.md)
- [Definición de terminado](../09-definition-of-done.md)
