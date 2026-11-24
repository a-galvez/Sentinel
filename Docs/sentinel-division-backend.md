# División del backend en 5 personas

| Persona | Foco                | Historias                            | Qué construye                                                                                                                          |
| ------- | ------------------- | ------------------------------------ | -------------------------------------------------------------------------------------------------------------------------------------- |
| **P1**  | Datos y motor       | Base de todas                        | Entidades, `DbContext`, migraciones, seeds, catálogo de reglas y `IRequirementsEngine` (elige el documento y crea el checklist)        |
| **P2**  | Acceso y base común | H1, H2                               | Registro, login, refresh, logout, `/auth/me`, `ICurrentTenant`, `IClock`, `ExpirationStatusCalculator` y los endpoints de `/catalogos` |
| **P3**  | Productos           | H3, H4, H5                           | Crear, listar, editar y dar de baja productos. Verificación, emisión y baja del registro sanitario                                     |
| **P4**  | Establecimientos    | H6, H7                               | Crear y dar de baja establecimientos. Verificación, emisión y baja de la licencia. Listado de licencias                                |
| **P5**  | Seguimiento         | H8, H9, H10, H11 (lectura y edición) | Checklist (ver y marcar requisitos), bloqueos, alertas, expediente y dashboard                                                         |

## Por qué así

- **P3 y P4** hacen casi lo mismo (producto con registro, establecimiento con licencia), así que pueden ayudarse entre sí.
- **P5** junta todo lo que lee el estado de los documentos.
- **P1 y P2** dan la base que usan todos.

