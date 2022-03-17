namespace NaGet.Core;

public class UriToStringConverter : ValueConverter<Uri?, string>
{
    public static readonly UriToStringConverter Instance = new();

    public UriToStringConverter()
        : base(
            v => v.AbsoluteUri,
            v => new Uri(v))
    {
    }
}
