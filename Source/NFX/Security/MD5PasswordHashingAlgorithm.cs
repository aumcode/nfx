/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2018 Agnicore Inc. portions ITAdapter Corp. Inc.
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

using NFX.ApplicationModel;
using NFX.Environment;

namespace NFX.Security
{
  public struct MD5PasswordHashingOptions : IPasswordHashingOptions
  {
    public byte[] Salt { get; set; }
  }

  public class MD5PasswordHashingAlgorithm : PasswordHashingAlgorithm<MD5PasswordHashingOptions>
  {
    public const int DEFAULT_SALT_MAX_LENGTH = 32;

    public MD5PasswordHashingAlgorithm(IPasswordManagerImplementation director, string name) : base(director, name)
    {
      SaltMinLenght = DEFAULT_SALT_MAX_LENGTH / 2;
      SaltMaxLenght = DEFAULT_SALT_MAX_LENGTH;
    }

    #region Properties

      [Config(Default = DEFAULT_SALT_MAX_LENGTH / 2)]
      public int SaltMinLenght { get; set; }

      [Config(Default = DEFAULT_SALT_MAX_LENGTH)]
      public int SaltMaxLenght { get; set; }

    #endregion

    protected override HashedPassword DoComputeHash(PasswordFamily family, SecureBuffer password, MD5PasswordHashingOptions options)
    {
      using (var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider())
      {
        var content = password.Content;
        var contentLength = content.Length;
        var salt = options.Salt;
        var buffer = new byte[contentLength + salt.Length];
        Array.Copy(content, buffer, contentLength);
        Array.Copy(salt, 0, buffer, contentLength, salt.Length);
        var hash = md5.ComputeHash(buffer);
        Array.Clear(buffer, 0, buffer.Length);

        return new HashedPassword(Name, family)
        {
          { "hash", Convert.ToBase64String(hash) },
          { "salt", Convert.ToBase64String(salt) }
        };
      }
    }

    protected override MD5PasswordHashingOptions DefaultPasswordHashingOptions
    {
      get
      {
        return new MD5PasswordHashingOptions
        {
          Salt = ExternalRandomGenerator.Instance.NextRandomBytes(SaltMinLenght, SaltMaxLenght)
        };
      }
    }

    protected override MD5PasswordHashingOptions DoExtractPasswordHashingOptions(HashedPassword hash, out bool needRehash)
    {
      needRehash = false;
      return new MD5PasswordHashingOptions
      {
        Salt = Convert.FromBase64String(hash["salt"].AsString())
      };
    }

    protected override bool DoAreEquivalent(HashedPassword hash, HashedPassword rehash)
    {
      return hash["hash"].AsString().EqualsOrdIgnoreCase(rehash["hash"].AsString());
    }
  }
}
