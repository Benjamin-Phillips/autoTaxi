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

            int vehicles = 1;
            int frequency = 10 * 60; //60 seconds / request
            int simTime = 1 * 3600; //3600 seconds = 1 hour
            double medianDist = 36960; //7 miles in feet
            double stdDev = 8800; //1.66667 miles in feet
            double gridWidth = 2 * (medianDist + stdDev * 3); //width of the area

            Form1 greedyForm = new Form1();
            Form1 closestPathForm = new Form1();
            greedyForm.requests = Request.generateRequests(frequency, simTime, medianDist, stdDev, gridWidth);
            greedyForm.cars = Program.generateCars(vehicles, gridWidth);
            greedyForm.Assign = Dispatcher.greedyAssign;
            greedyForm.gridWidth = gridWidth;
            closestPathForm.requests = greedyForm.requests;
            closestPathForm.cars = greedyForm.cars;
            closestPathForm.Assign = Dispatcher.closestPathAssign;
            closestPathForm.gridWidth = gridWidth;

            var forms = new List<Form>() {
                greedyForm, closestPathForm
            };
            foreach(var form in forms) {
                form.FormClosed += onFormClosed;
            }

            //to show all the forms on start
            //can be included in the previous foreach
            foreach(var form in forms) {
                form.Show();
            }

            //to show only the first form on start
            //forms[0].Show();
        }
    }
}
