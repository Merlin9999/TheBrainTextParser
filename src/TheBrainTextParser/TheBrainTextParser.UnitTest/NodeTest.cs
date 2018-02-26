﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Bogus;
using FluentAssertions;
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
    }

    public interface IAeonEvent
    {
        string Text { get; set; }
        AeonTimelineDate Start { get; }
        AeonTimelineDate End { get; }
        TimeSpan Duration { get; }

        List<IAeonEvent> Children { get; }
    }

    public class AeonEvent : IAeonEvent
    {
        private static readonly Regex EventRegex = new Regex(@"^\s*((?<Start>[.0-9]{1,15})( *[-] *(?<End>[.0-9]{1,15}))? *[-] *)?(?<Name>.*)$");
        private AeonTimelineDate _start;
        private AeonTimelineDate _end;

        public static IAeonEvent Read(Node node)
        {
            IAeonEvent aeonEvent = AeonEvent.ReadOneNode(node);
            if (aeonEvent == null)
                return null;
            foreach (Node childNode in node.Children)
            {
                IAeonEvent childEvent = AeonEvent.Read(childNode);
                if (childEvent == null)
                    throw new ArgumentException($"Invalid child Node \"{childNode.Line}\"", nameof(node));
                aeonEvent.Children.Add(childEvent);
            }

            return aeonEvent;
        }

        public static AeonEvent ReadOneNode(Node node)
        {
            Match match = EventRegex.Match(node.Line);
            if (!match.Success)
                return null;

            Group startGroup = match.Groups["Start"];
            Group endGroup = match.Groups["End"];
            Group nameGroup = match.Groups["Name"];

            string tempText = nameGroup?.Value;
            if (tempText == null)
                return null;
            AeonEvent aeonEvent = new AeonEvent(tempText);

            aeonEvent.Start = ParseAeonTimelineDate(startGroup);
            aeonEvent.End = ParseAeonTimelineDate(endGroup);

            return aeonEvent;
        }

        private static AeonTimelineDate ParseAeonTimelineDate(Group group)
        {
            if (@group != null)
            {
                string temp = @group.Value;
                int yearLength = temp.IndexOf('.');
                if (yearLength < 0)
                    yearLength = temp.Length;
                if (yearLength >= 4)
                    return AeonTimelineDate.Parse(group.Value);
            }
            return null;
        }

        private AeonEvent(string text)
        {
            this.Text = text;
            this.Children = new List<IAeonEvent>();
        }

        public string Text { get; set; }

        public AeonTimelineDate Start
        {
            get => this._start ?? this.Children
                       .Select(e => new {ChildEvent = e, StartDateTime = e.Start.AsDateTime()})
                       .OrderBy(x => x.StartDateTime)
                       .FirstOrDefault()
                       ?.ChildEvent
                       .Start;
            set => this._start = value;
        }

        public AeonTimelineDate End
        {
            get => this._end ?? this.Children
                       .Select(e => new {ChildEvent = e, EndDateTime = e.End.AsDateTime()})
                       .OrderByDescending(x => x.EndDateTime)
                       .FirstOrDefault()
                       ?.ChildEvent
                       .End ?? this.Start;
            set => this._end = value;
        }

        public TimeSpan Duration => this.End.AsDateTime() - this.Start.AsDateTime();

        public List<IAeonEvent> Children { get; }

    }

    public class Node
    {
        public string Line { get; }
        public string Notes { get; }
        public List<Node> Children { get; }

        public Node()
            :this(string.Empty, (string)null)
        {
        }

        public Node(string line, params Node[] childNodes)
            : this(line, null, childNodes.AsEnumerable())
        {
        }

        public Node(string line, IEnumerable<Node> childNodes)
            : this(line, null, childNodes)
        {
        }

        public Node(string line, string notes, params Node[] childNodes)
            : this(line, notes, childNodes.AsEnumerable())
        {
        }

        public Node(string line, string notes, IEnumerable<Node> childNodes)
        {
            this.Line = line;
            this.Notes = notes ?? string.Empty;
            this.Children = childNodes.ToList();
        }

        public static Node Read(IEnumerable<string> lines)
        {
            return Read(lines.ToArray());
        }

        public static Node Read(string[] lines)
        {
            var rootNodes = new List<Node>();

            var readContext = new ReadContext(lines, rootNodes);

            while (readContext.Next())
                ;

            if (rootNodes.Count == 1)
                return rootNodes[0];
            return new Node(string.Empty, rootNodes);
        }

        internal class ReadContext
        {
            public string[] Lines { get; }
            public List<Node> NodeList { get; }

            private int curLineIdx;
            private Stack<Node> _nodeStack;

            public ReadContext(string[] lines, List<Node> nodeList)
            {
                this.Lines = lines;
                this.NodeList = nodeList;
                this.curLineIdx = 0;
                this._nodeStack = new Stack<Node>();
            }

            public bool Next()
            {
                string curLine = this.Lines[this.curLineIdx++];
                int currentAncestorCount = this._nodeStack.Count;
                int nextAncestorCount = curLine.TakeWhile(c => c == '\t').Count() + 1;
                if (currentAncestorCount + 1 == nextAncestorCount)
                    this.Push(curLine);
                else if (currentAncestorCount == nextAncestorCount + 1)
                    this.Pop(curLine);
                else if (currentAncestorCount == nextAncestorCount)
                    this.Next(curLine);
                else
                    throw new ArgumentException($"Invalid line: \"{curLine}\" Line Number {this.curLineIdx})");

                return this.Lines.Length > this.curLineIdx;
            }

            private void Pop(string curLine)
            {
                this._nodeStack.Pop();
                this._nodeStack.Pop();
                int nodeStackCount = this._nodeStack.Count;
                Node parentNode = nodeStackCount == 0 ? null : this._nodeStack.Peek();
                this.HandleNext(curLine, parentNode, nodeStackCount);
            }

            private void Push(string curLine)
            {
                int nodeStackCount = this._nodeStack.Count;
                Node parentNode = nodeStackCount == 0 ? null : this._nodeStack.Peek();
                this.HandleNext(curLine, parentNode, nodeStackCount);
            }

            private void Next(string curLine)
            {
                this._nodeStack.Pop();
                int nodeStackCount = this._nodeStack.Count;
                Node parentNode = nodeStackCount == 0 ? null : this._nodeStack.Peek();
                this.HandleNext(curLine, parentNode, nodeStackCount);
            }

            private void HandleNext(string curLine, Node parentNode, int currentAncestorCount)
            {
                Node node = new Node(curLine);
               
                if (currentAncestorCount == 0)
                    this.NodeList.Add(node);
                else
                    parentNode.Children.Add(node);

                this._nodeStack.Push(node);
            }
        }
    }
}
