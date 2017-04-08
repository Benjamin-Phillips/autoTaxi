using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace autoTaxi{
    class Dispatcher{

        /// <summary>
        /// Assigns the new request to the car who's current route comes nearest to the request pickup point.
        /// </summary>
        /// <returns>false if all cars are full, true otherwise</returns>
        public static bool closestPathAssign(List<Car> cars, Request newReq) {
            double closestPathDist = double.MaxValue;
            int closestPathIndex = -1;
            int bestCarIndex = -1;

            foreach(Car c in cars) {
                double distance; //distance from point to a line-segment
                if(c.requests.Count == 0) { //edge case: car has no requests
                    distance = Dispatcher.distance(c.pos, newReq.start);
                    setNewBestPath(cars, ref closestPathDist, ref closestPathIndex, ref bestCarIndex, c, distance, 0);
                } else if(c.Passengers + newReq.passengers <= Car.capacity) { //if car has capacity for request
                    Position pathStart = c.pos; //start point of first line-segment starts at car
                    foreach(Request r in c.requests) { //find best time to pickup new request for each car
                        distance = pathPointDistance(newReq.start, pathStart, r.end);
                        setNewBestPath(cars, ref closestPathDist, ref closestPathIndex, ref bestCarIndex, c, distance, c.requests.IndexOf(r));
                        pathStart = r.end; //set next line-segment start point
                    } //edge case: most convenient pickup time is after all passengers dropped off
                    distance = Dispatcher.distance(newReq.start, c.requests.Last().end);
                    setNewBestPath(cars, ref closestPathDist, ref closestPathIndex, ref bestCarIndex, c, distance, c.requests.Count);
                }
            }

            if(bestCarIndex == -1) { //all cars full
                return false;
            } else { //add request to car
                assignRequest(cars[bestCarIndex], newReq, closestPathIndex);
                return true;
            }
        }

        private static void setNewBestPath(List<Car> cars, ref double closestPathDist, ref int closestPathIndex, ref int bestCarIndex, Car c, double distance, int index) {
            if(distance < closestPathDist) {
                closestPathDist = distance;
                closestPathIndex = index;
                bestCarIndex = cars.IndexOf(c);
            }
        }

        /// <summary>
        /// calculates the shortest distance from a point to a line-segment. http://stackoverflow.com/a/1501725
        /// </summary>
        public static double pathPointDistance(Position point, Position start, Position end) {
            Position endStartDelta = end.distance(start); //(w - v)
            double segmentLengthSqr = endStartDelta.dotProduct(endStartDelta); //|w - v|^2
            if(segmentLengthSqr == 0.0) { //start == end
                return distance(point, start);
            }
            Position pointStartDelta = point.distance(start); //(p - v)
            double t = Math.Max(0, Math.Min(1, pointStartDelta.dotProduct(endStartDelta) / segmentLengthSqr));
            endStartDelta.scalarMultiply(t); // t * (w - v)
            Position projection = start.sum(endStartDelta); //v + [t * (w - v)]
            return distance(point, projection);
        }

        private static void assignRequest(Car car, Request req, int reqIndex) {
            car.Passengers += req.passengers; // Add passengers to the car
            car.requests.Add(req); // Add the dropoff request to the requests list
            car.requests.Insert(reqIndex, new Request(new Position(0, 0), req.start, -1, 0)); //add pickup request
            greedySort(car.requests);
        }

        // Takes a list of cars and a request, and decides which 
        // car should handle that request. It adds the appropriate 
        // requests to that car.
        public static bool greedyAssign(List<Car> cars, Request curRequest) {
            double shortestDist = double.PositiveInfinity;

            double dist;
            int bestCarIndex = -1;

            // Find the index of the car that is closest to the request start point 
            for (int i = 0; i < cars.Count; i++) {
                dist = distance(cars[i].pos, curRequest.start);
                if (dist < shortestDist && cars[i].Passengers + curRequest.passengers <= Car.capacity) {
                    shortestDist = dist;
                    bestCarIndex = i;
                }
            }

            // If best car index remains -1, then no car has the capacity to pick up the request
            if (bestCarIndex == -1) {
                return false;
            }
            else {
                cars[bestCarIndex].Passengers += curRequest.passengers; // Add passengers to the car
                cars[bestCarIndex].requests.Add(curRequest); // Add the request to the requests list

                // Figure out where to insert the pickup mock request
                for (int i = 0; i < cars[bestCarIndex].requests.Count; i++) {
                    if (cars[bestCarIndex].requests[i].passengers > 0 ||
                        distance(cars[bestCarIndex].pos, curRequest.start) < distance(cars[bestCarIndex].pos, cars[bestCarIndex].requests[i].end)) {
                        cars[bestCarIndex].requests.Insert(i, new Request(new Position(0, 0), curRequest.start, -1, 0));
                        break;
                    }
                }
                greedySort(cars[bestCarIndex].requests);
                return true;
            }
        }

        public static double distance(Position start, Position end) {
            return Math.Sqrt(Math.Pow((start.x - end.x), 2.0) + Math.Pow((start.y - end.y), 2.0));
        }

        /* 
         * Assumes that the first index is taken by the request we are picking up 
         * immediately. Sorts the rest of the list based on closest distance relative
         * to the current request being considered on each iteration.
         */
        public static void greedySort(List<Request> requests) {
            int shortestDistIndex;
            double shortestDistance;
            int curPoint = 0;
            while (curPoint < requests.Count - 1 && requests[curPoint + 1].passengers == 0) {
                curPoint++;
            }

            for (; curPoint < requests.Count - 1; curPoint++) {
                shortestDistIndex = curPoint + 1;
                shortestDistance = distance(requests[curPoint].end, requests[shortestDistIndex].end);
                for (int j = curPoint + 1; j < requests.Count; j++) {
                    double tempDistance = distance(requests[j].end, requests[curPoint].end);
                    if (tempDistance < shortestDistance) {
                        shortestDistance = tempDistance;
                        shortestDistIndex = j;
                    }
                }
                if (curPoint + 1 != shortestDistIndex) { //swap if they're not the same
                    Request temp = requests[curPoint + 1];
                    requests[curPoint + 1] = requests[shortestDistIndex];
                    requests[shortestDistIndex] = temp;
                }
            }
        }
    }
}

