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
 * Originated: 2003.11
 * Revision: NFX 0.1 2007.11.25
 */
using System;
using System.Collections.Generic;
using System.Text;


namespace NFX.Financial
{
    /// <summary>
    /// Implements time value financial functions such as: PV, NPV, IRR
    /// </summary>
    public static class TimeValue
    {


        /// <summary>
        /// Returns present value of amount returned in X number of periods
        /// at a certain interest rate
        /// </summary>
        /// <param name="fv">Future value (returned amount in X periods)</param>
        /// <param name="intRate">Interest rate per period</param>
        /// <param name="periods">Period length</param>
        /// <returns>Present Value</returns>
        /// <example>
        /// Assume that A gives $100 cash to B. B returns the same amount to A in 1 year.
        /// Banks give 5% annual rate, therefore present value of this $100 dollar
        /// returned is going to be $95.24 - A lost $4.76, because B did not pay any interest and
        ///  A could have deposited $100 in regular bank instead of giving it to B.
        /// Had A charged B 5% interest rate , A would have made money.
        /// </example>
        public static double PV(double fv, double intRate, int periods)
        {
            return fv / Math.Pow(1 + intRate, periods);
        }

        /// <summary>
        /// Returns future value of presently-valued amount in X number of periods
        /// at a certain interest rate
        /// </summary>
        /// <param name="pv">Present value</param>
        /// <param name="intRate">Interest rate</param>
        /// <param name="periods">Period length</param>
        /// <returns>Future value</returns>
        /// <example>
        /// $100 loaned for 1 period at 5% interest per period have future value of $105;
        /// same amount loaned for 10 periods at the same rate would have future value of $162.89
        /// </example>
        public static double FV(double pv, double intRate, int periods)
        {
            return pv * Math.Pow(1 + intRate, periods);
        }


        /// <summary>
        /// Returns a net present value calculated over series of cashflows
        /// </summary>
        /// <param name="cashFlows">A series of net cashflows per period(supplied as a summ of inflows and outflows), usually the first cashflow is an outflow of initial project investment</param>
        /// <param name="discountRate">Interest rate used for net cashflow discount</param>
        /// <returns>Net present value</returns>
        public static double NPV(IEnumerable<double> cashFlows, double discountRate)
        {
            double result = 0;
            int periodNumber = 1;
            foreach (double cashFlow in cashFlows)
            {
                double pv = PV(cashFlow, discountRate, periodNumber);

                result = result + pv;

                periodNumber++;
            }

            return result;
        }

        /// <summary>
        /// Calculates the internal rate of return over series of cash flows
        /// </summary>
        public static double IRR(IEnumerable<Double> cashFlows, Double estimatedResult)
        {
            List<double> cashFlowsList = new List<Double>(cashFlows);
            double[] cashFlowsArray = cashFlowsList.ToArray();

            double irr = IRR(cashFlowsArray, estimatedResult);
            if (irr != Double.NaN && irr < 0)
            {
                return .0d;
            }
            else
            {
                return irr;
            }
        }


        /// <summary>
        /// Calculates the internal rate of return over series of cash flows
        /// </summary>
        private static Double IRR(Double[] cashFlows, Double estimatedResult)
        {
            double result = Double.NaN;

            if (cashFlows != null && cashFlows.Length > 0.0)
            {
                  double noOfCashFlows = cashFlows.Length;

                  double sumCashFlows = 0.0d;
                  double absSumCashFlows = 0.0d;

                  int noOfNegativeCashFlows = 0;
                  int noOfPositiveCashFlows = 0;

                  for (int i = 0; i < noOfCashFlows; i++)
                  {
                      sumCashFlows += cashFlows[i];
                      absSumCashFlows += Math.Abs(cashFlows[i]);
                      if (cashFlows[i] > 0)
                      {
                          noOfPositiveCashFlows++;
                      }
                      else if (cashFlows[i] < 0)
                      {
                          noOfNegativeCashFlows++;
                      }
                  }

                  if (noOfNegativeCashFlows > 0 && noOfPositiveCashFlows > 0)
                  {
                      double irrGuess = 0.1;//default 10%

                      if (!Double.IsNaN(estimatedResult))
                      {
                          irrGuess = estimatedResult;
                          if (irrGuess <= 0.0)
                              irrGuess = 0.5;
                      }

                      // start IRR =  guessed
                      double irr = 0.0;
                      if (sumCashFlows < 0.0)
                          irr = -irrGuess;
                      else
                          irr = irrGuess;

                      // loop --------------------------------------------------------------------------------
                      double minDistance = .0000001;
                      double cashFlowStart = cashFlows[0];
                      int maxIteration = 500;
                      bool wasHi = false;
                      double cashValue = 0.0;

                      double accuracy = absSumCashFlows * .000001;

                      for (int i = 0; i < maxIteration; i++)
                      {
                          // calculate cash with the current IRR ( NPV)
                          cashValue = 0.0;
                          for (int j = 0; j < noOfCashFlows; j++)
                          {
                              cashValue += cashFlows[j] / Math.Pow(1.0 + irr, j + 1);
                          }


                          if (Math.Abs(cashValue) < accuracy)
                          {
                              result = irr;
                              break;
                          }

                          // adjust irr for the next iteration
                          if (cashValue > 0.0)
                          {
                              if (wasHi)
                              {
                                  irrGuess /= 2.0;
                              }

                              irr += irrGuess;

                              if (wasHi)
                              {
                                  irrGuess -= minDistance;
                                  wasHi = false;
                              }
                          }
                          else
                          {
                              irrGuess /= 2.0;
                              irr -= irrGuess;
                              wasHi = true;
                          }

                          if (irrGuess <= minDistance) break;
                      }
                  }
            }

            return result;
        }




    }
}
