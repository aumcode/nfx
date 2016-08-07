/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2014 Dmitriy Khmaladze, IT Adapter Inc / 2015-2016 Aum Code LLC
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

namespace NFX.Erlang
{
  public static class ConstAtoms
  {
    internal static AtomTable AtomTable = AtomTable.Instance; // To ensure it gets created before the following atoms

    public static ErlAtom ANY           = new ErlAtom(ErlConsts.ANY);
    public static ErlAtom BadRpc        = new ErlAtom("badrpc");
    public static ErlAtom Call          = new ErlAtom("call");
    public static ErlAtom Cast          = new ErlAtom("cast");
    public static ErlAtom Erlang        = new ErlAtom("erlang");
    public static ErlAtom Error         = new ErlAtom("error");
    public static ErlAtom Format        = new ErlAtom("format");
    public static ErlAtom GenCast       = new ErlAtom("$gen_cast");
    public static ErlAtom Io_Lib        = new ErlAtom("io_lib");
    public static ErlAtom Latin1        = new ErlAtom("latin1");
    public static ErlAtom Normal        = new ErlAtom("normal");
    public static ErlAtom NoProc        = new ErlAtom("noproc");
    public static ErlAtom NoConnection  = new ErlAtom("noconnection");
    public static ErlAtom Ok            = new ErlAtom("ok");
    public static ErlAtom Request       = new ErlAtom("request");
    public static ErlAtom Rex           = new ErlAtom("rex");
    public static ErlAtom Rpc           = new ErlAtom("rpc");
    public static ErlAtom Undefined     = new ErlAtom("undefined");
    public static ErlAtom Unsupported   = new ErlAtom("unsupported");
    public static ErlAtom User          = new ErlAtom("user");
  }
}
