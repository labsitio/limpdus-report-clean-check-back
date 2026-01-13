# Como Limpar Dados de Teste

## 📝 Status Atual

A limpeza automática de dados está **DESABILITADA** para permitir consulta dos dados após a execução dos testes.

## 🔍 Consultar Dados no MongoDB

Os dados dos testes de integração estão salvos na base **`limpidus-test`**.

### Via MongoDB Compass

1. Conecte ao cluster MongoDB
2. Selecione a base de dados **`limpidus-test`**
3. Navegue pelas coleções:
   - `areaActivity` - Áreas migradas
   - `project` - Projetos
   - `employee` - Funcionários
   - etc.

### Via mongosh

```bash
# Conectar ao MongoDB
mongosh "mongodb+srv://producao:7diEnLIjhtCa5Xxr@cluster0.nmool17.mongodb.net/?retryWrites=true&w=majority&appName=Cluster0"

# Selecionar base de teste
use limpidus-test

# Consultar áreas migradas
db.areaActivity.find().pretty()

# Consultar por projectId
db.areaActivity.find({ projectId: 9999 }).pretty()

# Contar documentos
db.areaActivity.countDocuments()
```

## 🧹 Limpar Dados Manualmente

### Opção 1: Limpar coleções específicas

```javascript
// No mongosh, dentro da base limpidus-test
db.areaActivity.deleteMany({})
db.project.deleteMany({})
db.employee.deleteMany({})
// ... outras coleções
```

### Opção 2: Deletar a base inteira

```javascript
// No mongosh
use limpidus-test
db.dropDatabase()
```

### Opção 3: Limpar via código C#

Crie um script ou método para limpar:

```csharp
// Limpar todas as coleções
var collections = new[] { "areaActivity", "project", "employee", ... };
foreach (var collectionName in collections)
{
    var collection = database.GetCollection<object>(collectionName);
    collection.DeleteMany(FilterDefinition<object>.Empty);
}
```

## ✅ Reativar Limpeza Automática

Para reativar a limpeza automática após cada teste:

1. Abra `BaseIntegrationTest.cs`
2. No método `Dispose()`, descomente a linha:
   ```csharp
   // CleanupTestData();  // ← Remova o comentário desta linha
   ```

3. Ficará assim:
   ```csharp
   public void Dispose()
   {
       if (!_disposed)
       {
           CleanupTestData();  // ← Limpeza reativada
           _disposed = true;
       }
   }
   ```

## 📊 Comparar com Dados de Produção

### Consultar dados de produção

```javascript
// No mongosh
use limpidus

// Consultar áreas do projeto 4698 (produção)
db.areaActivity.find({ projectId: 4698 }).pretty()

// Consultar áreas do projeto 9999 (teste)
use limpidus-test
db.areaActivity.find({ projectId: 9999 }).pretty()
```

### Comparar estruturas

```javascript
// Produção
use limpidus
var prodArea = db.areaActivity.findOne({ projectId: 4698 })

// Teste
use limpidus-test
var testArea = db.areaActivity.findOne({ projectId: 9999 })

// Comparar campos
// prodArea.name vs testArea.name
// prodArea.headerId vs testArea.headerId
// prodArea.items.length vs testArea.items.length
```

## ⚠️ Importante

- A base `limpidus-test` **não é limpa automaticamente** no momento
- Os dados permanecerão no banco até limpeza manual
- Use IDs de projeto diferentes nos testes (9999, 9998, etc.) para não conflitar
- Sempre limpe os dados após suas análises para não acumular dados de teste
