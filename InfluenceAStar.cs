using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class InfluenceAStar
{
    private static Dictionary<Vector2Int, bool> _closedSet = new Dictionary<Vector2Int, bool>();
    private static Dictionary<Vector2Int, bool> _openSet = new Dictionary<Vector2Int, bool>();
    private static Dictionary<Vector2Int, int> _gScore = new Dictionary<Vector2Int, int>();
    private static Dictionary<Vector2Int, int> _fScore = new Dictionary<Vector2Int, int>();
    private static Dictionary<Vector2Int, Vector2Int> _nodeLinks = new Dictionary<Vector2Int, Vector2Int>();

    public static float[][] GridValues;
    public static int sliceValue = 4;


    public static List<Vector2Int> RecalculatePath(Vector2Int start, List<Vector2Int> path, List<Vector2Int> exceptions = null)
    {
        if (path.Count < sliceValue)
            return path;
            
        Vector2Int end = new Vector2Int();
        end = path[sliceValue-1];
        
        path.RemoveRange(0, sliceValue);

        path.InsertRange(0, GetPath(start, end));

        return path;
    }

    public static bool ContainsPos (List<Vector2Int> exceptions, Vector2Int pos)
    {
        foreach (var x in exceptions)
        {
            if (x == pos)
                return true;
        }
        return false;
    }

    public static Vector2Int[] GetPath(Vector2Int start, Vector2Int end)
    {

        _nodeLinks.Clear();
        _fScore.Clear();
        _gScore.Clear();
        _openSet.Clear();
        _closedSet.Clear();

        _openSet[start] = true;
        _gScore[start] = 0;
        _fScore[start] = ManhattanDistance(start, end);

        while (_openSet.Count > 0)
        {
            Vector2Int current = GetNextBest();
            if (current.Equals(end))
                return CreatePath(current);

            _openSet.Remove(current);
            _closedSet[current] = true;
        
            List<Vector2Int> neighbors = GetNeighbors(current);

            foreach (Vector2Int node in neighbors)
            {
                if (_closedSet.ContainsKey(node))
                    continue;

                int projectedScoreG;
                
                //if (ContainsPos(exceptions, node))
                //    projectedScoreG = GetScoreG(current);
                //else
                    projectedScoreG = Mathf.RoundToInt(GetScoreG(current) * GridValues[current.x][current.y]);

                
                if (!_openSet.ContainsKey(node))
                    _openSet[node] = true;
                else if (projectedScoreG >= GetScoreG(node))
                    continue;

                _nodeLinks[node] = current;
                _gScore[node] = projectedScoreG;
                _fScore[node] = projectedScoreG + ManhattanDistance(node, end);
            }
        }

        Debug.Log("Could not find Path!");
        return new Vector2Int[0];
    }

    private static List<Vector2Int> GetNeighbors(Vector2Int toSample)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();

        if (toSample.x + 1 < GridValues.Length)
        {
            if (GridValues[toSample.x + 1][toSample.y] > 0.1f )
                neighbors.Add(new Vector2Int(toSample.x + 1, toSample.y));
        }

        if (toSample.x - 1 >= 0)
        {
            if (GridValues[toSample.x - 1][toSample.y] > 0.1f)
                neighbors.Add(new Vector2Int(toSample.x - 1, toSample.y));
        }

        if (toSample.y + 1 < GridValues[toSample.x].Length)
        {
            if (GridValues[toSample.x][toSample.y + 1] > 0.1f )
                neighbors.Add(new Vector2Int(toSample.x, toSample.y + 1));
        }

        if (toSample.y - 1 >= 0)
        {
            if (GridValues[toSample.x][toSample.y - 1] > 0.1f)
                neighbors.Add(new Vector2Int(toSample.x, toSample.y - 1));
        }

        return neighbors;
    }

    private static Vector2Int[] CreatePath(Vector2Int current)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        while (_nodeLinks.ContainsKey(current))
        {
            path.Add(current);
            current = _nodeLinks[current];
        }


        path.Reverse();
        return path.ToArray();
    }

    private static int ManhattanDistance(Vector2Int start, Vector2Int goal)
    {
        int dx = goal.x - start.x;
        int dy = goal.y - start.y;
        return Math.Abs(dx) + Math.Abs(dy);
    }

    private static Vector2Int GetNextBest()
    {
        int best = int.MaxValue;
        Vector2Int bestNode = new Vector2Int();

        foreach (Vector2Int node in _openSet.Keys)
        {
            int score = GetScoreF(node);
            if (score < best)
            {
                bestNode = node;
                best = score;
            }
        }

        return bestNode;
    }

    private static int GetScoreF(Vector2Int node)
    {
        int score = int.MaxValue;
        _fScore.TryGetValue(node, out score);
        return score;
    }

    private static int GetScoreG(Vector2Int node)
    {
        int score = int.MaxValue;
        _gScore.TryGetValue(node, out score);
        return score;
    }
}