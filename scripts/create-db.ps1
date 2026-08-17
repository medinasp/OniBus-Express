<#
    Cria o usuário (role) e o banco de dados da aplicação em um PostgreSQL
    já instalado, lendo as credenciais do arquivo .env. É idempotente:
    rodar mais de uma vez não causa erro nem recria o que já existe.

    Uso (a partir da raiz do projeto):  ./scripts/create-db.ps1

    Requisitos: PostgreSQL instalado e em execução, e o cliente `psql` no PATH.
#>
$ErrorActionPreference = 'Stop'

$RootDir = Split-Path -Parent $PSScriptRoot

# ---------------------------------------------------------------------------
# Credenciais da APLICAÇÃO: vêm do .env (as mesmas usadas para rodar a API).
# ---------------------------------------------------------------------------
$EnvFile = if ($env:ENV_FILE) { $env:ENV_FILE } else { Join-Path $RootDir '.env' }
if (-not (Test-Path $EnvFile)) {
    Write-Error "Arquivo .env não encontrado em '$EnvFile'. Coloque o .env (enviado separadamente) na raiz do projeto."
}

Get-Content $EnvFile | Where-Object { $_ -match '^\s*[^#].*=' } | ForEach-Object {
    $name, $value = $_ -split '=', 2
    Set-Item -Path "Env:$($name.Trim())" -Value $value.Trim()
}

foreach ($required in 'POSTGRES_USER', 'POSTGRES_PASSWORD', 'POSTGRES_DB') {
    if (-not (Get-Item "Env:$required" -ErrorAction SilentlyContinue).Value) {
        Write-Error "Defina $required no .env."
    }
}

# ---------------------------------------------------------------------------
# Credenciais de SUPERUSUÁRIO: necessárias apenas para criar o banco/role.
# São as do seu PostgreSQL local (definidas na instalação), NÃO vêm do .env.
#
# Se o seu PostgreSQL exige senha, informe-a de uma destas formas:
#   - defina antes de rodar:  $env:SUPERUSER_PASSWORD = 'suasenha'
#   - ou preencha o valor na linha abaixo (entre as aspas), se necessário.
# ---------------------------------------------------------------------------
$Superuser = if ($env:SUPERUSER) { $env:SUPERUSER } else { 'postgres' }
$SuperuserPassword = if ($env:SUPERUSER_PASSWORD) { $env:SUPERUSER_PASSWORD } else { '' }

$DbHost = if ($env:SUPERUSER_HOST) { $env:SUPERUSER_HOST } else { 'localhost' }
$Port = if ($env:POSTGRES_PORT) { $env:POSTGRES_PORT } else { '5432' }

$env:PGPASSWORD = $SuperuserPassword

# ---------------------------------------------------------------------------
# Preflight: detectar e orientar antes de tentar provisionar.
# ---------------------------------------------------------------------------
if (-not (Get-Command psql -ErrorAction SilentlyContinue)) {
    Write-Error "Cliente 'psql' não encontrado no PATH. Instale o PostgreSQL client: https://www.postgresql.org/download/"
}

$reachable = $false
if (Get-Command pg_isready -ErrorAction SilentlyContinue) {
    pg_isready -h $DbHost -p $Port *> $null
    $reachable = ($LASTEXITCODE -eq 0)
} else {
    $env:PGCONNECT_TIMEOUT = '3'
    'SELECT 1' | psql -h $DbHost -p $Port -U $Superuser -d postgres *> $null
    $reachable = ($LASTEXITCODE -eq 0)
}

if (-not $reachable) {
    Write-Error @"
Nenhum servidor PostgreSQL respondeu em ${DbHost}:${Port}.
Verifique se o PostgreSQL está instalado, em execução, e se POSTGRES_PORT ($Port) está correta.
  Iniciar (Windows): net start postgresql-x64-16   (ajuste a versão) — ou pelo aplicativo 'Serviços'.
  Instalar:          https://www.postgresql.org/download/
"@
}

Write-Host "Provisionando em ${DbHost}:${Port} como superusuário '$Superuser'..."

$Sql = @"
DO `$`$
BEGIN
   IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname = '$($env:POSTGRES_USER)') THEN
      CREATE ROLE "$($env:POSTGRES_USER)" LOGIN PASSWORD '$($env:POSTGRES_PASSWORD)';
   END IF;
END
`$`$;

SELECT 'CREATE DATABASE "$($env:POSTGRES_DB)" OWNER "$($env:POSTGRES_USER)"'
 WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = '$($env:POSTGRES_DB)')\gexec
"@

$Sql | psql -v ON_ERROR_STOP=1 --no-psqlrc -h $DbHost -p $Port -U $Superuser -d postgres

Write-Host "Pronto: role '$($env:POSTGRES_USER)' e banco '$($env:POSTGRES_DB)' disponíveis."
Write-Host "Agora rode a API com: dotnet run --project src/OniBusExpress.Api"
