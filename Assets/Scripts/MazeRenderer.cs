using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeRenderer : MonoBehaviour
{
    [SerializeField]
    [Range(1, 50)]
    private int width;

    [SerializeField]
    [Range(1, 50)]
    private int height;

    static float size = 1f;

    [SerializeField]
    private Transform wallPrefab = null;

    [SerializeField]
    private Transform parent = null;

    [SerializeField]
    private Transform floorPrefab = null;

    [SerializeField]
    private Transform pathPrefab;

    private AStarPathfinding pathfindingManager;

    private System.Random random;
    private MazeAlgorithm mazeAlgorithm;

    public class Changes
    {
        public int X;
        public int Y;
        public WallSides wallSides;

        public Changes(int X, int Y, WallSides wallSides)
        {
            this.X = X;
            this.Y = Y;
            this.wallSides = wallSides;
        }
    }

    private void Start()
    {

        /*pathfindingManager = new AStarPathfinding();

        random = new System.Random();

        Camera.main.transform.position = new Vector3(width / 2, height / 2 - 1, - 10);
        Camera.main.orthographicSize = Mathf.Pow(height, 0.8f);
        var emptyMaze = MazeGenerator.GenerateEmpty(width, height);
        //DrawEmpty(emptyMaze);

        List<Cell> mazeOrder = new List<Cell>();
        WallSides[,] aStarMap = new WallSides[width, height];

        (mazeOrder, aStarMap) = MazeGenerator.Generate(width, height);

        //List<PathfindingCell> path = pathfindingManager.AStar(aStarMap, width, height);
        List<PathfindingCell> path = new List<PathfindingCell>();

        StartCoroutine(Draw(mazeOrder, path));*/

    }


    IEnumerator Draw(List<Cell> maze, List<PathfindingCell> path)
    {
        /* for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var cell = maze[x, y];
                var position = new Vector3(-width / 2 + x, -height / 2 + y);

                if(cell.HasFlag(WallSides.Up))
                {
                    var topWall = Instantiate(wallPrefab, transform) as Transform;
                    topWall.position = position + new Vector3(0, size / 2);
                }

                if(cell.HasFlag(WallSides.Left))
                {
                    var leftWall = Instantiate(wallPrefab, transform) as Transform;
                    leftWall.position = position + new Vector3(-size / 2, 0);
                    leftWall.eulerAngles = new Vector3(0, 0, 90);
                }

                if(x == width - 1)
                {
                    if(cell.HasFlag(WallSides.Right))
                    {
                        var rightWall = Instantiate(wallPrefab, transform) as Transform;
                        rightWall.position = position + new Vector3(size / 2, 0);
                        rightWall.eulerAngles = new Vector3(0, 0, 90);
                    }
                }

                if(y == 0)
                {
                    if(cell.HasFlag(WallSides.Down))   
                    {
                        var bottomWall = Instantiate(wallPrefab, transform) as Transform;
                        bottomWall.position = position + new Vector3(0, -size / 2);
                    }
                }
                yield return new WaitForSeconds(0.1f);
            }
        } */

        foreach (Cell cell in maze)
        {
            var cellWallSides = cell.CellWallSides;
            var cellPosition = cell.CellPosition;

            if (cellWallSides.HasFlag(WallSides.Up))
            {
                var topWall = Instantiate(wallPrefab, transform) as Transform;
                topWall.position = new Vector3(cellPosition.X, cellPosition.Y + size / 2);
            }

            if (cellWallSides.HasFlag(WallSides.Left))
            {
                var leftWall = Instantiate(wallPrefab, transform) as Transform;
                leftWall.position = new Vector3(cellPosition.X - size / 2, cellPosition.Y);
                leftWall.eulerAngles = new Vector3(0, 0, 90);
            }

             if (cellPosition.X == width - 1)
            {
                if (cellWallSides.HasFlag(WallSides.Right))
                {
                    var rightWall = Instantiate(wallPrefab, transform) as Transform;
                    rightWall.position = new Vector3(cellPosition.X + size / 2,cellPosition.Y);
                    rightWall.eulerAngles = new Vector3(0, 0, 90);
                }
            }

            if (cellPosition.Y == 0)
            {
                if (cellWallSides.HasFlag(WallSides.Down))
                {
                    var bottomWall = Instantiate(wallPrefab, transform) as Transform;
                    bottomWall.position = new Vector3(cellPosition.X , cellPosition.Y - size / 2);
                }
            }

            var floor = Instantiate(floorPrefab, transform);
            floor.position = new Vector3(cellPosition.X, cellPosition.Y, 1);

            yield return new WaitForSeconds(0.02f);
        }

        for(int i = path.Count - 1; i >= 0; i--)
        {
            var step = Instantiate(pathPrefab, transform) as Transform;
            step.position = new Vector3(path[i].X, path[i].Y, 0.9f);

            yield return new WaitForSeconds(0.02f);
        }
    }

    private void DrawEmpty(WallSides[,] maze)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var cell = maze[x, y];
                var position = new Vector3(-width / 2 + x, -height / 2 + y);

                var parentTransform = Instantiate(parent, transform) as Transform;

                if (cell.HasFlag(WallSides.Up))
                {
                    var topWall = Instantiate(wallPrefab, transform) as Transform;
                    topWall.position = position + new Vector3(0, size / 2);
                }

                if (cell.HasFlag(WallSides.Left))
                {
                    var leftWall = Instantiate(wallPrefab, transform) as Transform;
                    leftWall.position = position + new Vector3(-size / 2, 0);
                    leftWall.eulerAngles = new Vector3(0, 0, 90);
                }

                if (x == width - 1)
                {
                    if (cell.HasFlag(WallSides.Right))
                    {
                        var rightWall = Instantiate(wallPrefab, transform) as Transform;
                        rightWall.position = position + new Vector3(size / 2, 0);
                        rightWall.eulerAngles = new Vector3(0, 0, 90);
                    }
                }

                if (y == 0)
                {
                    if (cell.HasFlag(WallSides.Down))
                    {
                        var bottomWall = Instantiate(wallPrefab, transform) as Transform;
                        bottomWall.position = position + new Vector3(0, -size / 2);
                    }
                }

            }
        }

    }
}
