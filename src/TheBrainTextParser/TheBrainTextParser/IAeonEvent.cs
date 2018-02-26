using System;
using System.Collections.Generic;

namespace TheBrainTextParser
{
    public interface IAeonEvent
    {
        string Text { get; set; }
        AeonTimelineDate Start { get; }
        AeonTimelineDate End { get; }
        TimeSpan Duration { get; }

        List<IAeonEvent> Children { get; }
    }
}