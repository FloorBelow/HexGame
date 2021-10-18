using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HexUtils;

[CreateAssetMenu]
public class TerrainTriSetData : ScriptableObject {
    public GameObject flat;

    public GameObject hillIn;
    public GameObject hillOut;

    public GameObject cliffIn;
    public GameObject cliffOut;
    public GameObject cliffHill;

    public GameObject tallIn;
    public GameObject tallOut;
    public GameObject tallHill;
    public GameObject tallCliff;

    Dictionary<int, Lookup> lookupObjects;

    struct Lookup {
        public GameObject o;
        public int rotation;
        public bool flip;
	}


    public void BuildLookups() {
        lookupObjects = new Dictionary<int, Lookup>();
        AddLookup(flat, 0, 0, 0);

        AddLookup(hillIn, 0, 1, 1);
        AddLookup(hillOut, 1, 0, 0);

        AddLookup(cliffIn, 0, 2, 2);
        AddLookup(cliffOut, 2, 0, 0);
        AddLookup(cliffHill, 0, 2, 1);
        AddLookup(cliffHill, 0, 1, 2, true);

        AddLookup(tallIn, 0, 3, 3);
        AddLookup(tallOut, 3, 0, 0);
        AddLookup(tallHill, 0, 3, 1);
        AddLookup(tallHill, 0, 1, 3, true);
        AddLookup(tallCliff, 0, 3, 2);
        AddLookup(tallCliff, 0, 2, 3, true);

    }

    void AddLookup(GameObject o, byte a, byte b, byte c, bool flip = false) {
        lookupObjects[a + (b << 8) + (c << 16)] = new Lookup() { o = o, rotation = 0, flip = flip };
        lookupObjects[c + (a << 8) + (b << 16)] = new Lookup() { o = o, rotation = 1, flip = flip };
        lookupObjects[b + (c << 8) + (a << 16)] = new Lookup() { o = o, rotation = 2, flip = flip };
        //lookupObjects[a + (b << 8) + (c << 16)] = new Lookup() { o = o, rotation = 0, flip = false };
    }

    Lookup GetLookup(int a, int b, int c) {
        int key = a + (b << 8) + (c << 16);
        if (lookupObjects.ContainsKey(key)) {
            return lookupObjects[key];
        }
        return new Lookup() { o = null };
	}

    

    public GameObject CreateTriangle(int a, int b, int c, HexVec position, Transform parent, bool uptriangle) {

        int min = System.Math.Min( System.Math.Min(a, b), c);

        Lookup lookup = GetLookup(a - min, b - min, c - min);

        HexVec offset = HexVec.Zero;
        float rotation = 0f;

        if (lookup.o == null) {
            return new GameObject();
		}
        
        if(lookup.rotation == 1) {
            rotation = 120f;
            offset = uptriangle ? HexVec.UpRight : HexVec.Right;
		} else if (lookup.rotation == 2) {
            rotation = 240f;
            offset = uptriangle ? HexVec.Right : HexVec.DownRight;
        }

        GameObject tri = Instantiate(lookup.o, (position + offset).ToVector3(0.2f * min), Quaternion.Euler(0, rotation + (uptriangle ? 60f : 120f), 0), parent);
        tri.transform.localScale = lookup.flip ? new Vector3(-1, 1, 1) : Vector3.one;
        return tri;
    }
}
