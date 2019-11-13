using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RoadSystem 
{
    private static InfluenceMapper IM;
    
    [SerializeField] private List<Tuple<Vector2Int, Vector2Int>> _roads = new List<Tuple<Vector2Int, Vector2Int>>();

    public void Init(InfluenceMapper im)
    {
        IM = im;
        
        InfluenceMap roadMap = new InfluenceMap(MapType.Road, IM.Box,
            0f, 1f, 0.5f);
        IM.Maps.Add(MapType.Road, roadMap);
    }

    public void OnUpdate()
    {
        
    
    }

}