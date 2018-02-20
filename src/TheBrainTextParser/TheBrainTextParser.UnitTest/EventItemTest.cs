using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using Xunit;

namespace TheBrainTextParser.UnitTest
{
    public class EventItemTest
    {
        [Fact]
        public void NoEndDate()
        {
            EventItem eventItem = EventItem.Read(@"1898.00.00 - Spanish-American War");
            eventItem.Should().NotBeNull();
            AeonTimelineDate startDate = AeonTimelineDate.Parse("1898");
            eventItem.Start.Should().BeEquivalentTo(startDate);
            eventItem.End.Should().BeEquivalentTo(startDate);
            eventItem.Name.Should().BeEquivalentTo("Spanish-American War");
            eventItem.Duration.Should().Be(new TimeSpan(0));
            eventItem.Duration.Stringified().Should().Be("0 Days");
        }

        [Fact]
        public void WithEndDate()
        {
            EventItem eventItem = EventItem.Read(@"1939.09.00 - 1945.00.00 - World War II (WWII WW2)");
            eventItem.Should().NotBeNull();
            eventItem.Start.Should().BeEquivalentTo(AeonTimelineDate.Parse("1939.09"));
            eventItem.End.Should().BeEquivalentTo(AeonTimelineDate.Parse("1945"));
            eventItem.Name.Should().BeEquivalentTo("World War II (WWII WW2)");
            eventItem.Duration.Days.Should().Be(1949);
            eventItem.Duration.Stringified().Should().Be("1949 Days");
        }

    }
}
