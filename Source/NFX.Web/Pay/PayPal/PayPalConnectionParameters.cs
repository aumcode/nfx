using NFX.Environment;
using NFX.Security;

namespace NFX.Web.Pay.PayPal
{
    public class PayPalConnectionParameters : PayConnectionParameters
    {
        public const string CFG_EMAIL = "email";
        public const string CFG_CLIENT_ID = "client-id";
        public const string CFG_CLIENT_SECRET = "client-secret";

        #region .ctor

        public PayPalConnectionParameters() : base()
        {
        }

        public PayPalConnectionParameters(IConfigSectionNode node) : base(node)
        {
        }

        public PayPalConnectionParameters(string connectionString, string format = Configuration.CONFIG_LACONIC_FORMAT)
            : base(connectionString, format)
        {
        }

        #endregion

        public override void Configure(IConfigSectionNode node)
        {
            base.Configure(node);

            var email = node.AttrByName(CFG_EMAIL).Value;
            var clientID = node.AttrByName(CFG_CLIENT_ID).Value;
            var clientSecret = node.AttrByName(CFG_CLIENT_SECRET).Value;

            var credentials = new PayPalCredentials(email, clientID, clientSecret);
            var token = new AuthenticationToken(PayPalSystem.PAYPAL_REALM, null); // OAuth token is empty at start
            User = new User(credentials, token, email, Rights.None);
        }
    }
}
