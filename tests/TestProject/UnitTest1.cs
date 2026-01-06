using AccountingSystem.Api.Controllers;

namespace TestProject;

public sealed class CommonControllerTests
{
    [Fact]
    public void AddChecked_ThrowsOverflowException_WhenOverflow()
    {
        Assert.Throws<OverflowException>(() => CommonController.AddChecked(int.MaxValue, 1));
    }

    [Fact]
    public void AddChecked_ThrowsOverflowException_WhenUnderflow()
    {
        Assert.Throws<OverflowException>(() => CommonController.AddChecked(int.MinValue, -1));
    }

    [Fact]
    public void Add_Demo_FailingTest_OutputReadingPractice()
    {
        // わざと失敗させるデモ: 1 + 2 は 3 なのに 4 を期待している
        var actual = CommonController.Add(1, 2);

        // Assert.Equ(4, actual);
        Assert.Equal(3, actual);
    }

    [Theory]
    [MemberData(nameof(AddTestCases.AddCases), MemberType = typeof(AddTestCases))]
    public void Add_ReturnsSum(int a, int b, int expected)
    {
        var actual = CommonController.Add(a, b);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(3, 5)]
    [InlineData(-10, 7)]
    [InlineData(0, 123)]
    public void Add_IsCommutative_ForRepresentativeValues(int a, int b)
    {
        var ab = CommonController.Add(a, b);
        var ba = CommonController.Add(b, a);

        Assert.Equal(ab, ba);
    }
}