namespace NaGet.Core;

// See https://github.com/NuGet/NuGet.Services.Metadata/blob/master/src/NuGet.Indexing/Downloads.cs
public class PackageDownloadsJsonSource : IPackageDownloadsSource
{
    public const string PackageDownloadsV1Url = "https://nugetprod0.blob.core.windows.net/ng-search-data/downloads.v1.json";

    private readonly HttpClient httpClient;
    private readonly ILogger<PackageDownloadsJsonSource> logger;

    public PackageDownloadsJsonSource(HttpClient httpClient, ILogger<PackageDownloadsJsonSource> logger)
    {
        this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Dictionary<string, Dictionary<string, long>>> GetPackageDownloadsAsync()
    {
        logger.LogInformation("Fetching package downloads...");

        var results = new Dictionary<string, Dictionary<string, long>>();

        using (var downloadsStream = await GetDownloadsStreamAsync())
        using (var downloadStreamReader = new StreamReader(downloadsStream))
        using (var jsonReader = new JsonTextReader(downloadStreamReader))
        {
            logger.LogInformation("Parsing package downloads...");

            jsonReader.Read();

            while (jsonReader.Read())
            {
                try
                {
                    if (jsonReader.TokenType == JsonToken.StartArray)
                    {
                        // TODO: This line reads the entire document into memory...
                        var record = JToken.ReadFrom(jsonReader);
                        var id = string.Intern(record[0].ToString().ToLowerInvariant());

                        // The second entry in each record should be an array of versions, if not move on to next entry.
                        // This is a check to safe guard against invalid entries.
                        if (record.Count() == 2 && record[1].Type != JTokenType.Array)
                        {
                            continue;
                        }

                        if (!results.ContainsKey(id))
                        {
                            results.Add(id, new Dictionary<string, long>());
                        }

                        foreach (var token in record)
                        {
                            if (token is not null && token.Count() == 2)
                            {
                                var version = string.Intern(NuGetVersion.Parse(token[0].ToString()).ToNormalizedString().ToLowerInvariant());
                                var downloads = token[1].ToObject<int>();

                                results[id][version] = downloads;
                            }
                        }
                    }
                }
                catch (JsonReaderException e)
                {
                    logger.LogError(e, "Invalid entry in downloads.v1.json");
                }
            }

            logger.LogInformation("Parsed package downloads");
        }

        return results;
    }

    private async Task<Stream> GetDownloadsStreamAsync()
    {
        logger.LogInformation("Downloading downloads.v1.json...");

        var fileStream = File.Open(Path.GetTempFileName(), FileMode.Create);
        var response = await httpClient.GetAsync(PackageDownloadsV1Url, HttpCompletionOption.ResponseHeadersRead);

        response.EnsureSuccessStatusCode();

        using (var networkStream = await response.Content.ReadAsStreamAsync())
        {
            await networkStream.CopyToAsync(fileStream);
        }

        fileStream.Seek(0, SeekOrigin.Begin);

        logger.LogInformation("Downloaded downloads.v1.json");

        return fileStream;
    }
}
