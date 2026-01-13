# Troubleshooting - Testes E2E

## ❌ Erro: "An item with the same key has already been added"

**Causa**: O MongoDB está tentando registrar a mesma classe múltiplas vezes.

**Solução**: ✅ **CORRIGIDO** - Adicionada verificação `BsonClassMap.IsClassMapRegistered()` antes de registrar.

---

## ❌ Erro: "Unable to cast object of type 'System.Double' to type 'System.Int32'"

**Causa**: Alguns campos numéricos no SQL Server retornam como `Double` (float/numeric) em vez de `Int32`.

**Solução**: ✅ **CORRIGIDO** - O método `GetInt32Value()` agora trata conversões de `Double` para `Int32` automaticamente.

---

## ❌ Erro: "Unable to cast object of type 'System.Int32' to type 'System.String'"

**Causa**: Alguns campos que esperamos como string estão retornando como `Int32` do SQL Server.

**Solução**: ✅ **CORRIGIDO** - O método `GetStringValue()` agora trata conversões de qualquer tipo para `String` automaticamente.

---

## ❌ Erro: "A connection was successfully established with the server, but then an error occurred during the pre-login handshake"

**Causa**: Problema de conexão com o SQL Server.

### Possíveis causas:

1. **Problema de SSL/TLS**
   - O SQL Server pode estar exigindo conexão criptografada
   - Adicione `Encrypt=True;TrustServerCertificate=True` na connection string

2. **Firewall bloqueando**
   - Verifique se o firewall permite conexão na porta do SQL Server (geralmente 1433)

3. **Connection string incorreta**
   - Verifique usuário, senha e servidor
   - Teste a connection string manualmente

4. **Servidor indisponível**
   - Verifique se o SQL Server está rodando e acessível

### Soluções:

#### Opção 1: Adicionar parâmetros de SSL na connection string

```csharp
// Adicione estes parâmetros:
"Data Source=sql2.limpidus.com.br;Initial Catalog=limpcalc;Persist Security Info=True;User ID=limpcalc;Password=Limp741852963;Encrypt=True;TrustServerCertificate=True"
```

#### Opção 2: Usar variável de ambiente

```bash
export TEST_SQLSERVER_CONNECTION_STRING="Data Source=sql2.limpidus.com.br;Initial Catalog=limpcalc;User ID=limpcalc;Password=Limp741852963;Encrypt=True;TrustServerCertificate=True"
```

#### Opção 3: Testar conexão manualmente

Use um cliente SQL (SQL Server Management Studio, Azure Data Studio, etc.) para testar a conexão com a mesma connection string.

---

## ❌ Erro: "Unable to connect to the database" (MongoDB)

**Causa**: Problema de conexão com o MongoDB.

### Soluções:

1. **Verifique a connection string do MongoDB**
   ```bash
   export TEST_MONGODB_CONNECTION_STRING="mongodb+srv://..."
   ```

2. **Teste a conexão manualmente**
   ```bash
   mongosh "mongodb+srv://producao:7diEnLIjhtCa5Xxr@cluster0.nmool17.mongodb.net/..."
   ```

3. **Verifique se o cluster está acessível**
   - Pode estar bloqueado por firewall
   - Pode estar com problemas de rede

---

## ✅ Verificar se o problema foi resolvido

Após aplicar as correções, execute novamente:

```bash
dotnet test --filter "FullyQualifiedName~MigrateFromSqlServerAsync_QuandoMigraProjetoReal_DeveSalvarDadosCorretamente" --verbosity normal
```

---

## 📝 Logs Úteis

Os testes E2E agora mostram logs detalhados:

- ✅ Connection string sendo usada
- ❌ Erros detalhados com possíveis causas
- 💡 Sugestões de como resolver

---

## 🔧 Connection String Recomendada

Para SQL Server com SSL:

```
Data Source=sql2.limpidus.com.br;
Initial Catalog=limpcalc;
Persist Security Info=True;
User ID=limpcalc;
Password=Limp741852963;
Encrypt=True;
TrustServerCertificate=True;
Connection Timeout=30;
```
