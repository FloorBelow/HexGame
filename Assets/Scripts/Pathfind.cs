using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HexUtils;
using UnityEngine;
using Priority_Queue;

static class Pathfind {

    public struct SearchHex {
        public int distance;
        public HexVec parent;
    }

    public class HexQueueNode : FastPriorityQueueNode {
        public HexVec hex;

        public HexQueueNode(HexVec hex) { this.hex = hex; }
	}

    public static Dictionary<HexVec, SearchHex> Frontier(HexVec pos, int distance, Dictionary<HexVec, Hex> hexes) {


        Dictionary<HexVec, SearchHex> pathfindData = new Dictionary<HexVec, SearchHex>(128);
        pathfindData[pos] = new SearchHex { distance = 0, parent = pos };


        FastPriorityQueue<HexQueueNode> frontier = new FastPriorityQueue<HexQueueNode>(256);
    
        frontier.Enqueue(new HexQueueNode(pos) , 0);
        while (frontier.Count != 0) {
#if UNITY_EDITOR
            if (frontier.Count > 240) Debug.LogWarning("PATHFIND FRONTIER MIGHT GO OVER");
#endif
            HexVec hex = frontier.Dequeue().hex;
            Expand(hex, hex + HexVec.Right, pathfindData, frontier, hexes, distance);
            Expand(hex, hex + HexVec.UpRight, pathfindData, frontier, hexes, distance);
            Expand(hex, hex + HexVec.UpLeft, pathfindData, frontier, hexes, distance);
            Expand(hex, hex + HexVec.Left, pathfindData, frontier, hexes, distance);
            Expand(hex, hex + HexVec.DownLeft, pathfindData, frontier, hexes, distance);
            Expand(hex, hex + HexVec.DownRight, pathfindData, frontier, hexes, distance);
        }
        return pathfindData;
    }

    static void Expand(HexVec pos, HexVec newPos, Dictionary<HexVec, SearchHex> pathfindData, FastPriorityQueue<HexQueueNode> frontier, Dictionary<HexVec, Hex> hexes, int maxDistance) {
        if (!hexes.ContainsKey(newPos)) return;
        int newDistance = pathfindData[pos].distance + hexes[newPos].GetMovementCost(hexes[pos].height);
        if (newDistance > maxDistance) return;
        if (!pathfindData.ContainsKey(newPos) || pathfindData[newPos].distance > newDistance) {
            pathfindData[newPos] = new SearchHex { distance = newDistance, parent = pos };
            frontier.Enqueue(new HexQueueNode(newPos), newDistance);
        }
    }
}
