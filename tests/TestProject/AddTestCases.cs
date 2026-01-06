namespace TestProject;

public static class AddTestCases
{
    public static IEnumerable<object[]> AddCases =>
    [
        [1, 2, 3],
        [0, 0, 0],
        [-1, 1, 0],
        [-10, -5, -15],
        [int.MaxValue, 0, int.MaxValue],
        [int.MinValue, 0, int.MinValue],
    ];
}
