using AccountingSystem.Api.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace TestProject;

public sealed class CommonControllerGetHogeTests
{
    [Fact]
    public void GetHoge_ReturnsOk_WithExpectedPayload()
    {
        // Arrange
        var controller = new CommonController();

        // Act
        var result = controller.GetHoge();

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var payload = Assert.IsType<CommonController.HogeResponse>(ok.Value);
        Assert.Equal("Taro", payload.Name);
        Assert.Equal(20, payload.Age);
    }
}
