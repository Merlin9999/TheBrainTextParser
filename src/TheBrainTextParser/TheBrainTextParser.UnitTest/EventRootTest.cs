using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace TheBrainTextParser.UnitTest
{
    public class EventRootTest
    {
        [Fact]
        public void ReadFromFile()
        {
            EventRoot root = new EventRoot();
            root.LoadFromFile(@"C:\Users\Marc\Desktop\TheBrain_Events.txt");
            root.Should().NotBeNull();
            root.Groups.Should().NotBeNull();
            root.Groups.Should().HaveCount(7);

            root.Groups.Should().Match(x => x.All(g => g.Items.Any()));
        }
    }
}
