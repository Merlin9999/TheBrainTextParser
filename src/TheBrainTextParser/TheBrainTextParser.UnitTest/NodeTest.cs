using System.Collections;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Bogus;
using FluentAssertions;
using NodaTime;
using Xunit;

namespace TheBrainTextParser.UnitTest
{
    public class NodeTest
    {
        private static readonly string[] NodeLinesWithoutNotes = new[]
        {
            "01 - Peace",
            "\t2018.06.01 - 2020.09.17 - Peace Demonstrations",
            "\t\t2018.06.01 - 2018.06.14 - Two Weeks of Peace",
            "\t\t2018.09.19 - 2018.09.29 - Remember Peace",
            "\t\t2019.10.03 - 2019.11.03 - Know the Peace",
            "\t\t2020.08.15 - 2020.09.17 - Peace as a Way of Life",
            "\t2021.05.13 - World Integration Declaration",
            "\t2025.02.27 - Articles of World Cooperation",
            "02 - Reconciling History",
            "\t2022.08.02 - Forgetting the Dogma of the Past",
            "\t2023.07.23 - Scientific Common Ground",
            "\t2025.11.19 - Integrating the Real Past",
        };

        private static readonly string Level0CategoryLine = NodeLinesWithoutNotes[0];
        private static readonly string Level1HistoricRangeLine = NodeLinesWithoutNotes[1];
        private static readonly string Level2HistoricRangeLine = NodeLinesWithoutNotes[2];
        private static readonly string Level1HistoricDateLine = NodeLinesWithoutNotes[7];


        [Fact]
        public void CreateEmptyTextNode()
        {
            Node node = new Node();
            node.Line.Should().BeEquivalentTo(string.Empty);
        }

        [Fact]
        public void CreateNodeFromTextLine()
        {
            Node node = new Node(Level0CategoryLine);
            node.Line.Should().BeEquivalentTo(Level0CategoryLine);
            node.Children.Should().HaveCount(0);
            node.Notes.Should().BeEmpty();
        }

        [Fact]
        public void CreateNodeTreeWithChildren()
        {
            Node rootNode = new Node(Level0CategoryLine,
                new Node(Level1HistoricRangeLine,
                    new Node(Level2HistoricRangeLine)),
                new Node(Level1HistoricDateLine));
            rootNode.Children.Should().HaveCount(2);
            rootNode.Notes.Should().BeEmpty();
        }

        [Fact]
        public void CreateNodeTreeWithNotes()
        {
            var faker = new Faker("en");
            
            Node rootNode = new Node(Level0CategoryLine, faker.Lorem.Paragraphs(2),
                new Node(Level1HistoricRangeLine, faker.Lorem.Paragraphs(5),
                    new Node(Level2HistoricRangeLine, faker.Lorem.Paragraphs(4))),
                new Node(Level1HistoricDateLine, faker.Lorem.Paragraphs(8)));
            rootNode.Children.Should().HaveCount(2);
            rootNode.Notes.Should().NotBeEmpty();
        }

        [Fact]
        public void CreateNodeFromTextLines()
        {
            Node rootNode = Node.Read(NodeLinesWithoutNotes);
            rootNode.Notes.Should().BeEmpty();
            rootNode.Children.Should().HaveCount(2);
            rootNode.Children[0].Children.Should().HaveCount(3);
            rootNode.Children[0].Children[0].Children.Should().HaveCount(4);
            rootNode.Children[1].Children.Should().HaveCount(3);
        }

        [Fact]
        public void ReadAeonGroupEvent()
        {
            Node rootNode = Node.Read(NodeLinesWithoutNotes);

            AeonEvent groupEvent = AeonEvent.ReadOneNode(rootNode.Children[0]);
            groupEvent.Should().NotBeNull();
            groupEvent.Text.Should().BeEquivalentTo("Peace");
        }

