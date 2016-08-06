
/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2014 IT Adapter Inc / 2015 Aum Code LLC
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
</FILE_LICENSE>*/


/* NFX by ITAdapter
 * Originated: 2006.01
 * Revision: NFX 1.0  2013.12.23
 * Author: Denis Latushkin<dxwizard@gmail.com>
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

using NFX;
using NFX.Environment;

namespace NFX.Collections
{
  public abstract class Matrix2DBase<T>: MatrixBase<T>
  {
    #region .ctor

      public Matrix2DBase(int width, int height)
      {
        if (width <= 0 || height <= 0)
          throw new NFXException(StringConsts.ARGUMENT_ERROR + GetType().Name + ".ctor(width>0&height>0)");

        Width = width;
        Height = height;
      }

    #endregion

    #region Properties

      public readonly int Width, Height;

    #endregion

    #region Abstract

      public abstract T this[int x, int y] { get; set; }

    #endregion

    #region MatrixBase<T> implementation

      public override int Rank
      {
        get { return 2; }
      }

      public override int GetLowerBound(int dimension)
      {
        return 0;
      }

      public override int GetUpperBound(int dimension)
      {
        switch (dimension)
        {
          case 0:
            return Width;
          case 1:
            return Height;
          default:
            throw new NFXException(StringConsts.ARGUMENT_ERROR + GetType().Name + ".GetUpperBound(dimension>=0&<2)");
        }
      }

      public override IEnumerator<T> GetMatrixEnumerator()
      {
        for (int y = 0; y < Height; y++)
          for (int x = 0; x < Width; x++)
            yield return this[x, y];
      }

    #endregion

    #region Object overrides

      public override string ToString()
      {
        StringBuilder b = new StringBuilder();

        for (int y = 0; y < Height; y++)
        {
          if (y != 0)
            b.AppendLine();

          for (int x = 0; x < Width; x++)
          {
            if (x != 0)
              b.Append(' ');

            b.Append(this[x, y]);
          }
        }

        return b.ToString();
      }

    #endregion
  }//class

}
