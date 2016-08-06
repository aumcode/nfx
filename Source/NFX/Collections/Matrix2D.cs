
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
 * Revision: NFX 1.0  2013.12.18
 * Author: Denis Latushkin<dxwizard@gmail.com>
 */

using NFX;

namespace NFX.Collections
{
  /// <summary>
  /// Represents a two deminsional matrix of T.
  /// This class uses jagged arrays for internal implementation ensuring proper array sizing per matrix structure
  /// </summary>
  /// <typeparam name="T">Any desired type</typeparam>
  public class Matrix2D<T>: Matrix2DBase<T>
  {
    #region .ctor

      public Matrix2D(int width, int height): base(width, height)
      {
        Array = new T[height][];
        for (int y = 0; y < height; y++)
          Array[y] = new T[width];
      }

    #endregion

    #region Fields / Properties

      public readonly T[][] Array;

      public override T this[int x, int y]
      {
        get { return Array[y][x]; }
        set { Array[y][x] = value; }
      }

    #endregion

    #region Public

      public void Fill(T value)
      {
        for (int y = 0; y < Array.Length; y++)
          for (int x = 0; x < Array.Length; x++)
            Array[y][x] = value;
      }

    #endregion

  }//class

}
