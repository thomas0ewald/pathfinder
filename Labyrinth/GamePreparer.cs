using System;
using System.Collections.Generic;
using System.Windows;

namespace Labyrinth
{
  /// <summary>
  /// Responsible for preparing the game logic
  /// </summary>
  public class GamePreparer
  {

    public Drawer mDraw;

    private const int OFFSET_OUTSIDE = 2;
    public GamePreparer(Drawer aDraw)
    {
      mDraw = aDraw;
    }

    public void PrepareOneField(int aX, int aY, Field[,] aFields, Random aRandom)
    {
      Field fieldFigurePos = aFields[aX, aY];
      List<Field> neighbours = fieldFigurePos.ShuffleNeighbours(aRandom);

      foreach (Field neighbour in neighbours)
      {
        if (neighbour.DistanceStart == null)
        {
          WallLocation wallToBrake = fieldFigurePos.GetWallLocation(neighbour.X, neighbour.Y);
          fieldFigurePos.BreakWall(wallToBrake);

          // Walls neighbour = Break opposite wall
          switch (wallToBrake)
          {
            case WallLocation.Top:
              wallToBrake = WallLocation.Bottom;
              break;
            case WallLocation.Bottom:
              wallToBrake = WallLocation.Top;
              break;
            case WallLocation.Left:
              wallToBrake = WallLocation.Right;
              break;
            case WallLocation.Right:
              wallToBrake = WallLocation.Left;
              break;
          }
          aFields[neighbour.X, neighbour.Y].BreakWall(wallToBrake);

          neighbour.DistanceStart = -1;
          PrepareOneField(neighbour.X, neighbour.Y, aFields, aRandom);
        }
      }
    }

    public void InitBoard(
      ref Field[,] aFields,
      int aWidth,
      int aHeight,
      Random aRandom,
      FieldCoordinate aFigurePosition)
    {
      aFields = new Field[aWidth, aHeight];
      CreateFields(aFields, aWidth, aHeight);

      foreach (Field field in aFields)
      {
        field.SetNeighbours(aFields, aWidth - 1, aHeight - 1);
      }

      // per 50 fields = 1 exit
      int totalExits = (aHeight * aWidth) / 50;

      // make sure there's at least one exit
      totalExits = Math.Max(totalExits, 1);

      for (int i = 0; i < totalExits; i++)
      {
        CreateExit(aFields, aWidth, aHeight, aRandom);
      }

      UpdateBoard(ref aFields, aWidth, aHeight);

      mDraw.DrawFigure(aFigurePosition);
    }

    public void UpdateBoard(ref Field[,] aFields, int aWidth, int aHeight)
    {
      mDraw.ClearBoard();

      foreach (Field field in aFields)
      {
        mDraw.DrawField(field, aWidth + OFFSET_OUTSIDE, aHeight + OFFSET_OUTSIDE);
      }
    }

    private void CreateFields(Field[,] aFields, int aWidth, int aHeight)
    {
      for (int i = 0; i < aWidth; i++)
      {
        for (int j = 0; j < aHeight; j++)
        {
          aFields[i, j] = new Field(i, j);
        }
      }
    }

    private void CreateExit(Field[,] aFields, int aWidth, int aHeight, Random aRandom)
    {
      List<string> sides = new List<string> { "top", "bottom", "left", "right" };
      int posSide;
      Field match;

      int sideIndex = aRandom.Next(0, sides.Count);
      string side = sides[sideIndex];

      switch (side.ToUpper())
      {
        case "TOP":
          // get a position on side
          posSide = aRandom.Next(0, aWidth);
          // accomplish exit by breaking top wall
          match = aFields[posSide, 0];
          match.BreakWall(WallLocation.Top);
          break;
        case "BOTTOM":
          posSide = aRandom.Next(0, aWidth);
          match = aFields[posSide, aHeight - 1];
          match.BreakWall(WallLocation.Bottom);
          break;
        case "LEFT":
          posSide = aRandom.Next(0, aHeight);
          match = aFields[0, posSide];
          match.BreakWall(WallLocation.Left);
          break;
        case "RIGHT":
          posSide = aRandom.Next(0, aHeight);
          match = aFields[aWidth - 1, posSide];
          match.BreakWall(WallLocation.Right);
          break;
      }
    }

    /// <summary>
    /// initializes a Figure to indicate where the player is standing on the board
    /// </summary>
    public FieldCoordinate InitFigure(int aWidth, int aHeight, Random aRandom)
    {
      if ((aWidth < 1) || (aHeight < 1))
      {
        return new FieldCoordinate(-1, -1);
      }

      return new FieldCoordinate(aRandom.Next(0, aWidth), aRandom.Next(0, aHeight));
    }

    public FieldCoordinate CalculateFigurePosition(
      double aWidth,
      double aHeight,
      Point aMousePosition,
      int aFieldsForWidth,
      int aFieldsForHeight)
    {
      int fieldWidth = (int)Math.Max(aWidth / (aFieldsForWidth + OFFSET_OUTSIDE), 3);
      int fieldHeight = (int)Math.Max(aHeight / (aFieldsForHeight + OFFSET_OUTSIDE), 3);

      int figureX = (int)(aMousePosition.X / fieldWidth) - 1;
      int figureY = (int)(aMousePosition.Y / fieldHeight) - 1;

      return new FieldCoordinate(figureX, figureY);
    }
  }
}
