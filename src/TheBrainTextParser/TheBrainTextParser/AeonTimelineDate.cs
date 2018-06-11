using System;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using NodaTime;
using NodaTime.Calendars;

namespace TheBrainTextParser
{
    [DebuggerDisplay("Year: {Year}, Month: {Month}, Day: {Day} : {nameof(AeonTimelineDate)}")]
    public class AeonTimelineDate
    {
        private static readonly Regex DateParseRegex = new Regex(@"^(?<Year>(([0-9]{1,7})|([(][0-9]{1,7}[)])|([-][0-9]{1,7})))([.](?<Month>[0-9]{1,2})([.](?<Day>[0-9]{1,2}))?)?$");

        public static AeonTimelineDate Parse(string dateString)
        {
            Match match = DateParseRegex.Match(dateString);
            if (!match.Success)
                return null;

            AeonTimelineDate date = new AeonTimelineDate();

            Group yearGroup = match.Groups["Year"];
            Group monthGroup = match.Groups["Month"];
            Group dayGroup = match.Groups["Day"];

            date.Year = int.Parse(yearGroup.Value.Replace('(', '-').Replace(")", string.Empty));
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

        public override string ToString()
        {
            return this.AsString();
        }

        public int Year { get; set; }
        public int? Month { get; set; }
        public int? Day { get; set; }

        protected internal int ForcedMonth => this.Month == null || this.Month.Value < 1 ? 1 : this.Month.Value;
        protected internal int ForcedDay => this.Day == null || this.Day.Value < 1 ? 1 : this.Day.Value;
    }

    public static class AeontimelineDateExtensions
    {
        public static LocalDate? AsLocalDate(this AeonTimelineDate _this)
        {
            if (_this == null)
                return null;

            return _this.Year < 0 
                ? new LocalDate(Era.BeforeCommon, -_this.Year, _this.ForcedMonth, _this.ForcedDay) 
                : new LocalDate(Era.Common, _this.Year, _this.ForcedMonth, _this.ForcedDay);
        }

        internal static string AsString(this AeonTimelineDate _this)
        {
            LocalDate? localDate = _this.AsLocalDate();

            if (localDate == null)
                return string.Empty;
            
            if (_this.Year < 1)
                return localDate.Value.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture) + " BC";

            return localDate.Value.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
        }
    }
}