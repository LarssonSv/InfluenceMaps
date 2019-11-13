using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HazardSystem
{
    private static InfluenceMapper IM;

    public List<Hazard> _hazards = new List<Hazard>();

    public void Init(InfluenceMapper mp)
    {
        IM = mp;

        InfluenceMap hazardMap = new InfluenceMap(MapType.Hazard, IM.Box,
            0f, 1f, 1);
        mp.Maps.Add(MapType.Hazard, hazardMap);
    }

    public void OnUpdate()
    {
        MoveHazard();
        IM.ResetMap(MapType.Hazard);

        foreach (Hazard hazard in _hazards)
        {
            if (!hazard.Modified)
                continue;

            Vector2Int newPos = IM.WorldToGrid(hazard.transform.position);
            IM.DrawCircleOnMap(MapType.Hazard, newPos.x, newPos.y, hazard.Radius, hazard.MapValue);
            hazard.Modified = false;
        }
    }

    public void MoveHazard()
    {
        foreach (Hazard hazard in _hazards)
        {
            //hazard.transform.Translate(Vector3.forward * Time.deltaTime);
            hazard.Modified = true;
        }
    }
}