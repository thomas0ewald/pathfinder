using System.Collections.Generic;

using Labyrinth;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestLabyrinth
{
  [TestClass]
  public class UnitTestField
  {
    [TestMethod]
    public void TestGetWallLocationRight()
    {
      Field field = new Field(1, 1);

      WallLocation location = field.GetWallLocation(2, 1);

      Assert.AreEqual(WallLocation.Right, location);
    }

    [TestMethod]
    public void TestGetWallLocationNone()
    {
      Field field = new Field(1, 1);

      WallLocation location = field.GetWallLocation(1, 1);

      Assert.AreEqual(WallLocation.None, location);
    }

    [TestMethod]
    public void TestBreakWallRight()
    {
      Field field = new Field(1, 1);

      field.BreakWall(WallLocation.Right);

      Assert.AreEqual(WallLocation.Top | WallLocation.Bottom | WallLocation.Left, field.Walls);
    }

    [TestMethod]
    public void TestBreakWallNone()
    {
      Field field = new Field(1, 1);

      field.BreakWall(WallLocation.None);

      Assert.AreEqual(WallLocation.Top | WallLocation.Bottom | WallLocation.Left | WallLocation.Right, field.Walls);
    }

    [TestMethod]
    public void TestSetNeighbour4()
    {
      List<Field> expectedNeighbours = new List<Field> {new Field(6,5), new Field(4, 5), new Field(5, 6), new Field(5, 4) };
      Field field = new Field(5, 5);
      Field[,] fieldsOnBoard = new Field[10, 10];

      for (int i = 0; i < 10; i++)
      {
        for (int j = 0; j < 10; j++)
        {
          fieldsOnBoard[i, j] = new Field(i, j);
        }
      }

      field.SetNeighbours(fieldsOnBoard, 9, 9);
      List<Field> actualNeighbours = field.GetNeighbours();
      Assert.AreEqual(expectedNeighbours.Count, actualNeighbours.Count);
    }

    [TestMethod]
    public void TestSetNeighbour2()
    {
      List<Field> expectedNeighbours = new List<Field> { new Field(9, 10), new Field(10, 9)};
      Field field = new Field(9, 9);

      Field[,] fieldsOnBoard = new Field[10, 10];

      for (int i = 0; i < 10; i++)
      {
        for (int j = 0; j < 10; j++)
        {
          fieldsOnBoard[i, j] = new Field(i, j);
        }
      }

      field.SetNeighbours(fieldsOnBoard, 9, 9);
      List<Field> actualNeighbours = field.GetNeighbours();
      Assert.AreEqual(expectedNeighbours.Count, actualNeighbours.Count);
    }

    [TestMethod]
    public void TestSetNeighbourPosition()
    {
      int[] posNeighbours =  {5, 4, 5, 6, 4, 5, 6, 5};
      string expectedPos = string.Empty;

      foreach (int pos in posNeighbours)
      {
        expectedPos += pos;
      }

      Field field = new Field(5, 5);

      Field[,] fieldsOnBoard = new Field[10, 10];

      for (int i = 0; i < 10; i++)
      {
        for (int j = 0; j < 10; j++)
        {
          fieldsOnBoard[i,j] = new Field(i, j);
        }
      }

      field.SetNeighbours(fieldsOnBoard, 9, 9);

      List<Field> actualNeighbours = field.GetNeighbours();
      string[] actualPosNeighbours = new string[actualNeighbours.Count * 2];
      string actualPos = string.Empty;
      int neighbourCounter = 0;

      foreach (Field neighbour in actualNeighbours)
      {
        string pos = neighbour.X.ToString() + neighbour.Y;
        neighbourCounter++;
        actualPosNeighbours[neighbourCounter] = pos;
      }

      foreach (string pos in actualPosNeighbours)
      {
        actualPos += pos;
      }

      Assert.AreEqual(expectedPos, actualPos);
    }

    /// <summary>
    /// assuming, we have a cross road with 3 possible exits of which 2 are dead ends
    ///
    ///   B
    /// _ _ _
    /// A-Z-C|
    /// - | -
    ///  |D|
    ///   -
    /// 
    /// </summary>
    [TestMethod]
    public void TestMoveFigureTwoDeadEnds()
    {
      // prepare circumstance as explaind above
      Field[,] fieldsOnBoard = new Field[10, 10];

      for (int i = 0; i < 10; i++)
      {
        for (int j = 0; j < 10; j++)
        {
          fieldsOnBoard[i, j] = new Field(i, j);
        }
      }

      Field a = fieldsOnBoard[5, 5];
      Field b = fieldsOnBoard[6, 4];
      Field z = fieldsOnBoard[6, 5];
      Field c = fieldsOnBoard[7, 5];
      Field d = fieldsOnBoard[6, 6];
      Field currentField = a;

      foreach (Field field in fieldsOnBoard)
      {
       field.SetNeighbours(fieldsOnBoard, 9, 9); 
      }

      // break walls to create cross road
      a.BreakWall(WallLocation.Right);
      b.BreakWall(WallLocation.Bottom);
      z.BreakWall(WallLocation.Right);
      z.BreakWall(WallLocation.Left);
      z.BreakWall(WallLocation.Top);
      z.BreakWall(WallLocation.Bottom);
      c.BreakWall(WallLocation.Left);
      d.BreakWall(WallLocation.Top);

      MockOrganiser organiser = new MockOrganiser();
      currentField = organiser.MockMoveFigure(a.X, a.Y, a.X, a.Y, fieldsOnBoard);

      Assert.AreEqual(currentField.X, a.X);
    }
  }
}
