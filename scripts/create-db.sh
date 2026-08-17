#!/usr/bin/env bash
#
# Cria o usuário (role) e o banco de dados da aplicação em um PostgreSQL
# já instalado, lendo as credenciais do arquivo .env. É idempotente:
# rodar mais de uma vez não causa erro nem recria o que já existe.
#
# Uso (a partir da raiz do projeto): ./scripts/create-db.sh
#
# Requisitos: PostgreSQL instalado e em execução, e o cliente `psql` no PATH.
#
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"

# ---------------------------------------------------------------------------
# Credenciais da APLICAÇÃO: vêm do .env (as mesmas usadas para rodar a API).
# ---------------------------------------------------------------------------
ENV_FILE="${ENV_FILE:-$ROOT_DIR/.env}"
if [[ ! -f "$ENV_FILE" ]]; then
  echo "Erro: arquivo .env não encontrado em '$ENV_FILE'." >&2
  echo "Coloque o .env (enviado separadamente) na raiz do projeto e tente de novo." >&2
  exit 1
fi
set -a
# shellcheck disable=SC1090
source "$ENV_FILE"
set +a

: "${POSTGRES_USER:?defina POSTGRES_USER no .env}"
: "${POSTGRES_PASSWORD:?defina POSTGRES_PASSWORD no .env}"
: "${POSTGRES_DB:?defina POSTGRES_DB no .env}"

# ---------------------------------------------------------------------------
# Credenciais de SUPERUSUÁRIO: necessárias apenas para criar o banco/role.
# São as do seu PostgreSQL local (definidas na instalação), NÃO vêm do .env.
#
# Se o seu PostgreSQL exige senha, informe-a de uma destas formas:
#   - exporte a variável antes de rodar:  SUPERUSER_PASSWORD=suasenha ./scripts/create-db.sh
#   - ou preencha o valor na linha abaixo (entre as aspas), se necessário.
# ---------------------------------------------------------------------------
SUPERUSER="${SUPERUSER:-postgres}"
SUPERUSER_PASSWORD="${SUPERUSER_PASSWORD:-}"

HOST="${SUPERUSER_HOST:-localhost}"
PORT="${POSTGRES_PORT:-5432}"

export PGPASSWORD="$SUPERUSER_PASSWORD"

# ---------------------------------------------------------------------------
# Preflight: detectar e orientar antes de tentar provisionar.
# ---------------------------------------------------------------------------
if ! command -v psql >/dev/null 2>&1; then
  echo "Erro: cliente 'psql' não encontrado no PATH." >&2
  echo "  Instale o PostgreSQL client: https://www.postgresql.org/download/" >&2
  exit 1
fi

server_reachable() {
  if command -v pg_isready >/dev/null 2>&1; then
    pg_isready -h "$HOST" -p "$PORT" >/dev/null 2>&1
  else
    PGCONNECT_TIMEOUT=3 psql -h "$HOST" -p "$PORT" -U "$SUPERUSER" -d postgres -c "SELECT 1" >/dev/null 2>&1
  fi
}

if ! server_reachable; then
  echo "Erro: nenhum servidor PostgreSQL respondeu em $HOST:$PORT." >&2
  if command -v pg_lsclusters >/dev/null 2>&1 \
     && pg_lsclusters 2>/dev/null | awk 'NR>1 && tolower($4)=="down"{f=1} END{exit !f}'; then
    echo "  Há um cluster PostgreSQL local instalado, porém PARADO. Inicie-o, por exemplo:" >&2
    echo "    sudo pg_ctlcluster <versao> main start   (ou: sudo systemctl start postgresql)" >&2
  else
    echo "  Verifique se o PostgreSQL está instalado, em execução, e se POSTGRES_PORT ($PORT) está correta." >&2
    echo "    Iniciar (Linux): sudo systemctl start postgresql" >&2
    echo "    Instalar:        https://www.postgresql.org/download/" >&2
  fi
  exit 1
fi

echo "Provisionando em $HOST:$PORT como superusuário '$SUPERUSER'..."

psql -v ON_ERROR_STOP=1 --no-psqlrc -h "$HOST" -p "$PORT" -U "$SUPERUSER" -d postgres <<SQL
DO \$\$
BEGIN
   IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname = '${POSTGRES_USER}') THEN
      CREATE ROLE "${POSTGRES_USER}" LOGIN PASSWORD '${POSTGRES_PASSWORD}';
   END IF;
END
\$\$;

SELECT 'CREATE DATABASE "${POSTGRES_DB}" OWNER "${POSTGRES_USER}"'
 WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = '${POSTGRES_DB}')\gexec
SQL

echo "Pronto: role '${POSTGRES_USER}' e banco '${POSTGRES_DB}' disponíveis."
echo "Agora rode a API com: dotnet run --project src/OniBusExpress.Api"
