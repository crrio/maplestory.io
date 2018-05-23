using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PKG1;

namespace maplestory.io.Data
{
    public class Job
    {
        public int Id;
        public string Name;
        public Job()
        {
            Id = 0;
            Name = "Beginner";
        }
        public Job(WZProperty jobEntry)
        {
            Id = int.Parse(jobEntry.NameWithoutExtension);
            Name = jobEntry.ResolveForOrNull<string>("info");
        }
    }
}
