using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace autoTaxi{
    class Dispatcher{
        private static List<List<Position>> permutations;

        //TODO: CONSIDER HOW TO HANDLE CAPACITY
        public static bool permutationAssign(List<Car> cars, Request newReq) {
            List<List<Position>> bestPermutations = new List<List<Position>>();
            List<double> permutationLengths = new List<double>();

            foreach(Car c in cars) { //find best permutation for each car
                permutations = new List<List<Position>>(); //reset
                List<Position> endpoints = new List<Position>();
                endpoints.Add(newReq.start); //must be in endpoints to work
                foreach(Request r in c.requests) {
                    endpoints.Add(r.end); //init endpoints
                }
                generatePermutations(new List<Position>(), endpoints, newReq.start, newReq.end);

                double bestPerm = double.MaxValue;
                int bestPermIndex = -1; //find index of best perm
                foreach(List<Position> perm in permutations) {
                    perm.Insert(0, c.pos); //first point in perm is always car.pos
                    double routeLength = getRouteLength(perm);
                    if(routeLength < bestPerm) { //new best permutation
                        bestPerm = routeLength;
                        bestPermIndex = permutations.IndexOf(perm);
                    }
                }
                bestPermutations.Add(permutations[bestPermIndex]); //record perm
                permutationLengths.Add(bestPerm); //record length
            }

            double bestLength = double.MaxValue;
            int bestCarIndex = -1; //find car w/absolute best permutation
            foreach(double length in permutationLengths) { 
                if(length < bestLength) {
                    bestCarIndex = permutationLengths.IndexOf(length);
                }
            }

            //change cars route to match the permutation
            Car bestCar = cars[bestCarIndex];
            List<Position> positions = bestPermutations[bestCarIndex];
            for(int i = 0; i < bestCar.requests.Count - 1; i++) {
                Request temp = bestCar.requests[i];
                int swapIndex = -1;
                for(int j = i; j < bestCar.requests.Count; j++) {
                    //if(bestCar.requests[j].end != positions[i]) { //TODO: overload me

                    //}

                }
            }
            return false;
        }

        /// <summary>
        /// Method assumes pickup is also in the list of endpoints, generates all permutations s.t. dropoff comes after pickup.
        /// </summary>
        public static void generatePermutations(List<Position> permutation, List<Position> endpoints, Position pickup, Position dropoff) {
            if(permutation.Count < endpoints.Count + 2) { //create permutation using all points
                //if pickup is in the permutation and dropoff is not already in the permutation add it
                if(permutation.Contains(pickup) && !permutation.Contains(dropoff)) {
                    endpoints.Add(dropoff);
                }
                foreach(Position p in endpoints) { //create legal permutations recursively
                    if(!permutation.Contains(p)) {
                        List<Position> newPerm = new List<Position>(permutation);
                        newPerm.Add(p);
                        generatePermutations(newPerm, endpoints, pickup, dropoff);
                    }
                }
            } else {
                permutations.Add(permutation); //TODO SUPAH IMPORTANTE, MUST MAINTAIN LEGAL PERMUTATION FOR MULTIPLE PICKUP REQUESTS.
            }
        }

        public static double getRouteLength(List<Position> route) { //TODO: finish this?
            return 0;
        }

        /// <summary>
        /// Assigns the new request to the car who's current route comes nearest to the request pickup point.
        /// If the car is over capacity it will not consider the first n segments.
        /// </summary>
        /// <returns>false if all cars are full, true otherwise</returns>
        public static bool closestPathAssign(List<Car> cars, Request newReq) {
            double closestPathDist = double.MaxValue;
            int closestPathIndex = -1;
            int bestCarIndex = -1;

            foreach(Car c in cars) {
                double deltaDistance; //distance from point to a line-segment
                if(c.requests.Count == 0) { //edge case: car has no requests
                    deltaDistance = Dispatcher.distance(c.pos, newReq.start);
                    updatePathIfBest(cars, ref closestPathDist, ref closestPathIndex, ref bestCarIndex, c, deltaDistance, 0);
                } else if(c.Passengers + newReq.passengers <= Car.capacity) { //if car has capacity for request
                    Position pathStart = c.pos; //start point of first line-segment starts at car
                    foreach(Request r in c.requests) { //find best time to pickup new request for each car
                        deltaDistance = pathPointDistance(newReq.start, pathStart, r.end);
                        updatePathIfBest(cars, ref closestPathDist, ref closestPathIndex, ref bestCarIndex, c, deltaDistance, c.requests.IndexOf(r));
                        pathStart = r.end; //set next line-segment start point
                    } //edge case: most convenient pickup time is after all passengers dropped off
                    deltaDistance = Dispatcher.distance(newReq.start, c.requests.Last().end);
                    updatePathIfBest(cars, ref closestPathDist, ref closestPathIndex, ref bestCarIndex, c, deltaDistance, c.requests.Count);
                } else { //edge case: overloaded car, drop passengers off first.
                    int passengers = c.Passengers + newReq.passengers;
                    for(int reqIndex = 0; reqIndex < c.requests.Count - 1; reqIndex++) {
                        passengers -= c.requests[reqIndex].passengers; //dropoff x passengers at request i
                        if(passengers <= Car.capacity) { //if room at this point in route, find distance
                            deltaDistance = pathPointDistance(newReq.start, c.requests[reqIndex].end, c.requests[reqIndex + 1].end);
                            updatePathIfBest(cars, ref closestPathDist, ref closestPathIndex, ref bestCarIndex, c, deltaDistance, reqIndex);
                        }
                    }
                }
            }

            assignRequest(cars[bestCarIndex], newReq, closestPathIndex);
            return true;
        }

        /// <summary>
        /// Updates the shortest distance, and indices of the best car/path
        /// </summary>
        private static void updatePathIfBest(List<Car> cars, ref double closestPathDist, ref int closestPathIndex, ref int bestCarIndex, Car c, double distance, int index) {
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
            if(car.Id == 0) { //test code: remove later
                Console.WriteLine("Insert pickup at {0}", reqIndex);
            }
            car.Passengers += req.passengers; // Add passengers to the car
            car.requests.Add(req); // Add the dropoff request to the requests list
            Request mockRequest = new Request(req.end, req.start, -1, 0);
            req.Pickup = mockRequest;
            car.requests.Insert(reqIndex, mockRequest); //add pickup request
            nearestPathSort(car.requests, reqIndex);
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
                curPoint++; //find first nonzero passenger request
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

        public static void nearestPathSort(List<Request> requests, int index) {
            List<Request> legalChoices = new List<Request>();
            List<Request> illegalChoices = new List<Request>();
            findValidChoices(requests, legalChoices, illegalChoices, index + 1);
            Position curPos = requests[index].end;
            for(int i = index + 1; i < requests.Count; i++) {

            }
        }

        private static void findValidChoices(List<Request> requests, List<Request> legalChoices, List<Request> illegalChoices, int index) {
            for(int i = index; i < requests.Count; i++) { //index of first sortable request
                if(requests[i].passengers == 0) { //pickup request is legal destination
                    legalChoices.Add(requests[i]);
                } else if(requests.Contains(requests[i].Pickup) && //dropoff before pickup is illegal
                    requests.IndexOf(requests[i].Pickup) >= index) { //when pickup request is sortable
                    illegalChoices.Add(requests[i]);
                } else {
                    legalChoices.Add(requests[i]);
                }
            }
        }
    }
}

