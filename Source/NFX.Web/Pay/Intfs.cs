/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2016 IT Adapter Inc.
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

using System.Collections.Generic;
using NFX.Environment;
using NFX.Financial;
using NFX.Instrumentation;

namespace NFX.Web.Pay
{
  /// <summary>
  /// Represents a web terminal for pay systems that tokenize sensitive CC data via a call to provider
  /// so that actual CC numbers never touch our servers in a plain form, instead tokens/nonces are supplied
  /// back by the provider tokenizer. This is needed for PCI compliance.
  /// </summary>
  public interface IPayWebTerminal
  {
    /// <summary>
    /// References pay system that this terminal services
    /// </summary>
    IPaySystem PaySystem { get; }

    /// <summary>
    /// Returns client script body that initializes WAVE.Pay by calling WAVE.Pay.init(...) to perform operation
    /// against the provider.
    /// </summary>
    object GetPayInit();
  }


  /// <summary>
  /// Describes an entity that can perform pay functions (i.e. charge, transfer)
  /// </summary>
  public interface IPaySystem: INamed
  {
    /// <summary>
    /// Returns a pay terminal is this payment provider supports it or null
    /// </summary>
    IPayWebTerminal WebTerminal { get; }

    /// <summary>
    /// Config node of params used inside <see cref="StartSession(ConnectionParameters, IPaySessionContext)"/> method
    /// if PayConnectionParameters parameter is null
    /// </summary>
    IConfigSectionNode DefaultSessionConnectParamsCfg { get; set; }

    /// <summary>
    /// Processing fee types, such as: included in amount and surcharged.
    /// </summary>
    ProcessingFeeKind ChargeFeeKind { get; }

    /// <summary>
    /// Processing fee types, such as: included in amount and surcharged.
    /// </summary>
    ProcessingFeeKind TransferFeeKind { get; }

    /// <summary>
    /// Returns currency ISOs that are supported by this isntance. The processing of charges/transafers may be done
    /// only in these currencies
    /// </summary>
    IEnumerable<string> SupportedCurrencies{ get; }

    /// <summary>
    /// Returns true if this system supports transaction type in the specified currency (optional)
    /// </summary>
    bool IsTransactionTypeSupported(TransactionType type, string currencyISO = null);

    /// <summary>
    /// Starts new pay session of system-specific type.
    /// If cParams parameter is null <see cref="DefaultSessionConnectParamsCfg"/> is used
    /// </summary>
    PaySession StartSession(ConnectionParameters cParams = null, IPaySessionContext context = null);
  }

  /// <summary>
  /// Describes an entity that can perform pay functions with several usefull interfaces in NFX style
  /// </summary>
  public interface IPaySystemImplementation: IPaySystem, IConfigurable, IInstrumentable
  {
    /// <summary>
    /// Specifies the log level for operations performed by Pay System.
    /// </summary>
    NFX.Log.MessageType LogLevel { get; set; }
  }
}
