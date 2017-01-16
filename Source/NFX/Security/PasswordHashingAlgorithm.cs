using System;

using NFX.ApplicationModel;
using NFX.Environment;
using NFX.ServiceModel;

namespace NFX.Security
{
  public interface IPasswordHashingOptions
  {
  }

  /// <summary>
  /// Represents an abstraction of password algorithm that performs hashing and verification of passwords supplied as SecureBuffer
  /// </summary>
  public abstract class PasswordHashingAlgorithm : ServiceWithInstrumentationBase<IPasswordManagerImplementation>, INamed
  {
    #region .ctor
      protected PasswordHashingAlgorithm(IPasswordManagerImplementation director, string name) : base(director)
      {
        this.Name = name;
        m_StrengthLevel = PasswordStrengthLevel.Normal;
      }
    #endregion

    #region Fields

      [Config("$default|$is-default")]
      private bool m_IsDefault;

      [Config(Default = PasswordStrengthLevel.Normal)]
      private PasswordStrengthLevel m_StrengthLevel;

      private bool m_InstrumentationEnabled;

    #endregion

    #region Properties

      [Config(Default = false)]
      [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION, CoreConsts.EXT_PARAM_GROUP_PAY)]
      public override bool InstrumentationEnabled
      {
        get { return m_InstrumentationEnabled; }
        set { m_InstrumentationEnabled = value; }
      }

      public bool IsDefault { get { return m_IsDefault; } }

      public PasswordStrengthLevel StrengthLevel { get { return m_StrengthLevel; } }

    #endregion

    #region Public

      public virtual bool Match(PasswordFamily family) { return true; }

      public HashedPassword ComputeHash(PasswordFamily family, SecureBuffer password)
      {
        if (password == null)
          throw new SecurityException(StringConsts.ARGUMENT_ERROR + "PasswordManager.ComputeHash(password==null)");
        if (!password.IsSealed)
          throw new SecurityException(StringConsts.ARGUMENT_ERROR + "PasswordManager.ComputeHash(!password.IsSealed)");

        CheckServiceActive();

        return DoComputeHash(family, password);
      }

      public bool Verify(SecureBuffer password, HashedPassword hash, out bool needRehash)
      {
        if (password == null || hash == null)
          throw new SecurityException(StringConsts.ARGUMENT_ERROR + "PasswordManager.Verify((password|hash)==null)");
        if (!password.IsSealed)
          throw new SecurityException(StringConsts.ARGUMENT_ERROR + "PasswordManager.Verify(!password.IsSealed)");

        needRehash = false;
        if (!Running)
          return false;

        return DoVerify(password, hash, out needRehash);
      }

      public bool AreEquivalent(HashedPassword hash, HashedPassword rehash)
      {
        if (hash == null || rehash == null) return false;
        if (!hash.AlgoName.EqualsOrdIgnoreCase(rehash.AlgoName)) return false;
        return DoAreEquivalent(hash, rehash);
      }

    #endregion

    #region Protected

      protected abstract HashedPassword DoComputeHash(PasswordFamily family, SecureBuffer password);
      protected abstract bool DoVerify(SecureBuffer password, HashedPassword hash, out bool needRehash);
      protected abstract bool DoAreEquivalent(HashedPassword hash, HashedPassword rehash);

    #endregion
  }

  public abstract class PasswordHashingAlgorithm<TOptions> : PasswordHashingAlgorithm
    where TOptions : IPasswordHashingOptions
  {
    public PasswordHashingAlgorithm(IPasswordManagerImplementation director, string name) : base(director, name)
    {}

    public HashedPassword ComputeHash(PasswordFamily family, SecureBuffer password, TOptions options)
    {
      if (password == null)
        throw new SecurityException(StringConsts.ARGUMENT_ERROR + "PasswordManager.ComputeHash(password==null)");
      if (!password.IsSealed)
        throw new SecurityException(StringConsts.ARGUMENT_ERROR + "PasswordManager.ComputeHash(!password.IsSealed)");

      CheckServiceActive();

      return DoComputeHash(family, password, options);
    }

    public TOptions ExtractPasswordHashingOptions(HashedPassword hash, out bool needRehash)
    {
      if (hash == null || hash["salt"] == null)
        throw new SecurityException(StringConsts.ARGUMENT_ERROR + "MD5PasswordHashingAlgorithm.ExtractPasswordHashingOptions((hash|hash[salt])==null)");
      if (hash.AlgoName != Name)
        throw new SecurityException(StringConsts.ARGUMENT_ERROR + "MD5PasswordHashingAlgorithm.ExtractPasswordHashingOptions(hash[algo] invalid)");
      return DoExtractPasswordHashingOptions(hash, out needRehash);
    }

    protected override HashedPassword DoComputeHash(PasswordFamily family, SecureBuffer password)
    { return DoComputeHash(family, password, DefaultPasswordHashingOptions); }

    protected override bool DoVerify(SecureBuffer password, HashedPassword hash, out bool needRehash)
    {
      var options = ExtractPasswordHashingOptions(hash, out needRehash);
      var rehash = ComputeHash(hash.Family, password, options);
      return AreEquivalent(hash, rehash);
    }

    protected abstract HashedPassword DoComputeHash(PasswordFamily family, SecureBuffer password, TOptions options);
    protected abstract TOptions DefaultPasswordHashingOptions { get; }
    protected abstract TOptions DoExtractPasswordHashingOptions(HashedPassword hash, out bool needRehash);
  }
}
