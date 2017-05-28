using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using reWZ.WZProperties;

namespace WZData
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
        public Job(WZObject jobEntry)
        {
            Id = int.Parse(jobEntry.Name);
            Name = jobEntry.ResolvePath("info").ValueOrDefault<string>(null);
        }
    }
}
