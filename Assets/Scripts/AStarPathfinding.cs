using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ConstrainedExecution;
using UnityEngine;
using UnityEngine.Rendering;

public class PathfindingCell
{
    public int X;
    public int Y;
    public int gValue;
    public int hValue;
    public int fValue;
    public PathfindingCell prevCell;
    public Cell correspondingCell;
    public PathfindingCell(int X, int Y, Cell cell)
    {
        this.X = X;
        this.Y = Y;
        this.gValue = int.MaxValue;
        this.correspondingCell = cell;
    }
}

public class AStarPathfinding
{
    private PathfindingCell[,] maze;
    private int height;
    private int width;
    PathfindingCell startCell;

    public List<PathfindingCell> AStar(List<Cell> mazeList, int inputWidth, int inputHeight)
    {
        Debug.Log(mazeList.Count);

        foreach(Cell cell in mazeList) 
        {
            Debug.Log($"({cell.CellPosition.X}, {cell.CellPosition.Y})");
        }

        maze = new PathfindingCell[inputWidth, inputHeight];
        height = inputHeight; 
        width = inputWidth;

        foreach(Cell c in mazeList)
        {
            maze[c.CellPosition.X, c.CellPosition.Y] = new PathfindingCell(c.CellPosition.X, c.CellPosition.Y, c);
        }

        startCell = maze[0, 0];
        PathfindingCell endCell = maze[width - 1, height - 1];

        return FindPath(maze, startCell, endCell);
    }

    public List<PathfindingCell> FindPath(PathfindingCell[,] maze, PathfindingCell startCell, PathfindingCell endCell)
    {
        List<PathfindingCell> exploredCells = new List<PathfindingCell>();
        List<PathfindingCell> cellsToExplore = new List<PathfindingCell> { startCell };

        for (int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                CalculateHCost(maze[x, y], endCell);
                CalculateFCost(maze[x, y]);
                maze[x, y].prevCell = null;
            }
        }

        startCell.gValue = 0;

        var path = RecursiveCall(cellsToExplore, exploredCells, endCell);

        if (path != null)
        {
            foreach (PathfindingCell cell in path)
            {
                Debug.Log($"{cell.X}, {cell.Y}");
            }
        }
        else
        {
            Debug.Log("No possible path");
        }
        return path;
    }

    public List<PathfindingCell> RecursiveCall(List<PathfindingCell> cellsToExplore, List<PathfindingCell> exploredCells, PathfindingCell endCell)
    {
        if(cellsToExplore.Count > 0)
        {
            PathfindingCell currentCell = GetLowestFCost(cellsToExplore);

            if(currentCell == endCell)
            {
                return CalculatePath(endCell);
            }

            cellsToExplore.Remove(currentCell);
            exploredCells.Add(currentCell);

            List<PathfindingCell> neighbours = FindNeighbours(currentCell);

            bool checkValid = false;
            foreach(PathfindingCell neighbour in neighbours)
            {
                if (!exploredCells.Contains(neighbour))
                {
                    checkValid = true;
                }
            }

            if (!checkValid)
            {
                return null;
            }

            foreach (PathfindingCell neighbour in neighbours)
            {
                if (exploredCells.Contains(neighbour))
                {
                    continue;
                }

                int tentativeGValue = currentCell.gValue + 1;

                if(tentativeGValue < neighbour.gValue)
                {
                    neighbour.prevCell = currentCell;
                    neighbour.gValue= tentativeGValue;
                    CalculateHCost(neighbour, endCell);
                    CalculateFCost(neighbour);
                    if(!cellsToExplore.Contains(neighbour)) 
                    {
                        cellsToExplore.Add(neighbour);
                        
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

        for(int i = 0; i < cellsToExplore.Count; i++)
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
        if (cell.correspondingCell.CellWallSides.HasFlag(WallSides.Left) && cell.X != width - 1)
        {
            result.Add(maze[cell.X + 1, cell.Y]);

        }
        if (cell.correspondingCell.CellWallSides.HasFlag(WallSides.Right) && cell.X != 0)
        {
           
            result.Add(maze[cell.X - 1, cell.Y]);

        }
        if (cell.correspondingCell.CellWallSides.HasFlag(WallSides.Up) && cell.Y != height - 1)
        {
            result.Add(maze[cell.X, cell.Y + 1]);
   
        }
        if (cell.correspondingCell.CellWallSides.HasFlag(WallSides.Down) && cell.Y != 0)
        {

            result.Add(maze[cell.X, cell.Y - 1]);

        }
        return result;
    }

    public void CalculateHCost(PathfindingCell cell, PathfindingCell endPoint)
    {
        if(cell == null || endPoint == null)
        {
            Debug.Log("Null Object");
        }
        else
        {
            cell.hValue = (endPoint.X - cell.X) + (endPoint.Y - cell.Y);
        }
    }

    public void CalculateFCost(PathfindingCell cell)
    {
        cell.fValue = cell.hValue + cell.gValue;
    }
}   

