# Sentinel MVP: explicación simple

> Los datos del ejemplo (empresa, personas, números, fechas y requisitos) son **ficticios**. El contenido real del catálogo sale de la normativa vigente de ARSA.

## 1. Idea general

Sentinel es un sistema experto. Tiene dos partes:

- **Base de conocimiento (global):** qué documentos existen, qué requisitos pide cada uno, de cuáles depende y cuándo aplican. La siembran los desarrolladores.
- **Motor:** cuando la empresa registra un producto o establecimiento, decide qué documento le toca. Si el usuario no tiene ese documento, crea la lista de requisitos pendientes.

El sistema le dice a la empresa: qué documento necesita, qué le falta y qué está bloqueado.

## 2. Las tablas

### Datos de cada empresa (llevan `EmpresaId`)

| Tabla               | Qué guarda                                                                                                                                                                                                                                                                      |
| ------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| EMPRESA             | El cliente. Raíz del aislamiento multi-tenant.                                                                                                                                                                                                                                  |
| USUARIO             | Persona que inicia sesión y cumple los requisitos. En el MVP cada empresa tiene **un solo usuario**. Tiene nombre y activo.                                                                                                                                                     |
| ESTABLECIMIENTO     | Local físico. Su tipo (catálogo `TIPO_ESTABLECIMIENTO`: Farmacia, Droguería, Bodega) define qué licencia necesita.                                                                                                                                                              |
| LICENCIA_SANITARIA  | Licencia del establecimiento. Se crea junto con él, con el tipo de documento que le toca. Número y fechas son NULL hasta que el usuario los registre. Guarda su estado en `EstadoVencimientoId`.                                                                                 |
| PRODUCTO            | Datos generales del producto. Su categoría (catálogo `CATEGORIA_PRODUCTO`) define qué registro necesita. **No guarda** número, fechas ni estado: eso vive en `REGISTRO_SANITARIO`.                                                                                              |
| REGISTRO_SANITARIO  | Registro del producto (uno por producto). Está separado para poder darlo de baja sin dar de baja el producto. Se crea junto con el producto, con el tipo de documento que le toca. Número y fechas son NULL hasta que el usuario los registre. Guarda su estado en `EstadoVencimientoId`. |
| CHECKLIST_REQUISITO | Lista de pendientes de un registro o una licencia. Sus filas se copian del catálogo cuando el usuario indica que no tiene el documento. Guarda estado, responsable (quien lo cumple), orden, fecha límite y fecha de cumplimiento. La descripción y el tipo del requisito se leen del catálogo (ver sección 4). |

### Sesión

| Tabla | Qué guarda |
|---|---|
| REFRESH_TOKEN | Tokens de sesión (guardados con hash). No lleva `EmpresaId`: se aísla por `UsuarioId`. |

### Catálogos simples (globales, sin `EmpresaId`)

Son tablas, no campos con lista cerrada.

| Tabla | Valores |
|---|---|
| TIPO_ESTABLECIMIENTO | Farmacia, Droguería, Bodega |
| CATEGORIA_PRODUCTO | Medicamento, Cosmético |
| ESTADO_VENCIMIENTO | No verificado, En trámite, Vigente, Por vencer, Vencido |
| ESTADO_REQUISITO | Pendiente, Cumplido |
| TIPO_REQUISITO | Documento, Dato, Acción |
| AMBITO_DOCUMENTO | Producto, Establecimiento |

### Base de conocimiento (global, sin `EmpresaId`)

| Tabla | Qué guarda |
|---|---|
| TIPO_DOCUMENTO | Clase de documento: a qué aplica (ámbito), autoridad, base legal y `VigenciaMeses`. |
| REQUISITO | Un requisito, escrito una sola vez. |
| DOCUMENTO_REQUISITO | Qué requisitos pide cada documento (muchos a muchos), con orden. |
| DOCUMENTO_DEPENDENCIA | Qué documento necesita a otro. |
| REGLA_APLICABILIDAD | Cuándo aplica un documento. La condición va en `CondicionJson`. |

## 3. Cómo trabaja el motor

1. El usuario ingresa los datos generales del establecimiento o del producto.
2. El motor evalúa las reglas con su tipo o categoría.
3. Elige **un solo documento** (si varias reglas aplican, gana la de menor `Prioridad`).
4. Se guardan el establecimiento o producto y, aparte, su licencia o registro con la empresa, el tipo de documento y el estado **No verificado**.
5. Mientras siga **No verificado**, el sistema pide al usuario los datos de la licencia o registro:
   - **Si los tiene:** se actualiza la licencia o registro con número y fecha de emisión. Se calcula vencimiento y estado. No se crea checklist.
   - **Si no los tiene:** el estado pasa a **En trámite**. El motor copia los requisitos del documento al checklist.
6. El sistema revisa si el documento depende de otro y muestra avance, pendientes y bloqueos.

## 4. Reglas clave

