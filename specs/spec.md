# Gerador de Senhas Fortes

## Problema
Criar senhas fortes na mão é trabalhoso e propenso a erro, e não há um lugar simples para recuperar depois uma senha gerada.

## Objetivo
Uma API com dois endpoints: um gera uma senha forte, guarda no banco e devolve um GUID; o outro consulta a senha a partir desse GUID.

## Usuários
- Desenvolvedor ou cliente HTTP que consome a API (sem autenticação)

## Histórias
- Como consumidor, quero gerar uma senha forte para não precisar criá-la manualmente
- Como consumidor, quero consultar uma senha pelo GUID para recuperá-la depois

## Requisitos funcionais
- **RF01 – Gerar senha:** `POST /passwords`
  - Corpo opcional: `{ "length": int }`. Se não for informado, o tamanho é 16
  - Gera a senha, persiste e responde `201 Created` com `{ "id": guid }` e o header `Location: /passwords/{id}`
- **RF02 – Consultar senha:** `GET /passwords/{id}`
  - Responde `200 OK` com `{ "id": guid, "password": string, "createdAt": datetime }`
- **RF03 – Documentação interativa:**
  - Documento OpenAPI em `GET /openapi/v1.json`
  - Swagger UI em `/swagger`, permitindo testar os endpoints pelo navegador
  - Disponível em todos os ambientes, inclusive no container Docker (projeto lúdico)

## Regras de negócio
- **RN01:** a senha tem no mínimo 16 e no máximo 128 caracteres
- **RN02:** a senha contém pelo menos 1 letra maiúscula, 1 letra minúscula, 1 dígito e 1 caractere especial
- **RN03:** a senha não contém espaços em branco
- **RN04:** os caracteres são escolhidos com gerador aleatório criptograficamente seguro
- **RN05:** toda senha gerada é persistida e identificada por um GUID gerado pela aplicação
- **RN06:** a senha é armazenada em texto puro (projeto lúdico, sem criptografia)

## Casos de borda
- `length` menor que 16 ou maior que 128 → `400` com ProblemDetails
- Corpo ausente ou vazio → usa o tamanho padrão 16
- `id` que não é um GUID válido → `400` com ProblemDetails
- `id` válido, mas inexistente → `404` com ProblemDetails
- Duas gerações seguidas produzem senhas e GUIDs diferentes

## Fora de escopo
- Autenticação e autorização
- Criptografia ou hash da senha
- Listagem, edição ou exclusão de senhas
- Escolha do conjunto de caracteres pelo usuário
- Interface gráfica própria (o Swagger UI é só documentação/teste)

## Critérios de aceite
- `POST /passwords` sem corpo retorna `201` com um GUID, e a senha consultada tem 16 caracteres e atende RN02 e RN03
- `POST /passwords` com `length: 32` gera uma senha de 32 caracteres
- `POST /passwords` com `length: 10` retorna `400` com ProblemDetails
- `GET /passwords/{id}` de uma senha gerada retorna `200` com a mesma senha
- `GET /passwords/{id}` com GUID inexistente retorna `404`
- `GET /passwords/abc` retorna `400` com ProblemDetails
- `GET /openapi/v1.json` retorna `200` e descreve `POST /passwords` e `GET /passwords/{id}` com seus status (201/400; 200/400/404)
- `GET /swagger` abre o Swagger UI apontando para o documento OpenAPI
