namespace NaGet.Core;

public class PackageDatabase : IPackageDatabase
{
    private readonly IContext context;

    public PackageDatabase(IContext context)
    {
        this.context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<PackageAddResult> AddAsync(Package package, CancellationToken cancellationToken)
    {
        try
        {
            context.Packages.Add(package);

            await context.SaveChangesAsync(cancellationToken);

            return PackageAddResult.Success;
        }
        catch (DbUpdateException e)
            when (context.IsUniqueConstraintViolationException(e))
        {
            return PackageAddResult.PackageAlreadyExists;
        }
    }

    public async Task<bool> ExistsAsync(string id, CancellationToken cancellationToken)
    {
        return await context
            .Packages
            .Where(p => p.Id == id)
            .AnyAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(string id, NuGetVersion version, CancellationToken cancellationToken)
    {
        return await context
            .Packages
            .Where(p => p.Id == id)
            .Where(p => p.NormalizedVersionString == version.ToNormalizedString())
            .AnyAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Package>> FindAsync(string id, bool includeUnlisted, CancellationToken cancellationToken)
    {
        var query = context.Packages
            .Include(p => p.Dependencies)
            .Include(p => p.PackageTypes)
            .Include(p => p.TargetFrameworks)
            .Where(p => p.Id == id);

        if (!includeUnlisted)
        {
            query = query.Where(p => p.Listed);
        }

        return (await query.ToListAsync(cancellationToken)).AsReadOnly();
    }

    public Task<Package> FindOrNullAsync(
        string id,
        NuGetVersion version,
        bool includeUnlisted,
        CancellationToken cancellationToken)
    {
        var query = context.Packages
            .Include(p => p.Dependencies)
            .Include(p => p.TargetFrameworks)
            .Where(p => p.Id == id)
            .Where(p => p.NormalizedVersionString == version.ToNormalizedString());

        if (!includeUnlisted)
        {
            query = query.Where(p => p.Listed);
        }

        return query.FirstOrDefaultAsync(cancellationToken);
    }

    public Task<bool> UnlistPackageAsync(string id, NuGetVersion version, CancellationToken cancellationToken)
    {
        return TryUpdatePackageAsync(id, version, p => p.Listed = false, cancellationToken);
    }

    public Task<bool> RelistPackageAsync(string id, NuGetVersion version, CancellationToken cancellationToken)
    {
        return TryUpdatePackageAsync(id, version, p => p.Listed = true, cancellationToken);
    }

    public async Task AddDownloadAsync(string id, NuGetVersion version, CancellationToken cancellationToken)
    {
        await TryUpdatePackageAsync(id, version, p => p.Downloads += 1, cancellationToken);
    }

    public async Task<bool> HardDeletePackageAsync(string id, NuGetVersion version, CancellationToken cancellationToken)
    {
        var package = await context.Packages
            .Where(p => p.Id == id)
            .Where(p => p.NormalizedVersionString == version.ToNormalizedString())
            .Include(p => p.Dependencies)
            .Include(p => p.TargetFrameworks)
            .FirstOrDefaultAsync(cancellationToken);

        if (package is null)
        {
            return false;
        }

        context.Packages.Remove(package);
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task<bool> TryUpdatePackageAsync(
        string id,
        NuGetVersion version,
        Action<Package> action,
        CancellationToken cancellationToken)
    {
        var package = await context.Packages
            .Where(p => p.Id == id)
            .Where(p => p.NormalizedVersionString == version.ToNormalizedString())
            .FirstOrDefaultAsync();

        if (package is not null)
        {
            action(package);
            await context.SaveChangesAsync(cancellationToken);

            return true;
        }

        return false;
    }
}
