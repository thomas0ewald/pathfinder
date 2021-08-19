using System;
using System.Collections.Generic;
using System.Linq;

namespace Labyrinth
{
  /// <summary>
  /// represents a field on the board
  /// </summary>
  public class Field
  {
    private readonly List<Field> mNeighbours = new List<Field>();


    
    public Field(int aX, int aY)
    {
      X = aX;
      Y = aY;

      PreviousFields = new Queue<Field>();

      // surround field with Walls
      Walls = WallLocation.Top | WallLocation.Bottom | WallLocation.Left | WallLocation.Right;
    }

    // public bool IsVisited { get; set; }

    public int X { get; }

    public int Y { get; }

    public int? DistanceStart { get; set; }

    public Queue<Field> PreviousFields { get; }

    public WallLocation Walls { get; private set; }

    public WallLocation GetWallLocation(int aXNeighbour, int aYNeighbour)
    {
      WallLocation location = WallLocation.None;

      if ((aXNeighbour != X) || (aYNeighbour != Y))
      {
        if (aXNeighbour > X)
        {
          location = WallLocation.Right;
        }
        else if (aXNeighbour < X)
        {
          location = WallLocation.Left;
        }
        else if (aYNeighbour > Y)
        {
          location = WallLocation.Bottom;
        }
        else
        {
          location = WallLocation.Top;
        }
      }

      return location;
    }

    public void BreakWall(WallLocation aWall)
    {
      Walls = (Walls | aWall) ^ aWall;
    }

    public List<Field> ShuffleNeighbours(Random aRandom)
    {
      int n = mNeighbours.Count;
      while (n > 1)
      {
        n--;
        int k = aRandom.Next(n + 1);
        Field value = mNeighbours[k];
        mNeighbours[k] = mNeighbours[n];
        mNeighbours[n] = value;
      }

      return mNeighbours.ToList();
    }

    public void SetNeighbours(Field[,] aFields, int aMaxRight, int aMaxBottom)
    {
      Field match;

      const int MIN_LEFT = 0;
      const int MIN_TOP = 0;

      int yNeighbourTop = Y - 1;
      int yNeighbourBottom = Y + 1;
      int xNeighbourLeft = X - 1;
      int xNeighbourRight = X + 1;

      if (yNeighbourTop >= MIN_TOP)
      {
        match = aFields[X, Y - 1];

        if (match != null)
        {
          mNeighbours.Add(match);
        }
      }

      if (yNeighbourBottom <= aMaxBottom)
      {
        match = aFields[X, Y + 1];

        if (match != null)
        {
          mNeighbours.Add(match);
        }
      }

      if (xNeighbourLeft >= MIN_LEFT)
      {
        match = aFields[X - 1, Y];

        if (match != null)
        {
          mNeighbours.Add(match);
        }
      }

      if (xNeighbourRight <= aMaxRight)
      {
        match = aFields[X + 1, Y];

        if (match != null)
        {
          mNeighbours.Add(match);
        }
      }
    }

    public List<Field> GetNeighbours()
    {
      return mNeighbours;
    }

    public void AddNeighbour(Field aNeighbour)
    {
      mNeighbours.Add(aNeighbour);
    }

    /// <summary>
    /// prevent an infinity loop while moving from one field to another
    /// </summary>
    /// <param name="aX">x-coord of the previous field</param>
    /// <param name="aY">y-coor of the previous field</param>
    /// <returns></returns>
    public void HandleMovingOverBoard(int aX, int aY, Field[,] aFields)
    {
      Field previous = mNeighbours.Find(aNeighbour => (aNeighbour.X == aX) && (aNeighbour.Y == aY));

      if (previous != null)
      {
        mNeighbours.Remove(previous);
        PreviousFields.Enqueue(aFields[aX, aY]);
      }
    }
  }
}
