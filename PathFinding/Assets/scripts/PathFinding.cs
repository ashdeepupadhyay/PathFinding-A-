using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System;
using System.Linq;

public class PathFinding : MonoBehaviour {
    Grid grid;
    PathRequestManager requestManager;
    //public Transform seeker, target;
    private void Awake()
    {
        requestManager = GetComponent<PathRequestManager>();
        grid = GetComponent<Grid>();
    }
    /*
    void Update()
    {
        if (Input.GetButtonDown("Jump"))
        {
            UnityEngine.Debug.Log("finding path");
            FindPath(seeker.position, target.position);
        }
    }
    */
    IEnumerator FindPath(Vector3 startPos,Vector3 targetPos)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        Vector3[] wayPoints = new Vector3[0];
        bool pathSuccess = false;
        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);

        if (startNode.walkable && targetNode.walkable)
        {
            //List<Node> openSet = new List<Node>();
            Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
            HashSet<Node> closedSet = new HashSet<Node>();

            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                Node currentNode = openSet.RemoveFirst();
                /*
                Node currentNode = openSet[0];
                for(int i=1;i < openSet.Count; i++)
                {
                    if (openSet[i].fCost < currentNode.fCost||openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost)
                    {
                        currentNode = openSet[i];
                    }
                }
                openSet.Remove(currentNode);
                */
                closedSet.Add(currentNode);

                if (currentNode == targetNode)
                {
                    sw.Stop();
                    print("Path found " + sw.ElapsedMilliseconds + "ms");
                    pathSuccess = true;
                    //RetracePath(startNode, targetNode);
                    break;
                    //return;              
                }

                foreach (Node neigbour in grid.GetNeighbours(currentNode))
                {
                    if (!neigbour.walkable || closedSet.Contains(neigbour))
                    {
                        continue;
                    }

                    int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neigbour);
                    if (newMovementCostToNeighbour < neigbour.gCost || !openSet.Contains(neigbour))
                    {
                        neigbour.gCost = newMovementCostToNeighbour;
                        neigbour.hCost = GetDistance(neigbour, targetNode);
                        neigbour.parent = currentNode;
                        if (!openSet.Contains(neigbour))
                        {
                            openSet.Add(neigbour);
                        }
                    }
                }

            }

        }
        
        yield return null;
        if (pathSuccess)
        {
            wayPoints = RetracePath(startNode, targetNode);
        }
        requestManager.FinishedProcessingPath(wayPoints, pathSuccess);
    }

    private Vector3[] RetracePath(Node startNode,Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        Vector3[] waypoints = SimplifyPath(path);
        //waypoints.Reverse();
        Array.Reverse(waypoints);
        return waypoints;
        //path.Reverse();

        //grid.path = path;
    }
    Vector3[] SimplifyPath(List<Node> path)
    {
        List<Vector3> wayPoints = new List<Vector3>();
        Vector2 directionOld = Vector2.zero;
        for(int i = 1; i < path.Count; i++)
        {
            Vector2 directionNew = new Vector2(path[i - 1].gridX - path[i].gridX, path[i - 1].gridY - path[i].gridY);
            if (directionNew != directionOld)
            {
                wayPoints.Add(path[i].worldPositon);
            }
            directionOld = directionNew;
        }
        return wayPoints.ToArray();
    }
    int GetDistance(Node nodeA,Node nodeB)
    {
        int distX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int distY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (distX > distY)
        {
            return 14 * distY + 10 * (distX - distY);
        }
        else
        {
            return 14 * distX + 10 * (distY - distX);
        }
    }
    public void StartFindPath(Vector3 startPos,Vector3 targetPos)
    {
        StartCoroutine(FindPath(startPos,targetPos));
    }
}
