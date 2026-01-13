# Guia de Execução dos Testes

## 📋 Tipos de Testes

### 1. **Testes Unitários** (`Services/MigrationServiceTests.cs`)
❌ **NÃO testam persistência real no MongoDB**

- Usam **mocks** para todas as dependências
- Testam apenas a **lógica de negócio**
- São **rápidos** (milissegundos)
- Não dependem de recursos externos

**O que testam:**
- Validação de regras de negócio
- Fluxo de dados
- Tratamento de erros
- Mapeamento de dados

**O que NÃO testam:**
- ❌ Persistência real no MongoDB
- ❌ Conexão com banco de dados
- ❌ Integração entre serviços

---

### 2. **Testes de Integração** (`Integration/Services/MigrationServiceIntegrationTests.cs`)
✅ **SIM testam persistência real no MongoDB**

- Usam **repositórios e serviços reais**
- Conectam ao **MongoDB real** (base `limpidus-test`)
- Testam **persistência completa**
- Limpam dados automaticamente após cada teste

**O que testam:**
- ✅ Persistência real no MongoDB
- ✅ Integração entre serviços e repositórios
- ✅ Estrutura completa dos dados salvos
- ✅ Atualização vs criação de registros

**O que NÃO testam:**
- ❌ Conexão com SQL Server (ainda usa mock)

---

## 🚀 Como Executar

### Executar TODOS os testes (unitários + integração)
```bash
dotnet test
```

### Executar APENAS testes unitários (sem persistência)
```bash
dotnet test --filter "FullyQualifiedName!~Integration"
```

### Executar APENAS testes de integração (com persistência real)
```bash
dotnet test --filter "FullyQualifiedName~Integration"
```

### Executar um teste específico
```bash
# Teste unitário
dotnet test --filter "FullyQualifiedName~MigrationServiceTests.MigrateFromSqlServerAsync_QuandoEncontraAreasESalvaComSucesso"

# Teste de integração
dotnet test --filter "FullyQualifiedName~MigrationServiceIntegrationTests.MigrateFromSqlServerAsync_QuandoPersisteDados"
```

---

## 📊 Comparação

| Característica | Testes Unitários | Testes de Integração |
|----------------|------------------|----------------------|
| **Persistência MongoDB** | ❌ Não | ✅ Sim |
| **Velocidade** | ⚡ Muito rápido | 🐢 Mais lento |
| **Dependências** | Mocks | Serviços reais |
| **Base de dados** | Não usa | `limpidus-test` |
| **Limpeza de dados** | Não precisa | Automática |
| **Quando usar** | Desenvolvimento rápido | Validação completa |

---

## 💡 Resposta à sua pergunta

> "Posso executar os testes unitários da migração e neles também serão testadas as persistências na base de dados?"

**Resposta: NÃO**

Os testes unitários **NÃO testam persistência real**. Eles usam mocks.

Para testar persistência real, você precisa executar os **testes de integração**:

```bash
# Testes de integração (testam persistência real)
dotnet test --filter "FullyQualifiedName~Integration"
```

---

## 🎯 Recomendação

Execute **ambos os tipos de teste**:

1. **Durante desenvolvimento**: Use testes unitários (rápidos)
2. **Antes de commit**: Execute todos os testes (unitários + integração)
3. **Validação completa**: Execute testes de integração para garantir persistência

```bash
# Desenvolvimento rápido
dotnet test --filter "FullyQualifiedName!~Integration"

# Validação completa
dotnet test
```
