using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace autoTaxi {
    class Program {
        static void Main(string[] args) {
            //Area of Logan = 18.5 sq. mi. = 4.3012 mi x 4.3012 mi = 22710.1387 ft * 22710.137 ft
            //Median commute time cache county = 16.8 min. or 7.0 mi. at 25 mph. std. dev. = 4 min. = 1.66667 mi.
            //
            int frequency = 5; //5 minutes / request
            int simTime = 3600; //3600 seconds = 1 hour
            double medianDist = 36960; //7 miles in feet
            double stdDev = 8800; //1.66667 miles in feet
            double gridWidth = 45420.274; //width of the area ~8.6 mi in feet
            //testGenerateRequests(medianDist, stdDev, gridWidth);
            List<Request> requests = Request.generateRequests(frequency, simTime, medianDist, stdDev, gridWidth);
            List<Car> cars = generateCars(10, gridWidth);

            int prevTime = 0;
            for(int i = 0; i < requests.Count; i++) {
                int elapsedTime = requests[i].time - prevTime;
                prevTime = requests[i].time;
                Dispatcher.greedy(cars, requests[i]);
                update(elapsedTime, cars);
            }
        }

        static public void update(int elapsedTime, List<Car> cars) {
            foreach(Car car in cars) {
                double travelDistance = Car.speed * elapsedTime;
                while (Dispatcher.distance(car.requests[0].end,  car.pos) < travelDistance) {
                    travelDistance -= Dispatcher.distance(car.requests[0].end, car.pos);
                    car.pos = car.requests[0].end;
                    car.passengers -= car.requests[0].passengers;
                    car.requests.RemoveAt(0);
                }
                if(travelDistance > 0) {
                    double angle = Math.Atan((Math.Abs(car.pos.y - car.requests[0].end.y) / Math.Abs(car.pos.x - car.requests[0].end.x)));
                    car.pos = new Position(car.pos.x + travelDistance * Math.Cos(angle), car.pos.y + travelDistance * Math.Sin(angle));
                }
            }
        }

        private void testGreedySort() {
            List<Request> reqs = new List<Request>();
            Random rand = new Random();
            for (int i = 0; i < 15; i++) {
                reqs.Add(new Request(new Position(0, 0), new Position(0, rand.Next() % 100), 0));
            }
            Dispatcher.greedySort(reqs);

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
