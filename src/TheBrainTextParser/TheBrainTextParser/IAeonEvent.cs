using System;
using System.Collections.Generic;
using NodaTime;

namespace TheBrainTextParser
{
    public interface IAeonEvent
    {
        string Text { get; set; }
        AeonTimelineDate Start { get; }
        AeonTimelineDate End { get; }
        Period Period { get; }

        List<IAeonEvent> Children { get; }

        EventValidationResults Validate();
    }
}