# ADR-0007 — Geração do código de reserva

- **Status:** aceito
- **Data:** 2026-08-14

## Contexto
A reserva precisa de um identificador público, único e **legível** (fácil de ditar por telefone),
no formato `ABC-12345`.

## Opções consideradas
1. **GUID exposto** — único, porém ilegível e impraticável de ditar.
2. **Sequência numérica** — previsível e enumerável por terceiros.
3. **Código curto legível com verificação de unicidade** — 3 letras + 5 dígitos, alfabeto sem
   caracteres ambíguos (`I`, `O`), unicidade garantida por índice único e nova tentativa em colisão.

## Decisão
Código no formato `ABC-12345`, gerado por uma fábrica; unicidade assegurada por índice único na
coluna `code`, com regeneração em caso de colisão.

## Consequências
- Identificador legível e amigável ao atendimento.
- Colisões (raras) exigem nova tentativa de geração — tratada no fluxo.
