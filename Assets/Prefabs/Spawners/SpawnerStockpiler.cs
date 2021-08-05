using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerStockpiler : GoalSpawner
{
    public bool stockpiling = false; // for inheriting class later on...
    public int doorOpenDelay = -1; // assuming not using
    public int timeBetweenDoorOpens = -1; // assuming not usings

    private GameObject Door;

    // Start is called before the first frame update
    public override void Awake()
    {
        base.Awake();
        if (stockpiling)
        {
            Door = transform.GetChild(1).gameObject;
            if (!Door.name.ToLower().Contains("door")) throw new Exception("WARNING: a stockpiling GoalSpawner has not found its Door.");
        }

        if (stockpiling) { StartCoroutine(manageDoor()); }
    }

    private IEnumerator manageDoor()
    {
        yield return new WaitForSeconds(doorOpenDelay);
        float dt = 0f; float newSize;
        while (dt < 1)
        {
            newSize = base.interpolate(0, 1, dt, 1, 0);
            Door.transform.localScale = new Vector3(Door.transform.localScale.x, newSize, Door.transform.localScale.z);
            dt += Time.fixedDeltaTime;
            yield return new WaitForSeconds(Time.fixedDeltaTime);
        }


        yield return null;
    }
}
