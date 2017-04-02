﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace autoTaxi{
    public class Car{
        // Speed of each car
        static public int speed;
        public const int capacity = 4;

        // Running delivery time avg. Update every time you drop off a person.
        public double deliveryTimeAvg {
            get; private set;
        }

        // Passengers in/waiting for this car
        public List<Request> requests {
            get; private set;
        }

        // Current position of the car
        public Position pos {
            get; private set;
        }

        public int passengers {
            get { return passengers; }
            set {
                if (value >= 0) {
                    passengers = value;
                }
            }
        }

        // Constructor
        public Car(Position pos) {
            this.pos = pos;
            requests = new List<Request>();
        }

        // Update current locatoin using elapsed time, speed, and routes
        public void update(int elapsedTime) {
            int distanceTraveled = elapsedTime * speed;


        }
    }
}
