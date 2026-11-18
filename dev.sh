#!/usr/bin/env bash
# =============================================================================
# Sentinel · script de desarrollo
#
#   ./dev.sh up          Levanta el stack completo (db + api + web) en Docker
#   ./dev.sh down        Detiene el stack (conserva los datos)
#   ./dev.sh reset       Detiene el stack y BORRA la base de datos
#   ./dev.sh logs [svc]  Muestra logs (api, web, db o todo)
#   ./dev.sh db          Levanta SOLO Postgres (para desarrollar la API con dotnet run)
#   ./dev.sh migrate     Aplica migraciones con dotnet ef
#   ./dev.sh test        Corre las pruebas del backend
#   ./dev.sh format      Verifica el formato del backend
#   ./dev.sh status      Estado del stack y migraciones aplicadas
#   ./dev.sh help        Esta ayuda
# =============================================================================
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$ROOT"

COMPOSE="docker compose"
DB_SERVICE="db"
API_SERVICE="api"

# Colores (se desactivan si no hay terminal).
if [ -t 1 ]; then
  BOLD=$'\e[1m'; GREEN=$'\e[32m'; YELLOW=$'\e[33m'; RED=$'\e[31m'; DIM=$'\e[2m'; RESET=$'\e[0m'
else
  BOLD=''; GREEN=''; YELLOW=''; RED=''; DIM=''; RESET=''
fi

info()  { echo "${GREEN}▶${RESET} $*"; }
warn()  { echo "${YELLOW}!${RESET} $*"; }
error() { echo "${RED}✖${RESET} $*" >&2; }

ensure_env() {
  if [ ! -f .env ]; then
    warn "No existe .env en la raíz; creándolo desde .env.example"
    cp .env.example .env
  fi
}

ensure_docker() {
  if ! docker info >/dev/null 2>&1; then
    error "Docker no está corriendo. Inicia Docker e inténtalo de nuevo."
    exit 1
  fi
}

cmd_up() {
  ensure_docker
  ensure_env
  info "Construyendo y levantando el stack (esto puede tardar la primera vez)..."
  $COMPOSE up -d --build
  echo
  info "Listo."
  echo "   Frontend: ${BOLD}http://localhost:$(env_val WEB_PORT 5173)${RESET}"
  echo "   API:      ${BOLD}http://localhost:$(env_val API_PORT 5186)${RESET}"
  echo "   OpenAPI:  http://localhost:$(env_val API_PORT 5186)/openapi/v1.json"
  echo "   Scalar:   http://localhost:$(env_val API_PORT 5186)/scalar/v1"
  echo "   ${DIM}Logs: ./dev.sh logs api${RESET}"
}

cmd_down() {
  ensure_docker
  info "Deteniendo el stack (los datos se conservan)..."
  $COMPOSE down
}

cmd_reset() {
  ensure_docker
  warn "Esto detendrá el stack y borrará TODOS los datos de la base."
  read -r -p "   ¿Continuar? [y/N] " resp
  case "$resp" in
    y|Y|yes|YES) $COMPOSE down -v && info "Stack y datos eliminados." ;;
    *) info "Cancelado." ;;
  esac
}

cmd_logs() {
  ensure_docker
  local svc="${1:-}"
  $COMPOSE logs -f --tail=100 ${svc:+$svc}
}

cmd_db() {
  ensure_docker
  ensure_env
  info "Levantando solo Postgres..."
  $COMPOSE up -d "$DB_SERVICE"
  info "Postgres listo en localhost:$(env_val POSTGRES_PORT 5432)."
  echo "   ${DIM}Migraciones: ./dev.sh migrate  (o deja que la API las aplique al arrancar)${RESET}"
}

cmd_migrate() {
  ensure_docker
  ensure_env
  info "Aplicando migraciones..."
  ( cd backend && dotnet tool restore >/dev/null && \
    dotnet ef database update \
      --project Sentinel.Infrastructure \
      --startup-project Sentinel.Infrastructure )
  info "Migraciones aplicadas."
}

cmd_test() {
  info "Ejecutando pruebas del backend..."
  ( cd backend && dotnet test Sentinel.slnx --nologo )
}

cmd_format() {
  info "Verificando formato del backend..."
  ( cd backend && dotnet format Sentinel.slnx --verify-no-changes --no-restore )
}

cmd_status() {
  ensure_docker
  echo "${BOLD}Contenedores${RESET}"
  $COMPOSE ps
  echo
  echo "${BOLD}Migraciones aplicadas${RESET}"
  $COMPOSE exec -T "$DB_SERVICE" \
    psql -U "$(env_val POSTGRES_USER admin)" -d "$(env_val POSTGRES_DB HabitForgeDB)" \
    -c 'SELECT "MigrationId" FROM "__EFMigrationsHistory" ORDER BY "MigrationId";' 2>/dev/null \
    || warn "No se pudo consultar la tabla de migraciones (¿stack arriba?)."
}

# Lee una variable del .env raíz, con un valor por defecto.
env_val() {
  local key="$1" default="$2"
  if [ -f .env ]; then
    local val
    val="$(grep -E "^${key}=" .env | head -n1 | cut -d= -f2- || true)"
    [ -n "$val" ] && { echo "$val"; return; }
  fi
  echo "$default"
}

cmd_help() {
  sed -n '3,15p' "${BASH_SOURCE[0]}" | sed 's/^# \{0,1\}//'
}

main() {
  local cmd="${1:-help}"
  shift || true
  case "$cmd" in
    up)      cmd_up "$@" ;;
    down)    cmd_down "$@" ;;
    reset)   cmd_reset "$@" ;;
    logs)    cmd_logs "$@" ;;
    db)      cmd_db "$@" ;;
    migrate) cmd_migrate "$@" ;;
    test)    cmd_test "$@" ;;
    format)  cmd_format "$@" ;;
    status)  cmd_status "$@" ;;
    help|-h|--help) cmd_help ;;
    *) error "Comando desconocido: $cmd"; echo; cmd_help; exit 1 ;;
  esac
}

main "$@"
