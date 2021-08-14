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

    private bool mExited;

    private Field mStartField = new Field(-1, -1);

    private Field[,] mFields;

    private Field mExitField;

    private FieldCoordinate mFigurePosition;

    /// <summary>
    /// Queue of Fields which do not contain a border between themselves and the previous field
    /// </summary>
    private readonly Queue<Field> mPossibleNeighbours = new Queue<Field>();

    private readonly List<Field> mShortestDistance = new List<Field>();

    private int mWidth;

    private int mHeight;

    public Pathfinder(int aWidth, int aHeight, Field[,] aFields, FieldCoordinate aFigurePosition, Drawer aDrawer)
    {
      mWidth = aWidth;
      mHeight = aHeight;
      mFields = aFields;
      mFigurePosition = aFigurePosition;
      mDrawer = aDrawer;
    }

    // moves one position from starting point in all possible directions
    public void StepOneTime()
		{
		}

		// moves five positions from starting point in all possible directions
		public void StepFiveTimes()
		{
		}

		// moves until exit is found
		public void StepToExit()
		{
      //mGamePreparer.UpdateBoard(ref mFields, Width, Height);
      mStartField = mFields[mFigurePosition.X, mFigurePosition.Y];
      //mDraw = mGamePreparer.mDraw;

      if (mStartField.DistanceStart < 0)
      {
        mExited = false;
        mStartField.DistanceStart = 0;
        mPossibleNeighbours.Enqueue(mStartField);

        while (mPossibleNeighbours.Count > 0 && !mExited)
        {
          Field startField = mPossibleNeighbours.Dequeue();
          GetNearestExit(mStartField);
        }
        mDrawer.DrawFigure(mFigurePosition);
        GetShortestWay(mExitField);
      }

      // mark shortest way
      foreach (Field field in mShortestDistance)
      {
        mDrawer.FillVisitedField(field.X, field.Y, field.DistanceStart, Brushes.Green);
      }

      // mark start
      mDrawer.FillVisitedField(mStartField.X, mStartField.Y, mStartField.DistanceStart, Brushes.DarkBlue);

      // mark exit
      mDrawer.FillVisitedField(mExitField.X, mExitField.Y, mExitField.DistanceStart, Brushes.LightSeaGreen);

    }

    /// <summary>
    /// Gets the nearest exit by increasing the distance for each neighbour field.
    /// The first exit found will automatically be the nearest!
    /// </summary>
    /// <param name="aField">the field to be checked</param>
    private void GetNearestExit(Field aField)
    {
      TryToExit(aField.X, aField.Y);

      if (!mExited)
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

        while (mPossibleNeighbours.Any() && !mExited)
        {
          Field f = mPossibleNeighbours.Dequeue();
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
        mExited = true;
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
