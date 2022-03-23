namespace NaGet.Web;

/// <summary>
/// The NuGet Service Index. This aids NuGet client to discover this server's services.
/// </summary>
public class ServiceIndexController : Controller
{
    private readonly IServiceIndexService serviceIndexService;

    public ServiceIndexController(IServiceIndexService serviceIndexService)
    {
        this.serviceIndexService = serviceIndexService ?? throw new ArgumentNullException(nameof(serviceIndexService));
    }

    // GET v3/index
    [HttpGet]
    public async Task<ServiceIndexResponse> GetAsync(CancellationToken cancellationToken)
    {
        return await serviceIndexService.Get(cancellationToken);
    }
}
