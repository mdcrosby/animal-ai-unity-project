using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LE_AddObjectDropdown : MonoBehaviour
{
    private TrainingArena _TA;
    private Dropdown _DD;
    Dictionary<string, GameObject> objectDict;

    // Start is called before the first frame update
    void Awake()
    {
        _DD = this.GetComponent<Dropdown>();
    }

    // Called externally when it is safe to look for spawnable-object list
    public void GenerateObjectDropdownList()
    {
        _TA = GameObject.FindGameObjectWithTag("arena").GetComponent<TrainingArena>();
        List<GameObject> objectList = _TA.prefabs.GetList();
        if (objectList == null) { throw new Exception("No list of prefab objects found... can't generate dropdown menu!!"); }
        if (this.GetComponent<Dropdown>() == null) { throw new Exception("No dropdown component found.. can't generate dropdown menu!!"); }

        // objectDict is a Zip of object *names* and objects *themselves*
        // so contains string names for AddObjectDropdown list
        // and references to the objects themselves for when they are selected
        objectDict = (from obj in objectList select obj.name).ToList().Zip<string, GameObject, KeyValuePair<string, GameObject>>(
                objectList, (x, y) => new KeyValuePair<string, GameObject>(x, y)
            ).ToDictionary(x => x.Key, x => x.Value);

        List<string> SortOutputList(List<string> L) { L.Sort(); return L; }
        _DD.AddOptions(SortOutputList(objectDict.Keys.ToList()));
    }
}
