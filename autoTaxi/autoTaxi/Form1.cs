﻿using System;
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
        public List<Request> requests;
        public List<Car> cars;
        public Func<List<Car>, Request, bool> Assign;
        public TextBox textBox1;
        public double gridWidth;

        public int netOverIdealTime;

        public Form1() {
            InitializeComponent();
        }

        public async Task simulation(int delay, bool draw) { //delay in ms
            int updateFrequency = 5; //seconds per update
            int time = 0;
            for(int req = 0; req < requests.Count; time += updateFrequency) {
                if(req < requests.Count) { //if more requests to process
                    Request r = requests[req];
                    if (time >= r.time) { //if time for next request
                        if(draw) { 
                            drawObject(r.start, r.passengers, CreateGraphics(), Color.Red, r, gridWidth);
                            drawObject(r.end, r.passengers, CreateGraphics(), Color.Green, r, gridWidth);
                        }
                        if(!Assign(cars, requests[req++])) { //try to assign the car
                            req--; //If all cars are full don't move to next request
                        }
                    }
                }
                await Task.Delay(delay);

                if(Assign == Dispatcher.greedyAssign) {
                    Program.greedyUpdate(updateFrequency, cars, time);
                }
                else {
                    Program.update(updateFrequency, cars, time);
                }
                if(draw) {
                    drawSystem(cars, gridWidth);
                }
                    updateDistance(cars);
            }

            int passengers = 0;
            int max = 0;
            foreach(Car c in cars) {
                if(c.Passengers > max) {
                    max = c.Passengers;
                }
                passengers += c.Passengers;
            }
            Console.Write("avg: " + (double)passengers / cars.Count + ", max: " + max + ", ");

            //finish delivering remaining passengers
            foreach(Car c in cars) {
                while(c.Passengers > 0) {
                    if(Assign == Dispatcher.greedyAssign) {
                        Program.greedyUpdate(updateFrequency, cars, time);
                    }
                    else {
                        Program.update(updateFrequency, cars, time);
                    }
                    if(draw) {
                        drawSystem(cars, gridWidth);
                    }
                    updateDistance(cars);
                    time += updateFrequency;
                    await Task.Delay(delay);
                }
            }

            //Console.WriteLine("All passengers delivered.");
            //int i = 0;
            int totalDelivered = 0;
            foreach(Car c in cars) {
                //Console.WriteLine("Car {0} passenger stats", i++);
                //Console.WriteLine("\ttotal time\tideal time\tdelta time");
                totalDelivered += c.delivered.Count;
                foreach (DeliveredPassenger d in c.delivered) {
                    //Console.WriteLine("\t{0, -10}\t{1, -10}\t{2, -10}", d.totalRideTime, d.idealRideTime, d.totalRideTime - d.idealRideTime);
                    netOverIdealTime += d.totalRideTime - d.idealRideTime;
                }
                //Console.WriteLine("Average delta time (error): {0}", sum / c.delivered.Count);
                //Console.WriteLine();
            }
            netOverIdealTime /= 60; //convert to minutes
            netOverIdealTime /= totalDelivered;

        }

        /// <summary>
        /// Adjust total milage driven box
        /// </summary>
        public void updateDistance(List<Car> cars) {
            double distance = 0;
            foreach(Car c in cars) {
                distance += c.totalMiles;
            }
            textBox1.Text = String.Format("Mi: {0:F}", (distance / 5280));
        }

        public void drawSystem(List<Car> cars, double gridWidth) {
            Color[] color = { Color.Blue, Color.Red, Color.Green, Color.Indigo, Color.Gold, Color.Fuchsia };

            Graphics graphics = CreateGraphics();
            foreach(Car c in cars) {
                drawObject(c.pos, c.Passengers, graphics, color[c.Id % color.Length], c, gridWidth);
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
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(12, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(1880, 33);
            this.button1.TabIndex = 0;
            this.button1.Text = "Start";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox1.Location = new System.Drawing.Point(-1, 1012);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(159, 31);
            this.textBox1.TabIndex = 1;
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // Form1
            // 
            this.ClientSize = new System.Drawing.Size(1904, 1041);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.button1);
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.ShowIcon = false;
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.Form1_Load_1);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void Form1_Load_1(object sender, EventArgs e) {
        }

        private void button1_Click(object sender, EventArgs e) {
            button1.Visible = false;
            Task.Run(async () => await simulation(0, true));
        }

        private void textBox1_TextChanged(object sender, EventArgs e) {

        }
    }
}
