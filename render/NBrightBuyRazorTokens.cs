using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Web;
using System.Web.Routing;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using DotNetNuke.Collections;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.WebControls;
using NBrightCore.common;
using NBrightCore.providers;
using NBrightCore.render;
using NBrightDNN;
using NBrightDNN.render;
using Nevoweb.DNN.NBrightBuy.Components;
using RazorEngine.Templating;
using RazorEngine.Text;
using System.Drawing.Imaging;
using NBrightCore.images;
using System.IO;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Users;
using Nevoweb.DNN.NBrightBuy;
using Nevoweb.DNN.NBrightBuy.Admin;
using Nevoweb.DNN.NBrightBuy.Components.Interfaces;

namespace NBrightBuy.render
{
    public class NBrightBuyRazorTokens<T> : RazorEngineTokens<T>
    {

        #region "NBS display tokens"

        #region "products"

        /// <summary>
        /// Product Name
        /// </summary>
        /// <param name="info">NBrightInfo class of PRD type</param>
        /// <returns>Product name text</returns>
        public IEncodedString ProductName(NBrightInfo info)
        {
            return new RawString(info.GetXmlProperty("genxml/lang/genxml/textbox/txtproductname"));
        }

        public IEncodedString EditLangaugeButton(int size = 32)
        {
            return EditLanguageButton(size);
        }

        public IEncodedString EditLanguageButton(int size = 32)
        {
            try
            {
                var strOut1 = "";
                //NOTE: We need to recreate dynamically created controls on postback for them to pickup the event. 
                var enabledlanguages = LocaleController.Instance.GetLocales(PortalSettings.Current.PortalId);
                strOut1 = "<ul class='editlanguage'>";
                foreach (var l in enabledlanguages)
                {
                    strOut1 += "<li class='selectlang' editlang='" + l.Value.Code + "'>";
                    strOut1 += "<img src='" + StoreSettings.NBrightBuyPath() + "/Themes/config/img/flags/" + size + "/" + l.Value.Code + ".png' alt='" + l.Value.EnglishName + "' />";
                    strOut1 += "</li>";
                }
                strOut1 += "</ul>";
                return new RawString(strOut1);
            }
            catch (Exception e)
            {
                return new RawString(e.ToString());
            }
        }

        public IEncodedString LanguageDropDown(NBrightInfo info, String xpath, String attributes = "", Boolean allowEmpty = false)
        {

            var strOut = "";
            var rtnList = LocaleController.Instance.GetLocales(PortalSettings.Current.PortalId);

            if (rtnList != null && rtnList.Count > 0)
            {
                // we have a list, so do a dropdownlist
                if (attributes.StartsWith("ResourceKey:")) attributes = ResourceKey(attributes.Replace("ResourceKey:", "")).ToString();

                var upd = getUpdateAttr(xpath, attributes);
                var id = getIdFromXpath(xpath);
                strOut = "<select id='" + id + "' " + upd + " " + attributes + ">";
                var s = "";
                if (allowEmpty) strOut += "    <option value=''></option>";
                foreach (var l in rtnList)
                {
                    if (info.GetXmlProperty(xpath) == l.Value.Code)
                        s = "selected";
                    else
                        s = "";
                    strOut += "    <option value='" + l.Value.Code + "' " + s + ">" + l.Value.NativeName + "</option>";
                }
                strOut += "</select>";
            }
            return new RawString(strOut);
        }

        public IEncodedString TaxRateDropDown(NBrightInfo info,string xpath,string providerkey, String attributes = "",bool allowblank = true)
        {
            try
            {
                var strOut = "";
                var id = getIdFromXpath(xpath);
                strOut = "<select id='" + id + "' "  + attributes + " >";
                var tList = TaxInterface.Instance(providerkey).GetName();
                if (allowblank)
                {
                    strOut += "    <option value='' ></option>";
                }
                foreach (var tItem in tList)
                {
                    var s = "";
                    if (tItem.Key == info.GetXmlProperty(xpath)) s = "selected";

                    strOut += "    <option value='" + tItem.Key + "' " + s + " >" + tItem.Value + "</option>";
                }

                strOut += "</select>";
                return new RawString(strOut);
            }
            catch (Exception e)
            {
                return new RawString(e.ToString());
            }
        }


        public IEncodedString ModelStatusDropDown(NBrightInfo info, string xpath, string attributes = "", bool allowblank = true)
        {
            try
            {

                var resxpath = StoreSettings.NBrightBuyPath() + "/App_LocalResources/General.ascx.resx";
                var orderstatuscode = DnnUtils.GetLocalizedString("modelstatus.Code", resxpath, Utils.GetCurrentCulture());
                var orderstatustext = DnnUtils.GetLocalizedString("modelstatus.Text", resxpath, Utils.GetCurrentCulture());
                if (orderstatuscode != null && orderstatustext != null)
                {
                    if (allowblank)
                    {
                        orderstatuscode = "000," + orderstatuscode;
                        orderstatustext = "," + orderstatustext;
                    }
                }

                var strOut = "";
                var id = getIdFromXpath(xpath);
                strOut = "<select id='" + id + "' " + attributes + " >";

                var aryCode = orderstatuscode.Split(',');
                var aryText = orderstatustext.Split(',');

                var lp = 0;
                foreach (var c in aryCode)
                {
                    var s = "";
                    if (c == info.GetXmlProperty(xpath)) s = "selected";
                    strOut += "    <option value='" + c + "' " + s + " >" + aryText[lp] + "</option>";
                    lp += 1;
                }


                strOut += "</select>";
                return new RawString(strOut);
            }
            catch (Exception e)
            {
                return new RawString(e.ToString());
            }

        }


        /// <summary>
        /// image url or Thumbnail url
        /// </summary>
        /// <param name="info">NBrightInfo class of PRD type</param>
        /// <param name="width">width</param>
        /// <param name="height">height</param>
        /// <param name="idx">index of the image to display</param>
        /// <param name="attributes">free text added onto end of url parameters</param>
        /// <returns>image url</returns>
        public IEncodedString ProductImageUrl(NBrightInfo info, int width = 0, int height = 0, int idx = 1, string attributes = "")
        {
            var url = info.GetXmlProperty("genxml/lang/genxml/imgs/genxml[" + idx + "]/hidden/fimageurl");
            if (url == "" || width > 0 || height > 0)
            {
                if (url == "")
                {
                    url = info.GetXmlProperty("genxml/imgs/genxml[" + idx + "]/hidden/imageurl");
                }
                url = StoreSettings.NBrightBuyPath() + "/NBrightThumb.ashx?src=" + url + "&w=" + width + "&h=" + height + attributes;
            }
            return new RawString(url);
        }

