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
using System.Runtime.Serialization;
using System.Net;

using NFX;
using NFX.Environment;
using NFX.Serialization.JSON;

namespace NFX.Web.Pay.Tax
{
  /// <summary>
  /// General NFX tax system specific exception
  /// </summary>
  public class TaxException: NFXException
  {
    /// <summary>
    /// Constructs exception composing details from response, method specific error description and actual inner exception
    /// </summary>
    /// <param name="response">Response of failed request</param>
    /// <param name="stripeErrorMessage">Method specific error description</param>
    /// <param name="inner">Actual inner exception</param>
    /// <returns>Composed exception</returns>
    public static TaxException Compose(HttpWebResponse response, string stripeErrorMessage, Exception inner)
    {
      int statusCode = (int)response.StatusCode;

      string responseErrMsg = string.Empty;
      try
      {
        using(var reponseStream  = response.GetResponseStream())
        {
          using (var responseReader = new System.IO.StreamReader(reponseStream))
          {
            string responseStr = responseReader.ReadToEnd();
            dynamic responseObj = responseStr.JSONToDynamic();
            var sb = new StringBuilder();
            foreach (var item in ((JSONDataMap)responseObj.Data))
            {
              sb.AppendFormatLine("{0}: {1}", item.Key, item.Value);
            }
            responseErrMsg = sb.ToString();
          }
        }
      }
      catch (Exception)
      {
        // dlatushkin 2014/04/07:
        // there is no way to test some cases (50X errors for example)
        // so try/catch is used to swallow exception
      }

      string specificError = System.Environment.NewLine;
      if (responseErrMsg.IsNotNullOrWhiteSpace())
        specificError += StringConsts.PAYMENT_STRIPE_ERR_MSG_ERROR.Args( responseErrMsg) + System.Environment.NewLine;

      specificError += stripeErrorMessage;

      TaxException ex = new TaxException(specificError, inner);

      return ex;
    }

    public TaxException()
    {
    }

    public TaxException(string message)
      : base(message)
    {
    }

    public TaxException(string message, Exception inner)
      : base(message, inner)
    {
    }

    protected TaxException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {

    }

  } //TaxException

}
