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
using System.Linq;
using System.Text;

namespace NFX.Web.Shipping
{
  /// <summary>
  /// Shipping status
  /// </summary>
  public enum TrackStatus
  {
    Unknown = 0,
    Transit = 1,
    Delivered = 2,
    Failure = 3,
    Returned = 4,
    Cancelled = 5,
    Error = 4
  }

  /// <summary>
  /// Track shipping info
  /// </summary>
  public class TrackInfo
  {
    #region Inner

      public class HistoryItem
      {
        public DateTime? Date     { get; set; }
        public TrackStatus Status { get; set; }
        public string Details     { get; set; }
        public Address Location   { get; set; }
      }

    #endregion

    public TrackInfo()
    {
      History = new List<HistoryItem>();
    }

    public DateTime? Date        { get; set; }
    public string Service        { get; set; }
    public string TrackingNumber { get; set; }
    public TrackStatus Status    { get; set; }
    public string Details        { get; set; }
    public Address FromAddress   { get; set; }
    public Address ToAddress     { get; set; }
    public Address Location      { get; set; }

    public List<HistoryItem> History { get; private set; }
  }
}
