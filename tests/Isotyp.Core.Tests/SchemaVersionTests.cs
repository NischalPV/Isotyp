using FluentAssertions;
using Isotyp.Core.Entities;
using Isotyp.Core.Enums;
using Xunit;

namespace Isotyp.Core.Tests;

public class SchemaVersionTests
{
    [Fact]
    public void Create_ShouldInitializeWithPendingStatus()
    {
        // Arrange & Act
        var schemaVersion = SchemaVersion.Create(
            Guid.NewGuid(),
            1, 0, 0,
            "{ \"tables\": [] }",
            "Initial schema",
            "// ORM mappings",
            "-- Migration",
            "-- Rollback",
            null,
            "user1");

        // Assert
        schemaVersion.Status.Should().Be(ApprovalStatus.Pending);
        schemaVersion.LockType.Should().Be(SchemaLockType.None);
        schemaVersion.GetVersionString().Should().Be("1.0.0");
    }

    [Fact]
    public void Submit_ShouldChangeStatusToSubmitted()
    {
        // Arrange
        var schemaVersion = SchemaVersion.Create(
            Guid.NewGuid(), 1, 0, 0,
            "{ }", "Test", "// ORM", "-- Mig", "-- Roll", null, "user1");

        // Act
        schemaVersion.Submit("user1");

        // Assert
        schemaVersion.Status.Should().Be(ApprovalStatus.Submitted);
    }

    [Fact]
    public void ApproveTechnical_ShouldOnlyWorkOnSubmittedVersions()
    {
        // Arrange
        var schemaVersion = SchemaVersion.Create(
            Guid.NewGuid(), 1, 0, 0,
            "{ }", "Test", "// ORM", "-- Mig", "-- Roll", null, "user1");

        // Act & Assert - should fail when pending
        var act = () => schemaVersion.ApproveTechnical("approver1");
        act.Should().Throw<InvalidOperationException>();

        // Submit first, then approve
        schemaVersion.Submit("user1");
        schemaVersion.ApproveTechnical("approver1");
        schemaVersion.Status.Should().Be(ApprovalStatus.TechnicalApproved);
    }

    [Fact]
    public void ApprovalWorkflow_ShouldFollowCorrectOrder()
    {
        // Arrange
        var schemaVersion = SchemaVersion.Create(
            Guid.NewGuid(), 1, 0, 0,
            "{ }", "Test", "// ORM", "-- Mig", "-- Roll", null, "user1");

        // Act & Assert
        schemaVersion.Submit("user1");
        schemaVersion.Status.Should().Be(ApprovalStatus.Submitted);

        schemaVersion.ApproveTechnical("tech1");
        schemaVersion.Status.Should().Be(ApprovalStatus.TechnicalApproved);

        schemaVersion.ApproveBusiness("business1");
        schemaVersion.Status.Should().Be(ApprovalStatus.BusinessApproved);

        schemaVersion.ApproveGovernance("governance1");
        schemaVersion.Status.Should().Be(ApprovalStatus.FullyApproved);
    }

    [Fact]
    public void MarkAsApplied_ShouldOnlyWorkOnFullyApprovedVersions()
    {
        // Arrange
        var schemaVersion = SchemaVersion.Create(
            Guid.NewGuid(), 1, 0, 0,
            "{ }", "Test", "// ORM", "-- Mig", "-- Roll", null, "user1");

        // Act & Assert - should fail when not fully approved
        var act = () => schemaVersion.MarkAsApplied("user1", DateTime.UtcNow);
        act.Should().Throw<InvalidOperationException>();

        // Complete approval workflow
        schemaVersion.Submit("user1");
        schemaVersion.ApproveTechnical("tech1");
        schemaVersion.ApproveBusiness("business1");
        schemaVersion.ApproveGovernance("governance1");

        // Now it should work
        schemaVersion.MarkAsApplied("user1", DateTime.UtcNow);
        schemaVersion.Status.Should().Be(ApprovalStatus.Applied);
        schemaVersion.AppliedAt.Should().NotBeNull();
    }

    [Fact]
    public void ApplyLock_ShouldOnlyWorkOnAppliedVersions()
    {
        // Arrange
        var schemaVersion = CreateFullyAppliedVersion();

        // Act
        schemaVersion.ApplyLock(SchemaLockType.HardLock, "admin");

        // Assert
        schemaVersion.LockType.Should().Be(SchemaLockType.HardLock);
        schemaVersion.IsFullyLocked().Should().BeTrue();
    }

    [Fact]
    public void CanBeModified_ShouldRespectLockTypes()
    {
        // Arrange
        var schemaVersion = CreateFullyAppliedVersion();

        // No lock - can be modified
        schemaVersion.CanBeModified().Should().BeTrue();

        // Soft lock - can be modified
        schemaVersion.ApplyLock(SchemaLockType.SoftLock, "admin");
        schemaVersion.CanBeModified().Should().BeTrue();

        // Additive only - cannot be freely modified
        schemaVersion.ApplyLock(SchemaLockType.AdditiveOnly, "admin");
        schemaVersion.AllowsOnlyAdditive().Should().BeTrue();

        // Hard lock - cannot be modified
        schemaVersion.ApplyLock(SchemaLockType.HardLock, "admin");
        schemaVersion.IsFullyLocked().Should().BeTrue();
        schemaVersion.CanBeModified().Should().BeFalse();
    }

    [Fact]
    public void Rollback_ShouldOnlyWorkOnAppliedVersions()
    {
        // Arrange
        var schemaVersion = CreateFullyAppliedVersion();

        // Act
        schemaVersion.MarkAsRolledBack("admin", DateTime.UtcNow);

        // Assert
        schemaVersion.Status.Should().Be(ApprovalStatus.RolledBack);
        schemaVersion.RolledBackAt.Should().NotBeNull();
    }

    private static SchemaVersion CreateFullyAppliedVersion()
    {
        var schemaVersion = SchemaVersion.Create(
            Guid.NewGuid(), 1, 0, 0,
            "{ }", "Test", "// ORM", "-- Mig", "-- Roll", null, "user1");
        schemaVersion.Submit("user1");
        schemaVersion.ApproveTechnical("tech1");
        schemaVersion.ApproveBusiness("business1");
        schemaVersion.ApproveGovernance("governance1");
        schemaVersion.MarkAsApplied("user1", DateTime.UtcNow);
        return schemaVersion;
    }
}
