using FluentAssertions;
using Isotyp.Core.Entities;
using Isotyp.Core.Enums;
using Xunit;

namespace Isotyp.Core.Tests;

public class DataSourceTests
{
    [Fact]
    public void Create_ShouldInitializeAsActive()
    {
        // Arrange & Act
        var dataSource = DataSource.Create(
            "Test Database",
            "A test database for unit tests",
            DataSourceType.SqlServer,
            "local:test-connection-ref",
            "user1");

        // Assert
        dataSource.IsActive.Should().BeTrue();
        dataSource.Name.Should().Be("Test Database");
        dataSource.Type.Should().Be(DataSourceType.SqlServer);
        dataSource.LastConnectedAt.Should().BeNull();
        dataSource.LastValidatedAt.Should().BeNull();
    }

    [Fact]
    public void Update_ShouldUpdateNameAndDescription()
    {
        // Arrange
        var dataSource = DataSource.Create(
            "Original Name", "Original Description",
            DataSourceType.PostgreSql, "conn-ref", "user1");

        // Act
        dataSource.Update("New Name", "New Description", "user2");

        // Assert
        dataSource.Name.Should().Be("New Name");
        dataSource.Description.Should().Be("New Description");
        dataSource.UpdatedBy.Should().Be("user2");
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var dataSource = DataSource.Create(
            "Test", "Desc", DataSourceType.MySql, "conn-ref", "user1");

        // Act
        dataSource.Deactivate("admin");

        // Assert
        dataSource.IsActive.Should().BeFalse();
        dataSource.UpdatedBy.Should().Be("admin");
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var dataSource = DataSource.Create(
            "Test", "Desc", DataSourceType.MySql, "conn-ref", "user1");
        dataSource.Deactivate("admin");

        // Act
        dataSource.Activate("admin");

        // Assert
        dataSource.IsActive.Should().BeTrue();
    }

    [Fact]
    public void RecordConnection_ShouldUpdateLastConnectedAt()
    {
        // Arrange
        var dataSource = DataSource.Create(
            "Test", "Desc", DataSourceType.Sqlite, "conn-ref", "user1");
        var connectionTime = DateTime.UtcNow;

        // Act
        dataSource.RecordConnection(connectionTime);

        // Assert
        dataSource.LastConnectedAt.Should().Be(connectionTime);
    }

    [Fact]
    public void RecordValidation_ShouldUpdateLastValidatedAt()
    {
        // Arrange
        var dataSource = DataSource.Create(
            "Test", "Desc", DataSourceType.CsvFile, "conn-ref", "user1");
        var validationTime = DateTime.UtcNow;

        // Act
        dataSource.RecordValidation(validationTime);

        // Assert
        dataSource.LastValidatedAt.Should().Be(validationTime);
    }
}
