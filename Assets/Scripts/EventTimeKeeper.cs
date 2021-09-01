using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AAIEvents;

public class EventTimeKeeper : MonoBehaviour
{
    public enum EventType { FoodEaten, ZoneEntered, ZoneExited, DetectorEntered, DetectorExited, GoalGeneric };
    public List<AAIEvent> events = new List<AAIEvent>();
    public int nextID = 0;
    public int fixedFramesElapsed = 0;

    public void LogEvent(AAIEvent aaiEvent) {
        events.Add(aaiEvent);
        nextID++;
    }

    public string ExportEvents(bool logToDebug=true) {
        string printout = "List<AAIEvent> - [";
        foreach (AAIEvent e in events) {
            printout += e.ToString();
        }
        printout += "]";
        if (logToDebug) { Debug.Log(printout); }
        return printout;
    }

    private void FixedUpdate()
    {
        fixedFramesElapsed++;
    }
}
