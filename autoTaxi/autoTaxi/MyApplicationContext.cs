using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace autoTaxi {
    class MyApplicationContext : ApplicationContext {
        private void onFormClosed(object sender, EventArgs e) {
            if(Application.OpenForms.Count == 0) {
                Console.WriteLine("exiting");
                ExitThread();
            }
        }

        // source: http://stackoverflow.com/a/13406508
        public MyApplicationContext() {
            //If WinForms exposed a global event that fires whenever a new Form is created,
            //we could use that event to register for the form's `FormClosed` event.
            //Without such a global event, we have to register each Form when it is created
            //This means that any forms created outside of the ApplicationContext will not prevent the 
            //application close.

            int vehicles = 5;
            int frequency = 5 * 60; //x * 60 seconds / request
            int simTime = 2 * 3600; //3600 seconds = 1 hour
            double medianDist = 36960; //7 miles in feet
            double stdDev = 8800; //1.66667 miles in feet
            double gridWidth = 2 * (medianDist + stdDev * 3); //width of the area
            int numTest = 5;

            double[,] miles = new double[3, numTest];
            double[,] times = new double[3, numTest];
            List<double> data;
            for(int i = 0; i < numTest; i++) {
                data = compareMethods(vehicles, frequency, simTime, medianDist, stdDev, gridWidth);
                Console.WriteLine("Finished test " + (i + 1));
                for(int j = 0; j < data.Count; j += 2) {
                    miles[j / 2, i] = data[j];
                    times[j / 2, i] = data[j + 1];
                }
            }

            var csv = new StringBuilder();
            csv.Append("Greedy M,Greedy T,Line M,Line T,Perm M,Perm T\n");
            for(int i = 0; i < numTest; i++) {
                for(int j = 0; j < 3; j++) {
                    csv.Append(miles[j, i] + "," + times[j, i] + ",");
                }
                csv.Append("\r\n");
                Console.WriteLine();
            }
            File.WriteAllText("data.csv", csv.ToString());

            ExitForm exitForm = new ExitForm();
            exitForm.FormClosed += onFormClosed;
            exitForm.Show();
        }

        private List<double> compareMethods(int vehicles, int frequency, int simTime, double medianDist, double stdDev, double gridWidth) {
            List<Request> requests = Request.generateRequests(frequency, simTime, medianDist, stdDev, gridWidth);
            List<Car> cars = Program.generateCars(vehicles, gridWidth);

            Form1 greedyForm = configureForm(requests, cars, Dispatcher.greedyAssign, gridWidth, "Greedy Algorithm");
            Form1 closestPathForm = configureForm(requests, cars, Dispatcher.closestPathAssign, gridWidth, "Closest Path Algorithm");
            Form1 permutationForm = configureForm(requests, cars, Dispatcher.permutationAssign, gridWidth, "Permutation Algorithm");

            var forms = new List<Form>() {
                greedyForm, closestPathForm, permutationForm
            };

            List<double> data = new List<double>(6);
            foreach(var form in forms) {
                //form.FormClosed += onFormClosed;
                ((Form1)form).simulation(0, false); //ignore warning
                string miles = ((Form1)form).textBox1.Text;
                miles = miles.Remove(0, 4);
                data.Add(double.Parse(miles));
                data.Add(((Form1)form).netOverIdealTime);
                //Task.Run(async () => await ((Form1)form).simulation(0, false));
                //form.Show();
            }

            return data;
        }

        private Form1 configureForm(List<Request> requests, List<Car> cars, Func<List<Car>, Request, bool> dispatcher, double gridWidth, string method) {
            Form1 newForm = new Form1();

            newForm.requests = requests;
            newForm.cars = new List<Car>();
            foreach(Car c in cars) {
                newForm.cars.Add(new Car(c));
            }
            newForm.Assign = dispatcher;
            newForm.gridWidth = gridWidth;
            newForm.Text = method;

            return newForm;
        }
    }
}
