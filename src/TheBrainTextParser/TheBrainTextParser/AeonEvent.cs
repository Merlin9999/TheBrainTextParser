using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NodaTime;

namespace TheBrainTextParser
{
    public class AeonEvent : IAeonEvent
    {
        private static readonly Regex EventRegex = new Regex(@"^[\s_]*((?<Start>[.\(\)\-0-9]{1,15})([\s]*[-][\s_]*(?<End>[.\(\)\-0-9]{1,15}))?\s*[-]\s*)?(?<Name>.*)$");
        private AeonTimelineDate _start;
        private AeonTimelineDate _end;

        public static IAeonEvent Read(Node node)
        {
            AeonEvent aeonEvent = AeonEvent.ReadOneNode(node);
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
                temp = temp.Trim('-', '(', ')');
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
            get => this._start ?? this.Children.Select(e => new {ChildEvent = e, StartDate = e.Start.AsLocalDate()})
                       .OrderBy(x => x.StartDate)
                       .FirstOrDefault()
                       ?.ChildEvent
                       .Start;
            set => this._start = value;
        }

        public AeonTimelineDate End
        {
            get => this._end ?? this.Children.Select(e => new {ChildEvent = e, EndDate = e.End.AsLocalDate()})
                       .OrderByDescending(x => x.EndDate)
                       .FirstOrDefault()
                       ?.ChildEvent
                       .End ?? this.Start;
            set => this._end = value;
        }

        public Period Period => this.End.AsLocalDate() - this.Start.AsLocalDate();

        public List<IAeonEvent> Children { get; }
        public EventValidationResults Validate()
        {
            var evr = new EventValidationResults();
            evr.IsValid = true;
            this.ValidateEvents(evr, this);
            return evr;
        }

        private void ValidateEvents(EventValidationResults evr, IAeonEvent aeonEvent)
        {
            foreach (IAeonEvent childEvent in aeonEvent.Children)
            {
                this.ValidateEvents(evr, childEvent);
            }

            try
            {
                string temp = aeonEvent.Text;
            }
            catch (Exception e)
            {
                evr.IsValid = false;
                evr.Errors.Add(new EventValidationError()
                {
                    Message = $"Failed to access {nameof(this.Text)} property of event named <Idunno, {nameof(this.Text)} is not accessable>",
                    Exception = e,
                });
            }

            try
            {
                AeonTimelineDate temp = aeonEvent.Start;
            }
            catch (Exception e)
            {
                evr.IsValid = false;
                evr.Errors.Add(new EventValidationError()
                {
                    Message = $"Failed to access {nameof(this.Start)} property of event named \"{this.Text}\"",
                    Exception = e,
                });
            }

            try
            {
                AeonTimelineDate temp = aeonEvent.End;
            }
            catch (Exception e)
            {
                evr.IsValid = false;
                evr.Errors.Add(new EventValidationError()
                {
                    Message = $"Failed to access {nameof(this.End)} property of event named \"{this.Text}\"",
                    Exception = e,
                });
            }

            try
            {
                Period temp = aeonEvent.Period;
            }
            catch (Exception e)
            {
                evr.IsValid = false;
                evr.Errors.Add(new EventValidationError()
                {
                    Message = $"Failed to access {nameof(this.Period)} property of event named \"{this.Text}\"",
                    Exception = e,
                });
            }
        }
    }
}