        [Fact]
        public void ReadAeonEvent()
        {
            Node rootNode = Node.Read(NodeLinesWithoutNotes);

            AeonEvent aeonEvent = AeonEvent.ReadOneNode(rootNode.Children[0].Children[0]);
            aeonEvent.Should().NotBeNull();
            aeonEvent.Text.Should().BeEquivalentTo("Peace Demonstrations");
            aeonEvent.Start.Should().BeEquivalentTo(new AeonTimelineDate() { Year = 2018, Month = 6, Day = 1 });
            aeonEvent.End.Should().BeEquivalentTo(new AeonTimelineDate() { Year = 2020, Month = 9, Day = 17 });
            aeonEvent.Duration.Should().BeEquivalentTo(Duration.FromDays(839));
        }

        [Fact]
        public void ReadEmptyTextAeonEvent()
        {
            Node node = new Node();
            AeonEvent aeonEvent = AeonEvent.ReadOneNode(node);
            aeonEvent.Should().NotBeNull();
            aeonEvent.Text.Should().BeEquivalentTo(string.Empty);
        }


        [Fact]
        public void ReadAeonEvents()
        {
            Node rootNode = Node.Read(NodeLinesWithoutNotes);
            IAeonEvent rootAeonEvent = AeonEvent.Read(rootNode);
            
            rootAeonEvent.Children.Should().HaveCount(2);
            rootAeonEvent.Children[0].Children.Should().HaveCount(3);
            rootAeonEvent.Children[0].Children[0].Children.Should().HaveCount(4);
            rootAeonEvent.Children[1].Children.Should().HaveCount(3);

            IAeonEvent aeonEvent = rootAeonEvent.Children[0];
            aeonEvent.Start.Should().BeEquivalentTo(new AeonTimelineDate() { Year = 2018, Month = 6, Day = 1 }, $"Start Date for {nameof(IAeonEvent)} with Text of \"{aeonEvent.Text}\"");
            aeonEvent.End.Should().BeEquivalentTo(new AeonTimelineDate() { Year = 2025, Month = 2, Day = 27 }, $"End Date for {nameof(IAeonEvent)} with Text of \"{aeonEvent.Text}\"");

            aeonEvent = rootAeonEvent.Children[0].Children[0];
            aeonEvent.Start.Should().BeEquivalentTo(new AeonTimelineDate() { Year = 2018, Month = 6, Day = 1 }, $"Start Date for {nameof(IAeonEvent)} with Text of \"{aeonEvent.Text}\"");
            aeonEvent.End.Should().BeEquivalentTo(new AeonTimelineDate() { Year = 2020, Month = 9, Day = 17 }, $"End Date for {nameof(IAeonEvent)} with Text of \"{aeonEvent.Text}\"");
        }

        [Fact]
        public void ReadExportedDataAndWriteCsv()
        {
            string theBrainNodeTextFileName = @"C:\Users\Marc\Desktop\TheBrain_Events.txt";
            string aeonTimelineEventFileNameCsv = @"C:\Users\Marc\Desktop\AeonTime_Events.csv";

            string[] lines = File.ReadAllLines(theBrainNodeTextFileName);
            Node rootNode = Node.Read(lines);
            IAeonEvent rootEvent = AeonEvent.Read(rootNode);
            rootEvent.Should().NotBeNull();
            rootEvent.Start.Should().NotBeNull();
            EventValidationResults evr = rootEvent.Validate();
            evr.Should().NotBeNull();
            evr.IsValid.Should().Be(true, evr.ToString());
            evr.Errors.Should().BeEmpty();
            rootEvent.Start.Should().NotBeNull();
            rootEvent.End.Should().NotBeNull();

            var csv = AeonTimelineCsv.Create(rootEvent);
            csv.Should().NotBeNull();
            csv.Rows.Should().NotBeNull();
            csv.Rows.Should().HaveCount(72);
            csv.Write(aeonTimelineEventFileNameCsv);
        }
    }
}
