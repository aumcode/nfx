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

using NFX.Environment;
using NFX.ApplicationModel;

namespace NFX.IO.Net.Gate
{
  /// <summary>
  /// Allow/Deny
  /// </summary>
  public enum GateAction {Deny=0, Allow}

  /// <summary>
  /// Stipulates general contract for nrtwork gates - entities similar to firewall.
  /// Network gates allow/deny in/out traffic based on a set of rules
  /// </summary>
  public interface INetGate : IApplicationComponent
  {

    /// <summary>
    /// When gate is not enabled it allows all traffic bypassing any rules
    /// </summary>
    bool Enabled {get; }

    /// <summary>
    /// Checks whether the specified traffic is allowed or denied
    /// </summary>
    GateAction CheckTraffic(ITraffic traffic);


    /// <summary>
    /// Checks whether the specified traffic is allowed or denied.
    /// Returns the rule that determined the allow/deny outcome or null when no rule matched
    /// </summary>
    GateAction CheckTraffic(ITraffic traffic, out Rule rule);

    /// <summary>
    /// Increases the named variable in the network scope which this specified traffic falls under
    /// </summary>
    void IncreaseVariable(TrafficDirection direction, string address, string varName, int value);

    /// <summary>
    /// Sets the named variable in the network scope which this specified traffic falls under
    /// </summary>
    void SetVariable(TrafficDirection direction, string address, string varName, int value);
  }

  public interface INetGateImplementation : INetGate, IConfigurable
  {

  }


  /// <summary>
  /// Represents an implementation of INetGate that allows all traffic
  /// </summary>
  public class NOPNetGate : ApplicationComponent,  INetGate
  {
    /// <summary>
    /// Default instance of INetGate implementation that allows all traffic
    /// </summary>
    public static readonly NOPNetGate Instance = new NOPNetGate();

    protected NOPNetGate():base() {}

    public bool Enabled {get{return false;}}


    public GateAction CheckTraffic(ITraffic traffic)
    {
      return GateAction.Allow;
    }

    public GateAction CheckTraffic(ITraffic traffic, out Rule rule)
    {
      rule = null;
      return GateAction.Allow;
    }

    public void IncreaseVariable(TrafficDirection direction, string address, string varName, int value)
    {
    }

    public void SetVariable(TrafficDirection direction, string address, string varName, int value)
    {
    }
  }

}
