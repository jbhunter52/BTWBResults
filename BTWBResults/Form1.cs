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

namespace BTWBResults
{
    public partial class Form1 : Form
    {
        private List<Activity> Data;
        private int previousDays = 7;
        private HistogramType CurrentChart;
        public Chart chart;
        private Font Font;
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
            CurrentChart = HistogramType.Volume;

            Font = new Font("Microsoft Sans Serif", 12.0f, FontStyle.Bold);
            
        }
        private void SetChartFont()
        {
            ChartArea ca = this.chart.ChartAreas[0];
            ca.AxisX.TitleFont = Font;
            ca.AxisY.TitleFont = Font;
            this.chart.Series[0].Font = Font;
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

            string[] files = Directory.GetFiles(filePath);
            Data = new List<Activity>();
            foreach (string file in files)
            {
                string filename = Path.GetFullPath(file);
                Data.AddRange(ParseFile(filename));
            }
            Debug.WriteLine("Number of workouts, " + Data.Count.ToString());
            return true;
        }

        private List<Activity> ParseFile(string file)
        {
            List<Activity> workouts = new List<Activity>();
            TextReader sr = File.OpenText(file);
            var parser = new CsvParser(sr);
            string[] row = parser.Read();
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
            SetChartFont();
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

        void tbMonths_Leave(object sender, EventArgs e)
        {
            UpdatePowerChartData();
        }

        void tb_ValueChanged(object sender, EventArgs e)
        {
            //TrackBar tb = (TrackBar)sender;
            //TextBox tbMonths = (TextBox)splitContainer2.Panel1.Controls[2];

            //double months = Double.Parse(tbMonths.Text);
            //TimeSpan span = new TimeSpan((int)(months * 30), 0, 0, 0, 0);

            //DateTime minDate = GetMinDate();
            //DateTime maxDate = GetMaxDate();
            //int totDays = GetTotDays();



            //DateTime endTime = minDate.AddDays(tb.Value);
            //DateTime startTime = endTime.Subtract(span);

            UpdatePowerChartData();
        }

        void cbRunning_CheckedChanged(object sender, EventArgs e)
        {
            UpdatePowerChartData();
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
        
    }
}
