/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2017 ITAdapter Corp. Inc.
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
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;

using NFX.Serialization.JSON;

namespace NFX.Web.Shipping
{
  public class ShippingException : Exception
  {
    public static ShippingException ComposeError(string header, Exception inner)
    {
        var webError = inner as System.Net.WebException;
        if (webError == null || webError.Response==null)
            return new ShippingException(header, inner);

        var response = (HttpWebResponse)webError.Response;

        var responseMessage = string.Empty;
        try
        {
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream))
            {
                var responseStr = reader.ReadToEnd();
                var responseJSON = responseStr.JSONToDataObject() as JSONDataMap;
                if (responseJSON != null)
                    responseMessage = responseJSON.ToJSON();
            }
        }
        catch (Exception)
        {
        }

        var statusCode = (int)response.StatusCode;

        var message = "{0}{1}Status: {2}{3}Response: {4}"
                        .Args(header, System.Environment.NewLine,
                              statusCode, System.Environment.NewLine,
                              responseMessage);

        return new ShippingException(message, inner);
    }

    #region .ctor

      public ShippingException()
      {
      }

      public ShippingException(string message)
        : base(message)
      {
      }

      public ShippingException(string message, Exception inner)
        : base(message, inner)
      {
      }

      protected ShippingException(SerializationInfo info, StreamingContext context)
        : base(info, context)
      {
      }

    #endregion
  }

  public class ValidateShippingAddressException : ShippingException
  {
    #region .ctor

      public ValidateShippingAddressException()
        : base()
      {
      }

      public ValidateShippingAddressException(string message, string details)
        : base(message)
      {
        m_Details = details;
      }

      public ValidateShippingAddressException(string message, string details, Exception inner)
        : base(message, inner)
      {
        m_Details = details;
      }

      protected ValidateShippingAddressException(SerializationInfo info, StreamingContext context)
        : base(info, context)
      {
      }

    #endregion

    private readonly string m_Details;

    public string Details { get { return m_Details; } }
  }
}
