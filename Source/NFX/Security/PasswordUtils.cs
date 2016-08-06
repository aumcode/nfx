/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2014 IT Adapter Inc / 2015 Aum Code LLC
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFX.Security
{
  /// <summary>
  /// Facilitates working with password strings
  /// </summary>
  public static class PasswordUtils
  {


     private const int CREDIT_CHAR_PRESENT = 10;

     private const int CREDIT_CASE_MIX = 30;
     private const int CREDIT_DIGIT_MIX = 35;
     private const int CREDIT_SYMBOL_MIX = 50;

     private const int CREDIT_TYPE_TRANSITION = 12;

     private const int DEBIT_CHAR_REPEAT = 9;
     private const int DEBIT_ADJACENT_CHAR = 7;

     private const int DEBIT_COMMON_WORD = 30;



     public static readonly string[] COMMON_WORDS =
     {
       "noah","jesus","liam","jacob","mason","william","ethan","michael","alex","john","jack","jayden","daniel","bill","rick","frank","fred","mike","mark","jason","jeff","eugene",
       "dave","david","robert","roger","jerry","justin","elvis","adam","abraham","george","winston","jordan","peter","paul","joseph","jacob","nick","bob","rich","chris","greg",
       "tim", "charlie", "thomas", "sam", "pat", "drew", "don", "phil",

       "sophia", "emma","olivia","ava", "mia", "zoe", "lisa", "emily","abigail","madison","elizabeth","tanya","tonya","suzi","anna","sarah","maggie","helena","marilyn","mary","cheryl","sheryl",
       "jen", "lily", "ella", "aria", "chloe", "kay", "lee", "madelyn", "julia", "jasmine",

       "god", "lord", "budda","buddha", "muhammad", "evil", "hell", //CHRISt , dEVIL

       "sinatra","monroe","lennon","bach","mozart","chopin","beethoven","beatles",
       "music", "game", "drum", "piano", "bass", "guitar", "violin", "trump", "ace", "diamond", "spade", "card", "play", "chess", "compute",

       "kennedy","fuck","suck","dick","cunt","sex","pussy","monkey","master", "anus", "asshole", "bitch","demon","daemon","angel","link","work","love","connect","dragon","soccer",
       "kill","pepper","princess", "mother", "father", "brother", "sister", "cousin", "uncle", "good", "bad", "ugly", "mustang", "tango", "ball", "shadow", "test", "access",

       "qwerty", "asdf", "zxcv", "letme", "secret",

       "silver", "old",

       "boy", "girl", "man", "lady",

       "summer", "spring", "winter", "fall", "autumn",

       "sun","moon","mercury","jupiter","mars","saturn","venus","earth",

       "cool", "dude", "trust", "time", "pass", "word", "port", "friend", "tigger", "dog", "cat", "fish", "bird", "super",

       "york","washington","cleveland","chicago","boston","tampa","dallas","angeles","phoenix","seattle","odessa","moscow","london","paris","milan","berlin","dresden",
       "rome","madrid","cairo","dehli","india","chin","german","russ","beijing","france","fren", "spain","italy","england","usa","amer","mexi",

       //Common CARS
       "honda","toyota","nissan","suzuki","mazda","mitsubishi","buick","cadillac","ford","pontiac","jeep","lexus","acura","infinit","chrysler",
       "audi","porsche","opel","subaru","volkswagen","mercedes","citroen","renault","peugeot",

       "apple","droid","sony","sharp","ibm","phone","tab",

       "4you", "4me", "4them", "4him", "4her", "4it", "4us", "2you", "2me", "2them", "2him", "2her", "2it", "2us"
     };

     public const int TOP_SCORE_EASY = 180;
     public const int TOP_SCORE_NORMAL = 237;
     public const int TOP_SCORE_HARD = 350;



     /// <summary>
     /// Calculates password strength as int score which gets computed per set of rules
     /// </summary>
     public static int PasswordStrengthScore(this string pwd)
     {
        if (pwd.IsNullOrWhiteSpace()) return 0;
        pwd = pwd.Trim();

        var len = pwd.Length;
        var score = len * CREDIT_CHAR_PRESENT;
        if (score==0) return 0;

        var wasUpper = false;
        var wasLower = false;
        var wasDigit = false;
        var wasSymbol = false;

        char pc = (char)0;
        for(var i=0; i<len; i++)
        {
          var c = pwd[i];

          if (Char.IsUpper(c)) wasUpper = true;
          if (Char.IsLower(c)) wasLower = true;
          if (Char.IsDigit(c)) wasDigit = true;
          if (isSymbol(c)) wasSymbol = true;

          if (i>0 &&
              (Char.IsUpper(c) != Char.IsUpper(pc) ||
               Char.IsDigit(c) != Char.IsDigit(pc) ||
               isSymbol(c) != isSymbol(pc))
             ) score += CREDIT_TYPE_TRANSITION;

          if (c==pc) score -= DEBIT_CHAR_REPEAT;

          if (Math.Abs(c-pc)==1) score -= DEBIT_ADJACENT_CHAR;
          pc = c;
        }

        if (wasUpper && wasLower) score += CREDIT_CASE_MIX;
        if (wasDigit && (wasUpper || wasLower || wasSymbol)) score += CREDIT_DIGIT_MIX;
        if (wasSymbol) score += CREDIT_SYMBOL_MIX;

        for(var i=0; i<COMMON_WORDS.Length; i++)
         if (pwd.IndexOf(COMMON_WORDS[i], StringComparison.InvariantCultureIgnoreCase)>=0) score -= DEBIT_COMMON_WORD;


        return score<0 ? 0 : score;
     }

     private static bool isSymbol(char c)
     {
        return Char.IsSymbol(c) || Char.IsPunctuation(c);
     }

     /// <summary>
     /// Calculates password strength as int percentage 0..100%
     /// </summary>
     public static int PasswordStrengthPercent(this string pwd, int maxScore = 0)
     {
        if (maxScore<=0) maxScore = TOP_SCORE_NORMAL;

        double max = maxScore;
        var score = pwd.PasswordStrengthScore();

        var result = (int)( 100d * (score / max) );
        return result>100 ? 100 : result;
     }


  }
}
