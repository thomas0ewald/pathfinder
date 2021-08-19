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

    private int mHeight = 10;

    private int mWidth = 10;

    private Random mRandom;

    private bool mIsExited = true;

    private Func<Point> mGetPosition;

    private Pathfinder mPathfinder;

    public ProcedureOrganiser(Canvas aBoardControl, Func<Point> aGetMousePosition)
    {
      mDraw.Board = aBoardControl;
      mGamePreparer = new GamePreparer(mDraw);
      SetStartPos = new RelayCommand(ExecuteSetStartPos, CanExecuteSetStartPos);
      Reset = new RelayCommand(ExecuteReset);
      Solve = new RelayCommand(ExecuteSolve, CanExecuteMove);
      StepOneTime = new RelayCommand(ExecuteStepOneTime, CanExecuteMove);
      StepFiveTimes = new RelayCommand(ExecuteStepFiveTimes, CanExecuteMove);
      mGetPosition = aGetMousePosition;
    }

    public Canvas Board { get => mDraw.Board; set => mDraw.Board = value; }

    public int Width { get => mWidth; set { mWidth = value; OnPropertyChanged(); } }

    public int Height { get => mHeight; set { mHeight = value; OnPropertyChanged(); } }

    public bool IsExited { get => mIsExited; set { mIsExited = value; OnPropertyChanged(); } }

    public RelayCommand SetStartPos { get; }

    public RelayCommand Reset { get; }

    public RelayCommand Solve { get; }

    public RelayCommand StepOneTime { get; }

    public RelayCommand StepFiveTimes { get; }


    public bool CanExecuteSetStartPos(object aParameter)
    {
      return !IsExited && (mStartField.X < 0) && (mStartField.X < 0);
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
        mPathfinder = new Pathfinder(this, Width, Height, mFields, mFigurePosition, mGamePreparer.mDraw);
      }
      catch (Exception)
      {
        string message = "Invalid start point";
        MessageBox.Show(message);
      }
    }

    public void ExecuteReset(object aParameter)
    {
      // save height/width to prevent updating while drawing
      mHeight = Height;
      mWidth = Width;

      mFieldsToCheck.Clear();
      mShortestDistance.Clear();
      IsExited = false;
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

    public void ExecuteStepOneTime(object aParameter)
    {
      mPathfinder.StepOneTime();
    }

    public void ExecuteStepFiveTimes(object aParameter)
    {
      mPathfinder.StepFiveTimes();
    }

    public bool CanExecuteMove(object aParameter)
    {
      return !IsExited && (mStartField.X > -1) && (mStartField.Y > -1);
    }

    private void ExecuteSolve(object aParameter)
    {
      mPathfinder.StepToExit();

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

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string aPropertyName = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(aPropertyName));
    }
  }
}
