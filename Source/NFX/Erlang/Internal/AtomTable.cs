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
// Author: Serge Aleynikov <saleyn@gmail.com>
// Created: 2013-08-27
// This is a clone of atom_table from
// https://github.com/saleyn/eixx/blob/master/include/eixx/marshal/atom.hpp
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace NFX.Erlang
{
  /// <summary>
  /// Non-garbage collected hash table for atoms.
  ///
  /// It stores strings as atoms so that atoms can be quickly compared to
  /// with O(1) complexity.  The instance of this table is statically maintained
  /// and its content is never cleared.  The table contains a unique
  /// list of strings represented as atoms added throughout the lifetime
  /// of the application.
  /// </summary>
  internal class AtomTable
  {
  #region CONSTS

    internal const int    MAX_ATOM_LEN        = 0xff;
    private  const int    DEFAULT_MAX_ATOMS   = 1024*1024;
    private  const string ENV_ATOM_TABLE_SIZE = "EI_ATOM_TABLE_SIZE";

    public   const int    EMPTY_INDEX         = 0;
    public   const int    TRUE_INDEX          = 1;
    public   const int    FALSE_INDEX         = 2;
    public   const int    UNDEFINED_INDEX     = 3;

  #endregion

  #region Static

    private static volatile AtomTable s_Instance;
    private static object             s_InstLock = new object();

  #endregion

  #region Fields

    List<string>                      m_Atoms;
    ConcurrentDictionary<string, int> m_Index;
    object                            m_Lock;

  #endregion

  #region .ctor

    private AtomTable()
    {
      int maxAtoms = defaultCapacity();
      m_Atoms = new List<string>(maxAtoms);
      m_Index = new ConcurrentDictionary<string, int>();
      m_Lock = new object();

      var a = new string[] { string.Empty, ErlConsts.TRUE, ErlConsts.FALSE, ErlConsts.UNDEFINED };
      for (int i = 0; i < a.Length; i++)
      {
        m_Atoms.Add(a[i]); m_Index[a[i]] = i;
      }
    }

  #endregion

  #region Props

    /// <summary>
    /// Returns true if AtomTable.Instance is initialized
    /// </summary>
    internal static bool Initialized { get { return s_Instance != null; } }

    /// <summary>
    /// Returns singleton instance of the atom table
    /// </summary>
    public static AtomTable Instance
    {
      get
      {
        // Note that the common language runtime resolves issues related to using
        // Double-Check Locking that are common in other environments:
        // http://msdn.microsoft.com/en-us/library/ff650316.aspx
        if (s_Instance == null)
          lock (s_InstLock)
          {
            if (s_Instance == null)
              s_Instance = new AtomTable();
          }
        return s_Instance;
      }
    }

    /// <summary>
    /// Returns the current number of atoms stored in the atom table
    /// </summary>
    public int Count { get { return m_Atoms.Count; } }

    /// <summary>
    /// Lookup an atom in the atom table by index
    /// </summary>
    public string this[int n] { get { return m_Atoms[n]; } }

    /// <summary>
    /// Lookup an atom in the atom table by name. If the atom is not
    /// present in the atom table - add it.  Return the index of the
    /// atom in the atom table
    /// </summary>
    public int this[string atom]
    {
      get
      {
        if (atom.Length > MAX_ATOM_LEN)
          throw new ErlException(StringConsts.ERL_ATOM_SIZE_TOO_LONG_ERROR);

        int n;
        if (m_Index.TryGetValue(atom, out n))
          return n;

        lock (m_Lock)
        {
          if (m_Index.TryGetValue(atom, out n))
            return n;

          if (m_Atoms.Count + 1 > m_Atoms.Capacity)
            throw new ErlException(StringConsts.ERL_ATOM_TABLE_IS_FULL_ERROR);

          m_Atoms.Add(atom);
          n = m_Atoms.Count - 1;
          m_Index.TryAdd(atom, n);
          return n;
        }
      }
    }

  #endregion

  #region Public

    /// <summary>
    /// Returns true if given atom is present in the atom table
    /// </summary>
    public bool Exists(string atom)
    {
      return IndexOf(atom) >= 0;
    }

    /// <summary>
    /// Returns index of atom in the atom table. If atom is not
    /// found returns -1
    /// </summary>
    public int IndexOf(string atom)
    {
      int i;
      return m_Index.TryGetValue(atom, out i) ? i : -1;
    }

  #endregion

  #region .pvt

    /// <summary>
    /// Returns the default atom table maximum size. The value can be
    /// changed by setting the EI_ATOM_TABLE_SIZE environment variable
    /// </summary>
    private static int defaultCapacity()
    {
      int n = App.Available
          ? App.ConfigRoot.NavigateSection(
              ErlConsts.ERLANG_CONFIG_SECTION)[ENV_ATOM_TABLE_SIZE].ValueAsInt()
          : DEFAULT_MAX_ATOMS;
      return n > 0 && n < 1024 * 1024 * 1024 ? n : DEFAULT_MAX_ATOMS;
    }

  #endregion
  }
}
