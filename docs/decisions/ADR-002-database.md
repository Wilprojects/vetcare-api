# ADR-002: SQL Server en desarrollo y Azure SQL Database en producción

- **Estado:** Aceptada
- **Fecha:** 2026-09-13
- **Responsable:** Proyecto VetCare
- **Decisión relacionada:** Persistencia principal

## 1. Contexto

El lineamiento del bootcamp recomienda SQLite, pero permite utilizar cualquier base de datos. VetCare pretende ser una API multiusuario preparada para un frontend futuro y para un despliegue real en Azure.

El sistema administra operaciones de escritura potencialmente concurrentes, especialmente:

- Creación de citas.
- Reprogramación.
- Cancelación.
- Confirmación por administradores.
- Registro y actualización de mascotas.

SQLite sería suficiente para una demostración pequeña o una aplicación de una sola instancia, pero su modelo basado en archivo y su capacidad limitada de escrituras concurrentes representan restricciones para la evolución prevista.

## 2. Decisión

VetCare utilizará:

```text
Desarrollo local: SQL Server Developer o SQL Server en Docker
Pruebas de integración: instancia SQL Server aislada
Producción: Azure SQL Database
ORM: Entity Framework Core para SQL Server
```

Paquete de proveedor previsto:

```text
Microsoft.EntityFrameworkCore.SqlServer
```

Configuración conceptual:

```csharp
options.UseSqlServer(connectionString);
```

## 3. Modelo de ejecución

### 3.1 Desarrollo

```text
VetCare.Api
    ↓ conexión TCP
SQL Server local o contenedor
```

### 3.2 Producción

```text
VetCare API en Azure
    ↓ conexión protegida
Azure SQL Database
```

## 4. Motivos

- Mejor soporte para múltiples escrituras concurrentes.
- Adecuado para una API web multiusuario.
- Integración directa con Entity Framework Core.
- Soporte nativo de `decimal` para importes.
- Herramientas de administración y diagnóstico maduras.
- Azure SQL Database proporciona una opción administrada para producción.
- Permite escalar la API sin depender de un archivo local compartido.
- Alinea el entorno de desarrollo con el motor productivo.

## 5. Decisiones derivadas

### 5.1 Importes

Los precios utilizarán:

```text
.NET: decimal
SQL Server: decimal(10,2)
```

No se utilizarán `float` ni `double` para valores monetarios.

### 5.2 Fechas

- Fechas y horas: `DateTime` en UTC y `datetime2`.
- Fechas sin hora: `DateOnly` y `date`.

### 5.3 Migraciones

- Las migraciones pertenecerán a `VetCare.Infrastructure`.
- Solo se mantendrá el historial oficial para SQL Server.
- Las pruebas de integración aplicarán las migraciones.

### 5.4 Pruebas

No se utilizará SQLite ni EF Core InMemory como sustituto principal de SQL Server para pruebas de integración, porque podrían ocultar diferencias de consultas, tipos y restricciones.

### 5.5 Docker

`docker-compose.yml` podrá iniciar:

```text
vetcare-api
vetcare-sqlserver
```

SQL Server utilizará un volumen persistente durante desarrollo.

## 6. Alternativas consideradas

### 6.1 SQLite

**Ventajas:** configuración mínima, base en un archivo, adecuada para aprendizaje y demostraciones.

**Rechazada como base principal porque:** VetCare está orientada a una API web multiusuario, posible escalamiento y publicación productiva. Un archivo SQLite también complica el despliegue de varias instancias.

SQLite sigue siendo una tecnología válida para otros escenarios; la decisión no implica que sea inadecuada en general.

### 6.2 PostgreSQL

**Ventajas:** motor robusto, libre y con excelente soporte de concurrencia.

**No seleccionado porque:** SQL Server se integra naturalmente con el ecosistema .NET/Azure elegido y facilita el objetivo formativo del proyecto.

### 6.3 SQL Server instalado únicamente en Windows

**Ventaja:** uso directo de SQL Server Developer/LocalDB.

**No seleccionado como única opción porque:** Docker permite un entorno reproducible en diferentes equipos y en CI.

## 7. Consecuencias positivas

- Arquitectura más cercana a producción.
- Mejor comportamiento para operaciones concurrentes.
- Tipos monetarios apropiados.
- Azure SQL ofrece backups y administración del servicio.
- Entorno de integración fiel a producción.

## 8. Consecuencias negativas

- Mayor consumo de recursos local que SQLite.
- Requiere instalación o contenedor.
- La configuración inicial es más extensa.
- Azure SQL puede generar costos.
- Las pruebas de integración requieren una instancia SQL Server disponible.

## 9. Riesgos y mitigaciones

| Riesgo | Mitigación |
|---|---|
| SQL Server no está instalado | Proporcionar Docker Compose. |
| Contraseña expuesta | Variables de entorno y Secret Manager. |
| Diferencias entre local y Azure | Usar el mismo proveedor EF Core. |
| Costos en Azure | Elegir un nivel apropiado y eliminar recursos al finalizar la demostración si corresponde. |
| Conflictos de reserva concurrente | Validación, transacción y manejo de conflictos en la capa de aplicación. |

## 10. Criterios de verificación

- La API utiliza `UseSqlServer`.
- La migración inicial se aplica desde cero.
- Docker Compose inicia SQL Server y la API.
- Las pruebas de integración usan SQL Server.
- La aplicación publicada se conecta a Azure SQL Database.
- Ninguna cadena de conexión real está versionada.

## 11. Referencias

- [Modelo de datos](../04-data-model.md)
- [Plan de pruebas](../07-test-plan.md)
- [Modelo de seguridad](../06-security-model.md)
