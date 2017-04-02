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
            double gridWidth = 45420.274; //width of the area ~8.6 mi in feet

            List<Request> requests = Request.generateRequests(frequency, simTime, medianDist, stdDev, gridWidth);
            List<Car> cars = Program.generateCars(vehicles, gridWidth);
            cars[0].pos = new Position(960 * 24, 540 * 42);

            int updateFrequency = 1; //seconds per update
            for(int time = 0, req = 0; time < simTime; time++) {
                Request r = requests[req];
                if(time == r.time) { //time for next request
                    Dispatcher.greedy(cars, requests[req++]);
                }
                Program.update(updateFrequency, cars);
                drawSystem(cars);
            }
        }

        private static void TimerEventProcessor(Object myObject, EventArgs myEventArgs) {
        }

        public void drawSystem(List<Car> cars) {
            Graphics graphics = CreateGraphics();
            foreach(Car c in cars) {
                if(c.Id == 0) {
                    drawObject(c.pos, c.Passengers, graphics, Pens.Blue);
                } else {
                    drawObject(c.pos, c.Passengers, graphics, Pens.Red);
                }
            }
        }

        public void drawObject(Position p, int passengers, Graphics graphics, Pen color) {
            Rectangle rectangle = new Rectangle(
               (int)(p.x / 24), (int)(p.y / 42), 10, 10);
            graphics.DrawRectangle(color, rectangle);
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
