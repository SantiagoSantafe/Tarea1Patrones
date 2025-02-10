using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;

namespace FrontendK8s.Pages;

public class IndexModel : PageModel
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly IConfiguration _configuration;

    [BindProperty]
    public string Nombre {get; set;}
    public List<string> Nombres {get; set;} = new List<string>();

    public IndexModel(IHttpClientFactory clientFactory, IConfiguration configuration){
        httpClientFactory = clientFactory;
        _configuration = configuration;
    }

    public async Task OnGetAsync(){
        await CargarNombres(); //Espera para cargar los nombres del endpoint
    }

    public async Task<IActionResult> OnPostAsync(){
        if(string.IsNullOrWhiteSpace(Nombre)){
            return Page();
        }
        var client = httpClientFactory.CreateClient();
        var apiUrl = _configuration["ApiSettings:BaseUrl"];

        try{
            var content = new StringContent(
                JsonSerializer.Serialize(new {nombre = Nombre}),
                Encoding.UTF8,
                "application/json"
                );
            
            var response = await client.PostAsync($"{apiUrl}/", content);
            response.EnsureSuccessStatusCode();

            await CargarNombres();
        } catch (Exception ex){
            ModelState.AddModelError(string.Empty, $"Error {ex.Message} guardando nombre");
        }

        return Page();
    }

    private async Task CargarNombres(){
        var client = httpClientFactory.CreateClient();
        var apiUrl = _configuration["ApiSettings:BaseUrl"];

        try
        {
        var response = await client.GetAsync($"{apiUrl}/");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var resultado = JsonSerializer.Deserialize<ResultadoNombres>(content);
        Nombres = resultado?.nombres ?? new List<string>();
        
        }
        catch (Exception ex)
        {
           ModelState.AddModelError(string.Empty, $"Error {ex.Message} al cargar los nombres.");
        }
    }

    public class ResultadoNombres
    {
        public List<string> nombres { get; set; } = new List<string>();
    }
}
