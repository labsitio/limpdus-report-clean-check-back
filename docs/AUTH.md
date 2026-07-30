# Autenticação JWT — Clean Check API

## Papéis

| Role | Login | Acesso a projetos | Relatório |
|------|--------|-------------------|-----------|
| **Franqueado** | `POST /v1/Auth/login` (FRANQ_LOGIN, nó folha) | Só os dele (`WORK_HEADER.ID_DONO` + `WORK_HEADER_SHARE`) | Completo (export / justificativas) |
| **Consultor** | Mesmo login; detectado se o nó em `TBL_NIVEIS_GRUPO` tem filhos | Carteira = `ID_DONO` de franqueados nos nós **abaixo** (+ próprio nó se `VER_NIVEL`), filtrado por `FRANQ_REGIOES` / `VIEW_FRANQ_NIVEIS` (alinhar LimpCalc `Niveis.Children` + `Project.List`); + share do usuário | Completo |
| **Admin** | Mesmo login + `GRUPOS_USER` grupo `1` | Todos os projetos migrados (Mongo); JWT com bypass por role | Completo |
| **ProjectViewer** | Login do `WORK_HEADER` | 1 projeto | Campos sensíveis ocultos |

Senha de franqueado: MD5 hex (mesmo algoritmo do LimpCalc / `Business.Security.Criptografar`).

Consultor e Franqueado recebem `allowedProjects` intersectado com projetos existentes no Mongo (quando há dados migrados), para o combo do front listar só o que o Clean Check conhece.

## Configuração

```json
"ConnectionStrings": {
  "SqlServerDB": "Data Source=...;Initial Catalog=limpcalc;..."
},
"Jwt": {
  "Key": "mínimo 32 caracteres — use User Secrets / env Jwt__Key em produção",
  "Issuer": "limpidus-clean-check",
  "Audience": "limpidus-clean-check",
  "ExpirationHours": 12
}
```

Variável de ambiente: `Jwt__Key`.

## Breaking change (mobile)

Todos os endpoints (exceto `/v1/Auth/*` e `/v1/HealthCheck`) exigem `Authorization: Bearer {token}`.

Fluxo recomendado no app de campo:

1. `POST /v1/Auth/project` com `{ "login", "password", "type": "project" }`
2. Guardar `token`
3. Enviar header em todas as chamadas

## Login web (auto-detect)

```http
POST /v1/Auth/login
Content-Type: application/json

{ "type": "auto", "login": "usuario", "password": "senha" }
```

Ordem: FRANQ_LOGIN (Admin / Consultor / Franqueado) -> se falhar, WORK_HEADER (ProjectViewer). type pode ser omitido (default auto). Atalhos /franqueado e /project permanecem para mobile.

## Exemplo login projeto

```http
POST /v1/Auth/login
Content-Type: application/json

{ "type": "project", "login": "meuLogin", "password": "minhaSenha" }
```

## Exemplo login franqueado / consultor

```http
POST /v1/Auth/login
Content-Type: application/json

{ "type": "franqueado", "login": "usuarioFranq", "password": "senha" }
```

A resposta traz `role`: `Admin` | `Consultor` | `Franqueado` conforme hierarquia/`GRUPOS_USER`.

## Gestão de usuários (somente Admin)

Lista franqueados ativos e permite marcar/desmarcar Admin (`GRUPOS_USER` grupo `1`). Consultor não é um grupo SQL: é inferido no login pela árvore `TBL_NIVEIS_GRUPO`.

```http
GET /v1/Users
Authorization: Bearer {tokenAdmin}
```

```http
PUT /v1/Users/{franqId}/admin
Authorization: Bearer {tokenAdmin}
Content-Type: application/json

{ "isAdmin": true }
```

Regras: só role Admin; não é permitido remover o próprio Admin; outros grupos do franqueado não são alterados.
