using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class MazeAlgorithm : MonoBehaviour
{
    [SerializeField]
    public Node nodePrefab;

    [SerializeField]
    public GameObject pathPrefab;

    [SerializeField]
    public int width = 10;
    public int height = 10;

    public Node[,] nodeMap;

    AStarPathfinding aStar;

    [SerializeField]
    float speed = 0f;

    public void Awake()
    {
        Camera.main.transform.position = new Vector3(width / 2, height / 2, -10f);
        Camera.main.orthographicSize = height * 0.7f;

        aStar = new AStarPathfinding();

        StartCoroutine(MazeBacktracker(width, height));

    }

    IEnumerator MazeBacktracker(int width, int height)
    {
        nodeMap = new Node[width, height];

        //create the nodes
        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                Node node = Instantiate(nodePrefab, new Vector2(x, y), Quaternion.identity, transform);
                node.X = x;
                node.Y = y;
                node.Init();
                node.ChangePrefab();
                nodeMap[x, y] = node;
            }
        }

        List<Node> currentPath = new List<Node>();
        List<Node> completedNodes = new List<Node>();

        //Choosing start node at random
        currentPath.Add(nodeMap[Random.Range(0, width), Random.Range(0, height)]);

        currentPath[0].nodeState = NodeState.Visited;

        Node currentNode = currentPath[0];

        while(completedNodes.Count <= width*height)
        {
            List<Node> neighbours = FindNeighbours(currentNode, nodeMap, width, height);

            if(neighbours.Count > 0)
            {
                Node nextNode = neighbours[Random.Range(0, neighbours.Count)];

                KnockDownWalls(currentNode, nextNode);

                nextNode.nodeState = NodeState.Visited;

                currentPath.Add(nextNode);

                currentNode.ChangePrefab();
                nextNode.ChangePrefab();

                currentNode = nextNode;

                yield return new WaitForSeconds(speed);
            }
            else if(neighbours.Count == 0)
            {
                completedNodes.Add(currentNode);

                currentNode.nodeState = NodeState.Completed;

                currentPath.Remove(currentNode);

                currentNode.ChangePrefab();

                if (completedNodes.Count == width * height)
                {
                    break;
                }
                else
                {
                    currentNode = currentPath[currentPath.Count - 1];

                    yield return new WaitForSeconds(speed);
                }
            }
        }

        List<PathfindingCell> path = aStar.AStar(nodeMap, width, height);
        StartCoroutine(DrawPath(path));

    }

    private List<Node> FindNeighbours(Node currentNode, Node[,] nodeMap, int width, int height)
    {
        List<Node> neighbours = new List<Node>();

        if(currentNode.X > 0)
        {
            if (nodeMap[currentNode.X - 1, currentNode.Y].nodeState.HasFlag(NodeState.Available))
            {
                neighbours.Add(nodeMap[currentNode.X - 1, currentNode.Y]);
            }
        }
        if (currentNode.X < width - 1)
        {
            if (nodeMap[currentNode.X + 1, currentNode.Y].nodeState.HasFlag(NodeState.Available))
            {
                neighbours.Add(nodeMap[currentNode.X + 1, currentNode.Y]);
            }
        }
        if (currentNode.Y > 0)
        {
            if (nodeMap[currentNode.X, currentNode.Y - 1].nodeState.HasFlag(NodeState.Available))
            {
                neighbours.Add(nodeMap[currentNode.X, currentNode.Y - 1]);
            }
        }
        if (currentNode.Y < height - 1)
        {
            if (nodeMap[currentNode.X, currentNode.Y + 1].nodeState.HasFlag(NodeState.Available))
            {
                neighbours.Add(nodeMap[currentNode.X, currentNode.Y + 1]);
            }
        }

        return neighbours;
    }

    private void KnockDownWalls(Node currentNode, Node nextNode)
    {
        if(nextNode.X == currentNode.X + 1)
        {
            currentNode.nodeWalls &= ~NodeWalls.Right;
            nextNode.nodeWalls &= ~NodeWalls.Left;
        }
        else if(nextNode.X == currentNode.X - 1)
        {
            currentNode.nodeWalls &= ~NodeWalls.Left;
            nextNode.nodeWalls &= ~NodeWalls.Right;
        }
        else if(nextNode.Y == currentNode.Y + 1)
        {
            currentNode.nodeWalls &= ~NodeWalls.Up;
            nextNode.nodeWalls &= ~NodeWalls.Down;
        }
        else if (nextNode.Y == currentNode.Y - 1)
        {
            currentNode.nodeWalls &= ~NodeWalls.Down;
            nextNode.nodeWalls &= ~NodeWalls.Up;
        }
        else
        {
            Debug.Log("Couldn't find wall in KnockDownwalls()");
        }
    }

    IEnumerator DrawPath(List<PathfindingCell> path)
    {
        for (int x = path.Count - 1; x >= 0; x--)
        {
            GameObject circle = Instantiate(pathPrefab);
            circle.transform.position = new Vector2(path[x].X, path[x].Y);
            yield return new WaitForSeconds(speed);
        }
    }
}
