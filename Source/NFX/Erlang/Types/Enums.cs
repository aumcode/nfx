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
// Author:  Serge Aleynikov <saleyn@gmail.com>
// Created: 2013-08-26
// This code is derived from:
// https://github.com/saleyn/otp.net

namespace NFX.Erlang
{
  /// <summary>
  /// Defines ordering semantics for Erlang types
  /// </summary>
  public enum ErlTypeOrder
  {
    ErlObject = 0,
    ErlAtom,
    ErlBinary,
    ErlBoolean,
    ErlByte,
    ErlDouble,
    ErlLong,
    ErlList,
    ErlPid,
    ErlPort,
    ErlRef,
    ErlString,
    ErlTuple,
    ErlVar
  }

  /// <summary>
  /// Tags used for external format serialization
  /// </summary>
  /// <remarks>
  /// https://github.com/erlang/otp/blob/master/lib/erl_interface/include/ei.h
  /// </remarks>
  internal enum ErlExternalTag
  {
    SmallInt        = 97,
    Int             = 98,
    Float           = 99,
    NewFloat        = 70,
    Atom            = 100,
    SmallAtom       = 115,
    AtomUtf8        = 118,
    SmallAtomUtf8   = 119,
    Ref             = 101,
    NewRef          = 114,
    Port            = 102,
    Pid             = 103,
    SmallTuple      = 104,
    LargeTuple      = 105,
    Nil             = 106,
    String          = 107,
    List            = 108,
    Bin             = 109,
    SmallBigInt     = 110,
    LargeBigInt     = 111,

    Version         = 131,
  }
}
