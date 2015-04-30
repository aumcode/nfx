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

using NFX;

namespace NFX.Web.Pay
{

  /// <summary>
  /// Represents an account data vector that is - 
  /// type of account identity (i.e. 'customer'), identity id (i.e. customer number '125') and account id within this identity (i.e. ref to customer #125 card '223322.....')
  /// </summary>
  public struct Account : IEquatable<Account>
  {
    private static Account s_EmptyInstance = new Account(null, null, null);

    public static Account EmptyInstance { get { return s_EmptyInstance; } }

    public Account(object identity, object identityID, object accountID)
      : this()
    {
      Identity = identity;
      IdentityID = identityID;
      AccountID = accountID;
    }

    /// <summary>
    /// For example 'customer' - name of table
    /// </summary>
    public object Identity { get; private set; }

    /// <summary>
    /// For example '125' - id of customer table row 125
    /// </summary>
    public object IdentityID { get; private set; }

    /// <summary>
    /// Account id within identity id domain.
    /// For example '2' - id of method of payment for customer #125
    /// </summary>
    public object AccountID { get; private set; }

    public bool IsEmpty { get { return Identity == null && IdentityID == null && AccountID == null; } }

    #region Object overrides

    public override string ToString()
    {
      if (IsEmpty)
        return "[EMPTY]";
      else
        return "{0}, {1}, {2}".Args(Identity, IdentityID, AccountID);
    }

    public override int GetHashCode()
    {
      return (Identity == null ? 0 : Identity.GetHashCode())
        ^ (IdentityID == null ? 0 : IdentityID.GetHashCode())
        ^ (AccountID == null ? 0 : AccountID.GetHashCode());
    }

    public override bool Equals(object obj)
    {
      if (!(obj is Account)) return false;
      return Equals((Account)obj);
    }

    public bool Equals(Account other)
    {
      return object.Equals(Identity, other.Identity)
        && object.Equals(IdentityID, other.IdentityID)
        && object.Equals(AccountID, other.AccountID);
    }

    public static bool operator ==(Account account0, Account account1)
    {
      return account0.Equals(account1);
    }

    public static bool operator !=(Account account0, Account account1)
    {
      return !account0.Equals(account1);
    }

    #endregion

  } 
}