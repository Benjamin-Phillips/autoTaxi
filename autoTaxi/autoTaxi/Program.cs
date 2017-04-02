﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace autoTaxi {
    class Program {
        static void Main(string[] args) {
            testGenerateRequests();
        }

        private static void testGenerateRequests() {
            //Area of Logan = 18.5 sq. mi. = 4.3012 mi x 4.3012 mi = 22710.1387 ft * 22710.137 ft
            //Median commute time cache county = 16.8 min. or 7.0 mi. at 25 mph. std. dev. = 4 min. = 1.66667 mi.
            //
            double medianDist = 36960; //7 miles in feet
            double stdDev = 8800; //1.66667 miles in feet
            List<Request> requests = Request.generateRequests(5, 3600, medianDist, stdDev);

            foreach(Request r in requests) {
                double distance = Math.Sqrt(Math.Pow(r.start.x - r.end.x, 2) + Math.Pow(r.start.y - r.end.y, 2));
                double theta = Math.Acos(r.end.x / distance);
                Console.WriteLine("({0:f}, {1:f}) -> ({2:f}, {3:f}), dist: {4:f}, radians: {5:f}",
                    r.start.x, r.start.y, r.end.x, r.end.y, distance, theta);
            }
        }
    }
}
