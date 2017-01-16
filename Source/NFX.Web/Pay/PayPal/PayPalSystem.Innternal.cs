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
using System.Linq;
using System.Collections.Generic;

using NFX;
using NFX.Financial;
using NFX.Serialization.JSON;
using NFX.Log;
using System.Net;
using System.IO;

namespace NFX.Web.Pay.PayPal
{
  public partial class PayPalSystem : PaySystem
  {
    #region CONSTS
    private const string URI_OAUTH_TOKEN_GENERATE = "/v1/oauth2/token";

    private const string URI_PAYOUTS_CREATE = "/v1/payments/payouts";
    private const string URI_PAYOUTS_GET = "/v1/payments/payouts/{0}";
    private const string URI_PAYOUT_ITEM_GET = "/v1/payments/payouts-item/{0}";
    private const string URI_PAYOUT_ITEM_CANCEL = "/v1/payments/payouts-item/{0}/cancel";
    #endregion

    #region Inner
    private enum payoutRecipientType
    {
      UNDEFINED,
      EMAIL,
      PHONE,
      PAYPAL_ID
    }

    private enum payoutBatchStatus
    {
      UNDEFINED,
      ACKNOWLEDGED,
      DENIED,
      PENDING,
      PROCESSING,
      SUCCESS,
      NEW,
      CANCELED
    }

    private enum payoutTransactionStatus
    {
      UNDEFINED,
      SUCCESS,
      DENIED,
      PENDING,
      PROCESSING,
      FAILED,
      UNCLAIMED,
      RETURNED,
      ONHOLD,
      BLOCKED,
      CANCELLED
    }

    private class errorDetails : IJSONWritable
    {
      public errorDetails(JSONDataMap map)
      {
        Field = map["field"].AsString();
        Issue = map["issue"].AsString();
      }

      public string Field { get; set; }
      public string Issue { get; set; }

      public JSONDataMap ToJSONDataMap() { return new JSONDataMap { { "field", Field }, { "issue", Issue } }; }
      public void WriteAsJSON(TextWriter wri, int nestingLevel, JSONWritingOptions options = null)  { ToJSONDataMap().ToJSON(wri, options); }
    }

    private class error
    {
      public error(JSONDataMap map)
      {
        Name = map["name"].AsString();
        Message = map["message"].AsString();
        var details = (map["details"] as JSONDataArray) ?? Enumerable.Empty<object>();
        Details = new List<errorDetails>(details.Select(d => new errorDetails(d as JSONDataMap)));
      }

      public string Name { get; set; }
      public string Message { get; set; }
      public List<errorDetails> Details { get; set; }
    }

    private class payoutItemDetails
    {
      public payoutItemDetails(JSONDataMap map)
      {
        PayoutItemID = map["payout_item_id"].AsString();
        if (PayoutItemID.IsNullOrWhiteSpace())
          throw new PaymentException(GetType().Name + ".ctor(payout_item_id is null)");
        TransactionID = map["transaction_id"].AsString();
        TransactionStatus = map["transaction_status"].AsEnum(payoutTransactionStatus.UNDEFINED);
        if (TransactionStatus == payoutTransactionStatus.UNDEFINED)
          throw new PaymentException(GetType().Name + ".ctor(transaction_status is UNDEFINED)");
        var fee = parseAmount(map["payout_item_fee"] as JSONDataMap);
        if (!@fee.HasValue)
          throw new PaymentException(GetType().Name + ".ctor(payout_item_fee is null)");
        PayoutItemFee = fee.Value;
        PayoutBatchID = map["payout_batch_id"].AsString();
        SenderBatchID = map["sender_batch_id"].AsString();
        if (map.ContainsKey("payout_item"))
          PayoutItem = new payoutItem(map["payout_item"] as JSONDataMap);
        else throw new PaymentException(GetType().Name + ".ctor(payout_item is null)");
        TimeProcessed = map["time_processed"].AsNullableDateTime();
        if (map.ContainsKey("errors"))
          Error = new error(map["errors"] as JSONDataMap);
      }

      public string PayoutItemID { get; set; }
      public string TransactionID { get; set; }
      public payoutTransactionStatus TransactionStatus { get; set; }
      public Amount PayoutItemFee { get; set; }
      public string PayoutBatchID { get; set; }
      public string SenderBatchID { get; set; }
      public payoutItem PayoutItem { get; set; }
      public DateTime? TimeProcessed { get; set; }
      public error Error { get; set; }

