using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PathEditor : MonoBehaviour
{
    public List<Transform> roadNodes; // Các điểm road
    public float nodeConnectionRadius = 10f; // Khoảng cách tối đa để nối các node
    public Transform finalTarget;

    private Dictionary<Transform, List<Transform>> graph = new();

    private void Awake()
    {
        BuildGraph();
    }

    void BuildGraph()
    {
        graph.Clear();
        foreach (Transform node in roadNodes)
        {
            graph[node] = new List<Transform>();

            foreach (Transform otherNode in roadNodes)
            {
                if (node == otherNode) continue;
                float dist = Vector3.Distance(node.position, otherNode.position);
                if (dist <= nodeConnectionRadius)
                {
                    graph[node].Add(otherNode);
                }
            }
        }
    }

    public List<Transform> FindPath(Transform start)
    {
        // Tìm node gần nhất tới Final Target
        Transform nearestToTarget = roadNodes.OrderBy(n => Vector3.Distance(n.position, finalTarget.position)).FirstOrDefault();

        return DijkstraPath(start, nearestToTarget);
    }

    List<Transform> DijkstraPath(Transform start, Transform end)
    {
        Dictionary<Transform, float> dist = new();
        Dictionary<Transform, Transform> prev = new();
        List<Transform> unvisited = new(roadNodes);

        foreach (Transform node in roadNodes)
        {
            dist[node] = float.MaxValue;
            prev[node] = null;
        }

        dist[start] = 0f;

        while (unvisited.Count > 0)
        {
            Transform current = unvisited.OrderBy(n => dist[n]).First();
            unvisited.Remove(current);

            if (current == end)
                break;

            foreach (Transform neighbor in graph[current])
            {
                float alt = dist[current] + Vector3.Distance(current.position, neighbor.position);
                if (alt < dist[neighbor])
                {
                    dist[neighbor] = alt;
                    prev[neighbor] = current;
                }
            }
        }

        // Build path
        List<Transform> path = new();
        for (Transform at = end; at != null; at = prev[at])
        {
            path.Insert(0, at);
        }

        return path;
    }
}