        /// <summary>
        /// Edit Product url
        /// </summary>
        /// <param name="info">NBrightInfo class of PRD type</param>
        /// <param name="model">Razor Model class (NBrightRazor)</param>
        /// <returns>Url to edit product</returns>
        public IEncodedString EditUrl(NBrightInfo info, NBrightRazor model,String ctrl = "products")
        {
            var itemid = -1;
            if (info != null) itemid = info.ItemID;
            return EditUrl(itemid, model,ctrl);
        }

        public IEncodedString EditUrl(int itemid, NBrightRazor model, String ctrl = "products")
        {
            var entryid = itemid;
            var url = "Unable to find BackOffice Setting, go into Back Office settings and save.";
            if (Utils.IsNumeric(entryid) && StoreSettings.Current.GetInt("backofficetabid") > 0)
            {
                var param = new List<String>();

                param.Add("eid=" + entryid.ToString(""));
                param.Add("ctrl=" + ctrl);
                param.Add("rtntab=" + PortalSettings.Current.ActiveTab.TabID.ToString());
                if (model.GetSetting("moduleid") != "") param.Add("rtnmid=" + model.GetSetting("moduleid").Trim());
                if (model.GetUrlParam("page") != "") param.Add("PageIndex=" + model.GetUrlParam("page").Trim());
                if (model.GetUrlParam("catid") != "") param.Add("catid=" + model.GetUrlParam("catid").Trim());

                var paramlist = new string[param.Count];
                for (int lp = 0; lp < param.Count; lp++)
                {
                    paramlist[lp] = param[lp];
                }

                url = Globals.NavigateURL(StoreSettings.Current.GetInt("backofficetabid"), "", paramlist);
            }
            return new RawString(url);
        }

        public IEncodedString ModelsRadio(NBrightInfo info, String attributes = "", String template = "{name} ({bestprice})", Int32 defaultIndex = 0, Boolean displayprice = false)
        {
            var strOut = "";
            var objL = NBrightBuyUtils.BuildModelList(info, true);

            if (!displayprice)
            {
                displayprice = NBrightBuyUtils.HasDifferentPrices(info);
            }

            var c = 0;
            var id = info.ItemID + "_rblmodelsel";
            var s = "";
            var v = "";
            var atts = "";
            var attributeshidden = attributes + " style='display:none;' ";

            foreach (var obj in objL)
            {
                var text = NBrightBuyUtils.GetItemDisplay(obj, template, displayprice);
                var value = obj.GetXmlProperty("genxml/hidden/modelid");
                if (value == v || (v == "" && defaultIndex == c))
                    s = "checked";
                else
                    s = "";

                atts = (text == "") ? attributeshidden : attributes;

                strOut += "<div " + atts + "><input id='" + id + "_" + c.ToString("") + "' update='save' name='" + id + "' type='radio' value='" + value + "'  " + s + "/><label>" + text + "</label></div>";
                c += 1;

            }
            return new RawString(strOut);
        }

        public IEncodedString ModelsDropDown(NBrightInfo info, String attributes = "", String template = "{name} ({bestprice})", Int32 defaultIndex = 0, Boolean displayprice = false)
        {
            var strOut = "";
            var objL = NBrightBuyUtils.BuildModelList(info, true);

            if (!displayprice)
            {
                displayprice = NBrightBuyUtils.HasDifferentPrices(info);
            }

            var c = 0;
            var id = info.ItemID + "_ddlmodelsel";
            var s = "";
            var v = "";
            strOut = "<select id='" + id + "' update='save' " + attributes + ">";
            foreach (var obj in objL)
            {
                var text = NBrightBuyUtils.GetItemDisplay(obj, template, displayprice);
                var value = obj.GetXmlProperty("genxml/hidden/modelid");
                if (value == v || (v == "" && defaultIndex == c))
                    s = "selected";
                else
                    s = "";

                if (text != "")  // no stock so don;t display.
                {
                    strOut += "    <option value='" + value + "' " + s + ">" + text + "</option>";
                }
                c += 1;
            }
            strOut += "</select>";

            return new RawString(strOut);
        }

        public IEncodedString ProductOption(ProductData productdata, int index, String attributes = "", Boolean required = false)
        {
            var strOut = "";

            var objL = productdata.Options;

            if (objL.Count > index)
            {
                var requiredattr = "";
                var obj = objL[index];
                var optid = obj.GetXmlProperty("genxml/hidden/optionid");
                var optvalList = productdata.GetOptionValuesById(optid);
                if (obj.GetXmlPropertyBool("genxml/checkbox/optrequired")) required = true;

                strOut += "<div  class='option option" + (index + 1) + "' " + attributes + ">";
                strOut += "<span class='optionname optionname" + (index + 1) + "'>" + obj.GetXmlProperty("genxml/lang/genxml/textbox/txtoptiondesc") + "</span>";
                strOut += "<span class='optionvalue optionvalue" + (index + 1) + "'>";

                if (optvalList.Count > 1)
                {
                    if (required) requiredattr = " required='' name='optionddl" + (index + 1) + "'"; // name also needs to be added for JQuery Validation to work correctly
                    //dropdown
                    strOut += "<select id='optionddl" + (index + 1) + "' " + requiredattr + " update='save'>";
                    if (required)
                    {
                        strOut += "    <option value=''>" + NBrightBuyUtils.ResourceKey("General.pleaseselect") + "</option>";
                    }

                    foreach (var optval in optvalList)
                    {
                        var addcost = optval.GetXmlPropertyDouble("genxml/textbox/txtaddedcost");
                        var addedcostdisplay = "";
                        if (addcost > 0)
                        {
                            addedcostdisplay = "    (+" + NBrightBuyUtils.FormatToStoreCurrency(addcost) + ")";
                        }
                            

                        strOut += "    <option value='" + optval.GetXmlProperty("genxml/hidden/optionvalueid") + "'>" + optval.GetXmlProperty("genxml/lang/genxml/textbox/txtoptionvaluedesc") + addedcostdisplay + "</option>";
                    }
                    strOut += "</select>";
                }
                if (optvalList.Count == 1)
                {
                    //checkbox
                    foreach (var optval in optvalList)
                    {
                        var addcost = optval.GetXmlPropertyDouble("genxml/textbox/txtaddedcost");
                        var addedcostdisplay = "";
                        if (addcost > 0)
                        {
                            addedcostdisplay = "    (+" + NBrightBuyUtils.FormatToStoreCurrency(addcost) + ")";
                        }

                        strOut += "    <input id='optionchk" + (index + 1) + "' type='checkbox' " + attributes + " update='save' /><label>" + optval.GetXmlProperty("genxml/lang/genxml/textbox/txtoptionvaluedesc") + addedcostdisplay + "</label>";
                    }
                }
                if (optvalList.Count == 0)
                {
                    // textbox
                    if (required) requiredattr = " required='' name='optiontxt" + (index + 1) + "'"; // name also needs to be added for JQuery Validation to work correctly
                    strOut += "<input id='optiontxt" + (index + 1) + "' " + requiredattr + " update='save' type='text' />";
                }
                strOut += "<input id='optionid" + (index + 1) + "' update='save' type='hidden' value='" + optid + "' />";
                strOut += "</span>";
                strOut += "</div>";

            }


            return new RawString(strOut);
        }

