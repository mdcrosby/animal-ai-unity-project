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

    private System.Random RNG = new System.Random();

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
        // i.e. weak check for if special symbol code is intended
        // or might just be a mistake...
        if (texIndex == -1)
        {
            char c = selectedSymbolName[0];
            // if starting with 0-9|*, then assume a special symbol code is intended
            if ((c >= '0' && c <= '9') || c == '*')
            {
                // try to parse the prospective special code
                // if successful, we will have procedurally generated a texture so can use it
                Texture2D tex;
                // specialCodeCase returns true iff parsing was successful - else, is invalid symbolName!
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
        for (int i = 0; i < 3; ++i) { // clamping to sensible size range as specified in docs
            if (size[i] != -1) { size[i] = Mathf.Clamp((float)size[i], 0.5f, 2.5f); }
        }
        base.SetSize((size==Vector3.one*-1) ? Vector3.one : size);
    }

    bool parseSpecialTextureCode(string texCode, out Texture2D tex) {
        print("RUNNING 1");
        //print("texCode: " + texCode);

        int pixelWidth, pixelHeight;
        Color[] texCols; // colour pixel value output will go here

        // first, look for an 'x', which indicates a random "M x N" is intended
        int xIndex = texCode.IndexOf('x');
        if (!(xIndex == -1 || xIndex == texCode.Length))
        {
            // split into first dimension and second dimension
            string[] splitCode = texCode.Split('x');
            // if not 2D, >1 'x' was used, so INVALID
            if (splitCode.Length != 2) { tex = null; return false; }
            // otherwise, generate a symbol with dimensions M x N
            bool dimensionParseSuccess = int.TryParse(splitCode[0], out pixelWidth);
            dimensionParseSuccess = int.TryParse(splitCode[1], out pixelHeight) && dimensionParseSuccess;
            // if not successfully parsed, then INVALID
            if (!dimensionParseSuccess) { tex = null; return false; }
            // else, we can generate!
            print("about to run generateSpecialSymbolByDims with: " + texCode);
            texCols = generateSpecialSymbolByDims(pixelWidth, pixelHeight);
        }
        else
        {
            int k = 0; char c = texCode[k];
            // iterate through first row to ascertain width
            while ((c == '0' || c == '1' || c == '*') && c != '/') { k++; c = texCode[k]; }
            print("RUNNING 2");
            // terminate if row ended incorrectly
            if (c != '/') { tex = null; return false; }
            // ...or if code isn't 'rectangular'
            pixelWidth = k;
            pixelHeight = (texCode.Length + 1) / (pixelWidth + 1);
            print("RUNNING 3");
            if ((texCode.Length + 1) % (pixelWidth + 1) != 0) { tex = null; return false; }
            //print("pixelWidth: "+pixelWidth+", pixelHeight"+pixelHeight);

            print("about to run specialCodeToTextureColours with: " + texCode);
            texCols = specialCodeToTextureColours(texCode, pixelHeight, pixelWidth);
        }

        if (texCols == null) { tex = null; return false; }

        Texture2D specialSymbolTex = new Texture2D(pixelWidth, pixelHeight);
        specialSymbolTex.SetPixels(0, 0, pixelWidth, pixelHeight, texCols);
        specialSymbolTex.filterMode = FilterMode.Point;
        specialSymbolTex.Apply();

        // pass texture object back to 'out tex'
        tex = specialSymbolTex;
        return true;

    }

    Color[] specialCodeToTextureColours(string texCode, int pH, int pW) {
        // convert to matrix coordinate form, checking each character is in {0,1}
        char[,] texBinary = new char[pH, pW];
        char c;
        for (int i = 0; i < pH; ++i)
        {
            for (int j = 0; j < pW; ++j)
            {
                c = texCode[(pW + 1) * i + j];
                if (c != '0' && c != '1' && c != '*') { return null; }
                texBinary[i, j] = c;
            }
        }
        // string s = "texBinary: "; foreach (char x in texBinary) { s += x.ToString() + ", "; } print(s);
        // process binary matrix into flattened colour array for SetPixels()
        Color[] texCols = new Color[pW * pH];
        int k = 0;
        Color col = Color.cyan;//placeholder - something wrong if Color.cyan is used!
        for (int i = 0; i < pH; ++i)
        {
            for (int j = 0; j < pW; ++j)
            {
                switch (texBinary[pH - 1 - i, j]) {
                    case '0':
                        col = Color.black; break;
                    case '1':
                        col = Color.white; break;
                    case '*':
                        col = (RNG.Next(0,2) == 0) ? Color.black : Color.white;
                        break;
                    default: break;
                }
                texCols[k] = col; k++;
            }
        }

        return texCols;
    }

    Color[] generateSpecialSymbolByDims(int pW, int pH) {
        int k = 0;
        Color[] texCols = new Color[pW * pH];

        for (int i=0; i < pH; ++i) { for (int j = 0; j < pW; ++j) { 
                texCols[k] = (RNG.Next(0, 2) == 0) ? Color.black : Color.white;
                k++;
            }
        }

        return texCols;
    }
}
