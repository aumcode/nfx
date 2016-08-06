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


/* NFX by ITAdapter
 * Originated: 2006.01
 * Revision: NFX 1.0  2011.02.03
 */
using System;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Runtime.Serialization;

namespace NFX.Parsing
{
  /// <summary>
  /// Evaluator class performs math and logical expression parsing and evaluation without allocating .NET compilers
  /// </summary>
  public delegate string IdentifierLookup(string ident);

  public sealed class Evaluator
  {
    #region Evaluator Constants
                //String
                private const char tkESC = '\\';
                private const char tkSTRING = '\'';
                //Arithmetic
                private const string tkPLUS = "+";
                private const string tkMINUS = "-";//unary
                private const string tkMUL = "*";
                private const string tkDIV = "/";
                private const string tkMOD = "%";
                //Logical test
                private const string tkEQ = "==";
                private const string tkLE = "<=";
                private const string tkGE = ">=";
                private const string tkL = "<";
                private const string tkG = ">";
                private const string tkNE = "!=";
                //Logical
                private const string tkAND = "&&";
                private const string tkOR = "||";
                private const string tkXOR = "^^";
                private const string tkNOT = "!!";//unary

                //Ternary
                private const char tkIF = '?';
                private const char tkIFSEP = ';';

                //Precedence
                private const char tkOPEN = '(';
                private const char tkCLOSE = ')';

                private const char _SOPEN = (char)0xffff;
                private const char _SCLOSE = (char)0xff00;

                //warning! list order is important, Ternary operator SHOULD not be included here
                private static string[] OPERATORS = {tkAND, tkOR, tkXOR, tkNOT,
	                                                    tkEQ, tkLE, tkGE, tkL, tkG, tkNE,
	                                                    tkPLUS, tkMINUS,
	                                                    tkMUL, tkDIV, tkMOD };
    #endregion

    #region .ctor

        public Evaluator(string expression)
        {
          m_Expression = ((expression == null) ? string.Empty : expression);
          Parse();
        }//Evaluator .ctor

    #endregion


    #region Evaluator Private members
        private string m_Expression;

        [NonSerialized]
        private string m_NoStringsExpression;

        [NonSerialized]
        private Node m_Root;

        [NonSerialized]
        private Hashtable m_StringTable = new Hashtable();
    #endregion

    #region Evaluator Properties
    /// <summary>
    /// Expression beeing evaluated
    /// </summary>
    public string Expression
    {
      get
      {
        return m_Expression;
      }
    }

    /// <summary>
    /// Expression with all strings replaced with tokens
    /// </summary>
    public string NoStringsExpression
    {
      get
      {
        return m_NoStringsExpression;
      }
    }

    /// <summary>
    /// Root node of expression evaluation tree
    /// </summary>
    public Node Root
    {
      get
      {
        return m_Root;
      }
    }

    /// <summary>
    /// Event which is fired upon leaf token resolution during evaluation, used to resolve host variables
    /// </summary>
    public event IdentifierLookup OnIdentifierLookup;
    #endregion





    /// <summary>
    /// Evaluate expression and return result - either string or numerical value
    /// </summary>
    public string Evaluate(IdentifierLookup lookup = null)
    {
      return doEvaluate(m_Root, lookup);
    }//Evaluator.evaluate

    /// <summary>
    /// Node class represents evaluation b-tree element
    /// </summary>
    public sealed class Node
    {
      #region Node Private members
      private string m_Expression;
      private Node m_Condition;
      private Node m_Left;
      private Node m_Right;
      internal string m_Operator = "";
      private int m_Level = 0;
      #endregion

      #region Node Properties
      /// <summary>
      /// This node sub-expression
      /// </summary>
      public string Expression
      {
        get
        {
          return m_Expression;
        }
      }

      /// <summary>
      /// Boolean condition expression (subnode) used for conditional (ternary) operator
      /// </summary>
      public Node Condition
      {
        get
        {
          return m_Condition;
        }
      }

