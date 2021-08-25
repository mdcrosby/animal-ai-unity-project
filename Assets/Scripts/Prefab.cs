using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
// using UnityEngineExtensions;
using PrefabInterface;


/// <summary>
/// A Prefab represents a GameObject that cna be spawned in an arena, it also contains the range of
/// values that the user can pass as parameters
/// </summary>
public class Prefab : MonoBehaviour, IPrefab
{

    public Vector2 rotationRange;
    public Vector3 sizeMin;
    public Vector3 sizeMax;
    public bool canRandomizeColor = true;
    public Vector3 ratioSize;
    public float sizeAdjustement = 0.999f;
    // to scale textures on dynamically-sized objects
    public bool textureUVOverride = false;

    protected float _height;

    public virtual void SetColor(Vector3 color)
    {
        if (canRandomizeColor)
        {
            Color newColor = new Color();
            newColor.a = 1f;
            newColor.r = color.x >=0 ? color.x/255f : Random.Range(0f,1f);
            newColor.g = color.y >=0 ? color.y/255f : Random.Range(0f,1f);
            newColor.b = color.z >=0 ? color.z/255f : Random.Range(0f,1f);

            if (GetComponent<Renderer>() != null)
            {
                GetComponent<Renderer>().material.color = newColor;
            }
            foreach (Renderer r in GetComponentsInChildren<Renderer>())
            {
                r.material.color = newColor;
            }
        }
    }

    public virtual void SetSize(Vector3 size)
    {
        Vector3 clippedSize = Vector3.Max(sizeMin, Vector3.Min(sizeMax, size)) * sizeAdjustement;
        float sizeX = size.x < 0 ? Random.Range(sizeMin[0], sizeMax[0]) : clippedSize.x;
        float sizeY = size.y < 0 ? Random.Range(sizeMin[1], sizeMax[1]) : clippedSize.y;
        float sizeZ = size.z < 0 ? Random.Range(sizeMin[2], sizeMax[2]) : clippedSize.z;

        _height = sizeY;
        transform.localScale = new Vector3(sizeX * ratioSize.x,
                                            sizeY * ratioSize.y,
                                            sizeZ * ratioSize.z);

        if (textureUVOverride) { RescaleUVs(); }
    }

    public virtual Vector3 GetRotation(float rotationY)
    {
        return new Vector3(0,
                        rotationY < 0 ? Random.Range(rotationRange.x, rotationRange.y) : rotationY,
                        0);
    }

    public virtual Vector3 GetPosition(Vector3 position,
                                        Vector3 boundingBox,
                                        float rangeX,
                                        float rangeZ)
    {
        float xBound = boundingBox.x;
        float zBound = boundingBox.z;
        float xOut = position.x < 0 ? Random.Range(xBound, rangeX - xBound) 
                                    : Math.Max(0,Math.Min(position.x, rangeX));
        float yOut = Math.Max(position.y,0);
        float zOut = position.z < 0 ? Random.Range(zBound, rangeZ - zBound) 
                                    : Math.Max(0,Math.Min(position.z, rangeZ));

        return new Vector3(xOut, AdjustY(yOut), zOut);
    }

    protected virtual float AdjustY(float yIn)
    {
        return yIn + _height / 2 + 0.01f;
    }

    protected virtual void RescaleUVs() {
        if (GetComponent<Renderer>() != null && GetComponent<Renderer>().material.GetTexture("_BaseMap") != null)
        {
            GetComponent<MeshFilter>().sharedMesh = Instantiate<Mesh>(GetComponent<MeshFilter>().mesh);
            Mesh MESH = GetComponent<MeshFilter>().sharedMesh;

            Debug.Log("Wall _BaseMap GET: " + GetComponent<Renderer>().material.GetTexture("_BaseMap"));
            Debug.Log("Wall material NAME: " + GetComponent<Renderer>().material.name);

            /*
            string meshVertices = "MeshVertex array, length " + MESH.vertices.Length + ": ";
            foreach (Vector3 vCoord in MESH.vertices)
            {
                meshVertices += vCoord.ToString() + ", ";
            }
            Debug.Log(meshVertices);

            string meshUV = "MeshUV array, length " + MESH.uv.Length + ": ";
            foreach (Vector2 uvCoord in MESH.uv) {
                meshUV += uvCoord.ToString() + ", ";
            }
            Debug.Log(meshUV);

            string meshNormals = "MeshNormal array, length " + MESH.normals.Length + ": ";
            foreach (Vector3 normCoord in MESH.normals)
            {
                meshNormals += normCoord.ToString() + ", ";
            }
            Debug.Log(meshNormals);
            */

            Vector2[] uvs = new Vector2[MESH.uv.Length];
            Dictionary<Vector3, Vector2Int> uvStretchLookup = new Dictionary<Vector3, Vector2Int> {
                { new Vector3(0f, 0f, 1f), new Vector2Int(0, 1) },
                { new Vector3(0f, 1f, 0f), new Vector2Int(0, 2) },
                { new Vector3(0f, 0f, -1f), new Vector2Int(0,1) },
                { new Vector3(0f, -1f, 0f), new Vector2Int(0,2) },
                { new Vector3(-1f, 0f, 0f), new Vector2Int(2,1) },
                { new Vector3(1f, 0f, 0f), new Vector2Int(2, 1) }
            };
            for (int i = 0; i < uvs.Length; ++i) {
                uvs[i].x = (MESH.uv[i].x > 0) ? transform.localScale[uvStretchLookup[MESH.normals[i]].x] : 0;
                uvs[i].y = (MESH.uv[i].y > 0) ? transform.localScale[uvStretchLookup[MESH.normals[i]].y] : 0;
            }
            MESH.uv = uvs;

            Debug.Log(transform.localScale[0] +", "+ transform.localScale[1] + ", " + transform.localScale[2]);
            /*meshUV = "MeshUV array, length " + MESH.uv.Length + ": ";
            foreach (Vector2 uvCoord in MESH.uv)
            {
                meshUV += uvCoord.ToString() + ", ";
            }
            Debug.Log(meshUV);*/
        }
    }

}