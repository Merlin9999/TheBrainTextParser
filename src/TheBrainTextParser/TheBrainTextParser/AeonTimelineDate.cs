using System;
using System.Text.RegularExpressions;

namespace TheBrainTextParser
{
    public class AeonTimelineDate
    {
        private static readonly Regex DateParseRegex = new Regex(@"^(?<Year>[0-9]{1,7})([.](?<Month>[0-9]{1,2})([.](?<Day>[0-9]{1,2}))?)?$");

        public static AeonTimelineDate Parse(string dateString)
        {
            Match match = DateParseRegex.Match(dateString);
            if (!match.Success)
                return null;

            AeonTimelineDate date = new AeonTimelineDate();

            Group yearGroup = match.Groups["Year"];
            Group monthGroup = match.Groups["Month"];
            Group dayGroup = match.Groups["Day"];

            date.Year = int.Parse(yearGroup.Value);
            date.Month = monthGroup.Success ? int.Parse(monthGroup.Value) : (int?) null;
            date.Day = dayGroup.Success ? int.Parse(dayGroup.Value) : (int?)null;

            if (date.Month <= 0)
                date.Month = null;

            if (date.Month > 12)
                return null;

            if (date.Month == null || date.Day <= 0)
                date.Day = null;

            if (date.Month != null && date.Day != null)
                if (DateTime.DaysInMonth(date.Year, date.Month.Value) < date.Day.Value)
                    return null;

            return date;
        }

        public int Year { get; set; }
        public int? Month { get; set; }
        public int? Day { get; set; }

        public DateTime AsDateTime()
        {
            return new DateTime(this.Year, this.ForcedMonth, this.ForcedDay);
        }

        private int ForcedMonth => this.Month == null || this.Month.Value < 1 ? 1 : this.Month.Value;
        private int ForcedDay => this.Day == null || this.Day.Value < 1 ? 1 : this.Day.Value;
    }
}