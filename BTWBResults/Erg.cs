using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using System.IO;
using System.Xml;

namespace BTWBResults
{
    public class Erg
    {
        [XmlIgnore]
        public Activity Activity;
        public string WO;
        public ErgType Type;
        public int WorkTime;
        public int Calories;
        public double calPerMin;
        public int RndTime;
        public int RestTime;
        public int Rounds;
        public bool ParsedErg = false;

        public Erg(Activity a)
        {
            WO = a.Workout;
            if (a.Workout.StartsWith("\"Tabata\""))
            {
                if (a.Workout.Contains(" - Row Calories") || a.Workout.Contains(" - Echo Bike Calories") || a.Workout.Contains(" - Row (calories)") || a.Workout.Contains(" - Assault Bike Calories"))
                {
                    int i = a.Workout.IndexOf(':');
                    string[] split = a.Workout.Substring(i+1).Split('/');
                    split = split[0].Replace(" ", "").Split('x');
                    int rds = int.Parse(split[0]);
                    int secs = 0;
                    int cals = 0;

                    //Get round time
                    if (split[1].Contains("secs"))
                    {
                        split[1] = split[1].Replace("secs", "");
                        secs = int.Parse(split[1]);
                    }
                    else if (split[1].Contains("mins"))
                    {
                        split[1] = split[1].Replace("mins", "");
                        secs = int.Parse(split[1]) * 60;
                    }
                    else if (split[1].Contains("min"))
                    {
                        split[1] = split[1].Replace("min", "");
                        secs = int.Parse(split[1]) * 60;
                    }
                    else
                    {
                        split = split[1].Split(':');
                        secs = int.Parse(split[0]) * 60 + int.Parse(split[1]);
                    }

                    //Get rest time
                    string rest = a.Workout.Split('/')[1];

                    if (rest.Contains("secs"))
                    {
                        RestTime = int.Parse(rest.Replace("secs", ""));
                    }
                    else if (rest.Contains("mins"))
                    {
                        RestTime = int.Parse(rest.Replace("mins", "")) * 60;
                    }
                    else if (rest.Contains("min"))
                    {
                        RestTime = int.Parse(rest.Replace("min", "")) * 60;
                    }
                    else if (rest.Contains(":"))
                    {
                        string[] t = rest.Split(':');
                        RestTime = int.Parse(t[0]) * 60 + int.Parse(t[1]);
                    }


                    int totSecs = rds * secs;
                    split = a.FormatedResult.Split('|');
                    cals = int.Parse(split[0].Replace("reps", ""));
                    if (a.Workout.Contains("Row"))
                    {
                        Activity = a;
                        Type = ErgType.Row;
                        WorkTime = totSecs;
                        Calories = cals;
                        calPerMin = cals / ((double)totSecs / 60);
                        RndTime = secs;
                        Rounds = (int)Math.Round((double)WorkTime / RndTime);
                        ParsedErg = true;

                    }
                    if (a.Workout.Contains("Echo") || a.Workout.Contains("Assault"))
                    {
                        Activity = a;
                        Type = ErgType.Bike;
                        WorkTime = totSecs;
                        Calories = cals;
                        calPerMin = cals / ((double)totSecs / 60);
                        RndTime = secs;
                        Rounds = (int)Math.Round((double)WorkTime / RndTime);
                        ParsedErg = true;
                    }
                }
            }
            if (a.Workout.StartsWith("AMReps"))
            {
                if (a.Workout.Contains(": Row"))
                {
                    int cals = int.Parse(a.FormatedResult.Replace("reps", ""));
                    int totsecs = a.WorkTime / 1000;
                    Activity = a;
                    Type = ErgType.Row;
                    WorkTime = totsecs;
                    Calories = cals;
                    calPerMin = cals / ((double)totsecs / 60);
                    RndTime = totsecs;
                    Rounds = (int)Math.Round((double)WorkTime / RndTime);
                    RestTime = 0;
                    ParsedErg = true;
                }
                if (a.Workout.Contains(": Echo Bike Calories") || a.Workout.Contains(": Assault Bike Calories"))
                {
                    int cals = int.Parse(a.FormatedResult.Replace("reps", ""));
                    int totsecs = a.WorkTime / 1000;
                    Activity = a;
                    Type = ErgType.Bike;
                    WorkTime = totsecs;
                    Calories = cals;
                    calPerMin = cals / ((double)totsecs / 60);
                    RndTime = totsecs;
                    Rounds = (int)Math.Round((double)WorkTime / RndTime);
                    RestTime = 0;
                    ParsedErg = true;
                }
            }
            if (a.Workout.StartsWith("Row (calories)s :") && a.Workout.Contains("Max Rep"))
            {
                string[] split = a.Workout.Split(':');
                split = split[1].Replace(" ", "").Split('x');
                int rds = int.Parse(split[0]);
                int cals = int.Parse(a.Result);
                Activity = a;
                Type = ErgType.Row;
                Regex regex = new Regex("[0-9]+:[0-9]+.[0-9]+");
                Match m = regex.Match(a.Notes);
                if (m.Success)
                {
                    split = m.Value.Split(':');
                    if (split.Length != 2)
                        ParsedErg = false;
                    int mins = int.Parse(split[0]);
                    double secs = double.Parse(split[1]);
                    double totsecs = (60*(double)mins + secs)*rds;
                    WorkTime = (int)Math.Round(totsecs);
                    Calories = cals;
                    calPerMin = cals / ((double)WorkTime / 60);
                    RndTime = WorkTime;
                    Rounds = (int)Math.Round((double)WorkTime / RndTime);
                    ParsedErg = true;
                    Activity = a;
                }
            }

            if (a.Workout.StartsWith("Row : "))
            {
                Type = ErgType.Row;
                string wo = a.Workout.Substring(6);
                string[] split = wo.Split('x');
                int num = int.Parse(split[0]);

                if (a.Workout.Contains("rest"))
                {
                    int start = a.Workout.IndexOf("rest");
                    string rest = a.Workout.Substring(start + 4);
                    if (rest.Contains("secs"))
                    {
                        RestTime = int.Parse(rest.Replace("secs", ""));
                    }
                    else if (rest.Contains("mins"))
                    {
                        RestTime = int.Parse(rest.Replace("mins", "")) * 60;
                    }
                    else if (rest.Contains("min"))
                    {
                        RestTime = int.Parse(rest.Replace("min", "")) * 60;
                    }
                    else if (rest.Contains(":"))
                    {
                        string[] t = rest.Split(':');
                        RestTime = int.Parse(t[0]) * 60 + int.Parse(t[1]);
                    }
                }
                else if (a.Description.Contains("rest"))
                {
                    int start = a.Description.IndexOf("rest");
                    string rest = a.Description.Substring(start + 4);
                    rest = rest.Split('\n')[0];
                    if (rest.Contains("secs"))
                    {
                        RestTime = int.Parse(rest.Replace("secs", ""));
                    }
                    else if (rest.Contains("mins"))
                    {
                        RestTime = int.Parse(rest.Replace("mins", "")) * 60;
                    }
                    else if (rest.Contains("min"))
                    {
                        RestTime = int.Parse(rest.Replace("min", "")) * 60;
                    }
                    else if (rest.Contains(":"))
                    {
                        string[] t = rest.Split(':');
                        RestTime = int.Parse(t[0]) * 60 + int.Parse(t[1]);
                    }
                }

                if (split[1].Contains("m"))
                {
                    split = split[1].Split('m');
                    int meters = int.Parse(split[0]);
                    Activity = a;
                    WorkTime = a.WorkTime / 1000;
                    Calories = a.WorkPerformed / 639;
                    calPerMin = Calories / ((double)WorkTime / 60);
                    RndTime = WorkTime/num;
                    Rounds = (int)Math.Round((double)WorkTime / RndTime);
                    ParsedErg = true;
                }
            }
        }
        public Erg() { }
        public string Serialize()
        {
            string t = "";
            if (this == null)
            {
                return string.Empty;
            }
            try
            {
                t += WO.Replace(",",";") + ",";
                t += Type.ToString() + ",";
                t += WorkTime.ToString() + ",";
                t += Calories.ToString() + ",";
                t += calPerMin.ToString() + ",";
                t += RndTime.ToString() + ",";
                t += RestTime.ToString() + ",";
                t += Rounds.ToString();
                return t;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred", ex);
            }
        }
        public string GetDesc()
        {
            string t = "";
            t += "workout" + ",";
            t += "Type" + ",";
            t += "WorkTime" + ",";
            t += "Calories" + ",";
            t += "calPerMin" + ",";
            t += "RndTime" + ",";
            t += "RestTime" + ",";
            t += "Rounds";
            return t;
        }
    }

    public enum ErgType
    { Bike,Row};
}
