using System.Text.RegularExpressions;

namespace PipeServerClient;

public partial class RegexRule
{
    [GeneratedRegex("^((25[0-5]|(2[0-4]|1\\d|[1-9]|)\\d)\\.?\\b){4}$")]
    public static partial Regex IpCheck();
}
