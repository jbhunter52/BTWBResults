using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using CsvHelper;
using System.Windows.Forms.DataVisualization.Charting;
using MathNet.Numerics;
using System.Text.RegularExpressions;

namespace BTWBResults
{
    public partial class Form1 : Form
    {
        private List<Activity> Data;
        private int previousDays = 7;
        private HistogramType CurrentChart;
        public Chart chart;
        private Font Font;
        private List<Erg> ErgList;
        private HRdata HRData;
        public Form1()
        {
            InitializeComponent();
            chart = new Chart();
            chart.GetToolTipText += chart_GetToolTipText;
            chart.Dock = DockStyle.Fill;
            GetData(false);
            treeView1.Nodes.Add("1", "Volume Chart");
            treeView1.Nodes.Add("2", "Power Chart");
            treeView1.Nodes.Add("3", "Active Days Chart");
            treeView1.Nodes.Add("4", "Sessions Chart");
            treeView1.Nodes.Add("5", "Erg Chart");
            treeView1.Nodes.Add("6", "Heart Rate");
            CurrentChart = HistogramType.Volume;

            Font = new Font("Microsoft Sans Serif", 12.0f, FontStyle.Bold);
            
        }
        private void SetChartFont()
        {
            try
            {
                ChartArea ca = this.chart.ChartAreas[0];
                ca.AxisX.TitleFont = Font;
                ca.AxisY.TitleFont = Font;
                this.chart.Series[0].Font = Font;
            }
            catch
            {

            }
        }
        private void importDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool result = GetData(true);
            if (result)
            {
                if (Data.Count != null)
                {
                    //MessageBox.Show("Received data");
                    TreeNodeMouseClickEventArgs arg = new TreeNodeMouseClickEventArgs(treeView1.Nodes[0], MouseButtons.Left, 1, 1, 1);
                    treeView1_NodeMouseClick(null, arg);
                }
            }

        }
        private void SetLoginInfo()
        {
            Login login = new Login();
            login.ShowDialog();
        }
        private bool GetData(bool newData)
        {
                        
            string appPath = Application.StartupPath;
            string filePath = Path.Combine(appPath, "Data");
            bool needNewData = !File.Exists(Path.Combine(filePath, "BTWB_data.csv"));
            if (newData || needNewData)
            {
                if (Settings1.Default.username == "" || Settings1.Default.pass == "")
                    SetLoginInfo();

                if (!Directory.Exists(filePath))
                    Directory.CreateDirectory(filePath);


                DownloadData dd = new DownloadData(filePath);
                bool result = dd.Login(Settings1.Default.username, Settings1.Default.pass);
                if (result == false)
                    return false;
            }

            string[] files = Directory.GetFiles(filePath, "*.csv");
            Data = new List<Activity>();
            foreach (string file in files)
            {
                string filename = Path.GetFullPath(file);
                Data.AddRange(ParseFile(filename));
            }

            //Fix work performed for Echo Bike
            //Fix volume for "Echo Bike Calories"
            //foreach (Activity wo in Data)
            //if (wo.Workout.Contains("Echo Bike Calories") && wo.WorkPerformed == 0)
            //{
            //    if (wo.Workout.Contains("AMReps") || wo.Workout.Contains("Tabata"))
            //    {
            //        wo.WorkPerformed = int.Parse(wo.Result) * 691;
            //    }
            //    if (wo.Workout.Contains("FT"))
            //    {
            //        string[] split = wo.Description.Split(' ');
            //        int cals = int.Parse(split[0]);
            //        wo.WorkPerformed = cals * 691;
            //    }
            //}

            Debug.WriteLine("Number of workouts, " + Data.Count.ToString());
            return true;
        }

        private List<Activity> ParseFile(string file)
        {
            List<Activity> workouts = new List<Activity>();

            //Get First Line
            StreamReader streamReader = new StreamReader(file);
            string firstLine = streamReader.ReadLine();
            streamReader.Close();


            TextReader sr = File.OpenText(file);
            var parser = new CsvParser(sr);
            string[] row = parser.Read();
            List<string[]> newLines = new List<string[]>();
            while (true)
            {
                row = parser.Read();
                newLines.Add(row);
                if (row == null)
                { break; }
            }
            sr.Close();

            File.Delete(file);
            StreamWriter sw = new StreamWriter(file, false);
            sw.WriteLine(firstLine);
            sw.Flush();
            using (var csv = new CsvWriter(sw))
            {
                foreach (string[] line in newLines)
                {
                    if (line != null)
                    {
                        string Workout = line[1];
                        string Description = line[9];
                        if ((Workout.Contains("Echo Bike Calories") || Description.Contains("Echo Bike Calories")) && line[5].Equals("0"))
                        {
                            string Result = line[2];
                            string WorkPerformed = line[5];
                            
                            if (Workout.Contains("AMReps") || Workout.Contains("Tabata"))
                            {
                                WorkPerformed = (int.Parse(Result) * 639).ToString();
                            }
                            else if (Workout.Contains("RFT"))
                            {
                                string[] split = Description.Split('\n');
                                Regex regex = new Regex("[0-9]+");
                                int rnds = 0;
                                int cals = 0;
                                foreach (string s in split)
                                {
                                    if (s.Contains("Echo"))
                                    {
                                        Match m = regex.Match(s);
                                        if (m.Success)
                                        {
                                            cals = int.Parse(m.Value);
                                            
                                        }
                                    }
                                    if (s.Contains("rounds"))
                                    {
                                        Match m = regex.Match(s);
                                        if (m.Success)
                                        {
                                            rnds = int.Parse(m.Value);

                                        }
                                    }
                                }
                                if (cals > 0 && rnds > 0)
                                    WorkPerformed = (rnds * cals * 639).ToString();
                            }
                            else if (Workout.Contains("FT"))
                            {
                                string[] split = Description.Split(' ');
                                int cals = int.Parse(split[0]);
                                WorkPerformed = (cals * 639).ToString();
                            }

                            line[5] = WorkPerformed;
                        }

                        for (int i = 0; i < 10; i++)
                        {
                            string s = line[i];
                            csv.WriteField(s);
                        }
                        Activity act = new Activity(line);
                        Erg erg = new Erg(act);
                        csv.WriteField(erg.ParsedErg.ToString());
                        
                        csv.NextRecord();
                    }
                }
            }
            sw.Close();


            sr = File.OpenText(file);
            parser = new CsvParser(sr);
            row = parser.Read();
            while (true)
            {
                row = parser.Read();
                if (row == null)
                { break; }
                Activity wo = new Activity(row);
                workouts.Add(wo);
            }
            sr.Close();



            return workouts;
        }


