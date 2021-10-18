using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HexUtils;
using Priority_Queue;

using System.Linq;

public class World : MonoBehaviour {


    public int height0Weight;
    public int height1Weight;
    public int height2Weight;
    public int height3Weight;
    int height2Cum;
    int height3Cum;
    int heightCum;


    Dictionary<HexVec, Hex> hexes;
    HashSet<HexObject> hexObjects;

    HexObject testPlayerHexObj;

    public int testPlayerMovement;
    public GameObject testPlayerPrefab;
    public GameObject hexPrefab;
    public int mapRadius;


    public TerrainTriSetData testTris;
    //should be moved somewhere asap
    Camera cam;
    public float cameraSpeed;

    public HexVec selectedPos;
    GameObject selectedObjMovementFrontierLine;
    Dictionary<HexVec, Pathfind.SearchHex> selectedObjMovementFrontier;
    GameObject selectedObjMovementLine;

    public Material lineMat;

    HexVec mouseHex;
    bool mouseHexChanged;

    void Start() {

        BuildHeightWeights();

        selectedPos = new HexVec(int.MaxValue, int.MaxValue);

        cam = Camera.main;



        hexes = CreateHexes(mapRadius);

        /*
        hexes[new HexVec(3, 0)].SetTerrain(Hex.TerrainType.Forest);
        hexes[new HexVec(4, 0)].SetTerrain(Hex.TerrainType.Forest);
        hexes[new HexVec(2, 0)].SetTerrain(Hex.TerrainType.Forest);
        hexes[new HexVec(3, 1)].SetTerrain(Hex.TerrainType.Forest);
        hexes[new HexVec(3, -1)].SetTerrain(Hex.TerrainType.Forest);
        hexes[new HexVec(2, -1)].SetTerrain(Hex.TerrainType.Forest);
        hexes[new HexVec(2, -2)].SetTerrain(Hex.TerrainType.Forest);
        */

        Debug.Log(hexes.Count);

        testPlayerHexObj = CreateObject(testPlayerPrefab, HexVec.Zero, testPlayerMovement);


        testTris.BuildLookups();
        CreateTerrain();
        StartTurn();

        //HashSet<HexVec> area = new HashSet<HexVec>() { HexVec.Zero, HexVec.Right, HexVec.UpLeft, HexVec.UpLeft + HexVec.Left };
        //CreateHexOutline(area, HexVec.Zero);
        //CreateMovementFrontier(testPlayerHexObj);
        //TestCreateHexBlob(new HexVec(12, 0), 64);
    }

