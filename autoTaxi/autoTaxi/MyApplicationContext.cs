using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace autoTaxi {
    class MyApplicationContext : ApplicationContext {
        private void onFormClosed(object sender, EventArgs e) {
            if(Application.OpenForms.Count == 0) {
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

            int vehicles = 3;
            int frequency = 5 * 60; //x * 60 seconds / request
            int simTime = 2 * 3600; //3600 seconds = 1 hour
            double medianDist = 36960; //7 miles in feet
            double stdDev = 8800; //1.66667 miles in feet
            double gridWidth = 2 * (medianDist + stdDev * 3); //width of the area
            List<Request> requests = Request.generateRequests(frequency, simTime, medianDist, stdDev, gridWidth);
            List<Car> cars = Program.generateCars(vehicles, gridWidth);

            Form1 greedyForm = configureForm(requests, cars, Dispatcher.greedyAssign, gridWidth, "Greedy Algorithm");
            Form1 closestPathForm = configureForm(requests, cars, Dispatcher.closestPathAssign, gridWidth, "Closest Path Algorithm");
            Form1 permutationForm = configureForm(requests, cars, Dispatcher.permutationAssign, gridWidth, "Permutation Algorithm");

            var forms = new List<Form>() {
                greedyForm, closestPathForm, permutationForm
            };

            foreach(var form in forms) {
                form.FormClosed += onFormClosed;
            }

            //to show all the forms on start
            //can be included in the previous foreach
            foreach(var form in forms) {
                form.Show();
            }

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
