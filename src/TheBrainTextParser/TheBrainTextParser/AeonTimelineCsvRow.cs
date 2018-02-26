using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml.Linq;

namespace TheBrainTextParser
{
    [DebuggerDisplay("EventId: {EventId}, Start: {Start}, End: {End}, Duration: {Duration}, Title: \"{Title}\"")]
    public class AeonTimelineCsvRow
    {
        public AeonTimelineCsvRow(IAeonEvent aeonEvent, string eventId)
        {
            this.EventId = eventId;
            this.Title = aeonEvent.Text;
            this.Start = aeonEvent.Start.ToString();
            this.Duration = aeonEvent.Duration.Days.ToString() + " Days";
            this.End = aeonEvent.End.ToString();
        }

        public string EventId { get; set; }
        public string Title { get; set; }
        public string Start { get; set; }
        public string Duration { get; set; }
        public string End { get; set; }
    }
}
