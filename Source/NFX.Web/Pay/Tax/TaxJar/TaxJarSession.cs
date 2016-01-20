using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NFX.Security;

namespace NFX.Web.Pay.Tax.TaxJar
{
  public class TaxJarSession: TaxSession
  {
    public TaxJarSession(TaxCalculator taxCalculator, TaxConnectionParameters cParams): base(taxCalculator, cParams)
    {
    }

    public string Email 
    { 
      get 
      {
        if (m_User == null || m_User == User.Fake) return string.Empty;
        var cred = m_User.Credentials as TaxJarCredentials;
        if (cred == null) return string.Empty;
        return cred.Email; 
      }
    }

    public string ApiKey 
    { 
      get 
      {
        if (m_User == null || m_User == User.Fake) return string.Empty;
        var cred = m_User.Credentials as TaxJarCredentials;
        if (cred == null) return string.Empty;
        return cred.ApiKey; 
      }
    }
  }
}
