// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;
using AAIEvents;


public class Goal : Prefab
{

    public int numberOfGoals = 1;
    public float reward = 1;
    public bool isMulti = false;

    public EventTimeKeeper ETK;

    void Awake()
    {
        canRandomizeColor = false;
    }

    public void Start()
    {
        ETK = GameObject.FindGameObjectWithTag("EventTimeKeeper").GetComponent<EventTimeKeeper>();
        //Debug.Log("ETK" + ETK + " found by " + this.name);
    }

    public virtual void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("agent"))
        {
            collision.GetComponent<TrainingAgent>().UpdateHealth(reward, true);
        }
    }

    public virtual void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("agent"))
        {
            registerNewAAIEvent();

            TrainingAgent agentScript = collision.gameObject.GetComponent<TrainingAgent>();
            // Debug.Break();
            if (!isMulti)
            {
                agentScript.UpdateHealth(reward, true);
            }
            else
            {
                agentScript.numberOfGoalsCollected++;
                if (agentScript.numberOfGoalsCollected == numberOfGoals)
                {
                    agentScript.UpdateHealth(reward, true);
                }
                else
                {
                    agentScript.UpdateHealth(reward);
                }
                gameObject.SetActive(false);
                Object.Destroy(gameObject);
            }
        }
    }
    
    // Facade method for registering new AAIEvent with a particular event type/description
    // Overwrite this with a different event type and description for each object
    public virtual void registerNewAAIEvent(bool isExiting=false) {
        bool episodeContinues = isMulti && numberOfGoals > 1;
        registerNewAAIEvent((episodeContinues ? EventTimeKeeper.EventType.GoalGeneric : EventTimeKeeper.EventType.EpisodeEnd),
                            "agent collided with " + decloneName(name) + (episodeContinues ? "." : " and episode ended."));
    }
    public virtual void registerNewAAIEvent(EventTimeKeeper.EventType eType, string eDescription) {

        Debug.Log(ETK.nextID + ETK.fixedFramesElapsed);
        AAIEvent e = new AAIEvent(ETK.nextID, ETK.fixedFramesElapsed,
            eType, transform.position, decloneName(name), eDescription);
        Debug.Log("new AAIEvent being registered: " + e);

        ETK.LogEvent(e);
    }

    public virtual string decloneName(string name) { return (name.EndsWith("(Clone)")) ? name.Substring(0,name.Length-7) : name; }

}