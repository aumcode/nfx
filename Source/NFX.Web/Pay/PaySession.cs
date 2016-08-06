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
using System.Threading;
using System.Diagnostics;

using NFX;
using NFX.Environment;
using NFX.Security;
using NFX.Financial;

namespace NFX.Web.Pay
{
  /// <summary>
  /// Represents session of PaySystem.
  /// All PaySystem operation requires session as mandatory parameter
  /// </summary>
  public abstract class PaySession : DisposableObject, INamed
  {
    #region .ctor

      protected PaySession(PaySystem paySystem, PayConnectionParameters cParams)
      {
        if (paySystem == null || cParams == null)
          throw new PaymentException(StringConsts.ARGUMENT_ERROR + this.GetType().Name + ".ctor(paySystem is not null and cParams is not null)");

        m_PaySystem = paySystem;

        m_Name = cParams.Name;

        m_User = cParams.User;

        lock (m_PaySystem.m_Sessions)
          m_PaySystem.m_Sessions.Add(this);
      }

      protected override void Destructor()
      {
        if (m_PaySystem != null)
          lock (m_PaySystem.m_Sessions)
            m_PaySystem.m_Sessions.Remove(this);

        base.Destructor();
      }

    #endregion

    #region Pvt/Prot/Int Fields

      private readonly PaySystem m_PaySystem;
      private readonly string m_Name;
      protected readonly User m_User;

    #endregion

    #region Properties

      protected PaySystem PaySystem { get { return m_PaySystem; } }
      public string Name { get { return m_Name; } }
      public User User { get { return m_User; } }

      public bool IsValid { get { return m_User != null && m_User != Security.User.Fake; } }
    #endregion

    #region Public

      /// <summary>
      /// Has the same semantics as corresponding PaySystem method executed in context of this session
      /// </summary>
      public PaymentException VerifyPotentialTransaction(ITransactionContext context, bool transfer, IActualAccountData from, IActualAccountData to, Amount amount)
      {
        return m_PaySystem.VerifyPotentialTransaction(this, context, transfer, from, to, amount);
      }

      /// <summary>
      /// Has the same semantics as corresponding PaySystem method executed in context of this session
      /// </summary>
      public Transaction Charge(ITransactionContext context, Account from, Account to, Amount amount, bool capture = true, string description = null, object extraData = null)
      {
        return m_PaySystem.Charge(this, context, from, to, amount, capture, description, extraData);
      }

      /// <summary>
      /// Has the same semantics as corresponding PaySystem method executed in context of this session
      /// </summary>
      public void Capture(ITransactionContext context, ref Transaction charge, Amount? amount = null, string description = null, object extraData = null)
      {
        m_PaySystem.Capture(this, context, ref charge, amount, description, extraData);
      }

      /// <summary>
      /// Has the same semantics as corresponding PaySystem method executed in context of this session
      /// </summary>
      public Transaction Refund(ITransactionContext context, ref Transaction charge, Amount? amount = null, string description = null, object extraData = null)
      {
        return m_PaySystem.Refund(this, context, ref charge, amount, description, extraData);
      }

      /// <summary>
      /// Has the same semantics as corresponding PaySystem method executed in context of this session
      /// </summary>
      public Transaction Transfer(ITransactionContext context, Account from, Account to, Amount amount, string description = null, object extraData = null)
      {
        return m_PaySystem.Transfer(this, context, from, to, amount, description, extraData);
      }

    #endregion

  } //PaySession

}
