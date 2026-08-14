# Constituição do Projeto — OniBus Express

> Documento fundador do Spec-Driven Development. Estabelece os princípios inegociáveis que
> governam todos os demais artefatos (spec, plan, tasks) e todo o código. Em caso de conflito
> entre esta constituição e qualquer outro documento, **esta constituição prevalece**.
> Emendas exigem nova versão e justificativa registrada. Versão 1.0.

---

## Princípios

### P1 — Especificação antes do código (SDD)
Nada é construído sem um artefato aprovado a montante. A ordem é imutável:
**constitution → spec → plan → tasks → implementação**. Código escrito sem spec aprovada
resolve o problema errado de forma correta.

### P2 — TDD é inegociável
Nenhuma linha de código de produção nasce sem um teste que falha antes. O ciclo é
**RED → GREEN → REFACTOR**, uma unidade de comportamento por vez. A lista de casos de teste é
derivada da spec (seção 11 do spec) antes de implementar, para não esquecer nenhum — mas os
testes são implementados incrementalmente, não todos de uma vez.

### P3 — Correção sob concorrência é requisito de integridade, não de código
A garantia de que um assento não é reservado duas vezes vive no **banco de dados** (restrição de
integridade), não apenas na camada de aplicação. Uma checagem "consulta-depois-insere" é
considerada defeito. Toda regra dependente de concorrência tem teste que a exercita em paralelo
contra um banco real.

### P4 — Arquitetura limpa, dependências apontando para dentro
Camadas: Domain ← Application ← Infrastructure/Api. O Domain é puro (sem EF, sem framework web).
Não se adiciona repositório genérico por reflexo — o `DbContext` já é *Unit of Work* + *Repository*;
só existem interfaces estreitas por agregado quando revelam intenção ou ajudam o teste. Cada
decisão arquitetural relevante vira um ADR.

### P5 — Erros previsíveis não usam exceções para controle de fluxo
Erros de negócio trafegam como *Result* na camada de aplicação e são traduzidos para
**Problem Details (RFC 7807)** apenas na borda HTTP. Exceções são para o excepcional.

### P6 — Determinismo temporal
Regras dependentes de tempo usam uma abstração de relógio injetável (`TimeProvider`).
`DateTime.Now` disperso é proibido. Datas são armazenadas e comparadas em **UTC**.

### P7 — Eficiência é requisito de primeira classe (green code)
Uso consciente de recursos e do coletor de lixo é obrigatório, não opcional: I/O assíncrona ponta
a ponta, *pooling* de conexões, leitura sem *tracking*, projeções para evitar N+1, paginação nas
listagens, ausência de alocação supérflua no caminho quente. O objetivo é demonstrar domínio da
linguagem e do GC.

### P8 — Observabilidade mínima desde o início
Health check, logs estruturados e um identificador de correlação por requisição existem desde o
primeiro endpoint.

### P9 — Padrões de documentação e autoria
A documentação é escrita em **primeira pessoa do singular ou de forma impessoal** — nunca em
primeira pessoa do plural nem citando assistentes. O código **não leva comentários**, exceto os
de documentação (`///` XML) nos endpoints, que devem ser completos o bastante para operar a API
pelo Swagger. O README é em português; identificadores de código, em inglês.

### P10 — Reprodutibilidade
O projeto executa de duas formas: com Docker (`docker compose up`) e sem Docker (`dotnet run`
contra um PostgreSQL local). O banco é **PostgreSQL único**; provider *in-memory* é proibido,
inclusive em teste (integração usa PostgreSQL real via Testcontainers).

### P11 — Disciplina de versionamento
Commits pequenos, incrementais e planejados, no padrão **Conventional Commits**, provando o
planejamento ponto a ponto. Sem rodapé de coautoria — autoria única do responsável pelo projeto.

### P12 — Qualidade sobre quantidade
Correção, testes e clareza vêm antes de quantidade de funcionalidades. Over-engineering é
defeito. Análises de capacidade, custo em nuvem e plano de escala são valiosas, mas entram como
**apêndice claramente marcado como visão de evolução**, jamais inflando o núcleo do MVP.

---

## Governança

- Esta constituição precede e restringe `spec.md`, `plan.md`, `tasks.md` e todo o código.
- Qualquer violação encontrada em revisão é bloqueante até ser corrigida ou a constituição ser
  emendada com justificativa.
- Emendas incrementam a versão deste documento e registram o motivo.
