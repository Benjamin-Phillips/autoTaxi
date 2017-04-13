using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace autoTaxi {
    public class Request {
        /// <summary>
        /// Percent time allowed over normal travel time to fulfill request. x > 1.
        /// </summary>
        public static double timeBuffer = 1.5;
        private Request pickup;
        /// <summary>
        /// request start point.
        /// </summary>
        public Position start {
            get; private set;
        }
        /// <summary>
        /// request destination.
        /// </summary>
        public Position end {
            get; private set;
        }
        /// <summary>
        /// Time the request is submitted in seconds from start of day.
        /// </summary>
        public int time {
            get; private set;
        }

        public int passengers {
            get; private set;
        }

        public Request Pickup {
            get {
                return pickup;
            }
            set {
                pickup = value;
            }
        }
        // True if this request has not been picked up from it's start point, else false
        public bool needsPickedUp = true;

        public Request(Position s, Position e, int t, int p = 1) {
            start = s;
            end = e;
            time = t;
            passengers = p;
        }

        /// <summary>
        /// Simulates ride requests over a time period given a frequency, median distance & deviation.
        /// </summary>
        /// <param name="frequency"> seconds per request.</param>
        /// <param name="simulationTime"> seconds of simulation time.</param>
        /// <param name="medianTripDistance"> distance in feet.</param>
        /// <param name="stdDev"> deviation in feet.</param>
        /// <returns></returns>
        public static List<Request> generateRequests(int frequency, int simulationTime, double medianTripDistance, double stdDev, double gridWidth) {
            List<Request> requests = new List<Request>();
            Random rand = new Random();
            int time = 0;
            while(time <= simulationTime) {
                double distance = randomNormal(medianTripDistance, stdDev, rand);
                double theta = ((rand.NextDouble() * 360) * Math.PI) / 180; //radians 0 - 6.28

                Position start = new Position((rand.NextDouble() * gridWidth), (rand.NextDouble() * gridWidth)); //random point in 8.6 mi x 8.6 mi area.
                Position end;
                int count = 0;
                do { //force points inside of area.
                    if(count++ > 15) {
                        distance = .95 * distance;
                        Console.WriteLine("50 or more failed attempts at setting endpoint of request; Decreasing distance to travel by 5%");
                    }
                    end = new Position(start.x + (distance * Math.Cos(theta)), start.y + (distance * Math.Sin(theta)));
                    theta = ((rand.NextDouble() * 360) * Math.PI) / 180; //radians 0 - 6.28
                } while(end.x < 0 || end.y < 0 || end.x > gridWidth || end.y > gridWidth);

                int deltaTime = rand.Next() % (2 * frequency + 1);
                requests.Add(new Request(start, end, time + deltaTime));
                time += deltaTime;
            }
            return requests;
        }

        /// <summary>
        /// Generates random values with a semi-normal distribution. Max = mean + 3*std.dev, Min = 0
        /// </summary>
        /// <param name="mean"></param>
        /// <param name="stdDev"></param>
        /// <returns></returns>
        public static double randomNormal(double mean, double stdDev, Random rand) {
            double u1 = 1.0 - rand.NextDouble(); //uniform(0,1] random doubles
            double u2 = 1.0 - rand.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                         Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
            double randStdDev = (mean + stdDev * randStdNormal) % (mean + (3 * stdDev)); //random normal(mean,stdDev^2)
            if(randStdDev < 0) {
                randStdDev += (mean + (3 * stdDev));
            }
            return randStdDev;
        }
    }

    /// <summary>
    /// x, y position from origin in feet. 1 mile = 5280 feet.
    /// </summary>
    public struct Position {
        public double x;
        public double y;

        public Position(double x, double y) {
            this.x = x;
            this.y = y;
        }

        public Position(Position pos, double x, double y) {
            this.x = pos.x + x;
            this.y = pos.y + y;
        }

        public override string ToString() {
            return string.Format("({0:f}, {1:f})", x / 5280 , y / 5280);
        }

        public double dotProduct(Position point) {
            return (x * point.x) + (y * point.y);
        }

        /// <summary>
        /// A.distance(B) returns A - B
        /// </summary>
        public Position distance(Position point) {
            return new Position(x - point.x, y - point.y);
        }

        public Position sum(Position point) {
            return new Position(x + point.x, y + point.y);
        }

        public void scalarMultiply(double a) {
            x *= a;
            y *= a;
        }

        public static bool operator ==(Position a, Position b) {
            if(a.x == b.x && a.y == b.y) {
                return true;
            }
            return false;
        }

        public static bool operator !=(Position a, Position b) {
            if(a.x != b.x || a.y != b.y) {
                return true;
            }
            return false;
        }
    }
}