      public string Token
      {
        get
        {
          if (PayoutItemID.IsNullOrWhiteSpace()) return PayoutBatchID;
          return "{0}:{1}:{2}".Args(PayoutBatchID, PayoutItemID, TransactionID);
        }
      }

      public decimal Amount { get { return PayoutItem.Amount.Value; } }
    }

    private class payoutBatchHeader
    {
      public payoutBatchHeader(JSONDataMap map)
      {
        PayoutBatchID = map["payout_batch_id"].AsString();
        if (PayoutBatchID.IsNullOrWhiteSpace())
          throw new PaymentException(GetType().Name + ".ctor(payout_batch_id is null)");
        BatchStatus = map["batch_status"].AsEnum(payoutBatchStatus.UNDEFINED);
        if (BatchStatus == payoutBatchStatus.UNDEFINED)
          throw new PaymentException(GetType().Name + ".ctor(batch_status is UNDEFINED)");
        TimeCreated = map["time_created"].AsNullableDateTime();
        TimeCompleted = map["time_completed"].AsNullableDateTime();
        if (map.ContainsKey("sender_batch_header"))
          SenderBatchHeader = new payoutSenderBatchHeader(map["sender_batch_header"] as JSONDataMap);
        else throw new PaymentException(GetType().Name + ".ctor(sender_batch_header is null)");
        var errors = (map["errors"] as JSONDataArray) ?? Enumerable.Empty<object>();
        Errors = new List<error>(errors.Select(error => new error(error as JSONDataMap)));
      }

      public string PayoutBatchID { get; set; }
      public payoutBatchStatus BatchStatus { get; set; }
      public DateTime? TimeCreated { get; set; }
      public DateTime? TimeCompleted { get; set; }
      public payoutSenderBatchHeader SenderBatchHeader { get; set; }
      public List<error> Errors { get; set; }
    }

    private class payoutBatch
    {
      public payoutBatch(JSONDataMap map)
      {
        if (map.ContainsKey("batch_header"))
          BatchHeader = new payoutBatchHeader(map["batch_header"] as JSONDataMap);
        else throw new PaymentException(GetType().Name + ".ctor(batch_header is null)");
        var items = (map["items"] as JSONDataArray) ?? Enumerable.Empty<object>();
        Items = new List<payoutItemDetails>(items.Select(item => new payoutItemDetails(item as JSONDataMap)));
      }

      public payoutBatchHeader BatchHeader { get; set; }
      public List<payoutItemDetails> Items { get; set; }

      public string Token { get { return BatchHeader.PayoutBatchID; } }
    }

    private class payoutSenderBatchHeader
    {
      public payoutSenderBatchHeader() { }
      public payoutSenderBatchHeader(JSONDataMap map)
      {
        SenderBatchID = map["sender_batch_id"].AsString();
        RecipientType = map["recipient_type"].AsEnum(payoutRecipientType.UNDEFINED);
        EmailSubject = map["email_subject"].AsString();
      }

      public string SenderBatchID { get; set; }
      public payoutRecipientType RecipientType { get; set; }
      public string EmailSubject { get; set; }

      public JSONDataMap ToJSONDataMap()
      {
        if (SenderBatchID.IsNullOrWhiteSpace()) throw new PaymentException(GetType().Name + ".ToJSONDataMap(SenderBatchID is null)");
        if (RecipientType == payoutRecipientType.UNDEFINED) throw new PaymentException(GetType().Name + ".ToJSONDataMap(RecipientType is UNDEFINED)");
        if (EmailSubject.IsNullOrWhiteSpace()) throw new PaymentException(GetType().Name + ".ToJSONDataMap(EmailSubject is null)");
        return new JSONDataMap { { "sender_batch_id", SenderBatchID }, { "recipient_type", RecipientType.ToString() }, { "email_subject", EmailSubject } };
      }
    }

