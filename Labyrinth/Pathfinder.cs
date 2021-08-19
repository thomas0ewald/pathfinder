using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace Labyrinth
{
  /// <summary>
  /// Solves the Labyrinth by stepping the requested steps from starting point to exit
  /// </summary>
  class Pathfinder
	{
    private readonly Drawer mDrawer;

    private Field mStartField = new Field(-1, -1);

    private Field[,] mFields;

    private Field mExitField;

    private Field mCurrentField;

    private FieldCoordinate mFigurePosition;

    /// <summary>
    /// Queue of Fields which do not contain a border between themselves and the previous field
    /// </summary>
    private readonly Queue<Field> mPossibleNeighbours = new Queue<Field>();

    /// <summary>
    /// Queue of the farthest visited fields from the starting point in each direction
    /// </summary>
    private readonly Queue<Field> mPaths = new Queue<Field>();

    private readonly List<Field> mShortestDistance = new List<Field>();

    private int mWidth;

    private int mHeight;

    private readonly ProcedureOrganiser mProcedureOrganiser;

    public Pathfinder(ProcedureOrganiser aProcedureOrganiser, int aWidth, int aHeight, Field[,] aFields, FieldCoordinate aFigurePosition, Drawer aDrawer)
    {
      mProcedureOrganiser = aProcedureOrganiser;
      mWidth = aWidth;
      mHeight = aHeight;
      mFields = aFields;
      mFigurePosition = aFigurePosition;
      mStartField = mFields[aFigurePosition.X, aFigurePosition.Y];
      mCurrentField = mStartField;
      mCurrentField.DistanceStart = 0;
      mDrawer = aDrawer;
    }

    public bool IsExited { get => mProcedureOrganiser.IsExited; set => mProcedureOrganiser.IsExited = value;  }

    // moves one position from starting point in all possible directions
    public void StepOneTime()
		{
      // mPaths is here a Queue of the farthest visited fields from the starting point in each direction
      if (!mPaths.Any())
      {
        mPaths.Enqueue(mCurrentField);
      }
      
      foreach (Field farthest in mPaths)
      {
        foreach (Field neighbour in farthest.GetNeighbours())
        {
          WallLocation wall = farthest.GetWallLocation(neighbour.X, neighbour.Y);
          // check if there's no wall and field is not starting position to prevent going back and forth
          if (!farthest.Walls.HasFlag(wall) && neighbour.DistanceStart < 0)
          {

            neighbour.DistanceStart = farthest.DistanceStart + 1; // (farthest == mStartField) ? farthest.DistanceStart + 2 : farthest.DistanceStart + 1;
            mPossibleNeighbours.Enqueue(neighbour);
          }
        }
      }

      while (mPossibleNeighbours.Any() && !IsExited)
      {
        Field f = mPossibleNeighbours.Dequeue();
        mCurrentField = f;
        mPaths.Enqueue(mCurrentField);

        TryToExit(mCurrentField.X, mCurrentField.Y);

        if (IsExited)
        {
          // mark shortest way
          GetShortestWay(mExitField);

          foreach (Field field in mShortestDistance)
          {
            mDrawer.FillVisitedField(field.X, field.Y, field.DistanceStart, Brushes.Green);
          }

          // mark start
          mDrawer.FillVisitedField(mStartField.X, mStartField.Y, mStartField.DistanceStart, Brushes.DarkBlue);

          // mark exit
          mDrawer.FillVisitedField(mExitField.X, mExitField.Y, mExitField.DistanceStart, Brushes.LightSeaGreen);

          // mark figure
          mDrawer.DrawFigure(mFigurePosition);
        }
        else
        {
          mDrawer.FillVisitedField(mCurrentField.X, mCurrentField.Y, mCurrentField.DistanceStart, Brushes.OrangeRed);
        }
      }
    }

    // moves five positions from starting point in all possible directions
    public void StepFiveTimes()
		{
      for (int i = 0; i < 5; i++)
      {
        StepOneTime();
      }
    }

    // moves until exit is found
    public void StepToExit()
		{
      while (!IsExited)
      {
        StepOneTime();
      }
    }

    /// <summary>
    /// Gets the nearest exit by increasing the distance for each neighbour field.
    /// The first exit found will automatically be the nearest!
    /// </summary>
    /// <param name="aField">the field to be checked</param>
    private void GetNearestExit(Field aField)
    {
      TryToExit(aField.X, aField.Y);

      if (!IsExited)
      {
        foreach (Field neighbour in aField.GetNeighbours())
        {
          WallLocation wall = aField.GetWallLocation(neighbour.X, neighbour.Y);
          // check if there's no wall and field is not starting position to prevent going back and forth
          if (!aField.Walls.HasFlag(wall) && neighbour.DistanceStart < 0)
          {
            neighbour.DistanceStart = aField.DistanceStart + 1;
            mPossibleNeighbours.Enqueue(neighbour);
          }
        }

        while (mPossibleNeighbours.Any() && !IsExited)
        {
          Field f = mPossibleNeighbours.Dequeue();
          mCurrentField = f;
          mDrawer.FillVisitedField(f.X, f.Y, f.DistanceStart, Brushes.OrangeRed);
          GetNearestExit(f);
        }
      }
    }

    private void TryToExit(int aX, int aY)
    {
      // exit left
      if (aX == 0)
      {
        HandleLeaving(WallLocation.Left, new FieldCoordinate(aX, aY), new FieldCoordinate(aX - 1, aY));
      }

      // exit top
      if (aY == 0)
      {
        HandleLeaving(WallLocation.Top, new FieldCoordinate(aX, aY), new FieldCoordinate(aX, aY - 1));
      }

      // exit right
      if (aX == mWidth - 1)
      {
        HandleLeaving(WallLocation.Right, new FieldCoordinate(aX, aY), new FieldCoordinate(aX + 1, aY));
      }
      
      // exit bottom
      if (aY == mHeight - 1)
      {
        HandleLeaving(WallLocation.Bottom, new FieldCoordinate(aX, aY), new FieldCoordinate(aX, aY + 1));
      }
    }

    /// <summary>
    /// attempt to exit the Labyrinth
    /// </summary>
    /// <param name="aWall">Wall to be checked</param>
    /// <param name="aCurrentField">the field, where the figure is standing</param>
    /// <param name="aNeighbourField">the field to move to</param>
    private void HandleLeaving(WallLocation aWall, FieldCoordinate aCurrentField, FieldCoordinate aNeighbourField)
    {
      WallLocation walls = mFields[aCurrentField.X, aCurrentField.Y].Walls;

      if (!walls.HasFlag(aWall))
      {
        IsExited = true;
        mExitField = mFields[aCurrentField.X, aCurrentField.Y];
        mFigurePosition = new FieldCoordinate(aNeighbourField.X, aNeighbourField.Y);
      }
    }

    private void GetShortestWay(Field aField)
    {
      bool isOk = false;

      foreach (Field neighbour in aField.GetNeighbours())
      {
        // check if field is on shortest way
        if (neighbour.DistanceStart > 0)
        {
          isOk = true;
        }
        else if ((neighbour.DistanceStart == 0) && (neighbour.Equals(mStartField)))
        {
          isOk = true;
        }

        if (isOk)
        {
          WallLocation wall = aField.GetWallLocation(neighbour.X, neighbour.Y);
          if (neighbour.DistanceStart >= 0)
          {
            if ((neighbour.DistanceStart < aField.DistanceStart) && (!aField.Walls.HasFlag(wall)))
            {
              mShortestDistance.Add(neighbour);

              if (neighbour.DistanceStart > 0)
              {
                GetShortestWay(neighbour);
              }
            }
          }
        }
      }
    }
  }
}
