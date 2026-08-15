# Backlog de tarefas — OniBus Express

> Derivado do `plan.md`, ordenado para respeitar o TDD (teste primeiro) e as dependências entre as
> peças. Cada tarefa indica o que entrega, quais casos de teste (`T-xx`) e regras (`RN-xx`) cobre, e
> de que depende. As tarefas de código seguem o ciclo RED → GREEN → REFACTOR.

## Fase 0 — Fundação

- [x] **TASK-01 — Estrutura da solução.** `.sln`, projetos `src/` (Domain, Application,
  Infrastructure, Api) e `tests/` (Unit, Integration, Functional, Architecture), `global.json`
  (SDK 8), `.editorconfig`. Compila vazio. — *depende de: —*
- [x] **TASK-02 — Empacotamento.** `Dockerfile` multi-stage (não-root), `.dockerignore`,
  `docker-compose.yml` (API + PostgreSQL com `healthcheck` e `depends_on: service_healthy`),
  `.env.example`. `docker compose up` sobe os dois serviços. — *depende de: TASK-01*

## Fase 1 — Domínio (testes unitários primeiro)

- [x] **TASK-03 — Value Object `Cpf`.** Validação por módulo 11 + rejeição de dígitos repetidos +
  normalização de máscara. — *cobre: T-01..T-05 · RN-03*
- [x] **TASK-04 — Value Object `ReservationCode`.** Formato `ABC-12345`, alfabeto sem `I`/`O`;
  fábrica de geração. — *cobre: T-06, T-07 · RN-04*
- [x] **TASK-05 — Entidades e regras temporais.** `Route`, `Trip`, `Reservation`; assento no
  intervalo, viagem no passado e janela de 2h usando `TimeProvider`. — *cobre: T-08..T-12 ·
  RN-02, RN-05, RN-06*
- [x] **TASK-06 — `Result` / `DomainError`.** Tipos de retorno para erros de negócio. — *depende de: —*

## Fase 2 — Persistência (integração com PostgreSQL real)

- [x] **TASK-07 — `AppDbContext` + mapeamentos + migration inicial.** Índice único **parcial**
  `(trip_id, seat_number) WHERE status='Confirmed'` e índice único em `code`. — *cobre: base de
  T-13..T-17 · RN-01*
- [x] **TASK-08 — `ReservationRepository`.** Inserção transacional; tradução de
  `PostgresException 23505` → conflito de domínio. — *cobre: T-14, **T-15 (concorrência)** · RN-01*
- [x] **TASK-09 — Seed de dados.** Rotas e viagens de exemplo: futura, passada e partindo em <2h. —
  *depende de: TASK-07*

## Fase 3 — Casos de uso (Application)

- [x] **TASK-10 — Consultas de leitura.** Portas `IRouteQueries`/`ITripQueries`: listar rotas,
  buscar viagens e detalhar a viagem (mapa de assentos). — *cobre: RF-01..RF-03*
- [x] **TASK-11 — `CreateReservation`.** Validação + regras (RN-02, RN-06, RN-09) + inserção. —
  *cobre: T-13, T-14, **T-15** · RN-01, RN-02, RN-06*
- [x] **TASK-12 — `GetReservation`.** Retorno com **CPF mascarado**. — *cobre: RF-05 · RNF-11*
- [x] **TASK-13 — `CancelReservation`.** Janela de 2h; bloqueio de recancelamento; liberação do
  assento. — *cobre: T-16, T-17 · RN-05, RN-07, RN-08*

## Fase 4 — Borda HTTP (Api)

- [x] **TASK-14 — Endpoints Minimal API em módulos.** Os 6 endpoints como adaptadores finos;
  `FluentValidation`; `IExceptionHandler` → **Problem Details (RFC 7807)**; status HTTP por caso. —
  *cobre: T-18..T-22 · toda a seção 7 do spec*
- [x] **TASK-15 — Documentação e observabilidade.** Swagger/OpenAPI com metadados nos endpoints
  (resumo, descrição e códigos de resposta); `/health` e `/health/ready`; Serilog estruturado + id
  de correlação; CPF fora do log. — *cobre: RNF-06, RNF-09, RNF-11*

## Fase 5 — Qualidade e entrega

- [x] **TASK-16 — Testes de arquitetura.** `NetArchTest`: fronteiras de camada. — *cobre: T-23*
- [x] **TASK-17 — Cobertura.** `coverlet` + `ReportGenerator` na execução local dos testes;
  foco em RN-01..RN-08. — *cobre: RNF-10*
- [x] **TASK-18 — README** (português). Seções: (1) como rodar local **com e sem Docker**;
  (2) tecnologias usadas e justificativa; (3) decisões de arquitetura (SOLID, Clean Architecture,
  ADRs); (4) **o que foi implementado vs. omitido** (backend; interface de usuário fora de escopo);
  (5) como rodar os testes. Extras: link do Swagger, screenshots, melhorias futuras. — *depende de:
  TASK-14*

## Integração contínua

- [x] **TASK-19 — CI (GitHub Actions).** Pipeline `restore → build → test → cobertura` a cada push,
  executando a suíte completa (incluindo integração via Testcontainers). — *depende de: TASK-17*

---

**Ordem de ataque:** Fase 0 → 1 → 2 → 3 → 4 → 5, com o ciclo TDD dentro de cada tarefa de código.
O teste de concorrência (**T-15**) é o marco central: valida a regra RN-01 contra um PostgreSQL real.
