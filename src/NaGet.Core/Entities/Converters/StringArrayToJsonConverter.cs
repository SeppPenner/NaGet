namespace NaGet.Core;

public class StringArrayToJsonConverter : ValueConverter<string[], string>
{
    public static readonly StringArrayToJsonConverter Instance = new StringArrayToJsonConverter();

    public StringArrayToJsonConverter()
        : base(
            v => JsonConvert.SerializeObject(v),
            v => (!string.IsNullOrWhiteSpace(v)) ? JsonConvert.DeserializeObject<string[]>(v) : new string[0])
    {
    }
}
