using Microsoft.EntityFrameworkCore;
using oedibud.Components;
using oedibud.Data;
using oedibud.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient();

builder.Services.AddDbContextFactory<BudgetDbContext>(options =>
    options.UseSqlite("Data Source=oedibud.db"));

builder.Services.AddSingleton<TvLSalaryService>();
builder.Services.AddSingleton<DataChangeNotifier>();

builder.Services.AddBlazorBootstrap();


builder.Services.AddScoped<ForecastEngine>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BudgetDbContext>();
    db.Database.EnsureCreated();
}

//using (var scope = app.Services.CreateScope())
//{
//    var salaryService =
//        scope.ServiceProvider.GetRequiredService<TvLSalaryService>();

//    await salaryService.UpdateSalaryTable(DateTime.Now.Year);
//}




app.Run();