    private class payoutItem
    {
      public payoutItem() { }
      public payoutItem(JSONDataMap map)
      {
        RecipientType = map["recipient_type"].AsEnum(payoutRecipientType.UNDEFINED);
        if (RecipientType == payoutRecipientType.UNDEFINED) throw new PaymentException(GetType().Name + ".ctor(recipient_type is null)");
        var amount = parseAmount(map["amount"] as JSONDataMap);
        if (!amount.HasValue) throw new PaymentException(GetType().Name + ".ctor(amount is null)");
        Amount = amount.Value;
        Note = map["note"].AsString();
        Receiver = map["receiver"].AsString();
        SenderItemId = map["sender_item_id"].AsString();
      }

      public payoutRecipientType RecipientType { get; set; }
      public Amount Amount { get; set; }
      public string Note { get; set; }
      public string Receiver { get; set; }
      public string SenderItemId { get; set; }

      public JSONDataMap ToJSONDataMap()
      {
        if (SenderItemId.IsNullOrWhiteSpace()) throw new PaymentException(GetType().Name + ".ToJSONDataMap(SenderItemId is null)");
        if (RecipientType == payoutRecipientType.UNDEFINED) throw new PaymentException(GetType().Name + ".ToJSONDataMap(RecipientType is UNDEFINED)");
        if (Receiver.IsNullOrWhiteSpace()) throw new PaymentException(GetType().Name + ".ToJSONDataMap(Receiver is null)");
        var map = new JSONDataMap
        {
          { "recipient_type", RecipientType },
          { "amount", toJSONDataMap(Amount) },
          { "receiver", Receiver },
          { "sender_item_id", SenderItemId }
        };
        if (Note.IsNotNullOrWhiteSpace()) map["note"] = Note;
        return map;
      }
    }

    private class payout
    {
      public payoutSenderBatchHeader SenderBatchHeader { get; set; }
      public List<payoutItem> Items { get; set; }

      public JSONDataMap ToJSONDataMap()
      {
        if (!Items.Any()) throw new PaymentException(GetType().Name + ".ToJSONDataMap(Items is empty)");
        return new JSONDataMap { { "sender_batch_header", SenderBatchHeader.ToJSONDataMap() }, { "items", new JSONDataArray(Items.Select(item => item.ToJSONDataMap())) } };
      }
    }
    #endregion

    #region Private
    private Uri URI_GenerateOAuthToken() { return new Uri(m_ApiUri, URI_OAUTH_TOKEN_GENERATE); }
    private Uri URI_CreatePayouts() { return new Uri(m_ApiUri, URI_PAYOUTS_CREATE); }
    private Uri URI_GetPayouts(string batchID) { return new Uri(m_ApiUri, URI_PAYOUTS_GET.Args(batchID)); }
    private Uri URI_GetPayoutItem(string payoutItemID) { return new Uri(m_ApiUri, URI_PAYOUT_ITEM_GET.Args(payoutItemID)); }
    private Uri URI_CancelPayoutItem(string payoutItemID) { return new Uri(m_ApiUri, URI_PAYOUT_ITEM_CANCEL.Args(payoutItemID)); }

    internal PayPalOAuthToken generateOAuthToken(PayPalCredentials credentials)
    {
      var logID = Log(MessageType.Info, "generateOAuthToken()", "Generate OAuth token");

      try
      {
        var request = new WebClient.RequestParams(this)
        {
          Method = HTTPRequestMethod.POST,
          ContentType = ContentType.FORM_URL_ENCODED,
          Headers = new Dictionary<string, string>
          { { HDR_AUTHORIZATION, credentials.AuthorizationHeader } },
          BodyParameters = new Dictionary<string, string>
          { { PRM_GRANT_TYPE, PRM_CLIENT_CREDENTIALS } }
        };

        var response = WebClient.GetJson(URI_GenerateOAuthToken(), request);
        Log(MessageType.Trace, "generateOAuthToken()", response.ToJSON(), relatedMessageID: logID);
        Log(MessageType.Info, "generateOAuthToken()", "OAuth token generated", relatedMessageID: logID);
        return new PayPalOAuthToken(
          response["app_id"].AsString(),
          response["expires_in"].AsInt(),
          response["token_type"].AsString(),
          response["access_token"].AsString(),
          response["scope"].AsString(),
          response["nonce"].AsString(),
          m_OAuthExpirationMargin);
      }
      catch (Exception ex)
      {
        var error = composeError(StringConsts.PAYPAL_GENERATE_OAUTH_ERROR_MESSAGE.Args(ex.ToMessageWithType()), ex);
        Log(MessageType.Error, "generateOAuthToken()", error.Message, ex, relatedMessageID: logID);

        throw error;
      }
    }

