using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace autoTaxi {
    class Program {
        [STAThread]
        static void Main(string[] args) {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MyApplicationContext());
        }

        private static void greedySimulation() {
            //Area of Logan = 18.5 sq. mi. = 4.3012 mi x 4.3012 mi = 22710.1387 ft * 22710.137 ft
            //Median commute time cache county = 16.8 min. or 7.0 mi. at 25 mph. std. dev. = 4 min. = 1.66667 mi.
            int vehicles = 5;
            int frequency = 5 * 60; //60 seconds / request
            int simTime = 3600; //3600 seconds = 1 hour
            double medianDist = 36960; //7 miles in feet
            double stdDev = 8800; //1.66667 miles in feet
            double gridWidth = 45420.274; //width of the area ~8.6 mi in feet
            //testGenerateRequests(medianDist, stdDev, gridWidth);

            //simulate the greedy algorithm dispatcher & update.
            List<Request> requests = Request.generateRequests(frequency, simTime, medianDist, stdDev, gridWidth);
            List<Car> cars = generateCars(vehicles, gridWidth);

            int prevTime = 0;
            for(int i = 0; i < requests.Count; i++) {
                int elapsedTime = requests[i].time - prevTime;
                prevTime = requests[i].time;
                Console.WriteLine("\tRequest {0}/{1}: {2} -> {3}", i + 1, requests.Count, requests[i].start, requests[i].end);
                Dispatcher.greedyAssign(cars, requests[i]);
                foreach(Car c in cars) {
                    Console.WriteLine(c + ", dist: {0:f}", Dispatcher.distance(c.pos, requests[i].start) / 5280);
                }
                // Console.ReadKey();
                // Console.Write("\b");
                greedyUpdate(elapsedTime, cars);
            }
            Console.WriteLine();
            finishDeliveries(cars);
        }

        public static void finishDeliveries(List<Car> cars) {
            //determine times of remaining dropoffs
            List<double> times = new List<double>();
            foreach(Car c in cars) {
                Position pos = c.pos;
                double eventTime = 0;
                for(int i = 0; i < c.requests.Count; i++) { //time = distance(a, b) / speed
                    eventTime += Dispatcher.distance(c.requests[i].end, pos) / Car.speed;
                    times.Add(eventTime);
                    pos = c.requests[i].end;
                }
            }
            times.Sort();

            double timePassed = 0;
            update((int)Math.Ceiling(times[0]), cars); //remove first leg of the last pickup
            timePassed += times[0];
            times.RemoveAt(0);

            foreach(double eventTime in times) { //iterate through each remaining drop off
                Console.WriteLine("dx = {0:f}", ((eventTime - timePassed) * Car.speed) / 5280);
                foreach(Car c in cars) {
                    if(c.requests.Count > 0) {
                        Console.WriteLine(c + " next: {0:f}", Dispatcher.distance(c.pos, c.requests[0].end) / 5280);
                    }
                }
                update((int)Math.Ceiling(eventTime - timePassed), cars);
                timePassed += eventTime - timePassed;
                Console.WriteLine();
            }
        }

        static public void greedyUpdate(int elapsedTime, List<Car> cars) {
            foreach (Car car in cars) {
                if(car.requests.Count <= 0) {
                    return;
                }
                double travelDistance = Car.speed * elapsedTime;
                Position curPoint = car.requests[0].needsPickedUp ? car.requests[0].start : car.requests[0].end;

                // move car through dropoff points
                while (car.requests.Count > 0 && Dispatcher.distance(curPoint, car.pos) < travelDistance) {
                    travelDistance -= Dispatcher.distance(curPoint, car.pos);
                    car.pos = curPoint;
                    if (car.requests[0].needsPickedUp) {
                        car.requests[0].needsPickedUp = false;
                    }
                    else {
                        car.Passengers -= car.requests[0].passengers;
                        car.requests.RemoveAt(0);
                    }
                    Dispatcher.greedySort(car);

                    // Move current point to next pickup/dropoff
                    if(car.requests.Count > 0) {
                        curPoint = car.requests[0].needsPickedUp ? car.requests[0].start : car.requests[0].end;
                    }
                }

                // if next point further than remaining travel distance
                if (travelDistance > 0 && car.requests.Count > 0) {
                    double deltaX = car.pos.x - curPoint.x;
                    double deltaY = car.pos.y - curPoint.y;
                    double angle = Math.Atan((Math.Abs(deltaY) / Math.Abs(deltaX)));
                    double xDir = 1;
                    double yDir = 1;
                    if (deltaX > 0) {
                        xDir = -1;
                    }
                    if (deltaY > 0) {
                        yDir = -1;
                    }
                    car.pos = new Position(car.pos.x + travelDistance * Math.Cos(angle) * xDir, car.pos.y + travelDistance * Math.Sin(angle) * yDir);
                }
            }
        }

        static public void update(int elapsedTime, List<Car> cars) {
            foreach(Car car in cars) {
                double travelDistance = Car.speed * elapsedTime;
                //move car through dropoff points
                while (car.requests.Count > 0 && Dispatcher.distance(car.requests[0].end, car.pos) < travelDistance) {
                    travelDistance -= Dispatcher.distance(car.requests[0].end, car.pos);
                    car.pos = car.requests[0].end;
                    car.Passengers -= car.requests[0].passengers;
                    car.requests.RemoveAt(0);
                }
                //next point further than remaining travel distance
                if(travelDistance > 0 && car.requests.Count > 0) {
                    double deltaX = car.pos.x - car.requests[0].end.x;
                    double deltaY = car.pos.y - car.requests[0].end.y;
                    double angle = Math.Atan((Math.Abs(deltaY) / Math.Abs(deltaX)));
                    double xDir = 1;
                    double yDir = 1;
                    if(deltaX > 0) {
                        xDir = -1;
                    }
                    if(deltaY > 0) {
                        yDir = -1;
                    }
                    car.pos = new Position(car.pos.x + travelDistance * Math.Cos(angle) * xDir, car.pos.y + travelDistance * Math.Sin(angle) * yDir);
                }
            }
        }

        private void testGreedySort() {
            List<Request> reqs = new List<Request>();
            Random rand = new Random();
            Car car = new Car(new Position(0, 0));
            for (int i = 0; i < 15; i++) {
                car.requests.Add(new Request(new Position(0, 0), new Position(0, rand.Next() % 100), 0));
            }
            Dispatcher.greedySort(car);

            for (int i = 0; i < 15; i++) {
                Console.WriteLine(reqs[i].end.y);
            }
        }

        private static void testDistance() {
            for (int i = 0; i < 5; i++) {
                for (int j = 0; j < 5; j++) {
                    Position temp = new Position(i, j);
                    Position temp2 = new Position(1, 1);
                    Console.WriteLine("Dist between " + temp.x + "," + temp.y + " and " + temp2.x + "," + temp2.y + " is " + Dispatcher.distance(temp, temp2));
                }
            }
        }

        public static List<Car> generateCars(int numCars, double gridWidth) {
            List<Car> cars = new List<Car>();
            Random rand = new Random();
            for(int i = 0; i < numCars; i++) {
                cars.Add(new Car(new Position(rand.NextDouble() * gridWidth, rand.NextDouble() * gridWidth)));
            }
            return cars;
        }

        private static void testGenerateRequests(double medianDist, double stdDev, double gridWidth) {
            List<Request> requests = Request.generateRequests(5, 3600, medianDist, stdDev, gridWidth);

            foreach(Request r in requests) {
                double distance = Math.Sqrt(Math.Pow(r.start.x - r.end.x, 2) + Math.Pow(r.start.y - r.end.y, 2));
                double theta = Math.Acos(r.end.x / distance);
                Console.WriteLine("({0:f}, {1:f}) -> ({2:f}, {3:f}), dist: {4:f}, radians: {5:f}",
                    r.start.x, r.start.y, r.end.x, r.end.y, distance, theta);
            }
        }
    }
}
