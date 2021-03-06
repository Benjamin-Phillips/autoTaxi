﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace autoTaxi{
    public class Car{
        // Speed of each car in feet per second
        public const double speed = 36.6667; // 36.6667 fps = 25 mph
        public const int capacity = 4;
        public double totalMiles = 0;
        private int passengers = 0;
        public List<DeliveredPassenger> delivered = new List<DeliveredPassenger>();
        private static int id = 0;

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
            get; set;
        }

        public int Id {
            get; private set;
        }

        public int Passengers {
            get {
                return passengers;
            }
            set {
                if (value >= 0) {
                    passengers = value;
                }
            }
        }

        // Constructor
        public Car(Position pos) {
            this.pos = pos;
            Id = id;
            id++;
            requests = new List<Request>();
        }

        public Car(Car c) {
            pos = c.pos;
            Id = c.Id;
            requests = new List<Request>();
        }
        /// <summary>
        /// Update the running delivery time average 
        /// </summary>
        /// <param name="time"> Time to use to update running average </param>
        public void updateDeliveryTimeAvg(int time) {
            if(deliveryTimeAvg == 0) {
                deliveryTimeAvg = time;
            }
            else {
                deliveryTimeAvg = (time + deliveryTimeAvg) / 2;
            }
        }

        public override string ToString() {
            return string.Format("id: {0}, pos: {1:f}, Psng: {2}", Id, pos, Passengers);
        }
    }

    // Struct to keep track of a delivered passenger's statistics
    public struct DeliveredPassenger {
        public Request passenger;
        public int timeOfDropoff;
        public int totalRideTime;
        public int idealRideTime;

        public DeliveredPassenger(Request passenger, int dropOffTime) {
            this.passenger = passenger;
            timeOfDropoff = dropOffTime;
            totalRideTime = dropOffTime - passenger.time;
            idealRideTime = (int)(Dispatcher.distance(passenger.start, passenger.end)/ Car.speed );
        }
    }
}
