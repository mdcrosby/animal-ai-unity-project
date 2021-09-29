using System;
using System.Collections.Generic;
using UnityEngine;

public class SignPosterboard : Prefab
{

    private Material _symbolMat;
    public string selectedSymbolName;

    // Treat the following arrays as if a dictionary := {symbolNames[0]:(textures[0],colours[0]), symbolNames[1]:(textures[1],colours[1]), ...} so is serializable
    public string[] symbolNames; public Texture[] textures; public Color[] colours;
    private int texIndex;
    public bool useDefaultColourArray;
    public Color assignedColourOverride;

    public void SetSymbol(string s) {
        selectedSymbolName = s;
        texIndex = Array.IndexOf(symbolNames, selectedSymbolName);
        if (useDefaultColourArray)
        {
            KeyValuePair<Texture, Color> texture_colour_pair = getTextureAndColourByIndex(texIndex);
            _symbolMat.SetTexture("_BaseMap", texture_colour_pair.Key);
            _symbolMat.color = texture_colour_pair.Value;
        }
        else
        {
            Texture texture = getTextureByIndex(texIndex);
            _symbolMat.SetTexture("_BaseMap", texture);
            Debug.Log("assignedColourOverride: " + assignedColourOverride);
            _symbolMat.color = assignedColourOverride;
        }
    }

    void Awake()
    {
        // attempts to retrieve symbol material (the third of three in current implementation)
        // this needs changing if the implementation of SignPosterboard prefab changes
        _symbolMat = this.gameObject.GetComponent<MeshRenderer>().materials[2];
        if (!_symbolMat.name.Contains("symbol")) { Debug.Log("WARNING: a SignPosterboard may not have found the correct symbol material!!"); }
        // sets texture to show correct chosen symbol according to symbol name provided
        SetSymbol(selectedSymbolName);

    }

    private Texture getTextureByIndex(int index) {
        if (index == -1) { Debug.Log("WARNING: a SignPosterboard has not been given a valid symbol name! Defaulting to empty texture..."); }
        index = (index >= 0 && index < textures.Length) ? index : 0;
        return textures[index];
    }

    private KeyValuePair<Texture, Color> getTextureAndColourByIndex(int index) {
        if (index == -1) { Debug.Log("WARNING: a SignPosterboard has not been given a valid symbol name! Defaulting to empty texture..."); }
        index = (index >= 0 && index < symbolNames.Length) ? index : 0;
        Debug.Log("colours[index]: " + colours[index]);
        return new KeyValuePair<Texture, Color>(textures[index], colours[index]);
    }
}
