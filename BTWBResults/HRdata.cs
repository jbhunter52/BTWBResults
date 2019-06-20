using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;


namespace BTWBResults
{
    public class HRdata
    {
        public List<HRWorkout> HRWorkouts;
        public HRdata(string appDir)
        {
            //Get xml files
            string dir = Path.Combine(appDir, "Data");

            var files = Directory.GetFiles(dir, "*.json", SearchOption.TopDirectoryOnly);

            //Get series data
            HRWorkouts = new List<HRWorkout>();
            foreach (var f in files)
            {
                string[] split = f.Split('-');
                DateTime date = new DateTime(int.Parse(split[2]), int.Parse(split[3]), int.Parse(split[4]));
                string json = File.ReadAllText(f);
                try
                {
                    var data = Newtonsoft.Json.JsonConvert.DeserializeObject<HRWorkout>(json);
                    HRWorkouts.Add(data);
                }
                catch
                {

                }
            }
        }
    }
}
