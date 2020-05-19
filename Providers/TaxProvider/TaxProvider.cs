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
        private static readonly NBrightInfo _info;
        private static readonly string _taxType;

        static TaxProvider()
        {
            _info = ProviderUtils.GetProviderSettings("tax");
            _taxType = _info.GetXmlProperty("genxml/radiobuttonlist/taxtype");
        }

        public override NBrightInfo Calculate(NBrightInfo cartInfo)
        {
            var enableShippingTax = _info.GetXmlPropertyBool("genxml/checkbox/enableshippingtax");
            var shippingTaxRate = _info.GetXmlPropertyDouble("genxml/textbox/shippingtaxrate");

            // return taxtype, 1 = included in cost, 2 = not included in cost, 3 = no tax, 4 = tax included in cost, but calculate to zero.
            cartInfo.SetXmlPropertyDouble("genxml/taxtype", _taxType);

            if (_taxType == "3" || _taxType == "4") // no tax calculation
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
                    taxtotal += CalculateItemTax(nbi, rateDic);
                }

                if (enableShippingTax && shippingTaxRate > 0)
                {
                    var isDealer = false; // default until dealer shipping cost implemented
                    taxtotal += CalculateShippingTax(cartInfo, shippingTaxRate, isDealer);
                }

                cartInfo.SetXmlPropertyDouble("genxml/taxcost", taxtotal);
                if (_taxType == "1") cartInfo.SetXmlPropertyDouble("genxml/appliedtax", 0); // tax already in total, so don't apply any more tax.
                if (_taxType == "2") cartInfo.SetXmlPropertyDouble("genxml/appliedtax", taxtotal);


                var taxcountry = cartInfo.GetXmlProperty("genxml/billaddress/genxml/dropdownlist/country");
                var storecountry = StoreSettings.Current.Get("storecountry");
                var valideutaxcountrycode = _info.GetXmlProperty("genxml/textbox/valideutaxcountrycode");
                var isvalidEU = false;
                valideutaxcountrycode = "," + valideutaxcountrycode.ToUpper().Replace(" ", "") + ",";
                if ((valideutaxcountrycode.Contains("," + taxcountry.ToUpper().Replace(" ", "") + ","))) isvalidEU = true;

                // Check for EU tax number.
                var enabletaxnumber = _info.GetXmlPropertyBool("genxml/checkbox/enabletaxnumber");
                if (enabletaxnumber)
                {
                    var taxnumber = cartInfo.GetXmlProperty("genxml/billaddress/genxml/textbox/taxnumber").Trim();
                    if (storecountry != taxcountry  && taxnumber != "")
                    {
                        // not matching merchant country, so remove tax 
                        if (_taxType == "1")
                        {
                            cartInfo.SetXmlPropertyDouble("genxml/taxcost", taxtotal*-1);
                            cartInfo.SetXmlPropertyDouble("genxml/appliedtax", taxtotal*-1);
                        }
                        if (_taxType == "2")
                        {
                            cartInfo.SetXmlPropertyDouble("genxml/taxcost", 0);
                            cartInfo.SetXmlPropertyDouble("genxml/appliedtax", 0);
                        }
                    }
                }

                // Check for country.
                var enabletaxcountry = _info.GetXmlPropertyBool("genxml/checkbox/enabletaxcountry");
                if (enabletaxcountry)
                {
                    if (taxcountry != "")
                    {
                        if (taxcountry != storecountry && !isvalidEU)
                        {
                            // not matching merchant country, so remove tax 
                            if (_taxType == "1")
                            {
                                cartInfo.SetXmlPropertyDouble("genxml/taxcost", taxtotal * -1);
                                cartInfo.SetXmlPropertyDouble("genxml/appliedtax", taxtotal * -1);
                            }
                            if (_taxType == "2")
                            {
                                cartInfo.SetXmlPropertyDouble("genxml/taxcost", 0);
                                cartInfo.SetXmlPropertyDouble("genxml/appliedtax", 0);
                            }
                        }
                    }
                }

                // check for region exempt (in same country)
                var taxexemptregions = _info.GetXmlProperty("genxml/textbox/taxexemptregions");
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
                            if (_taxType == "1")
                            {
                                cartInfo.SetXmlPropertyDouble("genxml/taxcost", taxtotal * -1);
                                cartInfo.SetXmlPropertyDouble("genxml/appliedtax", taxtotal * -1);
                            }
                            if (_taxType == "2")
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
            if (_taxType == "3") return 0;

            var rateDic = GetRates();
            return (CalculateItemTax(cartItemInfo, rateDic));
        }

        public Double CalculateItemTax(NBrightInfo cartItemInfo, Dictionary<string, double> rateDic)
        {

            if (_taxType == "3") return 0;

            if (!rateDic.Any()) return 0;

            // loop through each item and calc the tax for each.
            Double taxtotal = 0;

            var appliedcost = cartItemInfo.GetXmlPropertyDouble("genxml/appliedcost");
            // check if dealer and if dealertotal cost exists. ()
            if (cartItemInfo.GetXmlPropertyBool("genxml/isdealer")) appliedcost = cartItemInfo.GetXmlPropertyDouble("genxml/dealercost");

            var taxrate = GetTaxRate(cartItemInfo, rateDic);

            taxtotal = AddTaxCost(taxtotal, appliedcost, taxrate);

            return Math.Round(taxtotal, 2);
        }

        private static Double CalculateShippingTax(NBrightInfo cartInfo, double taxrate, Boolean isDealer)
        {

            if (_taxType == "3") return 0;

            Double taxtotal = 0;
            var shippingcost = cartInfo.GetXmlPropertyDouble("genxml/shippingcost");

            // check if dealer and if dealer shipping cost exists.
            var dealerShippingCost = cartInfo.GetXmlPropertyDouble("genxml/dealershippingcost");
            if (isDealer && dealerShippingCost > 0) shippingcost = dealerShippingCost;

            taxtotal = AddTaxCost(taxtotal, shippingcost, taxrate);

            return Math.Round(taxtotal, 2);
        }

        private static double GetTaxRate(NBrightInfo cartItemInfo, Dictionary<string, double> rateDic)
        {
            Double taxrate = 0;
            var taxratecode = cartItemInfo.GetXmlProperty("genxml/taxratecode");

            if (!Utils.IsNumeric(taxratecode)) taxratecode = "0";
            if (!rateDic.ContainsKey(taxratecode)) taxratecode = "0";
            
            if (rateDic.ContainsKey(taxratecode)) taxrate = rateDic[taxratecode]; // Can happen if no default tax added.
            return taxrate;
        }

        private static double AddTaxCost(double taxtotal, double cost, double taxrate)
        {
            if (_taxType == "1") // included in unit price
            {
                taxtotal += cost - ((cost / (100 + taxrate)) * 100);
            }
            if (_taxType == "2") // NOT included in unit price
            {
                taxtotal += (cost / 100) * taxrate;
            }

            return taxtotal;
        }

        public override Dictionary<string, double> GetRates()
        {
            var rtnDic = new Dictionary<string, double>();

            var rate = _info.GetXmlProperty("genxml/textbox/taxdefault").Replace("%", "").Trim();
            if (Utils.IsNumeric(rate)) rtnDic.Add("0", Convert.ToDouble(rate, CultureInfo.GetCultureInfo("en-US")));

            rate = _info.GetXmlProperty("genxml/textbox/rate1").Replace("%", "").Trim();
            if (Utils.IsNumeric(rate)) rtnDic.Add("1", Convert.ToDouble(rate, CultureInfo.GetCultureInfo("en-US")));
            rate = _info.GetXmlProperty("genxml/textbox/rate2").Replace("%", "").Trim();
            if (Utils.IsNumeric(rate)) rtnDic.Add("2", Convert.ToDouble(rate, CultureInfo.GetCultureInfo("en-US")));
            rate = _info.GetXmlProperty("genxml/textbox/rate3").Replace("%", "").Trim();
            if (Utils.IsNumeric(rate)) rtnDic.Add("3", Convert.ToDouble(rate, CultureInfo.GetCultureInfo("en-US")));
            rate = _info.GetXmlProperty("genxml/textbox/rate4").Replace("%", "").Trim();
            if (Utils.IsNumeric(rate)) rtnDic.Add("4", Convert.ToDouble(rate, CultureInfo.GetCultureInfo("en-US")));
            rate = _info.GetXmlProperty("genxml/textbox/rate5").Replace("%", "").Trim();
            if (Utils.IsNumeric(rate)) rtnDic.Add("5", Convert.ToDouble(rate, CultureInfo.GetCultureInfo("en-US")));

            return rtnDic;
        }

        public override Dictionary<string, string> GetName()
        {
            var rtnDic = new Dictionary<string, string>();

            var rate = _info.GetXmlProperty("genxml/textbox/taxdefault").Replace("%", "").Trim();
            if (Utils.IsNumeric(rate)) rtnDic.Add("0", rate + "%");
            rate = _info.GetXmlProperty("genxml/textbox/rate1").Replace("%", "").Trim();
            if (Utils.IsNumeric(rate)) rtnDic.Add("1", _info.GetXmlProperty("genxml/textbox/name1"));
            rate = _info.GetXmlProperty("genxml/textbox/rate2").Replace("%", "").Trim();
            if (Utils.IsNumeric(rate)) rtnDic.Add("2", _info.GetXmlProperty("genxml/textbox/name2"));
            rate = _info.GetXmlProperty("genxml/textbox/rate3").Replace("%", "").Trim();
            if (Utils.IsNumeric(rate)) rtnDic.Add("3", _info.GetXmlProperty("genxml/textbox/name3"));
            rate = _info.GetXmlProperty("genxml/textbox/rate4").Replace("%", "").Trim();
            if (Utils.IsNumeric(rate)) rtnDic.Add("4", _info.GetXmlProperty("genxml/textbox/name4"));
            rate = _info.GetXmlProperty("genxml/textbox/rate5").Replace("%", "").Trim();
            if (Utils.IsNumeric(rate)) rtnDic.Add("5", _info.GetXmlProperty("genxml/textbox/name5"));

            return rtnDic;
        }
    }
}
