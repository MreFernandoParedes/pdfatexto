using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PDFATEXTO.Web.Configuration;

namespace PDFATEXTO.Web.Services;

public sealed class ClaudeService : IClaudeService
{
    private const string Prompt = """
        Procesa el PDF completo y devuelve una unica salida consolidada en texto plano.

        El texto extraido debe ser fiel al contenido original del documento. No parafrasees, no resumas, no expliques y no reescribas el texto normal. Conserva el contenido util con la mayor fidelidad posible y elimina unicamente elementos sin valor informativo.

        Reglas obligatorias:
        - Conserva solo contenido con valor informativo.
        - Manten el texto original con la mayor fidelidad posible.
        - No parafrasees ni reformules el texto normal del documento.
        - Elimina encabezados, pies de pagina, numeros de pagina, sellos, marcas de agua, texto repetido, texto superpuesto irrelevante y ruido visual.
        - Convierte listas numeradas, numeracion de secciones y vinetas en lineas que comiencen con "-".
        - Si detectas tablas, convierte cada fila en una linea con valores separados por comas.
        - Si detectas un organigrama, describelo en texto natural, indicando las unidades y sus dependencias de forma clara y fiel a lo visible.
        - Si detectas un diagrama de flujo, describelo en lenguaje natural, siguiendo el orden del proceso y sus decisiones, sin usar flechas ni simbolos.
        - Si una parte no puede interpretarse con certeza, no la inventes ni la mezcles dentro del cuerpo principal.
        - Toda duda, ambiguedad o posible lectura incierta debe colocarse al final, en una seccion llamada [REVISAR].
        - Respeta el idioma original del documento.

        Formato obligatorio de salida:
        - Devuelve una sola salida consolidada.
        - Usa solo texto plano.
        - Usa lineas con "-" para listas y secciones normalizadas.
        - Usa lineas separadas por comas para tablas.
        - Usa parrafos breves y claros para organigramas y diagramas de flujo.
        - Al final del documento, agrega una seccion llamada [REVISAR] solo si existen dudas.

        Prohibido:
        - Explicar lo que hiciste.
        - Agregar introducciones, conclusiones o comentarios meta.
        - Completar partes ambiguas con suposiciones.
        """;

    private readonly HttpClient _httpClient;
    private readonly ClaudeOptions _options;

    public ClaudeService(HttpClient httpClient, IOptions<ClaudeOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<string> ExtractTextAsync(byte[] pdfBytes, string fileName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            return """
                No se configuro la clave API de Claude.

                [REVISAR]
                Pagina 0: configure la clave en appsettings.json o en secretos de usuario antes de usar el procesamiento real.
                """;
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, _options.BaseUrl);
        request.Headers.Add("x-api-key", _options.ApiKey);
        request.Headers.Add("anthropic-version", "2023-06-01");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var payload = new
        {
            model = _options.Model,
            max_tokens = _options.MaxTokens,
            messages = new object[]
            {
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new
                        {
                            type = "document",
                            source = new
                            {
                                type = "base64",
                                media_type = "application/pdf",
                                data = Convert.ToBase64String(pdfBytes)
                            }
                        },
                        new
                        {
                            type = "text",
                            text = $"Nombre de archivo: {fileName}\n\n{Prompt}"
                        }
                    }
                }
            }
        };

        request.Content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(responseStream, cancellationToken: cancellationToken);

        if (!document.RootElement.TryGetProperty("content", out var contentElement) ||
            contentElement.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException("La respuesta de Claude no tiene el formato esperado.");
        }

        var textParts = contentElement
            .EnumerateArray()
            .Where(item => item.TryGetProperty("type", out var type) && type.GetString() == "text")
            .Select(item => item.GetProperty("text").GetString())
            .Where(text => !string.IsNullOrWhiteSpace(text));

        return string.Join(Environment.NewLine, textParts!);
    }
}
