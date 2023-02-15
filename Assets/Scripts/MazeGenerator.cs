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

public static class MazeGenerator
{
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

        (list, newMaze) = ApplyRecursiveBacktracker(maze, width, height);

        return (list, newMaze);
    }

    private static (List<Cell>, WallSides[,]) ApplyRecursiveBacktracker(WallSides[,] maze, int width, int height)
    {
        var rnd = new System.Random(/*seed*/);

        //A stack is a simple last-in-first-out non-generic collection of objects
        var positionStack = new Stack<Position>();
        var position = new Position { X = 0, Y = 0};

        List<Cell> generateOrder = new List<Cell>();

        //marks the initial point as visited
        maze[position.X, position.Y] |= WallSides.Visited;
        //makes the current position the only thing in the stack
        positionStack.Push(position);

        bool prevNoNeighbours = false;

        while (positionStack.Count > 0)
        {
            //Pop() removes and returns the first instance of the stack
            //the first time, there will only be one instance in the stack
            var current = positionStack.Pop();

            if(prevNoNeighbours)
            {
                prevNoNeighbours = false;
                var visitedNeighbours = GetVisitedNeighbours(current, maze, width, height);

                if (visitedNeighbours.Count > 0)
                {
                    var randIndex = rnd.Next(0, visitedNeighbours.Count);
                    var randomVisitedNeighbour = visitedNeighbours[randIndex];
                    var vNPosition = randomVisitedNeighbour.Position;

                    maze[current.X, current.Y] &= ~randomVisitedNeighbour.SharedWall;
                    maze[vNPosition.X, vNPosition.Y] &= ~GetOppositeWall(randomVisitedNeighbour.SharedWall);

                    Cell newNeighbourCell = new Cell
                    {
                        CellPosition = vNPosition,
                        CellWallSides = maze[vNPosition.X, vNPosition.Y]
                    };

                    for(int i = 0; i < generateOrder.Count; i++)
                    {
                        if (generateOrder[i].CellPosition.X == newNeighbourCell.CellPosition.X && generateOrder[i].CellPosition.Y == newNeighbourCell.CellPosition.Y)
                        {
                            generateOrder[i] = newNeighbourCell;
                        }

                    }
                   
                }
            }

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

            }
            else if(neighbours.Count == 0)
            {
                prevNoNeighbours = true;
            }

            Cell cell = new Cell
            {
                CellPosition = current,
                CellWallSides = maze[current.X, current.Y]
            };

            generateOrder.Add(cell);
        }

        return (generateOrder, maze);
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
            if (maze[p.X - 1, p.Y].HasFlag(WallSides.Visited))
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
            if (maze[p.X, p.Y - 1].HasFlag(WallSides.Visited))
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
            if (maze[p.X, p.Y + 1].HasFlag(WallSides.Visited))
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
            if (maze[p.X + 1, p.Y].HasFlag(WallSides.Visited))
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

}
