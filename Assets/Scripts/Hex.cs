using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HexUtils;

[System.Serializable]
public class Hex
{
    public enum TerrainType {
        Plain,
        Forest,
        Impassable
    }

    public TerrainType terrain;
    public GameObject hexTile;
    public HexObject hexObj; //feels kinda oopish
    public int height;

    public void SetTerrain(TerrainType type) {
        terrain = type;
        hexTile.GetComponent<MeshRenderer>().sharedMaterial = GameData.g.terrainTypes[terrain].mat;
    }

    public int GetMovementCost(int height) {
        int heightDif = System.Math.Abs(height - this.height);
        if (heightDif > 1) return 16384;
        //if (heightDif == 1) return 2;
        return 1;
        return GameData.g.terrainTypes[terrain].baseMovementCost;
    }

    public Hex(GameObject tile, int height) {
        hexTile = tile;
        terrain = TerrainType.Plain;
        this.height = height;
    }
}

//not every object should have a direction
//inheritance?? interface??
public class HexObject {
    public HexVec pos;
    public int direction;
    public Transform obj;
    public int movement;
    public int moves;

    public HexObject(HexVec pos, int direction, Transform obj, int movement) {
        this.pos = pos;
        this.direction = direction;
        this.obj = obj;
        this.movement = movement;
    }

    public void ResetMovement() { moves = movement; }
}
