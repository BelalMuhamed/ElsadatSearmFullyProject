using Application.Common;
using Domain.Entities.Approvals;
using Domain.Enums;
using Xunit;

namespace Application.UnitTests.Common;

/// <summary>Unit tests for <see cref="ApprovalRules"/> and <see cref="ApprovalTypes"/> — pure, no DB.</summary>
public class ApprovalRulesTests
{
    private const string Requester = "user-1";
    private const string Manager = "user-2";

    [Fact]
    public void Another_user_can_decide_a_request()
        => Assert.True(ApprovalRules.CanDecide(Requester, Manager, isSuperAdmin: false));

    [Fact]
    public void Nobody_decides_their_own_request() // D40
        => Assert.False(ApprovalRules.CanDecide(Requester, Requester, isSuperAdmin: false));

    [Fact]
    public void Super_admin_can_decide_their_own_request() // D40
        => Assert.True(ApprovalRules.CanDecide(Requester, Requester, isSuperAdmin: true));

    [Fact]
    public void A_request_the_system_opened_can_be_decided()
        => Assert.True(ApprovalRules.CanDecide(null, Manager, isSuperAdmin: false));

    [Fact]
    public void A_request_is_overdue_only_after_24_hours()
    {
        var requestedAt = new DateTime(2026, 11, 27, 8, 0, 0, DateTimeKind.Utc);

        Assert.False(ApprovalRules.IsOverdue(requestedAt, requestedAt.AddHours(24)));
        Assert.True(ApprovalRules.IsOverdue(requestedAt, requestedAt.AddHours(24).AddMinutes(1)));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void A_missing_reason_is_rejected(string? reason)
        => Assert.Null(ApprovalRules.NormalizeReason(reason));

    [Fact]
    public void A_reason_is_trimmed_and_limited_to_its_column()
    {
        Assert.Equal("العميل رجّع البضاعة", ApprovalRules.NormalizeReason("  العميل رجّع البضاعة "));
        Assert.NotNull(ApprovalRules.NormalizeReason(new string('x', ApprovalRequest.ReasonMaxLength)));
        Assert.Null(ApprovalRules.NormalizeReason(new string('x', ApprovalRequest.ReasonMaxLength + 1)));
    }

    [Fact]
    public void Every_type_has_a_tab()
    {
        foreach (var type in Enum.GetValues<ApprovalType>())
            Assert.True(Enum.IsDefined(ApprovalTypes.GroupOf(type)));
    }

    [Fact]
    public void Leaves_and_loans_share_a_tab()
        => Assert.Equal(ApprovalTypes.GroupOf(ApprovalType.Leave), ApprovalTypes.GroupOf(ApprovalType.Loan));
}
