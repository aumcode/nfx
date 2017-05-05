/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2017 ITAdapter Corp. Inc.
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
using System.Collections.Generic;

using NFX.ApplicationModel;
using NFX.Environment;
using NFX.Instrumentation;
using NFX.ServiceModel;

namespace NFX.Security
{
  /// <summary>
  /// Defines password stregth levels: Minimum, Normal, Maximum etc.
  /// </summary>
  public enum PasswordStrengthLevel
  {
    Default = 0,
    Minimum,
    BelowNormal,
    Normal,
    AboveNormal,
    Maximum
  }

  /// <summary>
  /// Denoutes kinds of passwords i.e.: text that user types on login, short PIN,
  /// geometrical curve that users need to trace with their finger, select areas of picture
  /// </summary>
  public enum PasswordFamily
  {
    Unspecified,
    Text,
    PIN,
    Geometry,
    Picture,
    Other
  }


  /// <summary>
  /// Denotes an entity that manages passwords such as: computes and verified hash tokens
  /// and provides password strength verification
  /// </summary>
  public interface IPasswordManager : IApplicationComponent
  {
    HashedPassword ComputeHash(PasswordFamily family, SecureBuffer password, PasswordStrengthLevel level = PasswordStrengthLevel.Default);
    bool Verify(SecureBuffer password, HashedPassword hash, out bool needRehash);
    bool AreEquivalent(HashedPassword a, HashedPassword b);

    int CalculateStrenghtScore(PasswordFamily family, SecureBuffer password);
    int CalculateStrenghtPercent(PasswordFamily family, SecureBuffer password, int maxScore = 0);
    IEnumerable<PasswordRepresentation> GeneratePassword(PasswordFamily family, PasswordRepresentationType type, PasswordStrengthLevel level = PasswordStrengthLevel.Default);

    IRegistry<PasswordHashingAlgorithm> Algorithms { get; }
  }

  public interface IPasswordManagerImplementation : IPasswordManager, IConfigurable, IInstrumentable, IService
  {
    bool Register(PasswordHashingAlgorithm algo);
    bool Unregister(PasswordHashingAlgorithm algo);
    bool Unregister(string algoName);
  }
}

