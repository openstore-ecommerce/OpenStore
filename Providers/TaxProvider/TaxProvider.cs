using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using DotNetNuke.Entities.Content.Common;
using NBrightCore.common;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Components;
using TaxProvider;

namespace Nevoweb.DNN.NBrightBuy.Providers
{
    public class TaxProvider : Components.Interfaces.TaxInterface 
    {
        public override NBrightInfo Calculate(NBrightInfo cartInfo)
        {
            var info = ProviderUtils.GetProviderSettings("tax");
            var taxtype = info.GetXmlProperty("genxml/radiobuttonlist/taxtype");

            // return taxtype, 1 = included in cost, 2 = not included in cost, 3 = no tax, 4 = tax included in cost, but calculate to zero.
            cartInfo.SetXmlPropertyDouble("genxml/taxtype", taxtype);

            if (taxtype == "3" || taxtype == "4") // no tax calculation
            {
                cartInfo.SetXmlPropertyDouble("genxml/taxcost", 0);
                cartInfo.SetXmlPropertyDouble("genxml/appliedtax", 0);
                return cartInfo;
            }


            var rateDic = GetRates();
            if (!rateDic.Any()) return cartInfo;

            // loop through each item and calc the tax for each.
            var nodList = cartInfo.XMLDoc.SelectNodes("genxml/items/*");
            if (nodList != null)
            {
                Double taxtotal = 0;

                foreach (XmlNode nod in nodList)
                {
                    var nbi = new NBrightInfo();
                    nbi.XMLData = nod.OuterXml;
                    taxtotal += CalculateItemTax(nbi);
                }

                cartInfo.SetXmlPropertyDouble("genxml/taxcost", taxtotal);
                if (taxtype == "1") cartInfo.SetXmlPropertyDouble("genxml/appliedtax", 0); // tax already in total, so don't apply any more tax.
                if (taxtype == "2") cartInfo.SetXmlPropertyDouble("genxml/appliedtax", taxtotal);


                var taxcountry = cartInfo.GetXmlProperty("genxml/billaddress/genxml/dropdownlist/country");
                var storecountry = StoreSettings.Current.Get("storecountry");
                var valideutaxcountrycode = info.GetXmlProperty("genxml/textbox/valideutaxcountrycode");
                var isvalidEU = false;
                valideutaxcountrycode = "," + valideutaxcountrycode.ToUpper().Replace(" ", "") + ",";
                if ((valideutaxcountrycode.Contains("," + taxcountry.ToUpper().Replace(" ", "") + ","))) isvalidEU = true;

                // Check for EU tax number.
                var enabletaxnumber = info.GetXmlPropertyBool("genxml/checkbox/enabletaxnumber");
                if (enabletaxnumber)
                {
                    var taxnumber = cartInfo.GetXmlProperty("genxml/billaddress/genxml/textbox/taxnumber").Trim();
                    if (storecountry != taxcountry  && taxnumber != "")
                    {
                        // not matching merchant country, so remove tax 
                        if (taxtype == "1")
                        {
                            cartInfo.SetXmlPropertyDouble("genxml/taxcost", taxtotal*-1);
                            cartInfo.SetXmlPropertyDouble("genxml/appliedtax", taxtotal*-1);
                        }
                        if (taxtype == "2")
                        {
                            cartInfo.SetXmlPropertyDouble("genxml/taxcost", 0);
                            cartInfo.SetXmlPropertyDouble("genxml/appliedtax", 0);
                        }
                    }
                }

                // Check for country.
                var enabletaxcountry = info.GetXmlPropertyBool("genxml/checkbox/enabletaxcountry");
                if (enabletaxcountry)
                {
                    if (taxcountry != "")
                    {
                        if (taxcountry != storecountry && !isvalidEU)
                        {
                            // not matching merchant country, so remove tax 
                            if (taxtype == "1")
                            {
                                cartInfo.SetXmlPropertyDouble("genxml/taxcost", taxtotal * -1);
                                cartInfo.SetXmlPropertyDouble("genxml/appliedtax", taxtotal * -1);
                            }
                            if (taxtype == "2")
                            {
                                cartInfo.SetXmlPropertyDouble("genxml/taxcost", 0);
                                cartInfo.SetXmlPropertyDouble("genxml/appliedtax", 0);
                            }
                        }
                    }
                }

                // check for region exempt (in same country)
                var taxexemptregions = info.GetXmlProperty("genxml/textbox/taxexemptregions");
                if (taxexemptregions != "" && taxcountry == storecountry)
                {
                    taxexemptregions = "," + taxexemptregions.ToUpper().Replace(" ","") + ",";
                    var taxregioncode = cartInfo.GetXmlProperty("genxml/billaddress/genxml/dropdownlist/region").Trim();
                    if (taxregioncode == "") taxregioncode = cartInfo.GetXmlProperty("genxml/billaddress/genxml/textbox/txtregion").Trim();
                    if (taxregioncode != "")
                    {
                        if (!taxexemptregions.Contains("," + taxregioncode.ToUpper().Replace(" ", "") + ","))
                        {
                            // not matching merchant region, so remove tax 
                            if (taxtype == "1")
                            {
                                cartInfo.SetXmlPropertyDouble("genxml/taxcost", taxtotal * -1);
                                cartInfo.SetXmlPropertyDouble("genxml/appliedtax", taxtotal * -1);
                            }
                            if (taxtype == "2")
                            {
                                cartInfo.SetXmlPropertyDouble("genxml/taxcost", 0);
                                cartInfo.SetXmlPropertyDouble("genxml/appliedtax", 0);
                            }
                        }
                    }
                }
                

            }


            return cartInfo;
        }