      /// <summary>
      /// Left b-tree branch, may be blank (in case of unary operators)
      /// </summary>
      public Node Left
      {
        get
        {
          return m_Left;
        }
      }

      /// <summary>
      /// Right b-tree branch, used for unary operators
      /// </summary>
      public Node Right
      {
        get
        {
          return m_Right;
        }
      }

      /// <summary>
      /// Head operator performed on left and right b-tree branches
      /// </summary>
      public string Operator
      {
        get
        {
          return m_Operator;
        }
      }

      /// <summary>
      /// Node depth level within expression b-tree
      /// </summary>
      public int Level
      {
        get
        {
          return m_Level;
        }
      }
      #endregion

      public Node(string expression, int level)
      {
        m_Expression = (expression == null) ? string.Empty : expression;
        m_Level = level;
        Parse();
      }//Node .ctor 1


      //=======================================================================
      private string findSideOperator(string expr, bool left)
      {
        int j;
        for (int i = 0; i < OPERATORS.Length; i++)
        {
          j = left ? expr.IndexOf(OPERATORS[i]) : expr.LastIndexOf(OPERATORS[i]);
          if ((left) && (j == 0))
            return OPERATORS[i];
          if ((!left) && (j != -1) && ((j + OPERATORS[i].Length) == expr.Length))
            return OPERATORS[i];
        }

        return "";
      }//findSideOperator

      //Expression gets here without strings - guaranteed
      private void Parse()
      {
        string expr = m_Expression.Trim();


        int iop = -1;
        int icl = -1;



        while (true)
        {
          //check for ()
          iop = expr.IndexOf(tkOPEN);
          icl = -1;

          if (iop != -1)//( found
          {
            int cnt = 0;
            for (int i = iop; i < expr.Length; i++)//find corresponding )
            {
              if (expr[i] == tkOPEN)
                cnt++;
              else
                if (expr[i] == tkCLOSE)
                  cnt--;

              if (cnt == 0)
              {
                icl = i;
                break;
              }//if
            }//for

            if (icl == -1)
              throw new ArgumentException("Precedence mismatch or operator missing in: " + expr);


            if ((iop == 0) && (icl == expr.Length - 1))
            {
              expr = expr.Substring(1, expr.Length - 2);
              //check for if
              if (expr.Length > 3) //at least ?;;
                if (expr[0] == tkIF)
                {
                  //split by tkIFSEP

                  ArrayList ifst = new ArrayList();
                  StringBuilder buf = new StringBuilder();
                  int pc = 0;
                  for (int i = 0; i < expr.Length; i++)
                  {
                    if (expr[i] == tkOPEN)
                      pc++;
                    if (expr[i] == tkCLOSE)
                      pc--;
                    if ((expr[i] == tkIFSEP) && (pc == 0))
                    {
                      ifst.Add(buf.ToString());
                      buf.Length = 0;
                      continue;
                    }
                    buf.Append(expr[i]);
                  }//for
                  ifst.Add(buf.ToString());

                  if (ifst.Count < 3)
                    throw new ArgumentException("Missing required clause in conditional statement: " + expr);

                  m_Operator = tkIF.ToString();
                  m_Condition = new Node(((string)ifst[0]).Substring(1).Trim(), m_Level + 1);
                  m_Left = new Node(((string)ifst[1]).Trim(), m_Level + 1);
                  m_Right = new Node(((string)ifst[2]).Trim(), m_Level + 1);
                  return;
                }//if found
            }//outer ()
            else
              break;

          }
          else
            break;

        }//while

        if (iop != -1)//found()
        {

          if (iop == icl - 1)
            throw new ArgumentException("Missing expression");



          bool rbranch = false;
          int ind;
          for (int i = 0; i < OPERATORS.Length; i++)
          {
            ind = expr.IndexOf(OPERATORS[i]);
            if ((ind != -1) && (ind < iop))
            {
              rbranch = true;
              break;
            }
          }//for

          int olen;

          if (rbranch)//right
          {
            m_Right = new Node(expr.Substring(iop), m_Level + 1);
            expr = expr.Substring(0, iop).Trim();
            //like:  25*2- , 7+

            m_Operator = findSideOperator(expr, false);
            olen = m_Operator.Length;
            if (olen == 0)
              throw new ArgumentException("Missing operator in: " + expr);

            int l = expr.Length - olen;
            if (l > 0)
              m_Left = new Node(expr.Substring(0, l), m_Level + 1);

          }
          else//left
          {
            m_Left = new Node(expr.Substring(0, icl - iop + 1), m_Level + 1);
            expr = expr.Substring(icl + 1).Trim();
            //like:  -25*2 , +7

            m_Operator = findSideOperator(expr, true);
            olen = m_Operator.Length;
            if (olen == 0)
              throw new ArgumentException("Missing operator in: " + expr);
            m_Right = new Node(expr.Substring(olen), m_Level + 1);
          }
        }//if ()
        else//no () found
        {
          int opi = 0;

          m_Operator = "";

          for (int i = 0; i < OPERATORS.Length; i++)
          {
            opi = expr.IndexOf(OPERATORS[i]);
            if (opi != -1)
            {
              m_Operator = OPERATORS[i];
              break;
            }//if
          }//for

          if (m_Operator.Length == 0)
          {//leaf node
            m_Operator = expr;
            return;
          }//if

          string lstr = expr.Substring(0, opi).Trim();
          string rstr = expr.Substring(opi + m_Operator.Length).Trim();

          m_Left = (lstr != "") ? new Node(lstr, m_Level + 1) : null;
          m_Right = (rstr != "") ? new Node(rstr, m_Level + 1) : null;


        }//else - no ()

      }//Node.Parse
      //======================================================================

