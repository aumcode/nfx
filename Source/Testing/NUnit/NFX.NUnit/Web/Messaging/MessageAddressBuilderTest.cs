using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using NFX.Web.Messaging;

namespace NFX.NUnit.Web.Messaging
{
  [TestFixture]
  public class MessageAddressBuilderTest
  {
    [Test]
    public void BuildMessageAddress()
    {
      var config =
      @"nfx
        {
          a
          {
            name=Peter
            channel-name=Twilio
            channel-address='+15005550005'
          }
          a
          {
            name=Nick
            channel-name=Mailgun
            channel-address='nick@example.com'
          }
        }";

      var builder = new MessageAddressBuilder(config);
      var addressees = builder.All.ToArray();

      Aver.AreEqual(addressees.Count(), 2);
      Aver.AreEqual(builder.ToString(),
                    "nfx{a{name=Peter channel-name=Twilio channel-address=+15005550005}a{name=Nick channel-name=Mailgun channel-address=nick@example.com}}");

      Aver.AreEqual(addressees[0].Name, "Peter");
      Aver.AreEqual(addressees[0].ChannelName, "Twilio");
      Aver.AreEqual(addressees[0].ChannelAddress, "+15005550005");

      Aver.AreEqual(addressees[1].Name, "Nick");
      Aver.AreEqual(addressees[1].ChannelName, "Mailgun");
      Aver.AreEqual(addressees[1].ChannelAddress, "nick@example.com");

      var ann = new MessageAddressBuilder.Addressee
      {
        Name = "Ann",
        ChannelName = "SMTP",
        ChannelAddress = "ann@example.com"
      };
      builder.AddAddressee(ann);

      addressees = builder.All.ToArray();
      Aver.AreEqual(addressees.Count(), 3);
      var str = builder.ToString();
      Aver.AreEqual(builder.ToString(),
                    "nfx{a{name=Peter channel-name=Twilio channel-address=+15005550005}a{name=Nick channel-name=Mailgun channel-address=nick@example.com}a{name=Ann channel-name=SMTP channel-address=ann@example.com}}");

      Aver.AreEqual(addressees[2].Name, "Ann");
      Aver.AreEqual(addressees[2].ChannelName, "SMTP");
      Aver.AreEqual(addressees[2].ChannelAddress, "ann@example.com");

      builder = new MessageAddressBuilder(null);
      builder.AddAddressee(ann);
      Aver.AreEqual(builder.ToString(), "nfx{a{name=Ann channel-name=SMTP channel-address=ann@example.com}}");
      Aver.AreEqual(builder.All.Count(), 1);

      builder = new MessageAddressBuilder("nfx{}");
      Aver.AreEqual(builder.All.Count(), 0);
      Aver.AreEqual(builder.ToString(), "nfx{}");
    }

    [Test]
    public void MatchNames()
    {
      var config =
      @"nfx
        {
          a
          {
            name=Peter
            channel-name=Twilio
            channel-address=+15005550005
          }
          a
          {
            name=Nick
            channel-name=Mailgun
            channel-address=nick@example.com
          }
          a
          {
            name=Ann
            channel-name=SMTP
            channel-address=ann@example.com
          }
        }";
      var builder = new MessageAddressBuilder(config);

      var names = new string[] {"smtp"};
      Aver.IsTrue(builder.MatchNamedChannel(names));

      names = new string[] {"Twilio", "MailGun"};
      Aver.IsTrue(builder.MatchNamedChannel(names));

      names = new string[] {"Skype"};
      Aver.IsFalse(builder.MatchNamedChannel(names));
    }
  }
}