    private string getBatchID(Transaction transaction)
    {
      var token = transaction.Token.AsString();
      var ic = token.IndexOf(':');
      if (ic < 0) return token;
      return token.Substring(0, ic);
    }

    private string getPayoutItemID(Transaction transaction)
    {
      var token = transaction.Token.AsString();
      var ic = token.IndexOf(':');
      if (ic < 0) return string.Empty;
      return token.Substring(ic + 1, token.IndexOf(':', ic + 1) - ic - 1);
    }

    private payoutBatch createPayouts(payout payout, PayPalOAuthToken authToken, Guid logID)
    {
      Log(MessageType.Info, "createPayouts()", "Create payouts", relatedMessageID: logID);

      try
      {
        var request = new WebClient.RequestParams(this)
        {
          Method = HTTPRequestMethod.POST,
          ContentType = ContentType.JSON,
          Headers = new Dictionary<string, string>
          { { HDR_AUTHORIZATION, authToken.AuthorizationHeader } },
          QueryParameters = new Dictionary<string, string>
          { { PRM_SYNC_MODE, m_SyncMode.AsString() } },
          Body = payout.ToJSONDataMap().ToJSON(JSONWritingOptions.Compact)
        };

        var response = WebClient.GetJson(URI_CreatePayouts(), request);
        Log(MessageType.Trace, "createPayouts()", "Payouts", relatedMessageID: logID, parameters: response.ToJSON());
        Log(MessageType.Info, "createPayouts()", "Payouts created", relatedMessageID: logID);
        return new payoutBatch(response);
      }
      catch (Exception ex)
      {
        var error = composeError(StringConsts.PAYPAL_PAYOUTS_CREATE_ERROR_MESSAGE.Args(ex.ToMessageWithType()), ex);
        Log(MessageType.Error, "createPayouts()", error.Message, relatedMessageID: logID);
        throw error;
      }
    }

    private payoutBatch getPayouts(string batchID, PayPalOAuthToken authToken, Guid logID)
    {
      Log(MessageType.Info, "getPayouts()", "Fetch payouts", relatedMessageID: logID);

      try
      {
        var request = new WebClient.RequestParams(this)
        {
          Method = HTTPRequestMethod.GET,
          ContentType = ContentType.JSON,
          Headers = new Dictionary<string, string>
          { { HDR_AUTHORIZATION, authToken.AuthorizationHeader } }
        };

        var response = WebClient.GetJson(URI_GetPayouts(batchID), request);
        Log(MessageType.Trace, "getPayouts()", "Payouts", relatedMessageID: logID, parameters: response.ToJSON());
        Log(MessageType.Info, "getPayouts()", "Payotus fetched", relatedMessageID: logID);

        return new payoutBatch(response);
      }
      catch (Exception ex)
      {
        var error = composeError(StringConsts.PAYPAL_PAYOUTS_FETCH_ERROR_MESSAGE.Args(ex.ToMessageWithType()), ex);
        Log(MessageType.Error, "getPayouts()", error.Message, relatedMessageID: logID);
        throw error;
      }
    }

    private payoutItemDetails getPayoutItem(string payoutItemID, PayPalOAuthToken authToken, Guid logID)
    {
      Log(MessageType.Info, "getPayouts()", "Fetch payouts", relatedMessageID: logID);

      try
      {
        var request = new WebClient.RequestParams(this)
        {
          Method = HTTPRequestMethod.GET,
          ContentType = ContentType.JSON,
          Headers = new Dictionary<string, string>
          { { HDR_AUTHORIZATION, authToken.AuthorizationHeader } }
        };

        var response = WebClient.GetJson(URI_GetPayoutItem(payoutItemID), request);
        Log(MessageType.Trace, "getPayouts()", "Payouts", relatedMessageID: logID, parameters: response.ToJSON());
        Log(MessageType.Info, "getPayouts()", "Payotus fetched", relatedMessageID: logID);

        return new payoutItemDetails(response);
      }
      catch (Exception ex)
      {
        var error = composeError(StringConsts.PAYPAL_PAYOUT_ITEM_FETCH_ERROR_MESSAGE.Args(ex.ToMessageWithType()), ex);
        Log(MessageType.Error, "getPayouts()", error.Message, relatedMessageID: logID);
        throw error;
      }
    }

