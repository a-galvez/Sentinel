# Sentinel · Cómo trabajar en equipo (backend)

Resumen operativo para 5 personas trabajando en paralelo. El detalle completo está en
`Docs/sentinel-backend-reparto.md` y los contratos en `Docs/contratos-api-sentinel.yaml`.

---

## 1. Reparto (quién es dueño de qué)

| Persona | Módulo | HUs | Carpetas propias |
|---|---|---|---|
| **P1** | Sesión y plataforma | H2, GET /usuarios | `Features/Auth`, `Features/Usuarios`, `Controllers/Auth`, `Controllers/Usuarios`, `Program.cs`, `Migrations/`, YAML, CI |
| **P2** | Registro y alta de productos | H1, H3 | `Features/Registro`, `Features/Productos/Crear`, `Features/Catalogos/Categorias` |
| **P3** | Estado y productos | H4, H5, H7 | `Features/Productos/Listar·Editar·Baja`, `Features/Licencias`, `Domain/Calculos` |
| **P4** | Establecimientos y alertas | H6, H8 | `Features/Establecimientos`, `Features/Alertas`, `Features/Catalogos/TiposEstablecimiento` |
| **P5** | Motor de requisitos | H11, H9, H10 | `Features/Requisitos`, `Features/Expediente`, `Features/Dashboard`, `Infrastructure/Seeding` |

> `CODEOWNERS` ya refleja este reparto. Reemplaza `@P1..@P5` por los usuarios reales de GitHub.

---

## 2. Reglas de convivencia

1. **Sólo tocas tus carpetas.** Para cambiar un archivo ajeno: pídelo o manda un PR chico con sólo ese cambio.
2. **Una clase por caso de uso.** Nada de `ProductoService`/`IProductoRepository` compartidos.
3. **Un controlador por caso de uso**, con la misma ruta base (`api/v1/productos`).
4. **Sólo P1 crea migraciones.** Si necesitas cambio de esquema, escríbele a P1 con el cambio exacto.
5. **Sólo P1 edita el YAML.** Si un endpoint no coincide con el contrato, avisa a P1.
6. **PR chicos y diarios** (< ~400 líneas). `git pull --rebase origin main` cada mañana. `main` siempre verde.
7. **No reformatees ni muevas código ajeno.** Renombrados van en PR aparte y avisado.
8. **Datos de prueba con el `DbContext`**, nunca llamando endpoints de otra persona. Usa los builders.
9. **Seeds idempotentes con GUIDs fijos**, y sólo P5 los escribe.
10. **No filtres por empresa a mano.** El filtro global por `EmpresaId` ya lo hace.

**Archivos congelados** (cambios sólo por PR chico aprobado por P1): `Program.cs`, `Modules/`,
`Persistence/AppDbContext.cs`, `Middleware/`, `Abstractions/`, `Common/`, `Directory.Packages.props`,
`Migrations/`, el YAML y CI.

---

## 3. Contratos internos (ya publicados, M0)

Están en `Sentinel.Application/Abstractions` y `Sentinel.Domain/Calculos`. No los redefinas.

```csharp
ICurrentTenant   // EmpresaId (Guid), UsuarioId (Guid), TieneSesion
IClock           // HoyHonduras (UTC-6), UtcAhora
ITokenService    // EmitirAsync(Usuario, ct) -> AuthTokens
IMotorRequisitos // AplicarAProductoAsync / AplicarALicenciaAsync (sin SaveChanges)
IPasswordHasher  // Hash / Verificar
IAppDbContext    // acceso a los DbSet
```

```csharp
EstadoVencimientoCalculator.Calcular(vencimiento, hoy) // Vigente | PorVencer (<=90) | Vencido (<0)
```

**Regla clave del motor:** `IMotorRequisitos` **no** llama `SaveChanges`; deja todo listo en el
DbContext y el caso de uso hace **un solo** `SaveChanges` (misma transacción del alta).
Mientras no exista el motor real, está registrado `MotorRequisitosVacio` en Infrastructure;
P5 lo sustituye registrando el real en `RequisitosModule` (M6).

**No calcules el estado por tu cuenta:** usa siempre `EstadoVencimientoCalculator`.

---

## 4. Cómo escribir un caso de uso (plantilla)

