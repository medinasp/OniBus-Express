# ADR-0002 — Framework-alvo .NET 8 (LTS)

- **Status:** aceito
- **Data:** 2026-08-14

## Contexto
É preciso fixar o *target framework*. O ambiente dispõe de SDKs 8, 9 e 10.

## Opções consideradas
1. **`net8.0` (LTS)** — suporte estendido, amplamente disponível em ambientes de produção.
2. **`net10.0`** — mais recente, porém sem ganho para este escopo e com menor disponibilidade nos
   ambientes-alvo.

## Decisão
Alvo `net8.0`, com o SDK fixado por `global.json` (`rollForward: latestFeature`) para build
reproduzível mesmo com SDKs mais novos instalados.

## Consequências
- Estabilidade e portabilidade máximas.
- Recursos exclusivos de versões mais novas não são usados — irrelevante para o MVP.
