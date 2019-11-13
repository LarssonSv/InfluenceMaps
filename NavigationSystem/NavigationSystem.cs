using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NavigationSystem 
{
    private static InfluenceMapper IM;
    
    [SerializeField] private List<Tuple<Vector2Int, Vector2Int>> _roads = new List<Tuple<Vector2Int, Vector2Int>>();

    public void Init(InfluenceMapper im)
    {
        IM = im;
        
        InfluenceMap navigationMap = new InfluenceMap(MapType.Navigation, IM.Box,
            0f, 1f, 0, LayerMask.GetMask("Default"));
        IM.Maps.Add(MapType.Navigation, navigationMap);
    }

    public void OnUpdate()
    {
        
    
    }

}