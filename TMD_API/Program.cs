using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

class Program
{
    private const string TMDbApiKey = "2485efbdb1367ebd2417ba38d297df22";

    static async Task Main()
    {
            Console.WriteLine("Bienvenido a la aplicación de búsqueda de películas en TMDb!");

            while (true)
            {
                Console.Write("Escribe el nombre de la película (o escriba 'salir' para cerrar el programa): ");
                string userInput = Console.ReadLine();

                if (userInput.ToLower() == "salir")
                {
                    break;
                }

                if (string.IsNullOrWhiteSpace(userInput))
                {
                    Console.WriteLine("Por favor, ingrese un nombre de película válido.");
                    Console.ForegroundColor = ConsoleColor.Red;
                    continue;
                }

                await BuscarPelicula(userInput);
            }

            Console.WriteLine("Gracias por usar la aplicación");
    }

    // Busca y muestra información sobre una película
    static async Task BuscarPelicula(string titulo)
    {

        using var httpClient = new HttpClient();
        var url = $"https://api.themoviedb.org/3/search/movie?api_key={TMDbApiKey}&query={titulo}";

        try
        {
            // Solicitud a la API de TMDb para obtener detalles de la película
            var respuesta = await httpClient.GetStringAsync(url);
            var resultado = Newtonsoft.Json.JsonConvert.DeserializeObject<TMDbSearchResult<MovieDetails>>(respuesta);

            if (resultado.Results.Count > 0)
            {
                // Información detallada de la película
                var detallesPelicula = resultado.Results[0];
                Console.WriteLine("\nInformación de la película:");
                Console.WriteLine($"Título: {detallesPelicula.title}");
                Console.WriteLine($"Título original: {detallesPelicula.original_title}");
                Console.WriteLine($"Puntuación media: {detallesPelicula.vote_average}");
                Console.WriteLine($"Fecha de estreno: {detallesPelicula.release_date}");
                Console.WriteLine($"Sinopsis: {detallesPelicula.overview}");

                // Obtener películas similares
                var peliculasSimilares = await ObtenerPeliculasSimilares(detallesPelicula.Id);
                
                if (peliculasSimilares.Count > 0)
                {
                    Console.WriteLine("Películas similares:");
                    foreach (var peliculaSimilar in peliculasSimilares.Take(5))
                    {
                        Console.WriteLine($"{peliculaSimilar.title} ({ObtenerAñoEstreno(peliculaSimilar.release_date)})");
                    }
                }
                else
                {
                    Console.WriteLine("No se encontraron películas similares.");
                }
            }
            else
            {
                Console.WriteLine("No se encontraron resultados para la película especificada.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al buscar la película: {ex.Message}");
        }
    }

    // Obtener una lista de películas similares
    private static async Task<List<MovieDetails>> ObtenerPeliculasSimilares(int peliculaId)
    {
        using var httpClient = new HttpClient();
        var url = $"https://api.themoviedb.org/3/movie/{peliculaId}/similar?api_key={TMDbApiKey}";
        var respuesta = await httpClient.GetStringAsync(url);

        var resultado = JsonConvert.DeserializeObject<TMDbSearchResult<MovieDetails>>(respuesta);
        return resultado.Results;
    }

    // Obtener una el año de la pelicula
    private static int ObtenerAñoEstreno(string fechaEstreno)
    {
        if (DateTime.TryParse(fechaEstreno, out var fecha))
        {
            return fecha.Year;
        }
        return 0;
    }
}

public class TMDbSearchResult<T>
{
    public List<T> Results { get; set; }
}

public class MovieDetails
{
    public int Id { get; set; }

    public string title { get; set; }

    public string original_title { get; set; }

    public double vote_average { get; set; }

    public string release_date { get; set; }

    public string overview { get; set; }
}
