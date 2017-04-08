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

        public async Task greedyVisualization(Func<List<Car>, Request, bool> Assign, int delay) { //delay in ms
            int vehicles = 5;
            int frequency = 5 * 60; //60 seconds / request
            int simTime = 2 * 3600; //3600 seconds = 1 hour
            double medianDist = 36960; //7 miles in feet
            double stdDev = 8800; //1.66667 miles in feet
            double gridWidth = 2 * (medianDist + stdDev * 3); //width of the area

            List<Request> requests = Request.generateRequests(frequency, simTime, medianDist, stdDev, gridWidth);
            List<Car> cars = Program.generateCars(vehicles, gridWidth);

            int updateFrequency = 4; //seconds per update
            for(int time = 0, req = 0; req < requests.Count; time += updateFrequency) {
                if(req < requests.Count) { //if more requests to process
                    Request r = requests[req];
                    if (time >= r.time) { //if time for next request
                        Console.WriteLine("Time for request {0}/{1}", req + 1, requests.Count);
                        drawObject(r.start, r.passengers, CreateGraphics(), Color.Red, r, gridWidth);
                        drawObject(r.end, r.passengers, CreateGraphics(), Color.Green, r, gridWidth);
                        if(!Assign(cars, requests[req++])) {
                            req--; //If all cars are full don't move to next request
                        }
                    }
                }
                await Task.Delay(delay);

                Program.update(updateFrequency, cars);
                drawSystem(cars, gridWidth);
            }
            
            //finish delivering remaining passengers
            foreach(Car c in cars) {
                while(c.Passengers > 0) {
                    Program.update(updateFrequency, cars);
                    drawSystem(cars, gridWidth);
                    await Task.Delay(delay);
                }
            }
            Console.WriteLine("All passengers delivered.");
        }

        public void drawSystem(List<Car> cars, double gridWidth) {
            Color[] color = { Color.Blue, Color.Red, Color.Green, Color.Indigo, Color.Gold, Color.Fuchsia };

            Graphics graphics = CreateGraphics();
            foreach(Car c in cars) {
                drawObject(c.pos, c.Passengers, graphics, color[c.Id], c, gridWidth);
            }
        }

        public void drawObject(Position p, int passengers, Graphics graphics, Color color, object item, double gridWidth) {
            Rectangle rectangle = new Rectangle(
                (int)(p.x / (gridWidth / (Width - 25))), (int)(p.y / (gridWidth / (Height - 50))), 8, 8);
           
            if (item.GetType() == typeof(Car)) {
                graphics.FillEllipse(new SolidBrush(color), rectangle);
            }
            else if(item.GetType() == typeof(Request)) {
                SolidBrush myBrush = new SolidBrush(color);
                rectangle.Width = 15;
                rectangle.Height = 15;
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
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.ShowIcon = false;
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.Form1_Load_1);
            this.ResumeLayout(false);

        }

        private void Form1_Load_1(object sender, EventArgs e) {
        }

        private void button1_Click(object sender, EventArgs e) {
            button1.Visible = false;
            Task.Run(async () => await greedyVisualization(Dispatcher.closestPathAssign, 1));
        }
    }
}
