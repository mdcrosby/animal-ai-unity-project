using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerStockpiler : GoalSpawner
{
    public bool stockpiling = true; // for inheriting class later on...
    public int doorOpenDelay = -1; // assuming not using
    public int timeBetweenDoorOpens = -1; // assuming not using

    private Queue<BallGoal> waitingList; // for spawned goals waiting to materialise
    private GameObject Door;
    private int apparentSpawnCount; // = spawnCount + len(waitingList), i.e. goals yet-to-materialise

    // Start is called before the first frame update
    public override void Awake()
    {
        base.Awake();
        if (stockpiling)
        {
            Door = transform.GetChild(1).gameObject;
            if (!Door.name.ToLower().Contains("door")) throw new Exception("WARNING: a stockpiling GoalSpawner has not found its Door.");
        }

        waitingList = new Queue<BallGoal>();
        if (stockpiling) { StartCoroutine(manageDoor()); }
    }

    private void FixedUpdate()
    {
        if (waitingList.Count > 0 && freeToMaterialise(ripenedSpawnSize)) {
            print("waitingList.Count: " + waitingList.Count);
            BallGoal newGoal = waitingList.Dequeue();
            print("post-Dequeue() waitingList.Count: " + waitingList.Count);
            print("materialising newGoal: " + newGoal.name);
            immaterialStorageChange(newGoal, true);
        }
    }

    public override BallGoal spawnNewGoal(int listID) {
        BallGoal newGoal = base.spawnNewGoal(listID);
        newGoal.name += spawnCount + 1;

        if (!freeToMaterialise(ripenedSpawnSize, newGoal)) {
            immaterialStorageChange(newGoal, false);
            waitingList.Enqueue(newGoal);
        }

        return newGoal;
    }

    private bool freeToMaterialise(float r, BallGoal g=null) {
        Collider[] sphereCheck = Physics.OverlapSphere(transform.position + defaultSpawnPosition, r*0.4f);
        foreach (Collider col in sphereCheck) { if (col.gameObject.tag == spawnObjects[0].tag && (g==null || col.gameObject!=g)) { return false; } }
        return true;
    }

    // physically materialises a spawned goal stuck on the waiting list
    // or dematerialises if spawned goal needs to wait
    private void immaterialStorageChange(BallGoal goal, bool materialising) {
        goal.GetComponent<MeshRenderer>().enabled = materialising;
        goal.GetComponent<SphereCollider>().enabled = materialising;
        Rigidbody rb = goal.GetComponent<Rigidbody>();
        rb.useGravity = materialising;
        rb.isKinematic = !materialising;
        rb.constraints = materialising?RigidbodyConstraints.None:RigidbodyConstraints.FreezePosition;
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
