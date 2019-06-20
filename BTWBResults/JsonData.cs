using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTWBResults
{
    public class PhysicalInformationSnapshot
    {
        public DateTime dateTime { get; set; }
        public string sex { get; set; }
        public string birthday { get; set; }
        public double height { get; set; }
    public double weight { get; set; }
    }

    public class HeartRateMinMax
{
    public int min { get; set; }
    public int avg { get; set; }
    public int max { get; set; }
}

public class FitFat
{
    public int lowerLimit { get; set; }
    public int higherLimit { get; set; }
    public string inZone { get; set; }
    public int zoneIndex { get; set; }
}

public class HeartRateZone
{
    public int lowerLimit { get; set; }
    public int higherLimit { get; set; }
    public string inZone { get; set; }
    public int zoneIndex { get; set; }
}

public class Zones
{
    public IList<FitFat> fit_fat { get; set; }
    public IList<HeartRate> heart_rate { get; set; }
}

public class HeartRate
{
    public DateTime dateTime { get; set; }
    public int value { get; set; }
}

public class Samples
{
    public IList<HeartRate> heartRate { get; set; }
}

public class Exercis
{
    public DateTime startTime { get; set; }
    public DateTime stopTime { get; set; }
    public int timezoneOffset { get; set; }
    public string duration { get; set; }
    public double distance { get; set; }
    public string sport { get; set; }
    public int kiloCalories { get; set; }
    public HeartRate heartRate { get; set; }
    public Zones zones { get; set; }
    public Samples samples { get; set; }
}

    public class HRWorkout
    {
        public string exportVersion { get; set; }
        public string name { get; set; }
        public DateTime startTime { get; set; }
        public DateTime stopTime { get; set; }
        public int timeZoneOffset { get; set; }
        public double distance { get; set; }
        public string duration { get; set; }
        public int maximumHeartRate { get; set; }
        public int averageHeartRate { get; set; }
        public int kiloCalories { get; set; }
        public PhysicalInformationSnapshot physicalInformationSnapshot { get; set; }
        public IList<Exercis> exercises { get; set; }

        public int GetTotalSecs()
        {
            return (int)((stopTime - startTime).TotalSeconds);
        }
    }
}
