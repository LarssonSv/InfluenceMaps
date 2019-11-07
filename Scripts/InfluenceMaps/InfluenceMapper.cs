using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InfluenceMapper : MonoBehaviour
{
    [SerializeField] private Mesh _debugMesh;
    [SerializeField] private HazardSystem _hazardSystem;
    [SerializeField] private NpcController _npcController;

    private readonly List<InfluenceMap> _maps = new List<InfluenceMap>();
    public readonly List<float[][]> _drawMaps = new List<float[][]>();
    public readonly List<Vector3[][]> DrawPos = new List<Vector3[][]>();

    private BoundingBox _box;
    public static InfluenceMapper MP;

    private void Awake()
    {
        MP = this;

        _hazardSystem.Init();

        CreateMap();

        Invoke("NpcTest", 1f);
    }

    private void Update()
    {
        _hazardSystem.OnUpdate(); 
        _npcController.OnUpdate();
        CalculateMap();
    }

    private void DrawRayCasts()
    {
        for (int x = 0; x < _box.ScaleX; x++)
        {
            for (int z = 0; z < _box.ScaleZ; z++)
            {
                Vector3 rayOrigin = new Vector3(_box.Min.x + x, _box.Max.y, _box.Min.z + z);
            
                ExtDebug.DrawBoxCastBox(rayOrigin, new Vector3(0.5f, 0.5f, 0.5f), Quaternion.identity, Vector3.down, 10f,
                    Color.blue);
            }
        }
    }


    private void NpcTest()
    {
        InfluenceAStar.GridValues = _drawMaps[0];
        _npcController.Npcs[0].Path = InfluenceAStar.GetPath(
            _npcController.Npcs[0].GridPosition, new Vector2Int(20, 5)).ToList();
        
        _npcController.Npcs[0].TargetLocation = _npcController.Npcs[0].transform.position;
        _npcController.Npcs[0].Goal = new Vector2Int(15,5);
        
        _npcController.Npcs[1].Path = InfluenceAStar.GetPath(
            _npcController.Npcs[1].GridPosition, new Vector2Int(15, 7)).ToList();
        
        _npcController.Npcs[1].TargetLocation = _npcController.Npcs[1].transform.position;
        _npcController.Npcs[1].Goal = new Vector2Int(17,7);
        
    }


    private void CreateMap()
    {
        _box = new BoundingBox(Vector3.zero, 25, 10, 25);

        //NavMap
        InfluenceMap Map = new InfluenceMap("NavMap", _box,
            0f, 1f, 0, 0, true);
        _maps.Add(Map);

        //HazardMap
        InfluenceMap HazardMap = new InfluenceMap("HazardMap", _box,
            0f, 1f, 1, 0, false);
        _maps.Add(HazardMap);
        
        //HazardMap
        InfluenceMap AiMap = new InfluenceMap("AiMap", _box,
            0f, 1f, 1, 0, false);
        _maps.Add(AiMap);
        
        
        //Init FinalGrid 
        float[][] finalGrid = new float[_box.ScaleX][];
        for (int i = 0;
            i < _box.ScaleX;
            i++)
        {
            finalGrid[i] = new float[_box.ScaleZ];
        }
        finalGrid = GetMapByName("NavMap").Multiply(GetMapByName("HazardMap").Grid);
        _drawMaps.Add(finalGrid);
        DrawPos.Add(GetMapByName("NavMap").GridPosition);
    }

    public void CalculateMap()
    {
        _drawMaps[0] = GetMapByName("NavMap").Multiply(GetMapByName("HazardMap").Multiply(GetMapByName("AiMap").Grid));
        InfluenceAStar.GridValues = _drawMaps[0];
    }

    public void ResetMap(string mapName)
    {
        GetMapByName(mapName).Reset();
    }

    public void DrawCircleOnMap(string mapName, int x, int z, int radius, float value)
    {
        InfluenceMap map = GetMapByName(mapName);
        for (int i = radius;
            i >= 0;
            i--)
        {
            float angle = 0;
            while (angle < 360)
            {
                float newX = x + i * Mathf.Cos(angle);
                float newZ = z + i * Mathf.Sin(angle);

                if (newX > 0 && newX < _box.ScaleX - 1 && newZ > 0 && newZ < _box.ScaleZ - 1)
                    map.Grid[Mathf.RoundToInt(newX)][Mathf.RoundToInt(newZ)] = value * i;
                angle += 10f;
            }
        }
    }

    public void DrawOnMap(string mapName, int x, int z, float value)
    {
        GetMapByName(mapName).Grid[x][z] = value;
    }

    public Vector2Int WorldToGrid(Vector3 pos)
    {
        Vector3 localPos = pos - _box.Min;
        return new Vector2Int(Mathf.RoundToInt(localPos.x), Mathf.RoundToInt(localPos.z));
    }
    
    public Vector3 GridToWorld(Vector2Int pos)
    {
        return DrawPos[0][pos.x][pos.y];
    }

    private InfluenceMap GetMapByName(string name)
    {
        foreach (InfluenceMap map in _maps)
        {
            if (name == map._name)
                return map;
        }

        Debug.Log("Could not find map!");
        return null;
    }


    private void OnDrawGizmos()
    {
//Draw Map
        Gizmos.color = new Color(0, 1, 0, 0.5F);
        foreach (float[][] drawMap in _drawMaps)
        {
            for (int x = 0; x < _box.ScaleX; x++)
            {
                for (int z = 0; z < _box.ScaleZ; z++)
                {
                    if (drawMap[x][z] > 0.1f)
                    {
                        Gizmos.color = new Color(0, 1, 0, drawMap[x][z] / 2f);
                        Gizmos.DrawMesh(_debugMesh, DrawPos[0][x][z] + new Vector3(0, 0.1f, 0),
                            Quaternion.Euler(90, 0, 0));
                    }
                }
            }
        }

        if (_box == null)
            return;

//Draw Box
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(_box.Min, _box.Min + new Vector3(_box.ScaleX, 0f, 0f));
        Gizmos.DrawLine(_box.Min + new Vector3(_box.ScaleX, 0f, 0f),
            _box.Min + new Vector3(_box.ScaleX, 0f, _box.ScaleZ));
        Gizmos.DrawLine(_box.Min, _box.Min + new Vector3(0, 0, _box.ScaleZ));
        Gizmos.DrawLine(_box.Min + new Vector3(0, 0, _box.ScaleZ),
            _box.Min + new Vector3(_box.ScaleX, 0, _box.ScaleZ));
        Gizmos.DrawLine(_box.Max, _box.Max - new Vector3(_box.ScaleX, 0f, 0f));
        Gizmos.DrawLine(_box.Max - new Vector3(_box.ScaleX, 0f, 0f),
            _box.Max - new Vector3(_box.ScaleX, 0f, _box.ScaleZ));
        Gizmos.DrawLine(_box.Max, _box.Max - new Vector3(0, 0, _box.ScaleZ));
        Gizmos.DrawLine(_box.Max - new Vector3(0, 0, _box.ScaleZ),
            _box.Max - new Vector3(_box.ScaleX, 0, _box.ScaleZ));
        Gizmos.DrawLine(_box.Min, _box.Max - new Vector3(_box.ScaleX, 0, _box.ScaleZ));
        Gizmos.DrawLine(_box.Min + new Vector3(_box.ScaleX, 0, 0), _box.Max - new Vector3(0, 0, _box.ScaleZ));
        Gizmos.DrawLine(_box.Min + new Vector3(0, 0, _box.ScaleZ), _box.Max - new Vector3(_box.ScaleX, 0, 0));
        Gizmos.DrawLine(_box.Min + new Vector3(_box.ScaleX, 0, _box.ScaleZ), _box.Max);
    }
}