        public override Double CalculateItemTax(NBrightInfo cartItemInfo)
        {
            var info = ProviderUtils.GetProviderSettings("tax");
            var taxtype = info.GetXmlProperty("genxml/radiobuttonlist/taxtype");
            
            if (taxtype == "3") return 0;

            var rateDic = GetRates();
            if (!rateDic.Any()) return 0;

            // loop through each item and calc the tax for each.
            Double taxtotal = 0;

            var totalcost = cartItemInfo.GetXmlPropertyDouble("genxml/totalcost");
            // check if dealer and if dealertotal cost exists. ()
            if (cartItemInfo.GetXmlPropertyBool("genxml/isdealer")) totalcost = cartItemInfo.GetXmlPropertyDouble("genxml/dealercost");
            var taxratecode = cartItemInfo.GetXmlProperty("genxml/taxratecode");
            if (!Utils.IsNumeric(taxratecode)) taxratecode = "0";
            if (!rateDic.ContainsKey(taxratecode)) taxratecode = "0";
            Double taxrate = 0;
            if (rateDic.ContainsKey(taxratecode)) taxrate = rateDic[taxratecode]; // Can happen is no default tax added.
            
            if (taxtype == "1") // included in unit price
            {
                taxtotal += totalcost - ((totalcost / (100 + taxrate)) * 100);
            }
            if (taxtype == "2") // NOT included in unit price
            {
                taxtotal += (totalcost / 100) * taxrate;
            }

            return Math.Round(taxtotal, 2);
        }


        public override Dictionary<string, double> GetRates()
        {
            var info = ProviderUtils.GetProviderSettings("tax");
            var rtnDic = new Dictionary<string, double>();

            var rate = info.GetXmlProperty("genxml/textbox/taxdefault").Replace("%", "").Trim();
            if (Utils.IsNumeric(rate)) rtnDic.Add("0", Convert.ToDouble(rate, CultureInfo.GetCultureInfo("en-US")));

            rate = info.GetXmlProperty("genxml/textbox/rate1").Replace("%", "").Trim();
            if (Utils.IsNumeric(rate)) rtnDic.Add("1", Convert.ToDouble(rate, CultureInfo.GetCultureInfo("en-US")));
            rate = info.GetXmlProperty("genxml/textbox/rate2").Replace("%", "").Trim();
            if (Utils.IsNumeric(rate)) rtnDic.Add("2", Convert.ToDouble(rate, CultureInfo.GetCultureInfo("en-US")));
            rate = info.GetXmlProperty("genxml/textbox/rate3").Replace("%", "").Trim();
            if (Utils.IsNumeric(rate)) rtnDic.Add("3", Convert.ToDouble(rate, CultureInfo.GetCultureInfo("en-US")));
            rate = info.GetXmlProperty("genxml/textbox/rate4").Replace("%", "").Trim();
            if (Utils.IsNumeric(rate)) rtnDic.Add("4", Convert.ToDouble(rate, CultureInfo.GetCultureInfo("en-US")));
            rate = info.GetXmlProperty("genxml/textbox/rate5").Replace("%", "").Trim();
            if (Utils.IsNumeric(rate)) rtnDic.Add("5", Convert.ToDouble(rate, CultureInfo.GetCultureInfo("en-US")));

            return rtnDic;
        }

        public override Dictionary<string, string> GetName()
        {
            var info = ProviderUtils.GetProviderSettings("tax");
            var rtnDic = new Dictionary<string, string>();

            var rate = info.GetXmlProperty("genxml/textbox/taxdefault").Replace("%", "").Trim();
            if (Utils.IsNumeric(rate)) rtnDic.Add("0", rate + "%");
            rate = info.GetXmlProperty("genxml/textbox/rate1").Replace("%", "").Trim();
            if (Utils.IsNumeric(rate)) rtnDic.Add("1", info.GetXmlProperty("genxml/textbox/name1"));
            rate = info.GetXmlProperty("genxml/textbox/rate2").Replace("%", "").Trim();
            if (Utils.IsNumeric(rate)) rtnDic.Add("2", info.GetXmlProperty("genxml/textbox/name2"));
            rate = info.GetXmlProperty("genxml/textbox/rate3").Replace("%", "").Trim();
            if (Utils.IsNumeric(rate)) rtnDic.Add("3", info.GetXmlProperty("genxml/textbox/name3"));
            rate = info.GetXmlProperty("genxml/textbox/rate4").Replace("%", "").Trim();
            if (Utils.IsNumeric(rate)) rtnDic.Add("4", info.GetXmlProperty("genxml/textbox/name4"));
            rate = info.GetXmlProperty("genxml/textbox/rate5").Replace("%", "").Trim();
            if (Utils.IsNumeric(rate)) rtnDic.Add("5", info.GetXmlProperty("genxml/textbox/name5"));

            return rtnDic;
        }
    }
}
