# ADR-0003 — PostgreSQL como banco relacional

- **Status:** aceito
- **Data:** 2026-08-14

## Contexto
O sistema exige um banco relacional que suporte a garantia de integridade da reserva de assento
(índice único parcial) e permita testes de concorrência contra um banco real.

## Opções consideradas
1. **PostgreSQL 16** — índice único parcial nativo, robusto, gratuito, integração excelente com
   Testcontainers.
2. **SQL Server** — sólido no ecossistema .NET, mas imagem mais pesada e sem vantagem para este caso.

## Decisão
PostgreSQL 16, único banco do projeto, tanto em produção quanto nos testes de integração
(via Testcontainers).

## Consequências
- Índice parcial disponível de fábrica, essencial para a estratégia de concorrência (ver ADR-0008).
- Um só motor de banco em todos os ambientes, reduzindo divergência entre teste e produção.
