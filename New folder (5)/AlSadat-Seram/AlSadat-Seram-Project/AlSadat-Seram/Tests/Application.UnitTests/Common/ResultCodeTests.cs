using Domain.Common;
using System.Net;
using System.Text.Json;
using Xunit;

namespace Application.UnitTests.Common;

public class ResultCodeTests
{
    [Fact]
    public void Success_has_no_code()
    {
        Assert.Null(Result<int>.SuccessKey("Permission.AssignedSuccessfully", 1).Code);
    }

    [Fact]
    public void Failure_with_a_key_uses_the_key_as_code()
    {
        Assert.Equal("Auth.InvalidCredentials", Result<int>.FailureKey("Auth.InvalidCredentials").Code);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, ErrorCodes.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized, ErrorCodes.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden, ErrorCodes.PermissionDenied)]
    [InlineData(HttpStatusCode.NotFound, ErrorCodes.NotFound)]
    [InlineData(HttpStatusCode.MethodNotAllowed, ErrorCodes.MethodNotAllowed)]
    [InlineData(HttpStatusCode.Conflict, ErrorCodes.DataConflict)]
    [InlineData(HttpStatusCode.UnsupportedMediaType, ErrorCodes.UnsupportedMediaType)]
    [InlineData(HttpStatusCode.TooManyRequests, ErrorCodes.TooManyRequests)]
    [InlineData(HttpStatusCode.InternalServerError, ErrorCodes.ServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable, ErrorCodes.ServerError)]
    public void Failure_with_a_literal_message_uses_the_general_code_of_its_status(HttpStatusCode status, string expected)
    {
        Assert.Equal(expected, Result<int>.Failure("literal", status).Code);
    }

    [Fact]
    public void MessageKey_is_not_serialized()
    {
        var json = JsonSerializer.Serialize(Result<int>.FailureKey("Auth.InvalidCredentials"));

        Assert.DoesNotContain("MessageKey", json);
        Assert.Contains("\"Code\":\"Auth.InvalidCredentials\"", json);
    }
}
