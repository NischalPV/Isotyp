using FluentAssertions;
using Isotyp.Core.Entities;
using Isotyp.Core.Enums;
using Xunit;

namespace Isotyp.Core.Tests;

public class SchemaChangeRequestTests
{
    [Fact]
    public void Create_ShouldInitializeWithPendingStatus()
    {
        // Arrange & Act
        var request = SchemaChangeRequest.Create(
            Guid.NewGuid(),
            ChangeType.AddColumn,
            "{ \"column\": \"new_column\" }",
            "Add new column",
            string.Empty,
            "No impact",
            false,
            null,
            "user1");

        // Assert
        request.Status.Should().Be(ApprovalStatus.Pending);
        request.IsAiSuggested.Should().BeFalse();
    }

    [Fact]
    public void Create_ShouldIdentifyDestructiveChanges()
    {
        // Arrange & Act
        var removeColumn = SchemaChangeRequest.Create(
            Guid.NewGuid(), ChangeType.RemoveColumn, "{ }", "Remove column",
            string.Empty, "Impact", false, null, "user1");

        var addColumn = SchemaChangeRequest.Create(
            Guid.NewGuid(), ChangeType.AddColumn, "{ }", "Add column",
            string.Empty, "Impact", false, null, "user1");

        // Assert
        removeColumn.IsDestructive.Should().BeTrue();
        addColumn.IsDestructive.Should().BeFalse();
    }

    [Fact]
    public void Submit_ShouldRequireJustification()
    {
        // Arrange
        var request = SchemaChangeRequest.Create(
            Guid.NewGuid(), ChangeType.AddColumn, "{ }", "Add column",
            string.Empty, "Impact", false, null, "user1");

        // Act
        request.Submit("This change is needed because...", "user1");

        // Assert
        request.Status.Should().Be(ApprovalStatus.Submitted);
        request.Justification.Should().NotBeEmpty();
    }

    [Fact]
    public void AddApproval_ShouldUpdateStatus()
    {
        // Arrange
        var request = SchemaChangeRequest.Create(
            Guid.NewGuid(), ChangeType.AddColumn, "{ }", "Add column",
            string.Empty, "Impact", false, null, "user1");
        request.Submit("Justification", "user1");

        // Act - Technical approval
        var techApproval = SchemaChangeApproval.Create(
            request.Id, ApprovalLayer.Technical, true, "tech1", "Tech Approver", "Looks good", "tech1");
        request.AddApproval(techApproval);

        // Assert
        request.Status.Should().Be(ApprovalStatus.TechnicalApproved);
        request.Approvals.Count.Should().Be(1);
    }

    [Fact]
    public void AddApproval_ShouldRejectOnAnyRejection()
    {
        // Arrange
        var request = SchemaChangeRequest.Create(
            Guid.NewGuid(), ChangeType.AddColumn, "{ }", "Add column",
            string.Empty, "Impact", false, null, "user1");
        request.Submit("Justification", "user1");

        // Act - Technical rejection
        var techApproval = SchemaChangeApproval.Create(
            request.Id, ApprovalLayer.Technical, false, "tech1", "Tech Approver", "Needs work", "tech1");
        request.AddApproval(techApproval);

        // Assert
        request.Status.Should().Be(ApprovalStatus.Rejected);
    }

    [Fact]
    public void MultiLayerApproval_ShouldRequireAllLayers()
    {
        // Arrange
        var request = SchemaChangeRequest.Create(
            Guid.NewGuid(), ChangeType.AddColumn, "{ }", "Add column",
            string.Empty, "Impact", false, null, "user1");
        request.Submit("Justification", "user1");

        // Act - All three approvals
        request.AddApproval(SchemaChangeApproval.Create(
            request.Id, ApprovalLayer.Technical, true, "tech1", "Tech", "", "tech1"));
        request.AddApproval(SchemaChangeApproval.Create(
            request.Id, ApprovalLayer.Business, true, "bus1", "Business", "", "bus1"));
        request.AddApproval(SchemaChangeApproval.Create(
            request.Id, ApprovalLayer.DataGovernance, true, "gov1", "Governance", "", "gov1"));

        // Assert
        request.Status.Should().Be(ApprovalStatus.FullyApproved);
    }

    [Fact]
    public void RequiresExtraApproval_ShouldBeTrueForDestructiveChanges()
    {
        // Arrange
        var destructive = SchemaChangeRequest.Create(
            Guid.NewGuid(), ChangeType.RemoveColumn, "{ }", "Remove column",
            string.Empty, "Impact", false, null, "user1");

        var nonDestructive = SchemaChangeRequest.Create(
            Guid.NewGuid(), ChangeType.AddColumn, "{ }", "Add column",
            string.Empty, "Impact", false, null, "user1");

        // Assert
        destructive.RequiresExtraApproval().Should().BeTrue();
        nonDestructive.RequiresExtraApproval().Should().BeFalse();
    }

    [Fact]
    public void RequiresExtraApproval_ShouldBeTrueForLowConfidenceAiSuggestions()
    {
        // Arrange
        var lowConfidence = SchemaChangeRequest.Create(
            Guid.NewGuid(), ChangeType.AddColumn, "{ }", "Add column",
            string.Empty, "Impact", true, 70, "user1");

        var highConfidence = SchemaChangeRequest.Create(
            Guid.NewGuid(), ChangeType.AddColumn, "{ }", "Add column",
            string.Empty, "Impact", true, 90, "user1");

        // Assert
        lowConfidence.RequiresExtraApproval().Should().BeTrue();
        highConfidence.RequiresExtraApproval().Should().BeFalse();
    }
}
