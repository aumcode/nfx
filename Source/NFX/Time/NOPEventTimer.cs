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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.ApplicationModel;

namespace NFX.Time
{
  /// <summary>
  /// Normally this class should never be used as the dafult EventTimer is always present instead of nop
  /// </summary>
  public sealed class NOPEventTimer : ApplicationComponent, IEventTimerImplementation
  {
    private static NOPEventTimer s_Instance = new NOPEventTimer();

    public NOPEventTimer() {}

    public static NOPEventTimer Instance { get { return s_Instance;}}



    public int ResolutionMs{ get { return 1000;} set {}}

    public void __InternalRegisterEvent(Event evt)
    {

    }

    public void __InternalUnRegisterEvent(Event evt)
    {

    }


    public IRegistry<Event> Events
    {
      get { return new Registry<Event>(); }
    }

    public bool InstrumentationEnabled { get {return false; } set {}}


    public IEnumerable<KeyValuePair<string, Type>> ExternalParameters
    {
      get { return Enumerable.Empty<KeyValuePair<string, Type>>(); }
    }

    public IEnumerable<KeyValuePair<string, Type>> ExternalParametersForGroups(params string[] groups)
    {
      return Enumerable.Empty<KeyValuePair<string, Type>>();
    }

    public bool ExternalGetParameter(string name, out object value, params string[] groups)
    {
      value = null;
      return false;
    }

    public bool ExternalSetParameter(string name, object value, params string[] groups)
    {
      return false;
    }

    public void Configure(Environment.IConfigSectionNode node)
    {

    }
  }
}
