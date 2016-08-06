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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NFX.CodeAnalysis
{
    /// <summary>
    /// Describes transition instructions for Finate State Machine (FSM)
    /// </summary>
    public enum FSMI { Loop, Abort, Advance, AdvanceOnSameToken, Take, TakeAndComplete, Complete };



    /// <summary>
    /// Lazy Finate State Machine (FSM) predicate, tries to match condition and returns appropriate next action
    /// </summary>
    public delegate FSMI LazyFSMPredicate<TToken>(LazyFSMState<TToken> state, TToken token) where TToken : Token;



    /// <summary>
    /// Provides Token-pattern matching utilities
    /// </summary>
    public static class PatternSearch
    {

       /// <summary>
       /// Makes finite state machine fed from IEnumerable(Token) only considering primary language tokens
       /// </summary>
       /// <typeparam name="TToken">Concrete language-typed token</typeparam>
       /// <param name="tokens">Token enumerable</param>
       /// <param name="predicates">Predicates that supply machine state transition instructions</param>
       /// <returns>Resulting token fetched by FSMI.Take instruction or null</returns>
       public static TToken LazyFSM<TToken>(this IEnumerable<TToken> tokens,
                                            params LazyFSMPredicate<TToken>[] predicates) where TToken : Token
       {
          return tokens.LazyFSM(true, predicates);
       }




       /// <summary>
       /// Makes finite state machine fed from IEnumerable(Token)
       /// </summary>
       /// <typeparam name="TToken">Concrete language-typed token</typeparam>
       /// <param name="tokens">Token enumerable</param>
       /// <param name="onlyPrimary">Sets filter to consider only primary language tokens (skip comments, directives, etc.)</param>
       /// <param name="predicates">Predicates that supply machine state transition instructions</param>
       /// <returns>Resulting token fetched by FSMI.Take instruction or null</returns>
       public static TToken LazyFSM<TToken>(this IEnumerable<TToken> tokens,
                                               bool onlyPrimary,
                                               params LazyFSMPredicate<TToken>[] predicates) where TToken : Token
       {
          LazyFSMState<TToken> state = null;
          return tokens.LazyFSM<TToken>(onlyPrimary, ref state, predicates);
       }


       /// <summary>
       /// Makes finite state machine fed from IEnumerable(Token)
       /// </summary>
       /// <typeparam name="TToken">Concrete language-typed token</typeparam>
       /// <param name="tokens">Token enumerable</param>
       /// <param name="onlyPrimary">Sets filter to consider only primary language tokens (skip comments, directives, etc.)</param>
       /// <param name="state">Machine's state which will be allocated if null passed</param>
       /// <param name="predicates">Predicates that supply machine state transition instructions</param>
       /// <returns>Resulting token fetched by FSMI.Take instruction or null</returns>
       public static TToken LazyFSM<TToken>(this IEnumerable<TToken> tokens,
                                        bool onlyPrimary,
                                        ref LazyFSMState<TToken> state,
                                        params LazyFSMPredicate<TToken>[] predicates) where TToken : Token
       {
          var first = true;
          var pidx = 0;
          TToken result = null;

          var en = tokens.GetEnumerator();

          if (state==null)
            state = new LazyFSMState<TToken>();

          state.m_Tokens = tokens;
          state.m_CurrentTokenIndex = -1;
          state.m_PatternTokenLength = -1;
          state.m_OnlyPrimary = onlyPrimary;

          var dontMove = false;

          while(pidx<predicates.Length)
          {
            if (dontMove)
                dontMove = false;
            else
            {
                if (!en.MoveNext()) break;
                state.m_CurrentTokenIndex++;
                state.m_PatternTokenLength++;
            }

            TToken token = en.Current;
            if (token.IsEOF) break;

            if  (!onlyPrimary || token.IsPrimary || first)
            {
               first = false;

               state.m_CurrentToken = token;

               var action = predicates[pidx](state, token);

               switch(action)
               {
                 case FSMI.Loop: break;

                 case FSMI.Abort: return null;

                 case FSMI.Advance:  {
                                        pidx++;
                                        break;
                                     }

                 case FSMI.AdvanceOnSameToken:  {
                                        pidx++;
                                        dontMove = true;
                                        break;
                                     }

                 case FSMI.Take:
                                    {
                                    result = token;
                                    pidx++;
                                    break;
                                    }

                 case FSMI.TakeAndComplete:  return token;

                 case FSMI.Complete: return result;

                 default: return null;
               }
            }


          }//while

          return result;
       }



       /// <summary>
       /// Skips specified number of tokens by returning FSMI.Loop count times
       /// </summary>
       public static FSMI Skip<TToken>(this LazyFSMState<TToken> state,
                                        int count) where TToken : Token
       {
          if (count==0) return FSMI.Advance;
          if (count<0) throw new CodeAnalysisException("FSMI Skip(count) must be > 0");
          if (state.m_SkipCount>0)
          {
             state.m_SkipCount--;
             if (state.m_SkipCount==0) return FSMI.Advance;
             return FSMI.Loop;
          }
          state.m_SkipCount = count;
          return FSMI.Loop;
       }


       /// <summary>
       /// Returns true when token's type is any of the specified
       /// </summary>
       public static bool TypeIsAnyOf<TToken>(this TToken token,
                                              params int[] types) where TToken : Token
       {
         foreach(var t in types)
          if (token.OrdinalType == t) return true;

         return false;
       }

       /// <summary>
       /// Returns true when token's type is any of the specified
       /// </summary>
       public static bool TypeIsAnyOf<TToken, TTokenType>(this TToken token,
                                                          params TTokenType[] types) where TToken : Token
                                                                                     where TTokenType : struct, IConvertible
       {
         foreach(var t in types)
          if (token.OrdinalType == Convert.ToInt32(t)) return true;

         return false;
       }

       /// <summary>
       /// Returns true when token's text is any of specified
       /// </summary>
       public static bool TextIsAnyOf<TToken>(this TToken token,
                                              params string[] texts) where TToken : Token
       {
         foreach(var t in texts)
          if (token.Text == t) return true;

         return false;
       }

       /// <summary>
       /// Returns true when token's text is any of specified
       /// </summary>
       public static bool TextIsAnyOf<TToken>(this TToken token,
                                              StringComparison comparison,
                                              params string[] texts) where TToken : Token
       {
         foreach(var t in texts)
          if (string.Equals(token.Text, t, comparison)) return true;

         return false;
       }


       /// <summary>
       /// Advances FSM when token's type is any of specified or aborts FSM
       /// </summary>
       public static FSMI IsAnyOrAbort<TToken>(this TToken token,
                                               params int[] types) where TToken : Token
       {
         return token.TypeIsAnyOf(types) ? FSMI.Advance : FSMI.Abort;
       }

       /// <summary>
       /// Advances FSM when token's type is any of specified or aborts FSM
       /// </summary>
       public static FSMI IsAnyOrAbort<TToken, TTokenType>(this TToken token,
                                                           params TTokenType[] types) where TToken : Token
                                                                                      where TTokenType : struct, IConvertible
       {
         return token.TypeIsAnyOf(types) ? FSMI.Advance : FSMI.Abort;
       }

       /// <summary>
       /// Advances FSM when token's text is any of specified or aborts FSM
       /// </summary>
       public static FSMI IsAnyOrAbort<TToken>(this TToken token,
                                            params string[] texts) where TToken : Token
       {
         return token.TextIsAnyOf(texts) ? FSMI.Advance : FSMI.Abort;
       }

       /// <summary>
       /// Advances FSM when token's text is any of specified or aborts FSM
       /// </summary>
       public static FSMI IsAnyOrAbort<TToken>(this TToken token,
                                               StringComparison comparison,
                                               params string[] texts) where TToken : Token
       {
         return token.TextIsAnyOf(comparison, texts) ? FSMI.Advance : FSMI.Abort;
       }


       /// <summary>
       /// Advances FSM when token's type is any of specified or loops FSM
       /// </summary>
       public static FSMI LoopUntilAny<TToken>(this TToken token,
                                               params int[] types) where TToken : Token
       {
         return token.TypeIsAnyOf(types) ? FSMI.Advance : FSMI.Loop;
       }

       /// <summary>
       /// Advances FSM when token's type is any of specified or loops FSM
       /// </summary>
       public static FSMI LoopUntilAny<TToken, TTokenType>(this TToken token,
                                                           params TTokenType[] types) where TToken : Token
                                                                                      where TTokenType : struct, IConvertible
       {
         return token.TypeIsAnyOf(types) ? FSMI.Advance : FSMI.Loop;
       }


       /// <summary>
       /// Advances FSM when token's text is any of specified or loops FSM
       /// </summary>
       public static FSMI LoopUntilAny<TToken>(this TToken token,
                                            params string[] texts) where TToken : Token
       {
         return token.TextIsAnyOf(texts) ? FSMI.Advance : FSMI.Loop;
       }

       /// <summary>
       /// Advances FSM when token's text is any of specified or loops FSM
       /// </summary>
       public static FSMI LoopUntilAny<TToken>(this TToken token,
                                               StringComparison comparison,
                                               params string[] texts) where TToken : Token
       {
         return token.TextIsAnyOf(comparison, texts) ? FSMI.Advance : FSMI.Loop;
       }

       /// <summary>
       /// Performs FSM instruction unless token's type is any of the specified, then advances FSM
       /// </summary>
       public static FSMI DoUntilAny<TToken>(this TToken token,
                                            FSMI instruction,
                                            params int[] types) where TToken : Token
       {
         return token.TypeIsAnyOf(types) ? FSMI.Advance : instruction;
       }

       /// <summary>
       /// Performs FSM instruction unless token's type is any of the specified, then advances FSM
       /// </summary>
       public static FSMI DoUntilAny<TToken, TTokenType>(this TToken token,
                                            FSMI instruction,
                                            params TTokenType[] types) where TToken : Token
                                                                       where TTokenType : struct, IConvertible
       {
         return token.TypeIsAnyOf(types) ? FSMI.Advance : instruction;
       }

       /// <summary>
       /// Performs FSM instruction unless token's text is any of the specified, then advances FSM
       /// </summary>
       public static FSMI DoUntilAny<TToken>(this TToken token,
                                            FSMI instruction,
                                            params string[] texts) where TToken : Token
       {
         return token.TextIsAnyOf(texts) ? FSMI.Advance : instruction;
       }


       /// <summary>
       /// Performs FSM instruction unless token's text is any of the specified, then advances FSM
       /// </summary>
       public static FSMI DoUntilAny<TToken>(this TToken token,
                                            FSMI instruction,
                                            StringComparison comparison,
                                            params string[] texts) where TToken : Token
       {
         return token.TextIsAnyOf(comparison, texts) ? FSMI.Advance : instruction;
       }

       /// <summary>
       /// Loops FSM while token's type is any of the specified or aborts
       /// </summary>
       public static FSMI LoopWhileAnyOrAbort<TToken>(this TToken token,
                                            params int[] types) where TToken : Token
       {
         return token.TypeIsAnyOf(types) ? FSMI.Loop : FSMI.Abort;
       }


       /// <summary>
       /// Loops FSM while token's type is any of the specified or aborts
       /// </summary>
       public static FSMI LoopWhileAnyOrAbort<TToken, TTokenType>(this TToken token,
                                                                  params TTokenType[] types) where TToken : Token
                                                                                             where TTokenType : struct, IConvertible
       {
         return token.TypeIsAnyOf(types) ? FSMI.Loop : FSMI.Abort;
       }

       /// <summary>
       /// Loops FSM while token's text is any of the specified or aborts
       /// </summary>
       public static FSMI LoopWhileAnyOrAbort<TToken>(this TToken token,
                                            params string[] texts) where TToken : Token
       {
         return token.TextIsAnyOf(texts) ? FSMI.Loop : FSMI.Abort;
       }


       /// <summary>
       /// Loops FSM while token's text is any of the specified or aborts
       /// </summary>
       public static FSMI LoopWhileAnyOrAbort<TToken>(this TToken token,
                                                      StringComparison comparison,
                                                      params string[] texts) where TToken : Token
       {
         return token.TextIsAnyOf(comparison, texts) ? FSMI.Loop : FSMI.Abort;
       }


       /// <summary>
       /// Loops until token pattern match succeeds, considering only primary language tokens.
       /// Keeps state machine if match was found on the first matching token
       /// </summary>
       public static FSMI LoopUntilMatch<TToken>(this LazyFSMState<TToken> state,
                                        params LazyFSMPredicate<TToken>[] predicates
                                       ) where TToken : Token
       {
         return state.LoopUntilMatch(true, predicates);
       }

       /// <summary>
       /// Loops until token pattern match succeeds, conditionaly considering only primary language tokens.
       /// Keeps state machine if match was found on the first matching token
       /// </summary>
       public static FSMI LoopUntilMatch<TToken>(this LazyFSMState<TToken> state,
                                        bool onlyPrimary,
                                        params LazyFSMPredicate<TToken>[] predicates
                                       ) where TToken : Token
       {
         var stream = state.Tokens;

         if (state.m_CurrentTokenIndex > 0)
          stream = stream.Skip(state.m_CurrentTokenIndex);

         LazyFSMState<TToken> subState = new LazyFSMState<TToken>();

         return stream.LazyFSM(onlyPrimary, ref subState, predicates) != null ? FSMI.AdvanceOnSameToken : FSMI.Loop;
       }

       /// <summary>
       /// Loops until token pattern match succeeds.
       /// This method matches using the same primary token filter as the parent match.
       /// Keeps state machine if match was found on the first token that follows the match
       /// </summary>
       public static FSMI LoopUntilAfterMatch<TToken>(this LazyFSMState<TToken> state,
                                        params LazyFSMPredicate<TToken>[] predicates
                                       ) where TToken : Token
       {
         if (state.m_SkipCount>0)
         {
             state.m_SkipCount--;
             if (state.m_SkipCount==0) return FSMI.Advance;
             return FSMI.Loop;
         }

         var stream = state.Tokens;

         if (state.m_CurrentTokenIndex > 0)
          stream = stream.Skip(state.m_CurrentTokenIndex);

         LazyFSMState<TToken> subState = new LazyFSMState<TToken>();

         if (stream.LazyFSM(state.m_OnlyPrimary, ref subState, predicates) != null)
           return state.Skip(subState.m_PatternTokenLength-1);

         return FSMI.Loop;
       }

    }



    /// <summary>
    /// Represents a state object for Lazy Finate State Machine that enumerates tokes from IEnumerable(Token)
    ///  and does not support a notion of index addressing
    /// </summary>
    public class LazyFSMState<TToken> : Hashtable where TToken : Token
    {

      internal IEnumerable<TToken> m_Tokens;
      internal Token m_CurrentToken;
      internal int m_CurrentTokenIndex;

      internal bool m_OnlyPrimary;

      internal int m_SkipCount;
      internal int m_PatternTokenLength;


      /// <summary>
      /// Returns token that machine is at now
      /// </summary>
      public Token CurrentToken
      {
         get { return m_CurrentToken; }
      }


      /// <summary>
      /// Returns true when state machine only considers language-primary tokens
      /// </summary>
      public bool OnlyPrimary
      {
         get { return m_OnlyPrimary;}
      }

      /// <summary>
      /// Returns an index for token that machine is on now
      /// </summary>
      public int CurrentTokenIndex
      {
         get { return m_CurrentTokenIndex; }
      }

      /// <summary>
      /// Returns how many tokens have been covered by the current match pattern, i.e. if we loop until first int literal,
      ///  this property will count how many tokens have been looped + int literal itself
      /// </summary>
      public int PatternTokenLength
      {
         get{ return m_PatternTokenLength; }
      }


      /// <summary>
      /// Returns token enumerable that analysis is performed on
      /// </summary>
      public IEnumerable<TToken> Tokens
      {
         get { return m_Tokens; }
      }

      /// <summary>
      /// Gets typecasted value for a key
      /// </summary>
      public TCast Get<TCast>(object key)
      {
        return (TCast)this[key];
      }
    }

}
