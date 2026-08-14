# ADR-0009 — Minimal APIs organizadas em módulos

- **Status:** aceito
- **Data:** 2026-08-14

## Contexto
Os seis endpoints precisam ser expostos numa arquitetura em camadas, sem concentrar tudo no
`Program.cs` nem inflar a superfície da API.

## Opções consideradas
1. **Controllers** — muita convenção embutida (filtros, *model binding*); estrutura tradicional,
   porém mais cerimônia para um número pequeno de endpoints.
2. **Minimal APIs em módulos** — endpoints agrupados por recurso com `MapGroup` e métodos de
   extensão; `Program.cs` fino, apenas compondo DI e pipeline.

## Decisão
Minimal APIs agrupadas em módulos por recurso (`RouteEndpoints`, `TripEndpoints`,
`ReservationEndpoints`). Cada endpoint é um adaptador fino que delega ao caso de uso da camada de
Aplicação, sem lógica de negócio.

## Consequências
- `Program.cs` enxuto; melhor desempenho de *startup*; abordagem atual do .NET 8.
- Exige disciplina de organização em módulos — mitigada pela convenção de um arquivo por recurso.
