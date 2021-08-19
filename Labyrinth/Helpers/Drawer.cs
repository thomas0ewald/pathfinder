using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Labyrinth
{
  /// <summary>
  /// responsible for drawing the board
  /// </summary>
  public class Drawer
  {
    public Canvas Board { get; set; }

    private int mFieldWidth;

    private int mFieldHeight;

    private const int OFFSET_OUTSIDE = 1;
    /// <summary>
    /// updates a field on the board
    /// </summary>
    /// <param name="aField">the field to update</param>
    public void DrawField(Field aField, int aFieldsForWidth, int aFieldsForHeight)
    {
      mFieldWidth = (int)Math.Max(Board.Width / aFieldsForWidth, 3);
      mFieldHeight = (int)Math.Max(Board.Height / aFieldsForHeight, 3);

      int xOffset = mFieldWidth * (aField.X + OFFSET_OUTSIDE);
      int yOffset = mFieldHeight * (aField.Y + OFFSET_OUTSIDE);

      Line wallSide = new Line();

      if (aField.Walls.HasFlag(WallLocation.Top))
      {
        wallSide = new Line
          {
            X1 = xOffset,
            Y1 = yOffset,
            X2 = xOffset + mFieldWidth,
            Y2 = yOffset,
            Stroke = Brushes.Black
          };
        Board.Children.Add(wallSide);
      }

      if (aField.Walls.HasFlag(WallLocation.Bottom))
      {
        wallSide = new Line
          {
            X1 = xOffset,
            Y1 = yOffset + mFieldHeight,
            X2 = xOffset + mFieldWidth,
            Y2 = yOffset + mFieldHeight,
            Stroke = Brushes.Black
          };
        Board.Children.Add(wallSide);
      }

      if (aField.Walls.HasFlag(WallLocation.Left))
      {
        wallSide = new Line
          {
            X1 = xOffset,
            Y1 = yOffset,
            X2 = xOffset,
            Y2 = yOffset + mFieldHeight,
            Stroke = Brushes.Black
          };
        Board.Children.Add(wallSide);
      }

      if (aField.Walls.HasFlag(WallLocation.Right))
      {
        wallSide = new Line
          {
            X1 = xOffset + mFieldWidth,
            Y1 = yOffset,
            X2 = xOffset + mFieldWidth,
            Y2 = yOffset + mFieldHeight,
            Stroke = Brushes.Black
          };
        Board.Children.Add(wallSide);
      }
    }

    public void FillVisitedField(int aX, int aY, int? aStep, Brush aColor)
    {
      int posX = (aX + OFFSET_OUTSIDE) * mFieldWidth;
      int posY = (aY + OFFSET_OUTSIDE) * mFieldHeight;

      Rectangle field = new Rectangle
        {
          Width = mFieldWidth -4,
          Height = mFieldHeight -4,
          Fill = aColor,
          Visibility = Visibility.Visible
        };
      Board.Children.Add(field);
      Canvas.SetTop(field, posY + 2);
      Canvas.SetLeft(field, posX + 2);

      TextBlock stepNumber = new TextBlock { Text = aStep.ToString(), Foreground = Brushes.White};
      Board.Children.Add(stepNumber);
      Canvas.SetLeft(stepNumber, posX + 2);
      Canvas.SetTop(stepNumber, posY + 2);
    }

    /// <summary>
    /// Draws a point to the board to indicate where the figure is standing on the board
    /// </summary>
    public void DrawFigure(FieldCoordinate aFieldCoordinates)
    {
      int x = aFieldCoordinates.X + OFFSET_OUTSIDE;
      int y = aFieldCoordinates.Y + OFFSET_OUTSIDE;

      int xFieldMiddle = (mFieldWidth * x + mFieldWidth * (x + 1)) / 2;
      int yFieldMiddle = (mFieldHeight * y + mFieldHeight * (y + 1)) / 2;
      EllipseGeometry figure = new EllipseGeometry(new Point(xFieldMiddle, yFieldMiddle), mFieldWidth / 2.0, mFieldHeight / 2.0);
      
      Path path = new Path { Stroke = Brushes.Blue, Fill = Brushes.LightBlue, Data = figure };
      Board.Children.Add(path);
    }

    public void ClearBoard()
    {
      Board.Children.Clear();
    }
  }
}
