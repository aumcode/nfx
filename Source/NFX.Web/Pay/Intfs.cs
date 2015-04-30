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

using NFX.Environment;
using NFX.Financial;
using NFX.Instrumentation;

namespace NFX.Web.Pay
{
  /// <summary>
  /// Describes an entity that can perform pay functions (i.e. charge, transfer)
  /// </summary>
  public interface IPaySystem: INamed
  {
    /// <summary>
    /// Config node of params used inside <see cref="StartSession(PayConnectionParameters)"/> method
    /// if PayConnectionParameters parameter is null 
    /// </summary>
    IConfigSectionNode DefaultSesssionConnectParamsCfg { get; set; }

    /// <summary>
    /// Starts new pay session of system-specific type.
    /// If cParams parameter is null <see cref="DefaultSesssionConnectParamsCfg"/> is used
    /// </summary>
    PaySession StartSession(PayConnectionParameters cParams = null);

    /// <summary>
    /// Preliminarily checks possibility of given transaction.
    /// Is not implemented in some providers (e.g. Stripe)
    /// </summary>
    PaymentException VerifyPotentialTransaction(PaySession session, ITransactionContext context, bool transfer, IActualAccountData from, IActualAccountData to, Amount amount);

    /// <summary>
    /// Charges funds from one account to another
    /// </summary>
    Transaction Charge(PaySession session, ITransactionContext context, Account from, Account to, Amount amount, bool capture = true, string description = null, object extraData = null);

    /// <summary>
    /// Completely or partialy refunds previuosly charged funds
    /// </summary>
    Transaction Refund(PaySession session, ITransactionContext context, ref Transaction charge, Amount? amount = null, string description = null, object extraData = null);

    /// <summary>
    /// Transfers funds from one account to another.
    /// 
    /// </summary>
    Transaction Transfer(PaySession session, ITransactionContext context, Account from, Account to, Amount amount, string description = null, object extraData = null);
  }

  /// <summary>
  /// Describes an entity that can perform pay functions with several usefull interfaces in NFX style
  /// </summary>
  public interface IPaySystemImplementation: IPaySystem, IConfigurable, IInstrumentable
  {
  }
}
