# ADR-0001 — Arquitetura em camadas (Clean Architecture)

- **Status:** aceito
- **Data:** 2026-08-14

## Contexto
A regra de negócio (reservas, concorrência, validações) precisa ser isolada da infraestrutura
(banco, web) para ser testável, legível e evoluível sem efeitos colaterais.

## Opções consideradas
1. **CRUD acoplado ao EF** numa camada única — rápido de começar, mas mistura regra e persistência
   e dificulta o teste isolado.
2. **Clean Architecture** (Domain / Application / Infrastructure / Api), dependências para dentro.
3. **Vertical Slice** — bom para muitas features independentes; menos vantajoso num domínio pequeno.

## Decisão
Clean Architecture enxuta em quatro camadas, com o `Domain` puro (sem EF nem HTTP) e as dependências
sempre apontando para dentro. Um teste de arquitetura reprova o build se a regra for violada.

## Consequências
- Ganho de testabilidade e separação de responsabilidades.
- Mais projetos/arquivos que um CRUD simples — custo aceitável para o valor de isolamento.
