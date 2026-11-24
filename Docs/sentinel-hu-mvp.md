# Sentinel MVP: historias de usuario

## Épica 1 · Acceso

### H1 · Registrar empresa y administrador

**Historia.** Como representante de una empresa, quiero registrar mi empresa y mi usuario administrador en un solo formulario, para empezar a usar la plataforma.

**Criterios de aceptación**

- Se crean la empresa y el administrador juntos; si falla uno, no se crea ninguno.
- En el MVP, el administrador es el único usuario de la empresa.
- El correo es único en todo el sistema; si ya existe, se indica junto al campo.
- La contraseña tiene mínimo 8 caracteres y el requisito se ve antes de enviar.
- Los errores aparecen junto al campo que corresponde.
- Al terminar, quedo con la sesión iniciada.

### H2 · Iniciar sesión y proteger pantallas

**Historia.** Como usuario, quiero iniciar y cerrar sesión, y que las pantallas privadas solo se abran con sesión activa, para proteger la información de mi empresa.

**Criterios de aceptación**

- Con correo y contraseña correctos entro a la plataforma.
- Con datos incorrectos veo un solo mensaje, sin indicar cuál falló.
- Sin sesión, una pantalla privada me lleva al login.
- Si mi sesión vence mientras trabajo, se renueva sin pedirme los datos.
- Al cerrar sesión pierdo el acceso.

---

## Épica 2 · Productos

### H3 · Registrar un producto

**Historia.** Como encargado regulatorio, quiero registrar un producto con su nombre y categoría, y luego indicar si ya tiene registro sanitario, para tener el producto y su vigencia en un solo lugar.

**Criterios de aceptación**

- Nombre y categoría son obligatorios; la categoría se elige de una lista. El fabricante es mi empresa, no se pide.
- Al guardar, se crean el producto y su registro sanitario, con el documento que le toca según la categoría. El registro queda en estado **No verificado**.
- Mientras el registro esté No verificado, el sistema me pide indicar si ya lo tengo:
  - **Sí:** ingreso número de registro y fecha de emisión (van juntos). El sistema calcula el vencimiento y el estado. No se crea checklist.
  - **No:** el registro pasa a **En trámite** y se crea su checklist (H11).
- El número de registro no se repite dentro de mi empresa.
- Al guardar, el producto aparece en mi lista con su estado.
- Un usuario de otra empresa no ve ni abre mi producto.

### H4 · Ver productos con su estado

**Historia.** Como usuario, quiero listar, buscar y filtrar mis productos y ver su estado, para identificar riesgos de un vistazo.

**Criterios de aceptación**

- Cada producto muestra con color y texto el estado de su registro:
  - **No verificado:** aún no he indicado si tengo el registro.
  - **En trámite:** indiqué que no lo tengo; aún no tiene fechas.
  - **Vencido:** ya venció.
  - **Por vencer:** vence hoy o en los próximos 90 días.
  - **Vigente:** vence en más de 90 días.
- "Hoy" es la fecha de Honduras.
- Puedo buscar por nombre o número de registro.
- Puedo filtrar por estado.
- Veo mensajes distintos para cargando, lista vacía y error.
- Los productos dados de baja no aparecen.

### H5 · Editar y dar de baja un producto o su registro

**Historia.** Como encargado regulatorio, quiero editar y dar de baja un producto o su registro, para mantener el catálogo actualizado y registrar la emisión cuando ARSA la entrega.

**Criterios de aceptación**

- Al editar rigen las mismas reglas que al registrar.
- Si el registro estaba No verificado o En trámite y agrego número y emisión, se calcula el vencimiento y cambia su estado.
- La categoría no se puede cambiar.
- Al dar de baja un producto se pide confirmación.
- Un producto dado de baja desaparece de la lista, las alertas y el dashboard, pero sus datos no se borran.
- Puedo dar de baja solo el registro de un producto, con confirmación. El producto no se da de baja.
- Un registro dado de baja desaparece de las alertas, el dashboard y el expediente, pero sus datos no se borran.
- Un producto de otra empresa responde como si no existiera.

---

## Épica 3 · Establecimientos y licencias

### H6 · Registrar establecimientos y su licencia

**Historia.** Como administrador, quiero registrar mis establecimientos y luego indicar si ya tienen licencia sanitaria, y completarla cuando ARSA la emita, para saber si pueden operar.

**Criterios de aceptación**

- Nombre, tipo y ubicación son obligatorios; el tipo se elige de una lista (farmacia, droguería, bodega).
- Cada establecimiento tiene una sola licencia, creada junto con él, con el documento que le toca según su tipo. La licencia queda en estado **No verificado**.
- Mientras la licencia esté No verificado, el sistema me pide indicar si ya la tengo:
  - **Sí:** ingreso número y fecha de emisión (van juntos). El sistema calcula el vencimiento y el estado. No se crea checklist.
  - **No:** la licencia pasa a **En trámite** y se crea su checklist (H11).
