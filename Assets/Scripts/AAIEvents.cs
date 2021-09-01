using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EventTimeKeeper;

namespace AAIEvents
{
    public class AAIEvent : ScriptableObject
    {
        public int ID;
        public int timeStep;
        public EventTimeKeeper.EventType eventType;
        public Vector3 eventLocation;
        public string eventObject;
        public string description;

        public AAIEvent(int ID, int timeStep, EventTimeKeeper.EventType eventType,
                        Vector3 eventLocation, string eventObject, string description)
        {
            this.ID = ID;
            this.timeStep = timeStep;
            this.eventType = eventType;
            this.eventLocation = eventLocation;
            this.eventObject = eventObject;
            this.description = description;

            Debug.Log(string.Format("Event {0}, type {1} logged at time {2} at {3}, with object {4}, described as '{5}'",
                        this.ID, this.eventType, this.timeStep, this.eventLocation, this.eventObject, this.description));
        }

        public override string ToString()
        {
            string output = "AAIEvent(";
            foreach (object x in new object[] { ID, timeStep, eventType, eventLocation, eventObject, description }) {
                output += nameof(x) + ": " + x.ToString() + ", ";
            }
            output += ")";
            return output;
        }
    }
}