      public string PrintTree()
      {
        string pad = "";
        for (int i = 0; i <= m_Level; i++)
          pad += "    ";
        string tree = "\n" + pad + "Operator: " + m_Operator;
        tree += "\n" + pad + ". Left: " + ((m_Left == null) ? " " : m_Left.PrintTree());
        tree += "\n" + pad + ". Right: " + ((m_Right == null) ? " " : m_Right.PrintTree());
        return tree;
      }//Node.PrintTree


    }//class Node



    private void Parse()
    {
      //build string table
      string expr = " " + m_Expression + " ";
      StringBuilder buf = new StringBuilder();

      int cnt = 0;
      string key;
      int i = 1;
      bool str = false;
      string old;

      while (i < expr.Length - 1)
      {
        if ((!str) && (expr[i] == tkSTRING) && (expr[i - 1] != tkESC))
        {
          str = true;
          i++;
          continue;
        }//if

        if ((str) && (expr[i] == tkSTRING) && (expr[i - 1] != tkESC))
        {
          str = false;

          key = _SOPEN + cnt.ToString() + _SCLOSE;

          m_StringTable.Add(key, buf.ToString());
          cnt++;
          old = tkSTRING + buf.ToString() + tkSTRING;
          expr = expr.Replace(old, key);
          i -= (old.Length - 1);
          buf.Length = 0;
          continue;
        }//if

        if (str)
          buf.Append(expr[i]);
        i++;
      }//while

      m_NoStringsExpression = expr;
      m_Root = new Node(expr, 0);


      //put strings back
      returnStrings(m_Root);

    }//Evaluator.Parse


    /// <summary>
    /// Returns b-tree traversal in a string form
    /// </summary>
    public string PrintTree()
    {
      return (m_Root == null) ? "" : m_Root.PrintTree();
    }//Evaluator Print Tree

