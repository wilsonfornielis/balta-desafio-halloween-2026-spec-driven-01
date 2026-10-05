# Plano técnico

## Contexto
Implementa a [spec](spec.md) seguindo a [constituição](constitution.md): API em .NET 10 (Minimal API), SQLite via EF Core e testes com xUnit. Todo o código fica em `src/`.

## Arquitetura
```
src/
├── PasswordGenerator.slnx
├── Directory.Build.props              # net10.0, Nullable, TreatWarningsAsErrors
├── PasswordGenerator.Api/
│   ├── Program.cs                     # DI, ProblemDetails, mapeamento dos endpoints
│   ├── Endpoints/PasswordEndpoints.cs # só HTTP: recebe, chama serviço, traduz resposta
│   ├── Application/
│   │   ├── IPasswordGenerator.cs / PasswordGenerator.cs   # RN01–RN04
│   │   ├── PasswordService.cs         # orquestra validação, geração e persistência (RN05)
│   │   ├── ValidationException.cs
│   │   └── Contracts.cs               # CreatePasswordRequest, CreatePasswordResponse, PasswordResponse
│   ├── Domain/StoredPassword.cs       # entidade
│   └── Infrastructure/
│       ├── AppDbContext.cs
│       └── ValidationExceptionHandler.cs  # IExceptionHandler → 400 ProblemDetails
└── PasswordGenerator.Tests/
    ├── Unit/                          # PasswordGenerator e PasswordService
    └── Integration/                   # WebApplicationFactory, um teste por critério de aceite
```
Fluxo: `Endpoint → PasswordService → (PasswordGenerator, AppDbContext)`. O endpoint nunca usa o `AppDbContext`.

## Decisões
- **D01 – Projeto único para a API**, com pastas por camada. Duas rotas não justificam vários projetos.
- **D02 – Validação no serviço:** `PasswordService` lança `ValidationException` (tamanho fora de 16–128 ou `id` que não é GUID). Um `IExceptionHandler` global converte em `400` com ProblemDetails. Assim o endpoint não carrega regra de negócio.
- **D03 – Rota `GET /passwords/{id}` sem restrição `:guid`:** o `id` chega como `string` e é validado no serviço, para responder `400` em vez do `404` que a restrição daria.
- **D04 – Não encontrado:** o serviço retorna `null` e o endpoint responde `TypedResults.Problem(statusCode: 404)`.
- **D05 – Geração:** `RandomNumberGenerator.GetInt32` sobre 4 grupos (maiúsculas, minúsculas, dígitos, especiais `!@#$%^&*()-_=+[]{};:,.?/`). Garante 1 de cada grupo, completa com o conjunto total e embaralha (Fisher–Yates com o mesmo RNG). Nenhum grupo contém espaço.
- **D06 – Banco:** `Database.EnsureCreated()` na inicialização, sem migrations. Evita o pacote de design do EF e basta para um esquema de uma tabela. Connection string em `appsettings.json` (`Data Source=passwords.db`).
- **D07 – Corpo opcional no POST:** parâmetro `CreatePasswordRequest?`. Corpo ausente ou `length` nulo usa 16.
- **D08 – Testes:** unitários usam SQLite em memória (`DataSource=:memory:` com conexão aberta) para o `PasswordService`, sem mocks. Os de integração substituem o `DbContext` na `WebApplicationFactory` pela mesma configuração em memória.
- **D09 – Docker:** `Dockerfile` multi-stage em `src/` (build com `mcr.microsoft.com/dotnet/sdk:10.0`, runtime com `mcr.microsoft.com/dotnet/aspnet:10.0`, usuário não-root `app`, porta 8080) e `compose.yaml` com volume nomeado em `/app/data`. A connection string é sobrescrita por variável de ambiente (`ConnectionStrings__Default=Data Source=/app/data/passwords.db`) para o banco sobreviver à recriação do container. Não adiciona pacote NuGet.
- **D10 – OpenAPI + Swagger UI:** o documento é gerado pelo `Microsoft.AspNetCore.OpenApi` nativo do .NET 10 (`AddOpenApi` / `MapOpenApi` → `/openapi/v1.json`), e a interface vem do `Swashbuckle.AspNetCore.SwaggerUI` (`UseSwaggerUI` em `/swagger`, lendo `/openapi/v1.json`). Não usa o gerador do Swashbuckle, só a UI. Fica habilitado em todos os ambientes (RF03). Os endpoints declaram `WithName`, `WithSummary`, `WithDescription` e `ProducesValidationProblem()`; o GET declara também `ProducesProblem(404)`, já que `ProblemHttpResult` não informa status ao OpenAPI. 201/200 vêm dos `TypedResults`.

### Dependências (justificativa)
| Pacote | Projeto | Motivo |
|---|---|---|
| Microsoft.EntityFrameworkCore.Sqlite | Api | Persistência exigida pela constituição |
| xunit, xunit.runner.visualstudio, Microsoft.NET.Test.Sdk | Tests | Framework de testes exigido pela constituição |
| Microsoft.AspNetCore.Mvc.Testing | Tests | `WebApplicationFactory` para os testes de integração |
| Microsoft.AspNetCore.OpenApi | Api | Gera o documento OpenAPI (RF03); pacote oficial da Microsoft, sem gerador de terceiros |
| Swashbuckle.AspNetCore.SwaggerUI | Api | Interface Swagger UI (RF03); o .NET 10 não traz UI própria, e este pacote entrega só os arquivos estáticos da UI |

Nenhum outro pacote (sem FluentValidation, MediatR etc.).

## Modelo de dados
Tabela `Passwords` (entidade `StoredPassword`):

| Coluna | Tipo | Regra |
|---|---|---|
| Id | Guid (PK) | Gerado pela aplicação (`Guid.NewGuid()`) |
| Value | string(128), obrigatório | Senha em texto puro (RN06) |
| CreatedAt | DateTime (UTC), obrigatório | Momento da geração |

## Contratos
**`POST /passwords`**
- Request (opcional): `{ "length": 32 }`
- `201 Created` · `Location: /passwords/{id}` · `{ "id": "3f2b…" }`
- `400` ProblemDetails: `length` fora de 16–128

**`GET /passwords/{id}`**
- `200 OK` · `{ "id": "3f2b…", "password": "aB3!…", "createdAt": "2026-10-05T12:00:00Z" }`
- `400` ProblemDetails: `id` não é GUID
- `404` ProblemDetails: GUID inexistente

**Documentação**
- `GET /openapi/v1.json` · documento OpenAPI 3
- `GET /swagger` · Swagger UI

## Riscos
- **SQLite em memória fecha ao fechar a conexão:** manter uma `SqliteConnection` aberta durante cada teste.
- **`TreatWarningsAsErrors` com SDK RC/preview:** fixar a versão estável via `global.json` se aparecerem avisos do SDK.
- **`WebApplicationFactory` exige `Program` acessível:** adicionar `public partial class Program;` no `Program.cs`.
- **Corpo vazio no POST:** confirmar no teste de integração que corpo ausente não gera `400`/`415`.
- **Permissão de escrita no container:** o usuário `app` não escreve em `/app`; criar `/app/data` com dono `app` no `Dockerfile`.
- **Teste de aleatoriedade instável:** validar as regras (tamanho, grupos, sem espaço) em várias iterações, nunca valores fixos.
