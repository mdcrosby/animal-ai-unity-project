// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

public class DeathZone : Goal
{

    public override void SetSize(Vector3 size)
    {
        Vector3 clippedSize = Vector3.Max(sizeMin, Vector3.Min(sizeMax, size)) * sizeAdjustement;
        float sizeX = size.x < 0 ? Random.Range(sizeMin[0], sizeMax[0]) : clippedSize.x;
        float sizeY = size.x < 0 ? Random.Range(sizeMin[1], sizeMax[1]) : clippedSize.x;
        float sizeZ = size.z < 0 ? Random.Range(sizeMin[2], sizeMax[2]) : clippedSize.z;

        transform.localScale = new Vector3(sizeX * ratioSize.x,
                                            sizeY * ratioSize.y,
                                            sizeZ * ratioSize.z);
    }

    protected override float AdjustY(float yIn)
    {
        return -0.15f;
    }

}