        private void UpdateControls()
        {
            TreeNode selectedNode = treeView1.SelectedNode;

            if (selectedNode.Text.Equals("Volume Chart"))
            {
                UpdateHistogramChartDisplay();
                UpdateHistogramChartData(HistogramType.Volume);
                //UpdateVolumeChartData();
                CurrentChart = HistogramType.Volume;
            }
            else if (selectedNode.Text.Equals("Power Chart"))
            {
                UpdatePowerChartDisplay();
                UpdatePowerChartData();
                
            }
            else if (selectedNode.Text.Equals("Active Days Chart"))
            {
                UpdateHistogramChartDisplay();
                UpdateHistogramChartData(HistogramType.ActiveDays);
                //UpdateActiveDaysChartData();
                CurrentChart = HistogramType.ActiveDays;
            }
            else if (selectedNode.Text.Equals("Sessions Chart"))
            {
                UpdateHistogramChartDisplay();
                UpdateHistogramChartData(HistogramType.Sessions);
                //UpdateActiveDaysChartData();
                CurrentChart = HistogramType.Sessions;
            }
            else if (selectedNode.Text.Equals("Erg Chart"))
            {
                UpdateErgChartDisplay();
                UpdateErgChartData();
            }
            else if (selectedNode.Text.Equals("Heart Rate"))
            {
                UpdateHRChartDisplay();
                UpdateHRChartData();
            }
            SetChartFont();
        }

        private void UpdateHRChartDisplay()
        {
            splitContainer2.Panel1.Controls.Clear();
            TreeView tv = new TreeView();
            tv.NodeMouseClick += HRNodeClick;
            tv.Dock = DockStyle.Fill;
            Chart chart1 = new Chart();
            Chart chart2 = new Chart();
            chart1.Dock = DockStyle.Fill;
            chart2.Dock = DockStyle.Fill;
            chart1.Name = "chart1";
            chart2.Name = "chart2";

            splitContainer2.Orientation = Orientation.Vertical;
            splitContainer2.Panel1.Controls.Add(tv);

            SplitContainer chartsplit = new SplitContainer();
            chartsplit.Name = "chartsplit";
            chartsplit.Panel2MinSize = 25;
            chartsplit.Orientation = Orientation.Horizontal;
            chartsplit.Panel1.Controls.Add(chart1);
            chartsplit.Panel2.Controls.Add(chart2);
            chartsplit.Dock = DockStyle.Fill;
            splitContainer2.Panel2.Controls.Add(chartsplit);
        }

        private void HRNodeClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            SplitContainer chartsplit = (SplitContainer)splitContainer2.Panel2.Controls["chartsplit"];
            Chart chart1 = (Chart)chartsplit.Panel1.Controls["chart1"];

            Chart chart2 = (Chart)chartsplit.Panel2.Controls["chart2"];

            chart1.ChartAreas.Clear();
            chart2.ChartAreas.Clear();

            ChartArea area = new ChartArea("0");
            area.AxisX.Minimum = 0;
            area.AxisX.Interval = 3;
            area.AxisX.IntervalType = DateTimeIntervalType.Number;
            area.AxisX.LabelStyle.Format = "#.##";
            chart1.ChartAreas.Add("0");
            chart2.ChartAreas.Add("1");

            TreeView tv = (TreeView)splitContainer2.Panel1.Controls[0];

            if (ModifierKeys.HasFlag(Keys.Control))
            {
                TreeNode selected = tv.SelectedNode;
                selected.BackColor = Color.Aqua;
            }
            else
            {
                chart1.Series.Clear();
                chart2.Series.Clear();
                foreach (TreeNode tn in tv.Nodes)
                {
                    tn.BackColor = Color.White;
                }
            }
            TreeNode node = e.Node;

            Series s = new Series();
            s.ChartType = SeriesChartType.Line;
            s.BorderWidth = 3;
            HRWorkout hr = (HRWorkout)node.Tag;

            List<double> xarr = new List<double>();
            List<double> yarr = new List<double>();
            foreach (var samp in hr.exercises[0].samples.heartRate)
            {

                double x = (samp.dateTime - hr.startTime).TotalSeconds / 60;
                int y = samp.value;
                xarr.Add(x);
                yarr.Add(y);
            }

            for (int i = 0; i < xarr.Count; i++)
            {
                DataPoint dp = new DataPoint();
                dp.SetValueXY(xarr[i], yarr[i]);
                dp.ToolTip = xarr[i].ToString("#.##") + " mins" + "\n" + yarr[i].ToString() + " bpm" + "\n" + node.Text;
                s.Points.Add(dp);
            }

