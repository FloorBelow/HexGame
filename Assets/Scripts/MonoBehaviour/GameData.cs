using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData : MonoBehaviour
{

    public static GameData g;
    public TerrainTypeData[] _terrainTypes;

    public Dictionary<Hex.TerrainType, TerrainTypeData> terrainTypes;

    public void Start()
    {
        g = this;
        terrainTypes = new Dictionary<Hex.TerrainType, TerrainTypeData>();
        for(int i = 0; i < _terrainTypes.Length; i++) {
            terrainTypes[_terrainTypes[i].type] = _terrainTypes[i];
		}
    }
}
