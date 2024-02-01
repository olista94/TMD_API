using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

class Program
{
    private const string TMDbApiKey = "2485efbdb1367ebd2417ba38d297df22"; // Reemplaza 'tu_api_key' con la clave de tu cuenta en TMDb

    static async Task Main()
    {
        Console.Write("Ingrese el nombre de la película: ");
        string tituloPelicula = Console.ReadLine();

        if (string.IsNullOrEmpty(tituloPelicula))
        {
            Console.WriteLine("El título de la película no puede estar vacío.");
            return;
        }

        var detallesPelicula = await ObtenerDetallesPelicula(tituloPelicula);

        if (detallesPelicula == null)
        {
            Console.WriteLine("Película no encontrada.");
            return;
        }

        Console.WriteLine("\nInformación de la película:");
        Console.WriteLine($"Título: {detallesPelicula.title}");
        Console.WriteLine($"Título Original: {detallesPelicula.original_title}");
        Console.WriteLine($"Puntuación Media: {detallesPelicula.vote_average}");
        Console.WriteLine($"Fecha de Estreno: {detallesPelicula.release_date}");
        Console.WriteLine($"Sinopsis: {detallesPelicula.overview}");

        var peliculasSimilares = await ObtenerPeliculasSimilares(detallesPelicula.Id);

        if (peliculasSimilares != null && peliculasSimilares.Count > 0)
        {
            Console.WriteLine("\nPelículas similares:");
            foreach (var peliculaSimilar in peliculasSimilares.Take(5))
            {
                Console.WriteLine($"{peliculaSimilar.title} ({ObtenerAñoEstreno(peliculaSimilar.release_date)})");
            }
        }
    }

    private static async Task<MovieDetails> ObtenerDetallesPelicula(string titulo)
    {
        using var httpClient = new HttpClient();
        var url = $"https://api.themoviedb.org/3/search/movie?api_key={TMDbApiKey}&query={titulo}";
        var respuesta = await httpClient.GetStringAsync(url);

        var resultado = JsonConvert.DeserializeObject<TMDbSearchResult<MovieDetails>>(respuesta);
        return resultado.Results?.FirstOrDefault();
    }

    private static async Task<List<MovieDetails>> ObtenerPeliculasSimilares(int peliculaId)
    {
        using var httpClient = new HttpClient();
        var url = $"https://api.themoviedb.org/3/movie/{peliculaId}/similar?api_key={TMDbApiKey}";
        var respuesta = await httpClient.GetStringAsync(url);

        var resultado = JsonConvert.DeserializeObject<TMDbSearchResult<MovieDetails>>(respuesta);
        return resultado.Results;
    }

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
