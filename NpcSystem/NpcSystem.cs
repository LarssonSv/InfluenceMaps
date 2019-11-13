using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class NpcSystem
{
    private static InfluenceMapper IM;

    [SerializeField] public Dictionary<int, Npc> Npcs = new Dictionary<int, Npc>();
    [SerializeField] public GameObject NpcPrefab;

    private int npcIndex = 0;

    public void Init(InfluenceMapper im)
    {
        IM = im;

        InfluenceMap npcMap = new InfluenceMap(MapType.Npc, IM.Box,
            0f, 1f, 1);
        IM.Maps.Add(MapType.Npc, npcMap);
    }

    public void OnUpdate()
    {
        IM.ResetMap(MapType.Npc);

        foreach (Npc npc in Npcs.Values)
        {
            IM.CalculateMap(); //this should be optimezed to only multiple updated npc map pos with the same grid from start of frame
            Vector2Int temp = npc.GridPosition;

            if (npc.Path.Count < 1 && npc.GridPosition != npc.Goal)
                npc.Path = InfluenceAStar.GetPath(npc.GridPosition, npc.Goal).ToList();
            else
                npc.Path = InfluenceAStar.RecalculatePath(npc.GridPosition, npc.Path);

            if (npc.Path.Count > 4)
            {
                for (int i = 0; i < 4; i++)
                {
                    IM.DrawOnMap(MapType.Npc, npc.Path[i].x, npc.Path[i].y, 0f);
                }
            }

            if (Vector3.Distance(npc.transform.position, npc.TargetLocation) < 0.1f)
            {
                if (npc.Path.Count > 0)
                    npc.Path.RemoveAt(0);

                if (npc.Path.Count < 1)
                    continue;

                //npc.TargetLocation = InfluenceMapper.MP.DrawPos[0][npc.Path[0].x][npc.Path[0].y];
                npc.TargetLocation = IM.GridToWorld(npc.Path[0]);
            }

            npc.transform.position =
                Vector3.Lerp(npc.transform.position, npc.TargetLocation, npc.Speed * Time.deltaTime);

            Debug.DrawLine(npc.transform.position, npc.TargetLocation, Color.magenta);
        }
    }

    public bool AddNpc(Vector2Int pos)
    {
        if (NpcPrefab != null)
        {
            GameObject clone = GameObject.Instantiate(NpcPrefab, IM.GridToWorld(new Vector2Int(pos.x, pos.y)),
                Quaternion.identity);

            npcIndex++;

            Npc data = clone.GetComponent<Npc>();
            data.ID = npcIndex;
            Npcs.Add(npcIndex, data);
            return true;
        }

        return false;
    }

    public bool SetPath(int npcID, Vector2Int target)
    {
        if (!Npcs.ContainsKey(npcID))
            return false;

        Npcs[npcID].Goal = target;
        return true;
    }
}