using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using NFX.Serialization;
using NFX.Serialization.JSON;

namespace NFX.Web.Pay.PayPal
{
    /// <summary>
    /// Exception caused by PayPal API or NFX.Pay.PayPal infrastructure.
    /// </summary>
    public class PayPalPaymentException : PaymentException
    {
        #region .ctor

        public PayPalPaymentException()
        {
        }

        public PayPalPaymentException(string message)
          : base(message)
        {
        }

        public PayPalPaymentException(string message, Exception inner)
          : base(message, inner)
        {
        }

        protected PayPalPaymentException(SerializationInfo info, StreamingContext context)
          : base(info, context)
        {
        }

        #endregion

        public static PayPalPaymentException ComposeError(string header, Exception inner)
        {
            var webError = inner as System.Net.WebException;
            if (webError == null || webError.Response==null)
                return new PayPalPaymentException(header, inner);

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

            string statusMessage;

            var statusCode = (int)response.StatusCode;
            switch (statusCode)
            {
                case (200): statusMessage = StringConsts.PAYMENT_PAYPAL_200_STATUSCODE; break;
                case (201): statusMessage = StringConsts.PAYMENT_PAYPAL_201_STATUSCODE; break;
                case (400): statusMessage = StringConsts.PAYMENT_PAYPAL_400_STATUSCODE; break;
                case (401): statusMessage = StringConsts.PAYMENT_PAYPAL_401_STATUSCODE; break;
                case (402): statusMessage = StringConsts.PAYMENT_PAYPAL_402_STATUSCODE; break;
                case (403): statusMessage = StringConsts.PAYMENT_PAYPAL_403_STATUSCODE; break;
                case (404): statusMessage = StringConsts.PAYMENT_PAYPAL_404_STATUSCODE; break;
                case (405): statusMessage = StringConsts.PAYMENT_PAYPAL_405_STATUSCODE; break;
                case (415): statusMessage = StringConsts.PAYMENT_PAYPAL_415_STATUSCODE; break;
                case (422): statusMessage = StringConsts.PAYMENT_PAYPAL_422_STATUSCODE; break;
                case (429): statusMessage = StringConsts.PAYMENT_PAYPAL_429_STATUSCODE; break;
                default: statusMessage = StringConsts.PAYMENT_PAYPAL_UNKNOWN_STATUSCODE; break;
            }
            if (statusCode >= 500 && statusCode < 600)
                statusMessage = StringConsts.PAYMENT_PAYPAL_50x_STATUSCODE;

            var message = "{0}{1}Status: {2} - {3}{4}Response: {5}"
                            .Args(
                                header, System.Environment.NewLine,
                                statusCode, statusMessage, System.Environment.NewLine,
                                responseMessage);

            return new PayPalPaymentException(message, inner);
        }
    }
}