            RemoveZeros(yarr);

            #region old
            //double sampRate = (double)xarr.Count / (xarr[xarr.Count - 1] * 60);

            //MathNet.Filtering.Median.OnlineMedianFilter filter = 
            //    new MathNet.Filtering.Median.OnlineMedianFilter(17);

            //double[] filtered = filter.ProcessSamples(yarr.ToArray());

            //Series filteredHR = new Series();
            //filteredHR.ChartType = SeriesChartType.Line;
            //filteredHR.BorderWidth = 3;
            //filteredHR.Color = Color.Orange;

            //for (int i = 0; i < xarr.Count; i++)
            //{
            //    DataPoint dp = new DataPoint(xarr[i], filtered[i]);
            //    filteredHR.Points.Add(dp);
            //}
            //chart1.Series.Add(filteredHR);
            //chart2.Series.Add(SeriesDerivative(filteredHR, 100));
            #endregion

            MathNet.Numerics.Interpolation.IInterpolation interp = MathNet.Numerics.Interpolate.CubicSplineRobust(xarr, yarr);
            Series splineHR = new Series();
            splineHR.ChartType = SeriesChartType.Line;
            splineHR.BorderWidth = 3;

            for (int i = 0; i < xarr.Count; i++)
            {
                DataPoint dp = new DataPoint(xarr[i], interp.Interpolate(xarr[i]));
                splineHR.Points.Add(dp);
            }

            Series sdiff = new Series();
            sdiff.ChartType = SeriesChartType.Line;
            sdiff.BorderWidth = 3;

            for (int i = 0; i < xarr.Count; i++)
            {
                DataPoint dp = new DataPoint(xarr[i], interp.Differentiate(xarr[i]));
                sdiff.Points.Add(dp);
            }

