using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
//using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using nzy3D.Chart;
using nzy3D.Chart.Controllers.Thread.Camera;
using nzy3D.Colors;
using nzy3D.Colors.ColorMaps;
using nzy3D.Maths;
using nzy3D.Plot3D.Builder;
using nzy3D.Plot3D.Builder.Concrete;
using nzy3D.Plot3D.Primitives;
using nzy3D.Plot3D.Primitives.Axes.Layout;
using nzy3D.Plot3D.Rendering.Canvas;
using nzy3D.Plot3D.Rendering.View;
using nzy3D.Colors;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;

namespace BTWBResults
{
    public partial class Chart3D : Form
    {
        public List<Erg> ErgList;
        public bool Row = false;
        public bool Bike = false;
        private CameraThreadController t;
        List<int> workTimeArr;
        List<double> wrArr;
        List<double> calminArr;
        List<Coord3d> coor;
        double[] coeffs;
        double MaxWorkRestRatio = 10;

        public Chart3D()
        {
            InitializeComponent();
        }

        private void Chart3D_Load(object sender, EventArgs e)
        {
            
            if (Bike == false && Row == false)
                return;
            workTimeArr = new List<int>();
            wrArr = new List<double>();
            calminArr = new List<double>();
            coor = new List<Coord3d>();
            FilterToArrays();
            Optimize();

            InitRenderer();
 
        }
        private void InitRenderer()
        {

            // Create the Renderer 3D control.
            //Renderer3D myRenderer3D = new Renderer3D();

            // Add the Renderer control to the panel
            // mainPanel.Controls.Clear();
            //mainPanel.Controls.Add(myRenderer3D);

            // Create a range for the graph generation
            Range xRange = new Range(workTimeArr.Min(), workTimeArr.Max());
            Range yRange = new Range(wrArr.Min(), wrArr.Max());
            int steps = 50;

            // Build a nice surface to display with cool alpha colors 
            // (alpha 0.8 for surface color and 0.5 for wireframe)
            MyMapper mapper = new MyMapper();
            mapper.coeff = coeffs;
            Shape surface = Builder.buildOrthonomal(new OrthonormalGrid(xRange, steps, yRange, steps), mapper);
            surface.ColorMapper = new ColorMapper(new ColorMapRainbow(), surface.Bounds.zmin, surface.Bounds.zmax, new nzy3D.Colors.Color(1, 1, 1, 0.8));
            surface.FaceDisplayed = true;
            surface.WireframeDisplayed = true;
            surface.WireframeColor = Color.CYAN;
            surface.WireframeColor.mul(new Color(1, 1, 1, 0.5));

            // Create the chart and embed the surface within
            Chart chart = new Chart(myRenderer3D, Quality.Nicest);
            chart.Scene.Graph.Add(surface);



            Scatter scatter = new Scatter(coor.ToArray(), Color.RED, 10);
            chart.Scene.Graph.Add(scatter);


            IAxeLayout axeLayout = chart.AxeLayout;
            axeLayout.TickLineDisplayed = true;
            axeLayout.XTickLabelDisplayed = true;
            axeLayout.YTickLabelDisplayed = true;
            axeLayout.ZTickLabelDisplayed = true;
            axeLayout.XAxeLabelDisplayed = true;
            axeLayout.YAxeLabelDisplayed = true;
            axeLayout.ZAxeLabelDisplayed = true;
            axeLayout.XAxeLabel = "Work time (secs)";
            axeLayout.YAxeLabel = "Work rest ratio";
            axeLayout.ZAxeLabel = "Cal per min";

            axeLayout = chart.AxeLayout;

            // Create a mouse control
            nzy3D.Chart.Controllers.Mouse.Camera.CameraMouseController mouse = new nzy3D.Chart.Controllers.Mouse.Camera.CameraMouseController();
            mouse.addControllerEventListener(myRenderer3D);
            chart.addController(mouse);

            // This is just to ensure code is reentrant (used when code is not called in Form_Load but another reentrant event)
            DisposeBackgroundThread();

            // Create a thread to control the camera based on mouse movements
            t = new nzy3D.Chart.Controllers.Thread.Camera.CameraThreadController();
            t.addControllerEventListener(myRenderer3D);
            mouse.addSlaveThreadController(t);
            chart.addController(t);
            t.Start();

            // Associate the chart with current control
            myRenderer3D.setView(chart.View);

            this.Refresh();
        }
        private void DisposeBackgroundThread()
        {
            if ((t != null))
            {
                t.Dispose();
            }
        }
        private void Optimize()
        {
            int cnt = workTimeArr.Count;
            Matrix<double> s = Matrix<double>.Build.Dense(cnt,3);
            Matrix<double> s2 = Matrix<double>.Build.Dense(cnt, 3);

            Vector<double> srhs = Vector<double>.Build.Dense(cnt);

            for (int i = 0; i < cnt; i++)
            {
                s.At(i, 0, Math.Log(workTimeArr[i]));
                s.At(i, 1, Math.Log(wrArr[i]));
                s.At(i, 2, 1);

                srhs.At(i, calminArr[i]);
            }

            Vector<double> p = s.Solve(srhs);
            coeffs = p.ToArray();
        }
        private void FilterToArrays()
        {
            foreach (Erg erg in ErgList)
            {
                if (erg.Type == ErgType.Row)
                {
                    if (Row == false)
                        continue;
                }
                if (erg.Type == ErgType.Bike)
                {
                    if (Bike == false)
                        continue;
                }

                double wr = MaxWorkRestRatio;
                int worktime = erg.RndTime * erg.Rounds;
               
                if (erg.RestTime > 0)
                {
                    wr = (double)erg.RndTime / (double)erg.RestTime;
                }

                if (wr != 0)
                {
                    workTimeArr.Add(worktime);
                    wrArr.Add(wr);
                    calminArr.Add(erg.calPerMin);
                    double z = erg.calPerMin;
                    //double z = mapper.f(worktime, wr);
                    Coord3d c = new Coord3d(worktime, wr, z);
                    coor.Add(c);
                }
            }
        }
        private double FEval(Vector<double> v, double x, double y)
        {
            double a = v[0];
            double b = v[1];
            double c = v[2];                        
            return a*Math.Log(x) + b*Math.Log(y) + c;
        }

