using JevApplicationTeste.Api.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddOptions<JevOptions>()
    .BindConfiguration("Jev")
    .Validate(o => double.IsFinite(o.MinConfidence) && o.MinConfidence is >= 0 and <= 1,
        "MinConfidence deve estar entre 0 e 1.")
    .Validate(o => o.UseMock || !string.IsNullOrWhiteSpace(o.ApiKey),
        "Configure Jev:ApiKey com user-secrets para usar a API real.")
    .ValidateOnStart();
builder.Services.AddHttpClient<JevClient>(client =>
{
    client.BaseAddress = new Uri("https://api.typesafe.ai/");
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddScoped<ClassificacaoService>();
var app = builder.Build();
app.UseExceptionHandler();
app.MapGet("/", () => new { projeto = "JevApplicationTeste", endpoint = "POST /chamados/classificar" });
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapControllers();
app.Run();
