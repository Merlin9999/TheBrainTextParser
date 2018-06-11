using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace TheBrainTextParser
{
    [DebuggerDisplay("EventId: {EventId}, Start: {Start}, End: {End}, Period: {Duration}, Title: \"{Title}\"")]
    public class AeonTimelineCsvRow
    {
        public AeonTimelineCsvRow(IAeonEvent aeonEvent, List<int> eventId)
        {
            this.EventId = GenerateEventIdString(eventId);
            this.ParentId = eventId.Count > 1
                ? GenerateEventIdString(eventId.Take(eventId.Count - 1).ToList())
                : string.Empty;
            this.Title = aeonEvent.Text;
            this.Start = aeonEvent.Start.AsString();
            this.Duration = aeonEvent.Duration == null ? string.Empty : aeonEvent.Duration.Value.Days + " Days";
            this.End = aeonEvent.End.AsString();
        }

        private static string GenerateEventIdString(List<int> eventId)
        {
            return eventId.Aggregate(string.Empty, (s, i) => s.Any() ? s + "." + i.ToString() : i.ToString());
        }

        public string EventId { get; set; }
        public string ParentId { get; set; }
        public string Title { get; set; }
        public string Start { get; set; }
        public string Duration { get; set; }
        public string End { get; set; }
    }
}