        int Rounds;
        double RestTime;
        double RoundTime;

        private void textBoxRounds_Leave(object sender, EventArgs e)
        {
            int i = ParseInt(sender);
            if (i > 0)
                Rounds = i;
            Predict();
        }

        private void textBoxRoundTime_Leave(object sender, EventArgs e)
        {
            double i = ParseDouble(sender);
            if (i > 0)
                RoundTime = i;

            Predict();
        }

        private void textBoxRestTime_Leave(object sender, EventArgs e)
        {
            double i = ParseDouble(sender);
            if (i > 0)
                RestTime = i;

            Predict();
        }
        private void Predict()
        {
            if (Rounds > 0 && RoundTime > 0 && RestTime > 0)
            {
                MyMapper mapper = new MyMapper();
                mapper.coeff = coeffs;

                double wr = MaxWorkRestRatio;
                double workTime = Rounds * RoundTime;
                if (Rounds > 1)
                {
                    wr = RoundTime / RestTime;
                }
                double calmin = mapper.f(workTime, wr);
                double calhr = calmin * 60;
                textBoxCalMin.Text = calmin.ToString("#.##");
                textBoxCalHr.Text = calhr.ToString("#");
            }
        }
        private double ParseDouble(object sender)
        {
            TextBox tb = (TextBox)sender;
            int result;
            string s;

            if (tb.Text.Contains("*"))
            {
                string[] split = tb.Text.Split('*');

                return double.Parse(split[0]) * double.Parse(split[1]);
            }



            if (int.TryParse(tb.Text, out result))
            {
                if (result > 0)
                    return result;
                else
                {
                    MessageBox.Show("Enter an integer greater than 0");
                    tb.Text = "";
                    return 0;
                }
            }
            else
            {
                MessageBox.Show("Enter an integer");
                tb.Text = "";
                return 0;
            }
        }
        private int ParseInt(object sender)
        {
            TextBox tb = (TextBox)sender;
            int result;
            string s;

            if (int.TryParse(tb.Text, out result))
            {
                if (result > 0)
                    return result;
                else
                {
                    MessageBox.Show("Enter an integer greater than 0");
                    tb.Text = "";
                    return 0;
                }
            }
            else
            {
                MessageBox.Show("Enter an integer");
                tb.Text = "";
                return 0;
            }
        }

    }
}