```
Sentinel.Application/Features/<Modulo>/<Caso>/
    <Caso>Handler.cs        // lógica; recibe IAppDbContext, IClock, ICurrentTenant...
    <Caso>Validator.cs      // FluentValidation
    <Caso>Request.cs        // DTO de entrada (sin empresaId)
Sentinel.API/Controllers/<Modulo>/
    <Caso>Controller.cs     // [Authorize], ruta api/v1/...
```

- El `EmpresaId` **nunca** viene en el request: sale de `ICurrentTenant`.
- Las consultas ya vienen filtradas por empresa y sin lo dado de baja.
- Errores: lanza `NotFoundException`, `ConflictException`, `BadRequestException` o
  `UnauthorizedAppException` desde `Common/Exceptions`. El middleware las traduce a 404/409/400/401.
- Devuelve DTOs con los nombres del YAML (camelCase, `estado`, `avance`, etc.).

---

## 5. Módulos DI

Cada persona registra sus servicios en su módulo de `Sentinel.API/Modules`:

| Archivo | Dueño |
|---|---|
| `AuthModule` | P1 |
| `RegistroModule`, `CatalogosModule` (categorías) | P2 |
| `ProductosModule`, `LicenciasModule` | P3 |
| `EstablecimientosModule`, `AlertasModule`, `CatalogosModule` (tipos) | P4 |
| `RequisitosModule`, `ExpedienteModule`, `DashboardModule` | P5 |

`Program.cs` ya llama a todos; sólo añade dentro de tu método `Add...Module`.

---

## 6. Pruebas

- Builders listos: `EmpresaBuilder`, `UsuarioBuilder`, `ProductoBuilder`, `EstablecimientoBuilder`,
  `LicenciaBuilder` (en `Sentinel.Tests/Builders`).
- `SentinelFactory.ClienteAutenticado(empresaId)` → cliente HTTP autenticado como esa empresa.
- `SentinelFactory.DosEmpresas()` → dos empresas y sus clientes, para probar aislamiento.
- Pruebas siempre en `Sentinel.Tests/<Modulo>/`, espejando tu carpeta de tu módulo.

```bash
./dev.sh test     # ó  cd backend && dotnet test Sentinel.slnx
```

---

## 7. Levantar el proyecto

```bash
# Todo el stack (Postgres + API + frontend) en Docker
./dev.sh up

# Sólo la base, para desarrollar la API con dotnet run
./dev.sh db
cd backend/Sentinel.API && dotnet run
```

Ver `README-dev` dentro de este archivo (sección 8) para el resto de comandos.

---

## 8. Comandos de `dev.sh`

| Comando | Qué hace |
|---|---|
| `./dev.sh up` | Construye y levanta db + api + web |
| `./dev.sh down` | Detiene el stack (conserva datos) |
| `./dev.sh reset` | Detiene y **borra** la base de datos |
| `./dev.sh logs [svc]` | Logs en vivo (`api`, `web`, `db`) |
| `./dev.sh db` | Levanta sólo Postgres |
| `./dev.sh migrate` | Aplica migraciones con `dotnet ef` |
| `./dev.sh test` | Pruebas del backend |
| `./dev.sh format` | Verifica formato (`dotnet format`) |
| `./dev.sh status` | Estado del stack y migraciones aplicadas |

Puertos por defecto: **web 5173**, **api 5186**, **db 5432** (se cambian en `.env` raíz).
La API aplica migraciones al arrancar (`MIGRATE_ON_START=true`).

---

## 9. Hitos

| Hito | Dueño | Qué desbloquea |
|---|---|---|
| **M0** | P1 | Contratos, builders, módulos DI ✅ hecho |
| **M1** | P1 | Migración real con índices únicos ✅ hecho |
| **M2** | P3 | `EstadoVencimientoCalculator` con pruebas ✅ hecho |
| **M3** | P1 | Auth completo (login, refresh, logout, tokens) |
| **M4** | P2 | `POST /productos` funcionando |
| **M5** | P4 | `POST /establecimientos` funcionando |
| **M6** | P5 | Motor real conectado |
| **M7** | Todos | Integración con dos empresas |

**Si te bloquea algo de otra persona:** no esperes. Programa contra la interfaz, usa un falso en
tus pruebas y avisa con el formato: *"Necesito X de Pn para el día d"*.