- **Un usuario por empresa** en el MVP.
- **Un documento por registro o licencia.** Una licencia por establecimiento y un registro por producto.
- **Sin regla aplicable:** el producto o establecimiento se guarda, pero no se crea licencia, registro ni checklist. Se muestra un mensaje.
- **No verificado:** estado inicial. Dura hasta que el usuario diga si ya tiene la licencia o registro.
- **Trámite en curso:** el usuario no lo tiene. Número y fechas son NULL hasta que ARSA emita. Mientras tanto el estado es **En trámite**.
- **Vigencia:** al emitirse (o al registrar uno que ya existe), el vencimiento = emisión + `VigenciaMeses` del documento.
- **Estados** (catálogo `ESTADO_VENCIMIENTO`, guardado en `EstadoVencimientoId` de registro y licencia): No verificado, En trámite, Vigente (más de 90 días), Por vencer (hasta 90 días), Vencido.
- **Bloqueo:** un documento queda bloqueado si falta una licencia **emitida** (con fechas) y **no vencida** del tipo que requiere. Una licencia No verificado o En trámite no cuenta. Se valida en el backend al consultar, no se guarda.
- **Checklist (MVP):** se crea solo cuando el usuario indica que no tiene el documento. Se copian las filas y su orden. Si el catálogo agrega o quita requisitos después, los checklists ya creados no cambian (filas y orden). La descripción y el tipo de cada requisito se leen del catálogo, así que un cambio de texto en `REQUISITO` sí se refleja en los checklists existentes. Congelar también la descripción y el tipo (copia completa) queda para **post-MVP**.
- **Baja lógica:** dar de baja un registro no es lo mismo que dar de baja el producto.
- **Renovación:** fuera del MVP.

## 5. Ejemplo

**Distribuidora FarmaCentro S.A.** tiene una bodega que opera como droguería y quiere distribuir **Paracetamol 500 mg**.

### Paso 0: catálogo sembrado (una vez)

| Documento | Aplica a | Vigencia |
|---|---|---|
| LIC-DROG: Licencia de droguería | Establecimiento | 12 meses |
| REG-MED: Registro de medicamento | Producto | 60 meses |

| Documento | Requisitos |
|---|---|
| LIC-DROG | Solicitud, Declaración jurada, Planos, Regente, Pago |
| REG-MED | Solicitud, Declaración jurada, Ficha técnica, BPM, Etiqueta, Pago |

- Dependencia: REG-MED requiere una LIC-DROG emitida y no vencida.
- Reglas: Tipo = Droguería → LIC-DROG. Categoría = Medicamento → REG-MED.

### Paso 1: registro

Se crea la empresa y su único usuario: María (administradora).

### Paso 2: establecimiento

María registra "Bodega Principal" de tipo Droguería (solo datos generales).

- Regla: aplica LIC-DROG.
- Se guarda el establecimiento y su licencia con estado **No verificado**.
- El sistema pide los datos de la licencia. María responde que aún no la tiene.
- La licencia pasa a **En trámite** y se crea su checklist de 5 requisitos.
- Si María ya la hubiera tenido, ingresa número y emisión: la licencia queda con su vencimiento y estado, y no se crea checklist.

### Paso 3: producto

María registra Paracetamol 500 mg, categoría Medicamento (solo datos generales).

- Regla: aplica REG-MED.
- Se guarda el producto y su registro con estado **No verificado**.
- María responde que aún no tiene el registro: pasa a **En trámite** y se crea su checklist de 6 requisitos.
- REG-MED depende de LIC-DROG y no hay ninguna licencia emitida y no vencida (la de la bodega sigue en trámite). El registro queda **bloqueado**.

### Paso 4: avance

María marca los requisitos como cumplidos. Cada requisito guarda su responsable (María) y la fecha de cumplimiento.

### Paso 5: lo que ve la empresa

- **Licencia de droguería:** 3 de 5 (60 %). Faltan Regente y Pago.
- **Registro de medicamento:** 1 de 6 (17 %). **Bloqueado**: falta una licencia de droguería emitida y no vencida.

### Paso 6: ARSA emite la licencia

María completa la licencia: número `LS-0001-EJ`, emisión 2026-12-10, vencimiento 2027-12-10 (12 meses). El estado de la licencia pasa a **Vigente**. Ahora existe una licencia emitida y no vencida y el registro del producto **se desbloquea**.

## 6. Notas

- Los ids de las entidades (empresa, usuario, establecimiento, producto, licencia, registro, checklist) son GUID. Los ids de los catálogos y la base de conocimiento (categorías, tipos, requisitos, documentos, etc.) son enteros fijos (seeds idempotentes).
- Cada fila del checklist apunta a una licencia **o** a un registro, nunca a ambos.
- Cada regla debe tener al menos una condición en `CondicionJson`; la aplicación lo valida (no hay restricción en la base de datos).
- El número de registro es único por empresa e ignora los NULL.
- Productos, registros y licencias dados de baja no se borran: no aparecen en listas, alertas, dashboard ni expediente.
- `EmpresaId` se repite a propósito en licencia, registro y checklist para filtrar sin joins.
- Un solo usuario por empresa se controla en la aplicación (no hay restricción en la base de datos).
