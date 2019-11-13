#pragma warning disable 0649
using System;
using System.Collections.Generic;
using UnityEngine;

public enum MapType
{
    Navigation,
    Hazard,
    Npc,
    Road
}

public class InfluenceMapper : MonoBehaviour
{
    [Header("Debug:")] 
    [SerializeField] private Mesh _debugMesh;

    [Header("Map Systems:")]
    [SerializeField] public HazardSystem HazardSystem;
    [SerializeField] public NpcSystem NpcSystem;
    [SerializeField] public RoadSystem RoadSystem;
    [SerializeField] public NavigationSystem NavigationSystem;

    
    public readonly Dictionary<MapType, InfluenceMap> Maps = new Dictionary<MapType, InfluenceMap>();
    private readonly List<Vector3[][]> _drawPos = new List<Vector3[][]>();

    private readonly List<float[][]>
        _drawMaps = new List<float[][]>(); //This is a bit dumb, just so we dont get null when in editor

    public BoundingBox Box;
    public static InfluenceMapper IM;

    public void Awake()
    {
        IM = this;
        Init();
    }

    public void Init()
    {
        Box = new BoundingBox(Vector3.down, 50, 10, 50); //Todo: Make this take values from MapGenerator
        
        HazardSystem.Init(this);
        NpcSystem.Init(this);
        RoadSystem.Init(this);
        NavigationSystem.Init(this);
        
        float[][] finalGrid = new float[Box.ScaleX][];
        for (int i = 0;
            i < Box.ScaleX;
            i++)
        {
            finalGrid[i] = new float[Box.ScaleZ];
        }

        finalGrid = GetMap(MapType.Navigation).Multiply(GetMap(MapType.Hazard).Grid);
        _drawMaps.Add(finalGrid);
        _drawPos.Add(GetMap(MapType.Navigation).GridPosition);
    }

    private void Update()
    {
        NavigationSystem.OnUpdate();
        HazardSystem.OnUpdate();
        RoadSystem.OnUpdate();
        NpcSystem.OnUpdate();
        CalculateMap();
    }
    

    public void CalculateMap()
    {
        _drawMaps[0] = GetMap(MapType.Navigation).Multiply(GetMap(MapType.Road).Multiply(GetMap(MapType.Hazard).Multiply(GetMap(MapType.Npc).Grid)));
        InfluenceAStar.GridValues = _drawMaps[0];
    }

    #region Debug

    private void DrawRayCasts()
    {
        for (int x = 0; x < Box.ScaleX; x++)
        {
            for (int z = 0; z < Box.ScaleZ; z++)
            {
                Vector3 rayOrigin = new Vector3(Box.Min.x + x, Box.Max.y, Box.Min.z + z);

                ExtDebug.DrawBoxCastBox(rayOrigin, new Vector3(0.5f, 0.5f, 0.5f), Quaternion.identity, Vector3.down,
                    10f,
                    Color.blue);
            }
        }
    }

    #endregion

    #region DrawToMap

    public void DrawCircleOnMap(MapType type, int x, int z, int radius, float value)
    {
        InfluenceMap map = GetMap(type);
        for (int i = radius;
            i >= 0;
            i--)
        {
            float angle = 0;
            while (angle < 360)
            {
                float newX = x + i * Mathf.Cos(angle);
                float newZ = z + i * Mathf.Sin(angle);

                if (newX > 0 && newX < Box.ScaleX - 1 && newZ > 0 && newZ < Box.ScaleZ - 1)
                    map.Grid[Mathf.RoundToInt(newX)][Mathf.RoundToInt(newZ)] = value * i;
                angle += 10f;
            }
        }
    }

    public void DrawOnMap(MapType type, int x, int z, float value)
    {
        GetMap(type).Grid[x][z] = value;
    }

    #endregion

    #region Helpers

    public Vector2Int WorldToGrid(Vector3 pos)
    {
        Vector3 localPos = pos - Box.Min;
        return new Vector2Int(Mathf.RoundToInt(localPos.x), Mathf.RoundToInt(localPos.z));
    }

    public Vector3 GridToWorld(Vector2Int pos)
    {
        return _drawPos[0][pos.x][pos.y];
    }

    public void ResetMap(MapType type)
    {
        GetMap(type).Reset();
    }

    private InfluenceMap GetMap(MapType type)
    {
        if (Maps.ContainsKey(type))
            return Maps[type];


        Debug.Log("Could not find map!");
        return null;
    }

    #endregion

    private void OnDrawGizmos()
    {
//Draw Map
        Gizmos.color = new Color(0, 1, 0, 0.5F);
        foreach (float[][] drawMap in _drawMaps)
        {
            for (int x = 0; x < Box.ScaleX; x++)
            {
                for (int z = 0; z < Box.ScaleZ; z++)
                {
                    if (drawMap[x][z] > 0.1f)
                    {
                        Gizmos.color = new Color(0, 1, 0, drawMap[x][z] / 2f);
                        Gizmos.DrawMesh(_debugMesh, _drawPos[0][x][z] + new Vector3(0, 0.1f, 0),
                            Quaternion.Euler(90, 0, 0));
                    }
                }
            }
        }

        if (Box == null)
            return;

//Draw Box
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(Box.Min, Box.Min + new Vector3(Box.ScaleX, 0f, 0f));
        Gizmos.DrawLine(Box.Min + new Vector3(Box.ScaleX, 0f, 0f),
            Box.Min + new Vector3(Box.ScaleX, 0f, Box.ScaleZ));
        Gizmos.DrawLine(Box.Min, Box.Min + new Vector3(0, 0, Box.ScaleZ));
        Gizmos.DrawLine(Box.Min + new Vector3(0, 0, Box.ScaleZ),
            Box.Min + new Vector3(Box.ScaleX, 0, Box.ScaleZ));
        Gizmos.DrawLine(Box.Max, Box.Max - new Vector3(Box.ScaleX, 0f, 0f));
        Gizmos.DrawLine(Box.Max - new Vector3(Box.ScaleX, 0f, 0f),
            Box.Max - new Vector3(Box.ScaleX, 0f, Box.ScaleZ));
        Gizmos.DrawLine(Box.Max, Box.Max - new Vector3(0, 0, Box.ScaleZ));
        Gizmos.DrawLine(Box.Max - new Vector3(0, 0, Box.ScaleZ),
            Box.Max - new Vector3(Box.ScaleX, 0, Box.ScaleZ));
        Gizmos.DrawLine(Box.Min, Box.Max - new Vector3(Box.ScaleX, 0, Box.ScaleZ));
        Gizmos.DrawLine(Box.Min + new Vector3(Box.ScaleX, 0, 0), Box.Max - new Vector3(0, 0, Box.ScaleZ));
        Gizmos.DrawLine(Box.Min + new Vector3(0, 0, Box.ScaleZ), Box.Max - new Vector3(Box.ScaleX, 0, 0));
        Gizmos.DrawLine(Box.Min + new Vector3(Box.ScaleX, 0, Box.ScaleZ), Box.Max);
    }
}