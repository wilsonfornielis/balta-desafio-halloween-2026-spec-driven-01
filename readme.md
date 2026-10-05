<img width="100%" alt="Desafio Halloween 2026 - Desafio 01" src="docs/img/IMG-DESAFIO.png" />

## 🎃 Halloween - Desafio 1

Oi, eu sou o **Wilson Fornielis** e este é o espaço onde compartilho minha jornada de aprendizado durante o desafio **Halloween 2026**, realizado pelo [balta.io](https://balta.io). 👻

Aqui você vai encontrar a especificação, o código e os testes que desenvolvi neste primeiro desafio.

### Sobre este desafio
Neste desafio o objetivo é consolidar os fundamentos do **Spec-Driven Development (SDD)**: escrever manualmente uma constituição, uma especificação, um plano técnico e as tarefas, e depois usar IA para implementar exatamente o que foi especificado.

O resultado é uma **API geradora de senhas fortes**, com dois endpoints: um gera e armazena a senha, devolvendo um GUID, e o outro consulta a senha a partir desse GUID.

## Problema
Criar senhas fortes na mão é trabalhoso e propenso a erro, e não há um lugar simples para recuperar depois uma senha gerada. A API gera a senha, guarda no banco e devolve um GUID para consulta posterior.

> ⚠️ Projeto lúdico, só para aprendizado: as senhas ficam em texto puro e não há autenticação. **Não use em produção.**

## Spec-Driven Development
Toda a implementação nasceu dos arquivos em [`specs/`](specs), nesta ordem:

| Arquivo | Conteúdo |
|---|---|
| [constitution.md](specs/constitution.md) | Regras que valem para o projeto inteiro: stack, arquitetura, qualidade, convenções e governança |
| [spec.md](specs/spec.md) | O que a API faz: requisitos funcionais (RF01–RF03), regras de negócio (RN01–RN06), casos de borda e critérios de aceite |
| [plan.md](specs/plan.md) | Como fazer: arquitetura, decisões técnicas (D01–D10), dependências justificadas, modelo de dados e contratos |
| [tasks.md](specs/tasks.md) | 17 tarefas rastreáveis, cada uma ligada à sua origem e com critério de conclusão |

Quando surgiu algo fora do previsto (Docker e Swagger), primeiro atualizei constituição, spec, plano e tarefas, e só depois o código.

## O que foi implementado

### Stack
- .NET 10, C# e Minimal API
- SQLite com EF Core
- xUnit para testes
- OpenAPI nativo do .NET 10 + Swagger UI
- Docker / Docker Compose

### Endpoints
| Método | Rota | Descrição | Respostas |
|---|---|---|---|
| `POST` | `/passwords` | Gera uma senha forte. Corpo opcional `{ "length": 32 }` (16 a 128, padrão 16) | `201` com `{ id }` · `400` |
| `GET` | `/passwords/{id}` | Consulta a senha pelo GUID | `200` com `{ id, password, createdAt }` · `400` · `404` |
| `GET` | `/openapi/v1.json` | Documento OpenAPI | `200` |
| `GET` | `/swagger` | Swagger UI | `200` |

Erros sempre retornam **ProblemDetails**.

### Regras da senha
- Entre 16 e 128 caracteres
- Pelo menos 1 letra maiúscula, 1 minúscula, 1 dígito e 1 caractere especial (`!@#$%^&*()-_=+[]{};:,.?/`)
- Nenhum espaço em branco
- Gerada com `RandomNumberGenerator` (aleatoriedade criptográfica)

### Arquitetura
```
src/
├── PasswordGenerator.Api/
│   ├── Endpoints/       # só HTTP: recebe, chama o serviço e devolve a resposta
│   ├── Application/     # gerador de senhas, PasswordService (regras) e contratos
│   ├── Domain/          # entidade StoredPassword
│   └── Infrastructure/  # AppDbContext e tratamento de erros (ProblemDetails)
└── PasswordGenerator.Tests/
    ├── Unit/            # gerador e serviço
    └── Integration/     # endpoints e documentação via WebApplicationFactory
```
Fluxo: `Endpoint → PasswordService → (PasswordGenerator, AppDbContext)`. O endpoint nunca acessa o banco diretamente.

### Qualidade
- **32 testes** passando: unitários para toda regra de negócio e de integração para cada critério de aceite
- Build com `TreatWarningsAsErrors`: **0 avisos e 0 erros**
- Testes usam SQLite em memória, sem mocks

## Como executar

### 🐳 Rodando com Docker (recomendado)

**Pré-requisito:** [Docker Desktop](https://www.docker.com/products/docker-desktop/) instalado e em execução. Não é preciso ter o .NET instalado, porque a imagem compila o projeto.

**1. Clone o repositório e entre na pasta `src`**
```bash
git clone https://github.com/wilsonfornielis/balta-desafio-halloween-2026-spec-driven-01
cd balta-desafio-halloween-2026-spec-driven-01/src
```

**2. Construa a imagem e suba o container**
```bash
docker compose up -d --build
```
Isso cria a imagem `password-generator-api:latest`, sobe o container `password-generator-api` na porta **8080** e cria o volume `password-generator_passwords-data`, onde fica o banco SQLite.

**3. Confira se está rodando**
```bash
docker compose ps
```
O status deve aparecer como `Up`, com a porta `0.0.0.0:8080->8080/tcp`.

**4. Acesse**
- API: http://localhost:8080
- Swagger UI: http://localhost:8080/swagger

**Comandos úteis**
| Ação | Comando |
|---|---|
| Ver os logs | `docker logs -f password-generator-api` |
| Parar e remover o container (mantém o banco) | `docker compose down` |
| Parar e apagar também o banco | `docker compose down -v` |
| Atualizar após mudar o código | `docker compose up -d --build` |

> 💾 As senhas ficam no volume e continuam disponíveis mesmo depois de recriar o container.

### 💻 Rodando localmente (sem Docker)
Requer o [.NET 10 SDK](https://dotnet.microsoft.com/download).
```bash
cd src
dotnet test                                   # roda os 32 testes
dotnet run --project PasswordGenerator.Api    # sobe em http://localhost:5297
```
Swagger local: http://localhost:5297/swagger

## 📖 Usando a API pelo Swagger

Com a API rodando, abra **http://localhost:8080/swagger** no navegador. Os endpoints aparecem agrupados em **Passwords**.

### 1. Gerar uma senha (`POST /passwords`)
1. Clique em **POST /passwords** para expandir
2. Clique em **Try it out**
3. No **Request body**, o Swagger sugere `{ "length": 0 }`. Ajuste para uma destas opções:
   - `{ "length": 32 }`, ou qualquer valor de 16 a 128, para escolher o tamanho
   - `{}`, ou apague todo o conteúdo, para usar o tamanho padrão de 16
4. Clique em **Execute**
5. Em **Server response**, confira:
   - **Code:** `201`
   - **Response body:** `{ "id": "5f204d8a-fb09-494f-ac6d-75a7730cd0fb" }`
6. **Copie o `id`**: ele é a única forma de consultar a senha depois

### 2. Consultar a senha (`GET /passwords/{id}`)
1. Clique em **GET /passwords/{id}** e depois em **Try it out**
2. Cole o `id` copiado no campo **id**
3. Clique em **Execute**
4. A resposta vem com **Code** `200`:
```json
{
  "id": "5f204d8a-fb09-494f-ac6d-75a7730cd0fb",
  "password": "mUlQi=WfO789-@lw",
  "createdAt": "2026-10-05T23:17:08.8659411Z"
}
```

### 3. Testando os erros
| Teste | Onde | Resultado esperado |
|---|---|---|
| `{ "length": 10 }` (menor que 16) | POST | `400` com `errors.length` |
| `{ "length": 200 }` (maior que 128) | POST | `400` com `errors.length` |
| id `abc` (não é GUID) | GET | `400` com `errors.id` |
| GUID que não existe, ex.: `00000000-0000-0000-0000-000000000001` | GET | `404` "Senha não encontrada." |

Todos os erros voltam no formato **ProblemDetails**, por exemplo:
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Erro de validação.",
  "status": 400,
  "errors": { "length": ["O tamanho deve estar entre 16 e 128."] }
}
```

> 💡 Em **Schemas**, no fim da página, ficam os modelos de requisição e resposta. O documento OpenAPI bruto está em http://localhost:8080/openapi/v1.json.

### Usando via terminal (curl)
```bash
# gerar senha com o tamanho padrão (16)
curl -X POST http://localhost:8080/passwords

# gerar senha com 32 caracteres
curl -X POST http://localhost:8080/passwords -H "Content-Type: application/json" -d '{"length":32}'

# consultar (use o id retornado)
curl http://localhost:8080/passwords/{id}
```

## Neste processo eu aprendi
* ✅ Separar **o que** (spec) de **como** (plano) e de **quando** (tarefas) deixa a implementação com IA previsível
* ✅ A constituição evita decisões repetidas: dependências, arquitetura e padrões de erro já estavam definidos
* ✅ Critérios de aceite escritos antes viram testes de integração quase diretamente
* ✅ Mudanças de escopo (Docker, Swagger) entram primeiro nos specs, mantendo a rastreabilidade
* ✅ Testes e validação manual pegam detalhes que a spec não previu, como JSON malformado retornando 500 ou datas sem fuso UTC

## Badge
<img src="https://baltaio.blob.core.windows.net/static/images/v4/challenges/halloween-2026/01.png" width="200" />

## Sobre o Halloween 2026
O desafio **Halloween 2026** consiste em implementar o modelo Spec-Driven Development de ponta a ponta, criando apps completas com IA.

### Veja meu progresso no desafio
[github.com/wilsonfornielis](https://github.com/wilsonfornielis)
