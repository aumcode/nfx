using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NFX.Environment;

namespace NFX.Web.Pay.Tax.TaxJar
{
  using NFX.Financial;

  public class TaxJarCalculator: TaxCalculator
  {
    #region consts

      public const string BASE_URI = "https://api.taxjar.com/v2";

      private const string CALC_URI = BASE_URI + "/taxes";

      private const string AUTH_HEADER_KEY = "Authorization";
      private const string AUTH_HEADER_VALUE_PATTERN = "Bearer {0}";

      private const string PRM_AMOUNT = "amount";
      private const string PRM_SHIPPING = "shipping";

      private const string PRM_TO_COUNTRY = "to_country";
      private const string PRM_TO_ZIP = "to_zip";
      private const string PRM_TO_STATE = "to_state";
      private const string PRM_FROM_COUNTRY = "from_country";
      private const string PRM_FROM_ZIP = "from_zip";
      private const string PRM_FROM_STATE = "from_state";

    #endregion

    #region .ctor

      public TaxJarCalculator(string name, IConfigSectionNode node) : base(name, node) {}

      public TaxJarCalculator(string name, IConfigSectionNode node, object director) : base(name, node, director) {}

    #endregion

    public override ITaxStructured DoCalc(
        TaxSession session,
        IAddress fromAddress, 
        IAddress toAddress, 
        Amount amount, 
        Amount shipping)
    {
      return DoCalc(session as TaxJarSession, fromAddress, toAddress, amount, shipping);
    }

    public ITaxStructured DoCalc(
        TaxJarSession session,
        IAddress fromAddress, 
        IAddress toAddress, 
        Amount amount, 
        Amount shipping)
    {
      if (fromAddress == null)
        throw new TaxException(StringConsts.TAX_TAXJAR_TOADDRESS_ISNT_SUPPLIED_ERROR + this.GetType() + ".DoCalc");

      if (toAddress == null)
        throw new TaxException(StringConsts.TAX_TAXJAR_TOADDRESS_ISNT_SUPPLIED_ERROR + this.GetType() + ".DoCalc");

      try
      {
        var bodyPrms = new Dictionary<string, string>() { 
          {PRM_AMOUNT, amount.Value.ToString()},
          {PRM_SHIPPING, 0.ToString()}
        };

        bodyPrms.Add(PRM_FROM_COUNTRY, fromAddress.Country);
        bodyPrms.Add(PRM_FROM_STATE, fromAddress.Region);
        bodyPrms.Add(PRM_FROM_ZIP, fromAddress.PostalCode);

        bodyPrms.Add(PRM_TO_COUNTRY, toAddress.Country);
        bodyPrms.Add(PRM_TO_STATE, toAddress.Region);
        bodyPrms.Add(PRM_TO_ZIP, toAddress.PostalCode);

        var prms = new WebClient.RequestParams() {
          Uri = new Uri(CALC_URI),
          Caller = this,
          Headers = new Dictionary<string, string>() { { AUTH_HEADER_KEY, AUTH_HEADER_VALUE_PATTERN.Args(session.ApiKey) } },
          Method = HTTPRequestMethod.POST,
          BodyParameters = bodyPrms
        };

        dynamic obj = WebClient.GetJsonAsDynamic(prms);

        var result = new TaxStructured();

        result.Total = (decimal)obj.Data["tax"]["amount_to_collect"];

        return result;
      }
      catch (Exception ex)
      {
        var wex = ex as System.Net.WebException;

        if (wex != null)
        {
          var response = wex.Response as System.Net.HttpWebResponse;
          if (response != null)
          {
            string errorMessage = this.GetType().Name +
              ".DoCalc(fromAddress: {0}, toAddress: {1}, amount: '{2}', shipping: '{3}')".Args(fromAddress, toAddress, amount, shipping);
            var taxEx = TaxException.Compose(response, errorMessage, wex);
            throw taxEx;  
          }
        }

        throw new TaxException(StringConsts.TAX_CALC_ERROR + this.GetType()
          + " .Calc(session='{0}', fromAddress='{1}', toAddress='{2}', amount='{3}', shipping='{4}')"
            .Args(session, fromAddress, toAddress, amount, shipping), ex);
      }
    }

    protected override TaxConnectionParameters MakeDefaultSessionConnectParams(Environment.IConfigSectionNode paramsSection)
    {
      return TaxConnectionParameters.Make<TaxJarConnectionParameters>(paramsSection);
    }

    protected override TaxSession DoStartSession(TaxConnectionParameters cParams = null)
    {
      TaxConnectionParameters sessionParams = cParams ?? DefaultSessionConnectParams;
      return this.StartSession((TaxJarConnectionParameters)sessionParams);
    }

    public TaxJarSession StartSession(TaxJarConnectionParameters cParams)
    {
      return new TaxJarSession(this, cParams);
    }
  }
}
