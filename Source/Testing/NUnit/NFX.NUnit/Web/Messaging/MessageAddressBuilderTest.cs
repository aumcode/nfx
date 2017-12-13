using System;
using System.Linq;

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
      @"as
        {
          a
          {
            nm=Peter
            cn=Twilio
            ca='+15005550005'
          }
          a
          {
            nm=Nick
            cn=Mailgun
            ca='nick@example.com'
          }
        }";

      var builder = new MessageAddressBuilder(config);
      var addressees = builder.All.ToArray();

      Aver.AreEqual(addressees.Count(), 2);
      Aver.AreEqual(builder.ToString(),
                    "as{a{nm=Peter cn=Twilio ca=+15005550005}a{nm=Nick cn=Mailgun ca=nick@example.com}}");

      Aver.AreEqual(addressees[0].Name, "Peter");
      Aver.AreEqual(addressees[0].ChannelName, "Twilio");
      Aver.AreEqual(addressees[0].ChannelAddress, "+15005550005");

      Aver.AreEqual(addressees[1].Name, "Nick");
      Aver.AreEqual(addressees[1].ChannelName, "Mailgun");
      Aver.AreEqual(addressees[1].ChannelAddress, "nick@example.com");

      var ann = new MessageAddressBuilder.Addressee
      (
        "Ann",
        "SMTP",
        "ann@example.com"
      );
      builder.AddAddressee(ann);

      addressees = builder.All.ToArray();
      Aver.AreEqual(addressees.Count(), 3);
      var str = builder.ToString();
      Aver.AreEqual(builder.ToString(),
                    "as{a{nm=Peter cn=Twilio ca=+15005550005}a{nm=Nick cn=Mailgun ca=nick@example.com}a{nm=Ann cn=SMTP ca=ann@example.com}}");

      Aver.AreEqual(addressees[2].Name, "Ann");
      Aver.AreEqual(addressees[2].ChannelName, "SMTP");
      Aver.AreEqual(addressees[2].ChannelAddress, "ann@example.com");

      builder = new MessageAddressBuilder(null);
      builder.AddAddressee(ann);
      Aver.AreEqual(builder.ToString(), "as{a{nm=Ann cn=SMTP ca=ann@example.com}}");
      Aver.AreEqual(builder.All.Count(), 1);

      builder = new MessageAddressBuilder("as{}");
      Aver.AreEqual(builder.All.Count(), 0);
      Aver.AreEqual(builder.ToString(), "as{}");
    }

    [Test]
    public void MatchNames()
    {
      var config =
      @"as
        {
          a
          {
            nm=Peter
            cn=Twilio
            ca=+15005550005
          }
          a
          {
            nm=Nick
            cn=Mailgun
            ca=nick@example.com
          }
          a
          {
            nm=Ann
            cn=SMTP
            ca=ann@example.com
          }
        }";
      var builder = new MessageAddressBuilder(config);

      var names = new string[] {"smtp"};
      Aver.IsTrue(builder.MatchNamedChannel(names));
      var matches = builder.GetMatchesForChannels(names).ToArray();
      Aver.AreEqual(matches.Length, 1);
      Aver.AreEqual(matches[0].Name, "Ann");

      names = new string[] {"Twilio", "MailGun"};
      Aver.IsTrue(builder.MatchNamedChannel(names));

      matches = builder.GetMatchesForChannels(names).ToArray();
      Aver.AreEqual(matches.Length, 2);
      Aver.AreEqual(matches[0].Name, "Peter");
      Aver.AreEqual(matches[1].Name, "Nick");
      var first = builder.GetFirstOrDefaultMatchForChannels(names);
      Aver.IsNotNull(first);
      Aver.IsTrue(first.Assigned);
      Aver.AreEqual(first.Name, "Peter");

      names = new string[] {"Skype"};
      Aver.IsFalse(builder.MatchNamedChannel(names));
      matches = builder.GetMatchesForChannels(names).ToArray();
      Aver.AreEqual(matches.Length, 0);
      first = builder.GetFirstOrDefaultMatchForChannels(names);
      Aver.IsNotNull(first);
      Aver.IsFalse(first.Assigned);
    }
  }
}
