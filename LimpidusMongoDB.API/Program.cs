// Desenvolvimento mobile (Android emulador / celular na mesma rede):
// - Libere a porta TCP 5234 no Firewall do Windows (regra de entrada, perfil Privado)
//   para o host aceitar conexões de 10.0.2.2 (emulador) ou do IP da sua LAN.
// - Rode a API com: dotnet run --urls "http://0.0.0.0:5234"
//   (ou dotnet run com o perfil LimpidusMongoDB.API: applicationUrl em launchSettings).
// Veja também: LimpidusMongoDB.API/DEV_MOBILE_ANDROID.md
//
// Auth: endpoints exigem JWT (exceto /v1/Auth/* e /v1/HealthCheck).
// Mobile/campo: POST /v1/Auth/project com LOGIN/SENHA do WORK_HEADER → role ProjectViewer.

using LimpidusMongoDB.Api.Configurations;
using LimpidusMongoDB.Application.Helpers;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

DotEnvLoader.Load();

builder.Services.AddCors(options =>
{
    // Apenas para desenvolvimento: app React Native / web local em qualquer origem.
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();
builder.Services.AddJwtAuth(builder.Configuration);
builder.Services.AddServices();
builder.Services.AddSwagger();
builder.Services.AddMvc().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.DefaultIgnoreCondition
                       = JsonIgnoreCondition.WhenWritingNull;
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
// CORS antes de auth e endpoints (inclui preflight OPTIONS).
app.UseCors();

// Sem UseHttpsRedirection: clientes mobile usam HTTP na LAN (ex.: http://192.168.x.x:5234).
// Evita "redirect" inesperado quando a API só expõe HTTP em desenvolvimento.
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
