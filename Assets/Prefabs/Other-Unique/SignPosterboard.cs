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
    //public bool useStandardSize = true;

    public void SetSymbol(string s, bool needsUpdating = false) {
        selectedSymbolName = s;
        texIndex = Array.IndexOf(symbolNames, selectedSymbolName);
        if (needsUpdating) {
            UpdatePosterboard();
            //print("UpdatePosterboard() called with useDefaultColourArray " + useDefaultColourArray.ToString() + " from SetSymbol()");
        }
    }

    public void SetColourOverride(Color c, bool activateOverride = false, bool needsUpdating = false) {
        assignedColourOverride = c;
        if (activateOverride) {
            useDefaultColourArray = false;
            if (needsUpdating) {
                UpdatePosterboard();
                //print("UpdatePosterboard() called with useDefaultColourArray "+useDefaultColourArray.ToString()+" from SetColourOverride()");
            }
        }
    }

    public void SetColourOverride(Vector3 v, bool activateOverride = false, bool needsUpdating = false) {
        Color c = new Color(v.x / 255.0f, v.y / 255.0f, v.z / 255.0f);
        SetColourOverride(c, activateOverride, needsUpdating);
    }

    //public void ActivateResizing() { useStandardSize = false; }

    public void UpdatePosterboard() {
        // evaluate possible special case
        bool specialCodeCase = false;
        // i.e. weak check for if binary symbol code is intended
        if (texIndex == -1)
        {
            char c = selectedSymbolName[0];
            if (c == '0' || c == '1')
            {
                // try to parse the possible special-texture-code
                // if successful, we will have procedurally generated a texture so can use it
                Texture2D tex;
                // outputting tex correctly in test case according to pixel read
                specialCodeCase = parseSpecialTextureCode(selectedSymbolName, out tex);
                if (specialCodeCase) {
                    _symbolMat.SetTexture("_BaseMap", tex);
                    //string s = "codeTexture"; foreach (Color x in tex.GetPixels()) { s += x.ToString(); } print(s);
                }
            }
        }
        if (useDefaultColourArray)
        {
            if (!specialCodeCase) {
                KeyValuePair<Texture, Color> texture_colour_pair = getTextureAndColourByIndex(texIndex);
                _symbolMat.SetTexture("_BaseMap", texture_colour_pair.Key);
                //print(selectedSymbolName + ": useDefaultColourArray texture assignment");
                _symbolMat.color = texture_colour_pair.Value;
            }
            else {
                _symbolMat.color = Color.white;
            }
        }
        else
        {
            if (!specialCodeCase) {
                Texture texture = getTextureByIndex(texIndex);
                _symbolMat.SetTexture("_BaseMap", texture);
                //print(selectedSymbolName + ": non-default colour texture assignment");
            }
            //Debug.Log("assignedColourOverride: " + assignedColourOverride);
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
        //Debug.Log("colours[index]: " + colours[index]);
        return new KeyValuePair<Texture, Color>(textures[index], colours[index]);
    }


    public override void SetSize(Vector3 size)
    {
        base.SetSize((size==Vector3.one*-1) ? Vector3.one : size);
    }

    bool parseSpecialTextureCode(string texCode, out Texture2D tex) {

        int pixelWidth, pixelHeight;
        //print("texCode: " + texCode);

        int k = 0; char c = texCode[k];
        // iterate through first row to ascertain width
        while ((c == '0' || c == '1') && c != '/') { k++; c = texCode[k]; }
        
        // terminate if row ended incorrectly
        if (c != '/') { tex = null; return false; }
        // ...or if code isn't 'rectangular'
        pixelWidth = k;
        pixelHeight = (texCode.Length + 1) / (pixelWidth + 1);

        if ((texCode.Length + 1) % (pixelWidth + 1) != 0) { tex = null; return false; }
        //print("pixelWidth: "+pixelWidth+", pixelHeight"+pixelHeight);

        // convert to matrix coordinate form, checking each character is in {0,1}
        char[,] texBinary = new char[pixelHeight, pixelWidth];
        for (int i = 0; i < pixelHeight; ++i) {
            for (int j = 0; j < pixelWidth; ++j) {
                c = texCode[(pixelWidth+1) * i + j];
                if (c != '0' && c != '1') { tex = null; return false; }
                texBinary[i, j] = c;
            }
        }
        // string s = "texBinary: "; foreach (char x in texBinary) { s += x.ToString() + ", "; } print(s);
        // process binary matrix into flattened colour array for SetPixels()
        Color[] texCols = new Color[pixelWidth * pixelHeight];
        k = 0; Color col;
        for (int i = 0; i < pixelHeight; ++i) { for (int j = 0; j < pixelWidth; ++j) {
                col = (texBinary[pixelHeight-1-i, j] == '0') ? Color.black : Color.white;
                texCols[k] = col; k++;
            }
        }

        Texture2D specialSymbolTex = new Texture2D(pixelWidth, pixelHeight);
        specialSymbolTex.SetPixels(0, 0, pixelWidth, pixelHeight, texCols);
        specialSymbolTex.filterMode = FilterMode.Point;
        specialSymbolTex.Apply();

        // pass texture object back to 'out tex'
        tex = specialSymbolTex;
        return true;
    }
}
