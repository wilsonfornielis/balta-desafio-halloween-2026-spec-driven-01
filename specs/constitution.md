# Constituição do projeto

## Stack
- .NET 10, C#, Minimal API
- Persistência em SQLite com EF Core
- Testes com xUnit
- Documentação da API com OpenAPI e Swagger UI

## Arquitetura
- Endpoint não acessa o banco diretamente; sempre via serviço de aplicação
- Regra de negócio não vive no endpoint nem no handler HTTP
- Nenhuma dependência nova entra sem justificativa escrita no plano

## Qualidade
- Toda regra de negócio tem teste unitário
- Todo endpoint tem ao menos um teste de integração de caminho feliz
- Erro de validação sempre retorna 400 com ProblemDetails
- Todo endpoint é documentado no OpenAPI com resumo e todos os status de resposta possíveis
- Pronto = compila sem erros ou warnings, todos os testes passam e a aplicação roda sem erros

## Convenções
- Código, nomes e rotas em inglês; documentação em português
- Identificadores públicos de recursos são GUID
- Projeto lúdico, só para aprendizado: sem criptografia nem recursos de segurança, não usar em produção

## Governança
- Esta constituição prevalece sobre spec, plano e tarefas
- Mudanças nela são feitas de forma explícita, antes da implementação que dependa delas
- A IA implementa somente o que está nas tarefas; desvios voltam para spec/plano
