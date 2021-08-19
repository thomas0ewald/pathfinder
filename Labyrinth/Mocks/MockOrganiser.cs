using System;
using System.Collections.Generic;

namespace Labyrinth
{
  /// <summary>
  /// Mock class for Unittests
  /// </summary>
  public class MockOrganiser
  {
    public Field MockMoveFigure(int aX, int aY, int aXPrevious, int aYPrevious, Field[,] aFields)
    {
      Field currentFigurePos = aFields[aX, aY];

      currentFigurePos.HandleMovingOverBoard(aXPrevious, aYPrevious, aFields);
      List<Field> neighbours = currentFigurePos.ShuffleNeighbours(new Random());

      int count = neighbours.Count;
      foreach (Field neighbour in neighbours)
      {
        WallLocation wall = currentFigurePos.GetWallLocation(neighbour.X, neighbour.Y);
        if (!currentFigurePos.Walls.HasFlag(wall))
        {
          MockMoveFigure(neighbour.X, neighbour.Y, aX, aY, aFields);
        }
        else
        {
          count--;
        }

        if (count == 0)
        {
          currentFigurePos.AddNeighbour(currentFigurePos.PreviousFields.Peek());

          // go to previous field
          MockMoveFigure(
            currentFigurePos.PreviousFields.Peek().X,
            currentFigurePos.PreviousFields.Peek().Y,
            aX,
            aY,
            aFields);
        }
      }

      if (currentFigurePos == aFields[5, 5])
      {
        return currentFigurePos;
      }

      return currentFigurePos;
    }
  }
}
