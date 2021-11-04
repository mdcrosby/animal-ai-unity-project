using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EventTimeKeeper;

namespace AAIEvents
{
    public class AAIEvent// : ScriptableObject
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

            DeclareLogInstantiation();
        }

        public override string ToString()
        {
            string output = "AAIEvent(";
            foreach (KeyValuePair<string,object> x in new Dictionary<string, object>() {
                { "ID", ID }, { "timeStep", timeStep }, {"eventType", eventType }, {"eventLocation", eventLocation }, {"eventObject", eventObject }, {"description", description }
            }) {
                output += x.Key + ": " + x.Value.ToString() + ", ";
            }
            output = output.Remove(output.Length-2);
            output += ")";
            return output;
        }

        public void DeclareLogInstantiation() {
            Debug.Log(string.Format("Event {0}, type {1} logged at time {2} at {3}, with object {4}, described as '{5}'",
                        this.ID, this.eventType, this.timeStep, this.eventLocation, this.eventObject, this.description));
        }
    }
}
