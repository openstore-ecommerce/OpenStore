using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Xsl;
using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using NBrightCore;
using NBrightCore.TemplateEngine;
using NBrightCore.common;
using NBrightCore.images;
using NBrightCore.providers;
using NBrightCore.render;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Admin;
using Nevoweb.DNN.NBrightBuy.Components;
using Nevoweb.DNN.NBrightBuy.Components.Interfaces;
using Image = System.Web.UI.WebControls.Image;

namespace Nevoweb.DNN.NBrightBuy.render
{
    public class GenXmlTemplateExt : GenXProvider
    {

        private string _rootname = "genxml";
        private string _databindColumn = "XMLData";
        private Dictionary<string, string> _settings = null;
        private ConcurrentStack<Boolean> visibleStatus;

        #region "Override methods"

        // This section overrides the interface methods for the GenX provider.
        // It allows providers to create controls/Literals in the NBright template system.

        public override bool CreateGenControl(string ctrltype, Control container, XmlNode xmlNod, string rootname = "genxml", string databindColum = "XMLData", string cultureCode = "", Dictionary<string, string> settings = null, ConcurrentStack<Boolean> visibleStatusIn = null)
        {
            visibleStatus = visibleStatusIn;

            //remove namespace of token.
            // If the NBrigthCore template system is being used across mutliple modules in the portal (that use a provider interface for tokens),
            // then a namespace should be added to the front of the type attribute, this stops clashes in the tokening system. NOTE: the "tokennamespace" tag is now supported as well
            if (ctrltype.StartsWith("nbs:")) ctrltype = ctrltype.Substring(4);

            _rootname = rootname;
            _databindColumn = databindColum;
            _settings = settings;
            switch (ctrltype)
            {
                case "addqty":
                    CreateQtyField(container, xmlNod);
                    return true;
                case "modelqty":
                    CreateModelQtyField(container, xmlNod);
                    return true;
                case "relatedproducts":
                    CreateRelatedlist(container, xmlNod);
                    return true;
                case "productcount":
                    CreateProductCount(container, xmlNod);
                    return true;
                case "productdoclink":
                    CreateProductDocLink(container, xmlNod);
                    return true;
                case "productdocdesc":
                    CreateProductDocDesc(container, xmlNod);
                    return true;
                case "productimgdesc":
                    CreateProductImageDesc(container, xmlNod);
                    return true;
                case "productdocfilename":
                    CreateProductDocFileName(container, xmlNod);
                    return true;
                case "productdoctitle":
                    CreateProductDocTitle(container, xmlNod);
                    return true;
                case "productoptionname":
                    CreateProductOptionName(container, xmlNod);
                    return true;
                case "productoption":
                    Createproductoptions(container, xmlNod);
                    return true;
                case "productlist":
                    Createproductlist(container, xmlNod);
                    return true;
                case "modelslist":
                    Createmodelslist(container, xmlNod);
                    return true;
                case "orderitemlist":
                    CreateOrderItemlist(container, xmlNod);
                    return true;
                case "orderauditlist":
                    CreateOrderAuditlist(container, xmlNod);
                    return true;
                case "cartitemlist":
                    CreateCartItemlist(container, xmlNod);
                    return true;
                case "modelsradio":
                    Createmodelsradio(container, xmlNod);
                    return true;
                case "modelsdropdown":
                    Createmodelsdropdown(container, xmlNod);
                    return true;
                case "modeldefault":
                    Createmodeldefault(container, xmlNod);
                    return true;
                case "productname":
                    CreateProductName(container, xmlNod);
                    return true;
                case "manufacturer":
                    CreateManufacturer(container, xmlNod);
                    return true;
                case "summary":
                    CreateSummary(container, xmlNod);
                    return true;
                case "seoname":
                    CreateSEOname(container, xmlNod);
                    return true;
                case "seopagetitle":
                    CreateSEOpagetitle(container, xmlNod);
                    return true;
                case "tagwords":
                    CreateTagwords(container, xmlNod);
                    return true;
                case "description":
                    CreateDescription(container, xmlNod);
                    return true;
                case "currencyisocode":
                    CreateCurrencyIsoCode(container, xmlNod);
                    return true;
                case "price":
                    CreateFromPrice(container, xmlNod);
                    return true;
                case "saleprice":
                    CreateSalePrice(container, xmlNod);
                    return true;
                case "dealerprice":
                    CreateDealerPrice(container, xmlNod);
                    return true;
                case "bestprice":
                    CreateBestPrice(container, xmlNod);
                    return true;
                case "quantity":
                    CreateQuantity(container, xmlNod);
                    return true;
                case "thumbnail":
                    CreateThumbNailer(container, xmlNod);
                    return true;
                case "editlink":
                    CreateEditLink(container, xmlNod);
                    return true;
                case "editurl":
                    CreateEditUrl(container, xmlNod);
                    return true;
                case "entrylink":
                    CreateEntryLink(container, xmlNod);
                    return true;
                case "entryurl":
                    CreateEntryUrl(container, xmlNod);
                    return true;                    
                case "returnlink":
                    CreateReturnLink(container, xmlNod);
                    return true;
                case "currenturl":
                    CreateCurrentUrl(container, xmlNod);
                    return true;
                case "hrefpagelink":
                    Createhrefpagelink(container, xmlNod);
                    return true;                    
                case "catdropdown":
                    CreateCatDropDownList(container, xmlNod);
                    return true;
                case "propertydropdown":
                    CreatePropertyDropDownList(container, xmlNod);
                    return true;
                case "categoryparentname":
                    CreateCategoryParentName(container, xmlNod);
                    return true;
                case "catlistbox":
                    CreateCatListBox(container, xmlNod);
                    return true;
                case "grouplistbox":
                    CreateGroupListBox(container, xmlNod);
                    return true;
                case "catcheckboxlist":
                    CreateCatCheckBoxList(container, xmlNod);
                    return true;
                case "countrydropdown":
                    CreateCountryDropDownList(container, xmlNod);
                    return true;
                case "countrylist":
                    CreateCountryList(container, xmlNod);
                    return true;
                case "regioncontrol":
                    CreateRegionControl(container, xmlNod);
                    return true;
                case "addressdropdown":
                    CreateAddressDropDownList(container, xmlNod);
                    return true;
                case "catbreadcrumb":
                    CreateCatBreadCrumb(container, xmlNod);
                    return true;
                case "catshortcrumb":
                    CreateCatBreadCrumb(container, xmlNod);
                    return true;
                case "catdefaultname":
                    CreateCatDefaultName(container, xmlNod);
                    return true;
                case "catdefault":
                    CreateCatDefault(container, xmlNod);
                    return true;
                case "catvalueof":
                    CreateCatValueOf(container, xmlNod);
                    return true;
                case "catbreakof":
                    CreateCatBreakOf(container, xmlNod);
                    return true;
                case "cathtmlof":
                    CreateCatHtmlOf(container, xmlNod);
                    return true;
                case "cartqtytextbox":
                    CreateCartQtyTextbox(container, xmlNod);
                    return true;
                case "orderstatus":
                    Createorderstatusdropdown(container, xmlNod);
                    return true;
                case "orderstatusdisplay":
                    Createorderstatusdisplay(container, xmlNod);
                    return true;
                case "modelstatus":
                    Createmodelstatusdropdown(container, xmlNod);
                    return true;
                case "cartemailaddress":
                    CreateCartEmailAddress(container, xmlNod);
                    return true;
                case "groupdropdown":
                    Creategroupdropdown(container, xmlNod);
                    return true;
                case "culturecodedropdown":
                    Createculturecodedropdown(container, xmlNod);
                    return true;
                case "selectlocalebutton":
                    CreateSelectLangaugeButton(container, xmlNod);
                    return true;                    
                case "editculture":
                    CreateCurrentEditCulture(container, xmlNod);
                    return true;
                case "editflag":
                    CreateEditFlag(container, xmlNod);
                    return true;                    
                case "imageof":
                    CreateImage(container, xmlNod);
                    return true;
                case "concatenate":
                    CreateConcatenate(container, xmlNod);
                    return true;    
                case "shippingproviders":
                    CreateShippingProviderRadio(container, xmlNod);
                    return true;
                case "taxdropdown":
                    CreateTaxDropDownList(container, xmlNod);
                    return true;
                case "currencyof":
                    CreateCurrencyOf(container, xmlNod);
                    return true;
                case "xslinject":
                    CreateXslInject(container, xmlNod);
                    return true;
                case "xchartorderrevenue":
                    CreateXchartOrderRev(container, xmlNod);
                    return true;
                case "friendlyurl":
                    GetFriendlyUrl(container, xmlNod);
                    return true;
                case "labelof":
                    CreateLabelOf(container, xmlNod);
                    return true;
                case "cultureselect":
                    CreateEditCultureSelect(container, xmlNod);
                    return true;
                case "currentlang":
                    CreateCurrentLang(container, xmlNod);
                    return true;
                default:
                    return false;

            }

        }

        public override string GetField(Control ctrl)
        {
            return "";
        }

        public override void SetField(Control ctrl, string newValue)
        {
        }

        public override string GetGenXml(List<Control> genCtrls, XmlDocument xmlDoc, string originalXml, string folderMapPath, string xmlRootName = "genxml")
        {
            return "";
        }

        public override string GetGenXmlTextBox(List<Control> genCtrls, XmlDocument xmlDoc, string originalXml, string folderMapPath, string xmlRootName = "genxml")
        {
            return "";
        }

        public override object PopulateGenObject(List<Control> genCtrls, object obj)
        {
            return null;
        }

        public override void CtrlItemCommand(object source, RepeaterCommandEventArgs e)
        {
        }

        #endregion

        #region "nbs:testof functions"

        public override TestOfData TestOfDataBinding(object sender, EventArgs e)
        {
            var rtnData = new TestOfData();
            rtnData.DataValue = null;
            rtnData.TestValue = null;
            var lc = (Literal)sender;
            var container = (IDataItemContainer)lc.NamingContainer;
            try
            {
                NBrightInfo info;
                                
                ProductData prodData;
                CatProdXref xrefData;
 
                var xmlDoc = new XmlDocument();
                CartData currentcart;

                xmlDoc.LoadXml("<root>" + lc.Text + "</root>");
                var xmlNod = xmlDoc.SelectSingleNode("root/tag");

                if (container.DataItem != null && xmlNod != null && (xmlNod.Attributes != null && xmlNod.Attributes["function"] != null))
                {
                    rtnData.DataValue = "FALSE";

                    XmlNode nod;
                    var testValue = "";
                    if ((xmlNod.Attributes["testvalue"] != null)) testValue = xmlNod.Attributes["testvalue"].Value;

                    // check for setting key
                    var settingkey = "";
                    if ((xmlNod.Attributes["key"] != null)) settingkey = xmlNod.Attributes["key"].Value;

                    var role = "";
                    if ((xmlNod.Attributes["role"] != null)) role = xmlNod.Attributes["role"].Value;

                    var index = "";
                    if ((xmlNod.Attributes["index"] != null)) index = xmlNod.Attributes["index"].Value;

                    var modulekey = "";
                    if ((xmlNod.Attributes["modulekey"] != null)) modulekey = xmlNod.Attributes["modulekey"].Value;

                    var targetmodulekey = "";
                    if ((xmlNod.Attributes["targetmodulekey"] != null)) targetmodulekey = xmlNod.Attributes["targetmodulekey"].Value;


                    // do special tests for named fucntions
                    switch (xmlNod.Attributes["function"].Value.ToLower())
                    {
                        case "searchactive":
                            var navdata2 = new NavigationData(PortalSettings.Current.PortalId, targetmodulekey);
                            if (navdata2.Criteria == "") rtnData.DataValue = "FALSE";
                            else rtnData.DataValue = "TRUE";
                            break;
                        case "productcount":
                            var navdata = new NavigationData(PortalSettings.Current.PortalId, modulekey);
                            rtnData.DataValue = navdata.RecordCount;
                            break;
                        case "price":
                            rtnData.DataValue = NBrightBuyUtils.GetFromPrice((NBrightInfo) container.DataItem);
                            break;
                        case "dealerprice":
                            rtnData.DataValue = NBrightBuyUtils.GetDealerPrice((NBrightInfo) container.DataItem);
                            break;
                        case "saleprice":
                            rtnData.DataValue = NBrightBuyUtils.GetSalePrice((NBrightInfo) container.DataItem);
                            break;
                        case "imgexists":
                            rtnData.DataValue = testValue;
                            nod = GenXmlFunctions.GetGenXmLnode(DataBinder.Eval(container.DataItem, _databindColumn).ToString(), "genxml/imgs/genxml[" + rtnData.DataValue + "]");
                            if (nod == null) rtnData.DataValue = "FALSE";
                            break;
                        case "modelexists":
                            rtnData.DataValue = testValue;
                            nod = GenXmlFunctions.GetGenXmLnode(DataBinder.Eval(container.DataItem, _databindColumn).ToString(), "genxml/models/genxml[" + rtnData.DataValue + "]");
                            if (nod == null) rtnData.DataValue = "FALSE";
                            break;
                        case "optionexists":
                            rtnData.DataValue = testValue;
                            nod = GenXmlFunctions.GetGenXmLnode(DataBinder.Eval(container.DataItem, _databindColumn).ToString(), "genxml/options/genxml[" + rtnData.DataValue + "]");
                            if (nod == null) rtnData.DataValue = "FALSE";
                            break;
                        case "isinstock":
                            if (NBrightBuyUtils.IsInStock((NBrightInfo) container.DataItem))
                            {
                                rtnData.DataValue = "TRUE";
                                rtnData.TestValue = "TRUE";
                            }
                            break;
                        case "ismodelinstock":
                            if (NBrightBuyUtils.IsModelInStock((NBrightInfo) container.DataItem))
                            {
                                rtnData.DataValue = "TRUE";
                                rtnData.TestValue = "TRUE";
                            }
                            break;
                        case "inwishlist":
                            var productid = DataBinder.Eval(container.DataItem, "ItemId").ToString();
                            if (Utils.IsNumeric(productid))
                            {
                                var listname = "ItemList";
                                if ((xmlNod.Attributes["listname"] != null)) listname = xmlNod.Attributes["listname"].Value;

                                var wl = new ItemListData(PortalSettings.Current.PortalId,UserController.Instance.GetCurrentUserInfo().UserID);
                                if (wl.IsInList(productid))
                                {
                                    rtnData.DataValue = "TRUE";
                                    rtnData.TestValue = "TRUE";
                                }
                            }
                            break;
                        case "isonsale":
                            var saleprice = NBrightBuyUtils.GetSalePriceDouble((NBrightInfo) container.DataItem);
                            if (saleprice > 0)
                            {
                                rtnData.DataValue = "TRUE";
                                rtnData.TestValue = "TRUE";
                            }
                            break;
                        case "hasrelateditems":
                            info = (NBrightInfo) container.DataItem;
                            prodData = ProductUtils.GetProductData(info.ItemID, info.Lang);
                            if (prodData.GetRelatedProducts().Count > 0)
                            {
                                rtnData.DataValue = "TRUE";
                                rtnData.TestValue = "TRUE";
                            }
                            break;
                        case "hasdocuments":
                            info = (NBrightInfo) container.DataItem;
                            prodData = ProductUtils.GetProductData(info.ItemID, info.Lang);
                            if (prodData.Docs.Count > 0)
                            {
                                rtnData.DataValue = "TRUE";
                                rtnData.TestValue = "TRUE";
                            }
                            break;
                        case "haspurchasedocuments":
                            info = (NBrightInfo) container.DataItem;
                            prodData = ProductUtils.GetProductData(info.ItemID, info.Lang);
                            if (prodData.Docs.Select(i => i.GetXmlProperty("genxml/checkbox/chkpurchase") == "True").Any())
                            {
                                rtnData.DataValue = "TRUE";
                                rtnData.TestValue = "TRUE";
                            }
                            break;
                        case "hasproperty":
                            info = (NBrightInfo) container.DataItem;
                            xrefData = new CatProdXref();
                            if (xrefData.IsProductInCategory(info.ItemID,testValue))
                            {
                                rtnData.DataValue = "TRUE";
                                rtnData.TestValue = "TRUE";
                            }
                            break;
                        case "isincategory":
                            info = (NBrightInfo) container.DataItem;
                            xrefData = new CatProdXref();
                            if (xrefData.IsProductInCategory(info.ItemID,testValue))
                            {
                                rtnData.DataValue = "TRUE";
                                rtnData.TestValue = "TRUE";
                            }
                            break;
                        case "isdocpurchasable":
                            nod = GenXmlFunctions.GetGenXmLnode(DataBinder.Eval(container.DataItem, _databindColumn).ToString(), "genxml/docs/genxml[" + index + "]/hidden/docid");
                            if (nod != null)
                            {
                                info = (NBrightInfo) container.DataItem;
                                prodData = ProductUtils.GetProductData(info.ItemID, info.Lang);
                                if (prodData.Docs.Select(i => i.GetXmlProperty("genxml/checkbox/chkpurchase") == "True" && i.GetXmlProperty("genxml/hidden/docid") == nod.InnerText).Any())
                                {
                                    rtnData.DataValue = "TRUE";
                                    rtnData.TestValue = "TRUE";
                                }
                            }
                            break;
                        case "isdocpurchased":
                            nod = GenXmlFunctions.GetGenXmLnode(DataBinder.Eval(container.DataItem, _databindColumn).ToString(), "genxml/docs/genxml[" + index + "]/hidden/docid");
                            if (nod != null && Utils.IsNumeric(nod.InnerText))
                            {
                                var uInfo = UserController.Instance.GetCurrentUserInfo();
                                //[TODO: work out method of finding if user purchased document.]
                                //if (NBrightBuyV2Utils.DocHasBeenPurchasedByDocId(uInfo.UserID, Convert.ToInt32(nod.InnerText)))
                                //{
                                //    rtnData.DataValue = "TRUE";
                                //    rtnData.TestValue = "TRUE";
                                //}
                            }
                            break;
                        case "hasmodelsoroptions":
                            nod = GenXmlFunctions.GetGenXmLnode(DataBinder.Eval(container.DataItem, _databindColumn).ToString(), "genxml/models/genxml[2]/hidden/modelid");
                            if (nod != null && nod.InnerText != "")
                            {
                                rtnData.DataValue = "TRUE";
                                rtnData.TestValue = "TRUE";
                            }
                            if (rtnData.DataValue == "FALSE")
                            {
                                nod = GenXmlFunctions.GetGenXmLnode(DataBinder.Eval(container.DataItem, _databindColumn).ToString(), "genxml/options/genxml[1]/hidden/optionid");
                                if (nod != null && nod.InnerText != "")
                                {
                                    rtnData.DataValue = "TRUE";
                                    rtnData.TestValue = "TRUE";
                                }
                            }
                            break;
                        case "isproductincart":
                            var cartData = new CartData(PortalSettings.Current.PortalId);
                            info = (NBrightInfo) container.DataItem;
                            if (cartData.GetCartItemList().Select(i => i.GetXmlProperty("genxml/productid") == info.ItemID.ToString("")).Any())
                            {
                                rtnData.DataValue = "TRUE";
                                rtnData.TestValue = "TRUE";
                            }
                            break;
                        case "settings":
                            if (_settings != null && _settings.ContainsKey(settingkey) && _settings[settingkey] == testValue)
                            {
                                rtnData.DataValue = "TRUE";
                                rtnData.TestValue = "TRUE";
                            }
                            break;
                        case "debugmode":
                            if (StoreSettings.Current.DebugMode)
                            {
                                rtnData.DataValue = "TRUE";
                                rtnData.TestValue = "TRUE";
                            }
                            break;
                        case "isinrole":
                            if (CmsProviderManager.Default.IsInRole(role))
                            {
                                rtnData.DataValue = "TRUE";
                                rtnData.TestValue = "TRUE";
                            }
                            break;
                        case "issuperuser":
                            if (UserController.Instance.GetCurrentUserInfo().IsSuperUser)
                            {
                                rtnData.DataValue = "TRUE";
                                rtnData.TestValue = "TRUE";
                            }
                            break;
                        case "isuser":
                            if (UserController.Instance.GetCurrentUserInfo().UserID >= 0)
                            {
                                rtnData.DataValue = "TRUE";
                                rtnData.TestValue = "TRUE";
                            }
                            break;
                        case "isclientordermode":
                            currentcart = new CartData(PortalSettings.Current.PortalId);
                            if (currentcart.IsClientOrderMode())
                            {
                                rtnData.DataValue = "TRUE";
                                rtnData.TestValue = "TRUE";
                            }
                            break;
                        case "carteditmode":
                            currentcart = new CartData(PortalSettings.Current.PortalId);
                            var editmode = currentcart.GetInfo().GetXmlProperty("genxml/carteditmode");
                            if (editmode == testValue)
                            {
                                rtnData.DataValue = "TRUE";
                                rtnData.TestValue = "TRUE";
                            }
                            break;
                        case "iscartempty":
                            currentcart = new CartData(PortalSettings.Current.PortalId);
                            var l = currentcart.GetCartItemList();
                            if (!l.Any())
                            {
                                rtnData.DataValue = "TRUE";
                                rtnData.TestValue = "TRUE";
                            }
                            break;
                        case "hasshippingproviders":
                            var pluginData = new PluginData(PortalSettings.Current.PortalId);
                            var provList = pluginData.GetShippingProviders();
                            if (provList.Count > 1)
                            {
                                rtnData.DataValue = "TRUE";
                                rtnData.TestValue = "TRUE";
                            }
                            break;
                        case "profile":
                            var userInfo = UserController.Instance.GetCurrentUserInfo();
                            if (userInfo.UserID >= 0) rtnData.DataValue = userInfo.Profile.GetPropertyValue(settingkey);
                            break;
                        case "static":
                            break;
                        default:
                            rtnData.DataValue = null;
                            break;
                    }
                }
            }
            catch (Exception)
            {
                lc.Text = "";
            }
            return rtnData;
        }


        #endregion

        #region "create Shortcuts"

        private void CreateProductImageDesc(Control container, XmlNode xmlNod)
        {
            if (xmlNod.Attributes != null && (xmlNod.Attributes["index"] != null))
            {
                if (Utils.IsNumeric(xmlNod.Attributes["index"].Value)) // must have a index
                {
                    var l = new Literal();
                    l.DataBinding += ShortcutDataBindingUrlDecode;
                    l.Text = xmlNod.Attributes["index"].Value;
                    l.Text = "genxml/lang/genxml/imgs/genxml[" + xmlNod.Attributes["index"].Value + "]/textbox/txtimagedesc";
                    container.Controls.Add(l);
                }
            }
        }

