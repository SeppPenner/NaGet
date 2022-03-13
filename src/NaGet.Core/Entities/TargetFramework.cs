namespace NaGet.Core;

public class TargetFramework
{
    public int Key { get; set; }

    public string Moniker { get; set; } = string.Empty;

    public Package Package { get; set; } = new();
}
