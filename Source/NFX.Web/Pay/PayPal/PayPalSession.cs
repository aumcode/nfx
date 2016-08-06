using NFX.Security;

namespace NFX.Web.Pay.PayPal
{
    /// <summary>
    /// Represents PayPal pay session
    /// </summary>
    public class PayPalSession : PaySession
    {
        public PayPalSession(PayPalSystem paySystem, PayPalConnectionParameters cParams)
            : base(paySystem, cParams)
        {
            m_ConnectionParameters = cParams;
        }

        private readonly PayPalConnectionParameters m_ConnectionParameters;
        public PayPalConnectionParameters ConnectionParameters
        {
            get { return m_ConnectionParameters; }
        }

        public PayPalOAuthToken AuthorizationToken
        {
            get
            {
                if (m_User == null || m_User == User.Fake) return null;
                return m_User.AuthToken.Data as PayPalOAuthToken;
            }
        }
    }
}