            chart1.Series.Add(splineHR);
            chart2.Series.Add(sdiff);

        }

        
        public void RemoveZeros(List<double> yarr)
        {
            List<int> ind = new List<int>();

            for (int i = 1; i < yarr.Count-1; i++)
            {
                double y = yarr[i];

                if (y == 0)
                {
                    ind.Add(i);
                }
                else if (ind.Count > 0)
                {
                    double initial = yarr[ind[0] - 1];
                    double final = yarr[ind[ind.Count - 1] + 1];

                    double diff = final - initial;
                    double dy = diff / (ind.Count + 2);

                    for (int j = 0; j < ind.Count; j++)
                    {
                        yarr[ind[j]] = initial + dy * (j + 1);
                    }
                    ind.Clear();
                }

                
            }
        }
        public Series SeriesDerivative(Series s, int hrCutoff)
        {
            Series d = new Series();
            d.ChartType = s.ChartType;
            d.BorderWidth = s.BorderWidth;

            double dx = s.Points[1].XValue - s.Points[0].XValue;
            double dy = s.Points[1].YValues[0] - s.Points[0].YValues[0];
            DataPoint first = new DataPoint(s.Points[0].XValue, dy / dx);
            d.Points.Add(first);

            for (int i = 1; i < s.Points.Count-1;i++)
            {
                dx = s.Points[i+1].XValue - s.Points[i-1].XValue;
                dy = s.Points[i+1].YValues[0] - s.Points[i-1].YValues[0];
                DataPoint dp = new DataPoint(s.Points[i].XValue, dy / dx);
                d.Points.Add(dp);
            }

            int cnt = s.Points.Count;
            dx = s.Points[cnt-1].XValue - s.Points[cnt-2].XValue;
            dy = s.Points[cnt-1].YValues[0] - s.Points[cnt-2].YValues[0];
            DataPoint last = new DataPoint(s.Points[cnt-1].XValue, dy / dx);
            d.Points.Add(last);

            //cutoff
            for (int i = 0; i < s.Points.Count; i++)
            {
                if (s.Points[i].YValues[0] < hrCutoff)
                {
                    d.Points[i].YValues[0] = 0;
                }
            }

            return d;
        }

        private void UpdateHRChartData()
        {
            TreeView tv = (TreeView)splitContainer2.Panel1.Controls[0];

            HRData = new HRdata(Application.StartupPath);
            UpdateErgList();

            foreach (var series in HRData.HRWorkouts)
            {
                TreeNode n = new TreeNode();
                n.Text = series.startTime.ToShortDateString();
                n.Tag = series;

                foreach (Erg e in ErgList)
                {
                    List<Erg> matches = new List<Erg>();
                    if (e.Activity.Date.Date == series.startTime.Date)
                    {
                        int ergTime = e.WorkTime + e.RestTime;
                        int hrTime = series.GetTotalSecs();

                        if (hrTime > ergTime)
                        {
                            n.Text += ", " + e.Activity.Workout;
                            e.Activity.Date = new DateTime();
                            break;
                        }
                    }  
                }

                tv.Nodes.Add(n);
            }
        }
        

        private void chart_MouseWheel(object sender, MouseEventArgs e)
        {
            var chart = (Chart)sender;
            var xAxis = chart.ChartAreas[0].AxisX;
            var yAxis = chart.ChartAreas[0].AxisY;

            try
            {
                if (IsControlDown() == false)
                {
                    if (e.Delta < 0) // Scrolled down.
                    {
                        xAxis.ScaleView.ZoomReset();
                    }
                    else if (e.Delta > 0) // Scrolled up.
                    {
                        var xMin = xAxis.ScaleView.ViewMinimum;
                        var xMax = xAxis.ScaleView.ViewMaximum;

                        var posXStart = xAxis.PixelPositionToValue(e.Location.X) - (xMax - xMin) / 4;
                        var posXFinish = xAxis.PixelPositionToValue(e.Location.X) + (xMax - xMin) / 4;

                        xAxis.ScaleView.Zoom(posXStart, posXFinish);
                        //chart.Series[0]["PixelPointWidth"] = ((int)(chart.Width/(posXFinish - posXStart))).ToString();
                    }
                }
                else
                {
                    if (e.Delta < 0) // Scrolled down.
                    {
                        yAxis.ScaleView.ZoomReset();
                    }
                    else if (e.Delta > 0) // Scrolled up.
                    {
                        //var yMin = yAxis.ScaleView.ViewMinimum;
                        var yMax = yAxis.ScaleView.ViewMaximum;

                        //var posYStart = yAxis.PixelPositionToValue(e.Location.Y) - (yMax - yMin) / 4;
                        var posYFinish = yAxis.PixelPositionToValue(e.Location.Y) + (yMax - 0) / 4;

                        yAxis.ScaleView.Zoom(0, posYFinish);
                    }
                }
            }
            catch { }
        }
        private static bool IsControlDown()
        {
            return (System.Windows.Forms.Control.ModifierKeys & Keys.Control) == Keys.Control;
        }
        private void UpdateHistogramChartDisplay()
        {
            splitContainer2.Panel1.Controls.Clear();
            Label label1 = new Label();
            label1.Text = "Days";
            label1.Location = new Point(10, 10);
            label1.Size = new System.Drawing.Size(35, 12);

            TextBox textBoxDays = new TextBox();
            textBoxDays.Text = previousDays.ToString();
            textBoxDays.Location = new Point(50, 10);
            textBoxDays.Size = new System.Drawing.Size(35, 12);
            textBoxDays.Leave += textBoxBins_Leave;

            splitContainer2.Panel1.Controls.Add(label1);
            splitContainer2.Panel1.Controls.Add(textBoxDays); 
        }

        #region Old
        //private void UpdateVolumeChartData()
        //{
        //    splitContainer2.Panel2.Controls.Clear();
        //    splitContainer2.Panel2.Controls.Add(chart);

        //    chart.ChartAreas.Clear();
        //    chart.Series.Clear();
        //    ChartArea area = new ChartArea("1");
        //    area.AxisX.LabelStyle.Format = "MM/dd/yyyy";
        //    area.AxisX.LabelStyle.Angle = -90;
        //    area.AxisX.IntervalType = DateTimeIntervalType.Months;
        //    chart.ChartAreas.Add(area);



        //    TextBox textBoxBins = (TextBox)splitContainer2.Panel1.Controls[1];
        //    int binCount = Int32.Parse(textBoxBins.Text);
        //    DateTime minDate = GetMinDate();
        //    DateTime maxDate = GetMaxDate();
        //    int totDays = GetTotDays();

        //    int binDiff = totDays / binCount;

        //    int[] bins = new int[binCount];
        //    for (int i = 0; i < bins.Length; i++)
        //    {
        //        bins[i] = 0;
        //    }
        //    foreach (Activity a in Data)
        //    {
        //        int currentDay = a.Date.Subtract(minDate).Days;
        //        if (currentDay == 0)
        //            bins[0] = bins[0] + 1;
        //        else
        //        {
        //            double i = ((double)currentDay / totDays) * binCount;
        //            int bin = (int)Math.Ceiling(i) - 1;
        //            bins[bin] = bins[bin] + 1;
        //        }
        //    }

        //    Series ser = new Series();
        //    ser.ChartType = SeriesChartType.Column;
        //    ser.ChartArea = "1";
        //    ser.XValueType = ChartValueType.DateTime;
        //    for (int i = 0; i < bins.Length; i++)
        //    {
        //        DateTime center = minDate.AddDays((binDiff * (i)) + binDiff / 2);
        //        DataPoint pt = new DataPoint();
        //        pt.SetValueXY(center, bins[i]);
        //        pt.ToolTip = minDate.AddDays((binDiff * (i))).ToString() + "\n" + minDate.AddDays((binDiff * (i)) + binDiff).ToString();
        //        ser.Points.Add(pt);
        //    }

        //    //int width = (int)chart.ChartAreas[0].Position.Width;
        //    int pixelWidth = 1;// (int)Math.Floor((double)width / binCount);
        //    ser["PointWidth"] = pixelWidth.ToString();
        //    ser.BorderColor = Color.Black;
        //    ser.BorderWidth = 1;
        //    chart.Series.Add(ser);



            
        //}
        //private void UpdateActiveDaysChartData()
        //{
        //    splitContainer2.Panel2.Controls.Clear();
        //    splitContainer2.Panel2.Controls.Add(chart);

        //    chart.ChartAreas.Clear();
        //    chart.Series.Clear();
        //    ChartArea area = new ChartArea("1");
        //    area.AxisX.LabelStyle.Format = "MM/dd/yyyy";
        //    area.AxisX.LabelStyle.Angle = -90;
        //    area.AxisX.IntervalType = DateTimeIntervalType.Months;
        //    chart.ChartAreas.Add(area);



        //    TextBox textBoxBins = (TextBox)splitContainer2.Panel1.Controls[1];

        //    DateTime minDate = GetMinDate();
        //    DateTime maxDate = GetMaxDate();
        //    int totDays = GetTotDays();

        //    int binDiff = 7;
        //    int binCount = (int)Math.Ceiling((double)totDays / binDiff);

        //    int[] bins = new int[binCount];
        //    for (int i = 0; i < bins.Length; i++)
        //    {
        //        bins[i] = 0;
        //    }
        //    List<DateTime> dates = new List<DateTime>();
        //    foreach (Activity a in Data)
        //    {
        //        if (!dates.Contains(a.Date))
        //        {
        //            dates.Add(a.Date);
        //            int currentDay = a.Date.Subtract(minDate).Days;
        //            if (currentDay == 0)
        //                bins[0] = bins[0] + 1;
        //            else
        //            {
        //                int i = (int)Math.Floor((double)currentDay / binDiff);
        //                bins[i] = bins[i] + 1;
        //            }
        //        }
        //    }

        //    Series ser = new Series();
        //    ser.ChartType = SeriesChartType.Column;
        //    ser.ChartArea = "1";
        //    ser.XValueType = ChartValueType.DateTime;
        //    for (int i = 0; i < bins.Length; i++)
        //    {
        //        DateTime center = minDate.AddDays((binDiff * (i)) + binDiff / 2);
        //        DataPoint pt = new DataPoint();
        //        pt.SetValueXY(center, bins[i]);
        //        pt.ToolTip = minDate.AddDays((binDiff * (i))).ToString() + "\n" + minDate.AddDays((binDiff * (i)) + binDiff).ToString();
        //        ser.Points.Add(pt);
        //    }

        //    //int width = (int)chart.ChartAreas[0].Position.Width;
        //    int pixelWidth = 1;// (int)Math.Floor((double)width / binCount);
        //    ser["PointWidth"] = pixelWidth.ToString();
        //    ser.BorderColor = Color.Black;
        //    ser.BorderWidth = 1;
        //    chart.Series.Add(ser);




        //}
        #endregion

        private void UpdateHistogramChartData(HistogramType histogramType)
        {
            splitContainer2.Panel2.Controls.Clear();
            splitContainer2.Panel2.Controls.Add(chart);

            chart.ChartAreas.Clear();
            chart.Series.Clear();

            ChartArea area = new ChartArea("1");
            area.AxisX.LabelStyle.Format = "MM/dd/yyyy";
            area.AxisX.LabelStyle.Angle = -90;
            area.AxisX.IntervalType = DateTimeIntervalType.Months;
            chart.ChartAreas.Add(area);

            TextBox textBoxDays = (TextBox)splitContainer2.Panel1.Controls[1];
            int binDays = Int16.Parse(textBoxDays.Text);


            DateTime minDate = GetMinDate();
            DateTime maxDate = DateTime.Now;
            int totDays = maxDate.Subtract(minDate).Days;

            int binCount = (int)Math.Ceiling((double)totDays / binDays);
            int remDays;
            Math.DivRem(totDays, binDays, out remDays);

            int[] bins = new int[binCount+1];
            for (int i = 0; i < bins.Length; i++)
            {
                bins[i] = 0;
            }
            List<DateTime> workoutDays = new List<DateTime>();
            Data = Data.OrderBy(a => a.Date).ToList();
            foreach (Activity a in Data)
            {
                int currentDay = a.Date.Subtract(minDate).Days;
                int bin;
                if (currentDay < remDays)
                {
                    bin = 0;
                }
                else
                {
                    bin = (int)Math.Floor((currentDay - remDays-.001) / (double)binDays)+1;
                    if (bin == bins.Length)
                    {
                        bin = bin - 1;
                    }
                }
                

                if (histogramType == HistogramType.ActiveDays)
                {
                    if (!workoutDays.Contains(a.Date))
                        bins[bin] = bins[bin] + 1;
                    workoutDays.Add(a.Date);
                }
                if (histogramType == HistogramType.Sessions)
                {
                    bins[bin] = bins[bin] + 1;
                }
                if (histogramType == HistogramType.Volume)
                {
                    bins[bin] = bins[bin] + a.WorkPerformed;
                }
            }

            Series ser = new Series();
            ser.ChartType = SeriesChartType.Column;
            ser.ChartArea = "1";
            ser.XValueType = ChartValueType.DateTime;
            for (int i = 0; i < bins.Length; i++)
            {
                DateTime center;
                double start;
                double end;
                if (i == 0)
                {
                    start = 0;
                    center = minDate.AddDays(remDays / 2);
                    end = remDays;
                }
                else
                {
                    start = (i-1) * binDays + remDays;
                    center = minDate.AddDays(start + (double)binDays / 2);
                    end = start + binDays;
                }

                DataPoint pt = new DataPoint();
                pt.SetValueXY(center, bins[i]);
                pt.ToolTip = GetDate(minDate.AddDays(start)) + "\n" + GetDate(minDate.AddDays(end)) + "\n" + String.Format("{0:n0}",bins[i]);
                ser.Points.Add(pt);
            }

            //int width = (int)chart.ChartAreas[0].Position.Width;
            int pixelWidth = 1;// (int)Math.Floor((double)width / binCount);
            ser["PointWidth"] = pixelWidth.ToString();
            ser.BorderColor = Color.Black;
            ser.BorderWidth = 1;
            chart.Series.Add(ser);

            if (histogramType == HistogramType.ActiveDays)
            {
                chart.ChartAreas[0].AxisY.Title = "Active Days";
            }
            if (histogramType == HistogramType.Sessions)
            {
                chart.ChartAreas[0].AxisY.Title = "Total Sessions";
            }
            if (histogramType == HistogramType.Volume)
            {
                chart.ChartAreas[0].AxisY.Title = "Total Volume (ft-lbs)";
            }

            chart.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
            chart.MouseWheel += chart_MouseWheel;

        }

        void chart_GetToolTipText(object sender, ToolTipEventArgs e)
        {
            // Check selevted chart element and set tooltip text
            if (e.HitTestResult.ChartElementType == ChartElementType.DataPoint)
            {
                int i = e.HitTestResult.PointIndex;
                DataPoint dp = e.HitTestResult.Series.Points[i];
                e.Text = dp.ToolTip;
            }
        }

        void textBoxBins_Leave(object sender, EventArgs e)
        {
            UpdateHistogramChartData(CurrentChart);
            TextBox tb = (TextBox)splitContainer2.Panel1.Controls[1];
            previousDays = Int16.Parse(tb.Text);
        }


        private void UpdatePowerChartDisplay()
        {
            splitContainer2.Panel1.Controls.Clear();

            CheckBox cbRunning = new CheckBox();
            cbRunning.Text = "Remove Running";
            cbRunning.Location = new Point(10, 10);
            cbRunning.Size = new System.Drawing.Size(140, 17);
            cbRunning.CheckAlign = ContentAlignment.MiddleLeft;
            cbRunning.Checked = false;
            cbRunning.CheckedChanged += cbRunning_CheckedChanged;

            Label label1 = new Label();
            label1.Text = "Months";
            label1.Size = new System.Drawing.Size(50, 12);
            label1.Location = new Point(10, 50);

            TextBox tbMonths = new TextBox();
            tbMonths.Text = "3";
            tbMonths.Size = new System.Drawing.Size(60, 12);
            tbMonths.Location = new Point(70, 50);
            tbMonths.Leave += tbMonths_Leave;

            TrackBar tb = new TrackBar();
            tb.Size = new System.Drawing.Size(600, 50);
            tb.Location = new Point(250, 50);
            tb.Minimum = 0;
            tb.Maximum = GetTotDays();
            tb.Value = tb.Maximum;
            tb.ValueChanged += tb_ValueChanged;

            splitContainer2.Panel1.Controls.Add(cbRunning);
            splitContainer2.Panel1.Controls.Add(label1);
            splitContainer2.Panel1.Controls.Add(tbMonths);
            splitContainer2.Panel1.Controls.Add(tb);


        }
        private void UpdateErgChartDisplay()
        {
            splitContainer2.Panel1.Controls.Clear();

            CheckBox cbRow = new CheckBox();
            cbRow.Text = "Include Row";
            cbRow.Name = "cbRow";
            cbRow.Location = new Point(10, 10);
            cbRow.Size = new System.Drawing.Size(140, 17);
            cbRow.CheckAlign = ContentAlignment.MiddleLeft;
            cbRow.Checked = false;
            cbRow.CheckedChanged += cbRow_CheckedChanged;

            Label label1 = new Label();
            label1.Text = "Months";
            label1.Size = new System.Drawing.Size(50, 12);
            label1.Location = new Point(10, 50);

            Button button3d = new Button();
            button3d.Text = "3D";
            button3d.Name = "button3d";
            button3d.UseVisualStyleBackColor = true;
            button3d.Size = new System.Drawing.Size(75, 23);
            button3d.Location = new Point(150, 10);
            button3d.Click += button3d_Click;

            TextBox tbMonths = new TextBox();
            tbMonths.Text = "3";
            tbMonths.Size = new System.Drawing.Size(60, 12);
            tbMonths.Location = new Point(70, 70);
            tbMonths.Leave += tbMonthsErg_Leave;

            TrackBar tb = new TrackBar();
            tb.Size = new System.Drawing.Size(600, 50);
            tb.Location = new Point(250, 50);
            tb.Minimum = 0;
            tb.Maximum = GetTotDays();
            tb.Value = tb.Maximum;
            tb.ValueChanged += tbErg_ValueChanged;

            CheckBox cbBike = new CheckBox();
            cbBike.Text = "Include Bike";
            cbBike.Name = "cbBike";
            cbBike.Location = new Point(10, 30);
            cbBike.Size = new System.Drawing.Size(140, 17);
            cbBike.CheckAlign = ContentAlignment.MiddleLeft;
            cbBike.Checked = false;
            cbBike.CheckedChanged += cbBike_CheckedChanged;

            splitContainer2.Panel1.Controls.Add(cbRow);
            splitContainer2.Panel1.Controls.Add(label1);
            splitContainer2.Panel1.Controls.Add(tbMonths);
            splitContainer2.Panel1.Controls.Add(tb);
            splitContainer2.Panel1.Controls.Add(cbBike);
            splitContainer2.Panel1.Controls.Add(button3d);

        }

        void button3d_Click(object sender, EventArgs e)
        {
            Chart3D chart3d = new Chart3D();
            chart3d.ErgList = ErgList;
            CheckBox cbRow = (CheckBox)this.Controls.Find("cbRow", true)[0];
            CheckBox cbBike = (CheckBox)this.Controls.Find("cbBike", true)[0];
            chart3d.Row = cbRow.Checked;
            chart3d.Bike = cbBike.Checked;
            chart3d.Show();


        }
        void tbMonths_Leave(object sender, EventArgs e)
        {
            UpdatePowerChartData();
        }

        void tb_ValueChanged(object sender, EventArgs e)
        {
            UpdatePowerChartData();
        }
        void tbMonthsErg_Leave(object sender, EventArgs e)
        {
            UpdateErgChartData();
        }

        void tbErg_ValueChanged(object sender, EventArgs e)
        {
            UpdateErgChartData();
        }

        void cbRunning_CheckedChanged(object sender, EventArgs e)
        {
            UpdatePowerChartData();
        }
        void cbRow_CheckedChanged(object sender, EventArgs e)
        {
            UpdateErgChartData();
        }
        void cbBike_CheckedChanged(object sender, EventArgs e)
        {
            UpdateErgChartData();
        }
        private void UpdatePowerChartData()
        {
            splitContainer2.Panel2.Controls.Clear();
            splitContainer2.Panel2.Controls.Add(chart);

            chart.ChartAreas.Clear();
            chart.Series.Clear();
            ChartArea area = new ChartArea("1");
            area.AxisX.Interval = 5;
            area.AxisX.IntervalType = DateTimeIntervalType.Number;
            area.AxisX.LabelStyle.Format = "{0.0}";
            chart.ChartAreas.Add(area);
            double maxPower = 0;
            double maxDuration = 0;

            foreach (Activity a in Data)
            {
                if (a.WorkPerformed > 0 && a.WorkTime > 0)
                {
                    int duration = a.WorkTime / 1000; //time in secs
                    double mins = duration / 60.0f;
                    int power = a.WorkPerformed / duration;
                    if (power > maxPower)
                        maxPower = power;
                    if (mins > maxDuration)
                        maxDuration = mins;
                }
            }
            area.AxisY.Minimum = 0;
            area.AxisY.Maximum = maxPower;
            area.AxisX.Minimum = 0;
            area.AxisX.Maximum = maxDuration;
            area.AxisY.Title = "ft-lbs/sec";

            CheckBox cbRunning = (CheckBox)splitContainer2.Panel1.Controls[0];
            TextBox tbMonths = (TextBox)splitContainer2.Panel1.Controls[2];
            TrackBar tb = (TrackBar)splitContainer2.Panel1.Controls[3];

            Series ser = new Series();
            
            ser.ChartType = SeriesChartType.Point;

            double months = Double.Parse(tbMonths.Text);
            TimeSpan span = new TimeSpan((int)(months * 30), 0, 0, 0, 0);

            DateTime minDate = GetMinDate();
            DateTime maxDate = GetMaxDate();
            int totDays = GetTotDays();

            DateTime endTime = minDate.AddDays(tb.Value);
            DateTime startTime = endTime.Subtract(span);
            List<double> x = new List<double>();
            List<double> y = new List<double>();
            foreach (Activity a in Data)
            {
                if (a.WorkPerformed > 0 && a.WorkTime > 0)
                {
                    bool include = true;
                    if (cbRunning.Checked)
                        if (a.IncludesRunning())
                            include = false;

                    if ((a.Date < startTime) | (a.Date > endTime))
                        include = false;

                    if (include)
                    {
                        int duration = a.WorkTime / 1000; //time in secs
                        double mins = duration / 60.0f;
                        int power = a.WorkPerformed / duration;

                        DataPoint dp = new DataPoint();
                        dp.ToolTip = a.Date.ToString("MM/dd/yyyy") + '\n' + a.Description;
                        dp.MarkerSize = 6;
                        dp.SetValueXY(mins, power);
                        ser.Points.Add(dp);
                        x.Add(mins);
                        y.Add(power);
                    }
                }
            }

            if (x.Count > 3)
            {
                //Func<double, double> f = Fit.LineFunc(x.ToArray(), y.ToArray());
                //Func<double, double> f = Fit.LinearCombinationFunc(x.ToArray(), y.ToArray(), x => 1.0, x => Math.Log(x));
                Func<double, double> f = Fit.LinearCombinationFunc(x.ToArray(), y.ToArray(), dx => 1.0, dx => Math.Log(dx));


                int num = 100;
                Series serFit = new Series();
                serFit.ChartType = SeriesChartType.Line;
                double minV = x.Min();
                double maxV = x.Max();
                double d = maxV - minV;
                for (int i = 0; i < num; i++)
                {
                    double t = ((i) * (d / 100))+minV;
                    serFit.Points.AddXY(t, f(t));
                    //Debug.WriteLine(f(t).ToString());
                }

                chart.Series.Add(serFit);
            }
            chart.Series.Add(ser);
            chart.ChartAreas[0].AxisX.Title = "Work Duration";
            chart.ChartAreas[0].AxisY.Title = "Total work (ft-lbs/sec)";
        }
        private void ErgSerialization(List<Erg> ergdata, bool show)
        {
            string ergListFile = Path.Combine(Environment.CurrentDirectory, "erg.csv");
            StreamWriter sw = new StreamWriter(ergListFile, false);

            Erg E = new Erg();
            sw.WriteLine(E.GetDesc());

            foreach (Erg e in ergdata)
            {
                sw.WriteLine(e.Serialize());
            }
            sw.Close();

            if (show)
            {
                Process.Start(ergListFile);
            }
        }
        private void UpdateErgList()
        {
            ErgList = new List<Erg>();

            foreach (Activity a in Data)
            {
                if (a.WorkPerformed > 0)
                {
                    Erg m = new Erg(a);
                    if (m.ParsedErg)
                    {
                        ErgList.Add(m);
                        a.Erg = true;
                    }
                }
            }
        }
        private void UpdateErgChartData()
        {
            splitContainer2.Panel2.Controls.Clear();
            splitContainer2.Panel2.Controls.Add(chart);

            chart.ChartAreas.Clear();
            chart.Series.Clear();
            ChartArea area = new ChartArea("1");
            //area.AxisX.Interval = 5;
            //area.AxisX.IntervalType = DateTimeIntervalType.Number;
            //area.AxisX.LabelStyle.Format = "{0.0}";
            chart.ChartAreas.Add(area);


            UpdateErgList();

            CheckBox cbC2 = (CheckBox)splitContainer2.Panel1.Controls[0];
            CheckBox cbEcho = (CheckBox)splitContainer2.Panel1.Controls[4];
            TextBox tbMonths = (TextBox)splitContainer2.Panel1.Controls[2];
            TrackBar tb = (TrackBar)splitContainer2.Panel1.Controls[3];

            Series ser = new Series();

            ser.ChartType = SeriesChartType.Point;

            double months = Double.Parse(tbMonths.Text);
            TimeSpan span = new TimeSpan((int)(months * 30), 0, 0, 0, 0);

            DateTime minDate = GetMinDate();
            DateTime maxDate = GetMaxDate();
            int totDays = GetTotDays();

            DateTime endTime = minDate.AddDays(tb.Value);
            DateTime startTime = endTime.Subtract(span);
            List<double> x = new List<double>();
            List<double> y = new List<double>();
            foreach (Erg m in ErgList)
            {
                bool include = false;
                Activity a = m.Activity;
                if (cbC2.Checked)
                    if (m.Type == ErgType.Row)
                        include = true;
                if (cbEcho.Checked)
                    if (m.Type == ErgType.Bike)
                        include = true;

                if ((a.Date < startTime) | (a.Date > endTime.AddDays(1)))
                    include = false;

                if (include)
                {
                    double mins = (double)m.RndTime / 60;
                    DataPoint dp = new DataPoint();
                    dp.ToolTip = mins.ToString("0.##") + " mins," + m.calPerMin.ToString("0.##") + " cal/min\n" + a.Date.ToString("MM/dd/yyyy") + '\n' + a.Workout + '\n' + a.Description + "\n\n" + a.Notes;
                    dp.MarkerSize = 6;

                    if (m.Type == ErgType.Row)
                        dp.Color = Color.Red;
                    if (m.Type == ErgType.Bike)
                        dp.Color = Color.Black;

                    dp.SetValueXY(mins, m.calPerMin);
                    ser.Points.Add(dp);
                    x.Add(mins);
                    y.Add(m.calPerMin);
                }
            }

            
            if (x.Count > 0)
            {
                area.AxisY.Minimum = ((int)(y.Min() / 5)) * 5;
                area.AxisY.Maximum = ((int)(y.Max() / 5) + 1) * 5;
                area.AxisX.Minimum = 0;
                area.AxisX.Maximum = ((int)(x.Max() / 5) + 1) * 5;
            }
            area.AxisY.Title = "cal/min";

            area.AxisX.Interval = 5;
            area.AxisX.MajorGrid.Enabled = true;
            area.AxisX.MinorGrid.Enabled = true;
            area.AxisX.MinorGrid.LineDashStyle = ChartDashStyle.Dash;
            area.AxisX.MinorGrid.LineWidth = 1;
            area.AxisX.MinorGrid.LineColor = Color.Gray;
            area.AxisX.MajorGrid.Interval = 5;
            area.AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Solid;
            area.AxisX.MajorGrid.LineWidth = 2;
            area.AxisX.MajorGrid.LineColor = Color.Black;

            area.AxisY.Interval = 5;
            area.AxisY.MajorGrid.Enabled = true;
            area.AxisY.MinorGrid.Enabled = true;
            area.AxisY.MinorGrid.LineDashStyle = ChartDashStyle.Dash;
            area.AxisY.MinorGrid.LineWidth = 1;
            area.AxisY.MinorGrid.LineColor = Color.Gray;
            area.AxisY.MajorGrid.Interval = 5;
            area.AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Solid;
            area.AxisY.MajorGrid.LineWidth = 2;
            area.AxisY.MajorGrid.LineColor = Color.Black;

            if (x.Count > 3)
            {
                //Func<double, double> f = Fit.LineFunc(x.ToArray(), y.ToArray());
                //Func<double, double> f = Fit.LinearCombinationFunc(x.ToArray(), y.ToArray(), x => 1.0, x => Math.Log(x));
                Func<double, double> f = Fit.LinearCombinationFunc(x.ToArray(), y.ToArray(), dx => 1.0, dx => Math.Log(dx));


                int num = 100;
                Series serFit = new Series();
                serFit.ChartType = SeriesChartType.Line;
                double minV = x.Min();
                double maxV = x.Max();
                double d = maxV - minV;
                for (int i = 0; i < num; i++)
                {
                    double t = ((i) * (d / 100)) + minV;
                    serFit.Points.AddXY(t, f(t));
                    //Debug.WriteLine(f(t).ToString());
                }

                chart.Series.Add(serFit);
            }
            chart.Series.Add(ser);
            chart.ChartAreas[0].AxisX.Title = "Work Duration";
            chart.ChartAreas[0].AxisY.Title = "Cal/min";
        }
        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            treeView1.SelectedNode = e.Node;

            if (Data != null)
                UpdateControls();
        }

        private DateTime GetMinDate()
        {
            DateTime minDate = DateTime.MaxValue;

            foreach (Activity a in Data)
            {
                if (minDate > a.Date)
                    minDate = a.Date;
            }
            return minDate;
        }
        private DateTime GetMaxDate()
        {
            DateTime maxDate = DateTime.MinValue;

            foreach (Activity a in Data)
            {
                if (maxDate < a.Date)
                    maxDate = a.Date;
            }
            return maxDate;
        }
        private string GetDate(DateTime date)
        {
            return date.ToString("MM/dd/yyyy");
        }
        private int GetTotDays()
        {
            int totDays = GetMaxDate().Subtract(GetMinDate()).Days;
            return totDays;
        }

        private enum HistogramType
        {
            Volume,
            ActiveDays,
            Sessions
        };

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void openDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string appPath = Application.StartupPath;
            string filePath = Path.Combine(appPath, "Data");
            string dataFile = Path.Combine(filePath, "BTWB_data.csv");
            System.Diagnostics.Process.Start(dataFile);

        }

        private void changeLoginToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetLoginInfo();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            About about = new About();
            about.ShowDialog();
        }

        private void ErgListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ErgSerialization(ErgList, true);
        }
    }
}
