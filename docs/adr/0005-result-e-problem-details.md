# ADR-0005 — Erros de domínio via Result + Problem Details

- **Status:** aceito
- **Data:** 2026-08-14

## Contexto
Erros previsíveis (assento ocupado, viagem no passado, fora da janela de cancelamento) precisam de
um tratamento consistente, sem usar exceções para controle de fluxo.

## Opções consideradas
1. **Exceções para tudo** — controle de fluxo por exceção, custo e imprevisibilidade.
2. **Result na aplicação + Problem Details na borda** — o caso de uso devolve sucesso/falha; a
   borda HTTP converte a falha no formato padrão.

## Decisão
Erros de domínio trafegam como `Result` na camada de Aplicação e são traduzidos para
**Problem Details (RFC 7807)** apenas na borda HTTP. Exceções ficam para o realmente excepcional.

## Consequências
- Fluxo previsível e performático; respostas de erro padronizadas em Problem Details.
- É necessário um mapeador de `DomainError → status HTTP` na borda.
