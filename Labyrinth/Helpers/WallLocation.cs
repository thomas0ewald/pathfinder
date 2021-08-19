using System;

namespace Labyrinth
{
  [Flags]
  public enum WallLocation
  {
    None = 0,
    Top = 1,
    Bottom = 2,
    Left = 4,
    Right = 8
  }
}
