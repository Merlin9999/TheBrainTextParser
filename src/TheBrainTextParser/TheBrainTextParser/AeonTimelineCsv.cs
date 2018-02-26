using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;

namespace TheBrainTextParser
{
    public class AeonTimelineCsv
    {
        public static AeonTimelineCsv Create(IAeonEvent rootEvent)
        {
            var retVal = new AeonTimelineCsv();
            GenerateCsv(rootEvent, retVal, new List<int>() {1});
            return retVal;
        }

        public static void GenerateCsv(IAeonEvent currentEvent, AeonTimelineCsv csv, List<int> eventId)
        {
            string eventIdString = GenerateEventIdString(eventId);
            csv.Rows.Add(new AeonTimelineCsvRow(currentEvent, eventIdString));

            List<int> childEventId = new List<int>(eventId);
            childEventId.Add(0);
            foreach (IAeonEvent childEvent in currentEvent.Children)
            {
                int lastValue = childEventId[childEventId.Count - 1];
                childEventId[childEventId.Count - 1] = ++lastValue;

                GenerateCsv(childEvent, csv, childEventId);
            }
        }

        private static string GenerateEventIdString(List<int> eventId)
        {
            return eventId.Aggregate(string.Empty, (s, i) => s.Any() ? s + "." + i.ToString() : i.ToString());
        }

        public void Write(string fileName)
        {
            using (var csv = new CsvWriter(new StreamWriter(new FileStream(fileName, FileMode.Create))))
            {
                csv.WriteRecords(this.Rows);
            }
        }

        public List<AeonTimelineCsvRow> Rows { get; set; } = new List<AeonTimelineCsvRow>();
    }
}
