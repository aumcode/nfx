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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NFX;

namespace NFX.Web.Pay
{
  /// <summary>
  /// Represents account type because some pay services (i.e. Stripe) requires this info
  /// </summary>
  public enum AccountType { Individual, Corporation };

  /// <summary>
  /// Represents actual data for supplied account object.
  /// Data represented by this interface is ALWAYS TRANSITIVE in memory as
  /// some fields are either never stored permanently (i.e. CVC) or ciphered in the store (account number)
  /// </summary>
  public interface IActualAccountData
  {
    Account Account { get; }

    string Identity { get; }

    bool IsNew { get; }
    object IdentityID { get; }

    bool IsWebTerminal { get; }
    object AccountID { get; }

    string FirstName { get; }
    string MiddleName { get; }
    string LastName { get; }

    string AccountTitle { get; }

    AccountType AccountType { get; }

    bool HadSuccessfullTransactions { get; }

    string IssuerID { get; }
    string IssuerName { get; }
    string IssuerPhone { get; }
    string IssuerEMail { get; }
    string IssuerUri { get; }

    string RoutingNumber { get; }

    string CardMaskedName { get; }
    string CardHolder { get; }
    DateTime? CardExpirationDate { get; }
    string CardVC { get; }

    bool IsCard { get; }

    string Phone { get; }
    string EMail { get; }

    IAddress BillingAddress { get; }
    IAddress ShippingAddress { get; }
  }

  /// <summary>
  /// Represents address
  /// </summary>
  public interface IAddress
  {
    string Address1 { get; }
    string Address2 { get; }
    string City { get; }
    string Region { get; }
    string PostalCode { get; }
    string Country { get; }

    string Company { get; }

    string Phone { get; }
    string EMail { get; }
  }

  /// <summary>
  /// Primitive (maybe temporary) implementation of IAddress
  /// </summary>
  public class Address: IAddress
  {
    public string Address1 { get; set; }

    public string Address2 { get; set; }

    public string City { get; set; }

    public string Region { get; set; }

    public string PostalCode { get; set; }

    public string Country { get; set; }

    public string Company { get; set; }

    public string Phone { get; set; }

    public string EMail { get; set; }

    public override bool Equals(object obj)
    {
      var other = obj as IAddress;
      if (other == null) return false;

      return PostalCode.EqualsIgnoreCase(other.PostalCode)
        && Phone.EqualsIgnoreCase(other.Phone)
        && EMail.EqualsIgnoreCase(other.EMail)
        && Company.EqualsIgnoreCase(other.Company)
        && Country.EqualsIgnoreCase(other.Country)
        && City.EqualsIgnoreCase(other.City)
        && Region.EqualsIgnoreCase(other.Region)
        && Address1.EqualsIgnoreCase(other.Address1)
        && Address2.EqualsIgnoreCase(other.Address2);
    }

    public override int GetHashCode()
    {
      return PostalCode.GetHashCodeIgnoreCase() ^ Address1.GetHashCodeIgnoreCase() ^ City.GetHashCodeIgnoreCase();
    }

    public override string ToString()
    {
      return "{0} {1} {2}".Args(City, Address1, PostalCode);
    }
  }

  public class ActualAccountData : IActualAccountData
  {
    public ActualAccountData(Account account) { m_Account = account; }

    private readonly Account m_Account;

    public Account Account { get { return m_Account; } }

    public string Identity { get; set; }
    public bool IsNew { get; set; }
    public object IdentityID { get; set; }
    public bool IsWebTerminal { get; set; }
    public object AccountID { get; set; }

    public string FirstName { get; set; }
    public string MiddleName { get; set; }
    public string LastName { get; set; }
    public string AccountTitle { get { return string.Join(" ", new string[] { FirstName, MiddleName, LastName }.Where(s => s.IsNotNullOrWhiteSpace())); } }

    public string Phone { get; set; }
    public string EMail { get; set; }

    public IAddress BillingAddress { get; set; }
    public IAddress ShippingAddress { get; set; }

    public AccountType AccountType { get; set; }

    public bool HadSuccessfullTransactions { get; set; }

    public string IssuerID { get; set; }
    public string IssuerName { get; set; }
    public string IssuerPhone { get; set; }
    public string IssuerEMail { get; set; }
    public string IssuerUri { get; set; }

    public string RoutingNumber { get; set; }

    public string CardMaskedName { get; set; }
    public string CardHolder { get; set; }
    public DateTime? CardExpirationDate { get; set; }
    public string CardVC { get; set; }
    public bool IsCard { get { return RoutingNumber.IsNullOrWhiteSpace(); } }
  }
}
