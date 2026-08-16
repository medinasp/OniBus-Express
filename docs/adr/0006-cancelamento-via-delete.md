# ADR-0006 — Cancelamento via DELETE (soft-cancel)

- **Status:** aceito
- **Data:** 2026-08-14

## Contexto
Cancelar uma reserva é uma **transição de estado**: a reserva passa a `Cancelled` e continua
consultável, não é removida fisicamente. Resta definir como expor essa operação na API — o verbo
HTTP e a semântica do cancelamento.

## Opções consideradas
1. **Remoção física no `DELETE`** — apagaria a reserva; perde histórico e contraria a regra de que
   uma reserva cancelada permanece consultável (o assento é liberado apenas logicamente).
2. **`DELETE` como *soft-cancel`** — o verbo `DELETE` representa "encerrar" a reserva; internamente
   a reserva transita para `Cancelled`, o registro permanece e continua consultável por `GET`.

## Decisão
Cancelamento por `DELETE /reservations/{code}`, implementado como **soft-cancel**: o status passa a
`Cancelled`, o `cancelledAt` é registrado e a reserva permanece consultável. Retorna `200` com a
reserva no estado `Cancelled` (útil para o cliente confirmar a transição).

## Consequências
- Adere ao verbo esperado no contrato, sem apagar dados.
- O assento volta a ficar disponível (o índice único parcial só considera reservas `Confirmed`).
- Recancelar uma reserva já cancelada retorna `409` (RN-08).
