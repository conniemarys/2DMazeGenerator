using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UIElements;

[Flags]
public enum WallSides
{
    Visited = 1, //00001
    Left = 2, //00010
    Right = 4, //00100
    Up = 8, //01000
    Down = 16 //10000
}
public struct Position
{
    public int X;
    public int Y;
}

public struct Cell
{
    public Position CellPosition;
    public WallSides CellWallSides;
    public int hValue;
    public int gValue;
    public int fValue;
    public int prevCellX;
    public int prevCellY;
}

public struct Neighbour
{
    public Position Position;
    public WallSides SharedWall;
}

public class MazeGenerator
{
    public static System.Random rnd = new System.Random(69);

    public static WallSides[,] GenerateEmpty(int width, int height)
    {
        WallSides[,] maze = new WallSides[width, height];
        WallSides initial = WallSides.Left | WallSides.Right | WallSides.Up | WallSides.Down;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                maze[x, y] = initial;
            }
        }

        return maze;
    }

    public static (List<Cell>, WallSides[,]) Generate(int width, int height)
    {
        WallSides[,] maze = new WallSides[width, height];
        WallSides initial = WallSides.Left | WallSides.Right | WallSides.Up | WallSides.Down;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                maze[x, y] = initial;
            }
        }

        WallSides[,] newMaze = new WallSides[width, height];
        List<Cell> list = new List<Cell>();

        //(list, newMaze) = ApplyRecursiveBacktracker(maze, width, height);
        Position tempPosition = new Position();
        (list, newMaze) = BackTracker(maze, width, height, tempPosition);

        return (list, newMaze);
    }

    private static (List<Cell>, WallSides[,]) ApplyRecursiveBacktracker(WallSides[,] maze, int width, int height)
    {
        var rnd = new System.Random(69);

        //A stack is a simple last-in-first-out non-generic collection of objects
        var positionStack = new Stack<Position>();
        var position = new Position { X = 0, Y = 0};

        List<Cell> generateOrder = new List<Cell>();

        //marks the initial point as visited
        maze[position.X, position.Y] |= WallSides.Visited;
        //makes the current position the only thing in the stack
        positionStack.Push(position);

        bool prevNoNeighbours = false;

        var current = new Position();

        while (positionStack.Count > 0)
        {
            //Pop() removes and returns the first instance of the stack
            //the first time, there will only be one instance in the stack
            current = positionStack.Pop();
            prevNoNeighbours = false;

            var neighbours = GetUnvisitedNeighbours(current, maze, width, height);

            if(neighbours.Count > 0)
            {
                //pushes the current instance back into the stack if there are more neighbours to assess
                positionStack.Push(current);

                var randIndex = rnd.Next(0, neighbours.Count);

                var randomNeighbour = neighbours[randIndex];

                var nPosition = randomNeighbour.Position;
                //deletes the shared wall from both current cell and random neighbour
                maze[current.X, current.Y] &= ~randomNeighbour.SharedWall;
                maze[nPosition.X, nPosition.Y] &= ~GetOppositeWall(randomNeighbour.SharedWall);

                //marks the chosen neighbour cell as visited
                maze[nPosition.X, nPosition.Y] |= WallSides.Visited;

                //makes the neighbouring cell the next to be addressed
                positionStack.Push(nPosition);

                Cell cell = new Cell
                {
                    CellPosition = current,
                    CellWallSides = maze[current.X, current.Y]
                };

                generateOrder.Add(cell);
                
            }

        }

        return (generateOrder, maze);
    }

    public static (List<Cell>, WallSides[,]) BackTracker(WallSides[,] maze, int width, int height, Position currentCell, List<Cell> orderGeneration = null, Stack<Position> positionStack = null, int count = 0)
    {
        if(positionStack == null)
        {
            positionStack = new Stack<Position>();
            Position position = new Position { X = 0, Y = 0 };
            positionStack.Push(position);

            currentCell = position;

            orderGeneration = new List<Cell>();
        }

        if(positionStack.Count == 0)
        {
            return (orderGeneration, maze);
        }

        else
        {
            var neighbours = GetUnvisitedNeighbours(currentCell, maze, width, height);

            if (neighbours.Count > 0)
            {
                count = 0;
                //pushes the current instance back into the stack if there are more neighbours to assess
                //positionStack.Push(currentCell);

                var randIndex = MazeGenerator.rnd.Next(0, neighbours.Count);

                var randomNeighbour = neighbours[randIndex];

                var nPosition = randomNeighbour.Position;
                //deletes the shared wall from both current cell and random neighbour
                maze[currentCell.X, currentCell.Y] &= ~randomNeighbour.SharedWall;
                maze[nPosition.X, nPosition.Y] &= ~GetOppositeWall(randomNeighbour.SharedWall);

                //marks the chosen neighbour cell as visited
                maze[nPosition.X, nPosition.Y] |= WallSides.Visited;

                //makes the neighbouring cell the next to be addressed
                positionStack.Push(nPosition);

                Cell cell = new Cell
                {
                    CellPosition = currentCell,
                    CellWallSides = maze[currentCell.X, currentCell.Y]
                };

                orderGeneration.Add(cell);
                currentCell = positionStack.Pop();

                return BackTracker(maze, width, height, currentCell, orderGeneration, positionStack);

            }
            else
            {
                count++;
                if(count == orderGeneration.Count)
                {
                    BackTracker(maze, width, height, currentCell, orderGeneration, positionStack);
                }
                currentCell = orderGeneration[orderGeneration.Count - count].CellPosition;
                return BackTracker(maze, width, height, currentCell, orderGeneration, positionStack, count);
            }

        }
    }

    public static WallSides GetOppositeWall(WallSides wall)
    {
        switch(wall)
        {
            case WallSides.Down:        return WallSides.Up;
            case WallSides.Up:          return WallSides.Down;
            case WallSides.Left:        return WallSides.Right;
            case WallSides.Right:       return WallSides.Left;
            default:                    return WallSides.Left;
        }
    }

    private static List<Neighbour> GetUnvisitedNeighbours(Position p, WallSides[,] maze, int width, int height)
    {
        var list = new List<Neighbour>();

        if (p.X > 0)
        {
            if(!maze[p.X-1, p.Y].HasFlag(WallSides.Visited))
            {
                list.Add(new Neighbour
                {
                    Position = new Position
                    {
                        X = p.X - 1,
                        Y = p.Y
                    },
                    SharedWall = WallSides.Left
                });
            }
        }

        if (p.Y > 0)
        {
            if(!maze[p.X, p.Y-1].HasFlag(WallSides.Visited))
            {
                list.Add(new Neighbour
                {
                    Position = new Position
                    {
                        X = p.X,
                        Y = p.Y - 1
                    },
                    SharedWall = WallSides.Down
                });
            }
        }

        if (p.Y < height - 1)
        {
            if (!maze[p.X, p.Y + 1].HasFlag(WallSides.Visited))
            {
                list.Add(new Neighbour
                {
                    Position = new Position
                    {
                        X = p.X,
                        Y = p.Y + 1
                    },
                    SharedWall = WallSides.Up
                });
            }
        }

        if (p.X < width - 1)
        {
            if (!maze[p.X + 1, p.Y].HasFlag(WallSides.Visited))
            {
                list.Add(new Neighbour
                {
                    Position = new Position
                    {
                        X = p.X + 1,
                        Y = p.Y
                    },
                    SharedWall = WallSides.Right
                });
            }
        }

        return list;
    }

    private static List<Neighbour> GetVisitedNeighbours(Position p, WallSides[,] maze, int width, int height)
    {
        var list = new List<Neighbour>();

        if (p.X > 0)
        {
            if (maze[p.X - 1, p.Y].HasFlag(WallSides.Visited) && CheckNumWalls(maze[p.X - 1, p.Y]) >= 3)
            {
                list.Add(new Neighbour
                {
                    Position = new Position
                    {
                        X = p.X - 1,
                        Y = p.Y
                    },
                    SharedWall = WallSides.Left
                });
            }
        }

        if (p.Y > 0)
        {
            if (maze[p.X, p.Y - 1].HasFlag(WallSides.Visited) && CheckNumWalls(maze[p.X, p.Y - 1]) >= 3)
            {
                list.Add(new Neighbour
                {
                    Position = new Position
                    {
                        X = p.X,
                        Y = p.Y - 1
                    },
                    SharedWall = WallSides.Down
                });
            }
        }

        if (p.Y < height - 1)
        {
            if (maze[p.X, p.Y + 1].HasFlag(WallSides.Visited) && CheckNumWalls(maze[p.X, p.Y + 1]) >= 3)
            {
                list.Add(new Neighbour
                {
                    Position = new Position
                    {
                        X = p.X,
                        Y = p.Y + 1
                    },
                    SharedWall = WallSides.Up
                });
            }
        }

        if (p.X < width - 1)
        {
            if (maze[p.X + 1, p.Y].HasFlag(WallSides.Visited) && CheckNumWalls(maze[p.X + 1, p.Y]) >= 3)
            {
                list.Add(new Neighbour
                {
                    Position = new Position
                    {
                        X = p.X + 1,
                        Y = p.Y
                    },
                    SharedWall = WallSides.Right
                });
            }
        }

        return list;
    }

    private static int CheckNumWalls(WallSides cell)
    {
        int count = 0;

        if (cell == WallSides.Up)
        {
            count++;
        }
        else if (cell == WallSides.Down)
        {
            count++;
        }
        else if (cell == WallSides.Left) 
        {
            count++;
        }
        else if (cell == WallSides.Right)
        {
            count++;
        }

        return count;
    }

}
