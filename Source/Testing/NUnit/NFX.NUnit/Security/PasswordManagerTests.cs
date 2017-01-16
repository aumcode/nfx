using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using NFX.Security;
using NFX.Serialization.JSON;

namespace NFX.NUnit.Security
{
  [TestFixture]
  public class PasswordManagerTests
  {
    private IPasswordManagerImplementation m_Manager;

    public IPasswordManagerImplementation Manager {get {return m_Manager;} }

    [TestFixtureSetUp]
    public void Setup()
    {
      m_Manager = new DefaultPasswordManager();
      m_Manager.Start();
    }

    [TestFixtureTearDown]
    public void Tear()
    {
      m_Manager.SignalStop();
    }

    [Test]
    public void CalcStrenghtScore()
    {
      var buf = IDPasswordCredentials.PlainPasswordToSecureBuffer("qwerty");
      var score = Manager.CalculateStrenghtScore(PasswordFamily.Text, buf);
      Assert.AreEqual(30, score);

      buf = IDPasswordCredentials.PlainPasswordToSecureBuffer("qwerty123");
      score = Manager.CalculateStrenghtScore(PasswordFamily.Text, buf);
      Assert.AreEqual(93, score);

      buf = IDPasswordCredentials.PlainPasswordToSecureBuffer("aaaaaaaaaaaaaaaaaaaaaaa");
      score = Manager.CalculateStrenghtScore(PasswordFamily.Text, buf);
      Assert.AreEqual(32, score);

      buf = IDPasswordCredentials.PlainPasswordToSecureBuffer("@blue+sky=");
      score = Manager.CalculateStrenghtScore(PasswordFamily.Text, buf);
      Assert.AreEqual(198, score);

      buf = IDPasswordCredentials.PlainPasswordToSecureBuffer("@8luE+5ky=");
      score = Manager.CalculateStrenghtScore(PasswordFamily.Text, buf);
      Assert.AreEqual(299, score);

      buf = IDPasswordCredentials.PlainPasswordToSecureBuffer(null);
      score = Manager.CalculateStrenghtScore(PasswordFamily.Text, buf);
      Assert.AreEqual(0, score);

      buf = IDPasswordCredentials.PlainPasswordToSecureBuffer(string.Empty);
      score = Manager.CalculateStrenghtScore(PasswordFamily.Text, buf);
      Assert.AreEqual(0, score);

      buf = IDPasswordCredentials.PlainPasswordToSecureBuffer("   ");
      score = Manager.CalculateStrenghtScore(PasswordFamily.Text, buf);
      Assert.AreEqual(0, score);
    }

    [Test]
    public void CalcStrenghtPercent()
    {
      var buf = IDPasswordCredentials.PlainPasswordToSecureBuffer("qwerty");
      var pcnt = Manager.CalculateStrenghtPercent(PasswordFamily.Text, buf);
      Assert.AreEqual(12, pcnt);

      buf = IDPasswordCredentials.PlainPasswordToSecureBuffer("@8luE+5ky=");
      pcnt = Manager.CalculateStrenghtPercent(PasswordFamily.Text, buf);
      Assert.AreEqual(100, pcnt);

      buf = IDPasswordCredentials.PlainPasswordToSecureBuffer("@8luE+5ky=");
      pcnt = Manager.CalculateStrenghtPercent(PasswordFamily.Text, buf, DefaultPasswordManager.TOP_SCORE_MAXIMUM);
      Assert.AreEqual(85, pcnt);
    }

    [Test]
    public void AreEquivalent()
    {
      var pm = new DefaultPasswordManager();
      pm.Start();

      var buf = IDPasswordCredentials.PlainPasswordToSecureBuffer("@8luE+5ky=");
      var hash1 = Manager.ComputeHash(PasswordFamily.Text, buf);
      var hash2 = HashedPassword.FromString(hash1.ToString());

      try
      {
        Assert.IsTrue(pm.AreEquivalent(hash1, hash2));
        Assert.Fail("no exception");
      }
      catch (NFXException e)
      {
        Assert.AreEqual(e.Message, StringConsts.SERVICE_INVALID_STATE +
                                   typeof(DefaultPasswordManager).Name);
      }

      pm.SignalStop();
      pm.WaitForCompleteStop();

      Assert.IsTrue(pm.AreEquivalent(hash1, hash2));

      Assert.IsFalse(pm.AreEquivalent(null, null));

      var hash3 = new HashedPassword("OTH", hash2.Family);
      hash3["hash"] = hash2["hash"];
      hash3["salt"] = hash2["salt"];
      Assert.IsFalse(pm.AreEquivalent(hash1, hash3));

      hash2 = Manager.ComputeHash(PasswordFamily.Text, buf);
      Assert.IsFalse(pm.AreEquivalent(hash1, hash2));

      buf = IDPasswordCredentials.PlainPasswordToSecureBuffer("qwerty");
      hash2 = Manager.ComputeHash(PasswordFamily.Text, buf);
      Assert.IsFalse(pm.AreEquivalent(hash1, hash2));
    }

    [Test]
    public void Compute_Verify_Pass()
    {
      var buf = IDPasswordCredentials.PlainPasswordToSecureBuffer("qwerty");
      var hash = Manager.ComputeHash(PasswordFamily.Text, buf);
      bool rehash, check;

      check = Manager.Verify(buf, hash, out rehash);
      Assert.IsTrue(check);

      buf = IDPasswordCredentials.PlainPasswordToSecureBuffer("@8luE+5ky=");
      hash = Manager.ComputeHash(PasswordFamily.Text, buf);
      check = Manager.Verify(buf, hash, out rehash);
      Assert.IsTrue(check);

      check = Manager.Verify(buf, HashedPassword.FromString(hash.ToJSON()), out rehash);
      Assert.IsTrue(check);
    }

    [Test]
    public void Compute_Verify_Fail()
    {
      var buf = IDPasswordCredentials.PlainPasswordToSecureBuffer("@8luE+5ky=");
      bool rehash, check;

     var hash = Manager.ComputeHash(PasswordFamily.Text, buf);
      buf = IDPasswordCredentials.PlainPasswordToSecureBuffer("qwerty");
      check = Manager.Verify(buf, hash, out rehash);
      Assert.IsFalse(check);
    }

    [Test]
    public void Verify_InvalidHash()
    {
      var buf = IDPasswordCredentials.PlainPasswordToSecureBuffer("@8luE+5ky=");
      var hash = Manager.ComputeHash(PasswordFamily.Text, buf);
      bool rehash, check;

      hash["salt"] = null;
      try
      {
        check = Manager.Verify(buf, hash, out rehash);
        Assert.Fail("no exception");
      }
      catch (NFXException e)
      {
        Assert.IsTrue(e.Message.Contains("ExtractPasswordHashingOptions((hash|hash[salt])==null)"));
      }

      hash = null;
      try
      {
        check = Manager.Verify(buf, hash, out rehash);
        Assert.Fail("no exception");
      }
      catch (NFXException e)
      {
        Assert.IsTrue(e.Message.Contains("Verify((password|hash)==null)"));
      }
    }

    [Test]
    public void CheckServiceActive()
    {
      var pm = new DefaultPasswordManager();
      var buf = IDPasswordCredentials.PlainPasswordToSecureBuffer("@8luE+5ky=");

      try
      {
        var hash = pm.ComputeHash(PasswordFamily.Text, buf);
        Assert.Fail("no exception");
      }
      catch (NFXException e)
      {
        Assert.AreEqual(e.Message, StringConsts.SERVICE_INVALID_STATE +
                                   typeof(DefaultPasswordManager).Name);
      }
    }
  }
}
