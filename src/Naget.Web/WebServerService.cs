namespace NaGet.Web;

/// <summary>
///     The main service class of the <see cref="WebServerService" />.
/// </summary>
public class WebServerService : BackgroundService
{
    /// <summary>
    /// The logger.
    /// </summary>
    private readonly ILogger logger = Log.ForContext<WebServerService>();

    /// <summary>
    /// Initializes a new instance of the <see cref="WebServerService"/> class.
    /// </summary>
    public WebServerService()
    {
    }

    /// <summary>
    /// Gets or sets the delay in milliseconds.
    /// </summary>
    public int DelayInMilliSeconds { get; set; }

    /// <summary>
    /// Start the web server service
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to kill start</param>
    /// <returns>A task representing the start action</returns>
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        Log.Information("Connecting to Orleans Silo");
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            this.logger.Information("Heartbeat.");
            await Task.Delay(this.DelayInMilliSeconds, stoppingToken);
        }
    }
}
