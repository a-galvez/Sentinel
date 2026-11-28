# Sentinel Frontend — Guía del equipo

Para personas y agentes (quien use agente, pásele este documento como contexto).
Fuente de verdad de la API: `Docs/sentinel-openapi.yaml`. Modelo de datos: `Docs/sentinel-erd.mermaid`.
**Alcance: se terminan las 7 pantallas. No hay recortes.**

## 1. Stack y reglas

- React 19 + TypeScript + Vite + **Tailwind** (solo clases utilitarias) + **react-hook-form + zod** + **axios**.
- **Prohibido agregar otras dependencias** (sin router, sin librerías de datos ni de UI). HTTP solo con axios, usando la instancia compartida; nada de `fetch` ni instancias propias.
- Estado: `useState`/`useReducer` local; un solo Context global, el de auth.
- Textos de UI en español. Antes de cada PR: lint y `tsc --noEmit` sin errores.
- Git: rama por tarea (`feat/<feature>-<algo>`), PR de menos de 300 líneas, 1 revisión de otra persona, `main` protegido.
- Quien use agente debe poder explicar su PR. Nadie hace merge de lo que no entiende.

## 2. Estructura y dueños

```
frontend/src/
  app/               router, layout, providers              F1
  shared/api/        api (axios), request, useFetch         F1
  shared/ui/         componentes base                       F1 + F5
  shared/lib/        fechas, formato                        F1
  features/auth/             login y registro               F1
  features/productos/        lista, detalle                 F2
  features/establecimientos/ lista, detalle                 F3
  features/documento/        DocumentoPanel, ChecklistPanel F4
  features/dashboard/        dashboard, alertas, expediente F5
```

Cada feature contiene `schemas.ts` (zod), `api.ts` (llamadas), `components/`, `pages/` y `routes.tsx`.
**Solo se edita la carpeta propia.** Cambios en `shared/` o `app/` se piden a F1 con un PR pequeño.

## 3. Pantallas y endpoints (7)

| Ruta | Dueño | Endpoints |
|---|---|---|
| `#/login` | F1 | `POST auth/login`, `auth/registro`, `GET auth/me` |
| `#/` Dashboard + alertas | F5 | `GET dashboard/resumen`, `alertas` |
| `#/productos` | F2 | `GET/POST productos`, `GET catalogos/categorias-producto` |
| `#/productos/:id` | F2 | `GET/PATCH/DELETE productos/{id}` + paneles de F4 |
| `#/productos/:id/expediente` | F5 | `GET productos/{id}/expediente` (imprimible con `print:`) |
| `#/establecimientos` | F3 | `GET/POST establecimientos`, `GET catalogos/tipos-establecimiento` |
| `#/establecimientos/:id` | F3 | `GET/DELETE establecimientos/{id}` + paneles de F4 |

`DocumentoPanel` y `ChecklistPanel` (F4) cubren `…/registro` y `…/licencia` (GET, PATCH de emisión, DELETE, `verificacion`), sus `…/checklist` y `PATCH checklist-requisitos/{id}`.

## 4. Contratos compartidos (F1 los publica primero)

```ts
// shared/api
api: AxiosInstance                       // baseURL, token en header, refresh automático en 401
request<T>(config: AxiosRequestConfig, schema?: ZodType<T>): Promise<T>   // valida con zod, lanza ApiError
class ApiError { status: number; message: string; errors?: Record<string, string[]> }
useFetch<T>(url: string | null, schema?: ZodType<T>): { data: T | null; loading: boolean; error: ApiError | null; reload: () => void }
applyApiErrors(err: ApiError, setError): void   // 400 → errores por campo de react-hook-form

// app (router de hash)
type AppRoute = { path: string; element: (params: Record<string, string>) => JSX.Element; protegida?: boolean }
navigate(path: string): void                    // cada feature exporta routes: AppRoute[]

// features/documento (F4)
<DocumentoPanel origen="registro" | "licencia" id={productoId | establecimientoId} />
<ChecklistPanel origen="registro" | "licencia" id={productoId | establecimientoId} />

// shared/ui
Button, Input, Select, Modal, Table, Paginador, Spinner, EstadoBadge({ estado })
```

## 5. Convenciones

- **Base URL:** `VITE_API_URL` (por defecto `http://localhost:5000/api/v1`). Access token en memoria, refresh token en `localStorage`.
- **401:** un interceptor intenta `auth/refresh` una sola vez (las peticiones simultáneas esperan ese mismo refresh); si falla, cierra sesión y va a `#/login`.
- **Errores:** 400 → `applyApiErrors`; 404 → mensaje "no existe"; 409 → mensaje en el formulario.
- **Zod:** el schema de cada respuesta y formulario se escribe desde el OpenAPI; tipos con `z.infer`. No escribir tipos a mano.
- **IDs:** los de catálogos (`categoriaProductoId`, `tipoEstablecimientoId`, `id` de `CatalogoItem`) son **enteros**; los de entidades (producto, establecimiento, registro, licencia, checklist) son `uuid` string.
- **Estados (`EstadoBadge`):** NoVerificado gris · EnTramite azul · Vigente verde · PorVencer amarillo · Vencido rojo.
- **Fechas:** `YYYY-MM-DD`; "hoy" es hora de Honduras. Paginación: `pagina`, `tamanoPagina`.
- **Multi-tenant:** nunca enviar `empresaId`; sale del token.
- **Bajas lógicas:** lo dado de baja no aparece en listas; tras un `DELETE`, volver a la lista.
- **Bloqueo del checklist** lo calcula el backend (`bloqueo.faltantes`); la UI solo lo muestra.

## 6. Pendiente (backend)

- Actualizar `Docs/sentinel-openapi.yaml` y `Docs/sentinel-erd.mermaid` para que `categoriaProductoId` sea entero. Hasta entonces, el frontend ya lo trata como entero.
