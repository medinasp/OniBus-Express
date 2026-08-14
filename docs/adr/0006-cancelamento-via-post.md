# ADR-0006 — Cancelamento via POST /cancellation

- **Status:** aceito
- **Data:** 2026-08-14

## Contexto
Cancelar uma reserva é uma **transição de estado**: a reserva passa a `Cancelled` e continua
consultável, não é removida.

## Opções consideradas
1. **`DELETE /reservations/{code}`** — sugere remoção do recurso, o que não corresponde ao
   comportamento (a reserva permanece).
2. **`POST /reservations/{code}/cancellation`** — cria um "cancelamento" sob a reserva; expressa a
   transição de estado.

## Decisão
Cancelamento por `POST /reservations/{code}/cancellation`, retornando `200` com a reserva em
`Cancelled`.

## Consequências
- Semântica REST coerente com o comportamento real.
- Rota um pouco mais verbosa que um `DELETE` — custo aceitável pela clareza.
