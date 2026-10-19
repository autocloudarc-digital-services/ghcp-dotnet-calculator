#nullable enable

using calculator.web.Components;
using calculator.web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddScoped<CalculatorService>();
builder.Services.AddScoped<HistoryService>();
builder.Services.AddScoped<ThemeService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.MapStaticAssets();
app.UseAntiforgery();
app.MapGet("/health", () => Results.Text("Healthy"));

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
