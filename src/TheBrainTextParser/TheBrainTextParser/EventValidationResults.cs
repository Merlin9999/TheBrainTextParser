using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace TheBrainTextParser
{
    public class EventValidationResults
    {
        public bool IsValid { get; set; }
        public List<EventValidationError> Errors { get; } = new List<EventValidationError>();

        /// <inheritdoc />
        public override string ToString()
        {
            var msg = new StringBuilder();

            foreach (EventValidationError error in this.Errors)
            {
                msg.AppendLine(error.ToString());
                msg.AppendLine();
            }

            return msg.ToString();
        }
    }

    public class EventValidationError
    {
        public string Message { get; set; }
        public Exception Exception { get; set; }
        public override string ToString()
        {
            return $"{this.Message}\n{this.Exception}";
        }
    }
}