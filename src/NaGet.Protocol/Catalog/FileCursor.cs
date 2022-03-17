namespace NaGet.Protocol.Catalog;

/// <summary>
/// A cursor implementation which stores the cursor in local file. The cursor value is written to the file as
/// a JSON object.
/// Based off: https://github.com/NuGet/NuGet.Services.Metadata/blob/3a468fe534a03dcced897eb5992209fdd3c4b6c9/src/NuGet.Protocol.Catalog/FileCursor.cs
/// </summary>
public class FileCursor : ICursor
{
    private readonly string path;
    private readonly ILogger<FileCursor> logger;

    public FileCursor(string path, ILogger<FileCursor> logger)
    {
        this.path = path ?? throw new ArgumentNullException(nameof(path));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<DateTimeOffset?> GetAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var file = File.OpenRead(path);
            var data = await JsonSerializer.DeserializeAsync<Data?>(file, options: null, cancellationToken);

            if (data is null)
            {
                return null;
            }

            logger.LogDebug("Read cursor value {cursor:O} from {path}.", data.Value, path);
            return data.Value;
        }
        catch (Exception e) when (e is FileNotFoundException || e is JsonException)
        {
            return null;
        }
    }

    public Task SetAsync(DateTimeOffset value, CancellationToken cancellationToken)
    {
        var data = new Data { Value = value };
        var jsonString = JsonSerializer.Serialize(data);
        File.WriteAllText(path, jsonString);
        logger.LogDebug("Wrote cursor value {cursor:O} to {path}.", data.Value, path);
        return Task.CompletedTask;
    }

    private class Data
    {
        [JsonPropertyName("value")]
        public DateTimeOffset Value { get; set; }
    }
}
