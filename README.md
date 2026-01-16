# Limpidus MongoDB API

API REST desenvolvida em .NET 8 para gerenciamento de projetos, funcionários, áreas e atividades de limpeza. Utiliza MongoDB como banco de dados e integra com SQL Server para migração de dados do sistema legado.

## 📋 Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- MongoDB (local ou Atlas)
- SQL Server (opcional, apenas para migração de dados)
- Visual Studio 2022, VS Code ou Rider (recomendado)

## 🏗️ Estrutura do Projeto

```
limpdus-report-clean-check-back/
├── LimpidusMongoDB.API/              # Camada de API (Controllers, Configurações)
├── LimpidusMongoDB.Application/      # Camada de Aplicação (Services, Repositories, Entities)
├── LimpidusMongoDB.Tests/            # Testes Unitários e de Integração
└── LimpidusMongoDB.sln               # Solution file
```

### Arquitetura

O projeto segue os princípios de **Clean Architecture** e **SOLID**:

- **API**: Controllers, configuração de serviços e Swagger
- **Application**: Lógica de negócio, serviços, repositórios e entidades
- **Tests**: Testes unitários (Moq), integração (MongoDB real) e E2E (SQL Server + MongoDB)

## 🚀 Como Executar

### 1. Configurar Connection Strings

Edite o arquivo `LimpidusMongoDB.API/appsettings.json` ou `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "LimpidusDB": "mongodb+srv://usuario:senha@cluster.mongodb.net/?retryWrites=true&w=majority",
    "SqlServerConnection": "Data Source=servidor;Initial Catalog=banco;User ID=usuario;Password=senha;Encrypt=True;TrustServerCertificate=True"
  },
  "AppSettings": {
    "Database": "limpidus"
  }
}
```

### 2. Executar a API

```bash
# Navegar para a pasta da API
cd LimpidusMongoDB.API

# Restaurar dependências
dotnet restore

# Executar
dotnet run
```

A API estará disponível em:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger: `https://localhost:5001/swagger`

### 3. Executar Testes

```bash
# Todos os testes
dotnet test

# Apenas testes unitários
dotnet test --filter "Category=Unit"

# Apenas testes de integração
dotnet test --filter "Category=Integration"

# Apenas testes E2E (requer SQL Server)
dotnet test --filter "Category=E2E"
```

## 📦 Principais Funcionalidades

### Endpoints Principais

- **Projetos**: `GET/POST /v1/Project`
- **Funcionários**: `GET/POST /v1/Employee`
- **Áreas e Atividades**: `GET/POST /v1/AreaActivity`
- **Migração**: `POST /v1/Migration/from-sqlserver?legacyProjectId=4698`
- **Relatórios**: `GET /v1/Report`

### Migração de Dados

O endpoint de migração permite importar dados do sistema legado (SQL Server) para o MongoDB:

```bash
POST /v1/Migration/from-sqlserver?legacyProjectId=4698
```

Este endpoint:
1. Busca o projeto do SQL Server (`WORK_HEADER`)
2. Cria/atualiza o projeto no MongoDB
3. Busca funcionários do SQL Server (`WORK_FUNCIONARIO`)
4. Cria/atualiza funcionários no MongoDB
5. Migra áreas e tarefas (`WORK_AREA`, `WORK_TAREFAS`)

## 🔧 Tecnologias Utilizadas

- **.NET 8**: Framework principal
- **MongoDB.Driver 2.25.0**: Driver oficial do MongoDB
- **Microsoft.Data.SqlClient 5.2.0**: Acesso ao SQL Server
- **Swashbuckle.AspNetCore**: Documentação Swagger/OpenAPI
- **xUnit**: Framework de testes
- **Moq**: Mocking para testes unitários
- **FluentAssertions**: Assertions mais legíveis

## 📝 Estrutura de Dados

### Principais Coleções MongoDB

- **project**: Projetos de limpeza
- **employee**: Funcionários vinculados aos projetos
- **areaActivity**: Áreas e suas atividades/tarefas
- **operationalTask**: Tarefas operacionais
- **history**: Histórico de execuções

## 🧪 Testes

O projeto possui três tipos de testes:

1. **Unit Tests**: Testam lógica isolada usando mocks
2. **Integration Tests**: Testam persistência no MongoDB (base `limpidus-test`)
3. **E2E Tests**: Testam migração completa do SQL Server para MongoDB

### Configuração de Testes

Os testes de integração usam a base `limpidus-test` que pode ser configurada via variável de ambiente:

```bash
export TEST_MONGODB_CONNECTION_STRING="mongodb+srv://..."
```

## 📚 Documentação Adicional

- [Guia de Testes](./LimpidusMongoDB.Tests/README.md)
- [Troubleshooting E2E](./LimpidusMongoDB.Tests/Integration/Services/TROUBLESHOOTING_E2E.md)

## 🚢 Publicação

### Build para Produção

```bash
# Build otimizado
dotnet build -c Release

# Publicar para pasta
dotnet publish LimpidusMongoDB.API/LimpidusMongoDB.Api.csproj -c Release -o ./publish

# Publicar para Docker (se configurado)
docker build -t limpidus-api .
```

### Variáveis de Ambiente Recomendadas

- `ASPNETCORE_ENVIRONMENT`: `Production`
- `ConnectionStrings__LimpidusDB`: Connection string do MongoDB
- `ConnectionStrings__SqlServerConnection`: Connection string do SQL Server (opcional)

## 🤝 Contribuindo

1. Crie uma branch para sua feature
2. Implemente seguindo os padrões do projeto (SOLID, Clean Code)
3. Adicione testes para novas funcionalidades
4. Execute todos os testes antes de fazer commit
5. Abra um Pull Request

## 📄 Licença

[Adicione informações de licença aqui]

## 👥 Contato

[Adicione informações de contato aqui]
