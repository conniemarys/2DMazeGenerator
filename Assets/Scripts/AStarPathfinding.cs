using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ConstrainedExecution;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class PathfindingCell
{   
    public int X;
    public int Y;
    public int gValue;
    public int hValue;
    public int fValue;
    public PathfindingCell prevCell;
    public NodeWalls walls;

    public PathfindingCell(int X, int Y, Node node)
    {
        this.X = X;
        this.Y = Y;
        this.gValue = int.MaxValue;
        this.hValue = 0;
        this.walls = node.nodeWalls;
    }
}

public class AStarPathfinding
{
    private PathfindingCell[,] maze;
    private int height;
    private int width;
    PathfindingCell startCell;
    public int count;

    public List<PathfindingCell> AStar(Node[,] mazeArray, int inputWidth, int inputHeight)
    {
        maze = new PathfindingCell[inputWidth, inputHeight];

        for (int x = 0; x < inputWidth; x++)
        {
            for (int y = 0; y < inputHeight; y++)
            {
                maze[x, y] = new PathfindingCell(x, y, mazeArray[x, y]);
            }
        }

        startCell = maze[0, 0];

        PathfindingCell endCell = maze[inputWidth - 1, inputHeight - 1];

        List<PathfindingCell> path = FindPath(maze, startCell, endCell);

        foreach (PathfindingCell node in path)
        {
            Debug.Log(node.X + ", " + node.Y);
        }

        return path;
    }

    public List<PathfindingCell> FindPath(PathfindingCell[,] maze, PathfindingCell startCell, PathfindingCell endCell)
    {
        List<PathfindingCell> exploredCells = new List<PathfindingCell>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                CalculateFCost(maze[x, y]);
                maze[x, y].prevCell = null;
            }
        }

        startCell.gValue = 0;
        CalculateHCost(startCell, endCell);

        List<PathfindingCell> cellsToExplore = new List<PathfindingCell> { startCell };

        var path = RecursiveCall(cellsToExplore, exploredCells, endCell);

        return path;
    }

    public List<PathfindingCell> RecursiveCall(List<PathfindingCell> cellsToExplore, List<PathfindingCell> exploredCells, PathfindingCell endCell)
    {
        if (cellsToExplore.Count > 0)
        {
            PathfindingCell currentCell = GetLowestFCost(cellsToExplore);

            if (currentCell == endCell)
            {
                return CalculatePath(endCell);
            }

            cellsToExplore.Remove(currentCell);
            exploredCells.Add(currentCell);

            List<PathfindingCell> neighbours = FindNeighbours(currentCell);

            bool checkValid = false;
            foreach (PathfindingCell neighbour in neighbours)
            {
                if (!exploredCells.Contains(neighbour))
                {
                    checkValid = true;
                }
            }

            if (checkValid)
            {
                int validCount = 0;

                foreach (PathfindingCell neighbour in neighbours)
                {
                    if (!exploredCells.Contains(neighbour))
                    {
                        validCount++;
                        int tentativeGValue = currentCell.gValue + 1;

                        if (tentativeGValue < neighbour.gValue)
                        {
                            neighbour.prevCell = currentCell;
                            neighbour.gValue = tentativeGValue;
                            CalculateHCost(neighbour, endCell);
                            CalculateFCost(neighbour);

                            cellsToExplore.Add(neighbour);
                        }
                    }

                }
                
            }
            
            return RecursiveCall(cellsToExplore, exploredCells, endCell);
        }

        return null;

    }
    public List<PathfindingCell> CalculatePath(PathfindingCell currentCell, List<PathfindingCell> path = null)
    {
        if (path == null)
        {
            path = new List<PathfindingCell>();
        }
        if (currentCell == startCell)
        {
            path.Add(currentCell);
            return path;
        }
        else
        {
            path.Add(currentCell);
            return CalculatePath(currentCell.prevCell, path);
        }

    }

    private PathfindingCell GetLowestFCost(List<PathfindingCell> cellsToExplore)
    {
        PathfindingCell lowestFCost = cellsToExplore[0];

        for (int i = 0; i < cellsToExplore.Count; i++)
        {
            if (cellsToExplore[i].fValue < lowestFCost.fValue)
            {
                lowestFCost = cellsToExplore[i];
            }
        }

        return lowestFCost;
    }

    public List<PathfindingCell> FindNeighbours(PathfindingCell cell)
    {
        List<PathfindingCell> result = new List<PathfindingCell>();

        if (!cell.walls.HasFlag(NodeWalls.Left))
        {
            result.Add(maze[cell.X - 1, cell.Y]);

        }
        if (!cell.walls.HasFlag(NodeWalls.Right))
        {

            result.Add(maze[cell.X + 1, cell.Y]);

        }
        if (!cell.walls.HasFlag(NodeWalls.Up))
        {
            result.Add(maze[cell.X, cell.Y + 1]);

        }
        if (!cell.walls.HasFlag(NodeWalls.Down))
        {

            result.Add(maze[cell.X, cell.Y - 1]);

        }

        return result;
    }

    public void CalculateHCost(PathfindingCell cell, PathfindingCell endPoint)
    {
        if (cell == null || endPoint == null)
        {
            Debug.Log("Null Object");
        }
        else
        {
            cell.hValue = (cell.X - endPoint.X) + (cell.Y - endPoint.Y);
        }
    }

    public void CalculateFCost(PathfindingCell cell)
    {
        cell.fValue = cell.hValue + cell.gValue;
    }
    
}

