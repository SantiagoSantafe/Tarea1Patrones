using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;

public class IndexModel : PageModel
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<IndexModel> _logger;

    [BindProperty]
    public string? Nombre {get; set;}
    public List<string> Nombres {get; set;} = new List<string>();

    public IndexModel(IHttpClientFactory clientFactory, IConfiguration configuration, ILogger<IndexModel> logger)
    {
        httpClientFactory = clientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task OnGetAsync()
    {
        await CargarNombres();
    }

    public async Task<IActionResult> OnPostAsync()
{
    if(string.IsNullOrWhiteSpace(Nombre))
    {
        return Page(); // Si hay un error de validación, puedes seguir mostrando la página actual con errores
    }

    var client = httpClientFactory.CreateClient("API");
    var apiUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://api-service:8000";

    try
    {
        var content = new StringContent(
            JsonSerializer.Serialize(new { nombre = Nombre }),
            Encoding.UTF8,
            "application/json"
        );

        _logger.LogInformation($"Enviando POST con nombre: {Nombre}");
        var response = await client.PostAsync(apiUrl, content);
        response.EnsureSuccessStatusCode();

        // **¡Cambio importante aquí! Redirige de vuelta a la página actual (GET)**
        return RedirectToPage();

        // No necesitas llamar a CargarNombres() aquí, 
        // RedirectToPage() hará que OnGetAsync se ejecute de nuevo y cargue los nombres.
        // await CargarNombres();
        // return Page(); // Ya no es necesario Page() aquí
    }
    catch (Exception ex)
    {
        _logger.LogError($"Error en POST: {ex.Message}");
        ModelState.AddModelError(string.Empty, $"Error guardando nombre: {ex.Message}");
        return Page(); // Si hay un error al guardar, puedes volver a mostrar la página con el mensaje de error
    }
}

    private async Task CargarNombres()
    {
        var client = httpClientFactory.CreateClient("API");
        var apiUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://api-service:8000"; // Obtén la URL base


        try
        {
            // **Usar URL absoluta aquí:**
            var response = await client.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var resultado = JsonSerializer.Deserialize<ResultadoNombres>(content);
            Nombres = resultado?.nombres ?? new List<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error cargando nombres: {ex.Message}");
            ModelState.AddModelError(string.Empty, $"Error cargando nombres: {ex.Message}");
        }
    }
        public class ResultadoNombres
    {
        public List<string> nombres { get; set; } = new List<string>();
    }
}