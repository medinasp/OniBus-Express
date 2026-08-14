# ADR-0008 — Prevenção de double-booking via índice único parcial

- **Status:** aceito
- **Data:** 2026-08-14

## Contexto
Duas requisições simultâneas podem tentar reservar o mesmo assento da mesma viagem. É a regra mais
crítica do sistema (RN-01) e não pode resultar em reserva duplicada sob nenhum nível de concorrência.

## Opções consideradas
1. **Checagem em código (consulta-depois-insere)** — sujeita a condição de corrida: ambas as
   requisições leem "livre" e inserem.
2. **Bloqueio pessimista** (lock de linha) — correto, porém serializa e adiciona contenção.
3. **Índice único parcial no banco** — a unicidade do assento vale apenas para reservas
   `Confirmed`; a segunda inserção é recusada pelo banco.

## Decisão
Garantir a unicidade no banco com um índice único **parcial**:
`UNIQUE (trip_id, seat_number) WHERE status = 'Confirmed'`. A violação de unicidade é traduzida em
`409 Conflict`. Um teste de integração dispara requisições paralelas contra o mesmo assento e exige
que exatamente uma vença.

## Consequências
- Correto sob qualquer concorrência, sem contenção de lock explícito.
- Permite reservar o assento novamente após um cancelamento (a condição exclui `Cancelled`).
- Fornece **idempotência** contra envios duplicados sem custo adicional.
- Depende de traduzir a exceção de unicidade do PostgreSQL (`23505`) na borda de Infraestrutura.
