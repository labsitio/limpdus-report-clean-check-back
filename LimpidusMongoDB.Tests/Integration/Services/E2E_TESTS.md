# Testes End-to-End (E2E) - MigrationService

## 📋 O que são Testes E2E?

Os testes E2E (`MigrationServiceE2ETests.cs`) são testes de integração **completos** que:

✅ **Conectam ao SQL Server REAL** (não usam mocks)  
✅ **Consultam dados reais** do sistema legado  
✅ **Migram para a base `limpidus-test`** no MongoDB  
✅ **Permitem comparação** com dados de produção  

## ⚠️ Importante

Estes testes **NÃO violam a estrutura de testes** - são uma categoria especial de testes de integração que validam o fluxo completo end-to-end.

## 🎯 Quando Usar

- ✅ Validar migração completa de um projeto real
- ✅ Comparar dados migrados com dados de produção
- ✅ Testar integração completa SQL Server → MongoDB
- ✅ Validar mapeamentos e conversões de dados

## 🚀 Como Executar

### Opção 1: Executar um teste específico

Primeiro, **remova o `Skip`** do teste que deseja executar:

```csharp
// De:
[Fact(Skip = "Teste E2E - Conecta ao SQL Server real...")]

// Para:
[Fact] // ou [Fact(Skip = "false")]
```

Depois execute:

```bash
# Executar teste específico
dotnet test --filter "FullyQualifiedName~MigrateFromSqlServerAsync_QuandoMigraProjetoReal_DeveSalvarDadosCorretamente"
```

### Opção 2: Executar todos os testes E2E

```bash
# Executar todos os testes E2E (após remover Skip)
dotnet test --filter "Category=E2E"
```

### Opção 3: Executar via Test Explorer

1. Abra o Test Explorer no Visual Studio/Rider
2. Filtre por "E2E" ou "MigrationServiceE2ETests"
3. Execute o teste desejado

## ⚙️ Configuração

### Connection String do SQL Server

O teste usa a connection string nesta ordem de prioridade:

1. **Variável de ambiente** `TEST_SQLSERVER_CONNECTION_STRING`
2. **Fallback**: Connection string padrão do appsettings.json

#### Configurar via Variável de Ambiente

```bash
# Linux/Mac
export TEST_SQLSERVER_CONNECTION_STRING="Data Source=sql2.limpidus.com.br;Initial Catalog=limpcalc;User ID=limpcalc;Password=Limp741852963"

# Windows (PowerShell)
$env:TEST_SQLSERVER_CONNECTION_STRING="Data Source=sql2.limpidus.com.br;Initial Catalog=limpcalc;User ID=limpcalc;Password=Limp741852963"
```

### Projeto ID

Por padrão, os testes usam o projeto **4698** (que foi migrado anteriormente).

Para testar outro projeto, altere no código:

```csharp
const int realProjectId = 4698; // ← Altere aqui
```

## 📊 Testes Disponíveis

### 1. `MigrateFromSqlServerAsync_QuandoMigraProjetoReal_DeveSalvarDadosCorretamente`

**O que testa:**
- Conecta ao SQL Server real
- Consulta áreas e tarefas do projeto
- Migra para `limpidus-test`
- Valida estrutura dos dados salvos

**Validações:**
- ✅ Migração bem-sucedida
- ✅ Dados persistidos no MongoDB
- ✅ Áreas têm nome e headerId
- ✅ Items têm nome e frequência

### 2. `MigrateFromSqlServerAsync_QuandoMigraProjetoReal_DeveMapearHeaderIdCorretamente`

**O que testa:**
- Valida que `headerId` corresponde ao `WORK_AREA_ID` do SQL Server

**Validações:**
- ✅ headerId não é vazio
- ✅ headerId é um número válido (WorkAreaId)

### 3. `MigrateFromSqlServerAsync_QuandoMigraProjetoReal_DeveMapearFrequenciaCorretamente`

**O que testa:**
- Valida mapeamento de frequência e período

**Validações:**
- ✅ Items têm frequência mapeada
- ✅ Tipo de frequência está correto

### 4. `MigrateFromSqlServerAsync_QuandoProjetoNaoExiste_DeveRetornarErro`

**O que testa:**
- Tratamento de erro para projeto inexistente

**Validações:**
- ✅ Retorna erro apropriado
- ✅ Mensagem indica que não encontrou áreas

## 🔍 Consultar Dados Migrados

Após executar os testes, os dados estarão na base **`limpidus-test`**:

### Via MongoDB Compass

1. Conecte ao cluster MongoDB
2. Selecione a base **`limpidus-test`**
3. Navegue pela coleção **`areaActivity`**
4. Filtre por `projectId: 4698`

### Via mongosh

```javascript
// Conectar
mongosh "mongodb+srv://producao:7diEnLIjhtCa5Xxr@cluster0.nmool17.mongodb.net/..."

// Selecionar base de teste
use limpidus-test

// Consultar áreas migradas
db.areaActivity.find({ projectId: 4698 }).pretty()

// Contar áreas
db.areaActivity.countDocuments({ projectId: 4698 })

// Ver estrutura de uma área
db.areaActivity.findOne({ projectId: 4698 })
```

## 🔄 Comparar com Produção

### Comparar dados de teste vs produção

```javascript
// Dados de TESTE (limpidus-test)
use limpidus-test
var testAreas = db.areaActivity.find({ projectId: 4698 }).toArray()

// Dados de PRODUÇÃO (limpidus)
use limpidus
var prodAreas = db.areaActivity.find({ projectId: 4698 }).toArray()

// Comparar contagens
print("Teste: " + testAreas.length + " áreas")
print("Produção: " + prodAreas.length + " áreas")

// Comparar uma área específica
var testArea = testAreas.find(a => a.name === "Hall Elevadores / Recepção")
var prodArea = prodAreas.find(a => a.name === "Hall Elevadores / Recepção")

// Comparar headerId
print("Teste headerId: " + testArea.headerId)
print("Produção headerId: " + prodArea.headerId)
```

## 🧹 Limpar Dados Após Teste

Após validar os dados, você pode limpar:

```javascript
// Limpar apenas áreas do projeto testado
use limpidus-test
db.areaActivity.deleteMany({ projectId: 4698 })

// Ou limpar todas as áreas de teste
db.areaActivity.deleteMany({})
```

## 📝 Exemplo de Saída do Teste

Ao executar, você verá logs como:

```
✅ Migração E2E concluída!
📊 Total de áreas migradas: 50
📋 Total de items: 306

💡 Para comparar com produção:
   use limpidus-test
   db.areaActivity.find({ projectId: 4698 }).pretty()

   use limpidus
   db.areaActivity.find({ projectId: 4698 }).pretty()
```

## ⚠️ Cuidados

1. **Não execute em CI/CD**: Estes testes conectam a sistemas reais
2. **Use com moderação**: Podem ser lentos e dependem de conectividade
3. **Limpe dados após uso**: Para não acumular dados de teste
4. **Valide projeto ID**: Certifique-se de que o projeto existe no SQL Server

## 🎯 Próximos Passos

Após executar os testes E2E:

1. ✅ Compare os dados migrados com produção
2. ✅ Valide mapeamentos (headerId, frequência, etc.)
3. ✅ Verifique estrutura dos items
4. ✅ Limpe os dados de teste quando terminar
