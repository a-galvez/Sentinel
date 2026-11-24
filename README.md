# Sentinel

Sistema experto para el control de **registros sanitarios de productos** y **licencias sanitarias de establecimientos** (ARSA, Honduras).

Sentinel decide qué documento le corresponde a cada producto o establecimiento, arma la lista de requisitos pendientes (checklist), calcula vencimientos y muestra bloqueos, alertas y el estado general de riesgo de la empresa.

## Cómo funciona (resumen)

- Una **base de conocimiento global** define qué documentos existen, qué requisitos piden, de qué dependen y cuándo aplican.
- Un **motor** evalúa reglas según la categoría del producto o el tipo de establecimiento y asigna **un documento** (si varias reglas aplican, gana la de menor prioridad).
- Al registrar un producto o establecimiento se crea su registro o licencia en estado **No verificado**. Si el usuario no tiene el documento, pasa a **En trámite** y se genera el checklist.
- El estado se calcula con la fecha de Honduras: **Vigente** (más de 90 días), **Por vencer** (hasta 90 días), **Vencido**, además de **No verificado** y **En trámite**.
- Los documentos pueden quedar **bloqueados** si dependen de una licencia emitida y no vencida que aún no existe.

Más detalle en [`Docs/sentinel-explicacion.md`](Docs/sentinel-explicacion.md).

## Stack

| Parte | Tecnología |
|---|---|
| Frontend | React 19, TypeScript, Vite, Tailwind CSS |
| Backend | .NET 10 (API mínima por capas: Domain / Application / Infrastructure / API) |
| Base de datos | PostgreSQL 16 |
| Sesión | JWT + refresh tokens |
| Pruebas | xUnit (backend), Vitest + Testing Library (frontend) |
| Infra | Docker Compose + GitHub Actions |

## Estructura del repositorio

```
Sentinel/
├── backend/                  # API .NET en capas
│   ├── Sentinel.API/         # Controladores y módulos de composición
│   ├── Sentinel.Application/ # Casos de uso e interfaces
│   ├── Sentinel.Domain/      # Entidades, enums y cálculos
│   ├── Sentinel.Infrastructure/ # EF Core, migraciones, seeds, servicios
│   └── Sentinel.Tests/       # Pruebas xUnit
├── frontend/                 # App React + Vite
├── Docs/                     # Documentación funcional y técnica
├── infra/                    # Compose solo para PostgreSQL
├── docker-compose.yml        # Stack completo (db + api + web)
└── dev.sh                    # Script de desarrollo
```

## Levantar el proyecto

Requisitos: Docker y Docker Compose (para el stack completo); .NET 10 y pnpm/Node para desarrollo local.

```bash
cp .env.example .env     # ajustar credenciales si hace falta
./dev.sh up              # construye y levanta db + api + web
```

Servicios:

- Frontend: http://localhost:5173
- API: http://localhost:5186
- OpenAPI: http://localhost:5186/openapi/v1.json
- Scalar (documentación interactiva): http://localhost:5186/scalar/v1

Otros comandos:

```bash
./dev.sh down      # detiene el stack (conserva datos)
./dev.sh reset     # detiene y borra la base de datos
./dev.sh logs api  # logs de un servicio
./dev.sh db        # solo PostgreSQL (para correr la API con dotnet run)
./dev.sh migrate   # aplica migraciones con dotnet ef
./dev.sh test      # pruebas del backend
./dev.sh format    # verifica formato
./dev.sh status    # estado del stack
```

## Desarrollo

**Backend** (con la base levantada con `./dev.sh db`):

```bash
cd backend
dotnet restore Sentinel.slnx
dotnet run --project Sentinel.API
```

**Frontend**:

```bash
cd frontend
pnpm install
pnpm dev
```

Variables de entorno: ver [`.env.example`](.env.example) y [`frontend/.env.example`](frontend/.env.example). Las credenciales de JWT y PostgreSQL deben cambiarse fuera de desarrollo.
