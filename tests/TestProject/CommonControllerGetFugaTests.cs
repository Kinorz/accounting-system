using AccountingSystem.Api.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace TestProject;

public sealed class CommonControllerGetFugaTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GetFuga_ReturnsBadRequest_WhenIdIsNonPositive(int id)
    {
        // Arrange
        var controller = new CommonController();
        var request = new CommonController.FugaRequest(id);

        // Act
        var result = controller.GetFuga(request);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Id must be positive.", badRequest.Value);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(4)]
    public void GetFuga_ReturnsNotFound_WhenIdIsEvenPositive(int id)
    {
        // Arrange
        var controller = new CommonController();
        var request = new CommonController.FugaRequest(id);

        // Act
        var result = controller.GetFuga(request);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Theory]
    [MemberData(nameof(FugaTestCases.SpecialStatusCodeCases), MemberType = typeof(FugaTestCases))]
    public void GetFuga_ReturnsExpectedStatusCode_ForSpecialIds(int id, int expectedStatusCode, string expectedMessage)
    {
        // Arrange
        var controller = new CommonController();
        var request = new CommonController.FugaRequest(id);

        // Act
        var result = controller.GetFuga(request);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(expectedStatusCode, objectResult.StatusCode);
        Assert.Equal(expectedMessage, objectResult.Value);
    }

    [Fact]
    public void GetFuga_ReturnsNoContent_WhenIdIs150()
    {
        // Arrange
        var controller = new CommonController();
        var request = new CommonController.FugaRequest(150);

        // Act
        var result = controller.GetFuga(request);

        // Assert
        var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(204, statusCodeResult.StatusCode);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    public void GetFuga_ReturnsOk_WithExpectedPayload_WhenIdIsOddPositiveAndNotSpecial(int id)
    {
        // Arrange
        var controller = new CommonController();
        var request = new CommonController.FugaRequest(id);

        // Act
        var result = controller.GetFuga(request);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var payload = Assert.IsType<CommonController.FugaResponse>(ok.Value);
        Assert.Equal("Hana", payload.Name);
        Assert.Equal(30, payload.Age);
    }
}
