# LimpidusMongoDB.Tests

Projeto de testes unitários e de integração para a solução LimpidusMongoDB, seguindo as melhores práticas de SOLID, Clean Code e TDD.

## Estrutura do Projeto

```
LimpidusMongoDB.Tests/
├── Integration/
│   ├── BaseIntegrationTest.cs          # Classe base para testes de integração
│   ├── MongoDbTestFixture.cs           # Fixture para configurar MongoDB de teste
│   └── Services/
│       └── MigrationServiceIntegrationTests.cs
├── Services/
│   └── MigrationServiceTests.cs        # Testes unitários com mocks
└── README.md
```

## Tecnologias Utilizadas

- **xUnit**: Framework de testes para .NET
- **Moq**: Biblioteca para criação de mocks e stubs
- **FluentAssertions**: Biblioteca para assertions mais legíveis e expressivas
- **Coverlet**: Ferramenta para cobertura de código
- **MongoDB.Driver**: Driver oficial do MongoDB para .NET

## Tipos de Testes

### Testes Unitários

Testes que usam mocks para isolar dependências e testar a lógica de negócio sem depender de recursos externos.

**Localização**: `Services/MigrationServiceTests.cs`

**Características**:
- Rápidos de executar
- Não dependem de recursos externos
- Usam mocks para todas as dependências
- Focam na lógica de negócio

### Testes de Integração

Testes que usam uma base de dados MongoDB real para validar a persistência e integração entre componentes.

**Localização**: `Integration/Services/MigrationServiceIntegrationTests.cs`

**Características**:
- Usam base de dados MongoDB real (`limpidus-test`)
- Testam persistência real dos dados
- Limpam dados automaticamente após cada teste
- Validam integração entre serviços e repositórios
- **Usam mocks para SQL Server** (não conectam ao SQL Server real)

### Testes End-to-End (E2E)

Testes que conectam a sistemas reais (SQL Server + MongoDB) para validar o fluxo completo de migração.

**Localização**: `Integration/Services/MigrationServiceE2ETests.cs`

**Características**:
- ✅ **Conectam ao SQL Server REAL** (não usam mocks)
- ✅ **Consultam dados reais** do sistema legado
- ✅ **Migram para `limpidus-test`** no MongoDB
- ✅ **Permitem comparação** com dados de produção
- ⚠️ **Requerem conexão** com SQL Server e MongoDB
- ⚠️ **São mais lentos** que testes unitários/integração

**📖 Veja o guia completo**: `Integration/Services/E2E_TESTS.md`

## Base de Dados de Teste

Os testes de integração usam uma base de dados MongoDB separada chamada **`limpidus-test`**:

- **Isolamento**: Dados de teste não interferem com produção
- **Limpeza Automática**: Dados são limpos após cada teste
- **Mesmo Cluster**: Usa o mesmo cluster MongoDB, mas base diferente
- **Configurável**: Pode ser configurada via variável de ambiente `TEST_MONGODB_CONNECTION_STRING`

### Configuração da Base de Teste

A base de dados de teste é configurada automaticamente através do `MongoDbTestFixture`:

```csharp
// Usa a mesma connection string, mas com base "limpidus-test"
var connectionString = Environment.GetEnvironmentVariable("TEST_MONGODB_CONNECTION_STRING")
    ?? "mongodb+srv://..."; // Connection string padrão
```

## Boas Práticas Aplicadas

### SOLID Principles

1. **Single Responsibility Principle (SRP)**
   - Cada classe de teste tem uma responsabilidade única
   - Métodos de teste focam em um único cenário

2. **Open/Closed Principle (OCP)**
   - Código aberto para extensão através de interfaces
   - Fechado para modificação através de abstrações

3. **Liskov Substitution Principle (LSP)**
   - Mocks implementam interfaces que podem ser substituídas

4. **Interface Segregation Principle (ISP)**
   - Interfaces específicas (`ISqlServerDataAccess`, `ISqlServerDataAccessFactory`)
   - Evita dependências desnecessárias

5. **Dependency Inversion Principle (DIP)**
   - Dependências são injetadas via construtor
   - Abstrações (`ISqlServerDataAccess`) em vez de implementações concretas

### Clean Code

1. **Nomenclatura Clara**
   - Nomes de testes descrevem o comportamento esperado
   - Padrão: `Método_QuandoCondicao_DeveResultado`

2. **Padrão AAA (Arrange-Act-Assert)**
   - **Arrange**: Configuração do cenário de teste
   - **Act**: Execução do método sob teste
   - **Assert**: Verificação do resultado esperado

3. **Testes Isolados**
   - Cada teste é independente
   - Uso de mocks para isolar dependências (testes unitários)
   - Limpeza automática de dados (testes de integração)

4. **Assertions Expressivas**
   - Uso de FluentAssertions para melhor legibilidade
   - Verificações claras e específicas

## Testes Implementados

### MigrationServiceTests (Unitários)

Cobre os seguintes cenários do serviço de migração usando mocks:

