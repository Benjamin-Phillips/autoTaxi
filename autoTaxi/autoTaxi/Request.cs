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
        /// Time the request is 'created'.
        /// </summary>
        public int time {
            get; private set;
        }

        public Request(Position s, Position e, int t) {
            start = s;
            end = e;
            time = t;
        }

        public static List<Request> generateRequests(int frequency, int simulationTime, double medianTripDistance) {
            List<Request> requests = new List<Request>();
            Random rand = new Random();
            int time = 0;
            while(time <= simulationTime) {
                Position start = new Position(0, 0); //TODO generate this randomly.
                Position end = new Position(1, 1);
                int delta = rand.Next() % (2 * frequency + 1);
                requests.Add(new Request(start, end, delta));
                time += delta;
            }
            return requests;
        }
    }

    public struct Position {
        public double x;
        public double y;

        public Position(double x, double y) {
            this.x = x;
            this.y = y;
        }
    }
}
