# ADR-0004 — Sem repositório genérico

- **Status:** aceito
- **Data:** 2026-08-14

## Contexto
Há uma tentação comum de criar uma abstração `IRepository<T>` genérica sobre o EF Core.

## Opções consideradas
1. **Repositório genérico** sobre o EF — costuma apenas duplicar o que o `DbContext` já oferece e
   esconde recursos do ORM (LINQ, `Include`, *change tracking*).
2. **`DbContext` direto + interfaces estreitas por agregado** — o `DbContext` já é *Unit of Work* +
   *Repository*; interfaces específicas (ex.: `IReservationRepository`) só onde revelam intenção.

## Decisão
Não adotar repositório genérico. Usar o `DbContext` como *Unit of Work*/repositório e expor
interfaces estreitas por agregado apenas onde ajudam o domínio ou o teste.

## Consequências
- Menos *boilerplate*; aproveita todo o poder do EF Core.
- Acoplamento controlado ao EF fica confinado à camada de Infraestrutura.
