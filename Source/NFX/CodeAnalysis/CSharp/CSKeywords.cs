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

namespace NFX.CodeAnalysis.CSharp
{
    /// <summary>
  /// Provides C# keyword resolution services, this class is thread safe
  /// </summary>
  public static class CSKeywords
  {
    private static Dictionary<string, CSTokenType> s_KeywordList = new Dictionary<string, CSTokenType>();

    static CSKeywords()
    {
      s_KeywordList["object"] = CSTokenType.tObject;
      s_KeywordList["string"] = CSTokenType.tString;
      s_KeywordList["bool"] = CSTokenType.tBool;
      s_KeywordList["byte"] = CSTokenType.tByte;
      s_KeywordList["char"] = CSTokenType.tChar;
      s_KeywordList["float"] = CSTokenType.tFloat;
      s_KeywordList["double"] = CSTokenType.tDouble;
      s_KeywordList["decimal"] = CSTokenType.tDecimal;
      s_KeywordList["sbyte"] = CSTokenType.tSByte;
      s_KeywordList["short"] = CSTokenType.tShort;
      s_KeywordList["int"] = CSTokenType.tInt;
      s_KeywordList["long"] = CSTokenType.tLong;
      s_KeywordList["ushort"] = CSTokenType.tUShort;
      s_KeywordList["uint"] = CSTokenType.tUInt;
      s_KeywordList["ulong"] = CSTokenType.tULong;



      s_KeywordList[";"] = CSTokenType.tTerminator;
      s_KeywordList["."] = CSTokenType.tDot;
      s_KeywordList[","] = CSTokenType.tComma;
      s_KeywordList[":"] = CSTokenType.tColon;

      s_KeywordList["{"] = CSTokenType.tBraceOpen;
      s_KeywordList["}"] = CSTokenType.tBraceClose;
      s_KeywordList["("] = CSTokenType.tBracketOpen;
      s_KeywordList[")"] = CSTokenType.tBracketClose;
      s_KeywordList["["] = CSTokenType.tSqBracketOpen;
      s_KeywordList["]"] = CSTokenType.tSqBracketClose;

      s_KeywordList["abstract"] = CSTokenType.tAbstract;
      s_KeywordList["base"] = CSTokenType.tBase;
      s_KeywordList["break"] = CSTokenType.tBreak;
      s_KeywordList["case"] = CSTokenType.tCase;
      s_KeywordList["catch"] = CSTokenType.tCatch;
      s_KeywordList["checked"] = CSTokenType.tChecked;
      s_KeywordList["class"] = CSTokenType.tClass;
      s_KeywordList["const"] = CSTokenType.tConst;
      s_KeywordList["continue"] = CSTokenType.tContinue;
      s_KeywordList["default"] = CSTokenType.tDefault;
      s_KeywordList["delegate"] = CSTokenType.tDelegate;
      s_KeywordList["do"] = CSTokenType.tDo;
      s_KeywordList["else"] = CSTokenType.tElse;
      s_KeywordList["enum"] = CSTokenType.tEnum;
      s_KeywordList["event"] = CSTokenType.tEvent;
      s_KeywordList["explicit"] = CSTokenType.tExplicit;
      s_KeywordList["extern"] = CSTokenType.tExtern;
      s_KeywordList["finally"] = CSTokenType.tFinally;
      s_KeywordList["fixed"] = CSTokenType.tFixed;
      s_KeywordList["for"] = CSTokenType.tFor;
      s_KeywordList["foreach"] = CSTokenType.tForeach;
      s_KeywordList["goto"] = CSTokenType.tGoto;
      s_KeywordList["implicit"] = CSTokenType.tImplicit;
      s_KeywordList["if"] = CSTokenType.tIf;
      s_KeywordList["in"] = CSTokenType.tIn;
      s_KeywordList["interface"] = CSTokenType.tInterface;
      s_KeywordList["lock"] = CSTokenType.tLock;
      s_KeywordList["namespace"] = CSTokenType.tNamespace;
      s_KeywordList["operator"] = CSTokenType.tOperator;
      s_KeywordList["out"] = CSTokenType.tOut;
      s_KeywordList["override"] = CSTokenType.tOverride;
      s_KeywordList["params"] = CSTokenType.tParams;
      s_KeywordList["private"] = CSTokenType.tPrivate;
      s_KeywordList["protected"] = CSTokenType.tProtected;
      s_KeywordList["public"] = CSTokenType.tPublic;
      s_KeywordList["readonly"] = CSTokenType.tReadonly;
      s_KeywordList["ref"] = CSTokenType.tRef;
      s_KeywordList["return"] = CSTokenType.tReturn;
      s_KeywordList["sealed"] = CSTokenType.tSealed;
      s_KeywordList["stackalloc"] = CSTokenType.tStackAlloc;
      s_KeywordList["static"] = CSTokenType.tStatic;
      s_KeywordList["struct"] = CSTokenType.tStruct;
      s_KeywordList["switch"] = CSTokenType.tSwitch;
      s_KeywordList["this"] = CSTokenType.tThis;
      s_KeywordList["throw"] = CSTokenType.tThrow;
      s_KeywordList["try"] = CSTokenType.tTry;
      s_KeywordList["unchecked"] = CSTokenType.tUnchecked;
      s_KeywordList["unsafe"] = CSTokenType.tUnsafe;
      s_KeywordList["using"] = CSTokenType.tUsing;
      s_KeywordList["var"] = CSTokenType.tVar;
      s_KeywordList["virtual"] = CSTokenType.tVirtual;
      s_KeywordList["volatile"] = CSTokenType.tVolatile;
      s_KeywordList["void"] = CSTokenType.tVoid;
      s_KeywordList["while"] = CSTokenType.tWhile;

      s_KeywordList["+"] = CSTokenType.tPlus;
      s_KeywordList["-"] = CSTokenType.tMinus;
      s_KeywordList["*"] = CSTokenType.tMul;
      s_KeywordList["/"] = CSTokenType.tDiv;
      s_KeywordList["%"] = CSTokenType.tMod;

      s_KeywordList["&"] = CSTokenType.tAnd;
      s_KeywordList["&&"] = CSTokenType.tAndShort;
      s_KeywordList["|"] = CSTokenType.tOr;
      s_KeywordList["||"] = CSTokenType.tOrShort;

      s_KeywordList["^"] = CSTokenType.tXor;
      s_KeywordList["!"] = CSTokenType.tNot;
      s_KeywordList["~"] = CSTokenType.tBitNot;

      s_KeywordList["++"] = CSTokenType.tInc;
      s_KeywordList["--"] = CSTokenType.tDec;

      s_KeywordList["<<"] = CSTokenType.tShl;
      s_KeywordList[">>"] = CSTokenType.tShr;

      s_KeywordList["=="] = CSTokenType.tE;
      s_KeywordList["!="] = CSTokenType.tNE;
      s_KeywordList["<"] = CSTokenType.tL;
      s_KeywordList[">"] = CSTokenType.tG;
      s_KeywordList["<="] = CSTokenType.tLE;
      s_KeywordList[">="] = CSTokenType.tGE;

      s_KeywordList["="] = CSTokenType.tAssign;
      s_KeywordList["."] = CSTokenType.tDot;
      s_KeywordList["->"] = CSTokenType.tDeref;

      s_KeywordList["+="] = CSTokenType.tPlusAssign;
      s_KeywordList["-="] = CSTokenType.tMinusAssign;
      s_KeywordList["*="] = CSTokenType.tMulAssign;
      s_KeywordList["/="] = CSTokenType.tDivAssign;
      s_KeywordList["%="] = CSTokenType.tModAssign;
      s_KeywordList["&="] = CSTokenType.tAndAssign;
      s_KeywordList["|="] = CSTokenType.tOrAssign;
      s_KeywordList["^="] = CSTokenType.tXorAssign;
      s_KeywordList["<<="] = CSTokenType.tShlAssign;
      s_KeywordList[">>="] = CSTokenType.tShrAssign;
      s_KeywordList["?"] = CSTokenType.tTernaryIf;
      s_KeywordList["??"] = CSTokenType.tNullCoalesce;
      s_KeywordList["=>"] = CSTokenType.tLambda;

      s_KeywordList["is"] = CSTokenType.tIs;
      s_KeywordList["as"] = CSTokenType.tAs;
      s_KeywordList["new"] = CSTokenType.tNew;
      s_KeywordList["sizeof"] = CSTokenType.tSizeOf;
      s_KeywordList["typeof"] = CSTokenType.tTypeOf;
      s_KeywordList["true"] = CSTokenType.tTrue;
      s_KeywordList["false"] = CSTokenType.tFalse;
    }

    /// <summary>
    /// Resolves a C# keyword - this method IS thread safe
    /// </summary>
    public static CSTokenType Resolve(string str)
    {
      CSTokenType tt;

      s_KeywordList.TryGetValue(str, out tt);

      return (tt != CSTokenType.tUnknown) ? tt : CSTokenType.tIdentifier;
    }

  }
}
