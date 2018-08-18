using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace BTWBResults
{
    class Activity
    {
        public DateTime Date;
        public string Workout;
        public string Result;
        public bool RX;
        public bool Pukie;
        public int WorkPerformed;
        public int WorkTime;
        public string FormatedResult;
        public string Notes;
        public string Description;

        public Activity(string[] row)
        {
            foreach (string s in row)
            {
                s.Replace("\"", string.Empty);
            }
            string[] split = row;
            Date = DateTime.Parse(split[0]);
            Workout = split[1];
            Result = split[2];
            if (split[3].Equals("TRUE"))
                RX = true;
            else
                RX = false;
            if (split[4].Equals("TRUE"))
                Pukie = true;
            else
                Pukie = false;
            if (split[5] == string.Empty)
                WorkPerformed = 0;
            else
                WorkPerformed = Int32.Parse(split[5]);
            if (split[6] == string.Empty)
                WorkTime = 0;
            else
                WorkTime = Int32.Parse(split[6]);
            FormatedResult = split[7];
            Notes = split[8];
            Description = split[9];
        }

        private IEnumerable<string> SplitCSV(string input)
        {
            Regex csvSplit = new Regex("(?:^|,)(\"(?:[^\"]+|\"\")*\"|[^,]*)", RegexOptions.Compiled);

            foreach (Match match in csvSplit.Matches(input))
            {
                yield return match.Value.TrimStart(',');
            }
        }

        public bool IncludesRunning()
        {
            
            Match mat = Regex.Match(Description, "(?i)run");
            if (mat.Success)
                return true;
            else
                return false;
                
        }

    }
}
