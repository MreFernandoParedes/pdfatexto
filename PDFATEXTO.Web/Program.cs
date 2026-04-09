using PDFATEXTO.Web.Configuration;
using PDFATEXTO.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Services.Configure<ClaudeOptions>(
    builder.Configuration.GetSection(ClaudeOptions.SectionName));

builder.Services.AddControllersWithViews();
builder.Services.AddSession();
builder.Services.AddHttpClient<IClaudeService, ClaudeService>();
builder.Services.AddScoped<IPdfExtractionService, PdfExtractionService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
