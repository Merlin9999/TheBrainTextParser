using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace TheBrainTextParser
{
    public class EventRoot
    {
        public EventRoot()
        {
            this.Groups = new List<EventGroup>();
        }

        public void LoadFromFile(string fileName)
        {
            string[] lines = File.ReadAllLines(fileName);

            this.Groups.Clear();

            foreach (string line in lines)
            {
                EventGroup group = EventGroup.Read(line);
                if (group != null)
                {
                    this.Groups.Add(group);
                    continue;
                }

                EventItem item = EventItem.Read(line);
                if (item == null)
                    throw new ApplicationException($"Line does not match: \"{line}\"");

                this.Groups[this.Groups.Count - 1].Items.Add(item);
                //continue;
            }
        }

        public List<EventGroup> Groups { get; set; }
    }

    [DebuggerDisplay("{Name} - Item Count: {Items.Count}")]
    public class EventGroup
    {
        private static readonly Regex EventGroupRegex = new Regex(@"^([0-9]+ [-] )(?<Name>.+)?$");

        public static EventGroup Read(string line)
        {
            Match match = EventGroupRegex.Match(line);
            if (!match.Success)
                return null;

            EventGroup eventGroup = new EventGroup();
            Group group = match.Groups["Name"];
            eventGroup.Items = new List<EventItem>();
            eventGroup.Name = group.Value;

            return eventGroup;
        }

        public string Name { get; set; }
        public List<EventItem> Items { get; set; }
    }

    [DebuggerDisplay("{Name} - Start: {Start.AsDateTime()} Duration Days: {Duration.Days} : {nameof(EventItem)}")]
    public class EventItem
    {
        private static readonly Regex EventItemRegex = new Regex(@"^\s*(?<Start>[.0-9]{1,15})( - (?<End>[.0-9]{1,15}))? - (?<Name>.+)$");

        public static EventItem Read(string line)
        {
            Match match = EventItemRegex.Match(line);
            if (!match.Success)
                return null;

            EventItem eventItem = new EventItem();
            Group startGroup = match.Groups["Start"];
            Group endGroup = match.Groups["End"];
            Group nameGroup = match.Groups["Name"];
            
            eventItem.Start = AeonTimelineDate.Parse(startGroup.Value);
            if (eventItem.Start == null)
                return null;
            eventItem.End = AeonTimelineDate.Parse(endGroup.Value) ?? eventItem.Start;

            eventItem.Name = nameGroup.Value;
            if (eventItem.Name == null)
                return null;

            return eventItem;
        }

        public string Name { get; set; }
        public AeonTimelineDate Start { get; set; }
        public AeonTimelineDate End { get; set; }
        public TimeSpan Duration => this.End.AsDateTime() - this.Start.AsDateTime();
    }

    public static class TimeSpanExtensions
    {
        public static string Stringified(this TimeSpan timeSpan)
        {
            return $"{timeSpan.Days} Days";
        }
    }
}