    void Update() {
        if(Input.GetKey(KeyCode.W)) cam.transform.position += Vector3.forward * cameraSpeed * Time.deltaTime;
        else if (Input.GetKey(KeyCode.S)) cam.transform.position += Vector3.back * cameraSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.A)) cam.transform.position += Vector3.left * cameraSpeed * Time.deltaTime;
        else if (Input.GetKey(KeyCode.D)) cam.transform.position += Vector3.right * cameraSpeed * Time.deltaTime;

        var ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100f, 1 << 6)) {
            HexVec newMouseHex = hit.collider.GetComponent<CoordComponent>().pos;
            if (mouseHex != newMouseHex) {
                mouseHex = newMouseHex;
                mouseHexChanged = true;
                if (selectedObjMovementLine != null) Destroy(selectedObjMovementLine);
            } else mouseHexChanged = false;
            //MoveObject(testPlayerHexObj, hit.collider.GetComponent<CoordComponent>().pos, false);
        }


        if (Input.GetMouseButton(0)) {
            UpdateSelectedHex(hit.collider.GetComponent<CoordComponent>().pos);
        } 
        //handle moving selected obj
        if(selectedObjMovementFrontier != null) {
            if (Input.GetMouseButton(1)) {

                //Don't draw line if out of range or 0 length (update for longer lengths later
                if (mouseHex == selectedPos || !selectedObjMovementFrontier.ContainsKey(mouseHex)) {
                    if (selectedObjMovementLine != null) Destroy(selectedObjMovementLine);
                } else {
                    if (selectedObjMovementLine == null || mouseHexChanged) {
                        selectedObjMovementLine = CreateMovementLine(mouseHex, selectedObjMovementFrontier);
                    }
                }
            } else if (Input.GetMouseButtonUp(1)) {
                if (selectedObjMovementFrontier.ContainsKey(mouseHex)) {
                    MoveObject(hexes[selectedPos].hexObj, mouseHex, false);
                    UpdateSelectedHex(mouseHex);
                    Destroy(selectedObjMovementLine);
				}

            }
        }
        

        
    }

    void BuildHeightWeights() {
        height2Cum = height0Weight + height1Weight;
        height3Cum = height2Cum + height2Weight;
        heightCum = height3Cum + height3Weight;
        Debug.Log($"{height0Weight} {height2Cum} {height3Cum} {heightCum}");
	}

    int GetRandHeight() {
        int roll = Random.Range(0, heightCum);
        if (roll >= height3Cum) return 3;
        if (roll >= height2Cum) return 2;
        if (roll >= height0Weight) return 1;
        return 0;
	}

    void UpdateSelectedHex(HexVec pos) {
        if (selectedPos == pos) return;
        if(selectedObjMovementFrontier != null) {
            selectedObjMovementFrontier = null;
            Destroy(selectedObjMovementFrontierLine);
		}

        if(hexes[pos].hexObj != null) {
            selectedObjMovementFrontier = Pathfind.Frontier(pos, hexes[pos].hexObj.moves, hexes);
            selectedObjMovementFrontierLine = CreateHexOutline(selectedObjMovementFrontier.Keys);
        }
        /*
        else {
            if (selectedObjMovementFrontierLine != null) {
                if(selectedObjMovementFrontier.ContainsKey(pos)) {
                    MoveObject(hexes[selectedPos].hexObj, pos, false);
                    selectedObjMovementFrontier = Pathfind.Frontier(pos, hexes[pos].hexObj.moves, hexes);
                    Destroy(selectedObjMovementFrontierLine);
                    selectedObjMovementFrontierLine = CreateHexOutline(selectedObjMovementFrontier.Keys);
                } else {
                    selectedObjMovementFrontier = null;
                    Destroy(selectedObjMovementFrontierLine);
                }
            }
		}
        */
        selectedPos = pos;
    }

    void TestCreateHexBlob(HexVec pos, int size) {
        HashSet<HexVec> blob = new HashSet<HexVec>();
        HashSet<HexVec> frontier = new HashSet<HexVec>();
        frontier.Add(pos);
        while(frontier.Count > 0 && blob.Count < size) {

            if((frontier.Count + blob.Count) > size) {
                var front = frontier.ToArray();
                for(int i = 0; i < size - blob.Count; i++) {
                    blob.Add(front[i]);
				}
                break;
			}

            HexVec newPos = frontier.ToArray()[Random.Range(0, frontier.Count)];
            frontier.Remove(newPos);
            blob.Add(newPos);
            for(int direction = 0; direction < 6; direction++) {
                HexVec v = newPos + HexVec.directions[direction];
                if (!blob.Contains(v)) frontier.Add(v);
			}
		}
        Debug.Log(blob.Count);
        Transform parent = new GameObject("blob" + pos.ToString()).transform;
        if (parent == null) Debug.Log("WHAT");
        foreach(HexVec hex in blob) {
			InstantiateHex(hex, parent, 0);
		}
	}



    void StartTurn() {
        foreach(var obj in hexObjects) {
            obj.ResetMovement();
		}
	}



    void CreateTerrain() {
        Transform parent = new GameObject("Triangles").transform;
        Dictionary<HexVec, GameObject> upTriangles = new Dictionary<HexVec, GameObject>(hexes.Count * 2);
        Dictionary<HexVec, GameObject> downTriangles = new Dictionary<HexVec, GameObject>(hexes.Count * 2);
        foreach (HexVec hexPos in hexes.Keys) {
            if (!hexes.ContainsKey(hexPos + HexVec.Right)) continue;
            int height = hexes[hexPos].height;
            int heightRight = hexes[hexPos + HexVec.Right].height;
            if (hexes.ContainsKey(hexPos + HexVec.UpRight)) {
                int heightUpRight = hexes[hexPos + HexVec.UpRight].height;
                GameObject tri = testTris.CreateTriangle(height, heightUpRight, heightRight, hexPos, parent, true);
#if UNITY_EDITOR
                tri.name = $"{height} {heightUpRight} {heightRight} up";
#endif
                upTriangles[hexPos] = tri;
            }
            if (hexes.ContainsKey(hexPos + HexVec.DownRight)) {
                int heightDownRight = hexes[hexPos + HexVec.DownRight].height;
                GameObject tri = testTris.CreateTriangle(height, heightRight, heightDownRight, hexPos, parent, false);


                #if UNITY_EDITOR
                tri.name = $"{height} {heightRight} {heightDownRight} down";
                #endif
                upTriangles[hexPos] = tri;
            }
        }
    }

    GameObject CreateMovementLine(HexVec pos, Dictionary<HexVec, Pathfind.SearchHex> pathfindData) {
        if (!pathfindData.ContainsKey(pos)) return null;
        List<Vector3> points = new List<Vector3>(8);
        while(pos != pathfindData[pos].parent) {
            points.Add(pos.ToVector3(0.8f));
            pos = pathfindData[pos].parent;
		}
        points.Add(pos.ToVector3(0.8f));
        LineRenderer line = new GameObject().AddComponent<LineRenderer>();
        line.gameObject.name = "LINEOBJ2";

        line.positionCount = points.Count;
        line.SetPositions(points.ToArray());

        line.SetWidth(0.1f, 0.1f);
        line.material = lineMat;

        return line.gameObject;
    }


    GameObject CreateHexOutline(IEnumerable<HexVec> hexes, float height = 0.61f) {
        HashSet<HexVec> leftBorderHexes = new HashSet<HexVec>();
        foreach(var hex in hexes) {
            if(!hexes.Contains(hex + HexVec.Right)) {
                leftBorderHexes.Add(hex);
			}
		}

        Transform parent = new GameObject("LINE").transform;

        int debug = 0;
        while (leftBorderHexes.Count > 0 && debug < 8192) {

            HexVec start = leftBorderHexes.ToArray()[0];
            leftBorderHexes.Remove(start);

            List<Vector3> points = new List<Vector3>();

            //look for start point
            //while(hexes.Contains(start + HexVec.Right)) {
            //    start += HexVec.Right;
            //}

            points.Add(start.ToVector3(height) + HexVec.hexPoints[0]);
            //points.Add(pos.ToVector3(0.1f) + HexVec.hexPoints[1]);


            HexVec pos = start;
            int direction = 1;
            do {
                if (direction < 3 && leftBorderHexes.Contains(pos)) leftBorderHexes.Remove(pos);
                debug++;
                points.Add(pos.ToVector3(height) + HexVec.hexPoints[direction]);
                if (hexes.Contains(pos + HexVec.directions[direction])) {
                    pos = pos + HexVec.directions[direction];
                    direction = (direction + 5) % 6;
                } else {
                    direction = (direction + 1) % 6;
                }
            } while (points[0] != points[points.Count - 1] && debug < 8192); //TODO why doesn't direction==0 && pos == start work sometimes? ANSWER = HOLES

            //int iter = 0;

            LineRenderer line = new GameObject().AddComponent<LineRenderer>();
            line.transform.SetParent(parent);
            line.gameObject.name = "LINEOBJ";

            line.positionCount = points.Count;
            line.SetPositions(points.ToArray());

            line.SetWidth(0.1f, 0.1f);
            line.loop = true;
            line.sharedMaterial = lineMat;
            
        }
        return parent.gameObject;

    }

    Dictionary<HexVec, Hex> CreateHexes(int radius) {
        hexes = new Dictionary<HexVec, Hex>(1024);
        hexObjects = new HashSet<HexObject>();
        Transform parent = new GameObject().transform;
        //hexes[HexVec.Zero] = InstantiateHex(HexVec.Zero, parent, 1, -0.1f);
        foreach(HexVec hex in HexVec.Circle(radius)) {
            int height = GetRandHeight();
            hexes[hex] = InstantiateHex(hex, parent, height, 0.2f * height);
        }
        return hexes;
    }

    Hex InstantiateHex(HexVec vec, Transform parent, int hexHeight, float height = 0f) {
        GameObject o = Instantiate(hexPrefab, vec.ToVector3(height), Quaternion.identity);
        Hex h = new Hex(o, hexHeight);
        o.transform.SetParent(parent);
        o.name = "Hex " + vec.ToString();
        CoordComponent c = o.GetComponent<CoordComponent>();
        c.pos = vec;
        c.hex = h;
        return h;
    }

    HexObject CreateObject(GameObject prefab, HexVec pos, int movement, int direction = 0) {
        Transform obj = Instantiate(prefab, pos.ToVector3(0.2f * hexes[pos].height), Quaternion.identity).transform;
        HexObject hexObj = new HexObject(pos, direction, obj, movement);
        hexObjects.Add(hexObj);
        hexes[pos].hexObj = hexObj;
        return hexObj;
    }

    void MoveObject(HexObject obj, HexVec pos, bool relative = true, bool costsMovement = false) {
        if (relative) pos += obj.pos;
        if (pos == obj.pos) return;
        if(!hexes.ContainsKey(pos)) {
            Debug.LogError("MOVING TO NONEXISTENT HEX");
            return;
		}
        hexes[obj.pos].hexObj = null;
        obj.pos = pos;
        obj.obj.position = pos.ToVector3(obj.obj.localScale.y / 2 + 0.2f * hexes[pos].height);
        hexes[pos].hexObj = obj;
	}
}
