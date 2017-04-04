using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace autoTaxi{
    class Dispatcher{

        // Takes a list of cars and a request, and decides which 
        // car should handle that request. It adds the appropriate 
        // requests to that car.
        public static bool greedy(List<Car> cars, Request curRequest) {
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
                        cars[bestCarIndex].requests.Insert(i, new Request(new Position(0, 0), curRequest.start, curRequest.time, 0));
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

