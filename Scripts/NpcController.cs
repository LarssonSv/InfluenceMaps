using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class NpcController
{
    [SerializeField] public List<Npc> Npcs = new List<Npc>();

    public void OnUpdate()
    {
        InfluenceMapper.MP.ResetMap("AiMap");
        
        foreach (Npc npc in Npcs)
        {
            InfluenceMapper.MP.CalculateMap();
            Vector2Int temp = npc.GridPosition;
            
            npc.Path = InfluenceAStar.GetPath(npc.GridPosition, npc.Goal).ToList();
            
            if (npc.Path.Count > 0)
            {
                for (int i = 0; i < npc.Path.Count; i++)
                {
                    InfluenceMapper.MP.DrawOnMap("AiMap",npc.Path[i].x , npc.Path[i].y, 0f);
                }
            }

            if (Vector3.Distance(npc.transform.position, npc.TargetLocation) < 0.1f)
            {
                if(npc.Path.Count > 0)
                    npc.Path.RemoveAt(0);
                
                if(npc.Path.Count < 1)
                    continue;
                
                npc.TargetLocation = InfluenceMapper.MP.DrawPos[0][npc.Path[0].x][npc.Path[0].y];
            }
            
            npc.transform.position =
                Vector3.Lerp(npc.transform.position, npc.TargetLocation, npc.Speed * Time.deltaTime);
            
            Debug.DrawLine(npc.transform.position, npc.TargetLocation, Color.magenta);
        }   
    }
    
    
}
