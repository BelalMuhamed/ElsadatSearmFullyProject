using Application.DTOs.Notifications;
using Domain.Enums;
using System.Text.Encodings.Web;
using System.Text.Json;
using Xunit;

namespace Application.UnitTests.Common;

/// <summary>Unit tests for <see cref="NotificationTypes"/> and the stored argument format — pure, no DB.</summary>
public class NotificationTypesTests
{
    [Fact]
    public void Every_type_has_a_tab()
    {
        foreach (var type in Enum.GetValues<NotificationType>())
            Assert.True(Enum.IsDefined(NotificationTypes.CategoryOf(type)));
    }

    [Theory]
    [InlineData(NotificationType.ApprovalRequested)]
    [InlineData(NotificationType.ApprovalDecided)]
    [InlineData(NotificationType.PermissionsChanged)]
    public void Approvals_and_permission_changes_can_not_be_muted(NotificationType type) // D44
        => Assert.False(NotificationTypes.CanMute(type));

    [Theory]
    [InlineData(NotificationType.StockLow)]
    [InlineData(NotificationType.InvoiceConfirmed)]
    [InlineData(NotificationType.ExportReady)]
    [InlineData(NotificationType.Message)]
    [InlineData(NotificationType.Offer)]
    public void Other_types_can_be_muted(NotificationType type)
        => Assert.True(NotificationTypes.CanMute(type));

    [Fact]
    public void Approval_notifications_are_under_the_approvals_tab()
    {
        Assert.Equal(NotificationCategory.Approvals, NotificationTypes.CategoryOf(NotificationType.ApprovalRequested));
        Assert.Equal(NotificationCategory.Approvals, NotificationTypes.CategoryOf(NotificationType.ApprovalDecided));
    }

    [Fact]
    public void Arguments_round_trip_as_short_json()
    {
        NotificationArg[] args = [NotificationArg.Of("محمد سعيد"), NotificationArg.Localized("ApprovalType.Leave")];

        var json = JsonSerializer.Serialize(args, new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
        var back = JsonSerializer.Deserialize<List<NotificationArg>>(json)!;

        Assert.Equal("""[{"t":"محمد سعيد"},{"k":"ApprovalType.Leave"}]""", json);
        Assert.Equal(args, back);
    }
}
