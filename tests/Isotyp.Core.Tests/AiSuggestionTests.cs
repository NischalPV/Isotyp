using FluentAssertions;
using Isotyp.Core.Entities;
using Isotyp.Core.Enums;
using Xunit;

namespace Isotyp.Core.Tests;

public class AiSuggestionTests
{
    [Fact]
    public void Create_ShouldInitializeAsUnreviewed()
    {
        // Arrange & Act
        var suggestion = AiSuggestion.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            ChangeType.AddColumn,
            "{ \"column\": \"suggested_column\" }",
            "Pattern detected in data",
            85,
            "High null rate detected",
            "ai-system");

        // Assert
        suggestion.IsReviewed.Should().BeFalse();
        suggestion.IsAccepted.Should().BeNull();
        suggestion.ConfidenceScore.Should().Be(85);
    }

    [Fact]
    public void Accept_ShouldMarkAsReviewed()
    {
        // Arrange
        var suggestion = AiSuggestion.Create(
            Guid.NewGuid(), Guid.NewGuid(), ChangeType.AddColumn,
            "{ }", "Reasoning", 90, "Patterns", "ai-system");

        // Act
        suggestion.Accept("reviewer1", "Accepted for implementation");

        // Assert
        suggestion.IsReviewed.Should().BeTrue();
        suggestion.IsAccepted.Should().BeTrue();
        suggestion.ReviewedBy.Should().Be("reviewer1");
        suggestion.ReviewComments.Should().Be("Accepted for implementation");
    }

    [Fact]
    public void Reject_ShouldMarkAsReviewed()
    {
        // Arrange
        var suggestion = AiSuggestion.Create(
            Guid.NewGuid(), Guid.NewGuid(), ChangeType.AddColumn,
            "{ }", "Reasoning", 60, "Patterns", "ai-system");

        // Act
        suggestion.Reject("reviewer1", "Not suitable for our use case");

        // Assert
        suggestion.IsReviewed.Should().BeTrue();
        suggestion.IsAccepted.Should().BeFalse();
        suggestion.ReviewComments.Should().Be("Not suitable for our use case");
    }

    [Fact]
    public void Accept_ShouldThrowIfAlreadyReviewed()
    {
        // Arrange
        var suggestion = AiSuggestion.Create(
            Guid.NewGuid(), Guid.NewGuid(), ChangeType.AddColumn,
            "{ }", "Reasoning", 90, "Patterns", "ai-system");
        suggestion.Accept("reviewer1", null);

        // Act & Assert
        var act = () => suggestion.Accept("reviewer2", null);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ConfidenceScore_ShouldBeClampedTo0To100()
    {
        // Arrange & Act
        var tooHigh = AiSuggestion.Create(
            Guid.NewGuid(), Guid.NewGuid(), ChangeType.AddColumn,
            "{ }", "Reasoning", 150, "Patterns", "ai-system");

        var tooLow = AiSuggestion.Create(
            Guid.NewGuid(), Guid.NewGuid(), ChangeType.AddColumn,
            "{ }", "Reasoning", -10, "Patterns", "ai-system");

        // Assert
        tooHigh.ConfidenceScore.Should().Be(100);
        tooLow.ConfidenceScore.Should().Be(0);
    }

    [Fact]
    public void RequiresHighConfidenceForAutoSuggestion_ShouldBeTrueForDestructiveChanges()
    {
        // Arrange
        var removeColumn = AiSuggestion.Create(
            Guid.NewGuid(), Guid.NewGuid(), ChangeType.RemoveColumn,
            "{ }", "Reasoning", 90, "Patterns", "ai-system");

        var addColumn = AiSuggestion.Create(
            Guid.NewGuid(), Guid.NewGuid(), ChangeType.AddColumn,
            "{ }", "Reasoning", 90, "Patterns", "ai-system");

        // Assert
        removeColumn.RequiresHighConfidenceForAutoSuggestion().Should().BeTrue();
        addColumn.RequiresHighConfidenceForAutoSuggestion().Should().BeFalse();
    }

    [Fact]
    public void LinkToChangeRequest_ShouldOnlyWorkForAcceptedSuggestions()
    {
        // Arrange
        var suggestion = AiSuggestion.Create(
            Guid.NewGuid(), Guid.NewGuid(), ChangeType.AddColumn,
            "{ }", "Reasoning", 90, "Patterns", "ai-system");

        // Act & Assert - should fail when not accepted
        var act = () => suggestion.LinkToChangeRequest(Guid.NewGuid(), "user1");
        act.Should().Throw<InvalidOperationException>();

        // Accept first
        suggestion.Accept("reviewer1", null);
        
        // Now should work
        var changeRequestId = Guid.NewGuid();
        suggestion.LinkToChangeRequest(changeRequestId, "user1");
        suggestion.CreatedChangeRequestId.Should().Be(changeRequestId);
    }
}
