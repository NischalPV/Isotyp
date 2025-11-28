using FluentAssertions;
using Isotyp.Core.Entities;
using Xunit;

namespace Isotyp.Core.Tests;

public class AgentTests
{
    [Fact]
    public void Create_ShouldInitializeAsDisconnected()
    {
        // Arrange & Act
        var agent = Agent.Create(
            "Test Agent",
            "A test agent",
            "agent-key-123",
            "1.0.0",
            "localhost",
            "user1");

        // Assert
        agent.IsConnected.Should().BeFalse();
        agent.LastHeartbeat.Should().BeNull();
        agent.AgentKey.Should().Be("agent-key-123");
    }

    [Fact]
    public void Connect_ShouldSetIsConnectedAndRecordHeartbeat()
    {
        // Arrange
        var agent = Agent.Create(
            "Test Agent", "Desc", "key1", "1.0.0", "host1", "user1");

        // Act
        agent.Connect();

        // Assert
        agent.IsConnected.Should().BeTrue();
        agent.LastHeartbeat.Should().NotBeNull();
    }

    [Fact]
    public void Disconnect_ShouldSetIsConnectedToFalse()
    {
        // Arrange
        var agent = Agent.Create(
            "Test Agent", "Desc", "key1", "1.0.0", "host1", "user1");
        agent.Connect();

        // Act
        agent.Disconnect();

        // Assert
        agent.IsConnected.Should().BeFalse();
    }

    [Fact]
    public void AuthorizeDataSource_ShouldAddToAuthorizedList()
    {
        // Arrange
        var agent = Agent.Create(
            "Test Agent", "Desc", "key1", "1.0.0", "host1", "user1");
        var dataSourceId = Guid.NewGuid();

        // Act
        agent.AuthorizeDataSource(dataSourceId, "admin");

        // Assert
        agent.IsAuthorizedForDataSource(dataSourceId).Should().BeTrue();
    }

    [Fact]
    public void RevokeDataSourceAuthorization_ShouldRemoveFromAuthorizedList()
    {
        // Arrange
        var agent = Agent.Create(
            "Test Agent", "Desc", "key1", "1.0.0", "host1", "user1");
        var dataSourceId = Guid.NewGuid();
        agent.AuthorizeDataSource(dataSourceId, "admin");

        // Act
        agent.RevokeDataSourceAuthorization(dataSourceId, "admin");

        // Assert
        agent.IsAuthorizedForDataSource(dataSourceId).Should().BeFalse();
    }

    [Fact]
    public void MultipleDataSources_ShouldBeAuthorizedIndependently()
    {
        // Arrange
        var agent = Agent.Create(
            "Test Agent", "Desc", "key1", "1.0.0", "host1", "user1");
        var ds1 = Guid.NewGuid();
        var ds2 = Guid.NewGuid();

        // Act
        agent.AuthorizeDataSource(ds1, "admin");
        agent.AuthorizeDataSource(ds2, "admin");
        agent.RevokeDataSourceAuthorization(ds1, "admin");

        // Assert
        agent.IsAuthorizedForDataSource(ds1).Should().BeFalse();
        agent.IsAuthorizedForDataSource(ds2).Should().BeTrue();
    }
}