1. **MigrateFromSqlServerAsync_QuandoNaoEncontraAreas_DeveRetornarErro**
   - Verifica que quando não há áreas no SQL Server, retorna erro apropriado

2. **MigrateFromSqlServerAsync_QuandoEncontraAreasESalvaComSucesso_DeveRetornarSucesso**
   - Testa o fluxo completo de migração bem-sucedida

3. **MigrateFromSqlServerAsync_QuandoAreaJaExiste_DeveAtualizarEmVezDeCriar**
   - Verifica que áreas existentes são atualizadas em vez de duplicadas

4. **MigrateFromSqlServerAsync_QuandoSalvarFalha_DeveRetornarErro**
   - Testa tratamento de erro ao salvar no MongoDB

5. **MigrateFromSqlServerAsync_QuandoOcorreExcecao_DeveRetornarErro**
   - Verifica tratamento de exceções genéricas

6. **MigrateFromSqlServerAsync_QuandoTarefasSaoAgrupadasCorretamente_DeveMapearParaItems**
   - Testa o agrupamento e mapeamento de tarefas para items

7. **MigrateFromSqlServerAsync_QuandoHeaderIdEhMapeadoCorretamente_DeveUsarWorkAreaId**
   - Verifica que o `headerId` é mapeado corretamente do `WorkAreaId`

### MigrationServiceIntegrationTests (Integração)

Testa a persistência real dos dados no MongoDB:

1. **MigrateFromSqlServerAsync_QuandoPersisteDados_DeveSalvarNoMongoDB**
   - Verifica que os dados são realmente persistidos no MongoDB
   - Valida estrutura completa dos dados salvos

2. **MigrateFromSqlServerAsync_QuandoAreaJaExiste_DeveAtualizarEmVezDeDuplicar**
   - Testa que áreas existentes são atualizadas, não duplicadas
   - Valida que o ID é mantido

3. **MigrateFromSqlServerAsync_QuandoPersisteItems_DeveMapearFrequenciaCorretamente**
   - Valida o mapeamento de frequência dos items
   - Verifica conversão de período e frequência

4. **MigrateFromSqlServerAsync_QuandoNaoEncontraAreas_DeveRetornarErroSemPersistir**
   - Garante que nada é persistido quando não há áreas

## Executando os Testes

### Via CLI

```bash
# Executar todos os testes (unitários + integração)
dotnet test

# Executar apenas testes unitários
dotnet test --filter "FullyQualifiedName!~Integration"

# Executar apenas testes de integração (com mocks)
dotnet test --filter "FullyQualifiedName~Integration&FullyQualifiedName!~E2E"

# Executar apenas testes E2E (conecta ao SQL Server real)
dotnet test --filter "Category=E2E"

# Executar com verbosidade detalhada
dotnet test --verbosity normal

# Executar com cobertura de código
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### Via Visual Studio / Rider

- Use o Test Explorer para executar testes individuais ou todos
- Os testes aparecem automaticamente após o build
- Testes de integração podem ser executados separadamente

## Limpeza de Dados

Os testes de integração limpam automaticamente os dados após cada execução:

- **Após cada teste**: Todas as coleções são limpas via `Dispose()` do `BaseIntegrationTest`
- **Coleções limpas**: `areaActivity`, `project`, `employee`, `operationalTask`, `itemOperationalTask`, `history`, `itemHistory`, `user`, `justification`
- **Isolamento**: Cada teste começa com base limpa

## Cobertura de Código

Para gerar relatório de cobertura:

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput=./coverage.xml
```

## Variáveis de Ambiente

### TEST_MONGODB_CONNECTION_STRING

Configura a connection string do MongoDB para testes de integração.

**Padrão**: Usa a mesma connection string de produção

**Exemplo**:
```bash
export TEST_MONGODB_CONNECTION_STRING="mongodb+srv://user:pass@cluster.mongodb.net/?retryWrites=true&w=majority"
```

## Próximos Passos

- [ ] Adicionar testes de integração para outros serviços
- [ ] Adicionar testes de performance para operações críticas
- [ ] Configurar pipeline CI/CD para execução automática de testes
- [ ] Adicionar testes de carga para endpoints críticos
- [ ] Implementar testes de contrato (Contract Testing)

## Contribuindo

Ao adicionar novos testes:

1. **Testes Unitários**: Use mocks para todas as dependências externas
2. **Testes de Integração**: Use `BaseIntegrationTest` como classe base
3. **Siga o padrão AAA**: Arrange-Act-Assert
4. **Use nomenclatura descritiva**: `Método_QuandoCondicao_DeveResultado`
5. **Mantenha testes independentes**: Cada teste deve poder executar isoladamente
6. **Limpe dados**: Testes de integração devem limpar dados após execução

## Notas Importantes

⚠️ **Atenção**: Os testes de integração usam uma base de dados MongoDB real. Certifique-se de:
- Ter acesso ao cluster MongoDB configurado
- Usar uma base de dados de teste (`limpidus-test`) para não interferir com produção
- Os dados são limpos automaticamente, mas em caso de falha, pode ser necessário limpar manualmente
