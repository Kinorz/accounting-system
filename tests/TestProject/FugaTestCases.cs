namespace TestProject;

public static class FugaTestCases
{
    public static IEnumerable<object[]> SpecialStatusCodeCases =>
    [
        [7, 403, "Access to Id 7 is forbidden."],
        [13, 500, "Unlucky Id caused server error."],
        [42, 418, "I'm a teapot."],
        [99, 429, "Too many requests for Id 99."],
        [100, 301, "Id 100 has been moved permanently."],
        [101, 302, "Id 101 has been found at a different location."],
        [102, 307, "Id 102 has been temporarily redirected."],
        [103, 308, "Id 103 has been permanently redirected."],
    ];
}
