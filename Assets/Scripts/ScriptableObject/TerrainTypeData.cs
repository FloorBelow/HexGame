using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class TerrainTypeData : ScriptableObject
{
    public Hex.TerrainType type;
    public Material mat;
    public int baseMovementCost;
}
