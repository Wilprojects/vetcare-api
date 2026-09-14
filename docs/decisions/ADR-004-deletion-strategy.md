# ADR-004: Eliminación lógica de mascotas y servicios; cancelación de citas

- **Estado:** Aceptada
- **Fecha:** 2026-09-13
- **Responsable:** Proyecto VetCare
- **Decisión relacionada:** Conservación de historial

## 1. Contexto

VetCare administra información relacionada:

```text
Usuario → Mascotas → Citas ← Servicios
```

Eliminar físicamente una mascota o servicio podría romper referencias o eliminar contexto necesario para comprender citas históricas. Las citas también representan eventos de negocio que deben conservarse aunque sean cancelados.

El proyecto necesita exponer operaciones HTTP `DELETE` para cumplir un CRUD completo, pero esto no obliga a borrar filas físicamente.

## 2. Decisión

### 2.1 Mascotas

```text
DELETE /api/v1/pets/{id}
    → Pet.IsActive = false
```

### 2.2 Servicios veterinarios

```text
DELETE /api/v1/veterinary-services/{id}
    → VeterinaryService.IsActive = false
```

### 2.3 Citas

Las citas no tendrán eliminación física. Se cancelarán:

```text
Appointment.Status = Cancelled
Appointment.CancellationReason = motivo
```

### 2.4 Relaciones

Las claves foráneas utilizarán un comportamiento restrictivo (`Restrict`/`NoAction`) para evitar eliminaciones en cascada accidentales.

## 3. Reglas asociadas

- Una mascota inactiva no puede utilizarse para nuevas citas.
- Un servicio inactivo no puede utilizarse para nuevas citas.
- Una mascota con citas futuras `Pending` o `Confirmed` no puede desactivarse.
- Desactivar un servicio no modifica las citas existentes.
- Las consultas públicas excluyen servicios inactivos.
- El listado predeterminado de mascotas muestra activas, con opción controlada para consultar inactivas propias.
- Una cita cancelada no puede reactivarse en VetCare v1.
- Las citas completadas y canceladas permanecen disponibles como historial.

## 4. Motivos

- Conserva la trazabilidad del negocio.
- Evita claves foráneas huérfanas.
- Permite explicar citas históricas con su mascota y servicio.
- Reduce el riesgo de pérdida accidental.
- Mantiene un CRUD HTTP completo sin sacrificar historial.
- Facilita futuras funciones como reportes o historias clínicas.

## 5. Alternativas consideradas

### 5.1 Eliminación física en cascada

**Ventaja:** simplifica la limpieza de datos.

**Rechazada porque:** una sola operación podría eliminar citas y destruir información histórica.

### 5.2 Eliminación física con restricción

**Ventaja:** impide borrar registros con relaciones.

**No seleccionada como comportamiento funcional porque:** impediría al usuario retirar una mascota o servicio del uso activo sin un mecanismo adicional.

### 5.3 Tabla histórica o archivado separado

**Ventaja:** separa datos operativos de históricos.

**Rechazada para v1 porque:** agrega procesos y modelos innecesarios para el volumen previsto.

### 5.4 Filtro global de EF Core para `IsActive`

**Ventaja:** excluye inactivos automáticamente.

**No se adopta como obligación general porque:** las rutas administrativas e históricas necesitan consultar inactivos. Se evaluará por entidad evitando filtros difíciles de ignorar accidentalmente.

## 6. Consecuencias positivas

- Historial consistente.
- Menos riesgo de pérdida.
- Relaciones preservadas.
- Operaciones DELETE fáciles de explicar.
- Posibilidad futura de reactivación administrativa de mascotas o servicios, si se define.

## 7. Consecuencias negativas

- Las tablas acumulan registros inactivos.
- Todas las consultas deben considerar explícitamente el estado activo.
- Un nombre único de servicio inactivo puede impedir reutilizar el mismo nombre, según la restricción elegida.
- Se requiere diferenciar eliminación HTTP de eliminación física en la documentación.

## 8. Decisión sobre nombres de servicios inactivos

En VetCare v1 el nombre de servicio continuará siendo único incluso si el registro está inactivo. Para volver a ofrecer el mismo servicio se deberá actualizar/reactivar el registro existente en una mejora futura o modificar su nombre de forma administrativa.

Esta regla evita duplicados históricos ambiguos.

## 9. Respuestas HTTP

| Operación | Resultado |
|---|---|
| Desactivar mascota correctamente | `204 No Content` |
| Desactivar servicio correctamente | `204 No Content` |
| Cancelar cita correctamente | `204 No Content` |
| Mascota con citas futuras activas | `409 Conflict` |
| Recurso inexistente o ajeno | `404 Not Found` |
| Transición de cita inválida | `409 Conflict` |

## 10. Criterios de verificación

- Después de `DELETE`, la fila de mascota o servicio sigue en la base de datos.
- `IsActive` cambia a `false`.
- El catálogo público no muestra servicios inactivos.
- No se puede crear una cita con una mascota o servicio inactivo.
- Las citas históricas conservan sus claves foráneas.
- Cancelar una cita no elimina el registro.
- Las migraciones no configuran cascadas destructivas.

## 11. Referencias

- [Reglas de negocio](../03-business-rules.md)
- [Modelo de datos](../04-data-model.md)
- [Contrato de API](../05-api-contract.md)
