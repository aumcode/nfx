using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.Financial;
using NFX.Environment;

namespace NFX.Web.Pay
{
  /// <summary>
  /// Provides rate conversion services based on an APP config file
  /// </summary>
  public class ConfigBasedCurrencyMarket : ICurrencyMarket, IConfigurable
  {
    public const string CONFIG_TABLES_SECTION = "tables";
    public const string CONFIG_RATE_ATTR = "rate";

    protected IConfigSectionNode m_Node;

    public ConfigBasedCurrencyMarket()
    {
      m_Node = findDefaultNode();
    }

    public ConfigBasedCurrencyMarket(IConfigSectionNode node)
    {
      m_Node = node ?? findDefaultNode();
    }

    public void Configure(IConfigSectionNode node)
    {
      m_Node = node ?? findDefaultNode();
    }

    public virtual Amount ConvertCurrency(string rateTable, Amount from, string targetCurrencyISO)
    {
       IConfigSectionNode node = null;

       if (rateTable.IsNotNullOrWhiteSpace())
       {
         node = m_Node[CONFIG_TABLES_SECTION].Children.FirstOrDefault( tbl => tbl.IsSameName(rateTable));
       }

       if (node == null) node = m_Node;

       var mapping = node.Children.FirstOrDefault( c => c.IsSameName(from.CurrencyISO) && c.Value.EqualsOrdIgnoreCase(targetCurrencyISO));

       if (mapping==null)
         throw new PaymentException(StringConsts.PAYMENT_CURRENCY_CONVERSION_MAPPING_ERROR.Args(from.CurrencyISO, targetCurrencyISO, GetType().FullName));


       var rateAttr = mapping.AttrByName(CONFIG_RATE_ATTR);
       if (!rateAttr.Exists)
         throw new PaymentException(StringConsts.PAYMENT_CURRENCY_CONVERSION_MAPPING_ERROR.Args(from.CurrencyISO, targetCurrencyISO, GetType().FullName));


       var rate = rateAttr.ValueAsDecimal();

       return new Amount(targetCurrencyISO, from.Value * rate);

       //m_Node...search
        //      {
        //  usd=yen{ rate=17.345 }
        //  usd=rub{ rate=69.01 price=100{ rate=68.11 } price=1000{ rate=67.11 } }

        //  tables
        //  {
        //     safe
        //     {
        //        usd=yen{ rate=12.345 }
        //        usd=rub{ rate=67.11  }
        //     }

        //  }
        //}
    }

    private IConfigSectionNode findDefaultNode()
    {
      var result = App.ConfigRoot[PaySystem.CONFIG_PAYMENT_PROCESSING_SECTION]
                                 [PaySystem.CONFIG_CURRENCY_MARKET_SECTION];
      return result;
    }

  }
}
