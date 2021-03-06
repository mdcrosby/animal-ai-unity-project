// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;


public class Goal : Prefab
{

    public int numberOfGoals = 1;
    public float reward = 1;
    public bool isMulti = false;

    void Awake()
    {
        canRandomizeColor = false;
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

}