        private void CreateProductDocDesc(Control container, XmlNode xmlNod)
        {
            if (xmlNod.Attributes != null && (xmlNod.Attributes["index"] != null))
            {
                if (Utils.IsNumeric(xmlNod.Attributes["index"].Value)) // must have a index
                {
                    var l = new Literal();
                    l.DataBinding += ShortcutDataBindingUrlDecode;
                    l.Text = xmlNod.Attributes["index"].Value;
                    l.Text = "genxml/lang/genxml/docs/genxml[" + xmlNod.Attributes["index"].Value + "]/textbox/txtdocdesc";
                    container.Controls.Add(l);
                }
            }
        }
        private void CreateProductDocFileName(Control container, XmlNode xmlNod)
        {
            if (xmlNod.Attributes != null && (xmlNod.Attributes["index"] != null))
            {
                if (Utils.IsNumeric(xmlNod.Attributes["index"].Value)) // must have a index
                {
                    var l = new Literal();
                    l.DataBinding += ShortcutDataBindingUrlDecode;
                    l.Text = xmlNod.Attributes["index"].Value;
                    l.Text = "genxml/docs/genxml[" + xmlNod.Attributes["index"].Value + "]/textbox/txtfilename";
                    container.Controls.Add(l);
                }
            }
        }
        private void CreateProductDocTitle(Control container, XmlNode xmlNod)
        {
            if (xmlNod.Attributes != null && (xmlNod.Attributes["index"] != null))
            {
                if (Utils.IsNumeric(xmlNod.Attributes["index"].Value)) // must have a index
                {
                    var l = new Literal();
                    l.DataBinding += ShortcutDataBindingUrlDecode;
                    l.Text = xmlNod.Attributes["index"].Value;
                    l.Text = "genxml/lang/genxml/docs/genxml[" + xmlNod.Attributes["index"].Value + "]/textbox/txttitle";
                    container.Controls.Add(l);
                }
            }
        }
        private void CreateProductOptionName(Control container, XmlNode xmlNod)
        {
            if (xmlNod.Attributes != null && (xmlNod.Attributes["index"] != null))
            {
                if (Utils.IsNumeric(xmlNod.Attributes["index"].Value)) // must have a index
                {
                    var l = new Literal();
                    l.DataBinding += ShortcutDataBinding;
                    l.Text = xmlNod.Attributes["index"].Value;
                    l.Text = "genxml/lang/genxml/options/genxml[" + xmlNod.Attributes["index"].Value + "]/textbox/txtoptiondesc";
                    container.Controls.Add(l);
                }
            }
        }
        private void CreateProductName(Control container, XmlNode xmlNod)
        {
            var l = new Literal();
            l.DataBinding += ShortcutDataBinding;
            l.Text = "genxml/lang/genxml/textbox/txtproductname";
            container.Controls.Add(l);
        }
        private void CreateManufacturer(Control container, XmlNode xmlNod)
        {
            var l = new Literal();
            l.DataBinding += ShortcutDataBinding;
            l.Text = "genxml/lang/genxml/textbox/manufacturer";
            container.Controls.Add(l);
        }
        private void CreateSummary(Control container, XmlNode xmlNod)
        {
            var l = new Literal();
            l.DataBinding += ShortcutDataBinding;
            l.Text = "genxml/lang/genxml/textbox/txtsummary";
            container.Controls.Add(l);
        }
        private void CreateSEOname(Control container, XmlNode xmlNod)
        {
            var l = new Literal();
            l.DataBinding += ShortcutDataBinding;
            l.Text = "genxml/lang/genxml/textbox/txtseoname";
            container.Controls.Add(l);
        }
        private void CreateTagwords(Control container, XmlNode xmlNod)
        {
            var l = new Literal();
            l.DataBinding += ShortcutDataBinding;
            l.Text = "genxml/lang/genxml/textbox/txttagwords";
            container.Controls.Add(l);
        }
        private void CreateSEOpagetitle(Control container, XmlNode xmlNod)
        {
            var l = new Literal();
            l.DataBinding += ShortcutDataBinding;
            l.Text = "genxml/lang/genxml/textbox/txtseopagetitle";
            container.Controls.Add(l);
        }
        private void CreateDescription(Control container, XmlNode xmlNod)
        {
            var l = new Literal();
            l.DataBinding += ShortcutDataBindingHtmlDecode;
            l.Text = "genxml/lang/genxml/edt/description";
            container.Controls.Add(l);
        }
        private void CreateQuantity(Control container, XmlNode xmlNod)
        {
            var l = new Literal();
            l.DataBinding += ShortcutDataBinding;
            //l.Text = "genxml/models/genxml/textbox/txtqtyremaining";
            //Get quantity with the lowest unitcost value with xpath
            l.Text = "(genxml/models/genxml/textbox/txtqtyremaining[not(number((.)[1]) > number((../../../genxml/textbox/txtqtyremaining)[1]))][1])[1]";
            container.Controls.Add(l);
        }
        private void ShortcutDataBinding(object sender, EventArgs e)
        {
            var l = (Literal)sender;
            var container = (IDataItemContainer)l.NamingContainer;
            try
            {
                l.Visible = visibleStatus.DefaultIfEmpty(true).First();
                XmlNode nod = GenXmlFunctions.GetGenXmLnode(DataBinder.Eval(container.DataItem, _databindColumn).ToString(), l.Text);
                if ((nod != null))
                {
                    l.Text = XmlConvert.DecodeName(nod.InnerText);  
                }
                else
                {
                    l.Text = "";
                }

            }
            catch (Exception ex)
            {
                l.Text = ex.ToString();
            }
        }
        private void ShortcutDataBindingUrlDecode(object sender, EventArgs e)
        {
            var l = (Literal)sender;
            var container = (IDataItemContainer)l.NamingContainer;
            try
            {
                l.Visible = visibleStatus.DefaultIfEmpty(true).First();
                XmlNode nod = GenXmlFunctions.GetGenXmLnode(DataBinder.Eval(container.DataItem, _databindColumn).ToString(), l.Text);
                if ((nod != null))
                {
                    l.Text = System.Web.HttpUtility.UrlDecode(XmlConvert.DecodeName(nod.InnerText)); // the urldecode is included for filename on documents, which was forced to encoded in v2 so it work correctly. 
                }
                else
                {
                    l.Text = "";
                }

            }
            catch (Exception ex)
            {
                l.Text = ex.ToString();
            }
        }
        private void ShortcutDataBindingHtmlDecode(object sender, EventArgs e)
        {
            var l = (Literal)sender;
            var container = (IDataItemContainer)l.NamingContainer;
            try
            {
                l.Visible = visibleStatus.DefaultIfEmpty(true).First();
                XmlNode nod = GenXmlFunctions.GetGenXmLnode(DataBinder.Eval(container.DataItem, _databindColumn).ToString(), l.Text);
                if ((nod != null))
                {
                    l.Text = System.Web.HttpUtility.HtmlDecode(XmlConvert.DecodeName(nod.InnerText));
                }
                else
                {
                    l.Text = "";
                }

            }
            catch (Exception ex)
            {
                l.Text = ex.ToString();
            }
        }
        private void ShortcutDataBindingCurrency(object sender, EventArgs e)
        {
            var l = (Literal)sender;
            var container = (IDataItemContainer)l.NamingContainer;
            try
            {
                l.Visible = visibleStatus.DefaultIfEmpty(true).First();
                XmlNode nod = GenXmlFunctions.GetGenXmLnode(DataBinder.Eval(container.DataItem, _databindColumn).ToString(), l.Text);
                if ((nod != null))
                {
                    Double v = 0;
                    if (Utils.IsNumeric(XmlConvert.DecodeName(nod.InnerText)))
                    {
                        v = Convert.ToDouble(XmlConvert.DecodeName(nod.InnerText), CultureInfo.GetCultureInfo("en-US"));
                    }
                    l.Text = NBrightBuyUtils.FormatToStoreCurrency(v); 
                }
                else
                {
                    l.Text = "";
                }

            }
            catch (Exception ex)
            {
                l.Text = ex.ToString();
            }
        }

        #endregion

        #region "Create Thumbnailer"

        private void CreateThumbNailer(Control container, XmlNode xmlNod)
        {
            var l = new Literal();

            var thumbparams = "";
            var imagenum = "1";
            if (xmlNod.Attributes != null)
            {
                foreach (XmlAttribute a in xmlNod.Attributes)
                {
                    if (a.Name.ToLower() != "type")
                    {
                        if (a.Name.ToLower() != "image")
                            thumbparams += "&amp;" + a.Name + "=" + a.Value; // don;t use the type in the params
                        else
                            imagenum = a.Value;
                    }
                }
            }

            l.Text = imagenum + ":" + thumbparams; // pass the attributes to be added

            l.DataBinding += ThumbNailerDataBinding;
            container.Controls.Add(l);
        }

        private void ThumbNailerDataBinding(object sender, EventArgs e)
        {
            var l = (Literal)sender;
            var container = (IDataItemContainer)l.NamingContainer;
            try
            {
                l.Visible = visibleStatus.DefaultIfEmpty(true).First();
                var imagesrc = "0";
                var imageparams = l.Text.Split(':');

                XmlNode nod = GenXmlFunctions.GetGenXmLnode(DataBinder.Eval(container.DataItem, _databindColumn).ToString(), "genxml/imgs/genxml[" + imageparams[0] + "]/hidden/imageurl");
                if ((nod != null)) imagesrc = nod.InnerText;
                var url = StoreSettings.NBrightBuyPath() + "/NBrightThumb.ashx?src=" + imagesrc + imageparams[1];
                l.Text = url;
            }
            catch (Exception ex)
            {
                l.Text = ex.ToString();
            }
        }

        #endregion

        #region "create EntryLink/URL control"

        private void CreateEntryLink(Control container, XmlNode xmlNod)
        {
            var lk = new HyperLink();
            lk = (HyperLink)GenXmlFunctions.AssignByReflection(lk, xmlNod);

            if (xmlNod.Attributes != null)
            {
                if (xmlNod.Attributes["tabid"] != null) lk.Attributes.Add("tabid", xmlNod.Attributes["tabid"].InnerText);
                if (xmlNod.Attributes["modkey"] != null) lk.Attributes.Add("modkey", xmlNod.Attributes["modkey"].InnerText);
                if (xmlNod.Attributes["xpath"] != null) lk.Attributes.Add("xpath", xmlNod.Attributes["xpath"].InnerText);
                if (xmlNod.Attributes["catid"] != null) lk.Attributes.Add("catid", xmlNod.Attributes["catid"].InnerText);
                if (xmlNod.Attributes["catid"] != null) lk.Attributes.Add("catref", xmlNod.Attributes["catref"].InnerText);
            }
            lk.DataBinding += EntryLinkDataBinding;
            container.Controls.Add(lk);
        }

        private void EntryLinkDataBinding(object sender, EventArgs e)
        {
            var lk = (HyperLink)sender;
            var container = (IDataItemContainer)lk.NamingContainer;
			try
			{
				//set a default url

                lk.Visible = visibleStatus.DefaultIfEmpty(true).First();

				var entryid = Convert.ToString(DataBinder.Eval(container.DataItem, "ItemID"));

			    var urlname = "Default";
                if (lk.Attributes["xpath"] != null)
                {
                    var nod = GenXmlFunctions.GetGenXmLnode(DataBinder.Eval(container.DataItem, _databindColumn).ToString(), lk.Attributes["xpath"]);
                    if ((nod != null)) urlname = nod.InnerText;
                }
                var t = "";
				if (lk.Attributes["tabid"] != null && Utils.IsNumeric(lk.Attributes["tabid"])) t = lk.Attributes["tabid"];
                var c = "";
                if (lk.Attributes["catid"] != null && Utils.IsNumeric(lk.Attributes["catid"])) c = lk.Attributes["catid"];
                var cref = "";
                if (lk.Attributes["catref"] != null && Utils.IsNumeric(lk.Attributes["catref"])) cref = lk.Attributes["catref"];
                var moduleref = "";
                if ((lk.Attributes["modkey"] != null)) moduleref = lk.Attributes["modkey"];

                var url = NBrightBuyUtils.GetEntryUrl(PortalSettings.Current.PortalId, entryid, moduleref, urlname, t, c, cref);
                lk.NavigateUrl = url;

			}
			catch (Exception ex)
			{
				lk.Text = ex.ToString();
			}
        }

        private void CreateEntryUrl(Control container, XmlNode xmlNod)
        {
            var l = new Literal();
            if (xmlNod.Attributes != null)
            {
                // we dont; have any attributes for a literal, so pass data as string (tabid,modulekey,entryname)
                var t = PortalSettings.Current.ActiveTab.TabID.ToString("");
                var mk = "";
                var xp = "";
                var c = "";
                var cref = "";
                var relative = "";
                if (xmlNod.Attributes["tabid"] != null) t = xmlNod.Attributes["tabid"].InnerText;
                if (xmlNod.Attributes["modkey"] != null) mk = xmlNod.Attributes["modkey"].InnerText;
                if (xmlNod.Attributes["xpath"] != null) xp = xmlNod.Attributes["xpath"].InnerText;
                if (xmlNod.Attributes["catid"] != null) c = xmlNod.Attributes["catid"].InnerText;
                if (xmlNod.Attributes["catref"] != null) cref = xmlNod.Attributes["catref"].InnerText;
                if (xmlNod.Attributes["relative"] != null) relative = xmlNod.Attributes["relative"].InnerText;                

                l.Text = t + '*' + mk + '*' + xp.Replace('*', '-') + '*' + c + "*" + cref + "*" + relative;
            }
            l.DataBinding += EntryUrlDataBinding;
            container.Controls.Add(l);
        }

        private void EntryUrlDataBinding(object sender, EventArgs e)
        {
            var l = (Literal)sender;
            var container = (IDataItemContainer)l.NamingContainer;
            try
            {
                //set a default url

                l.Visible = visibleStatus.DefaultIfEmpty(true).First();

                var entryid = Convert.ToString(DataBinder.Eval(container.DataItem, "ItemID"));
                var dataIn = l.Text.Split('*'); 
                var urlname = "";
                var t = "";
                var moduleref = "";
                var c = "";
                var cref = "";
                var relative = "";

                if (dataIn.Length == 6)
                {
                    if (Utils.IsNumeric(dataIn[0])) t = dataIn[0];
                    if (Utils.IsNumeric(dataIn[1])) moduleref = dataIn[1];
                    if (Utils.IsNumeric(dataIn[3])) c = dataIn[3];
                    if (dataIn[4] != "") cref = dataIn[4];
                    if (dataIn[5] != "") relative = dataIn[5];
                    var nod = GenXmlFunctions.GetGenXmLnode(DataBinder.Eval(container.DataItem, _databindColumn).ToString(), dataIn[2]);
                    if ((nod != null)) urlname = nod.InnerText;
                    // see if we've injected a categoryid into the data class, this is done in the case of the categorymenu when displaying products.
                    nod = GenXmlFunctions.GetGenXmLnode(DataBinder.Eval(container.DataItem, _databindColumn).ToString(), "genxml/categoryid");
                    if (nod != null && c == "" && Utils.IsNumeric(nod.InnerText)) c = nod.InnerText;
                }

                var url = NBrightBuyUtils.GetEntryUrl(PortalSettings.Current.PortalId, entryid, moduleref, urlname, t, c, cref);
                if (relative.ToLower() == "true") url = Utils.GetRelativeUrl(url);

                l.Text = url;               

            }
            catch (Exception ex)
            {
                l.Text = ex.ToString();
            }
        }

        #endregion

        #region "create ReturnLink control"

        private void CreateReturnLink(Control container, XmlNode xmlNod)
        {
            var lk = new HyperLink();
            lk = (HyperLink)GenXmlFunctions.AssignByReflection(lk, xmlNod);

            if (xmlNod.Attributes != null && (xmlNod.Attributes["tabid"] != null))
            {
                lk.Attributes.Add("tabid", xmlNod.Attributes["tabid"].InnerText);
            }

            lk.DataBinding += ReturnLinkDataBinding;
            container.Controls.Add(lk);
        }

        private void ReturnLinkDataBinding(object sender, EventArgs e)
        {
            var lk = (HyperLink)sender;
            var container = (IDataItemContainer)lk.NamingContainer;
            try
            {
                lk.Visible = visibleStatus.DefaultIfEmpty(true).First();

                var t = "";
                if (lk.Attributes["tabid"] != null && Utils.IsNumeric(lk.Attributes["tabid"]))
                {
                    t = lk.Attributes["tabid"];
                }

                var url = NBrightBuyUtils.GetReturnUrl(t);
                lk.NavigateUrl = url;

            }
            catch (Exception ex)
            {
                lk.Text = ex.ToString();
            }
        }

        #endregion

        #region "create HrefPageLink control"
        private void Createhrefpagelink(Control container, XmlNode xmlNod)
        {
            var l = new Literal();
            l.Text = "-1";
            if (xmlNod.Attributes != null && (xmlNod.Attributes["moduleid"] != null))
            {
                l.Text = xmlNod.Attributes["moduleid"].InnerXml;
            }
            l.DataBinding += hrefpagelinkbind;
            container.Controls.Add(l);
        }

        private void hrefpagelinkbind(object sender, EventArgs e)
        {
            var l = (Literal)sender;
            var container = (IDataItemContainer)l.NamingContainer;
            try
            {
                l.Visible = visibleStatus.DefaultIfEmpty(true).First();
                var catparam = "";
                var pagename = PortalSettings.Current.ActiveTab.TabName + ".aspx";
                var catid = Utils.RequestParam(HttpContext.Current, "catid");
                if (Utils.IsNumeric(catid))
                {
                    pagename = NBrightBuyUtils.GetCurrentPageName(Convert.ToInt32(catid)) + ".aspx";
                    catparam = "&catid=" + catid;
                }
                var catref = Utils.RequestParam(HttpContext.Current, "catref");
                if (catref != "")
                {
                    catparam = "&catref=" + catref;
                    pagename = "";
                }
                var url = DotNetNuke.Services.Url.FriendlyUrl.FriendlyUrlProvider.Instance().FriendlyUrl(PortalSettings.Current.ActiveTab, "~/Default.aspx?tabid=" + PortalSettings.Current.ActiveTab.TabID.ToString("") + catparam + "&page=" + Convert.ToString(DataBinder.Eval(container.DataItem, "PageNumber")) + "&pagemid=" + l.Text, pagename);
                l.Text = "<a href=\"" + url + "\">" + Convert.ToString(DataBinder.Eval(container.DataItem, "Text")) + "</a>";
            }
            catch (Exception ex)
            {
                l.Text = ex.ToString();
            }
        }


        #endregion

        #region "create CurrentUrl control"
        private void CreateCurrentUrl(Control container, XmlNode xmlNod)
        {
            var l = new Literal();

            l.DataBinding += CurrentUrlDataBinding;
            container.Controls.Add(l);
        }

        private void CurrentUrlDataBinding(object sender, EventArgs e)
        {
            var l = (Literal)sender;
            var container = (IDataItemContainer)l.NamingContainer;
            try
            {
                l.Visible = visibleStatus.DefaultIfEmpty(true).First();
                //set a default url
                var url = DotNetNuke.Entities.Portals.PortalSettings.Current.ActiveTab.FullUrl;
                l.Text = url;

            }
            catch (Exception ex)
            {
                l.Text = ex.ToString();
            }
        }

        #endregion

        #region "Friendly Tab and category id url"

        private void GetFriendlyUrl(Control container, XmlNode xmlNod)
        {
            var l = new Literal();
            l.Text = "";
            if (xmlNod.Attributes != null)
            {
                if (xmlNod.Attributes["tabid"] != null && Utils.IsNumeric(xmlNod.Attributes["tabid"].InnerXml)) l.Text += xmlNod.Attributes["tabid"].InnerXml;
                    l.Text += ",";
                if (xmlNod.Attributes["catid"] != null && Utils.IsNumeric(xmlNod.Attributes["catid"].InnerXml)) l.Text += xmlNod.Attributes["catid"].InnerXml;
                    l.Text += ",";
                if (xmlNod.Attributes["catref"] != null) l.Text += xmlNod.Attributes["catref"].InnerXml;

            }
            l.DataBinding += GetFriendlyUrlDataBinding;
            container.Controls.Add(l);
        }

        private void GetFriendlyUrlDataBinding(object sender, EventArgs e)
        {
            var l = (Literal)sender;
            var container = (IDataItemContainer)l.NamingContainer;
            try
            {
                var tabid = PortalSettings.Current.ActiveTab.TabID.ToString();
                var param = new string[2];

                var a = l.Text.Split(',');
                if (a.Length == 3)
                {
                    if (Utils.IsNumeric(a[0])) tabid = a[0];
                    if (Utils.IsNumeric(a[1])) param[0] = "catid=" + a[1];
                    if (a[2] != "") param[1] = "catref=" + a[2];

                    l.Visible = visibleStatus.DefaultIfEmpty(true).First();
                    //set a default url                    
                }
                var url = Globals.NavigateURL(Convert.ToInt32(tabid),"",param);
                l.Text = url;

            }
            catch (Exception ex)
            {
                l.Text = ex.ToString();
            }
        }

        #endregion


        #region  "category dropdown and checkbox list"

        private void CreateCatCheckBoxList(Control container, XmlNode xmlNod)
        {
            try
            {

                var cbl = new CheckBoxList();
                cbl = (CheckBoxList) GenXmlFunctions.AssignByReflection(cbl, xmlNod);
                var selected = false;
                if (xmlNod.Attributes != null && (xmlNod.Attributes["selected"] != null))
                {
                    if (xmlNod.Attributes["selected"].InnerText.ToLower() == "true") selected = true;
                }

                var tList = GetCatList(xmlNod);
                foreach (var tItem in tList)
                {
                    var li = new ListItem();
                    li.Text = tItem.Value;
                    li.Value = tItem.Key.ToString("");
                    li.Selected = selected;
                    cbl.Items.Add(li);
                }

                cbl.DataBinding += CbListDataBinding;
                container.Controls.Add(cbl);
            }
            catch (Exception e)
            {
                var lc = new Literal();
                lc.Text = e.ToString();
                container.Controls.Add(lc);
            }

        }