        public IEncodedString ProductOptions(ProductData  productdata, String attributes = "")
        {
            var strOut = "";

            var objL = productdata.Options;
            var c = objL.Count;
            for (int i = 0; i < c; i++)
            {
                strOut += ProductOption(productdata, i);
            }

            return new RawString(strOut);
        }

        #endregion

        #region "categories"

        public IEncodedString Category(String fieldname, NBrightRazor model)
        {
            var strOut = "";
            try
            {
                var navigationdata = new NavigationData(PortalSettings.Current.PortalId, model.GetSetting("modref"));

                // if we have no catid in url, we're going to need a default category from module.
                var grpCatCtrl = new GrpCatController(Utils.GetCurrentCulture());
                var objCInfo = grpCatCtrl.GetGrpCategory(navigationdata.CategoryId);
                if (objCInfo != null)
                {
                    GroupCategoryData objPcat;
                    switch (fieldname.ToLower())
                    {
                        case "categorydesc":
                            strOut = objCInfo.categorydesc;
                            break;
                        case "message":
                            strOut = System.Web.HttpUtility.HtmlDecode(objCInfo.message);
                            break;
                        case "archived":
                            strOut = objCInfo.archived.ToString(CultureInfo.InvariantCulture);
                            break;
                        case "breadcrumb":
                            strOut = objCInfo.breadcrumb;
                            break;
                        case "categoryid":
                            strOut = objCInfo.categoryid.ToString("");
                            break;
                        case "categoryname":
                            strOut = objCInfo.categoryname;
                            break;
                        case "categoryref":
                            strOut = objCInfo.categoryref;
                            break;
                        case "depth":
                            strOut = objCInfo.depth.ToString("");
                            break;
                        case "disabled":
                            strOut = objCInfo.disabled.ToString(CultureInfo.InvariantCulture);
                            break;
                        case "entrycount":
                            strOut = objCInfo.entrycount.ToString("");
                            break;
                        case "grouptyperef":
                            strOut = objCInfo.grouptyperef;
                            break;
                        case "attributecode":
                            strOut = objCInfo.attributecode;
                            break;
                        case "imageurl":
                            strOut = objCInfo.imageurl;
                            break;
                        case "ishidden":
                            strOut = objCInfo.ishidden.ToString(CultureInfo.InvariantCulture);
                            break;
                        case "isvisible":
                            strOut = objCInfo.isvisible.ToString(CultureInfo.InvariantCulture);
                            break;
                        case "metadescription":
                            strOut = objCInfo.metadescription;
                            break;
                        case "metakeywords":
                            strOut = objCInfo.metakeywords;
                            break;
                        case "parentcatid":
                            strOut = objCInfo.parentcatid.ToString("");
                            break;
                        case "parentcategoryname":
                            objPcat = grpCatCtrl.GetCategory(objCInfo.parentcatid);
                            strOut = objPcat.categoryname;
                            break;
                        case "parentcategoryref":
                            objPcat = grpCatCtrl.GetCategory(objCInfo.parentcatid);
                            strOut = objPcat.categoryref;
                            break;
                        case "parentcategorydesc":
                            objPcat = grpCatCtrl.GetCategory(objCInfo.parentcatid);
                            strOut = objPcat.categorydesc;
                            break;
                        case "parentcategorybreadcrumb":
                            objPcat = grpCatCtrl.GetCategory(objCInfo.parentcatid);
                            strOut = objPcat.breadcrumb;
                            break;
                        case "parentcategoryguidkey":
                            objPcat = grpCatCtrl.GetCategory(objCInfo.parentcatid);
                            strOut = objPcat.categoryrefGUIDKey;
                            break;
                        case "recordsortorder":
                            strOut = objCInfo.recordsortorder.ToString("");
                            break;
                        case "seoname":
                            strOut = objCInfo.seoname;
                            if (strOut == "") strOut = objCInfo.categoryname;
                            break;
                        case "seopagetitle":
                            strOut = objCInfo.seopagetitle;
                            break;
                        case "url":
                            strOut = objCInfo.url;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                strOut = ex.ToString();
            }

            return new RawString(strOut);
        }

        public IEncodedString CategoryBreadCrumb(Boolean includelinks, NBrightRazor model, Boolean aslist = true, int tabRedirect = -1, String separator = "", int wordlength = -1, int maxlength = 400,bool ajax = false)
        {
            var strOut = "";

            try
            {
                var catid = 0;
                if (model.GetUrlParam("eid") != "")
                {
                    // looking at detail, so use product categoryid
                    if (model.List.Any())
                    {
                        var product = (ProductData)model.List.First();
                        var catgrp = product.GetDefaultCategory();
                        catid = catgrp != null ? catgrp.categoryid : 0;
                    }
                }
                else
                {
                    var navigationdata = new NavigationData(PortalSettings.Current.PortalId, model.GetSetting("modref"));
                    catid = navigationdata.CategoryId;
                }

                if (catid <= 0) // check we have a catid
                {
                    return new RawString(""); ;
                }

                var grpCatCtrl = new GrpCatController(Utils.GetCurrentCulture());
                var objCInfo = grpCatCtrl.GetGrpCategory(catid);
                if (objCInfo != null)
                {
                    if (includelinks)
                    {
                        if (tabRedirect == 0) tabRedirect = PortalSettings.Current.ActiveTab.TabID;
                        if (tabRedirect == -1) tabRedirect = StoreSettings.Current.ProductListTabId;
                        strOut = grpCatCtrl.GetBreadCrumbWithLinks(catid, tabRedirect, wordlength, separator, aslist,false, ajax);
                    }
                    else
                    {
                        strOut = grpCatCtrl.GetBreadCrumb(catid, wordlength, separator, aslist);
                    }

                    if ((strOut.Length > maxlength) && (!aslist))
                    {
                        strOut = strOut.Substring(0, (maxlength - 3)) + "...";
                    }
                }
            }
            catch (Exception ex)
            {
                strOut = ex.ToString();
            }


            return new RawString(strOut);
        }

        public IEncodedString CategoryDropDownList(NBrightInfo info, String xpath, String attributes = "", Boolean allowEmpty = true, int displaylevels = 20, Boolean showHidden = false, Boolean showArchived = false, int parentid = 0, String catreflist = "", String prefix = "", bool displayCount = false, bool showEmpty = true, string groupref = "", string breadcrumbseparator = ">", string lang = "")
        {
            return CategorySelectList( info,  xpath, attributes ,  allowEmpty ,  displaylevels ,  showHidden ,  showArchived , parentid , catreflist , prefix ,  displayCount ,  showEmpty ,  groupref ,  breadcrumbseparator , lang);
        }

        public IEncodedString CategorySelectList(NBrightInfo info, String xpath, String attributes = "", Boolean allowEmpty = true, int displaylevels = 20, Boolean showHidden = false, Boolean showArchived = false, int parentid = 0, String catreflist = "", String prefix = "", bool displayCount = false, bool showEmpty = true, string groupref = "", string breadcrumbseparator = ">", string lang = "")
        {
            var rtnList = NBrightBuyUtils.BuildCatList(displaylevels, showHidden, showArchived, parentid, catreflist, prefix, displayCount, showEmpty, groupref, breadcrumbseparator, lang);

            if (attributes.StartsWith("ResourceKey:")) attributes = ResourceKey(attributes.Replace("ResourceKey:", "")).ToString();

            var strOut = "";

            var upd = getUpdateAttr(xpath, attributes);
            var id = getIdFromXpath(xpath);
            strOut = "<select id='" + id + "' " + upd + " " + attributes + ">";
            var s = "";
            if (allowEmpty) strOut += "    <option value=''></option>";
            foreach (var tItem in rtnList)
            {
                if (info.GetXmlProperty(xpath) == tItem.Key.ToString())
                    s = "selected";
                else
                    s = "";
                strOut += "    <option value='" + tItem.Key.ToString() + "' " + s + ">" + tItem.Value + "</option>";
            }
            strOut += "</select>";

            return new RawString(strOut);
        }


        public IEncodedString GroupSelectList(NBrightInfo info, String xpath, String attributes = "", Boolean allowEmpty = true,String groupType = "1")
        {
            var strOut = "";
            var upd = getUpdateAttr(xpath, attributes);
            var id = getIdFromXpath(xpath);
            if (attributes.StartsWith("ResourceKey:")) attributes = ResourceKey(attributes.Replace("ResourceKey:", "")).ToString();

            strOut = "<select id='" + id + "' " + upd + " " + attributes + ">";
            var s = "";
            if (allowEmpty) strOut += "    <option value=''></option>";

            var tList = NBrightBuyUtils.GetCategoryGroups(StoreSettings.Current.EditLanguage, true, groupType);
            foreach (var tItem in tList)
            {
                if (tItem.GetXmlProperty("genxml/textbox/groupref") != "cat")
                {
                    if (info.GetXmlProperty(xpath) == tItem.GetXmlProperty("genxml/textbox/groupref"))
                        s = "selected";
                    else
                        s = "";
                    strOut += "    <option value='" + tItem.GetXmlProperty("genxml/textbox/groupref") + "' " + s + ">" + tItem.GetXmlProperty("genxml/textbox/groupref") + ": " + tItem.GetXmlProperty("genxml/lang/genxml/textbox/groupname") + "</option>";
                }
            }

            strOut += "</select>";

            return new RawString(strOut);
        }

        public IEncodedString GroupCheckboxList(NBrightInfo info, String xpath, String attributes = "", Boolean allowEmpty = true,String groupType = "1", bool addSortOrderInput = false)
        {
            var strOut = "";
            var upd = getUpdateAttr(xpath, attributes);
            var id = getIdFromXpath(xpath);
            if (attributes.StartsWith("ResourceKey:")) attributes = ResourceKey(attributes.Replace("ResourceKey:", "")).ToString();

            strOut = $"<div {attributes}>";
            var @checked = "";
            var checkedClass = "";
            var tList = NBrightBuyUtils.GetCategoryGroups(StoreSettings.Current.EditLanguage, true, groupType);
            var hasItems = false;
            foreach (var tItem in tList)
            {
                var groupRef = tItem.GetXmlProperty("genxml/textbox/groupref");
                var groupName = tItem.GetXmlProperty("genxml/textbox/groupname");
                if (tItem.GetXmlProperty("genxml/textbox/groupref") != "cat")
                {
                    if (info.GetXmlPropertyBool($"genxml/checkbox/{id}-{groupRef}"))
                    {
                        @checked = "checked";
                        checkedClass = "dnnCheckbox-checked";
                    }
                    else
                    {
                        @checked = "";
                        checkedClass = "";
                    }
                    var sortOrderInput = "";
                    if (addSortOrderInput)
                    {
                        var sortValue = info.GetXmlPropertyInt($"genxml/checkbox/{id}Sort-{groupRef}");
                        sortOrderInput = $"<input type=\"hidden\" id=\"{id}Sort-{groupRef}\" value=\"{sortValue}\" {upd}>";
                    }

                    strOut += $"    <div>{sortOrderInput}<input id='{id}-{groupRef}' type='checkbox' value='{groupRef}' {@checked} {upd}><span class='dnnCheckbox dnnCheckboxLabel {checkedClass}'>{groupName} ({groupRef})</span></div>";
                    hasItems = true;
                }
            }

            strOut += "</div>";

            return new RawString(strOut);
        }


        #endregion

        #region "properties"
        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="xpath"></param>
        /// <param name="propertytype">type of property using propertygroup ref  (e.g. "man" = manufacturer)</param>
        /// <param name="attributes"></param>
        /// <param name="showHidden"></param>
        /// <param name="lang"></param>
        /// <returns></returns>
        public IEncodedString PropertyCheckboxList(NBrightInfo info, String xpath, string propertytype, String attributes = "", Boolean showHidden = false, string lang = "")
        {
            var objCtrl = new NBrightBuyController();
            var parentnbi = objCtrl.GetByGuidKey(PortalSettings.Current.PortalId, -1, "GROUP", propertytype);
            var rtnList = NBrightBuyUtils.BuildCatList(1, showHidden, true, parentnbi.ItemID, "", "",false, showHidden, propertytype, "", lang);

            if (attributes.StartsWith("ResourceKey:")) attributes = ResourceKey(attributes.Replace("ResourceKey:", "")).ToString();

            var strOut = "";

            var upd = getUpdateAttr(xpath, attributes);
            var id = getIdFromXpath(xpath);
            strOut = "<ul id='" + id + "' " + upd + " " + attributes + ">";
            var s = "";
            var lp = 0;
            foreach (var tItem in rtnList)
            {
                var checkedvar = "";
                if (info.GetXmlPropertyBool("genxml/checkboxlist/" + id + "/chk[" + (lp + 1) + "]/@value")) checkedvar = " checked='checked' ";
                if (info.GetXmlProperty("genxml/checkboxlist/" + id + "/chk[" + (lp + 1) + "]/@value") == "") checkedvar = " checked='checked' "; // no value so default to checked

                strOut += "   <li><input type='checkbox' " + checkedvar + " name='" + id + "$" + lp + "' id='" + id + "_" + lp + "' value='" + tItem.Key + "'/><label for='" + id + "_" +  lp + "'>" + tItem.Value + "</label></li>";
                lp += 1;
            }
            strOut += "</ul>";

            return new RawString(strOut);
        }


        /// <summary>
        /// Get property values
        /// </summary>
        /// <param name="productdata">productdata class</param>
        /// <param name="propertytype">type of property using propertygroup ref  (e.g. "man" = manufacturer)</param>
        /// <param name="fieldname">field name of data to return</param>
        /// <param name="propertyref">property ref to return</param>
        /// <returns></returns>
        public IEncodedString PropertyValue(ProductData productdata, String propertytype, String fieldname, String propertyref)
        {
            var strOut = "";
            try
            {
                var l = productdata.GetCategories(propertytype);
                foreach (var i in l)
                {
                    if (i.categoryref == propertyref)
                    {
                        return PropertyValue(i, fieldname);
                    }
                }
            }
            catch (Exception ex)
            {
                strOut = ex.ToString();
            }

            return new RawString(strOut);
        }
        /// <summary>
        /// Get property values
        /// </summary>
        /// <param name="productdata">productdata class</param>
        /// <param name="propertytype">type of property using propertygroup ref  (e.g. "man" = manufacturer)</param>
        /// <param name="fieldname">field name of data to return</param>
        /// <param name="index">zero based index of the property record to return</param>
        /// <returns></returns>
        public IEncodedString PropertyValue(ProductData productdata,String propertytype,String fieldname, int index = 0)
        {
            var strOut = "";
            try
            {
                var l = productdata.GetCategories(propertytype);
                if (l.Count > index)
                {                    
                var objCInfo = l[index];
                    return PropertyValue(objCInfo, fieldname);
                }
            }
            catch (Exception ex)
            {
                strOut = ex.ToString();
            }

            return new RawString(strOut);
        }

        public IEncodedString PropertyUrl(ProductData productdata, String propertytype, String propertyref, int tabRedirect = -1)
        {
            var strOut = "";
            try
            {
                var objGCC = new GrpCatController(productdata.Info.Lang);
                var l = productdata.GetProperties(propertytype);
                foreach (var i in l)
                {
                    if (i.categoryref == propertyref)
                    {
                        if (tabRedirect == -1) tabRedirect = PortalSettings.Current.ActiveTab.TabID;
                        return new RawString(objGCC.GetCategoryUrl(i, tabRedirect));
                    }
                }
            }
            catch (Exception ex)
            {
                strOut = ex.ToString();
            }
            return new RawString(strOut);
        }

        public IEncodedString PropertyUrl(ProductData productdata, String propertytype, int index = 0, int tabRedirect = -1)
        {
            var strOut = "";
            try
            {
                var objGCC = new GrpCatController(productdata.Info.Lang);
                var l = productdata.GetCategories(propertytype);
                if (l.Count > index)
                {
                    if (tabRedirect == -1) tabRedirect = PortalSettings.Current.ActiveTab.TabID;
                    return new RawString(objGCC.GetCategoryUrl(l[index], tabRedirect));
                }
            }
            catch (Exception ex)
            {
                strOut = ex.ToString();
            }
            return new RawString(strOut);
        }
        
        public IEncodedString PropertyValue(GroupCategoryData groupCategopryData, String fieldname)
        {
            var strOut = "";
            try
            {

                var objCInfo = groupCategopryData;
                if (objCInfo != null)
                {
                    switch (fieldname.ToLower())
                    {
                        case "propertydesc":
                            strOut = objCInfo.categorydesc;
                            break;
                        case "message":
                            strOut = System.Web.HttpUtility.HtmlDecode(objCInfo.message);
                            break;
                        case "archived":
                            strOut = objCInfo.archived.ToString(CultureInfo.InvariantCulture);
                            break;
                        case "breadcrumb":
                            strOut = objCInfo.breadcrumb;
                            break;
                        case "propertyid":
                            strOut = objCInfo.categoryid.ToString("");
                            break;
                        case "propertyname":
                            strOut = objCInfo.categoryname;
                            break;
                        case "propertyref":
                            strOut = objCInfo.categoryref;
                            break;
                        case "depth":
                            strOut = objCInfo.depth.ToString("");
                            break;
                        case "disabled":
                            strOut = objCInfo.disabled.ToString(CultureInfo.InvariantCulture);
                            break;
                        case "entrycount":
                            strOut = objCInfo.entrycount.ToString("");
                            break;
                        case "grouptyperef":
                            strOut = objCInfo.grouptyperef;
                            break;
                        case "imageurl":
                            strOut = objCInfo.imageurl;
                            break;
                        case "ishidden":
                            strOut = objCInfo.ishidden.ToString(CultureInfo.InvariantCulture);
                            break;
                        case "isvisible":
                            strOut = objCInfo.isvisible.ToString(CultureInfo.InvariantCulture);
                            break;
                        case "metadescription":
                            strOut = objCInfo.metadescription;
                            break;
                        case "metakeywords":
                            strOut = objCInfo.metakeywords;
                            break;
                        case "parentcatid":
                            strOut = objCInfo.parentcatid.ToString("");
                            break;
                        case "recordsortorder":
                            strOut = objCInfo.recordsortorder.ToString("");
                            break;
                        case "seoname":
                            strOut = objCInfo.seoname;
                            if (strOut == "") strOut = objCInfo.categoryname;
                            break;
                        case "seopagetitle":
                            strOut = objCInfo.seopagetitle;
                            break;
                        case "url":
                            strOut = objCInfo.url;
                            break;
                    }
                }
            }
            catch
                (Exception ex)
            {
                strOut = ex.ToString();
            }

            return new RawString(strOut);
        }

        public IEncodedString CatListBox(NBrightInfo info, String xpath, String attributes = "")
        {
            return new RawString("");
        }

        public IEncodedString PropertyDropDownList(NBrightInfo info, String xpath, String attributes = "", Boolean allowEmpty = true, Boolean showHidden = false, Boolean showArchived = false, int parentid = 0, String catreflist = "", String prefix = "", bool displayCount = false, bool showEmpty = true, string groupref = "", string breadcrumbseparator = ">", string lang = "")
        {
            var rtnList = NBrightBuyUtils.BuildPropertyList(10, showHidden, showArchived, parentid, catreflist, prefix, displayCount, showEmpty, groupref, breadcrumbseparator, lang);

            if (attributes.StartsWith("ResourceKey:")) attributes = ResourceKey(attributes.Replace("ResourceKey:", "")).ToString();

            var strOut = "";

            var upd = getUpdateAttr(xpath, attributes);
            var id = getIdFromXpath(xpath);
            strOut = "<select id='" + id + "' " + upd + " " + attributes + ">";
            var s = "";
            if (allowEmpty) strOut += "    <option value=''></option>";
            foreach (var tItem in rtnList)
            {
                if (info.GetXmlProperty(xpath) == tItem.Key.ToString())
                    s = "selected";
                else
                    s = "";
                strOut += "    <option value='" + tItem.Key.ToString() + "' " + s + ">" + tItem.Value + "</option>";
            }
            strOut += "</select>";

            return new RawString(strOut);
        }

        public IEncodedString CategoryTreeMenu(int parentCatId,String razorTemplate, String controlPath, String theme,int currentCatId, String lang,int tabid, string identClass = "nbrightbuy_catmenu", string styleClass = "", string activeClass = "active", int displaylevels = 20)
        {
            var catBuiler = new CatMenuRazorBuilder(razorTemplate,controlPath,theme,currentCatId,lang);
            var strOut = catBuiler.GetTreeCatList(displaylevels, parentCatId, tabid,identClass,styleClass,activeClass);
            return new RawString(strOut);
        }

        public IEncodedString CategoryDrillDown(int parentCatId, string razorTemplate, string controlPath, string theme, int currentCatId, string lang, int tabid, string itemClass = "drilldownitem")
        {
            var catBuiler = new CatMenuRazorBuilder(razorTemplate, controlPath, theme, currentCatId, lang);
            var strOut = catBuiler.GetDrillDownMenu(parentCatId, tabid, itemClass);
            return new RawString(strOut);
        }

        #endregion

        #region "Functional"

        public IEncodedString EntryUrl(NBrightInfo info, NBrightRazor model, Boolean relative = true, String categoryref = "")
        {
            categoryref = ""; // legacy
            var url = "";
            try
            {
                var navigationdata = new NavigationData(PortalSettings.Current.PortalId, model.GetSetting("modref"));

                var urlname = info.GetXmlProperty("genxml/lang/genxml/textbox/txtseoname");
                if (urlname == "") urlname = info.GetXmlProperty("genxml/lang/genxml/textbox/txtproductname");

                    // see if we've injected a categoryid into the data class, this is done in the case of the categorymenu when displaying products.
                var categoryid = info.GetXmlProperty("genxml/categoryid");
                if (categoryid == "") categoryid = navigationdata.CategoryId.ToString();
                if (categoryid == "0") categoryid = ""; // no category active if zero

                var eid = info.ItemID.ToString()!="0" ? info.ItemID.ToString() : info.GetXmlProperty("genxml/productid");

                url = NBrightBuyUtils.GetEntryUrl(PortalSettings.Current.PortalId, eid, model.GetSetting("detailmodulekey"), urlname, model.GetSetting("ddldetailtabid"), categoryid, categoryref);
                if (relative) url = Utils.GetRelativeUrl(url);

            }
            catch (Exception ex)
            {
                url = ex.ToString();
            }

            return new RawString(url);
        }

        public IEncodedString EntryReturnUrl(NBrightRazor model)
        {
            var product = (ProductData)model.List.First();
            var entryid = product.Info.ItemID;
            var url = "";

            var param = new List<String>();
            if (model.GetUrlParam("page") != "") param.Add("PageIndex=" + model.GetUrlParam("page").Trim());
            if (model.GetUrlParam("catid") != "") param.Add("catid=" + model.GetUrlParam("catid").Trim());
            var listtab = model.GetUrlParam("rtntab");
            if (listtab == "" && model.Settings.ContainsKey("productlisttab")) listtab = model.Settings["productlisttab"].Trim();
            if (listtab == "") listtab = model.GetUrlParam("tabid").Trim();
            var intlisttab = 0;
            if (Utils.IsNumeric(listtab)) intlisttab = Convert.ToInt32(listtab);

            var paramlist = new string[param.Count];
            for (int lp = 0; lp < param.Count; lp++)
            {
                paramlist[lp] = param[lp];
            }

            url = Globals.NavigateURL(intlisttab, "", paramlist);
            return new RawString(url);
        }

        public IEncodedString CurrencyOf(Double x)
        {
            var strOut = NBrightBuyUtils.FormatToStoreCurrency(x);
            return new RawString(strOut);
        }

        public IEncodedString DateOf(String rawDate, String dateFormat = "dd MMM yyyy HH:mm")
        {
            var strOut = rawDate;
            if (NBrightCore.common.Utils.IsDate(rawDate))
            {
                strOut = Convert.ToDateTime(rawDate).ToString(dateFormat);
            }

            return new RawString(strOut);
        }

        public IEncodedString LangFlag(string lang)
        {
            return LangFlag(lang,16);
        }

        public IEncodedString LangFlag(string lang, int size)
        {
            var strOut = "<img src = '/DesktopModules/NBright/NBrightBuy/Themes/config/img/flags/" + size + "/" + lang + ".png' />"; ;
            return new RawString(strOut);
        }
        
        public IEncodedString WebsiteUrl()
        {
            var strOut = "";
            var strAry = PortalSettings.Current.DefaultPortalAlias.Split('/');
            if (strAry.Any())
                strOut = strAry[0]; // Only display base domain, without lanaguge
            else
                strOut = PortalSettings.Current.DefaultPortalAlias;
            return new RawString(strOut);
        }

        /// <summary>
        /// Display a Sort order selection on the product list
        /// 
        /// @SortOrderDropDownList("ResourceKey:ProductView.orderby", Model, " class='sortorderdropdown'")
        /// 
        /// </summary>
        /// <param name="datatext"></param>
        /// <param name="model"></param>
        /// <param name="attributes"></param>
        /// <returns></returns>
        public IEncodedString SortOrderDropDownList(String datatext, NBrightRazor model, String cssclass = "", bool addId = false, bool addScriptPostbackOnChange = true)
        {
            if (datatext.StartsWith("ResourceKey:")) datatext = ResourceKey(datatext.Replace("ResourceKey:", "")).ToString();

            var navigationdata = new NavigationData(PortalSettings.Current.PortalId, model.ModuleRef);

            var strOut = "";
            var datat = datatext.Split(',');

            var id = addId ? $"id='sortorderdropdown{model.ModuleRef}'" : "";
            strOut = $"<select {id} class='sortorderdropdown{model.ModuleRef} {cssclass}'>";
            var c = 0;
            var s = "";
            foreach (var t in datat)
            {
                var url = "";
                var param = new List<String>();
                if (model.GetUrlParam("pagemid") != "") param.Add("pagemid=" + model.ModuleId.ToString("D"));
                if (model.GetUrlParam("catid") != "") param.Add("catid=" + model.GetUrlParam("catid").Trim());
                param.Add("orderby=" + c.ToString("D"));
                var paramlist = new string[param.Count];
                for (int lp = 0; lp < param.Count; lp++)
                {
                    paramlist[lp] = param[lp];
                }

                url = Globals.NavigateURL(PortalSettings.Current.ActiveTab.TabID, "", paramlist);

                s = "";
                if (c.ToString("D") == navigationdata.OrderByIdx) s = "selected";

                strOut += $"    <option value='{c.ToString("D")}' {s} selectedurl='{url}'>{t}</option>";
                c += 1;

            }
            strOut += "</select>";

            if (addScriptPostbackOnChange)
            {
                var onchangeScript = "function () { window.location.replace($('option:selected', this).attr('selectedurl')); $('#loader').show(); }";
                strOut += $"<script>$('.sortorderdropdown{model.ModuleRef}').change({onchangeScript});</script>";
            }
            return new RawString(strOut);
        }

        public IEncodedString SortOrderAjaxDropDownList(String datatext, NBrightRazor model, string attributes = "")
        {
            var navigationdata = new NavigationData(PortalSettings.Current.PortalId, model.ModuleRef);

            if (Utils.IsNumeric(model.GetUrlParam("catid").Trim()))
            {
                navigationdata.CategoryId = Convert.ToInt32(model.GetUrlParam("catid").Trim());
                navigationdata.PageModuleId = model.ModuleId.ToString("D");
            }
            var strOut = "";
            var datat = datatext.Split(',');

            strOut = "<select " + attributes + ">";
            var c = 0;
            var s = "";
            foreach (var t in datat)
            {
                s = "";
                if (c.ToString("D") == navigationdata.OrderByIdx) s = "selected";
                strOut += $"    <option value='{c.ToString("D")}' {s} selectedurl='javascript:void(0)'>{t}</option>";
                c += 1;

            }
            strOut += "</select>";

            return new RawString(strOut);
        }

        /// <summary>
        /// Display a page size option on the product list module.
        /// </summary>
        /// <param name="datatext"></param>
        /// <param name="model"></param>
        /// <param name="attributes"></param>
        /// <param name="cssclass"></param>
        /// <param name="addId"></param>
        /// <param name="addScriptPostbackOnChange">Inidaction to add javascript to initiate postback when the dropdown changes</param>
        /// <returns></returns>
        public IEncodedString PageSizeDropDownList(String datatext, NBrightRazor model, String cssclass = "", bool addId = false, bool addScriptPostbackOnChange = true)
        {
            if (datatext.StartsWith("ResourceKey:")) datatext = ResourceKey(datatext.Replace("ResourceKey:", "")).ToString();

            var navigationdata = new NavigationData(PortalSettings.Current.PortalId, model.ModuleRef);

            if (navigationdata.PageSize == "") navigationdata.PageSize = model.GetSetting("pagesize");

            var strOut = "";
            var datat = datatext.Split(',');
            var id = addId ? $"id='pagesizedropdown{model.ModuleRef}'" : "";
            strOut = $"<select {id} class='pagesizedropdown{model.ModuleRef} {cssclass} '>";
            var c = 0;
            var s = "";
            foreach (var t in datat)
            {
                var url = "";
                var param = new List<String>();
                if (model.GetUrlParam("pagemid") != "") param.Add("pagemid=" + model.ModuleId.ToString("D"));
                if (model.GetUrlParam("catid") != "") param.Add("catid=" + model.GetUrlParam("catid").Trim());
                param.Add("pagesize=" + t);
                var paramlist = new string[param.Count];
                for (int lp = 0; lp < param.Count; lp++)
                {
                    paramlist[lp] = param[lp];
                }

                url = Globals.NavigateURL(PortalSettings.Current.ActiveTab.TabID, "", paramlist);

                s = "";
                if (t == navigationdata.PageSize) s = "selected";

                strOut += $"    <option value='{t}' {s} selectedurl='{url}' >{t}</option>";
                c += 1;
            }
            strOut += "</select>";

            if (addScriptPostbackOnChange)
            {
                var onchangeScript =
                    "function () { window.location.replace($('option:selected', this).attr('selectedurl'));  $('#loader').show(); }";
                strOut += $"<script>$('.pagesizedropdown{model.ModuleRef}').change({onchangeScript});</script>";
            }

            return new RawString(strOut);
        }

        public IEncodedString RenderRazorInjectTemplate(NBrightInfo info, String razorTemplate, String controlPath = "/DesktopModules/NBright/NBrightBuy", String themeFolder = "config", String lang = "")
        {
            if (lang == "") lang = Utils.GetCurrentCulture();
            var strOut = NBrightBuyUtils.RazorTemplRender(razorTemplate, 0, "", info, controlPath, themeFolder, lang, StoreSettings.Current.Settings());

            return new RawString(strOut);
        }


        #endregion

        #region "Checkout"

        public IEncodedString CountrySelectList(NBrightInfo info, String xpath, String attributes = "", Boolean allowEmpty = true)
        {
            var rtnList = NBrightBuyUtils.GetCountryList();

            if (attributes.StartsWith("ResourceKey:")) attributes = ResourceKey(attributes.Replace("ResourceKey:", "")).ToString();

            var strOut = "";

            var upd = getUpdateAttr(xpath, attributes);
            var id = getIdFromXpath(xpath);
            strOut = "<select id='" + id + "' " + upd + " " + attributes + ">";
            var s = "";
            if (allowEmpty) strOut += "    <option value=''></option>";
            foreach (var tItem in rtnList)
            {
                if (info.GetXmlProperty(xpath) == tItem.Key.ToString())
                    s = "selected";
                else
                    s = "";
                strOut += "    <option value='" + tItem.Key.ToString() + "' " + s + ">" + tItem.Value + "</option>";
            }
            strOut += "</select>";

            return new RawString(strOut);
        }

        public IEncodedString RegionSelect(NBrightInfo info, String xpath,String countryCode, String attributes = "", Boolean allowEmpty = true)
        {
            if (countryCode == "")
            {
                // get the countrycode if already updated
                countryCode = info.GetXmlProperty(xpath.Replace("region", "country"));
            }

            var rtnList = NBrightBuyUtils.GetRegionList(countryCode);

            if (rtnList != null && rtnList.Count > 0)
            {
                // we have a list, so do a dropdownlist
                if (attributes.StartsWith("ResourceKey:")) attributes = ResourceKey(attributes.Replace("ResourceKey:", "")).ToString();

                var strOut = "";

                var upd = getUpdateAttr(xpath, attributes);
                var id = getIdFromXpath(xpath);
                strOut = "<select id='" + id + "' " + upd + " " + attributes + ">";
                var s = "";
                if (allowEmpty) strOut += "    <option value=''></option>";
                foreach (var tItem in rtnList)
                {
                    if (info.GetXmlProperty(xpath) == tItem.Key.ToString())
                        s = "selected";
                    else
                        s = "";
                    strOut += "    <option value='" + tItem.Key.ToString() + "' " + s + ">" + tItem.Value + "</option>";
                }
                strOut += "</select>";
                return new RawString(strOut);
            }
            else
            {
                // no list so output textbox
                xpath = xpath.Replace("dropdownlist", "textbox");
                return NBrightTextBox(info,xpath,attributes,"");
            }


        }

        public IEncodedString AddressSelectList(NBrightInfo info, String xpath, String formselector, String datafields, String attributes = "", Boolean allowEmpty = true)
        {
            var usrid = info.UserId;
            if (usrid <= 0)
            {
                var usr = UserController.Instance.GetCurrentUserInfo();
                usrid = usr.UserID;
            }
            var addressData = new AddressData(usrid.ToString(""));

            var rtnList = addressData.GetAddressList();

            if (attributes.StartsWith("ResourceKey:")) attributes = ResourceKey(attributes.Replace("ResourceKey:", "")).ToString();

            var fieldList = datafields.Split(',');

            var strOut = "";
            var upd = getUpdateAttr(xpath, attributes);
            var id = getIdFromXpath(xpath);
            strOut = "<select id='" + id + "' " + upd + " " + attributes + " formselector='" + formselector + "' >";
            var s = "";
            if (allowEmpty) strOut += "    <option value=''></option>";
            foreach (var tItem in rtnList)
            {
                var fields = tItem.ToDictionary();
                var datavalues = "";
                foreach (var xp in fieldList)
                {
                    if (xp != "" && fields.ContainsKey(xp))
                    {
                        datavalues += "," + fields[xp].Replace(",", " ");
                    }
                    else
                    {
                        datavalues += ",";
                    }
                }

                var addrname = "";
                if (tItem.GetXmlProperty("genxml/textbox/addressname") != "")
                {
                    addrname = tItem.GetXmlProperty("genxml/textbox/addressname") + ": ";
                }
                var itemtext = addrname + tItem.GetXmlProperty("genxml/textbox/firstname") + "," + tItem.GetXmlProperty("genxml/textbox/lastname") + "," + tItem.GetXmlProperty("genxml/textbox/unit") + "," + tItem.GetXmlProperty("genxml/textbox/street") + "," + tItem.GetXmlProperty("genxml/textbox/city");
                var idx = tItem.GetXmlProperty("genxml/hidden/index");
                if (idx != "")
                {
                    if (info.GetXmlProperty(xpath) == idx)
                        s = "selected";
                    else
                        s = "";
                    strOut += "    <option value='" + idx + "' " + s + " datafields = '" + datafields + "' datavalues = '" + datavalues.TrimStart(',') + "' > " + itemtext.TrimStart(',') + "</option>";
                }
            }
            strOut += "</select>";

            return new RawString(strOut);
        }

        public IEncodedString ShippingProviderList(NBrightInfo info, String attributes = "")
        {
            var xpath = "genxml/extrainfo/genxml/radiobuttonlist/shippingprovider";

            var strOut = "";
            var upd = getUpdateAttr(xpath, attributes);
            var id = getIdFromXpath(xpath);
            strOut = "<div " + attributes + ">";
            var c = 0;
            var s = "";
            var value = info.GetXmlProperty(xpath);

            var cartData = new CartData(PortalSettings.Current.PortalId);

            var pluginData = new PluginData(PortalSettings.Current.PortalId);
            var provList = pluginData.GetShippingProviders();
            foreach (var d in provList)
            {
                var isValid = true;
                var shipprov = ShippingInterface.Instance(d.Key);
                if (shipprov != null)
                {
                    shipprov.Shippingkey = d.Key;
                    isValid = shipprov.IsValid(cartData.PurchaseInfo);
                    var p = d.Value;
                    if (isValid)
                    {
                        if (value == "") value = d.Key;
                        s = "";
                        if (value == d.Key) s = "checked";
                        strOut += "    <input id='" + id + "_" + c.ToString("") + "' " + upd + " name='" + id + "radio' type='radio' value='" + d.Key + "'  " + s + "/><label>" + shipprov.Name() + "</label>";
                        c += 1;
                    }
                }
            }
            strOut += "</div>";
            return new RawString(strOut);
        }

        public IEncodedString PaymentProviderList(NBrightInfo info, String attributes = "")
        {
            var strOut = "";
            var cartData = new CartData(PortalSettings.Current.PortalId);
            var pluginData = new PluginData(PortalSettings.Current.PortalId);
            var provList = pluginData.GetPaymentProviders();

            foreach (var d in provList)
            {
                var p = d.Value;
                var key = p.GetXmlProperty("genxml/textbox/ctrl");
                var prov = PaymentsInterface.Instance(key);
                if (prov != null)
                {
                    var templ = prov.GetTemplate(cartData.PurchaseInfo);
                    if (templ == "")
                    {
                        var msgcode = "noproviderdata_" + NotifyCode.warning.ToString();
                        templ = "<div>" + key + "</div>";
                        templ += NBrightBuyUtils.GetResxMessage(msgcode);
                    }
                    strOut += templ;
                }
            }

            return new RawString(strOut);
        }


        #endregion

        #region "OrderAdmin"


        public IEncodedString OrderStatusDropDownList(String id, String selectedOrderStatus = "", String cssclass = "",bool allowBlank = false)
        {

            var strOut = "";
            var Resxpath = StoreSettings.NBrightBuyPath() + "/App_LocalResources/General.ascx.resx";
            var orderstatuscode = DnnUtils.GetLocalizedString("orderstatus.Code", Resxpath, Utils.GetCurrentCulture());
            var orderstatustext = DnnUtils.GetLocalizedString("orderstatus.Text", Resxpath, Utils.GetCurrentCulture());
            if (orderstatuscode != null && orderstatustext != null)
            {
                if (allowBlank)
                {
                    orderstatuscode = "," + orderstatuscode;
                    orderstatustext = "," + orderstatustext;
                }

                var aryCode = orderstatuscode.Split(',');
                var aryText = orderstatustext.Split(',');

                strOut = "<select id='" + id + "' update='save' class='orderstatusdropdown " + cssclass + " '>";
                var c = 0;
                var s = "";
                foreach (var t in aryCode)
                {
                    var url = "";

                    s = "";
                    if (selectedOrderStatus == t) s = "selected";

                    strOut += "    <option value='" + t + "' " + s + ">" + aryText[c] + "</option>";
                    c += 1;

                }
                strOut += "</select>";
            }
            return new RawString(strOut);
        }


        #endregion

        #endregion

    }


}
