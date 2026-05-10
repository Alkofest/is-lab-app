using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

// Регистрируем проверку БД (имитация пинга)
builder.Services.AddHealthChecks()
    .AddCheck("Database", () => HealthCheckResult.Healthy("БД доступна"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Swagger UI
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "IsLabApp v1.0");
    options.RoutePrefix = "swagger";
});

// ========== /health ==========
app.MapGet("/health", async (HealthCheckService healthCheckService) =>
{
    var report = await healthCheckService.CheckHealthAsync();
    return Results.Ok(new
    {
        status = report.Status.ToString(),
        checks = report.Entries.Select(e => new
        {
            name = e.Key,
            status = e.Value.Status.ToString(),
            description = e.Value.Description
        })
    });
}).WithName("HealthCheck");

// ========== /version ==========
app.MapGet("/version", () => new
{
    Version = "1.0.0",
    Environment = app.Environment.EnvironmentName
}).WithName("GetVersion");

// ========== /db/ping ==========
app.MapGet("/db/ping", () => new { status = "OK", message = "БД доступна" })
    .WithName("DbPing");

// ========== In-Memory хранилище заметок ==========
var notes = new List<Note>
{
    new Note { Id = 1, Title = "Первая заметка", Content = "Содержимое первой заметки" },
    new Note { Id = 2, Title = "Вторая заметка", Content = "Содержимое второй заметки" }
};

// GET /api/notes
app.MapGet("/api/notes", () => notes)
    .WithName("GetNotes");

// POST /api/notes
app.MapPost("/api/notes", (Note note) =>
{
    note.Id = notes.Count > 0 ? notes.Max(n => n.Id) + 1 : 1;
    notes.Add(note);
    return Results.Created($"/api/notes/{note.Id}", note);
}).WithName("CreateNote");

// GET /api/notes/{id}
app.MapGet("/api/notes/{id:int}", (int id) =>
{
    var note = notes.FirstOrDefault(n => n.Id == id);
    return note is not null ? Results.Ok(note) : Results.NotFound(new { message = "Заметка не найдена" });
}).WithName("GetNoteById");

// DELETE /api/notes/{id}
app.MapDelete("/api/notes/{id:int}", (int id) =>
{
    var note = notes.FirstOrDefault(n => n.Id == id);
    if (note is null) return Results.NotFound(new { message = "Заметка не найдена" });
    
    notes.Remove(note);
    return Results.Ok(new { message = "Заметка удалена" });
}).WithName("DeleteNote");

app.Run();

// ========== Модель Note ==========
public class Note
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}