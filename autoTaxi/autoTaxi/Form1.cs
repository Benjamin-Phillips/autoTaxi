using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace autoTaxi {
    class Form1 : Form {
        private Button button1;

        public Form1() {
            InitializeComponent();
        }

        public async void greedyVisualization(int delay) { //delay in ms
            int vehicles = 5;
            int frequency = 5 * 60; //60 seconds / request
            int simTime = 7200; //3600 seconds = 1 hour
            double medianDist = 36960; //7 miles in feet
            double stdDev = 8800; //1.66667 miles in feet
            double gridWidth = 2 * (medianDist + stdDev * 3); //width of the area

            List<Request> requests = Request.generateRequests(frequency, simTime, medianDist, stdDev, gridWidth);
            List<Car> cars = Program.generateCars(vehicles, gridWidth);
            cars[0].pos = new Position(960 * 24, 540 * 42);

            int updateFrequency = 1; //seconds per update
            for(int time = 0, req = 0; time < simTime * 2; ) {
                Request r = requests[req];
                if(time >= r.time) { //time for next request
                    Pen requestColor = Pens.Red;
                    drawObject(r.start, r.passengers, CreateGraphics(), requestColor, r, gridWidth);
                    Dispatcher.greedy(cars, requests[req++]);
                }
                Program.update(updateFrequency, cars);
                drawSystem(cars, gridWidth);
                time += updateFrequency;
            }
        }

        private static void TimerEventProcessor(Object myObject, EventArgs myEventArgs) {
        }

        public void drawSystem(List<Car> cars, double gridWidth) {
            Pen[] pen = { Pens.Blue, Pens.Red, Pens.Green, Pens.Indigo, Pens.Gold, Pens.Fuchsia };

            Graphics graphics = CreateGraphics();
            foreach(Car c in cars) {
                drawObject(c.pos, c.Passengers, graphics, pen[c.Id], c, gridWidth);
            }
        }

        public void drawObject(Position p, int passengers, Graphics graphics, Pen color, Object item, double gridWidth) {
            Rectangle rectangle = new Rectangle(
                (int)(p.x / (gridWidth/this.Width)), (int)(p.y / (gridWidth/this.Height)), 10, 10);
           
            if (item.GetType() == typeof(Car)) {
                graphics.DrawEllipse(color, rectangle);
            }
            else if(item.GetType() == typeof(Request)) {
                System.Drawing.SolidBrush myBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Red);
                graphics.FillRectangle(myBrush, rectangle);
                myBrush.Dispose();
            }
        }

        private void InitializeComponent() {
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(1880, 33);
            this.button1.TabIndex = 0;
            this.button1.Text = "Start";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Form1
            // 
            this.ClientSize = new System.Drawing.Size(1904, 1041);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load_1);
            this.ResumeLayout(false);

        }

        private void Form1_Load_1(object sender, EventArgs e) {
        }

        private void button1_Click(object sender, EventArgs e) {
            button1.Visible = false;
            greedyVisualization(1);
        }
    }
}
