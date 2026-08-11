# API acessível pelo React Native (Android)

## Erro ao compilar: MSB3027 / “file is being used by another process”

Significa que **já existe uma instância** da API rodando e o Windows mantém `LimpidusMongoDB.Api.exe` aberto.

1. Feche o terminal onde a API estava rodando, **ou**
2. No PowerShell, encerre todos os processos com o mesmo nome:

```powershell
Get-Process -Name "LimpidusMongoDB.Api" -ErrorAction SilentlyContinue | Stop-Process -Force
```

Depois rode `dotnet run` de novo.

## Rodar a API em todas as interfaces (HTTP, porta 5234)

Na raiz do repositório:

```bash
dotnet run --project LimpidusMongoDB.API/LimpidusMongoDB.Api.csproj --launch-profile LimpidusMongoDB.API
```

Ou forçando a URL (útil se não usar o perfil do `launchSettings.json`):

```bash
dotnet run --project LimpidusMongoDB.API/LimpidusMongoDB.Api.csproj --urls "http://0.0.0.0:5234"
```

A API deve aparecer como escutando em `http://0.0.0.0:5234` (aceita conexões da LAN e do emulador).

## Firewall do Windows

1. Abra **Firewall do Windows com Segurança Avançada**.
2. **Regras de Entrada** → **Nova Regra** → **Porta** → TCP → **Portas específicas**: `5234`.
3. **Permitir a conexão** → marque pelo menos **Domínio** e **Privado** (recomendado: não expor em **Público** em redes abertas).
4. Nome: por exemplo `Limpidus API dev 5234`.

Sem essa regra, o **celular físico** na mesma Wi‑Fi costuma falhar com erro de rede no app.

## Testar no emulador Android

No navegador do emulador (ou via `adb`), o host da máquina de desenvolvimento é:

- `http://10.0.2.2:5234/swagger`

## Testar no celular físico

1. No PC, descubra o IP na rede Wi‑Fi (ex.: `ipconfig` → adaptador Wi‑Fi → **IPv4**).
2. No celular (mesma rede): `http://<IPv4_DO_PC>:5234/swagger`

## Cliente React Native

Copie os arquivos de exemplo de `examples/react-native/` para o seu app e ajuste `LOCAL_MACHINE_IP` e `USE_ANDROID_PHYSICAL_DEVICE` conforme o ambiente.

## CORS

Em desenvolvimento a API usa política permissiva (`AllowAnyOrigin`, `AllowAnyMethod`, `AllowAnyHeader`). Restrinja antes de publicar em produção.

## Android: HTTP em desenvolvimento

Chamadas `http://` exigem tráfego em texto claro liberado no app. No **debug**, em `AndroidManifest.xml` use `android:usesCleartextTraffic="true"` no `<application>` **ou** configure `networkSecurityConfig` apontando apenas para o IP/host de desenvolvimento (preferível em builds de release).
