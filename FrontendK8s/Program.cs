using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;  // Add this line

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://0.0.0.0:80");

// Configurar Data Protection
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/app/keys"));

// Deshabilitar temporalmente la validación antiforgery para pruebas
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AddPageApplicationModelConvention("/",
        model => model.Filters.Add(new IgnoreAntiforgeryTokenAttribute()));
});

builder.Services.AddHttpClient("API", client =>
{
    var apiUrl = builder.Configuration["ApiSettings:BaseUrl"];
    client.BaseAddress = new Uri(apiUrl ?? "http://api-service:8000");
    client.DefaultRequestHeaders.Accept.Add(
        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();