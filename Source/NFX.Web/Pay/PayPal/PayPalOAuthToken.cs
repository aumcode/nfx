using System;
using NFX.Serialization.JSON;


namespace NFX.Web.Pay.PayPal
{
    /// <summary>
    /// Represents PayPal OAuth token
    /// </summary>
    public class PayPalOAuthToken
    {
        private const string APP_ID = "app_id";
        private const string EXPIRES_IN = "expires_in";
        private const string ACCESS_TOKEN = "access_token";
        private const string SCOPE = "scope";
        private const string NONCE = "nonce";

        public PayPalOAuthToken(JSONDataMap response, int expirationMargin)
        {
            m_ObtainTime = App.TimeSource.Now;
            m_ExpirationMargin = expirationMargin;
            m_ApplicationID = response[APP_ID].AsString();
            m_ExpiresInSeconds = response[EXPIRES_IN].AsInt();
            m_AccessToken = response[ACCESS_TOKEN].AsString();
            m_Scope = response[SCOPE].AsString();
            m_Nonce = response[NONCE].AsString();
        }

        private readonly int m_ExpirationMargin;
        private DateTime m_ObtainTime;
        private string   m_ApplicationID;
        private int      m_ExpiresInSeconds;
        private string   m_AccessToken;
        private string   m_Scope;
        private string   m_Nonce;

        public int      ExpirationMargin { get { return m_ExpirationMargin; } }
        public DateTime ObtainTime { get { return m_ObtainTime; } }
        public string   ApplicationID { get { return m_ApplicationID; } }
        public int      ExpiresInSeconds { get { return m_ExpiresInSeconds; } }
        public string   AccessToken { get { return m_AccessToken; } }
        public string   Scope { get { return m_Scope; } }
        public string   Nonce { get { return m_Nonce; } }

        public bool IsCloseToExpire()
        {
            return (App.TimeSource.Now - m_ObtainTime).TotalSeconds - m_ExpiresInSeconds >= m_ExpirationMargin;
        }

        public override string ToString()
        {
            return m_AccessToken;
        }
    }
}
