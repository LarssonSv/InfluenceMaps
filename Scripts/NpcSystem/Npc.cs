using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;

[System.Serializable]
public class Npc : MonoBehaviour
{
    public Vector2Int GridPosition => InfluenceMapper.IM.WorldToGrid(transform.position);
    public Vector3 TargetLocation;
    public List<Vector2Int> Path = new List<Vector2Int>();
    public float Speed = 2f;
    public int StepsTaken = 0;
    public Vector2Int Goal;
    public int ID = 0;

    private void Update()
    {
        for (int i = 1; i < Path.Count; i++)
        {
            Debug.DrawLine(InfluenceMapper.IM.GridToWorld(Path[i-1]), InfluenceMapper.IM.GridToWorld(Path[i]), Color.magenta);
        }
    }

    private void OnEnable()
    {
        TargetLocation = transform.position;
        Goal = InfluenceMapper.IM.WorldToGrid(transform.position);
    }
}
