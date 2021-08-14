using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Proxies;
using System.Windows;
using System.Windows.Annotations;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using Labyrinth.Annotations;
using Labyrinth.Selecta;

namespace Labyrinth
{
  /// <summary>
  /// manages the procedure of the Labyrinth
  /// </summary>
  public class ProcedureOrganiser : INotifyPropertyChanged
  {
    private Drawer mDraw = new Drawer();

    private Field[,] mFields;

    private FieldCoordinate mFigurePosition;

    private Field mStartField = new Field(-1, -1);

    private readonly Queue<Field> mFieldsToCheck = new Queue<Field>();

    private readonly List<Field> mShortestDistance = new List<Field>();

    private readonly GamePreparer mGamePreparer;

    private Field mFieldExit;

    private int mHeight;

    private int mWidth;

    private Random mRandom;

    private bool mExited = true;

    private Func<Point> mGetPosition;

    public ProcedureOrganiser(Canvas aBoardControl, Func<Point> aGetMousePosition)
    {
      mDraw.Board = aBoardControl;
      mGamePreparer = new GamePreparer(mDraw);
      SetStartPos = new RelayCommand(ExecuteSetStartPos, CanExecuteSetStartPos);
      Prepare = new RelayCommand(ExecutePrepare);
      Solve = new RelayCommand(ExecuteSolve, CanExecuteSolve);
      //StepOneTime = new RelayCommand(ExecuteStepOneTime, CanExecuteStepOneTime);
      mGetPosition = aGetMousePosition;
    }

    public Canvas Board { get => mDraw.Board; set => mDraw.Board = value; }

    public int Width { get; set; } = 10;

    public int Height { get; set; } = 10;

    public RelayCommand SetStartPos { get; }

    public RelayCommand Prepare { get; }

    public RelayCommand Solve { get; }

    public RelayCommand StepOneTime { get; }

    public RelayCommand StepFiveTimes { get; }


    public bool CanExecuteSetStartPos(object aParameter)
    {
      return !mExited && (mStartField.X < 0) && (mStartField.X < 0);
    }

    public void ExecuteSetStartPos(object aParameter)
    {
      mWidth = Width;
      mHeight = Height;

      try
      {
        Point mousePosition = mGetPosition();
        mFigurePosition = mGamePreparer.CalculateFigurePosition(
          Board.Width,
          Board.Height,
          mousePosition,
          mWidth,
          mHeight);

        if (mFigurePosition.X >= mWidth || mFigurePosition.X < 0 || mFigurePosition.Y >= mHeight
            || mFigurePosition.Y < 0)
        {
          mStartField = new Field(-1, -1);
          // start point is outside the maze
          return;
        }

        mDraw.DrawFigure(mFigurePosition);

        // set to start field
        mStartField = mFields[mFigurePosition.X, mFigurePosition.Y];
      }
      catch (Exception)
      {
        string message = "Invalid start point";
        MessageBox.Show(message);
      }
    }

    public void ExecutePrepare(object aParameter)
    {
      // save height/width to prevent updating while drawing
      mHeight = Height;
      mWidth = Width;

      mFieldsToCheck.Clear();
      mShortestDistance.Clear();
      mExited = false;
      mStartField = new Field(-1, -1);

      mRandom = new Random();
      mFigurePosition = mGamePreparer.InitFigure(mWidth, mHeight, mRandom);
      mGamePreparer.InitBoard(ref mFields, mWidth, mHeight, mRandom, mFigurePosition);

      // Prepare fields
      mGamePreparer.PrepareOneField(mFigurePosition.X, mFigurePosition.Y, mFields, mRandom);
      mFigurePosition = new FieldCoordinate(-1, -1);
      mGamePreparer.UpdateBoard(ref mFields, mWidth, mHeight);
      OnPropertyChanged(nameof(Board));
    }

    public bool CanExecuteSolve(object aParameter)
    {
      return !mExited && (mStartField.X > -1) && (mStartField.Y > -1);
    }

    private void ExecuteSolve(object aParameter)
    {
      Pathfinder pathfinder = new Pathfinder(mWidth, mHeight, mFields, mFigurePosition, mGamePreparer.mDraw);
      pathfinder.StepToExit();
      //mExited = false;
      //mGamePreparer.UpdateBoard(ref mFields, Width, Height);
      //mStartField = mFields[mFigurePosition.X, mFigurePosition.Y];
      //mDraw = mGamePreparer.mDraw;

      //// find shortest way from start to exit
      //if (mStartField.DistanceStart < 0)
      //{
      //  mExited = false;
      //  mStartField.DistanceStart = 0;
      //  mFieldsToCheck.Enqueue(mStartField);

      //  while (mFieldsToCheck.Count > 0 && !mExited)
      //  {
      //    Field startField = mFieldsToCheck.Dequeue();
      //    GetNearestExit(startField);
      //  }
      //  mDraw.DrawFigure(mFigurePosition);
      //  GetShortestWay(mFieldExit);
      //}

      //// mark shortest way
      //foreach (Field field in mShortestDistance)
      //{
      //  mDraw.FillVisitedField(field.X, field.Y, field.DistanceStart, Brushes.DarkGreen);
      //}

      //// mark start
      //mDraw.FillVisitedField(mStartField.X, mStartField.Y, mStartField.DistanceStart, Brushes.DarkBlue);

      //// mark exit
      //mDraw.FillVisitedField(mFieldExit.X, mFieldExit.Y, mFieldExit.DistanceStart, Brushes.LightSeaGreen);

      //// DEBUG-----------------
      //MessageBox.Show($"Start: {mStartField.X} {mStartField.Y}");

      //// nearest exit = first item in Queue mDistanceStart
      //Field exit = mFieldExit;
      //MessageBox.Show($"nearest exit: {exit.X},{exit.Y} | Entfernung: {exit.DistanceStart}" );

      //string debug = mFieldExit.X + "," + mFieldExit.Y + "|";
      //foreach (Field field in mShortestDistance)
      //{
      //  debug += field.X + "," + field.Y + "|";
      //}
      //MessageBox.Show($"shortest way: {debug}");
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
          if (!aField.Walls.HasFlag(wall) && neighbour.DistanceStart < 0)
          {
            neighbour.DistanceStart = aField.DistanceStart + 1;
            mFieldsToCheck.Enqueue(neighbour);
          }
        }

        while (mFieldsToCheck.Any() && !mExited)
        {
          Field f = mFieldsToCheck.Dequeue();
          mDraw.FillVisitedField(f.X, f.Y, f.DistanceStart, Brushes.DarkRed);
          GetNearestExit(f);
        }
      }
    }

    /// <summary>
    /// Gets the shortest way from start to exit.
    /// </summary>
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

    private void TryToExit(int aX, int aY)
    {
      if (aX == 0)
      {
        HandleLeaving(WallLocation.Left, new FieldCoordinate(aX, aY), new FieldCoordinate(aX - 1, aY));
      }

      if (aY == 0)
      {
        HandleLeaving(WallLocation.Top, new FieldCoordinate(aX, aY), new FieldCoordinate(aX, aY - 1));
      }

      if (aX == mWidth - 1)
      {
        HandleLeaving(WallLocation.Right, new FieldCoordinate(aX, aY), new FieldCoordinate(aX + 1, aY));
      }

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
        mFieldExit = mFields[aCurrentField.X, aCurrentField.Y];
        mFigurePosition = new FieldCoordinate(aNeighbourField.X, aNeighbourField.Y);
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string aPropertyName = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(aPropertyName));
    }
  }
}
