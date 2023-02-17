using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public enum NodeWalls
{
    Left = 1, //00010
    Right = 2, //00100
    Up = 4, //01000
    Down = 8 //10000
}

public enum NodeState
{
    Available = 1,
    Visited = 2,
    Completed = 4
}

public class Node : MonoBehaviour
{
    public NodeWalls nodeWalls;
    public NodeState nodeState;

    public int X;
    public int Y;

    [SerializeField]
    GameObject[] walls;

    [SerializeField]
    Color completed;
    [SerializeField]
    Color available;
    [SerializeField]
    Color visited;

    public SpriteRenderer floor;

    public Node()
    {
        this.nodeWalls = NodeWalls.Up | NodeWalls.Down | NodeWalls.Left | NodeWalls.Right;
        this.nodeState = NodeState.Available;
        X = 0;
        Y = 0;
    }

    public void Init()
    {
        this.floor = this.transform.Find("Square").GetComponent<SpriteRenderer>();

        Transform up = this.transform.Find("WallUp");
        Transform down = this.transform.Find("WallDown");
        Transform left = this.transform.Find("WallLeft");
        Transform right = this.transform.Find("WallRight");

        this.walls = new GameObject[] { up.GameObject(), down.GameObject(), left.GameObject(), right.GameObject() };
    }

    public void ChangePrefab()
    {

        switch (nodeState)
        {
            case NodeState.Available:
                floor.color = available;
                break;
            case NodeState.Visited:
                floor.color = visited;
                break;
            case NodeState.Completed:
                floor.color = completed;
                break;
            default:
                break;
        }

        if (!nodeWalls.HasFlag(NodeWalls.Up) && walls[0].activeSelf)
        {
            walls[0].SetActive(false);
        }

        if (!nodeWalls.HasFlag(NodeWalls.Down) && walls[1].activeSelf)
        {
            walls[1].SetActive(false);
        }

        if (!nodeWalls.HasFlag(NodeWalls.Left) && walls[2].activeSelf)
        {
            walls[2].SetActive(false);
        }

        if (!nodeWalls.HasFlag(NodeWalls.Right) && walls[3].activeSelf)
        {
            walls[3].SetActive(false);
        }
    }
}
