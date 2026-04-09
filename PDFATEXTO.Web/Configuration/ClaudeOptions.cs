namespace PDFATEXTO.Web.Configuration;

public sealed class ClaudeOptions
{
    public const string SectionName = "Claude";

    public string BaseUrl { get; set; } = string.Empty;

    public string ApiKey { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public int MaxTokens { get; set; } = 4000;
}
