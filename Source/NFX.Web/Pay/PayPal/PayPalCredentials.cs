using NFX.Security;

namespace NFX.Web.Pay.PayPal
{
    /// <summary>
    /// Represents basic PayPal credentials for registered application
    /// which include Business account email, client ID and client secret.
    /// </summary>
    public class PayPalCredentials : Credentials
    {
        public PayPalCredentials(string accountEmail, string clientID, string clientSecret)
        {
            m_AccountEmail = accountEmail;
            m_ClientID = clientID;
            m_ClientSecret = clientSecret;
        }

        private readonly string m_AccountEmail;
        private readonly string m_ClientID;
        private readonly string m_ClientSecret;

        public string AccountEmail { get { return m_AccountEmail; } }
        public string ClientID { get { return m_ClientID; } }
        public string ClientSecret { get { return m_ClientSecret; } }

        public override string ToString()
        {
            return m_AccountEmail;
        }
    }
}