    private void returnStrings(Node node)
    {
      if (node == null)
        return;
      //check this operator
      int iop = node.Operator.IndexOf(_SOPEN);
      if (iop != -1)
      {
        int icl = node.Operator.IndexOf(_SCLOSE);
        if (icl != -1)
        {
          string tok = node.Operator.Substring(iop, (icl - iop) + 1);
          node.m_Operator = node.Operator.Replace(tok, m_StringTable[tok] as string);
        }
      }

      returnStrings(node.Left);
      returnStrings(node.Right);
    }//Evaluator returnStrings


    //===========================================================================================
    private double getVal(string val)
    {
      string v = val.Trim();
      if (v == "True")
        return 1;
      if (v == "False")
        return 0;
      if (v == "Pi")
        return Math.PI;
      if (v == "E")
        return Math.E;
      return Convert.ToDouble(val);
    }//getVal

    private string doEvaluate(Node node, IdentifierLookup lookup)
    {

      if ((node.Operator == tkIF.ToString()) && (node.Condition != null))
      {

        var cond = doEvaluate(node.Condition, lookup);
        if (string.Equals(cond,"1") ||
            string.Equals(cond, "true", StringComparison.InvariantCultureIgnoreCase) ||
            string.Equals(cond, "yes", StringComparison.InvariantCultureIgnoreCase))
          return (node.Left == null) ? "" : doEvaluate(node.Left, lookup);
        else
          return (node.Right == null) ? "" : doEvaluate(node.Right, lookup);
      }//if  tkIF


      string lnd = (node.Left == null) ? "" : doEvaluate(node.Left, lookup);
      string rnd = (node.Right == null) ? "" : doEvaluate(node.Right, lookup);

      //resolve identifier
      if ((lnd == "") && (rnd == ""))
      {
        string rslt = node.Operator;

        if (lookup!=null) rslt = lookup(rslt);
        else
         if (OnIdentifierLookup != null) rslt = OnIdentifierLookup(rslt);

        return rslt;
      }//if


      double left;
      double right;


      try
      {
        left = getVal(lnd);
        right = getVal(rnd);
      }
      catch
      {
        if ((node.Operator == tkMINUS) && (lnd == ""))//test for unary -
        {
          try
          {
            right = getVal(rnd);//may fail again
            return (-right).ToString();
          }
          catch
          {
          }
        }//if

        if ((node.Operator == tkPLUS) && (lnd != null) && (rnd != null))
          return (((string)lnd) + ((string)rnd));
        else
          throw;
      }



      switch (node.Operator)
      {
        case tkPLUS:
          {
            return (left + right).ToString();
          }
        case tkMINUS:
          {
            return (left - right).ToString();
          }
        case tkMUL:
          {
            return (left * right).ToString();
          }
        case tkDIV:
          {
            return (left / right).ToString();
          }
        case tkMOD:
          {
            return (left % right).ToString();
          }

        case tkEQ:
          {
            return ((left == right) ? "1" : "0");
          }
        case tkLE:
          {
            return ((left <= right) ? "1" : "0");
          }
        case tkGE:
          {
            return ((left >= right) ? "1" : "0");
          }
        case tkL:
          {
            return ((left < right) ? "1" : "0");
          }
        case tkG:
          {
            return ((left > right) ? "1" : "0");
          }
        case tkNE:
          {
            return ((left != right) ? "1" : "0");
          }

        case tkAND:
          {
            return (((left != 0) && (right != 0)) ? "1" : "0");
          }
        case tkOR:
          {
            return (((left != 0) || (right != 0)) ? "1" : "0");
          }
        case tkXOR:
          {
            return (((left != 0) ^ (right != 0)) ? "1" : "0");
          }
        case tkNOT:
          {
            return ((!(right != 0)) ? "1" : "0");
          }

        default:
          {
            /* call delegate here*/
            break;
          }//default
      }//switch

      return null;
    }//Evaluator doEvaluate
    //===========================================================================================




  }//class Evaluator
}
