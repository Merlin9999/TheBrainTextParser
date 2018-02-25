using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Xunit;

namespace TheBrainTextParser.UnitTest
{
    public class ProgramTest
    {
        [Fact]
        public async Task Test1()
        {
            await Program.RunOptionsAndReturnExitCodeAsync(new ProgramArguments()
            {
                InputFile = @"C:\Users\Marc\Desktop\TheBrain_Events.txt"
            });
        }
    }
}