    private payoutItemDetails cancelPayoutItem(string payoutItemID, PayPalOAuthToken authToken, Guid logID)
    {
      Log(MessageType.Info, "cancelPayoutIyem()", "Cancel payout item", relatedMessageID: logID);

      try
      {
        var request = new WebClient.RequestParams(this)
        {
          Method = HTTPRequestMethod.POST,
          ContentType = ContentType.JSON,
          Headers = new Dictionary<string, string>
          { { HDR_AUTHORIZATION, authToken.AuthorizationHeader } }
        };

        var response = WebClient.GetJson(URI_CancelPayoutItem(payoutItemID), request);
        Log(MessageType.Trace, "cancelPayoutIyem()", "Payout Item", relatedMessageID: logID, parameters: response.ToJSON());
        var payoutItem = new payoutItemDetails(response);
        if (payoutItem.TransactionStatus == payoutTransactionStatus.RETURNED)
          Log(MessageType.Info, "cancelPayoutIyem()", "Payout item canceled", relatedMessageID: logID);
        else
          Log(MessageType.Info, "cancelPayoutIyem()", "Payout item not canceled", relatedMessageID: logID);

        return payoutItem;
      }
      catch (Exception ex)
      {
        var error = composeError(StringConsts.PAYPAL_PAYOUT_ITEM_CANCEL_ERROR_MESSAGE.Args(ex.ToMessageWithType()), ex);
        Log(MessageType.Error, "cancelPayoutIyem()", error.Message, ex, relatedMessageID: logID);
        throw error;
      }
    }

    private static Amount? parseAmount(JSONDataMap map)
    {
      if (map == null) return null;
      var currencyISO = map["currency"].AsString();
      var value = map["value"].AsDecimal();
      return new Amount(currencyISO, value);
    }

    private static JSONDataMap toJSONDataMap(Amount amount)
    {
      return new JSONDataMap { { "currency", amount.CurrencyISO.ToUpperInvariant() }, { "value", amount.Value.ToString("F2") } };
    }

    private TransactionStatus mapTransactionStatus(payoutTransactionStatus status)
    {
      switch (status)
      {
        case payoutTransactionStatus.SUCCESS:
          return TransactionStatus.Success;
        case payoutTransactionStatus.UNCLAIMED:
          return TransactionStatus.Unclaimed;
        case payoutTransactionStatus.RETURNED:
          return TransactionStatus.Refunded;
        case payoutTransactionStatus.ONHOLD:
        case payoutTransactionStatus.PENDING:
        case payoutTransactionStatus.PROCESSING:
          return TransactionStatus.Pending;
        case payoutTransactionStatus.DENIED:
          return TransactionStatus.Denied;
      }
      return TransactionStatus.Failed;
    }

    private PaymentException composeError(string message, Exception inner)
    {
      var webError = inner as System.Net.WebException;
      if (webError == null || webError.Response == null)
        return new PaymentException(message, inner);

      var response = (HttpWebResponse)webError.Response;

      var errorName = string.Empty;
      var errorDescription = string.Empty;
      var errorDetails = string.Empty;

      try
      {
        using (var stream = response.GetResponseStream())
        using (var sr = new StreamReader(stream))
        {
          var resp = sr.ReadToEnd().JSONToDataObject() as JSONDataMap;
          if (resp.ContainsKey("error"))
          {
            errorName = resp["error"].AsString();
            errorDescription = resp["error_description"].AsString();
          }
          else
          {
            var error = new error(resp);
            errorName = error.Name;
            errorDescription = error.Message;
            errorDetails = error.Details.ToJSON(JSONWritingOptions.Compact);
          }
        }
      }
      catch { }

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

      message = "{0}{1}Status: {2} - {3}{4}Error: {5} - {6}{7}"
                      .Args(message, System.Environment.NewLine,
                            statusCode, statusMessage, System.Environment.NewLine,
                            errorName, errorDescription, errorDetails.IsNotNullOrWhiteSpace() ? System.Environment.NewLine + errorDetails : string.Empty);

      return new PaymentException(message, inner);
    }
    #endregion
  }
}
