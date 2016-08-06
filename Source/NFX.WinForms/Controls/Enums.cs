using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFX.WinForms.Controls
{
    /// <summary>
    /// Defines horizontal alignment
    /// </summary>
    public enum HAlignment
    {
      Left = 0,
      Center,
      Right,
      Near,
      Far
    }


    /// <summary>
    /// Defines types of background brushes
    /// </summary>
    public enum BGKind
    {
      Solid = 0,
      Hatched,
      VerticalGradient,
      HorizontalGradient,
      ForwardDiagonalGradient,
      BackwardDiagonalGradient
    }


    /// <summary>
    /// Defines directions for sorting in grid columns
    /// </summary>
    public enum SortDirection
    {
      FIRST = 0,


      None = 0,
      Up,
      Down,


      LAST = Down
    }

}