        private void CbListDataBinding(object sender, EventArgs e)
        {
            var chk = (CheckBoxList)sender;
            var container = (IDataItemContainer)chk.NamingContainer;
            try
            {
                chk.Visible = visibleStatus.DefaultIfEmpty(true).First();
                var xmlNod = GenXmlFunctions.GetGenXmLnode(chk.ID, "checkboxlist", (string)DataBinder.Eval(container.DataItem, _databindColumn));
                var xmlNodeList = xmlNod.SelectNodes("./chk");
                if (xmlNodeList != null)
                {
                    foreach (XmlNode xmlNoda in xmlNodeList)
                    {
                        if (xmlNoda.Attributes != null)
                        {
                            if (xmlNoda.Attributes.GetNamedItem("data") != null)
                            {
                                var datavalue = xmlNoda.Attributes["data"].Value;
                                //use the data attribute if there
                                if ((chk.Items.FindByValue(datavalue).Value != null))
                                {
                                    chk.Items.FindByValue(datavalue).Selected = Convert.ToBoolean(xmlNoda.Attributes["value"].Value);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                //do nothing
            }

        }

        private void CreateTaxDropDownList(Control container, XmlNode xmlNod)
        {
            try
            {
                var ddl = new DropDownList();
                ddl = (DropDownList)GenXmlFunctions.AssignByReflection(ddl, xmlNod);
                var provkey = "";
                if (xmlNod.Attributes != null && (xmlNod.Attributes["providerkey"] != null)) provkey = xmlNod.Attributes["providerkey"].InnerText;

                var tList = Components.Interfaces.TaxInterface.Instance(provkey).GetName();
                foreach (var tItem in tList)
                {
                    var li = new ListItem();
                    li.Text = tItem.Value;
                    li.Value = tItem.Key;

                    ddl.Items.Add(li);
                }
                if (xmlNod.Attributes != null && (xmlNod.Attributes["blank"] != null))
                {
                    var li = new ListItem();
                    li.Text = "";
                    li.Value = "";

                    ddl.Items.Add(li);
                }

                ddl.DataBinding += DdListDataBinding;
                container.Controls.Add(ddl);
            }
            catch (Exception e)
            {
                var lc = new Literal();
                lc.Text = e.ToString();
                container.Controls.Add(lc);
            }
        }

        private void CreateCatDropDownList(Control container, XmlNode xmlNod)
        {
            try
            {
                var ddl = new DropDownList();
                ddl = (DropDownList)GenXmlFunctions.AssignByReflection(ddl, xmlNod);

                if (xmlNod.Attributes != null && (xmlNod.Attributes["allowblank"] != null))
                {
                        var li = new ListItem();
                        li.Text = "";
                        li.Value = "";
                        ddl.Items.Add(li);
                }

                var tList = GetCatList(xmlNod);
                foreach (var tItem in tList)
                {
                    var li = new ListItem();
                    li.Text = tItem.Value;
                    li.Value = tItem.Key.ToString("");
                    
                    ddl.Items.Add(li);
                }

                ddl.DataBinding += DdListDataBinding;
                container.Controls.Add(ddl);
            }
            catch (Exception e)
            {
                var lc = new Literal();
                lc.Text = e.ToString();
                container.Controls.Add(lc);
            }
        }

        private void CreatePropertyDropDownList(Control container, XmlNode xmlNod)
        {
            try
            {
                var ddl = new DropDownList();
                ddl = (DropDownList)GenXmlFunctions.AssignByReflection(ddl, xmlNod);

                if (xmlNod.Attributes != null && (xmlNod.Attributes["allowblank"] != null))
                {
                    var li = new ListItem();
                    li.Text = "";
                    li.Value = "";
                    ddl.Items.Add(li);
                }

                var tList = GetPropertyList(xmlNod);
                foreach (var tItem in tList)
                {
                    var li = new ListItem();
                    li.Text = tItem.Value;
                    li.Value = tItem.Key.ToString("");

                    ddl.Items.Add(li);
                }

                ddl.DataBinding += DdListDataBinding;
                container.Controls.Add(ddl);
            }
            catch (Exception e)
            {
                var lc = new Literal();
                lc.Text = e.ToString();
                container.Controls.Add(lc);
            }
        }

        private void DdListDataBinding(object sender, EventArgs e)
        {
            var ddl = (DropDownList)sender;
            var container = (IDataItemContainer)ddl.NamingContainer;

            try
            {
                ddl.Visible = visibleStatus.DefaultIfEmpty(true).First();

                var strValue = GenXmlFunctions.GetGenXmlValue(ddl.ID, "dropdownlist", Convert.ToString(DataBinder.Eval(container.DataItem, _databindColumn)));

                if ((ddl.Items.FindByValue(strValue) != null))
                {
                    ddl.SelectedValue = strValue;
                }
                else
                {
                    var nod = GenXmlFunctions.GetGenXmLnode(ddl.ID, "dropdownlist", Convert.ToString(DataBinder.Eval(container.DataItem, _databindColumn)));
                    if ((nod.Attributes != null) && (nod.Attributes["selectedtext"] != null))
                    {
                        strValue = XmlConvert.DecodeName(nod.Attributes["selectedtext"].Value);
                        if ((ddl.Items.FindByValue(strValue) != null))
                        {
                            ddl.SelectedValue = strValue;
                        }
                    }
                }
            }
            catch (Exception)
            {
                //do nothing
            }
        }

        private void CreateCatListBox(Control container, XmlNode xmlNod)
        {
            try
            {
                var ddl = new ListBox();
                ddl = (ListBox)GenXmlFunctions.AssignByReflection(ddl, xmlNod);

                if (xmlNod.Attributes != null && (xmlNod.Attributes["allowblank"] != null))
                {
                    var li = new ListItem();
                    li.Text = "";
                    li.Value = "";
                    ddl.Items.Add(li);
                }

                var tList = GetCatList(xmlNod);
                foreach (var tItem in tList)
                {
                    var li = new ListItem();
                    li.Text = tItem.Value;
                    li.Value = tItem.Key.ToString("");

                    ddl.Items.Add(li);
                }

                ddl.DataBinding += DdListBoxDataBinding;
                container.Controls.Add(ddl);
            }
            catch (Exception e)
            {
                var lc = new Literal();
                lc.Text = e.ToString();
                container.Controls.Add(lc);
            }
        }

        private void CreateGroupListBox(Control container, XmlNode xmlNod)
        {
            try
            {
                var ddl = new ListBox();
                ddl = (ListBox)GenXmlFunctions.AssignByReflection(ddl, xmlNod);

                var grptype = "1";
                if (ddl.Attributes["grouptype"] != null)
                {
                    grptype = ddl.Attributes["grouptype"];
                }

                var tList = NBrightBuyUtils.GetCategoryGroups(StoreSettings.Current.EditLanguage, true, grptype);
                foreach (var tItem in tList)
                {
                    if (tItem.GetXmlProperty("genxml/textbox/groupref") != "cat")
                    {
                        var li = new ListItem();
                        li.Text = tItem.GetXmlProperty("genxml/lang/genxml/textbox/groupname");
                        li.Value = tItem.GetXmlProperty("genxml/textbox/groupref");
                        ddl.Items.Add(li);                        
                    }
                }

                ddl.DataBinding += DdListBoxDataBinding;
                container.Controls.Add(ddl);
            }
            catch (Exception e)
            {
                var lc = new Literal();
                lc.Text = e.ToString();
                container.Controls.Add(lc);
            }
        }
        private void DdListBoxDataBinding(object sender, EventArgs e)
        {
            var ddl = (ListBox)sender;
            var container = (IDataItemContainer)ddl.NamingContainer;

            try
            {
                ddl.Visible = visibleStatus.DefaultIfEmpty(true).First();

                var strValue = GenXmlFunctions.GetGenXmlValue(ddl.ID, "dropdownlist", Convert.ToString(DataBinder.Eval(container.DataItem, _databindColumn)));

                if ((ddl.Items.FindByValue(strValue) != null))
                {
                    ddl.SelectedValue = strValue;
                }
                else
                {
                    var nod = GenXmlFunctions.GetGenXmLnode(ddl.ID, "dropdownlist", Convert.ToString(DataBinder.Eval(container.DataItem, _databindColumn)));
                    if ((nod.Attributes != null) && (nod.Attributes["selectedtext"] != null))
                    {
                        strValue = XmlConvert.DecodeName(nod.Attributes["selectedtext"].Value);
                        if ((ddl.Items.FindByValue(strValue) != null))
                        {
                            ddl.SelectedValue = strValue;
                        }
                    }
                }
            }
            catch (Exception)
            {
                //do nothing
            }
        }

        private void CreateCategoryParentName(Control container, XmlNode xmlNod)
        {
            try
            {
                var l = new Literal();
                l.DataBinding += CategoryParentNameDataBinding;
                container.Controls.Add(l);
            }
            catch (Exception e)
            {
                var lc = new Literal();
                lc.Text = e.ToString();
                container.Controls.Add(lc);
            }
        }

        private void CategoryParentNameDataBinding(object sender, EventArgs e)
        {
            var l = (Literal)sender;
            var container = (IDataItemContainer)l.NamingContainer;

            try
            {
                l.Visible = visibleStatus.DefaultIfEmpty(true).First();
                l.Text = "";
                 var strValue = Convert.ToString(DataBinder.Eval(container.DataItem, "ParentItemId"));
                if (Utils.IsNumeric(strValue))
                {
                    var pCat = CategoryUtils.GetCategoryData(Convert.ToInt32(strValue), StoreSettings.Current.EditLanguage);
                    if (pCat.Exists) l.Text = pCat.Info.GetXmlProperty("genxml/lang/genxml/textbox/txtcategoryname");                    
                }
            }
            catch (Exception)
            {
                //do nothing
            }
        }

        private Dictionary<int, string> GetCatList(XmlNode xmlNod)
        {
            var displaylevels = 20;
            var parentref = "";
            var prefix = "..";
            var showhidden = "False";
            var showarchived = "False";
            var showempty = "True";
            var showHidden = false;
            var showArchived = false;
            var catreflist = "";
            var parentid = 0;
            var displaycount = "False";
            var displayCount = false;
            var showEmpty = true;
            var groupref = "";
            var filtermode = "";
            List<int> validCatList = null;
            var modulekey = "";
            var redirecttabid = "";
            var tabid = "";
            var lang = Utils.GetCurrentCulture();
            var showall = "false";
            var showAll = false;

            if (xmlNod.Attributes != null)
            {
                if (xmlNod.Attributes["displaylevels"] != null)
                {
                    if (Utils.IsNumeric(xmlNod.Attributes["displaylevels"].Value)) displaylevels = Convert.ToInt32(xmlNod.Attributes["displaylevels"].Value);
                }

                if (xmlNod.Attributes["parentref"] != null) parentref = xmlNod.Attributes["parentref"].Value;
                if (xmlNod.Attributes["showhidden"] != null) showhidden = xmlNod.Attributes["showhidden"].Value;
                if (xmlNod.Attributes["showarchived"] != null) showarchived = xmlNod.Attributes["showarchived"].Value;
                if (xmlNod.Attributes["showempty"] != null) showempty = xmlNod.Attributes["showempty"].Value;
                if (xmlNod.Attributes["displaycount"] != null) displaycount = xmlNod.Attributes["displaycount"].Value;
                if (xmlNod.Attributes["prefix"] != null) prefix = xmlNod.Attributes["prefix"].Value;
                if (xmlNod.Attributes["groupref"] != null) groupref = xmlNod.Attributes["groupref"].Value;
                if (xmlNod.Attributes["filtermode"] != null) filtermode = xmlNod.Attributes["filtermode"].Value;
                if (xmlNod.Attributes["modulekey"] != null) modulekey = xmlNod.Attributes["modulekey"].Value;
                if (xmlNod.Attributes["lang"] != null) lang = xmlNod.Attributes["lang"].Value;
                if (xmlNod.Attributes["showall"] != null) showall = xmlNod.Attributes["showall"].Value;

                if (showall.ToLower() == "true") showAll = true;
                if (showhidden.ToLower() == "true") showHidden = true;
                if (showarchived.ToLower() == "true") showArchived = true;
                if (showempty.ToLower() == "false") showEmpty = false;
                if (displaycount.ToLower() == "true") displayCount = true;
                if (xmlNod.Attributes["catreflist"] != null) catreflist = xmlNod.Attributes["catreflist"].Value;
                var grpCatCtrl = new GrpCatController(lang);
                if (parentref != "")
                {
                    var p = grpCatCtrl.GetGrpCategoryByRef(parentref);
                    if (p != null) parentid = p.categoryid;
                }
                var catid = Utils.RequestQueryStringParam(HttpContext.Current.Request, "catid");
                if (!showAll && parentid == 0 && Utils.IsNumeric(catid)) parentid = Convert.ToInt32(catid); // needs to be passed to BuildCatList

                if (filtermode != "")
                {
                    var navigationData = new NavigationData(PortalSettings.Current.PortalId, modulekey);
                    if (String.IsNullOrEmpty(catid)) catid = navigationData.CategoryId.ToString("D"); 
                    if (Utils.IsNumeric(catid))
                    {
                        validCatList = GetCateoriesInProductList(Convert.ToInt32(catid));
                    }
                }

            }

            var rtnList = NBrightBuyUtils.BuildCatList(displaylevels, showHidden, showArchived, parentid, catreflist, prefix, displayCount, showEmpty, groupref,">",lang);

            if (validCatList != null)
            {
                var nonValid = new List<int>();
                // we have a filter on the list, so remove any categories not in valid list.
                foreach (var k in rtnList)
                {
                    if (!validCatList.Contains(k.Key)) nonValid.Add(k.Key);
                }
                foreach (var k in nonValid)
                {
                    rtnList.Remove(k);
                }
            }

            return rtnList;
        }


        private Dictionary<int, string> GetPropertyList(XmlNode xmlNod)
        {
            var displaylevels = 20;
            var parentref = "";
            var prefix = "..";
            var showhidden = "False";
            var showarchived = "False";
            var showempty = "True";
            var showHidden = false;
            var showArchived = false;
            var catreflist = "";
            var parentid = 0;
            var displaycount = "False";
            var displayCount = false;
            var showEmpty = true;
            var groupref = "";
            var filtermode = "";
            List<int> validCatList = null;
            var modulekey = "";
            var redirecttabid = "";
            var tabid = "";
            var lang = Utils.GetCurrentCulture();

            if (xmlNod.Attributes != null)
            {
                if (xmlNod.Attributes["displaylevels"] != null)
                {
                    if (Utils.IsNumeric(xmlNod.Attributes["displaylevels"].Value)) displaylevels = Convert.ToInt32(xmlNod.Attributes["displaylevels"].Value);
                }

                if (xmlNod.Attributes["parentref"] != null) parentref = xmlNod.Attributes["parentref"].Value;
                if (xmlNod.Attributes["showhidden"] != null) showhidden = xmlNod.Attributes["showhidden"].Value;
                if (xmlNod.Attributes["showarchived"] != null) showarchived = xmlNod.Attributes["showarchived"].Value;
                if (xmlNod.Attributes["showempty"] != null) showempty = xmlNod.Attributes["showempty"].Value;
                if (xmlNod.Attributes["displaycount"] != null) displaycount = xmlNod.Attributes["displaycount"].Value;
                if (xmlNod.Attributes["prefix"] != null) prefix = xmlNod.Attributes["prefix"].Value;
                if (xmlNod.Attributes["groupref"] != null) groupref = xmlNod.Attributes["groupref"].Value;
                if (xmlNod.Attributes["filtermode"] != null) filtermode = xmlNod.Attributes["filtermode"].Value;
                if (xmlNod.Attributes["modulekey"] != null) modulekey = xmlNod.Attributes["modulekey"].Value;
                if (xmlNod.Attributes["lang"] != null) lang = xmlNod.Attributes["lang"].Value;

                if (showhidden.ToLower() == "true") showHidden = true;
                if (showarchived.ToLower() == "true") showArchived = true;
                if (showempty.ToLower() == "false") showEmpty = false;
                if (displaycount.ToLower() == "true") displayCount = true;
                if (xmlNod.Attributes["catreflist"] != null) catreflist = xmlNod.Attributes["catreflist"].Value;
                var grpCatCtrl = new GrpCatController(lang);
                if (parentref != "")
                {
                    var p = grpCatCtrl.GetGrpCategoryByRef(parentref);
                    if (p != null) parentid = p.categoryid;
                }
                var catid = "";
                if (filtermode != "")
                {
                    var navigationData = new NavigationData(PortalSettings.Current.PortalId, modulekey);
                    catid = Utils.RequestQueryStringParam(HttpContext.Current.Request, "catid");
                    if (String.IsNullOrEmpty(catid)) catid = navigationData.CategoryId.ToString("D");
                    if (Utils.IsNumeric(catid))
                    {
                        validCatList = GetCateoriesInProductList(Convert.ToInt32(catid));
                    }
                }

            }

            var rtnList = NBrightBuyUtils.BuildPropertyList(displaylevels, showHidden, showArchived, parentid, catreflist, prefix, displayCount, showEmpty, groupref, ">", lang);

            if (validCatList != null)
            {
                var nonValid = new List<int>();
                // we have a filter on the list, so remove any categories not in valid list.
                foreach (var k in rtnList)
                {
                    if (!validCatList.Contains(k.Key)) nonValid.Add(k.Key);
                }
                foreach (var k in nonValid)
                {
                    rtnList.Remove(k);
                }
            }

            return rtnList;
        }


        /// <summary>
        /// Return a list of category ids for all the valid categories for a given product list (selected by a categoryid)  
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        private List<int> GetCateoriesInProductList(int categoryId)
        {
            var objQual = DotNetNuke.Data.DataProvider.Instance().ObjectQualifier;
            var dbOwner = DotNetNuke.Data.DataProvider.Instance().DatabaseOwner;

            var objCtrl = new NBrightBuyController();
            var strXML = objCtrl.GetSqlxml("select distinct XrefItemId from " + dbOwner + "[" + objQual + "NBrightBuy] where (typecode = 'CATCASCADE' or typecode = 'CATXREF') and parentitemid in (select parentitemid from " + dbOwner + "[" + objQual + "NBrightBuy] where (typecode = 'CATCASCADE' or typecode = 'CATXREF') and XrefItemId in (" + categoryId + ")) for xml raw ");
            // get returned XML into generic List
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml("<root>" + strXML + "</root>");
            var nList = xmlDoc.SelectNodes("root/row");
            var rtnList = new List<int>();
            foreach (XmlNode n in nList)
            {
                if (n.Attributes["XrefItemId"].Value != null && Utils.IsNumeric(n.Attributes["XrefItemId"].Value))
                {
                    rtnList.Add(Convert.ToInt32(n.Attributes["XrefItemId"].Value));
                }
            }
            return rtnList;
        }

        #endregion

        #region "catbreadcrumb"

        private void CreateCatBreadCrumb(Control container, XmlNode xmlNod)
        {
            var lc = new Literal();
            lc.Text = xmlNod.OuterXml;
            lc.DataBinding += CatBreadCrumbDataBind;
            container.Controls.Add(lc);
        }

        private void CatBreadCrumbDataBind(object sender, EventArgs e)
        {
            var lc = (Literal)sender;
            var container = (IDataItemContainer)lc.NamingContainer;
            try
            {
                var grpCatCtrl = new GrpCatController(Utils.GetCurrentCulture());

                lc.Visible = visibleStatus.DefaultIfEmpty(true).First();

                if (visibleStatus.DefaultIfEmpty(true).First())
                {

                    var xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml("<root>" + lc.Text + "</root>");
                    var xmlNod = xmlDoc.SelectSingleNode("root/tag");

                    var catid = -1;
                    if (container.DataItem is NBrightInfo)
                    {
                        // Must be displaying a product or category with (NBrightInfo), so get categoryid
                        var objCInfo = (NBrightInfo) container.DataItem;
                        if (String.IsNullOrEmpty(objCInfo.TypeCode) || objCInfo.TypeCode == "PRD") // no type is list header, so use catid in url if there. 
                        {
                            //Is product so get categoryid    
                            var id = Convert.ToString(DataBinder.Eval(container.DataItem, "ItemId"));
                            var targetModuleKey = "";
                            if (xmlNod != null && xmlNod.Attributes != null && xmlNod.Attributes["targetmodulekey"] != null) targetModuleKey = xmlNod.Attributes["targetmodulekey"].InnerText;
                            var obj = grpCatCtrl.GetCurrentCategoryData(PortalSettings.Current.PortalId, lc.Page.Request, Convert.ToInt32(id), _settings, targetModuleKey);
                            if (obj != null) catid = obj.categoryid;
                        }
                        else if (objCInfo.TypeCode == "CATEGORYLANG") // no type is list header, so use catid in url if there. 
                        {
                            catid = objCInfo.ParentItemId;

                        }
                        else
                        {
                            catid = objCInfo.ItemID;
                        }
                    }

                    if (container.DataItem is GroupCategoryData)
                    {
                        // GroupCategoryData class, so use categoryid
                        var id = Convert.ToString(DataBinder.Eval(container.DataItem, "categoryid"));
                        if (Utils.IsNumeric(id)) catid = Convert.ToInt32(id);
                    }


                    var intLength = 400;
                    var intShortLength = -1;
                    var isLink = false;
                    var separator = ">";
                    var aslist = false;

                    if (xmlNod != null && xmlNod.Attributes != null)
                    {
                        if (xmlNod.Attributes["length"] != null)
                        {
                            if (Utils.IsNumeric(xmlNod.Attributes["length"].InnerText))
                            {
                                intLength = Convert.ToInt32(xmlNod.Attributes["length"].InnerText);
                            }
                        }
                        if (xmlNod.Attributes["links"] != null) isLink = true;
                        if (xmlNod.Attributes["short"] != null)
                        {
                            if (Utils.IsNumeric(xmlNod.Attributes["short"].InnerText))
                            {
                                intShortLength = Convert.ToInt32(xmlNod.Attributes["short"].InnerText);
                            }
                        }
                        if (xmlNod.Attributes["separator"] != null) separator = xmlNod.Attributes["separator"].InnerText;
                        if (xmlNod.Attributes["aslist"] != null && xmlNod.Attributes["aslist"].InnerText.ToLower() == "true") aslist = true;
                    }

                    if (catid > 0) // check we have a catid
                    {
                        if (isLink)
                        {
                            var defTabId = PortalSettings.Current.ActiveTab.TabID;
                            if (xmlNod.Attributes["admin"] == null)
                            {
                            if (_settings.ContainsKey("ddllisttabid") && Utils.IsNumeric(_settings["ddllisttabid"])) defTabId = Convert.ToInt32(_settings["ddllisttabid"]);
                            }
                            lc.Text = grpCatCtrl.GetBreadCrumbWithLinks(catid, defTabId, intShortLength, separator, aslist);
                        }
                        else
                        {
                            lc.Text = grpCatCtrl.GetBreadCrumb(catid, intShortLength, separator, aslist);
                        }

                        if ((lc.Text.Length > intLength) && (!aslist))
                        {
                            lc.Text = lc.Text.Substring(0, (intLength - 3)) + "...";
                        }
                    }
                }
            }
            catch (Exception)
            {
                lc.Text = "";
            }
        }

        #endregion

        #region "CreateCatDefaultName"

        private void CreateCatDefaultName(Control container, XmlNode xmlNod)
        {
            var lc = new Literal();
            if (xmlNod.Attributes != null && xmlNod.Attributes["default"] != null) lc.Text = xmlNod.Attributes["default"].InnerText;
            lc.DataBinding += CatDefaultNameDataBind;
            container.Controls.Add(lc);
        }

        private void CatDefaultNameDataBind(object sender, EventArgs e)
        {
            var lc = (Literal)sender;
            var container = (IDataItemContainer)lc.NamingContainer;
            try
            {
                lc.Visible = visibleStatus.DefaultIfEmpty(true).First();
                var moduleId = DataBinder.Eval(container.DataItem, "ModuleId");
                var id = Convert.ToString(DataBinder.Eval(container.DataItem, "ItemId"));
                var lang = Convert.ToString(DataBinder.Eval(container.DataItem, "lang"));

                if (Utils.IsNumeric(id) && Utils.IsNumeric(moduleId))
                {
                    var grpCatCtrl = new GrpCatController(Utils.GetCurrentCulture());
                    var objCInfo = grpCatCtrl.GetCurrentCategoryData(PortalSettings.Current.PortalId, lc.Page.Request, Convert.ToInt32(id));
                    if (objCInfo != null)
                    {
                        lc.Text = objCInfo.categoryname;
                    }
                }

            }
            catch (Exception ex)
            {
                lc.Text = ex.ToString();
            }
        }

        #endregion

        #region "CreateCatDefault"

        private void CreateCatDefault(Control container, XmlNode xmlNod)
        {
            var lc = new Literal();
            if (xmlNod.Attributes != null && xmlNod.Attributes["name"] != null) lc.Text = xmlNod.Attributes["name"].InnerText;
            lc.DataBinding += CatDefaultDataBind;
            container.Controls.Add(lc);
        }

        private void CatDefaultDataBind(object sender, EventArgs e)
        {
            var lc = (Literal)sender;
            var name = lc.Text;
            lc.Text = "";
            var container = (IDataItemContainer)lc.NamingContainer;
            try
            {
                lc.Visible = visibleStatus.DefaultIfEmpty(true).First();
                var moduleId = DataBinder.Eval(container.DataItem, "ModuleId");
                var id = Convert.ToString(DataBinder.Eval(container.DataItem, "ItemId"));
                var lang = Convert.ToString(DataBinder.Eval(container.DataItem, "lang"));

                if (Utils.IsNumeric(id) && Utils.IsNumeric(moduleId))
                {
                    var moduleKey = "";
                    // if we have no catid in url, we're going to need a default category from module.
                    var settings = new System.Collections.Hashtable();
                    var modSettings = new ModSettings(Convert.ToInt32(moduleId), settings);
                    moduleKey = modSettings.Get("modulekey");

                    var grpCatCtrl = new GrpCatController(Utils.GetCurrentCulture());
                    var objCInfo = grpCatCtrl.GetCurrentCategoryData(PortalSettings.Current.PortalId, lc.Page.Request, Convert.ToInt32(id), modSettings.Settings(), moduleKey);
                    if (objCInfo != null)
                    {
                        GroupCategoryData objPcat;
                        switch (name.ToLower())
                        {
                            case "categorydesc":
                                lc.Text = objCInfo.categorydesc;
                                break;
                            case "message":
                                lc.Text = System.Web.HttpUtility.HtmlDecode(objCInfo.message);
                                break;
                            case "archived":
                                lc.Text = objCInfo.archived.ToString(CultureInfo.InvariantCulture);
                                break;
                            case "breadcrumb":
                                lc.Text = objCInfo.breadcrumb;
                                break;
                            case "categoryid":
                                lc.Text = objCInfo.categoryid.ToString("");
                                break;
                            case "categoryname":
                                lc.Text = objCInfo.categoryname;
                                break;
                            case "categoryref":
                                lc.Text = objCInfo.categoryref;
                                break;
                            case "depth":
                                lc.Text = objCInfo.depth.ToString("");
                                break;
                            case "disabled":
                                lc.Text = objCInfo.disabled.ToString(CultureInfo.InvariantCulture) ;
                                break;
                            case "entrycount":
                                lc.Text = objCInfo.entrycount.ToString("");
                                break;
                            case "grouptyperef":
                                lc.Text = objCInfo.grouptyperef;
                                break;
                            case "imageurl":
                                lc.Text = objCInfo.imageurl;
                                break;
                            case "ishidden":
                                lc.Text = objCInfo.ishidden.ToString(CultureInfo.InvariantCulture);
                                break;
                            case "isvisible":
                                lc.Text = objCInfo.isvisible.ToString(CultureInfo.InvariantCulture) ;
                                break;
                            case "metadescription":
                                lc.Text = objCInfo.metadescription;
                                break;
                            case "metakeywords":
                                lc.Text = objCInfo.metakeywords;
                                break;
                            case "parentcatid":
                                lc.Text = objCInfo.parentcatid.ToString("");
                                break;
                            case "parentcategoryname":
                                objPcat = grpCatCtrl.GetCategory(objCInfo.parentcatid);
                                lc.Text = objPcat.categoryname; 
                                break;
                            case "parentcategoryref":
                                objPcat = grpCatCtrl.GetCategory(objCInfo.parentcatid);
                                lc.Text = objPcat.categoryref; 
                                break;
                            case "parentcategorydesc":
                                objPcat = grpCatCtrl.GetCategory(objCInfo.parentcatid);
                                lc.Text = objPcat.categorydesc;
                                break;
                            case "parentcategorybreadcrumb":
                                objPcat = grpCatCtrl.GetCategory(objCInfo.parentcatid);
                                lc.Text = objPcat.breadcrumb;
                                break;
                            case "parentcategoryguidkey":
                                objPcat = grpCatCtrl.GetCategory(objCInfo.parentcatid);
                                lc.Text = objPcat.categoryrefGUIDKey;
                                break;
                            case "recordsortorder":
                                lc.Text = objCInfo.recordsortorder.ToString("");
                                break;
                            case "seoname":
                                lc.Text = objCInfo.seoname;
                                if (lc.Text == "") lc.Text = objCInfo.categoryname;
                                break;
                            case "seopagetitle":
                                lc.Text = objCInfo.seopagetitle ;
                                break;
                            case "url":
                                lc.Text = objCInfo.url ;
                                break;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                lc.Text = ex.ToString();
            }
        }

        #endregion

        #region "CreateCatValueOf"

        private void CreateCatValueOf(Control container, XmlNode xmlNod)
        {
            var lc = new Literal();
            if (xmlNod.Attributes != null && (xmlNod.Attributes["xpath"] != null))
            {
                lc.Text = xmlNod.Attributes["xpath"].Value;
            }
            lc.DataBinding += CatValueOfDataBind;
            container.Controls.Add(lc);
        }

        private void CatValueOfDataBind(object sender, EventArgs e)
        {
            var lc = (Literal)sender;
            var container = (IDataItemContainer)lc.NamingContainer;
            try
            {
                lc.Visible = visibleStatus.DefaultIfEmpty(true).First();

                var id = 0;
                try
                {
                    id = (int) DataBinder.Eval(container.DataItem, "ItemId");
                }
                catch (Exception)
                {
                    id = (int) DataBinder.Eval(container.DataItem, "categoryid");
                }
                var grpCatCtrl = new GrpCatController(Utils.GetCurrentCulture());
                var objCInfo = grpCatCtrl.GetCurrentCategoryInfo(PortalSettings.Current.PortalId, lc.Page.Request, id);
                if (objCInfo != null)
                {
                    lc.Text = objCInfo.GetXmlProperty(lc.Text);
                }
                else
                {
                    lc.Text = "";
                }
            }
            catch (Exception ex)
            {
                lc.Text = "";
            }
        }

        #endregion

        #region "CreateCatBreakOf"

        private void CreateCatBreakOf(Control container, XmlNode xmlNod)
        {
            var lc = new Literal();
            if (xmlNod.Attributes != null && (xmlNod.Attributes["xpath"] != null))
            {
                lc.Text = xmlNod.Attributes["xpath"].Value;
            }
            lc.DataBinding += CatBreakOfDataBind;
            container.Controls.Add(lc);
        }

        private void CatBreakOfDataBind(object sender, EventArgs e)
        {
            var lc = (Literal)sender;
            var container = (IDataItemContainer)lc.NamingContainer;
            try
            {
                lc.Visible = visibleStatus.DefaultIfEmpty(true).First();
                var id = 0;
                try
                {
                    id = (int) DataBinder.Eval(container.DataItem, "ItemId");
                }
                catch (Exception)
                {
                    id = (int) DataBinder.Eval(container.DataItem, "categoryid");
                }
                    var grpCatCtrl = new GrpCatController(Utils.GetCurrentCulture());
                    var objCInfo = grpCatCtrl.GetCurrentCategoryInfo(PortalSettings.Current.PortalId, lc.Page.Request, Convert.ToInt32(id));
                    if (objCInfo != null)
                    {
                        lc.Text = objCInfo.GetXmlProperty(lc.Text);
                        lc.Text = System.Web.HttpUtility.HtmlEncode(lc.Text);
                        lc.Text = lc.Text.Replace(Environment.NewLine, "<br/>");
                    }
                    else
                    {
                        lc.Text = "";
                    }
            }
            catch (Exception)
            {
                lc.Text = "";
            }
        }

        #endregion

        #region "CreateCatHtmlOf"

        private void CreateCatHtmlOf(Control container, XmlNode xmlNod)
        {
            var lc = new Literal();
            if (xmlNod.Attributes != null && (xmlNod.Attributes["xpath"] != null))
            {
                lc.Text = xmlNod.Attributes["xpath"].Value;
            }
            lc.DataBinding += CatHtmlOfDataBind;
            container.Controls.Add(lc);
        }

        private void CatHtmlOfDataBind(object sender, EventArgs e)
        {
            var lc = (Literal)sender;
            var container = (IDataItemContainer)lc.NamingContainer;
            try
            {
                lc.Visible = visibleStatus.DefaultIfEmpty(true).First();
                var id = 0;
                try
                {
                    id = (int) DataBinder.Eval(container.DataItem, "ItemId");
                }
                catch (Exception)
                {
                    id = (int) DataBinder.Eval(container.DataItem, "categoryid");
                }
                var grpCatCtrl = new GrpCatController(Utils.GetCurrentCulture());
                var objCInfo = grpCatCtrl.GetCurrentCategoryInfo(PortalSettings.Current.PortalId, lc.Page.Request, Convert.ToInt32(id));
                if (objCInfo != null)
                {
                    lc.Text = objCInfo.GetXmlProperty(lc.Text);
                    lc.Text = System.Web.HttpUtility.HtmlDecode(lc.Text);
                }
                else
                {
                    lc.Text = "";
                }
            }
            catch (Exception)
            {
                lc.Text = "";
            }
        }

        #endregion

        #region "Product Options"

        private void Createproductoptions(Control container, XmlNode xmlNod)
        {

                // create all 3 control possible
                var ddl = new DropDownList();
                var chk = new CheckBox();
                var txt = new TextBox();
                // pass wrapper templates using ddl attributes.
                if (xmlNod.Attributes != null && (xmlNod.Attributes["template"] != null))
                {
                    ddl.Attributes.Add("template", xmlNod.Attributes["template"].Value);
                    chk.Attributes.Add("template", xmlNod.Attributes["template"].Value);
                    txt.Attributes.Add("template", xmlNod.Attributes["template"].Value);
                }
                if (xmlNod.Attributes != null && (xmlNod.Attributes["index"] != null))
                {
                    if (Utils.IsNumeric(xmlNod.Attributes["index"].Value)) // must have a index
                    {
                        ddl.Attributes.Add("index", xmlNod.Attributes["index"].Value);
                        ddl = (DropDownList) GenXmlFunctions.AssignByReflection(ddl, xmlNod);
                        ddl.DataBinding += ProductoptionsDataBind;
                        ddl.Visible = false;
                        ddl.Enabled = false;
                        ddl.ID = "optionddl" + xmlNod.Attributes["index"].Value;
                        if (xmlNod.Attributes != null && (xmlNod.Attributes["blank"] != null))
                        {
                            ddl.Attributes.Add("blank", xmlNod.Attributes["blank"].Value);
                        }
                        container.Controls.Add(ddl);
                        chk.Attributes.Add("index", xmlNod.Attributes["index"].Value);
                        chk = (CheckBox) GenXmlFunctions.AssignByReflection(chk, xmlNod);
                        chk.DataBinding += ProductoptionsDataBind;
                        chk.ID = "optionchk" + xmlNod.Attributes["index"].Value;
                        chk.Visible = false;
                        chk.Enabled = false;
                        container.Controls.Add(chk);
                        txt.Attributes.Add("index", xmlNod.Attributes["index"].Value);
                        txt = (TextBox) GenXmlFunctions.AssignByReflection(txt, xmlNod);
                        txt.DataBinding += ProductoptionsDataBind;
                        txt.ID = "optiontxt" + xmlNod.Attributes["index"].Value;
                        if (xmlNod.Attributes["required"] != null) txt.Attributes.Add("required", "");
                        if (xmlNod.Attributes["datatype"] != null) txt.Attributes.Add("datatype", xmlNod.Attributes["datatype"].InnerText);
                        txt.Visible = false;
                        txt.Enabled = false;
                        container.Controls.Add(txt);
                        var hid = new HiddenField();
                        hid.DataBinding += ProductoptionsDataBind;
                        hid.ID = "optionid" + xmlNod.Attributes["index"].Value;
                        hid.Value = xmlNod.Attributes["index"].Value;
                        container.Controls.Add(hid);
                    }
                }
        }

        private void ProductoptionsDataBind(object sender, EventArgs e)
        {
            #region "Init"

            var ctl = (Control) sender;
            var container = (IDataItemContainer) ctl.NamingContainer;
            var objInfo = (NBrightInfo) container.DataItem;
            var useCtrlType = "";
            var index = "1";

            var xpathprefix = "";
            var cartrecord = objInfo.GetXmlProperty("genxml/productid") != ""; // if we have a productid node, then is datarecord is a cart item
            if (cartrecord) xpathprefix = "genxml/productxml/";

            DropDownList ddl = null;
            CheckBox chk = null;
            TextBox txt = null;
            HiddenField hid = null;

            if (ctl is HiddenField)
            {
                hid = (HiddenField) ctl;
                index = hid.Value;
            }

            if (ctl is DropDownList)
            {
                ddl = (DropDownList) ctl;
                index = ddl.Attributes["index"];
                ddl.Attributes.Remove("index");
            }
            if (ctl is CheckBox)
            {
                chk = (CheckBox) ctl;
                index = chk.Attributes["index"];
                chk.Attributes.Remove("index");
            }
            if (ctl is TextBox)
            {
                txt = (TextBox) ctl;
                index = txt.Attributes["index"];
                txt.Attributes.Remove("index");
            }


            var optionid = "";
            var optiondesc = "";
            XmlNodeList nodList = null;
            var nod = objInfo.XMLDoc.SelectSingleNode(xpathprefix + "genxml/options/genxml[" + index + "]/hidden/optionid");
            if (nod != null)
            {
                optionid = nod.InnerText;
                if (hid != null) hid.Value = optionid;
                var nodDesc = objInfo.XMLDoc.SelectSingleNode(xpathprefix + "genxml/lang/genxml/options/genxml[" + index + "]/textbox/txtoptiondesc");
                if (nodDesc != null) optiondesc = nodDesc.InnerText;

                nodList = objInfo.XMLDoc.SelectNodes(xpathprefix + "genxml/optionvalues[@optionid='" + optionid + "']/*");
                if (nodList != null)
                {
                    switch (nodList.Count)
                    {
                        case 0:
                            useCtrlType = "TextBox";
                            break;
                        case 1:
                            useCtrlType = "CheckBox";
                            break;
                        default:
                            useCtrlType = "DropDownList";
                            break;
                    }
                }
            }

            #endregion

            if (visibleStatus.DefaultIfEmpty(true).First())
            {
                // hide all the controls
                if (ddl != null) ddl.Visible = false;
                if (chk != null) chk.Visible = false;
                if (txt != null) txt.Visible = false;

                if (ddl != null && useCtrlType == "DropDownList")
                {
                    try
                    {
                        ddl.Visible = true;
                        ddl.Enabled = true;
                        if (nodList != null)
                        {

                            if (ddl.Attributes["blank"] != null)
                            {
                                var li = new ListItem();
                                li.Text = ddl.Attributes["blank"];
                                li.Value = "0";
                                ddl.Items.Add(li);
                                ddl.Attributes.Remove("blank");
                            }

                            var lp = 1;
                            foreach (XmlNode nodOptVal in nodList)
                            {
                                var nodVal = nodOptVal.SelectSingleNode("hidden/optionvalueid");
                                if (nodVal != null)
                                {
                                    var optionvalueid = nodVal.InnerText;
                                    var li = new ListItem();
                                    var nodLang = objInfo.XMLDoc.SelectSingleNode(xpathprefix + "genxml/lang/genxml/optionvalues[@optionid='" + optionid + "']/genxml[" + lp.ToString("") + "]/textbox/txtoptionvaluedesc");
                                    if (nodLang != null)
                                    {
                                        li.Text = nodLang.InnerText;
                                        li.Value = optionvalueid;
                                        if (li.Text != "") ddl.Items.Add(li);
                                    }
                                }
                                lp += 1;
                            }
                            if (cartrecord)
                            {
                                // assign cart value   
                                var selectSingleNode = objInfo.XMLDoc.SelectSingleNode("genxml/options/option[optid='" + optionid + "']/optvalueid");
                                if (selectSingleNode != null && ddl.Items.FindByValue(selectSingleNode.InnerText) != null) ddl.SelectedValue = selectSingleNode.InnerText;
                            }
                            else
                            {
                                if (nodList.Count > 0) ddl.SelectedIndex = 0;
                            }
                        }


                    }
                    catch (Exception)
                    {
                        ddl.Visible = false;
                    }
                }

                if (chk != null && useCtrlType == "CheckBox")
                {
                    try
                    {
                        chk.Visible = true;
                        chk.Enabled = true;
                        if (nodList != null)
                        {
                            var lp = 1;
                            foreach (XmlNode nodOptVal in nodList)
                            {
                                var nodVal = nodOptVal.SelectSingleNode("hidden/optionvalueid");
                                if (nodVal != null)
                                {
                                    chk.Text = "";
                                    var optionvalueid = nodVal.InnerText;
                                    var nodLang = objInfo.XMLDoc.SelectSingleNode(xpathprefix + "genxml/lang/genxml/optionvalues[@optionid='" + optionid + "']/genxml[" + lp.ToString("") + "]/textbox/txtoptionvaluedesc");
                                    if (nodLang != null) chk.Text = nodLang.InnerText;

                                    chk.Attributes.Add("optionvalueid", optionvalueid);
                                    chk.Attributes.Add("optionid", optionid);

                                    if (cartrecord)
                                    {
                                        // assign cart value   
                                        var selectSingleNode = objInfo.XMLDoc.SelectSingleNode("genxml/options/option[optid='" + optionid + "']/optvalueid");
                                        if (selectSingleNode != null && selectSingleNode.InnerText == "True") chk.Checked = true;
                                    }
                                }
                                lp += 1;
                            }
                        }

                    }
                    catch (Exception)
                    {
                        chk.Visible = false;
                    }

                }

                if (txt != null && useCtrlType == "TextBox")
                {
                    txt.Visible = true;
                    txt.Enabled = true;
                    txt.Attributes.Add("optionid", optionid);
                    txt.Attributes.Add("optiondesc", optiondesc);
                    if (cartrecord)
                    {
                        // assign cart value   
                        var selectSingleNode = objInfo.XMLDoc.SelectSingleNode("genxml/options/option[optid='" + optionid + "']/optvaltext");
                        if (selectSingleNode != null) txt.Text = selectSingleNode.InnerText;
                    }

                }
            }
            else
            {
                // hide all the controls
                if (ddl != null) ddl.Visible = false;
                if (chk != null) chk.Visible = false;
                if (txt != null) txt.Visible = false;
            }
        }

        #endregion

        #region "productlist"

        private void Createproductlist(Control container, XmlNode xmlNod)
        {
            if (xmlNod.Attributes != null && (xmlNod.Attributes["template"] != null))
            {

                var lc = new Literal();
                if (xmlNod.Attributes != null && (xmlNod.Attributes["template"] != null))
                {
                    lc.Text = xmlNod.Attributes["template"].Value;
                    if (xmlNod.Attributes["cascade"] != null) lc.Text = lc.Text + ":" + xmlNod.Attributes["cascade"].Value;
                    if (xmlNod.Attributes["orderby"] != null) lc.Text = lc.Text + ":" + xmlNod.Attributes["orderby"].Value;
                    if (xmlNod.Attributes["filter"] != null) lc.Text = lc.Text + ":" + xmlNod.Attributes["filter"].Value;
                }

                lc.DataBinding += ProductlistDataBind;
                container.Controls.Add(lc);

            }
        }

        private void ProductlistDataBind(object sender, EventArgs e)
        {


            var lc = (Literal)sender;
            var container = (IDataItemContainer)lc.NamingContainer;
            try
            {
                var strOut = "";
                lc.Visible = visibleStatus.DefaultIfEmpty(true).First();
                if (lc.Visible)
                {

                    var param = lc.Text.Split(':');
                    if (param.Count() == 4)
                    {
                        try
                        {

                            var strFilter = "";
                            var templName = param[0];
                            var cascade = param[1];
                            var strOrder = param[2];
                            var filter = param[3];

                            if ((templName != ""))
                            {

                                var nbi = (GroupCategoryData)container.DataItem;
                                var lang = Utils.GetCurrentCulture();

                                var strCacheKey = lang + "*" + nbi.categoryid + "*" + lc.Text;
                                if (!StoreSettings.Current.DebugMode) strOut = (String)Utils.GetCache(strCacheKey);

                                if (String.IsNullOrEmpty(strOut))
                                {
                                    var buyCtrl = new NBrightBuyController();
                                    var rpTempl = buyCtrl.GetTemplateData(-1, templName, lang, _settings, StoreSettings.Current.DebugMode);

                                    //remove templName from template, so we don't get a loop.
                                    if (rpTempl.Contains(templName)) rpTempl = rpTempl.Replace(templName, "");
                                    //build models list

                                    var objQual = DotNetNuke.Data.DataProvider.Instance().ObjectQualifier;
                                    var dbOwner = DotNetNuke.Data.DataProvider.Instance().DatabaseOwner;
                                    if (cascade.ToLower() == "true")
                                    {
                                        strFilter = strFilter + " and NB1.[ItemId] in (select parentitemid from " + dbOwner + "[" + objQual + "NBrightBuy] where (typecode = 'CATCASCADE' or typecode = 'CATXREF') and XrefItemId = " + nbi.categoryid.ToString("") + ") ";
                                    }
                                    else
                                        strFilter = strFilter + " and NB1.[ItemId] in (select parentitemid from " + dbOwner + "[" + objQual + "NBrightBuy] where typecode = 'CATXREF' and XrefItemId = " + nbi.categoryid.ToString("") + ") ";

                                    if (strOrder == "{bycategoryproduct}") strOrder += nbi.categoryid.ToString(""); // do special custom sort in each cateogry

                                    if (filter != "") strFilter += " AND " + filter;

                                    var objL = buyCtrl.GetDataList(PortalSettings.Current.PortalId, -1, "PRD", "PRDLANG", Utils.GetCurrentCulture(), strFilter, strOrder);

                                    // inject the categoryid into the data, so the entryurl can have the correct catid
                                    foreach (var i in objL)
                                    {
                                        i.SetXmlProperty("genxml/categoryid", nbi.categoryid.ToString(""));
                                    }

                                    var itemTemplate = NBrightBuyUtils.GetGenXmlTemplate(rpTempl, _settings, PortalSettings.Current.HomeDirectory,visibleStatus);
                                    strOut = GenXmlFunctions.RenderRepeater(objL, itemTemplate);
                                    if (!StoreSettings.Current.DebugMode) NBrightBuyUtils.SetModCache(-1, strCacheKey, strOut);                                    
                                }

                            }
                        }
                        catch (Exception exc)
                        {
                            strOut = "ERROR: <br/>" + exc;
                        }

                    }


                }
                lc.Text = strOut;

            }
            catch (Exception)
            {
                lc.Text = "";
            }

        }


        #endregion

        #region "Models"

        private void Createmodelslist(Control container, XmlNode xmlNod)
        {
            if (xmlNod.Attributes != null && (xmlNod.Attributes["template"] != null))
            {
                var templName = xmlNod.Attributes["template"].Value;
                var buyCtrl = new NBrightBuyController();
                var rpTempl = buyCtrl.GetTemplateData(-1, templName, Utils.GetCurrentCulture(), _settings, StoreSettings.Current.DebugMode);

                //remove templName from template, so we don't get a loop.
                if (rpTempl.Contains(templName)) rpTempl = rpTempl.Replace(templName, "");
                var rpt = new Repeater { ItemTemplate = new GenXmlTemplate(rpTempl, _settings,visibleStatus) };
                rpt.Init += ModelslistInit; // databind causes infinate loop
                container.Controls.Add(rpt);
            }
        }

        private void ModelslistInit(object sender, EventArgs e)
        {
            var rpt = (Repeater)sender;
            var container = (IDataItemContainer)rpt.NamingContainer;
            rpt.Visible = visibleStatus.DefaultIfEmpty(true).First();
            if (rpt.Visible && container.DataItem != null)  // check for null dataitem, becuase we won't have it on postback.
            {
                //build models list
                var objL = NBrightBuyUtils.BuildModelList((NBrightInfo)container.DataItem, true);
                rpt.DataSource = objL;
                rpt.DataBind();
            }
        }


        private void Createmodelsradio(Control container, XmlNode xmlNod)
        {
            //NOTE: rbl.Attributes.Add("required", xmlNod.Attributes["required"].Value); will not work on radiobuttonlist, placed into <span> not the input field
            var rbl = new RadioButtonList();
            if (xmlNod.Attributes != null && (xmlNod.Attributes["template"] != null))
            {
                rbl.Attributes.Add("template", xmlNod.Attributes["template"].Value);
            }
            if (xmlNod.Attributes != null && (xmlNod.Attributes["blank"] != null))
            {
                rbl.Attributes.Add("blank", xmlNod.Attributes["blank"].Value);
            }
            if (xmlNod.Attributes != null && (xmlNod.Attributes["displayprice"] != null))
            {
                rbl.Attributes.Add("displayprice", xmlNod.Attributes["displayprice"].Value);
            }
            rbl = (RadioButtonList)GenXmlFunctions.AssignByReflection(rbl, xmlNod);
            rbl.DataBinding += ModelsradioDataBind;
            rbl.ID = "rblModelsel";
            container.Controls.Add(rbl);
        }

        private void ModelsradioDataBind(object sender, EventArgs e)
        {
            var rbl = (RadioButtonList)sender;
            var container = (IDataItemContainer)rbl.NamingContainer;
            try
            {
                rbl.Visible = visibleStatus.DefaultIfEmpty(true).First();
                if (rbl.Visible)
                {
                    var templ = "{name} ({price})";
                    if (rbl.Attributes["template"] != null)
                    {
                        templ = rbl.Attributes["template"];
                        rbl.Attributes.Remove("template");
                    }

                    var objL = NBrightBuyUtils.BuildModelList((NBrightInfo) container.DataItem, true);

                    var displayPrice = true;
                    if (rbl.Attributes["displayprice"] != null)
                    {
                        displayPrice = Convert.ToBoolean(rbl.Attributes["displayprice"]);
                        rbl.Attributes.Remove("displayprice");
                    }
                    else
                    {
                        displayPrice = NBrightBuyUtils.HasDifferentPrices((NBrightInfo)container.DataItem);
                    }

                    if (rbl.Attributes["blank"] != null)
                    {
                        var li = new ListItem();
                        li.Text = rbl.Attributes["blank"];
                        li.Value = "0";
                        rbl.Items.Add(li);
                        rbl.Attributes.Remove("blank");
                    }

                    foreach (var obj in objL)
                    {
                        var li = new ListItem();
                        li.Text = NBrightBuyUtils.GetItemDisplay(obj, templ, displayPrice);
                        li.Value = obj.GetXmlProperty("genxml/hidden/modelid");
                        if (li.Text != "") rbl.Items.Add(li);
                    }
                }

            }
            catch (Exception)
            {
                rbl.Visible = false;
            }
        }

        private void Createmodelsdropdown(Control container, XmlNode xmlNod)
        {
            var rbl = new DropDownList();
            if (xmlNod.Attributes != null && (xmlNod.Attributes["template"] != null))
            {
                rbl.Attributes.Add("template", xmlNod.Attributes["template"].Value);
            }
            if (xmlNod.Attributes != null && (xmlNod.Attributes["blank"] != null))
            {
                rbl.Attributes.Add("blank", xmlNod.Attributes["blank"].Value);
            }
            if (xmlNod.Attributes != null && (xmlNod.Attributes["displayprice"] != null))
            {
                rbl.Attributes.Add("displayprice", xmlNod.Attributes["displayprice"].Value);
            }
            rbl = (DropDownList)GenXmlFunctions.AssignByReflection(rbl, xmlNod);
            rbl.DataBinding += ModelsdropdownDataBind;
            rbl.ID = "ddlModelsel";
            container.Controls.Add(rbl);
        }

        private void ModelsdropdownDataBind(object sender, EventArgs e)
        {
            var ddl = (DropDownList)sender;
            var container = (IDataItemContainer)ddl.NamingContainer;
            try
            {
                ddl.Visible = visibleStatus.DefaultIfEmpty(true).First();
                if (ddl.Visible)
                {
                    var templ = "{name} {price}";
                    if(ddl.Attributes["template"] != null)
                    {
                        templ = ddl.Attributes["template"];
                        ddl.Attributes.Remove("template");                        
                    }
                    var dataInfo = (NBrightInfo) container.DataItem;

                    var objL = NBrightBuyUtils.BuildModelList(dataInfo, true);

                    var displayPrice = true;
                    if (ddl.Attributes["displayprice"] != null)
                    {
                        displayPrice = Convert.ToBoolean(ddl.Attributes["displayprice"]);
                        ddl.Attributes.Remove("displayprice");
                    }
                    else
                    {
                        displayPrice = NBrightBuyUtils.HasDifferentPrices((NBrightInfo)container.DataItem);                        
                    }

                    if (ddl.Attributes["blank"] != null)
                    {                       
                        var li = new ListItem();
                        li.Text = ddl.Attributes["blank"];
                        li.Value = "0";
                        ddl.Items.Add(li);
                        ddl.Attributes.Remove("blank");
                    }

                    foreach (var obj in objL)
                    {
                        var li = new ListItem();
                        li.Text = NBrightBuyUtils.GetItemDisplay(obj, templ, displayPrice);
                        li.Value = obj.GetXmlProperty("genxml/hidden/modelid");
                        if (li.Text != "") ddl.Items.Add(li);
                    }

                    if (ddl.Items.Count > 0)
                    {
                        if (ddl.Items.FindByValue(dataInfo.GetXmlProperty("genxml/modelid")) != null)
                            ddl.SelectedValue = dataInfo.GetXmlProperty("genxml/modelid");
                        else
                            ddl.SelectedIndex = 0;
                }
                }

            }
            catch (Exception)
            {
                ddl.Visible = false;
            }
        }

        private void Createmodeldefault(Control container, XmlNode xmlNod)
        {
            var hf = new HiddenField();
            hf.DataBinding += ModeldefaultDataBind;
            hf.ID = "modeldefault";
            container.Controls.Add(hf);
        }

        private void ModeldefaultDataBind(object sender, EventArgs e)
        {
            var hf = (HiddenField)sender;
            var container = (IDataItemContainer)hf.NamingContainer;
            try
            {
                hf.Visible = visibleStatus.DefaultIfEmpty(true).First();
                var obj = (NBrightInfo)container.DataItem;
                if (obj != null) hf.Value = obj.GetXmlProperty("genxml/models/genxml[1]/hidden/modelid");
            }
            catch (Exception)
            {
                //do nothing
            }
        }


        private String GetItemDisplay(NBrightInfo obj, String templ, Boolean displayPrices)
        {
            var isDealer = NBrightBuyUtils.IsDealer();
            var outText = templ;
            var stockOn = obj.GetXmlPropertyBool("genxml/checkbox/chkstockon");
            var stock = obj.GetXmlPropertyDouble("genxml/textbox/txtqtyremaining");
            if (stock > 0 || !stockOn)
            {
                outText = outText.Replace("{ref}", obj.GetXmlProperty("genxml/textbox/txtmodelref"));
                outText = outText.Replace("{name}", obj.GetXmlProperty("genxml/lang/genxml/textbox/txtmodelname"));
                outText = outText.Replace("{stock}", stock.ToString(""));

                if (displayPrices)
                {
                    //[TODO: add promotional calc]
                    var saleprice = obj.GetXmlPropertyDouble("genxml/textbox/txtsaleprice");
                    var price = obj.GetXmlPropertyDouble("genxml/textbox/txtunitcost");
                    var bestprice = price;
                    if (saleprice > 0 && saleprice < price) bestprice = saleprice;

                    var strprice = NBrightBuyUtils.FormatToStoreCurrency(price);
                    var strbestprice = NBrightBuyUtils.FormatToStoreCurrency(bestprice);
                    var strsaleprice = NBrightBuyUtils.FormatToStoreCurrency(saleprice);

                    var strdealerprice = "";
                    var dealerprice = obj.GetXmlPropertyDouble("genxml/textbox/txtdealercost");
                    if (isDealer)
                    {
                        strdealerprice = NBrightBuyUtils.FormatToStoreCurrency(dealerprice);
                        if (!outText.Contains("{dealerprice}") && (price > dealerprice)) strprice = strdealerprice;
                        if (dealerprice < bestprice) bestprice = dealerprice;
                    }

                    outText = outText.Replace("{price}", "(" + strprice + ")");
                    outText = outText.Replace("{dealerprice}", strdealerprice);
                    outText = outText.Replace("{bestprice}", strbestprice);
                    outText = outText.Replace("{saleprice}", strsaleprice);
                }
                else
                {
                    outText = outText.Replace("{price}", "");
                    outText = outText.Replace("{dealerprice}", "");
                    outText = outText.Replace("{bestprice}", "");
                    outText = outText.Replace("{saleprice}", "");
                }

                return outText;
            }
            return ""; // no stock so return empty string.
        }

        #endregion

        #region "orderstatus"

        private void Createorderstatusdropdown(Control container, XmlNode xmlNod)
        {
            var ddl = new DropDownList();
            if (xmlNod.Attributes != null && (xmlNod.Attributes["blank"] != null)) ddl.Attributes.Add("blank", xmlNod.Attributes["blank"].Value);
            if (xmlNod.Attributes != null && (xmlNod.Attributes["id"] != null))
                ddl.ID = xmlNod.Attributes["id"].InnerText;
            else
                ddl.ID = "orderstatus";

            ddl = (DropDownList)GenXmlFunctions.AssignByReflection(ddl, xmlNod);
            ddl.DataBinding += OrderstatusDataBind;
            container.Controls.Add(ddl);
        }

        private void OrderstatusDataBind(object sender, EventArgs e)
        {
            var ddl = (DropDownList)sender;
            var container = (IDataItemContainer)ddl.NamingContainer;
            try
            {
                ddl.Visible = visibleStatus.DefaultIfEmpty(true).First();
                if (ddl.Visible)
                {

                    var Resxpath = StoreSettings.NBrightBuyPath() + "/App_LocalResources/General.ascx.resx";
                    var orderstatuscode = DnnUtils.GetLocalizedString("orderstatus.Code", Resxpath, Utils.GetCurrentCulture());
                    var orderstatustext = DnnUtils.GetLocalizedString("orderstatus.Text", Resxpath, Utils.GetCurrentCulture());
                    if (orderstatuscode != null && orderstatustext != null)
                    {
                        if (ddl.Attributes["blank"] != null)
                        {
                            orderstatuscode = "," + orderstatuscode;
                            orderstatustext = "," + orderstatustext;
                        }

                        var aryCode = orderstatuscode.Split(',');
                        var aryText = orderstatustext.Split(',');

                        var lp = 0;
                        foreach (var c in aryCode)
                        {
                            var li = new ListItem();
                            li.Text = aryText[lp];
                            li.Value = c;
                            if (li.Text != "")
                                ddl.Items.Add(li);
                            else
                            {
                                if (lp == 0) ddl.Items.Add(li); // allow the first entry to be blank.
                            }
                            lp += 1;
                        }
                        var strValue = GenXmlFunctions.GetGenXmlValue(ddl.ID, "dropdownlist", Convert.ToString(DataBinder.Eval(container.DataItem, _databindColumn)));
                        if ((ddl.Items.FindByValue(strValue) != null))
                            ddl.SelectedValue = strValue;
                        else if (aryCode.Length > 0) ddl.SelectedIndex = 0;
                    }
                }

            }
            catch (Exception)
            {
                ddl.Visible = false;
            }
        }


        #endregion

        #region "orderstatusdisplay"

        private void Createorderstatusdisplay(Control container, XmlNode xmlNod)
        {
            var l = new Literal();
            if (xmlNod.Attributes != null && (xmlNod.Attributes["xpath"] != null)) l.Text = xmlNod.Attributes["xpath"].Value;
            l.DataBinding += OrderstatusDisplayDataBind;
            container.Controls.Add(l);
        }

        private void OrderstatusDisplayDataBind(object sender, EventArgs e)
        {
            var l = (Literal)sender;
            var container = (IDataItemContainer)l.NamingContainer;
            try
            {
                    var resxpath = StoreSettings.NBrightBuyPath() + "/App_LocalResources/General.ascx.resx";
                    var orderstatuscode = DnnUtils.GetLocalizedString("orderstatus.Code", resxpath, Utils.GetCurrentCulture());
                    var orderstatustext = DnnUtils.GetLocalizedString("orderstatus.Text", resxpath, Utils.GetCurrentCulture());
                    if (orderstatuscode != null && orderstatustext != null)
                    {
                        var aryCode = orderstatuscode.Split(',');
                        var aryText = orderstatustext.Split(',');
                        var statuslist = new Dictionary<String, String>();
                        var lp = 0;
                        foreach (var c in aryCode)
                        {
                            statuslist.Add(c,aryText[lp]);
                            lp += 1;
                        }
                        var strValue = "";
                        if (l.Text == "")
                        strValue = GenXmlFunctions.GetGenXmlValue("orderstatus", "dropdownlist", Convert.ToString(DataBinder.Eval(container.DataItem, _databindColumn)));
                        else
                        {
                            var nbi = (NBrightInfo) container.DataItem;
                            strValue = nbi.GetXmlProperty(l.Text);
                        }
                        l.Text = "";
                        if (statuslist.ContainsKey(strValue))
                            l.Text = "<span class='orderstatus orderstatus" + strValue + "'>" + statuslist[strValue] + "</span>";
                        else
                        {
                            if (strValue != "") l.Text = "<span class='orderstatus orderstatus" + strValue + "'>" + strValue + "</span>";
                        }
                    }
            }
            catch (Exception)
            {
                l.Visible = false;
            }
        }


        #endregion

        #region "modelstatus"

        private void Createmodelstatusdropdown(Control container, XmlNode xmlNod)
        {
            var ddl = new DropDownList();
            if (xmlNod.Attributes != null && (xmlNod.Attributes["blank"] != null)) ddl.Attributes.Add("blank", xmlNod.Attributes["blank"].Value);
            if (xmlNod.Attributes != null && (xmlNod.Attributes["id"] != null))
                ddl.ID = xmlNod.Attributes["id"].InnerText;
            else
                ddl.ID = "modelstatus";

            ddl = (DropDownList)GenXmlFunctions.AssignByReflection(ddl, xmlNod);
            ddl.DataBinding += ModelstatusDataBind;
            container.Controls.Add(ddl);
        }

        private void ModelstatusDataBind(object sender, EventArgs e)
        {
            var ddl = (DropDownList)sender;
            var container = (IDataItemContainer)ddl.NamingContainer;
            try
            {
                ddl.Visible = visibleStatus.DefaultIfEmpty(true).First();
                if (ddl.Visible)
                {

                    var resxpath = StoreSettings.NBrightBuyPath() + "/App_LocalResources/General.ascx.resx";
                    var orderstatuscode = DnnUtils.GetLocalizedString("modelstatus.Code", resxpath, Utils.GetCurrentCulture());
                    var orderstatustext = DnnUtils.GetLocalizedString("modelstatus.Text", resxpath, Utils.GetCurrentCulture());
                    if (orderstatuscode != null && orderstatustext != null)
                    {
                        if (ddl.Attributes["blank"] != null)
                        {
                            orderstatuscode = "," + orderstatuscode;
                            orderstatustext = "," + orderstatustext;
                        }

                        var aryCode = orderstatuscode.Split(',');
                        var aryText = orderstatustext.Split(',');

                        var lp = 0;
                        foreach (var c in aryCode)
                        {
                            var li = new ListItem();
                            li.Text = aryText[lp];
                            li.Value = c;
                            if (li.Text != "")
                                ddl.Items.Add(li);
                            else
                            {
                                if (lp == 0) ddl.Items.Add(li); // allow the first entry to be blank.
                            }
                            lp += 1;
                        }
                        var strValue = GenXmlFunctions.GetGenXmlValue(ddl.ID, "dropdownlist", Convert.ToString(DataBinder.Eval(container.DataItem, _databindColumn)));
                        if ((ddl.Items.FindByValue(strValue) != null))
                            ddl.SelectedValue = strValue;
                        else if (aryCode.Length > 0) ddl.SelectedIndex = 0;
                    }
                }

            }
            catch (Exception)
            {
                ddl.Visible = false;
            }
        }


        #endregion

        #region "ProductCount"

        private void CreateProductCount(Control container, XmlNode xmlNod)
        {
            if (xmlNod.Attributes != null && (xmlNod.Attributes["modulekey"] != null))
            {
                var l = new Literal();
                l.DataBinding += ProductCountDataBind;
                l.Text = xmlNod.Attributes["modulekey"].Value;
                container.Controls.Add(l);
            }
        }

        private void ProductCountDataBind(object sender, EventArgs e)
        {
            var l = (Literal)sender;
            try
            {
                var navdata = new NavigationData(PortalSettings.Current.PortalId, l.Text);
                l.Text = navdata.RecordCount;
                l.Visible = visibleStatus.DefaultIfEmpty(true).First();
            }
            catch (Exception ex)
            {
                l.Text = ex.ToString();
            }
        }


        #endregion

        #region "Docs"

        private void CreateProductDocLink(Control container, XmlNode xmlNod)
        {
            if (xmlNod.Attributes != null && (xmlNod.Attributes["index"] != null))
            {
                if (Utils.IsNumeric(xmlNod.Attributes["index"].Value)) // must have a index
                {
                    var cmd = new LinkButton();
                    cmd = (LinkButton)GenXmlFunctions.AssignByReflection(cmd, xmlNod);
                    cmd.Attributes.Add("index",xmlNod.Attributes["index"].Value);
                    cmd.DataBinding += ProductDocLinkDataBind;
                    container.Controls.Add(cmd);
                }
            }
        }

        private void ProductDocLinkDataBind(object sender, EventArgs e)
        {
            var cmd = (LinkButton)sender;
            var container = (IDataItemContainer)cmd.NamingContainer;
            try
            {
                cmd.Visible = visibleStatus.DefaultIfEmpty(true).First();
                if (cmd.Visible)
                {
                    var index = cmd.Attributes["index"];
                    cmd.Attributes.Remove("index");

                    var objInfo = (NBrightInfo) container.DataItem;
                    cmd.CommandName = "docdownload";
                    if (cmd.Text == "")
                    {
                        var nodDesc = objInfo.XMLDoc.SelectSingleNode("genxml/lang/genxml/docs/genxml[" + index + "]/textbox/txtdocdesc");
                        if (nodDesc != null) cmd.Text = nodDesc.InnerText;
                    }
                    if (cmd.ToolTip == "")
                    {
                        var nodName = objInfo.XMLDoc.SelectSingleNode("genxml/docs/genxml[" + index + "]/textbox/txtfilename");
                        if (nodName != null) cmd.ToolTip = nodName.InnerText;
                    }
                    cmd.CommandArgument = objInfo.ItemID.ToString("") + ":" + index;

                    cmd.Visible = true;
                    var nodPurchase = objInfo.XMLDoc.SelectSingleNode("genxml/docs/genxml[" + index + "]/checkbox/chkpurchase");
                    if (nodPurchase != null && nodPurchase.InnerText == "True")
                    {
                        //[TODO: work out purchase document logic]                        
                        //if (NBrightBuyV2Utils.DocIsPurchaseOnlyByDocId(Convert.ToInt32(nodDocId.InnerText)))
                        //{
                        //    cmd.Visible = false;
                        //    var role = "Manager";
                        //    if (!String.IsNullOrEmpty(_settings["manager.role"])) role = _settings["manager.role"];
                        //    var uInfo = UserController.Instance.GetCurrentUserInfo();
                        //    if (NBrightBuyV2Utils.DocHasBeenPurchasedByDocId(uInfo.UserID, Convert.ToInt32(nodDocId.InnerText)) || CmsProviderManager.Default.IsInRole(role)) cmd.Visible = true;
                        //}
                    }
                }

            }
            catch (Exception)
            {
                cmd.Visible = false;
            }
        }


        #endregion

        #region "Related Products"

        private void CreateRelatedlist(Control container, XmlNode xmlNod)
        {
            var lc = new Literal();
            if (xmlNod.Attributes != null && (xmlNod.Attributes["template"] != null))
            {
                lc.Text = xmlNod.Attributes["template"].Value;
            }
            lc.DataBinding += RelatedlistDataBind;
            container.Controls.Add(lc);
        }

        private void RelatedlistDataBind(object sender, EventArgs e)
        {
            var lc = (Literal)sender;
            var container = (IDataItemContainer)lc.NamingContainer;
            try
            {
                var strOut = "";
                lc.Visible = visibleStatus.DefaultIfEmpty(true).First();
                if (lc.Visible)
                {

                    var id = Convert.ToString(DataBinder.Eval(container.DataItem, "ItemId"));
                    var templName = lc.Text;
                    if (Utils.IsNumeric(id) && (templName != ""))
                    {
                        var modCtrl = new NBrightBuyController();
                        var rpTempl = modCtrl.GetTemplateData(-1, templName, Utils.GetCurrentCulture(), _settings, StoreSettings.Current.DebugMode); 

                        //remove templName from template, so we don't get a loop.
                        if (rpTempl.Contains('"' + templName + '"')) rpTempl = rpTempl.Replace(templName, "");
                        //build list
                        var objInfo = (NBrightInfo)container.DataItem;

                        List<NBrightInfo> objL = null;
                        var strCacheKey = Utils.GetCurrentCulture() + "RelatedList*" + objInfo.ItemID;
                        if (!StoreSettings.Current.DebugMode) objL = (List<NBrightInfo>)Utils.GetCache(strCacheKey);
                        if (objL == null)
                        {
                            var prodData = ProductUtils.GetProductData(objInfo.ItemID, Utils.GetCurrentCulture());
                            objL = prodData.GetRelatedProducts();
                            if (!StoreSettings.Current.DebugMode) NBrightBuyUtils.SetModCache(-1, strCacheKey, objL);
                        }
                        // render repeater
                        try
                        {
                            var itemTemplate = NBrightBuyUtils.GetGenXmlTemplate(rpTempl, _settings, PortalSettings.Current.HomeDirectory, visibleStatus);
                            strOut = GenXmlFunctions.RenderRepeater(objL, itemTemplate);
                        }
                        catch (Exception exc)
                        {
                            strOut = "ERROR: NOTE: sub rendered templates CANNOT contain postback controls.<br/>" + exc;
                        }
                    }
                }
                lc.Text = strOut;

            }
            catch (Exception)
            {
                lc.Text = "";
            }
        }


        #endregion

        #region "Qty Field"

        private void CreateQtyField(Control container, XmlNode xmlNod)
        {
            var txt = new TextBox();
            txt = (TextBox)GenXmlFunctions.AssignByReflection(txt, xmlNod);
            txt.ID = "selectedaddqty";
            txt.DataBinding += QtyFieldDataBind;
            container.Controls.Add(txt);
        }

        private void QtyFieldDataBind(object sender, EventArgs e)
        {
            var txt = (TextBox)sender;
            txt.Visible = visibleStatus.DefaultIfEmpty(true).First();
        }

        #endregion

        #region "Model Qty Field"

        private void CreateModelQtyField(Control container, XmlNode xmlNod)
        {
            var txt = new TextBox();
            txt = (TextBox)GenXmlFunctions.AssignByReflection(txt, xmlNod);
            txt.ID = "selectedmodelqty";
            txt.DataBinding += ModelQtyFieldDataBind;
            container.Controls.Add(txt);
            var hid = new HiddenField();
            hid.ID = "modelid";
            hid.DataBinding += ModelHiddenFieldDataBind;
            container.Controls.Add(hid);
            
        }

        private void ModelHiddenFieldDataBind(object sender, EventArgs e)
        {
            var txt = (HiddenField)sender;
            var container = (IDataItemContainer)txt.NamingContainer;
            var strXml = DataBinder.Eval(container.DataItem, _databindColumn).ToString();
            var nbi = new NBrightInfo();
            nbi.XMLData = strXml;
            txt.Value = nbi.GetXmlProperty("genxml/hidden/modelid");
            txt.Visible = visibleStatus.DefaultIfEmpty(true).First();
        }

        private void ModelQtyFieldDataBind(object sender, EventArgs e)
        {
            var txt = (TextBox)sender;
            var container = (IDataItemContainer)txt.NamingContainer;
            var strXml = DataBinder.Eval(container.DataItem, _databindColumn).ToString();
            var nbi = new NBrightInfo();
            nbi.XMLData = strXml;
            if (nbi.GetXmlProperty("genxml/hidden/modelid") == "") txt.Text = "ERR! - MODELQTY can only be used on modellist template";
            txt.Attributes.Add("modelid", nbi.GetXmlProperty("genxml/hidden/modelid"));
            txt.Visible = visibleStatus.DefaultIfEmpty(true).First();
        }

        #endregion

        #region "create EditLink/URL control"

        private void CreateEditLink(Control container, XmlNode xmlNod)
        {
            var lk = new HyperLink();
            lk = (HyperLink)GenXmlFunctions.AssignByReflection(lk, xmlNod);

            // if we are using xsl then we might not have a databind ItemId (if the xsl is in the header loop).  So pass it in here, via the xsl, so we can use it in the link. 
            if (xmlNod.Attributes != null && (xmlNod.Attributes["itemid"] != null))
            {
                lk.NavigateUrl = xmlNod.Attributes["itemid"].InnerXml;
            }
            else
            {
                lk.NavigateUrl = "";
            }

            lk.DataBinding += EditLinkDataBinding;
            container.Controls.Add(lk);
        }

        private void EditLinkDataBinding(object sender, EventArgs e)
        {
            var lk = (HyperLink)sender;
            var container = (IDataItemContainer)lk.NamingContainer;
            try
            {
                lk.Visible = visibleStatus.DefaultIfEmpty(true).First();

                var entryid = Convert.ToString(DataBinder.Eval(container.DataItem, "ItemID"));

                if (lk.NavigateUrl != "") entryid = lk.NavigateUrl; // use the itemid passed in (XSL loop in display header)

                var url = "Unable to find BackOffice Setting, go into Back Office settings and save.";
                if (Utils.IsNumeric(entryid) && StoreSettings.Current.GetInt("backofficetabid") > 0)
                {
                    var paramlist = new string[4];
                    paramlist[1] = "eid=" + entryid;
                    paramlist[2] = "ctrl=products";
                    paramlist[2] = "language=" + Utils.GetCurrentCulture();
                    if (_settings != null && _settings.ContainsKey("currenttabid")) paramlist[3] = "rtntab=" + _settings["currenttabid"];
                    if (_settings != null && _settings.ContainsKey("moduleid")) paramlist[3] = "rtnmid=" + _settings["moduleid"];
                    var urlpage = Utils.RequestParam(HttpContext.Current, "page");
                    if (urlpage.Trim() != "")
                    {
                        NBrightBuyUtils.IncreaseArray(ref paramlist, 1);
                        paramlist[paramlist.Length - 1] = "PageIndex=" + urlpage.Trim();
                    }
                    var urlcatid = Utils.RequestParam(HttpContext.Current, "catid");
                    if (urlcatid.Trim() != "")
                    {
                        NBrightBuyUtils.IncreaseArray(ref paramlist, 1);
                        paramlist[paramlist.Length - 1] = "catid=" + urlcatid.Trim();
                    }
                    url = Globals.NavigateURL(StoreSettings.Current.GetInt("backofficetabid"), "", paramlist);                    
                }
                lk.NavigateUrl = url;
            }
            catch (Exception ex)
            {
                lk.Text = ex.ToString();
            }
        }


        private void CreateEditUrl(Control container, XmlNode xmlNod)
        {
            var l = new Literal();
            if (xmlNod.Attributes != null)
            {
                var i = "";
                if (xmlNod.Attributes["itemid"] != null) i = xmlNod.Attributes["itemid"].InnerText; // for xsl
                l.Text = i;
            }
            l.DataBinding += EditUrlDataBinding;
            container.Controls.Add(l);
        }

        private void EditUrlDataBinding(object sender, EventArgs e)
        {
            var l = (Literal)sender;
            var container = (IDataItemContainer)l.NamingContainer;
            try
            {
                //set a default url

                l.Visible = visibleStatus.DefaultIfEmpty(true).First();
                var entryid = l.Text;
                if (entryid == "") entryid = Convert.ToString(DataBinder.Eval(container.DataItem, "ItemID"));
                var url = "Unable to find BackOffice Setting, go into Back Office settings and save.";
                if (Utils.IsNumeric(entryid) && StoreSettings.Current.GetInt("backofficetabid") > 0)
                {
                    var paramlist = new string[4];
                    paramlist[1] = "eid=" + entryid;
                    paramlist[2] = "ctrl=products";
                    if (_settings != null && _settings.ContainsKey("currenttabid")) paramlist[3] = "rtntab=" + _settings["currenttabid"];
                    if (_settings != null && _settings.ContainsKey("moduleid")) paramlist[3] = "rtnmid=" + _settings["moduleid"];
                    var urlpage = Utils.RequestParam(HttpContext.Current, "page");
                    if (urlpage.Trim() != "")
                    {
                        NBrightBuyUtils.IncreaseArray(ref paramlist, 1);
                        paramlist[paramlist.Length - 1] = "PageIndex=" + urlpage.Trim();
                    }
                    var urlcatid = Utils.RequestParam(HttpContext.Current, "catid");
                    if (urlcatid.Trim() != "")
                    {
                        NBrightBuyUtils.IncreaseArray(ref paramlist, 1);
                        paramlist[paramlist.Length - 1] = "catid=" + urlcatid.Trim();
                    }
                    url = Globals.NavigateURL(StoreSettings.Current.GetInt("backofficetabid"), "", paramlist);
                }
               
                l.Text = url;

            }
            catch (Exception ex)
            {
                l.Text = ex.ToString();
            }
        }


        #endregion

        #region "Sale Price"

        private void CreateSalePrice(Control container, XmlNode xmlNod)
        {
            var l = new Literal();
            l.DataBinding += SalePriceDataBinding;
            l.Text = "";
            container.Controls.Add(l);
        }

        private void SalePriceDataBinding(object sender, EventArgs e)
        {
            var l = (Literal)sender;
            var container = (IDataItemContainer)l.NamingContainer;
            try
            {
                l.Text = "";
                l.Visible = visibleStatus.DefaultIfEmpty(true).First();
                var sp = NBrightBuyUtils.GetSalePrice((NBrightInfo)container.DataItem);
                if (Utils.IsNumeric(sp))
                {
                    Double v = -1;
                    if (Utils.IsNumeric(XmlConvert.DecodeName(sp)))
                    {
                        v = Convert.ToDouble(XmlConvert.DecodeName(sp), CultureInfo.GetCultureInfo("en-US"));
                    }
                    if (v >= 0) l.Text = NBrightBuyUtils.FormatToStoreCurrency(v);
                }
            }
            catch (Exception ex)
            {
                l.Text = ex.ToString();
            }
        }


        #endregion

        #region "Dealer Price"

        private void CreateDealerPrice(Control container, XmlNode xmlNod)
        {
            var l = new Literal();
            l.DataBinding += DealerPriceDataBinding;
            l.Text = "";
            container.Controls.Add(l);
        }

        private void DealerPriceDataBinding(object sender, EventArgs e)
        {
            var l = (Literal)sender;
            var container = (IDataItemContainer)l.NamingContainer;
            try
            {
                l.Text = "";
                l.Visible = visibleStatus.DefaultIfEmpty(true).First();
                var sp = NBrightBuyUtils.GetDealerPrice((NBrightInfo)container.DataItem);
                if (Utils.IsNumeric(sp))
                {
                    Double v = -1;
                    if (Utils.IsNumeric(XmlConvert.DecodeName(sp)))
                    {
                        v = Convert.ToDouble(XmlConvert.DecodeName(sp), CultureInfo.GetCultureInfo("en-US"));
                    }
                    if (v >= 0) l.Text = NBrightBuyUtils.FormatToStoreCurrency(v);
                }
            }
            catch (Exception ex)
            {
                l.Text = ex.ToString();
            }
        }


        #endregion

        #region "CreateCurrencyIsoCode"

        private void CreateCurrencyIsoCode(Control container, XmlNode xmlNod)
        {
            var l = new Literal();
            l.DataBinding += CreateCurrencyIsoCodeDataBinding;
            l.Text = "";
            container.Controls.Add(l);
        }

        private void CreateCurrencyIsoCodeDataBinding(object sender, EventArgs e)
        {
            var l = (Literal)sender;
            var container = (IDataItemContainer)l.NamingContainer;
            try
            {
                l.Text = "";
                l.Visible = visibleStatus.DefaultIfEmpty(true).First();
                l.Text = NBrightBuyUtils.GetCurrencyIsoCode();
            }
            catch (Exception ex)
            {
                l.Text = ex.ToString();
            }
        }


        #endregion

        #region "From Price"

        private void CreateFromPrice(Control container, XmlNode xmlNod)
        {
            var l = new Literal();
            l.DataBinding += FromPriceDataBinding;
            l.Text = "";
            container.Controls.Add(l);
        }

        private void FromPriceDataBinding(object sender, EventArgs e)
        {
            var l = (Literal)sender;
            var container = (IDataItemContainer)l.NamingContainer;
            try
            {
                l.Text = "";
                l.Visible = visibleStatus.DefaultIfEmpty(true).First();
                var sp = NBrightBuyUtils.GetFromPrice((NBrightInfo)container.DataItem);
                if (Utils.IsNumeric(sp))
                {
                    Double v = -1;
                    if (Utils.IsNumeric(XmlConvert.DecodeName(sp)))
                    {
                        v = Convert.ToDouble(XmlConvert.DecodeName(sp), CultureInfo.GetCultureInfo("en-US"));
                    }
                    if (v >= 0) l.Text = NBrightBuyUtils.FormatToStoreCurrency(v);
                }
            }
            catch (Exception ex)
            {
                l.Text = ex.ToString();
            }
        }


        #endregion

        #region "Best Price"

        private void CreateBestPrice(Control container, XmlNode xmlNod)
        {
            var l = new Literal();
            l.DataBinding += BestPriceDataBinding;
            l.Text = "";
            container.Controls.Add(l);
        }

        private void BestPriceDataBinding(object sender, EventArgs e)
        {
            var l = (Literal)sender;
            var container = (IDataItemContainer)l.NamingContainer;
            try
            {
                l.Text = "";
                l.Visible = visibleStatus.DefaultIfEmpty(true).First();
                var sp = NBrightBuyUtils.GetBestPrice((NBrightInfo)container.DataItem);
                if (Utils.IsNumeric(sp))
                {
                    Double v = -1;
                    if (Utils.IsNumeric(XmlConvert.DecodeName(sp)))
                    {
                        v = Convert.ToDouble(XmlConvert.DecodeName(sp), CultureInfo.GetCultureInfo("en-US"));
                    }
                    if (v >= 0) l.Text = NBrightBuyUtils.FormatToStoreCurrency(v);
                }
            }
            catch (Exception ex)
            {
                l.Text = ex.ToString();
            }
        }


        #endregion

        #region "CartQtyTextbox"

        private void CreateCartQtyTextbox(Control container, XmlNode xmlNod)
        {
            var txt = new TextBox { Text = "" };

            txt = (TextBox)GenXmlFunctions.AssignByReflection(txt, xmlNod);

            if (xmlNod.Attributes != null && (xmlNod.Attributes["text"] != null))
            {
                txt.Text = xmlNod.Attributes["text"].InnerXml;
            }

            txt.DataBinding += CartQtyTextDataBinding;
            container.Controls.Add(txt);
        }

        private void CartQtyTextDataBinding(object sender, EventArgs e)
        {
            var txt = (TextBox)sender;
            var container = (IDataItemContainer)txt.NamingContainer;

            try
            {
                txt.Visible = visibleStatus.DefaultIfEmpty(true).First();
                if (txt.Width == 0) txt.Visible = false; // always hide if we have a width of zero.
                else
                {
                    var strXML = Convert.ToString(DataBinder.Eval(container.DataItem,"XMLData" ));
                    var nbInfo = new NBrightInfo();
                    nbInfo.XMLData = strXML;
                    txt.Text = nbInfo.GetXmlProperty("genxml/qty");
                }
            }
            catch (Exception)
            {
                //do nothing
            }
        }


        #endregion

        #region "CartEmailAddress"

        private void CreateCartEmailAddress(Control container, XmlNode xmlNod)
        {
            var txt = new TextBox { Text = "" };

            txt = (TextBox)GenXmlFunctions.AssignByReflection(txt, xmlNod);
            txt.ID = "cartemailaddress";

            if (xmlNod.Attributes != null && (xmlNod.Attributes["required"] != null))
            {
                txt.Attributes.Add("required", xmlNod.Attributes["required"].InnerXml);
            }

            if (xmlNod.Attributes != null && (xmlNod.Attributes["datatype"] != null) && xmlNod.Attributes["datatype"].InnerXml == "email")
            {
                txt.Attributes.Add("type", "email");
            }

            if (xmlNod.Attributes != null && (xmlNod.Attributes["text"] != null))
            {
                txt.Text = xmlNod.Attributes["text"].InnerXml;
            }

            txt.DataBinding += CartEmailAddressDataBinding;
            container.Controls.Add(txt);
        }

        private void CartEmailAddressDataBinding(object sender, EventArgs e)
        {
            var txt = (TextBox)sender;
            var container = (IDataItemContainer)txt.NamingContainer;

            try
            {
                txt.Visible = visibleStatus.DefaultIfEmpty(true).First();
                if (txt.Width == 0) txt.Visible = false; // always hide if we have a width of zero.
                else
                {
                    var strXML = Convert.ToString(DataBinder.Eval(container.DataItem, "XMLData"));
                    var nbInfo = new NBrightInfo();
                    nbInfo.XMLData = strXML;
                    txt.Text = nbInfo.GetXmlProperty("genxml/textbox/cartemailaddress");
                    if (txt.Text == "")
                    {
                        var usr = UserController.Instance.GetCurrentUserInfo();
                        if (usr != null && usr.UserID > 0) txt.Text = usr.Email;
                    }
                }
            }
            catch (Exception)
            {
                //do nothing
            }
        }


        #endregion

        #region "groups"


        private void Creategroupdropdown(Control container, XmlNode xmlNod)
        {
            var rbl = new DropDownList();
            if (xmlNod.Attributes != null && (xmlNod.Attributes["blank"] != null))
            {
                rbl.Attributes.Add("blank", xmlNod.Attributes["blank"].Value);
            }
            if (xmlNod.Attributes != null && (xmlNod.Attributes["groupsonly"] != null))
            {
                rbl.Attributes.Add("groupsonly", xmlNod.Attributes["groupsonly"].Value);
            }
            if (xmlNod.Attributes != null && (xmlNod.Attributes["grouptype"] != null))
            {
                rbl.Attributes.Add("grouptype", xmlNod.Attributes["grouptype"].Value);
            }
            rbl = (DropDownList)GenXmlFunctions.AssignByReflection(rbl, xmlNod);
            rbl.DataBinding += GroupdropdownDataBind;
            if (xmlNod.Attributes != null && (xmlNod.Attributes["id"] != null))
                rbl.ID = xmlNod.Attributes["id"].InnerText;
            else
                rbl.ID = "ddlGroupsel";
            container.Controls.Add(rbl);
        }

        private void GroupdropdownDataBind(object sender, EventArgs e)
        {
            var ddl = (DropDownList)sender;
            var container = (IDataItemContainer)ddl.NamingContainer;
            try
            {
                ddl.Visible = visibleStatus.DefaultIfEmpty(true).First();
                if (ddl.Visible)
                {
                    var grptype = "1";
                    if (ddl.Attributes["grouptype"] != null)
                    {
                        grptype = ddl.Attributes["grouptype"];
                    }

                    var objL = NBrightBuyUtils.GetCategoryGroups(Utils.GetCurrentCulture(),true, grptype);

                    if (ddl.Attributes["blank"] != null)
                    {
                        var li = new ListItem();
                        li.Text = ddl.Attributes["blank"];
                        li.Value = "0";
                        ddl.Items.Add(li);
                        ddl.Attributes.Remove("blank");
                    }

                    var gref = "";
                    if (ddl.Attributes["groupsonly"] != null) gref = "cat";

                    foreach (var obj in objL)
                    {
                        if (obj.GetXmlProperty("genxml/textbox/groupref") != gref)
                        {
                            var li = new ListItem();
                            li.Text = obj.GetXmlProperty("genxml/lang/genxml/textbox/groupname");
                            li.Value = obj.GetXmlProperty("genxml/textbox/groupref");
                            if (li.Text == "") li.Text = li.Value;
                            ddl.Items.Add(li);
                        }
                    }
                    var strValue = GenXmlFunctions.GetGenXmlValue(ddl.ID, "dropdownlist", Convert.ToString(DataBinder.Eval(container.DataItem, _databindColumn)));
                    if ((ddl.Items.FindByValue(strValue) != null)) ddl.SelectedValue = strValue;
                }

            }
            catch (Exception)
            {
                ddl.Visible = false;
            }
        }



        #endregion

        #region "create Image control"

        private void CreateImage(Control container, XmlNode xmlNod)
        {
            var img = new Image();

            img = (Image)GenXmlFunctions.AssignByReflection(img, xmlNod);
            if (xmlNod.Attributes != null && (xmlNod.Attributes["xpath"] != null))
            {
                img.ImageUrl = xmlNod.Attributes["xpath"].InnerXml; // use imageurl to get the xpath of the image
            }
            if (xmlNod.Attributes != null && (xmlNod.Attributes["thumb"] != null))
            {
                img.Attributes.Add("thumb", xmlNod.Attributes["thumb"].InnerXml);
            }

            img.DataBinding += ImageDataBinding;
            container.Controls.Add(img);
        }

        private void ImageDataBinding(object sender, EventArgs e)
        {
            var img = (Image)sender;
            var container = (IDataItemContainer)img.NamingContainer;
            try
            {
                img.Visible = visibleStatus.DefaultIfEmpty(true).First();
                var src = "";

                XmlNode nod = GenXmlFunctions.GetGenXmLnode(DataBinder.Eval(container.DataItem, _databindColumn).ToString(), img.ImageUrl);
                if ((nod != null))
                {
                    src += nod.InnerText;
                }

                var altpath = img.ImageUrl.Replace("genxml/hidden/hid", "genxml/textbox/txt");
                nod = GenXmlFunctions.GetGenXmLnode(DataBinder.Eval(container.DataItem, _databindColumn).ToString(), altpath);
                if ((nod != null))
                {
                    img.AlternateText = nod.InnerText;
                }

                if (img.Attributes["thumb"] == null || img.Attributes["thumb"] == "")
                {
                    img.ImageUrl = src;
                }
                else
                {
                    var w = ImgUtils.GetThumbWidth(img.Attributes["thumb"]).ToString("");
                    var h = ImgUtils.GetThumbHeight(img.Attributes["thumb"]).ToString("");
                    if (w == "-1") w = "0";
                    if (h == "-1") h = "0";
                    img.Attributes.Remove("thumb");
                    img.ImageUrl = StoreSettings.NBrightBuyPath() + "/NBrightThumb.ashx?w=" + w + "&h=" + h + "&src=" + src;
                }

            }
            catch (Exception ex)
            {
                // no error
            }
        }

        #endregion

        #region "Country, Region and culture"

       
        private void Createculturecodedropdown(Control container, XmlNode xmlNod)
        {
            var rbl = new DropDownList();
            if (xmlNod.Attributes != null && (xmlNod.Attributes["blank"] != null))
            {
                rbl.Attributes.Add("blank", xmlNod.Attributes["blank"].Value);
            }
            rbl = (DropDownList) GenXmlFunctions.AssignByReflection(rbl, xmlNod);
            rbl.DataBinding += CultureCodeDropdownDataBind;
            if (xmlNod.Attributes != null && (xmlNod.Attributes["id"] != null))
            {
                rbl.ID = xmlNod.Attributes["id"].InnerText;
                container.Controls.Add(rbl);
            }
        }

        private void CultureCodeDropdownDataBind(object sender, EventArgs e)
        {
            

            var ddl = (DropDownList)sender;
            var container = (IDataItemContainer)ddl.NamingContainer;
            try
            {
                ddl.Visible = visibleStatus.DefaultIfEmpty(true).First();
                if (ddl.Visible)
                {

                    //var countries = CultureInfo.GetCultures(CultureTypes.AllCultures).Except(CultureInfo.GetCultures(CultureTypes.SpecificCultures));
                    var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
                    var dnnCultureCode = DnnUtils.GetCountryCodeList();

                    var joinItems = (from d1 in cultures where dnnCultureCode.ContainsKey(d1.Name.Split('-').Last()) select d1).ToList<CultureInfo>();


                    if (ddl.Attributes["blank"] != null)
                    {
                        var li = new ListItem();
                        li.Text = ddl.Attributes["blank"];
                        li.Value = "0";
                        ddl.Items.Add(li);
                        ddl.Attributes.Remove("blank");
                    }

                    foreach (var obj in joinItems)
                    {
                        var li = new ListItem();
                        li.Text = obj.DisplayName + " : " + obj.Name;
                        li.Value = obj.Name;
                        ddl.Items.Add(li);
                    }
                    var strValue = GenXmlFunctions.GetGenXmlValue(ddl.ID, "dropdownlist", Convert.ToString(DataBinder.Eval(container.DataItem, _databindColumn)));
                    if (strValue == "") strValue = Utils.GetCurrentCulture();
                    if ((ddl.Items.FindByValue(strValue) != null)) ddl.SelectedValue = strValue;
                }

            }
            catch (Exception)
            {
                ddl.Visible = false;
            }
        }

        private void CreateCountryDropDownList(Control container, XmlNode xmlNod)
        {
            var rbl = new DropDownList();
            if (xmlNod.Attributes != null && (xmlNod.Attributes["blank"] != null))
            {
                rbl.Attributes.Add("blank", xmlNod.Attributes["blank"].Value);
            }
            rbl = (DropDownList)GenXmlFunctions.AssignByReflection(rbl, xmlNod);
            rbl.DataBinding += CountryCodeDropdownDataBind;
            if (xmlNod.Attributes != null && (xmlNod.Attributes["id"] != null))
            {
                rbl.ID = xmlNod.Attributes["id"].InnerText;
                container.Controls.Add(rbl);
            }
        }

        private void CountryCodeDropdownDataBind(object sender, EventArgs e)
        {

            var ddl = (DropDownList)sender;
            var container = (IDataItemContainer)ddl.NamingContainer;
            try
            {
                ddl.Visible = visibleStatus.DefaultIfEmpty(true).First();
                if (ddl.Visible)
                {

                    if (ddl.Attributes["blank"] != null)
                    {
                        var li = new ListItem();
                        li.Text = ddl.Attributes["blank"];
                        li.Value = "";
                        ddl.Items.Add(li);
                        ddl.Attributes.Remove("blank");
                    }
                    
                    var tList = NBrightBuyUtils.GetCountryList();
                    foreach (var tItem in tList)
                    {
                        var li = new ListItem();
                        li.Text = tItem.Value;
                        li.Value = tItem.Key;
                        ddl.Items.Add(li);
                    }

                    var strValue = GenXmlFunctions.GetGenXmlValue(ddl.ID, "dropdownlist", Convert.ToString(DataBinder.Eval(container.DataItem, _databindColumn)));
                    if (strValue == "") strValue = Utils.GetCurrentCulture();
                    if ((ddl.Items.FindByValue(strValue) != null)) ddl.SelectedValue = strValue;

                }

            }
            catch (Exception)
            {
                ddl.Visible = false;
            }
        }

        private void CreateCountryList(Control container, XmlNode xmlNod)
        {
            var l = new Literal();
            l.DataBinding += CreateCountryListDataBind;
            if (xmlNod.Attributes != null && (xmlNod.Attributes["id"] != null))
            {
                l.ID = xmlNod.Attributes["id"].InnerText;
            }
            container.Controls.Add(l);
        }

        private void CreateCountryListDataBind(object sender, EventArgs e)
        {

            var l = (Literal)sender;
            var container = (IDataItemContainer)l.NamingContainer;
            try
            {
                l.Visible = visibleStatus.DefaultIfEmpty(true).First();
                if (l.Visible)
                {
                    l.Text = "<ul id='" + l.ID + "'>";
                    var tList = NBrightBuyUtils.GetCountryList();
                    foreach (var tItem in tList)
                    {
                        l.Text += "<li value='" + tItem.Key + "'>" + tItem.Value + "</li>";
                    }
                    l.Text += "</ul>";

                }

            }
            catch (Exception)
            {
                l.Visible = false;
            }
        }


        private void CreateEditFlag(Control container, XmlNode xmlNod)
        {
            var lc = new Literal();
            var size = "16";
            if (xmlNod.Attributes != null && (xmlNod.Attributes["size"] != null)) size = xmlNod.Attributes["size"].Value;
            lc.Text = "<img src='/DnnImageHandler.ashx?mode=file&file=/images/flags/" + StoreSettings.Current.EditLanguage + ".gif&h=" + size + "'/>";
            lc.DataBinding += EditFlagDataBind;
            container.Controls.Add(lc);
        }

        private void EditFlagDataBind(object sender, EventArgs e)
        {
            var lc = (Literal)sender;
            lc.Visible = visibleStatus.DefaultIfEmpty(true).First();
        }

        private void CreateSelectLangaugeButton(Control container, XmlNode xmlNod)
        {
            var cmd = new EditLanguage();
            cmd = (EditLanguage)GenXmlFunctions.AssignByReflection(cmd, xmlNod);
            cmd.DataBinding += SelectLangaugeDataBind;
            container.Controls.Add(cmd);
        }

        private void CreateCurrentEditCulture(Control container, XmlNode xmlNod)
        {
            var hid = new HtmlGenericControl("input");
            if (xmlNod.Attributes != null && (xmlNod.Attributes["id"] != null))
                hid.ID = xmlNod.Attributes["id"].InnerXml.ToLower();
            else
                hid.Attributes.Add("id", "lang");
            hid.Attributes.Add("type", "hidden");
            hid.Attributes.Add("value", StoreSettings.Current.EditLanguage);
            container.Controls.Add(hid);
        }

        private void CreateCurrentLang(Control container, XmlNode xmlNod)
        {
            var hid = new HtmlGenericControl("input");
            if (xmlNod.Attributes != null && (xmlNod.Attributes["id"] != null))
                hid.ID = xmlNod.Attributes["id"].InnerXml.ToLower();
            else
                hid.Attributes.Add("id", "currentlang");
            hid.Attributes.Add("type", "hidden");
            hid.Attributes.Add("value", Utils.GetCurrentCulture());
            container.Controls.Add(hid);
        }

        private void SelectLangaugeDataBind(object sender, EventArgs e)
        {
            var lc = (EditLanguage)sender;
            lc.Visible = visibleStatus.DefaultIfEmpty(true).First();
        }

        private void CreateRegionControl(Control container, XmlNode xmlNod)
        {
            var rbl = new DropDownList();
            if (xmlNod.Attributes != null && (xmlNod.Attributes["blank"] != null)) rbl.Attributes.Add("blank", xmlNod.Attributes["blank"].Value);
            rbl = (DropDownList)GenXmlFunctions.AssignByReflection(rbl, xmlNod);
            rbl.DataBinding += RegionControlDataBind;

            var txt = new TextBox();
            txt.DataBinding += RegionControlDataBind;

            if (xmlNod.Attributes != null && (xmlNod.Attributes["id"] != null))
            {
                rbl.ID = xmlNod.Attributes["id"].InnerText;
                container.Controls.Add(rbl);

                txt.ID = "txt" + xmlNod.Attributes["id"].InnerText;
                container.Controls.Add(txt);
            }


        }

        private void RegionControlDataBind(object sender, EventArgs e)
        {
            if (sender is DropDownList)
            {
                var ddl = (DropDownList)sender;
                var container = (IDataItemContainer)ddl.NamingContainer;
                try
                {
                    ddl.Visible = visibleStatus.DefaultIfEmpty(true).First();
                    if (ddl.Visible)
                    {

                        if (ddl.Attributes["blank"] != null)
                        {
                            var li = new ListItem();
                            li.Text = ddl.Attributes["blank"];
                            li.Value = "";
                            ddl.Items.Add(li);
                            ddl.Attributes.Remove("blank");
                        }
                        var show = false;
                        var countryCode = GenXmlFunctions.GetGenXmlValue("Country", "dropdownlist", Convert.ToString(DataBinder.Eval(container.DataItem, _databindColumn)));
                        if (countryCode == "")
                        {
                            var cList = NBrightBuyUtils.GetCountryList();
                            if (cList.Any()) countryCode = cList.First().Value;
                        }
                        if (countryCode != "")
                        {
                            var tList = NBrightBuyUtils.GetRegionList(countryCode);
                            if (tList.Count > 0)
                            {
                                show = true;
                                foreach (var tItem in tList)
                                {
                                    var li = new ListItem();
                                    li.Text = tItem.Value;
                                    li.Value = tItem.Key;
                                    ddl.Items.Add(li);
                                }
                                var strValue = GenXmlFunctions.GetGenXmlValue(ddl.ID, "dropdownlist", Convert.ToString(DataBinder.Eval(container.DataItem, _databindColumn)));
                                if ((ddl.Items.FindByValue(strValue) != null)) ddl.SelectedValue = strValue;                                
                            }
                        }
                        ddl.Visible = show;

                    }

                }
                catch (Exception)
                {
                    ddl.Visible = false;
                }                
            }

            if (sender is TextBox)
            {
                var txt = (TextBox)sender;
                var container = (IDataItemContainer)txt.NamingContainer;
                try
                {
                    txt.Visible = visibleStatus.DefaultIfEmpty(true).First();
                    if (txt.Visible)
                    {
                        var show = true;
                        var countryCode = GenXmlFunctions.GetGenXmlValue("Country", "dropdownlist", Convert.ToString(DataBinder.Eval(container.DataItem, _databindColumn)));
                        if (countryCode != "")
                        {
                            var tList = NBrightBuyUtils.GetRegionList(countryCode);
                            if (tList.Count > 0) show = false;
                        }

                        txt.Visible = show;
                        if (txt.Visible)
                        {
                            var strData = GenXmlFunctions.GetGenXmlValue(txt.ID, "textbox", Convert.ToString(DataBinder.Eval(container.DataItem, _databindColumn)));
                            if (txt.Text == "")
                            {
                                txt.Text = strData;
                            }
                            else
                            {
                                if (strData != "") txt.Text = strData;
                            }
                        }

                    }

                }
                catch (Exception)
                {
                    txt.Visible = false;
                }
            }


        }


        #endregion
        
        #region "Addressdropdown"

        private void CreateAddressDropDownList(Control container, XmlNode xmlNod)
        {
            var rbl = new DropDownList();
            if (xmlNod.Attributes != null && (xmlNod.Attributes["blank"] != null))
            {
                rbl.Attributes.Add("blank", xmlNod.Attributes["blank"].Value);
            }
            if (xmlNod.Attributes != null && (xmlNod.Attributes["template"] != null))
            {
                rbl.Attributes.Add("template", xmlNod.Attributes["template"].Value);
            }
            if (xmlNod.Attributes != null && (xmlNod.Attributes["data"] != null))
            {
                rbl.Attributes.Add("data", xmlNod.Attributes["data"].Value);
            }
            if (xmlNod.Attributes != null && (xmlNod.Attributes["datavalue"] != null))
            {
                rbl.Attributes.Add("datavalue", xmlNod.Attributes["datavalue"].Value);
            }
            if (xmlNod.Attributes != null && (xmlNod.Attributes["datavalue"] != null))
            {
                rbl.Attributes.Add("formselector", xmlNod.Attributes["formselector"].Value);
            }

            rbl = (DropDownList)GenXmlFunctions.AssignByReflection(rbl, xmlNod);
            rbl.DataBinding += AddressDropdownDataBind;
            if (xmlNod.Attributes != null && (xmlNod.Attributes["id"] != null))
            {
                rbl.ID = xmlNod.Attributes["id"].InnerText;
                container.Controls.Add(rbl);
            }
        }

        private void AddressDropdownDataBind(object sender, EventArgs e)
        {

            var ddl = (DropDownList)sender;
            var container = (IDataItemContainer)ddl.NamingContainer;
            try
            {
                ddl.Visible = visibleStatus.DefaultIfEmpty(true).First();
                if (ddl.Visible)
                {

                    var usr = UserController.Instance.GetCurrentUserInfo();
                    var addressData = new AddressData(usr.UserID.ToString(""));

                    if (ddl.Attributes["blank"] != null)
                    {
                        var li = new ListItem();
                        li.Text = ddl.Attributes["blank"];
                        li.Value = "-1";
                        ddl.Items.Add(li);
                        ddl.Attributes.Remove("blank");
                    }

                    var addrlist = addressData.GetAddressList();
                    foreach (var tItem in addrlist)
                    {
                        var itemtext = tItem.GetXmlProperty("genxml/textbox/firstname") + "," + tItem.GetXmlProperty("genxml/textbox/lastname") + "," + tItem.GetXmlProperty("genxml/textbox/unit") + "," + tItem.GetXmlProperty("genxml/textbox/street") + "," + tItem.GetXmlProperty("genxml/textbox/city");
                        if (ddl.Attributes["template"] != null)
                        {
                            itemtext = "";
                            var xpathList = ddl.Attributes["template"].Split(',');
                            foreach (var xp in xpathList)
                            {
                                itemtext += "," + tItem.GetXmlProperty(xp);
                            }
                        }
                        var datatext = "";
                        if (ddl.Attributes["data"] != null)
                        {
                            var xpathList = ddl.Attributes["data"].Split(',');
                            foreach (var xp in xpathList)
                            {
                                datatext += "," + tItem.GetXmlProperty(xp).Replace(","," ");
                            }
                        }
                        var datavalue = "";
                        if (ddl.Attributes["datavalue"] != null) datavalue += ddl.Attributes["datavalue"];

                        var idx = tItem.GetXmlProperty("genxml/hidden/index");
                        if (ddl.Items.FindByValue(idx) == null)
                        {
                            var li = new ListItem();
                            li.Text = itemtext.TrimStart(',');
                            li.Value = idx;
                            li.Attributes.Add("data", datatext.TrimStart(','));
                            li.Attributes.Add("datavalue", datavalue);
                            ddl.Items.Add(li);                            
                        }
                    }

                }

            }
            catch (Exception)
            {
                ddl.Visible = false;
            }
        }



#endregion

        #region "Order Itemlist"

        private void CreateOrderItemlist(Control container, XmlNode xmlNod)
        {
            var lc = new Literal();
            if (xmlNod.Attributes != null && (xmlNod.Attributes["template"] != null))
            {
                lc.Text = xmlNod.Attributes["template"].Value;
                if (xmlNod.Attributes["groupby"] != null || StoreSettings.Current.Get("chkgroupresults") == "True") lc.Text = lc.Text + ":GROUPBY";
            }
            
            lc.DataBinding += OrderItemlistDataBind;
            container.Controls.Add(lc);
        }

        private void OrderItemlistDataBind(object sender, EventArgs e)
        {
            var lc = (Literal)sender;
            var container = (IDataItemContainer)lc.NamingContainer;
            try
            {
                var strOut = "";
                lc.Visible = visibleStatus.DefaultIfEmpty(true).First();
                if (lc.Visible)
                {

                    var id = Convert.ToString(DataBinder.Eval(container.DataItem, "ItemId"));
                    var lang = Convert.ToString(DataBinder.Eval(container.DataItem, "lang"));
                    if (lang == "") lang = Utils.GetCurrentCulture();
                    var groupresults = false;
                    if (lc.Text.EndsWith(":GROUPBY"))
                    {
                        groupresults = true;
                        lc.Text = lc.Text.Replace(":GROUPBY", "");
                    }
                    var templName = lc.Text;
                    if (Utils.IsNumeric(id) && (templName != ""))
                    {
                        var buyCtrl = new NBrightBuyController();
                        var rpTempl = buyCtrl.GetTemplateData(-1, templName, lang, _settings, StoreSettings.Current.DebugMode);

                        //remove templName from template, so we don't get a loop.
                        if (rpTempl.Contains(templName)) rpTempl = rpTempl.Replace(templName, "");
                        //build models list

                        var objInfo = (NBrightInfo)container.DataItem;
                        var ordData = new OrderData(objInfo.PortalId,objInfo.ItemID);
                        // render repeater
                        try
                        {
                            var itemTemplate = NBrightBuyUtils.GetGenXmlTemplate(rpTempl, _settings, PortalSettings.Current.HomeDirectory, visibleStatus);
                            strOut = GenXmlFunctions.RenderRepeater(ordData.GetCartItemList(groupresults), itemTemplate);
                        }
                        catch (Exception exc)
                        {
                            strOut = "ERROR: NOTE: sub rendered templates CANNOT contain postback controls.<br/>" + exc;
                        }
                    }
                }
                lc.Text = strOut;

            }
            catch (Exception)
            {
                lc.Text = "";
            }
        }

        #endregion

        #region "Order Auditlist"

        private void CreateOrderAuditlist(Control container, XmlNode xmlNod)
        {
            var lc = new Literal();
            if (xmlNod.Attributes != null && (xmlNod.Attributes["template"] != null))
            {
                lc.Text = xmlNod.Attributes["template"].Value;
            }
            lc.DataBinding += OrderAuditlistDataBind;
            container.Controls.Add(lc);
        }

        private void OrderAuditlistDataBind(object sender, EventArgs e)
        {
            var lc = (Literal)sender;
            var container = (IDataItemContainer)lc.NamingContainer;
            try
            {
                var strOut = "";
                lc.Visible = visibleStatus.DefaultIfEmpty(true).First();
                if (lc.Visible)
                {

                    var id = Convert.ToString(DataBinder.Eval(container.DataItem, "ItemId"));
                    var lang = Convert.ToString(DataBinder.Eval(container.DataItem, "lang"));
                    if (lang == "") lang = Utils.GetCurrentCulture();
                    var templName = lc.Text;
                    if (Utils.IsNumeric(id) && (templName != ""))
                    {
                        var buyCtrl = new NBrightBuyController();
                        var rpTempl = buyCtrl.GetTemplateData(-1, templName, lang, _settings, StoreSettings.Current.DebugMode);

                        //remove templName from template, so we don't get a loop.
                        if (rpTempl.Contains(templName)) rpTempl = rpTempl.Replace(templName, "");
                        //build models list

                        var objInfo = (NBrightInfo)container.DataItem;
                        var ordData = new OrderData(objInfo.PortalId, objInfo.ItemID);
                        // render repeater
                        try
                        {
                            var itemTemplate = NBrightBuyUtils.GetGenXmlTemplate(rpTempl, _settings, PortalSettings.Current.HomeDirectory, visibleStatus);
                            strOut = GenXmlFunctions.RenderRepeater(ordData.GetAuditItemList(), itemTemplate);
                        }
                        catch (Exception exc)
                        {
                            strOut = "ERROR: NOTE: sub rendered templates CANNOT contain postback controls.<br/>" + exc;
                        }
                    }
                }
                lc.Text = strOut;

            }
            catch (Exception)
            {
                lc.Text = "";
            }
        }

        #endregion

        #region "Cart Itemlist"

        private void CreateCartItemlist(Control container, XmlNode xmlNod)
        {
            var lc = new Literal();
            if (xmlNod.Attributes != null && (xmlNod.Attributes["template"] != null))
            {
                lc.Text = xmlNod.Attributes["template"].Value;
                if (xmlNod.Attributes["groupby"] != null) lc.Text = lc.Text + ":GROUPBY";
            }
            lc.DataBinding += CartItemlistDataBind;
            container.Controls.Add(lc);
        }

        private void CartItemlistDataBind(object sender, EventArgs e)
        {
            var lc = (Literal)sender;
            var container = (IDataItemContainer)lc.NamingContainer;
            try
            {
                var strOut = "";
                lc.Visible = visibleStatus.DefaultIfEmpty(true).First();
                if (lc.Visible)
                {

                    var id = Convert.ToString(DataBinder.Eval(container.DataItem, "ItemId"));
                    var lang = Convert.ToString(DataBinder.Eval(container.DataItem, "lang"));
                    if (lang == "") lang = Utils.GetCurrentCulture();
                    var groupresults = false;
                    if (lc.Text.EndsWith(":GROUPBY"))
                    {
                        groupresults = true;
                        lc.Text = lc.Text.Replace(":GROUPBY", "");
                    }
                    var templName = lc.Text;
                    if (Utils.IsNumeric(id) && (templName != ""))
                    {
                        var buyCtrl = new NBrightBuyController();
                        var rpTempl = buyCtrl.GetTemplateData(-1, templName, lang, _settings, StoreSettings.Current.DebugMode);

                        //remove templName from template, so we don't get a loop.
                        if (rpTempl.Contains(templName)) rpTempl = rpTempl.Replace(templName, "");
                        //build models list

                        var objInfo = (NBrightInfo)container.DataItem;
                        var cartData = new CartData(objInfo.PortalId);
                        // render repeater
                        try
                        {
                            var itemTemplate = NBrightBuyUtils.GetGenXmlTemplate(rpTempl, _settings, PortalSettings.Current.HomeDirectory, visibleStatus);
                            strOut = GenXmlFunctions.RenderRepeater(cartData.GetCartItemList(groupresults), itemTemplate);
                        }
                        catch (Exception exc)
                        {
                            strOut = "ERROR: NOTE: sub rendered templates CANNOT contain postback controls.<br/>" + exc;
                        }
                    }
                }
                lc.Text = strOut;

            }
            catch (Exception)
            {
                lc.Text = "";
            }
        }

        #endregion

        #region "Sale Price"

        private void CreateConcatenate(Control container, XmlNode xmlNod)
        {
            var l = new Literal();
            l.Text = "";
            if (xmlNod.Attributes != null)
            {
                foreach (XmlAttribute attr in xmlNod.Attributes)
                {
                    if (attr.Name.StartsWith("xpath"))
                    {
                        l.Text += ";" + attr.InnerText;
                    }
                }
            }
            
            l.DataBinding += ConcatenateDataBinding;
 
            container.Controls.Add(l);
        }

        private void ConcatenateDataBinding(object sender, EventArgs e)
        {
            var l = (Literal)sender;
            var container = (IDataItemContainer)l.NamingContainer;
            var strXml = DataBinder.Eval(container.DataItem, _databindColumn).ToString();
            var nbi = new NBrightInfo();
            nbi.XMLData = strXml;
            try
            {
                var xlist = l.Text.Split(';');
                l.Text = "";
                foreach (var s in xlist)
                {
                    if (s != "" && !l.Text.Contains(nbi.GetXmlProperty(s))) l.Text += " " + nbi.GetXmlProperty(s);
                }
                l.Visible = visibleStatus.DefaultIfEmpty(true).First();

            }
            catch (Exception ex)
            {
                l.Text = ex.ToString();
            }
        }


        #endregion

        #region "Shipping"

        private void CreateShippingProviderRadio(Control container, XmlNode xmlNod)
        {
                var rbl = new RadioButtonList();
                rbl = (RadioButtonList)GenXmlFunctions.AssignByReflection(rbl, xmlNod);
                rbl.DataBinding += ShippingProviderDataBind;
                rbl.ID = "shippingprovider";
                container.Controls.Add(rbl);
        }

        private void ShippingProviderDataBind(object sender, EventArgs e)
        {
            var rbl = (RadioButtonList)sender;
            var container = (IDataItemContainer)rbl.NamingContainer;
            try
            {
                rbl.Visible = visibleStatus.DefaultIfEmpty(true).First();
                if (rbl.Visible)
                {
                    var strXML = Convert.ToString(DataBinder.Eval(container.DataItem, "XMLData"));
                    var nbInfo = new NBrightInfo();
                    nbInfo.XMLData = strXML;
                    var selectval = nbInfo.GetXmlProperty("genxml/radiobuttonlist/shippingprovider");

                    // get country code, to CheckBox if provider is valid.
                    var cartData = new CartData(PortalSettings.Current.PortalId);

                    var pluginData = new PluginData(PortalSettings.Current.PortalId);
                    var provList = pluginData.GetShippingProviders();
                        foreach (var d in provList)
                        {
                            var isValid = true;
                            var shipprov = ShippingInterface.Instance(d.Key);
                            if (shipprov != null) isValid = shipprov.IsValid(cartData.PurchaseInfo);
                            var p = d.Value;
                            if (isValid)
                            {
                                var li = new ListItem();
                                li.Text = p.GetXmlProperty("genxml/textbox/name");
                                li.Value = p.GetXmlProperty("genxml/textbox/ctrl");
                                if (li.Value == selectval) li.Selected = true;
                                rbl.Items.Add(li);
                            }
                        }
                        if (rbl.SelectedValue == "" && rbl.Items.Count > 0) rbl.SelectedIndex = 0;

                }

            }
            catch (Exception)
            {
                rbl.Visible = false;
            }
        }




        #endregion

        #region "CreateCurrencyOf"

        private void CreateCurrencyOf(Control container, XmlNode xmlNod)
        {
            var lc = new Literal();
            if (xmlNod.Attributes != null && (xmlNod.Attributes["xpath"] != null))
            {
                lc.Text = xmlNod.Attributes["xpath"].Value;
            }
            lc.DataBinding += CurrencyOfDataBind;
            container.Controls.Add(lc);
        }

        private void CurrencyOfDataBind(object sender, EventArgs e)
        {
            var lc = (Literal)sender;
            var container = (IDataItemContainer)lc.NamingContainer;
            var nbi = (NBrightInfo) container.DataItem;
            try
            {
                lc.Visible = visibleStatus.DefaultIfEmpty(true).First();
                lc.Text = NBrightBuyUtils.FormatToStoreCurrency(nbi.GetXmlPropertyDouble(lc.Text));
            }
            catch (Exception)
            {
                lc.Text = "";
            }
        }

        #endregion

        #region "XslInject"

        private void CreateXslInject(Control container, XmlNode xmlNod)
        {
            if (xmlNod.Attributes != null && (xmlNod.Attributes["template"] != null))
            {
                var argslist = "";
                if (xmlNod.Attributes["argslist"] != null) argslist = xmlNod.Attributes["argslist"].InnerText;
                var lc = new Literal();
                lc.DataBinding += XslInjectDataBind;
                lc.Text  = xmlNod.Attributes["template"].Value + "*" + argslist;                
                container.Controls.Add(lc);
            }
        }

        private void XslInjectDataBind(object sender, EventArgs e)
        {
            var lc = (Literal)sender;
            var container = (IDataItemContainer)lc.NamingContainer;
            lc.Visible = visibleStatus.DefaultIfEmpty(true).First();
            if (lc.Visible && container.DataItem != null)
            {
                var param = lc.Text.Split('*');
                if (param.Count() == 2)
                {
                    var argsList = new XsltArgumentList();
                    var templName = param[0];
                    foreach (var arg in param[1].Split(','))
                    {
                        if (_settings != null && _settings.ContainsKey(arg)) argsList.AddParam(arg, "", _settings[arg]);
                    }
                    var buyCtrl = new NBrightBuyController();
                    var xslTempl = buyCtrl.GetTemplateData(-1, templName, Utils.GetCurrentCulture(), _settings, StoreSettings.Current.DebugMode);
                    var nbi = (NBrightInfo) container.DataItem;
                    var strOut = XslUtils.XslTransInMemory(nbi.XMLData, xslTempl, argsList);
                    lc.Text = strOut;
                }
            }
        }


        #endregion

        #region "Xcharts"

        private void CreateXchartOrderRev(Control container, XmlNode xmlNod)
        {
                var lc = new Literal();
                lc.DataBinding += CreateXchartOrderRevDataBind;
                lc.Text  = "";
                container.Controls.Add(lc);
        }

        private void CreateXchartOrderRevDataBind(object sender, EventArgs e)
        {
            var lc = (Literal) sender;
            var container = (IDataItemContainer) lc.NamingContainer;
            lc.Visible = visibleStatus.DefaultIfEmpty(true).First();
            if (lc.Visible && container.DataItem != null)
            {
                var nbi1 = (NBrightInfo) container.DataItem;
                var strOut = "";
                var nodList = nbi1.XMLDoc.SelectNodes("root/orderstats/*");
                if (nodList != null)
                {
                    foreach (XmlNode nod in nodList)
                    {
                        var nbi = new NBrightInfo();
                        nbi.XMLData = nod.OuterXml;

                        strOut += "{'x': '" + nbi.GetXmlPropertyInt("item/createdyear") + "-" + nbi.GetXmlPropertyInt("item/createdmonth").ToString("D2") + "',";
                        strOut += "'y': " + nbi.GetXmlPropertyRaw("item/appliedtotal").ToString() + "},";

                    }
                    strOut = strOut.TrimEnd(',');
                }

                lc.Text = strOut;
            }
        }

        #endregion

        #region "labelof"

        private void CreateLabelOf(Control container, XmlNode xmlNod)
        {
            var lc = new Label();

            lc.Attributes.Add("data-toggle","tooltip");

            if (xmlNod.Attributes != null && (xmlNod.Attributes["Text"] != null)) lc.Text =  xmlNod.Attributes["Text"].InnerXml;
            if (xmlNod.Attributes != null && (xmlNod.Attributes["Help"] != null)) lc.Attributes.Add("data-original-title", xmlNod.Attributes["Help"].InnerXml);
            if (xmlNod.Attributes != null && (xmlNod.Attributes["data-original-title"] != null)) lc.Attributes.Add("data-original-title", xmlNod.Attributes["data-original-title"].InnerXml);
            if (xmlNod.Attributes != null && (xmlNod.Attributes["data-placement"] != null)) lc.Attributes.Add("data-placement", xmlNod.Attributes["data-placement"].InnerXml);

            lc.DataBinding += LabelOfDataBinding;
            container.Controls.Add(lc);
        }


        private void LabelOfDataBinding(object sender, EventArgs e)
        {
            // NOTE: Do not set Text = "", If we've assign a Text value in the template (or resourcekey) then use it as default. (unless Error)
            var lc = (Label)sender;
            lc.Visible = visibleStatus.DefaultIfEmpty(true).First();
            try
            {
                lc.Visible = visibleStatus.DefaultIfEmpty(true).First();
            }
            catch (Exception)
            {
                lc.Text = "";
            }
        }



        #endregion

        #region "Literal tokens"

        private void CreateEditCultureSelect(Control container, XmlNode xmlNod)
        {
            var cssclass = "";
            if (xmlNod.Attributes != null && (xmlNod.Attributes["cssclass"] != null)) cssclass = xmlNod.Attributes["cssclass"].InnerText;
            var cssclassli = "";
            if (xmlNod.Attributes != null && (xmlNod.Attributes["cssclassli"] != null)) cssclassli = xmlNod.Attributes["cssclassli"].InnerText;
            var size = "32";
            if (xmlNod.Attributes != null && (xmlNod.Attributes["size"] != null)) size = xmlNod.Attributes["size"].InnerText;
            if (size != "16" & size != "24" & size != "32") size = "32";

            var enabledlanguages = LocaleController.Instance.GetLocales(PortalSettings.Current.PortalId);
            var strOut = new StringBuilder("<ul class='" + cssclass + "'>");
            foreach (var l in enabledlanguages)
            {
                strOut.Append("<li><a href='javascript:void(0)' lang='" + l.Value.Code + "' class='" + cssclassli + "'>");
                strOut.Append("<img src='/DnnImageHandler.ashx?mode=file&file=/images/flags/" + l.Value.Code + ".gif&h=" + size + "' alt='" + l.Value.NativeName + "' />");
                strOut.Append("</a></li>");
            }
            strOut.Append("</ul>");

            var lc = new Literal();
            lc.Text = strOut.ToString();
            lc.DataBinding += LiteralDataBinding;
            container.Controls.Add(lc);
        }

        private void LiteralDataBinding(object sender, EventArgs e)
        {
            try
            {
                var lc = (Literal)sender;
                lc.Visible = visibleStatus.Last();
            }
            catch (Exception)
            {
                //do nothing
            }
        }

        #endregion


        #region "Functions"

        private List<NBrightInfo> BuildModelList(NBrightInfo dataItemObj,Boolean addSalePrices = false)
        {
            // see  if we have a cart record
            var xpathprefix = "";
            var cartrecord = dataItemObj.GetXmlProperty("genxml/productid") != ""; // if we have a productid node, then is datarecord is a cart item
            if (cartrecord) xpathprefix = "genxml/productxml/";

            //build models list
            var objL = new List<NBrightInfo>();
            var nodList = dataItemObj.XMLDoc.SelectNodes(xpathprefix + "genxml/models/*");
            if (nodList != null)
            {

                #region "Init"

                var isDealer = NBrightBuyUtils.IsDealer();


                #endregion

                var lp = 1;
                foreach (XmlNode nod in nodList)
                {
                    // check if Deleted
                    var selectDeletedFlag = nod.SelectSingleNode("checkbox/chkdeleted");
                    if ((selectDeletedFlag == null) || selectDeletedFlag.InnerText != "True")
                    {
                    // check if hidden
                        var selectHiddenFlag = nod.SelectSingleNode("checkbox/chkishidden");
                        if ((selectHiddenFlag == null) || selectHiddenFlag.InnerText != "True")
                        {
                            // check if dealer
                            var selectDealerFlag = nod.SelectSingleNode("checkbox/chkdealeronly");
                            if (((selectDealerFlag == null) || (!isDealer && (selectDealerFlag.InnerText != "True"))) | isDealer)
                            {
                                // get modelid
                                var nodModelId = nod.SelectSingleNode("hidden/modelid");
                                var modelId = "";
                                if (nodModelId != null) modelId = nodModelId.InnerText;

                                //Build NBrightInfo class for model
                                var o = new NBrightInfo();
                                o.XMLData = nod.OuterXml;

                                #region "Add Lanaguge Data"

                                var nodLang = dataItemObj.XMLDoc.SelectSingleNode(xpathprefix + "genxml/lang/genxml/models/genxml[" + lp.ToString("") + "]");
                                if (nodLang != null)
                                {
                                    o.AddSingleNode("lang", "", "genxml");
                                    o.AddXmlNode(nodLang.OuterXml, "genxml", "genxml/lang");
                                }

                                #endregion

                                #region "Prices"

                                if (addSalePrices)
                                {
                                    var uInfo = UserController.Instance.GetCurrentUserInfo();
                                    if (uInfo != null)
                                    {
                                        o.SetXmlPropertyDouble("genxml/hidden/saleprice", "-1"); // set to -1 so unitcost is displayed (turns off saleprice)
                                        //[TODO: convert to new promotion provider]
                                        //var objPromoCtrl = new PromoController();
                                        //var objPCtrl = new ProductController();
                                        //var objM = objPCtrl.GetModel(modelId, Utils.GetCurrentCulture());
                                        //var salePrice = objPromoCtrl.GetSalePrice(objM, uInfo);
                                        //o.AddSingleNode("saleprice", salePrice.ToString(CultureInfo.GetCultureInfo("en-US")), "genxml/hidden");
                                    }
                                }

                                #endregion

                                // product data for display in modellist
                                o.SetXmlProperty("genxml/lang/genxml/textbox/txtproductname", dataItemObj.GetXmlProperty(xpathprefix + "genxml/lang/genxml/textbox/txtproductname"));
                                o.SetXmlProperty("genxml/textbox/txtproductref", dataItemObj.GetXmlProperty(xpathprefix + "genxml/textbox/txtproductref"));

                                if (cartrecord)
                                    o.SetXmlProperty("genxml/hidden/productid", dataItemObj.GetXmlProperty("genxml/productid"));
                                else
                                o.SetXmlProperty("genxml/hidden/productid", dataItemObj.ItemID.ToString(""));


                                objL.Add(o);
                            }
                        }
                    }
                    lp += 1;
                }
            }
            return objL;
        }

        private Double GetSalePriceDouble(NBrightInfo dataItemObj)
        {
            Double price = -1;
            var l = BuildModelList(dataItemObj);
            foreach (var m in l)
            {
                var s = m.GetXmlPropertyDouble("genxml/textbox/txtsaleprice");
                if ((s > 0) && (s < price) | (price == -1)) price = s;
            }
            if (price == -1) price = 0;
            return price;
        }

        private String GetSalePrice(NBrightInfo dataItemObj)
        {
            double dealprice = -1;
            string price = "";
            var l = BuildModelList(dataItemObj);
            foreach (var m in l)
            {
                var s = m.GetXmlPropertyDouble("genxml/textbox/txtsaleprice");
                if (((s > 0) && (s < dealprice)) || (dealprice == -1))
                {
                    price = m.GetXmlPropertyRaw("genxml/textbox/txtsaleprice");
                    dealprice = s;
                }
            }
            return price;
        }

        private String GetDealerPrice(NBrightInfo dataItemObj)
        {
            double dealprice = -1;
            string price = "";
            var l = BuildModelList(dataItemObj);
            foreach (var m in l)
            {
                var s = m.GetXmlPropertyDouble("genxml/textbox/txtdealercost");
                if (((s > 0) && (s < dealprice)) || (dealprice == -1))
                {
                    price = m.GetXmlPropertyRaw("genxml/textbox/txtdealercost");
                    dealprice = s;
                }
            }
            return price;
        }

        private String GetFromPrice(NBrightInfo dataItemObj)
        {
            double tprice = -1;
            string price = "";
            var l = BuildModelList(dataItemObj);
            foreach (var m in l)
            {
                var s = m.GetXmlPropertyDouble("genxml/textbox/txtunitcost");
                if ((s < tprice) || (tprice == -1))
                {
                    price = m.GetXmlPropertyRaw("genxml/textbox/txtunitcost");
                    tprice = s;
                }
            }
            return price;
        }

        private Double GetFromPriceDouble(NBrightInfo dataItemObj)
        {
            Double price = -1;
            var l = BuildModelList(dataItemObj);
            foreach (var m in l)
            {
                var s = m.GetXmlPropertyDouble("genxml/textbox/txtunitcost");
                if ((s > 0) && (s < price) | (price == -1)) price = s;
            }
            if (price == -1) price = 0;
            return price;
        }

        private String GetBestPrice(NBrightInfo dataItemObj)
        {
            var fromprice = GetFromPriceDouble(dataItemObj);
            if (fromprice < 0) fromprice = 0; // make sure we have a valid price
            var saleprice = GetSalePriceDouble(dataItemObj);
            if (saleprice < 0) saleprice = fromprice; // sale price might not exists.

            if (NBrightBuyUtils.IsDealer())
            {
                var dealerprice = Convert.ToDouble(GetDealerPrice(dataItemObj), CultureInfo.GetCultureInfo("en-US"));
                if (dealerprice <= 0) dealerprice = fromprice; // check for valid dealer price.
                if (fromprice < dealerprice)
                {
                    if ((saleprice <= 0) || (fromprice < saleprice)) return fromprice.ToString(CultureInfo.GetCultureInfo("en-US"));
                    return saleprice.ToString(CultureInfo.GetCultureInfo("en-US"));
                }
                if ((dealerprice <= 0) || (dealerprice < saleprice)) return dealerprice.ToString(CultureInfo.GetCultureInfo("en-US"));
                return saleprice.ToString(CultureInfo.GetCultureInfo("en-US"));
            }
            if ((saleprice <= 0) || (fromprice < saleprice)) return fromprice.ToString(CultureInfo.GetCultureInfo("en-US"));
            return saleprice.ToString(CultureInfo.GetCultureInfo("en-US"));                
        }

        private Boolean HasDifferentPrices(NBrightInfo dataItemObj)
        {
            var saleprice = GetSalePriceDouble(dataItemObj);
            if (saleprice >= 0) return true;  // if it's on sale we can assume it has multiple prices
            var nodList = dataItemObj.XMLDoc.SelectNodes("genxml/models/*");
            if (nodList != null)
            {
                //check if we really need to add prices (don't if all the same)
                var holdPrice = "";
                var holdDealerPrice = "";
                var isDealer = NBrightBuyUtils.IsDealer();
                foreach (XmlNode nod in nodList)
                {
                    var mPrice = nod.SelectSingleNode("textbox/txtunitcost");
                    if (mPrice != null)
                    {
                        if (holdPrice != "" && mPrice.InnerText != holdPrice)
                        {
                            return true;
                        }
                        holdPrice = mPrice.InnerText;
                    }
                    if (isDealer)
                    {
                        var mDealerPrice = nod.SelectSingleNode("textbox/txtdealercost");
                        if (mDealerPrice != null)
                        {
                            if (holdDealerPrice != "" && mDealerPrice.InnerText != holdDealerPrice) return true;
                            holdDealerPrice = mDealerPrice.InnerText;
                        }                        
                    }

                }
            }
            return false;
        }

        public static void IncreaseArray(ref string[] values, int increment)
        {
            var array = new string[values.Length + increment];
            values.CopyTo(array, 0);
            values = array;
        }

        private Boolean IsInStock(NBrightInfo dataItem)
        {
            var nodList = BuildModelList(dataItem);
            foreach (var obj in nodList)
            {
                if (IsModelInStock(obj)) return true;
            }
            return false;
        }

        private Boolean IsModelInStock(NBrightInfo dataItem)
        {
            var stockOn = dataItem.GetXmlPropertyBool("genxml/checkbox/chkstockon");
            if (stockOn)
            {
                var modelstatus = dataItem.GetXmlProperty("genxml/dropdownlist/modelstatus");
                if (modelstatus == "010") return true;
            }
            else
            {
                return true;
            }
            return false;
        }

        #endregion
    }
}
