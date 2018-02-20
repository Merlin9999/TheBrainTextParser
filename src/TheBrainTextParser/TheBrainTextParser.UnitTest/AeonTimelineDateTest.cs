using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using FluentAssertions.Common;
using Xunit;

namespace TheBrainTextParser.UnitTest
{
    public class AeonTimelineDateTest
    {
        [Fact]
        public void EmptyDateString()
        {
            AeonTimelineDate date = AeonTimelineDate.Parse(@"");
            date.Should().BeNull();
        }

        [Fact]
        public void YearOnlyWithZerosForMonthDay()
        {
            AeonTimelineDate date = AeonTimelineDate.Parse(@"2014.00.00");
            date.Should().NotBeNull();
            date.Year.Should().Be(2014);
            date.Month.Should().BeNull();
            date.Day.Should().BeNull();
        }

        [Fact]
        public void YearOnlyWithZerosForMonth()
        {
            AeonTimelineDate date = AeonTimelineDate.Parse(@"2014.00");
            date.Should().NotBeNull();
            date.Year.Should().Be(2014);
            date.Month.Should().BeNull();
            date.Day.Should().BeNull();
        }

        [Fact]
        public void YearOnly()
        {
            AeonTimelineDate date = AeonTimelineDate.Parse(@"2014");
            date.Should().NotBeNull();
            date.Year.Should().Be(2014);
            date.Month.Should().BeNull();
            date.Day.Should().BeNull();
        }

        [Fact]
        public void YearOnlyAsOneDigit()
        {
            AeonTimelineDate date = AeonTimelineDate.Parse(@"2");
            date.Should().NotBeNull();
            date.Year.Should().Be(2);
            date.Month.Should().BeNull();
            date.Day.Should().BeNull();
        }

        [Fact]
        public void YearOnlyAsSevenDigits()
        {
            AeonTimelineDate date = AeonTimelineDate.Parse(@"9999999");
            date.Should().NotBeNull();
            date.Year.Should().Be(9999999);
            date.Month.Should().BeNull();
            date.Day.Should().BeNull();
        }

        [Fact]
        public void YearMonthWithZerosForDay()
        {
            AeonTimelineDate date = AeonTimelineDate.Parse(@"2014.12.00");
            date.Should().NotBeNull();
            date.Year.Should().Be(2014);
            date.Month.Should().Be(12);
            date.Day.Should().BeNull();
        }

        [Fact]
        public void YearMonthOnly()
        {
            AeonTimelineDate date = AeonTimelineDate.Parse(@"2014.12");
            date.Should().NotBeNull();
            date.Year.Should().Be(2014);
            date.Month.Should().Be(12);
            date.Day.Should().BeNull();
        }

        [Fact]
        public void YearMonthOnlyWithOneDigitMonth()
        {
            AeonTimelineDate date = AeonTimelineDate.Parse(@"2014.7");
            date.Should().NotBeNull();
            date.Year.Should().Be(2014);
            date.Month.Should().Be(7);
            date.Day.Should().BeNull();
        }

        [Fact]
        public void YearMonthDay()
        {
            AeonTimelineDate date = AeonTimelineDate.Parse(@"2014.12.18");
            date.Should().NotBeNull();
            date.Year.Should().Be(2014);
            date.Month.Should().Be(12);
            date.Day.Should().Be(18);
        }

        [Fact]
        public void YearMonthDayWithOndeDigitDay()
        {
            AeonTimelineDate date = AeonTimelineDate.Parse(@"2014.12.3");
            date.Should().NotBeNull();
            date.Year.Should().Be(2014);
            date.Month.Should().Be(12);
            date.Day.Should().Be(3);
        }

        [Fact]
        public void MonthGreaterThan12()
        {
            AeonTimelineDate date = AeonTimelineDate.Parse(@"2014.13");
            date.Should().BeNull();
        }

        [Fact]
        public void DayGreaterThanMonthAllows()
        {
            AeonTimelineDate date = AeonTimelineDate.Parse(@"2017.2.29");
            date.Should().BeNull();
        }

        [Fact]
        public void YearMonthDayAsDateTime()
        {
            AeonTimelineDate date = AeonTimelineDate.Parse(@"2014.12.18");
            date.Should().NotBeNull();
            date.AsDateTime().Should().Be(new DateTime(2014, 12, 18));
        }

        [Fact]
        public void YearMonthAsDateTime()
        {
            AeonTimelineDate date = AeonTimelineDate.Parse(@"2014.12");
            date.Should().NotBeNull();
            date.AsDateTime().Should().Be(new DateTime(2014, 12, 1));
        }

        [Fact]
        public void YearAsDateTime()
        {
            AeonTimelineDate date = AeonTimelineDate.Parse(@"2014");
            date.Should().NotBeNull();
            date.AsDateTime().Should().Be(new DateTime(2014, 1, 1));
        }
    }
}
