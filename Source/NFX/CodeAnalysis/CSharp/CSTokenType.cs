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

namespace NFX.CodeAnalysis.CSharp
{
    /// <summary>
    /// Denotes CSharp token types
    /// </summary>
    public enum CSTokenType
    {
      tUnknown = 0,

         NONLANG_START,
                tBOF,
                tEOF,
                tDirective,
         NONLANG_END,

                tComment,
                tIdentifier,

         SYMBOLS_START,
                    tTerminator,
                    tComma,
                    tColon,
                    tBraceOpen,
                    tBraceClose,

                    tBracketOpen,
                    tBracketClose,
                    tSqBracketOpen,
                    tSqBracketClose,

         SYMBOLS_END,


         OPERATORS_START,

                    tPlus,
                    tMinus,
                    tMul,
                    tDiv,
                    tMod,

                    tAnd,
                    tAndShort,
                    tOr,
                    tOrShort,
                    tXor,
                    tNot,
                    tBitNot,

                    tInc,
                    tDec,

                    tShl,
                    tShr,

                    tE,
                    tNE,
                    tL,
                    tG,
                    tLE,
                    tGE,

                    tAssign,
                    tPlusAssign,
                    tMinusAssign,
                    tMulAssign,
                    tDivAssign,
                    tModAssign,
                    tAndAssign,
                    tOrAssign,
                    tXorAssign,
                    tShlAssign,
                    tShrAssign,

                    tDot,

                    tAs,
                    tIs,
                    tNew,
                    tSizeOf,
                    tTypeOf,

                    tDeref, //->

                    tTernaryIf, // ?
                    tNullCoalesce,// ??
                    tLambda, // =>

         OPERATORS_END,

         KEYWORDS_START,
                    tAbstract,
                    tBase,
                    tBreak,
                    tCase,
                    tCatch,
                    tChecked,
                    tClass,
                    tConst,
                    tContinue,
                    tDefault,
                    tDelegate,
                    tDo,
                    tElse,
                    tEnum,
                    tEvent,
                    tExplicit,
                    tExtern,
                    tFinally,
                    tFixed,
                    tFor,
                    tForeach,
                    tGoto,
                    tIf,
                    tImplicit,
                    tIn,
                    tInterface,
                    tLock,
                    tNamespace,
                    tOperator,
                    tOut,
                    tOverride,
                    tParams,
                    tPrivate,
                    tProtected,
                    tPublic,
                    tReadonly,
                    tRef,
                    tReturn,
                    tSealed,
                    tStackAlloc,
                    tStatic,
                    tStruct,
                    tSwitch,
                    tThis,
                    tThrow,
                    tTry,
                    tUnchecked,
                    tUnsafe,
                    tUsing,
                    tVar,
                    tVirtual,
                    tVolatile,
                    tVoid,
                    tWhile,
         KEYWORDS_END,

         TYPES_START,
                    tBool,
                    tByte,
                    tChar,
                    tDecimal,
                    tDouble,
                    tFloat,
                    tLong,
                    tObject,
                    tSByte,
                    tShort,
                    tString,
                    tInt,
                    tUInt,
                    tULong,
                    tUShort,
         TYPES_END,

         LITERALS_START,
                NUMLITERALS_START,
                    tIntLiteral,
                    tLongIntLiteral,

                    tUIntLiteral,
                    tULongIntLiteral,

                    tFloatLiteral,
                    tDoubleLiteral,

                    tDecimalLiteral,
                NUMLITERALS_END,

                    tNull,
                    tStringLiteral,

                    tTrue,
                    tFalse,

         LITERALS_END
     }
}