- Cuando ARSA emite la licencia, puedo registrar su número y emisión.
- Al registrar la emisión, la licencia sale de En trámite y se desbloquean los documentos que dependen de ella.
- Al guardar, el establecimiento aparece en mi lista junto con su licencia.
- Solo veo establecimientos y licencias de mi empresa.

### H7 · Ver el estado de las licencias

**Historia.** Como usuario, quiero ver cada licencia con el mismo semáforo que los productos y filtrar por estado, para detectar riesgos en mis establecimientos.

**Criterios de aceptación**

- Cada licencia muestra su estado con color y texto, con las mismas definiciones de H4.
- Veo a qué establecimiento pertenece cada licencia.
- Puedo filtrar por estado.
- Veo los mismos mensajes de cargando, vacío y error que en H4.

---

## Épica 4 · Alertas

### H8 · Ver alertas de vencimiento

**Historia.** Como encargado regulatorio, quiero ver qué productos y licencias vencen en 30, 60 y 90 días, y cuáles ya vencieron, para actuar antes de que falle.

**Criterios de aceptación**

- Las alertas se agrupan en: vencidos, hasta 30 días (incluye hoy), de 31 a 60 días y de 61 a 90 días.
- Cada elemento aparece en un solo grupo.
- Dentro de cada grupo, el orden es por vencimiento, el más cercano primero.
- Cada alerta indica si es producto o licencia y lleva a su detalle.
- Lo No verificado, lo En trámite y lo dado de baja no genera alertas.
- Si no hay alertas, veo un mensaje que lo dice.

---

## Épica 5 · Expediente y checklist

### H9 · Ver e imprimir el expediente de un producto

**Historia.** Como encargado regulatorio, quiero generar con un clic el expediente de un producto, para presentarlo ante una visita de la ARSA.

**Criterios de aceptación**

- Desde el producto, un clic abre el expediente.
- Muestra datos del producto, registro sanitario (o su estado: No verificado o En trámite), vigencia y estado.
- Si el registro tiene checklist, muestra su avance y, si hay bloqueo, el aviso.
- Al imprimir o guardar como PDF, la hoja sale limpia, sin menús ni botones.
- Un producto de otra empresa o dado de baja no se puede abrir.
- Un registro dado de baja no aparece en el expediente.

### H11 · Generar el checklist del expediente

**Historia.** Como encargado regulatorio, quiero que, si no tengo el registro de un producto o la licencia de un establecimiento, se genere solo la lista de requisitos que debo reunir, para no depender de la memoria ni de una hoja aparte.

**Criterios de aceptación**

- Al guardar un producto, el sistema elige el documento que le corresponde según su categoría y lo asigna a su registro. El checklist se crea cuando indico que no tengo el registro.
- Al guardar un establecimiento, hace lo mismo para su licencia, según su tipo.
- Veo el documento asignado y sus requisitos, cada uno con estado (pendiente o cumplido) y tipo (documento, dato o acción).
- Si ninguna regla aplica, el producto o establecimiento se guarda, no se crea registro, licencia ni checklist, y veo un mensaje.
- Si varias reglas aplican, gana la de menor `Prioridad`.
- Cada requisito puede tener un responsable (el único usuario de mi empresa) y una fecha límite.
- Puedo marcar un requisito como cumplido o devolverlo a pendiente.
- Al cumplirlo, se guarda la fecha y el responsable pasa a ser quien lo completó; al devolverlo, se borra la fecha.
- Veo el avance ("3 de 6").
- Si el documento depende de otro, se muestra **bloqueado** y qué falta.
- El bloqueo desaparece cuando existe una licencia de ese tipo emitida (con fechas) y no vencida; una No verificado o En trámite no cuenta.
- Si el catálogo agrega o quita requisitos después, las filas y el orden de los checklists ya creados no cambian. La descripción y el tipo de cada requisito se leen del catálogo.
- Solo veo los checklists de mi empresa.

---

## Épica 6 · Dashboard

### H10 · Ver el dashboard de riesgo

**Historia.** Como gerencia, quiero ver los totales de productos y licencias por estado, para entender el riesgo general de mi empresa.

**Criterios de aceptación**

- Veo diez totales: productos y licencias, cada uno en no verificado, en trámite, vigente, por vencer y vencido.
- Los totales coinciden con las listas de H4 y H7.
- Al registrar, editar o dar de baja algo, los totales se actualizan.
- Solo cuenta datos de mi empresa y no incluye lo dado de baja.
