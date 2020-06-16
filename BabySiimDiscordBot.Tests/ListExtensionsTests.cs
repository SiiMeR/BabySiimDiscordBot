using System.Collections.Generic;
using BabySiimDiscordBot.Extensions;
using FluentAssertions;
using Xunit;

namespace BabySiimDiscordBot.Tests
{
    public class ListExtensionsTests
    {
        [Fact]
        public void ChunkBy_ChunksAsExpected_Basic()
        {
            // Arrange
            var list = new List<string> {"one", "two"};
            // Act
            var result = list.ChunkBy(1);
            // Assert
            var expected = new List<List<string>>{ new List<string>{"one"}, new List<string>{"two"}};
            result.Should().HaveCount(2)
                .And.BeEquivalentTo(expected);
        }
    }
}
