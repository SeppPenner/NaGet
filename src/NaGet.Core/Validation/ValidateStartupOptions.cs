namespace NaGet.Core;

/// <summary>
/// Validates NaGet's options, used at startup.
/// </summary>
public class ValidateStartupOptions
{
    private readonly IOptions<NaGetOptions> root;
    private readonly IOptions<DatabaseOptions> database;
    private readonly IOptions<StorageOptions> storage;
    private readonly IOptions<MirrorOptions> mirror;
    private readonly ILogger<ValidateStartupOptions> logger;

    public ValidateStartupOptions(
        IOptions<NaGetOptions> root,
        IOptions<DatabaseOptions> database,
        IOptions<StorageOptions> storage,
        IOptions<MirrorOptions> mirror,
        ILogger<ValidateStartupOptions> logger)
    {
        this.root = root ?? throw new ArgumentNullException(nameof(root));
        this.database = database ?? throw new ArgumentNullException(nameof(database));
        this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
        this.mirror = mirror ?? throw new ArgumentNullException(nameof(mirror));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public bool Validate()
    {
        try
        {
            // Access each option to force validations to run.
            // Invalid options will trigger an "OptionsValidationException" exception.
            _ = root.Value;
            _ = database.Value;
            _ = storage.Value;
            _ = mirror.Value;

            return true;
        }
        catch (OptionsValidationException e)
        {
            foreach (var failure in e.Failures)
            {
                logger.LogError("{OptionsFailure}", failure);
            }

            logger.LogError(e, "NaGet configuration is invalid.");
            return false;
        }
    }
}
