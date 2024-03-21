using Application;
using Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Adicione os servi�os ao cont�iner.
builder.Services.AddControllers();
// Aprenda mais sobre a configura��o do Swagger/OpenAPI em https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure o servi�o de aplica��o
builder.Services.AddScoped<ICDRService, CDRService>();

// Configure o reposit�rio
builder.Services.AddSingleton<ICDRRepository, CDRRepository>(); // Altera��o para Scoped

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var repository = services.GetRequiredService<ICDRRepository>();
}

// Configure o pipeline de solicita��o HTTP.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();