using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TheBrainTextParser
{
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
            get => this._start ?? Enumerable.Select(this.Children, e => new {ChildEvent = e, StartDateTime = e.Start.AsDateTime()})
                       .OrderBy(x => x.StartDateTime)
                       .FirstOrDefault()
                       ?.ChildEvent
                       .Start;
            set => this._start = value;
        }

        public AeonTimelineDate End
        {
            get => this._end ?? Enumerable.Select(this.Children, e => new {ChildEvent = e, EndDateTime = e.End.AsDateTime()})
                       .OrderByDescending(x => x.EndDateTime)
                       .FirstOrDefault()
                       ?.ChildEvent
                       .End ?? this.Start;
            set => this._end = value;
        }

        public TimeSpan Duration => this.End.AsDateTime() - this.Start.AsDateTime();

        public List<IAeonEvent> Children { get; }

    }
}