using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Razor;
using System.Web.Script.Serialization;
using System.Windows.Forms.VisualStyles;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using NBrightCore.common;
using NBrightCore.images;
using NBrightCore.render;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Admin;
using Nevoweb.DNN.NBrightBuy.Components.Interfaces;

namespace Nevoweb.DNN.NBrightBuy.Components.Products
{
    public class ProductFunctions
    {
        #region "product Admin Methods"
        public string EditLangCurrent = "";
        public string EntityTypeCode = "";
        public string UiLang = "";
        public string TemplateRelPath = "/DesktopModules/NBright/NBrightBuy";
        public string RazorTemplate = "";
        private bool DebugMode => StoreSettings.Current.DebugMode;

        public void ResetTemplateRelPath()
        {
            TemplateRelPath = "/DesktopModules/NBright/NBrightBuy";
        }

        public string ProcessCommand(string paramCmd, HttpContext context, string editlang = "", string uilang = "")
        {

            var strOut = "PRODUCT - ERROR!! - No Security rights or function command.";
            var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);
            var userId = ajaxInfo.GetXmlPropertyInt("genxml/hidden/userid");
            EntityTypeCode = ajaxInfo.GetXmlProperty("genxml/hidden/entitytypecode");
            if (EntityTypeCode == "") EntityTypeCode = "PRD"; // default to product
            UiLang = NBrightBuyUtils.GetUILang(ajaxInfo);
            EditLangCurrent = editlang;
            if (EditLangCurrent == "") EditLangCurrent = NBrightBuyUtils.GetEditLang(ajaxInfo);


            if (!paramCmd.ToLower().Contains("save"))
            {
                // pickup nextlang, indicates if we are changing languages. (Don't use if saving data, only for getting next language.)
                EditLangCurrent = NBrightBuyUtils.GetNextLang(ajaxInfo, EditLangCurrent);
            }

            switch (paramCmd)
            {
                case "product_admin_getlist":
                    if (!NBrightBuyUtils.CheckRights()) break;
                    strOut = ProductAdminList(context);
                    break;
                case "product_admin_getdetail":
                    if (!NBrightBuyUtils.CheckRights()) break;
                    strOut = ProductAdminDetail(context);
                    break;
                case "product_adminaddnew":
                    if (!NBrightBuyUtils.CheckRights()) break;
                    strOut = ProductAdminAddNew(context);
                    break;
                case "product_admin_save":
                    if (!NBrightBuyUtils.CheckRights()) break;
                    strOut = ProductAdminSave(context);
                    break;
                case "product_admin_saveexit":
                    if (!NBrightBuyUtils.CheckRights()) break;
                    strOut = ProductAdminSaveExit(context);
                    break;
                case "product_admin_saveas":
                    if (!NBrightBuyUtils.CheckRights()) break;
                    strOut = ProductAdminSaveAs(context);
                    break;
                case "product_admin_selectlist":
                    if (!NBrightBuyUtils.CheckRights()) break;
                    strOut = ProductAdminList(context);
                    break;
                case "product_moveproductadmin":
                    if (!NBrightBuyUtils.CheckRights()) break;
                    strOut = MoveProductAdmin(context);
                    break;
                case "product_addproductmodels":
                    if (!NBrightBuyUtils.CheckRights()) break;
                    strOut = AddModel(context);
                    break;
                case "product_addproductoptions":
                    if (!NBrightBuyUtils.CheckRights()) break;
                    strOut = AddOption(context);
                    break;
                case "product_addproductoptionvalues":
                    if (!NBrightBuyUtils.CheckRights()) break;
                    strOut = AddOptionValues(context);
                    break;
                case "product_admin_delete":
                    if (!NBrightBuyUtils.CheckRights()) break;
                    strOut = DeleteProduct(context);
                    break;
                case "product_updateproductimages":
                    if (!NBrightBuyUtils.CheckRights()) break;
                    strOut = UpdateProductImages(context);
                    break;
                case "product_updateproductdocs":
                    if (!NBrightBuyUtils.CheckRights()) break;
                    strOut = UpdateProductDocs(context);
                    break;
                case "product_addproductcategory":
                    if (!NBrightBuyUtils.CheckRights()) break;
                    strOut = AddProductCategory(context);
                    break;
                case "product_removeproductcategory":
                    if (!NBrightBuyUtils.CheckRights()) break;
                    strOut = RemoveProductCategory(context);
                    break;
                case "product_setdefaultcategory":
                    if (!NBrightBuyUtils.CheckRights()) break;
                    strOut = SetDefaultCategory(context);
                    break;
                case "product_populatecategorylist":
                    if (!NBrightBuyUtils.CheckRights()) break;
                    strOut = GetPropertyListBox(context);
                    break;
                case "product_addproperty":
                    if (!NBrightBuyUtils.CheckRights()) break;
                    strOut = AddProperty(context);
                    break;
                case "product_removeproperty":
                    if (!NBrightBuyUtils.CheckRights()) break;
                    strOut = RemoveProperty(context);
                    break;
                case "product_removerelated":
                    if (!NBrightBuyUtils.CheckRights()) break;
                    strOut = RemoveRelatedProduct(context);
                    break;
                case "product_addrelatedproduct":
                    if (!NBrightBuyUtils.CheckRights()) break;
                    strOut = AddRelatedProduct(context);
                    break;
                case "product_getproductselectlist":
                    if (!NBrightBuyUtils.CheckRights()) break;
                    strOut = ProductAdminList(context, true, EditLangCurrent);
                    break;
                case "product_getclientselectlist":
                    if (!NBrightBuyUtils.CheckRights()) break;
                    strOut = GetClientSelectList(context);
                    break;
                case "product_addproductclient":
                    if (!NBrightBuyUtils.CheckRights()) break;
                    strOut = AddProductClient(context);
                    break;
                case "product_productclients":
                    if (!NBrightBuyUtils.CheckRights()) break;
                    strOut = GetProductClients(context);
                    break;
                case "product_removeproductclient":
                    if (!NBrightBuyUtils.CheckRights()) break;
                    strOut = RemoveProductClient(context);
                    break;
                case "product_selectchangedisable":
                    if (!NBrightBuyUtils.CheckRights()) break;
                    strOut = ProductDisable(context);
                    break;
                case "product_selectchangehidden":
                    if (!NBrightBuyUtils.CheckRights()) break;
                    strOut = ProductHidden(context);
                    break;
                case "product_ajaxview_getlist":
                    strOut = ProductAjaxViewList(context);
                    break;
                case "product_ajaxview_getlistfilter":
                    strOut = ProductAjaxViewList(context);
                    break;
                case "product_ajaxview_getfilters":
                    strOut = ProductAjaxFilter(context);
                    break;
            }
            return strOut;
        }

        public String ProductAdminDetail(HttpContext context, int productid = 0)
        {
            try
            {
                if (NBrightBuyUtils.CheckRights())
                {
                    var settings = NBrightBuyUtils.GetAjaxDictionary(context);
                    var strOut = "";
                    var selecteditemid = settings["selecteditemid"];
                    if (productid > 0) selecteditemid = productid.ToString();
                    if (Utils.IsNumeric(selecteditemid))
                    {

                        if (!settings.ContainsKey("themefolder")) settings.Add("themefolder", "");
                        if (!settings.ContainsKey("razortemplate")) settings.Add("razortemplate", "");
                        if (!settings.ContainsKey("portalid")) settings.Add("portalid", PortalSettings.Current.PortalId.ToString("")); // aways make sure we have portalid in settings
                        if (!settings.ContainsKey("selecteditemid")) settings.Add("selecteditemid", "");

                        var themeFolder = settings["themefolder"];

                        var RazorTemplate = settings["razortemplate"];
                        var portalId = Convert.ToInt32(settings["portalid"]);

                        var passSettings = settings;
                        foreach (var s in StoreSettings.Current.Settings()) // copy store setting, otherwise we get a byRef assignement
                        {
                            if (passSettings.ContainsKey(s.Key))
                                passSettings[s.Key] = s.Value;
                            else
                                passSettings.Add(s.Key, s.Value);
                        }

                        if (!Utils.IsNumeric(selecteditemid)) return "";

                        if (themeFolder == "")
                        {
                            themeFolder = StoreSettings.Current.ThemeFolder;
                            if (settings.ContainsKey("themefolder")) themeFolder = settings["themefolder"];
                        }

                        var objCtrl = new NBrightBuyController();
                        var info = objCtrl.GetData(Convert.ToInt32(selecteditemid),EntityTypeCode + "LANG", EditLangCurrent);

                        strOut = NBrightBuyUtils.RazorTemplRender(RazorTemplate, 0, "", info, TemplateRelPath, themeFolder, EditLangCurrent, passSettings);
                    }
                    return strOut;
                }
                return "";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public String ProductAdminSaveExit(HttpContext context)
        {
            try
            {
                ProductSave(context);
                return "";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        public String ProductAdminSaveAs(HttpContext context)
        {
            try
            {
                ProductSave(context, true);
                return "";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public String ProductAdminAddNew(HttpContext context)
        {
            try
            {
                var productid = ProductSave(context, true);
                if (productid > 0)
                {
                    return ProductAdminDetail(context, productid);
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public String ProductAdminSave(HttpContext context)
        {
            try
            {
                try
                {
                    ProductSave(context);
                    return "";
                }
                catch (Exception ex)
                {
                    return ex.ToString();
                }
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public int ProductSave(HttpContext context, bool newrecord = false)
        {
            if (NBrightBuyUtils.CheckRights())
            {
                var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);
                var itemid = -1;
                if (!newrecord)
                {
                    itemid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/itemid");
                }
                if (itemid != 0)
                {
                    var prdData = new ProductData(itemid, EditLangCurrent, true, EntityTypeCode);
                    var modelXml = Utils.UnCode(ajaxInfo.GetXmlProperty("genxml/hidden/xmlupdatemodeldata"));
                    var optionXml = Utils.UnCode(ajaxInfo.GetXmlProperty("genxml/hidden/xmlupdateoptiondata"));
                    var optionvalueXml = Utils.UnCode(ajaxInfo.GetXmlProperty("genxml/hidden/xmlupdateoptionvaluesdata"));

                    prdData.UpdateModels(modelXml, UiLang);
                    prdData.UpdateOptions(optionXml, UiLang);
                    prdData.UpdateOptionValues(optionvalueXml, UiLang);
                    prdData.UpdateImages(ajaxInfo);
                    prdData.UpdateDocs(ajaxInfo);

                    ajaxInfo.RemoveXmlNode("genxml/hidden/xmlupdateproductimages");
                    ajaxInfo.RemoveXmlNode("genxml/hidden/xmlupdateoptionvaluesdata");
                    ajaxInfo.RemoveXmlNode("genxml/hidden/xmlupdateoptiondata");
                    ajaxInfo.RemoveXmlNode("genxml/hidden/xmlupdatemodeldata");
                    ajaxInfo.RemoveXmlNode("genxml/hidden/xmlupdateoptionvaluesdata");
                    var productXml = ajaxInfo.XMLData;

                    prdData.Update(productXml);
                    prdData.Save(true,newrecord);

                    ProductUtils.CreateFriendlyImages(prdData.DataRecord.ItemID, EditLangCurrent);

                    // remove save GetData cache
                    var strCacheKey = prdData.Info.ItemID.ToString("") + "*" + prdData.DataRecord.TypeCode  + "LANG*" + "*" + EditLangCurrent;
                    Utils.RemoveCache(strCacheKey);
                    DataCache.ClearCache();

                    return prdData.Info.ItemID;

                }

            }
            return -1;
        }


        public string ProductDisable(HttpContext context)
        {
            try
            {
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                var parentitemid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selecteditemid");
                if (parentitemid > 0)
                {
                    var prodData = ProductUtils.GetProductData(Convert.ToInt32(parentitemid), EditLangCurrent, false, EntityTypeCode);
                    if (prodData.Disabled)
                    {
                        prodData.DataRecord.SetXmlProperty("genxml/checkbox/chkdisable", "False");
                    }
                    else
                    {
                        prodData.DataRecord.SetXmlProperty("genxml/checkbox/chkdisable", "True");
                    }
                    prodData.Save();
                    // remove save GetData cache
                    var strCacheKey = prodData.Info.ItemID.ToString("") + prodData.DataRecord.TypeCode  + "LANG*" + "*" + EditLangCurrent;
                    Utils.RemoveCache(strCacheKey);
                    DataCache.ClearCache();

                    return "";
                }
                return "Invalid parentitemid";
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        public string ProductHidden(HttpContext context)
        {
            try
            {
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                var parentitemid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selecteditemid");
                if (parentitemid > 0)
                {
                    var prodData = ProductUtils.GetProductData(Convert.ToInt32(parentitemid), EditLangCurrent, false, EntityTypeCode);
                    if (prodData.DataRecord.GetXmlPropertyBool("genxml/checkbox/chkishidden"))
                    {
                        prodData.DataRecord.SetXmlProperty("genxml/checkbox/chkishidden", "False");
                    }
                    else
                    {
                        prodData.DataRecord.SetXmlProperty("genxml/checkbox/chkishidden", "True");
                    }
                    prodData.Save();
                    // remove save GetData cache
                    var strCacheKey = prodData.Info.ItemID.ToString("") + prodData.DataRecord.TypeCode  + "LANG*" + "*" + EditLangCurrent;
                    Utils.RemoveCache(strCacheKey);
                    DataCache.ClearCache();

                    return "";
                }
                return "Invalid parentitemid";
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        public string ProductAdminList(HttpContext context, bool paging = true, string editlang = "",string datatypecode = "",bool loadAjaxEntities = false)
        {

            try
            {
                if (NBrightBuyUtils.CheckRights())
                {
                    var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);

                    if (UserController.Instance.GetCurrentUserInfo().UserID <= 0) return null;

                    if (EditLangCurrent == "") EditLangCurrent = editlang;
                    if (EditLangCurrent == "") EditLangCurrent = Utils.GetCurrentCulture();                    

                    var strOut = "";

                    // select a specific entity data type for the product (used by plugins)
                    var entitytypecodelang = ajaxInfo.GetXmlProperty("genxml/hidden/entitytypecodelang");
                    var entitytypecode = ajaxInfo.GetXmlProperty("genxml/hidden/entitytypecode");
                    if (entitytypecode == "") entitytypecode = EntityTypeCode;
                    if (entitytypecode == "") entitytypecode = "PRD";
                    if (entitytypecodelang == "") entitytypecodelang = EntityTypeCode + "LANG";

                    if (datatypecode == "") datatypecode = entitytypecode;
                    var datatypecodelang = datatypecode + "LANG";

                    var filter = ajaxInfo.GetXmlProperty("genxml/hidden/filter");
                    var orderby = ajaxInfo.GetXmlProperty("genxml/hidden/orderby");
                    var returnLimit = ajaxInfo.GetXmlPropertyInt("genxml/hidden/returnlimit");
                    var pageNumber = ajaxInfo.GetXmlPropertyInt("genxml/hidden/pagenumber");
                    var pageSize = ajaxInfo.GetXmlPropertyInt("genxml/hidden/pagesize");
                    var cascade = ajaxInfo.GetXmlPropertyBool("genxml/hidden/cascade");
                    var portalId = PortalSettings.Current.PortalId;
                    if (ajaxInfo.GetXmlProperty("genxml/hidden/portalid") != "")
                    {
                        portalId = ajaxInfo.GetXmlPropertyInt("genxml/hidden/portalid");
                    }

                    var searchText = ajaxInfo.GetXmlProperty("genxml/hidden/searchtext");
                    var searchCategory = ajaxInfo.GetXmlProperty("genxml/hidden/searchcategory");

                    if (searchText != "") filter += " and (NB3.[ProductName] like '%" + searchText + "%' or NB3.[ProductRef] like '%" + searchText + "%' or NB3.[Summary] like '%" + searchText + "%' ) ";

                    if (Utils.IsNumeric(searchCategory))
                    {
                        if (orderby == "{bycategoryproduct}") orderby += searchCategory;
                        var objQual = DotNetNuke.Data.DataProvider.Instance().ObjectQualifier;
                        var dbOwner = DotNetNuke.Data.DataProvider.Instance().DatabaseOwner;
                        if (!cascade)
                            filter += " and NB1.[ItemId] in (select parentitemid from " + dbOwner + "[" + objQual + "NBrightBuy] where typecode = 'CATXREF' and XrefItemId = " + searchCategory + ") ";
                        else
                            filter += " and NB1.[ItemId] in (select parentitemid from " + dbOwner + "[" + objQual + "NBrightBuy] where (typecode = 'CATXREF' and XrefItemId = " + searchCategory + ") or (typecode = 'CATCASCADE' and XrefItemId = " + searchCategory + ")) ";
                    }
                    else
                    {
                        if (orderby == "{bycategoryproduct}") orderby = " order by NB3.productname ";
                    }

                    // logic for client list of products
                    if (NBrightBuyUtils.IsClientOnly())
                    {
                        filter += " and NB1.ItemId in (select ParentItemId from dbo.[NBrightBuy] as NBclient where NBclient.TypeCode = 'USERPRDXREF' and NBclient.UserId = " + UserController.Instance.GetCurrentUserInfo().UserID.ToString("") + ") ";
                    }

                    // get any plugin data records.
                    var plugindatasql = " and (NB1.TypeCode = '" + datatypecode + "'";

                    if (loadAjaxEntities)
                    {
                        var pluginData = new PluginData(PortalSettings.Current.PortalId);
                        var provList = pluginData.GetAjaxProviders();
                        foreach (var d in provList)
                        {
                            var ajaxprov = AjaxInterface.Instance(d.Key);
                            if (ajaxprov != null)
                            {
                                if (datatypecode != ajaxprov.Ajaxkey)
                                {
                                    plugindatasql += " or NB1.TypeCode = '" + ajaxprov.Ajaxkey + "'";
                                }
                            }
                        }
                    }

                    filter = plugindatasql + ") " + filter;

                    var recordCount = 0;
                    var objCtrl = new NBrightBuyController();

                    if (paging) // get record count for paging
                    {
                        if (pageNumber == 0) pageNumber = 1;
                        if (pageSize == 0) pageSize = 20;

                        // get only entity type required.  Do NOT use typecode, that is set by the filter.
                        recordCount = objCtrl.GetListCount(PortalSettings.Current.PortalId, -1, "", filter, "", EditLangCurrent);

                    }

                    // get selected entitytypecode.
                    var list = objCtrl.GetDataList(PortalSettings.Current.PortalId, -1, "", "", EditLangCurrent, filter, orderby, StoreSettings.Current.DebugMode, "", returnLimit, pageNumber, pageSize, recordCount);

                    return RenderProductAdminList(list,ajaxInfo,recordCount);

                }
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                return ex.ToString();
            }
            return "";
        }

        public String RenderProductAdminList(List<NBrightInfo> list,NBrightInfo ajaxInfo,int recordCount)
        {

            try
            {
                if (NBrightBuyUtils.CheckRights())
                {
                    if (list == null) return "";
                    if (UserController.Instance.GetCurrentUserInfo().UserID <= 0) return "";

                    if (EditLangCurrent == "") EditLangCurrent = Utils.GetCurrentCulture();

                    var strOut = "";

                    // select a specific entity data type for the product (used by plugins)
                    var themeFolder = ajaxInfo.GetXmlProperty("genxml/hidden/themefolder");
                    if (themeFolder == "") themeFolder = "config";
                    var pageNumber = ajaxInfo.GetXmlPropertyInt("genxml/hidden/pagenumber");
                    var pageSize = ajaxInfo.GetXmlPropertyInt("genxml/hidden/pagesize");
                    if (RazorTemplate == "") RazorTemplate = ajaxInfo.GetXmlProperty("genxml/hidden/razortemplate");

                    bool paging = pageSize > 0;

                    var passSettings = new Dictionary<string, string>();
                    foreach (var s in ajaxInfo.ToDictionary())
                    {
                        passSettings.Add(s.Key, s.Value);
                    }
                    foreach (var s in StoreSettings.Current.Settings()) // copy store setting, otherwise we get a byRef assignement
                    {
                        if (passSettings.ContainsKey(s.Key))
                            passSettings[s.Key] = s.Value;
                        else
                            passSettings.Add(s.Key, s.Value);
                    }

                    strOut = NBrightBuyUtils.RazorTemplRenderList(RazorTemplate, 0, "", list, TemplateRelPath, themeFolder, EditLangCurrent, passSettings);

                    // add paging if needed
                    if (paging && (recordCount > pageSize))
                    {
                        var pg = new NBrightCore.controls.PagingCtrl();
                        strOut += pg.RenderPager(recordCount, pageSize, pageNumber);
                    }

                    return strOut;

                }
                return "";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }

        public String MoveProductAdmin(HttpContext context)
        {
            try
            {
                if (NBrightBuyUtils.CheckRights())
                {
                    //get uploaded params
                    var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);
                    var moveproductid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/moveproductid");
                    var movetoproductid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/movetoproductid");
                    var searchcategory = ajaxInfo.GetXmlPropertyInt("genxml/hidden/searchcategory");
                    if (searchcategory > 0 && movetoproductid > 0 && moveproductid > 0)
                    {
                        var objCtrl = new NBrightBuyController();
                        objCtrl.GetListCustom(PortalSettings.Current.PortalId, -1, "NBrightBuy_MoveProductinCateogry", 0, "", searchcategory + ";" + moveproductid + ";" + movetoproductid);
                    }

                    DataCache.ClearCache();
                }
                return ProductAdminList(context, true, EditLangCurrent);
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public String AddModel(HttpContext context)
        {
            try
            {
                if (NBrightBuyUtils.CheckRights())
                {
                    var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);
                    var strOut = "";
                    var selecteditemid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selecteditemid");
                    if (Utils.IsNumeric(selecteditemid))
                    {
                        var themeFolder = ajaxInfo.GetXmlProperty("genxml/hidden/themefolder");
                        if (RazorTemplate == "") RazorTemplate = ajaxInfo.GetXmlProperty("genxml/hidden/razortemplate");
                        var portalId = ajaxInfo.GetXmlPropertyInt("genxml/hidden/portalid");
                        var addqty = ajaxInfo.GetXmlPropertyInt("genxml/hidden/addqty");

                        var passSettings = ajaxInfo.ToDictionary();
                        foreach (var s in StoreSettings.Current.Settings()) // copy store setting, otherwise we get a byRef assignement
                        {
                            if (passSettings.ContainsKey(s.Key))
                                passSettings[s.Key] = s.Value;
                            else
                                passSettings.Add(s.Key, s.Value);
                        }

                        if (!Utils.IsNumeric(selecteditemid)) return "";


                        var itemId = Convert.ToInt32(selecteditemid);
                        var prodData = ProductUtils.GetProductData(itemId, EditLangCurrent,true, EntityTypeCode);
                        var lp = 1;
                        var rtnKeys = new List<String>();
                        while (lp <= addqty)
                        {
                            rtnKeys.Add(prodData.AddNewModel());
                            lp += 1;
                            if (lp > 50) break; // we don;t want to create a stupid amount, it will slow the system!!!
                        }
                        prodData.Save();
                        ProductUtils.RemoveProductDataCache(PortalSettings.Current.PortalId, itemId);
                        NBrightBuyUtils.RemoveModCachePortalWide(prodData.Info.PortalId);

                        if (themeFolder == "")
                        {
                            themeFolder = StoreSettings.Current.ThemeFolder;
                            if (ajaxInfo.GetXmlProperty("genxml/hidden/themefolder") != "") themeFolder = ajaxInfo.GetXmlProperty("genxml/hidden/themefolder");
                        }

                        var objCtrl = new NBrightBuyController();
                        var info = objCtrl.Get(Convert.ToInt32(selecteditemid), EntityTypeCode + "LANG", EditLangCurrent);

                        strOut = NBrightBuyUtils.RazorTemplRender(RazorTemplate, 0, "", info, TemplateRelPath, themeFolder, EditLangCurrent, passSettings);
                    }
                    return strOut;
                }
                return "";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public String AddOption(HttpContext context)
        {
            try
            {
                if (NBrightBuyUtils.CheckRights())
                {
                    var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);
                    var strOut = "";
                    var selecteditemid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selecteditemid");
                    if (selecteditemid > 0)
                    {
                        var themeFolder = ajaxInfo.GetXmlProperty("genxml/hidden/themefolder");
                        if (RazorTemplate == "") RazorTemplate = ajaxInfo.GetXmlProperty("genxml/hidden/razortemplate");
                        var portalId = ajaxInfo.GetXmlPropertyInt("genxml/hidden/portalid");
                        var addqty = ajaxInfo.GetXmlPropertyInt("genxml/hidden/addqty");

                        var passSettings = ajaxInfo.ToDictionary();
                        foreach (var s in StoreSettings.Current.Settings()) // copy store setting, otherwise we get a byRef assignement
                        {
                            if (passSettings.ContainsKey(s.Key))
                                passSettings[s.Key] = s.Value;
                            else
                                passSettings.Add(s.Key, s.Value);
                        }

                        if (!Utils.IsNumeric(selecteditemid)) return "";


                        var itemId = Convert.ToInt32(selecteditemid);
                        var prodData = ProductUtils.GetProductData(itemId, EditLangCurrent, true, EntityTypeCode);
                        var lp = 1;
                        var rtnKeys = new List<String>();
                        while (lp <= addqty)
                        {
                            rtnKeys.Add(prodData.AddNewOption());
                            lp += 1;
                            if (lp > 50) break; // we don;t want to create a stupid amount, it will slow the system!!!
                        }
                        prodData.Save();
                        ProductUtils.RemoveProductDataCache(PortalSettings.Current.PortalId, itemId);
                        NBrightBuyUtils.RemoveModCachePortalWide(prodData.Info.PortalId);

                        var objCtrl = new NBrightBuyController();
                        var info = objCtrl.GetData(selecteditemid, EntityTypeCode + "LANG", EditLangCurrent);

                        if (RazorTemplate == "") RazorTemplate = "Admin_ProductOptions.cshtml";
                        strOut = NBrightBuyUtils.RazorTemplRender(RazorTemplate, 0, "", info, TemplateRelPath, "config", EditLangCurrent, passSettings);

                        NBrightBuyUtils.RemoveModCachePortalWide(prodData.Info.PortalId);
                    }
                    return strOut;
                }
                return "";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public String AddOptionValues(HttpContext context)
        {

            try
            {
                var strOut = "No Product ID ('itemid' hidden fields needed on input form)";
                if (NBrightBuyUtils.CheckRights())
                {

                    //get uploaded params
                    var settings = NBrightBuyUtils.GetAjaxDictionary(context);
                    if (!settings.ContainsKey("itemid")) settings.Add("itemid", "");
                    if (!settings.ContainsKey("addqty")) settings.Add("addqty", "1");
                    if (!settings.ContainsKey("selectedoptionid")) return "";

                    var optionid = settings["selectedoptionid"];
                    var productitemid = settings["selecteditemid"];
                    var qty = settings["addqty"];
                    if (!Utils.IsNumeric(qty)) qty = "1";

                    if (Utils.IsNumeric(productitemid))
                    {
                        var itemId = Convert.ToInt32(productitemid);
                        if (itemId > 0)
                        {

                            var prodData = ProductUtils.GetProductData(itemId, EditLangCurrent, true, EntityTypeCode);


                            var passSettings = settings;
                            foreach (var s in StoreSettings.Current.Settings()) // copy store setting, otherwise we get a byRef assignement
                            {
                                if (passSettings.ContainsKey(s.Key))
                                    passSettings[s.Key] = s.Value;
                                else
                                    passSettings.Add(s.Key, s.Value);
                            }

                            var lp = 1;
                            while (lp <= Convert.ToInt32(qty))
                            {
                                prodData.AddNewOptionValue(optionid);
                                lp += 1;
                                if (lp > 50) break; // we don;t want to create a stupid amount, it will slow the system!!!
                            }
                            prodData.Save();
                            ProductUtils.RemoveProductDataCache(PortalSettings.Current.PortalId, itemId);


                            var objCtrl = new NBrightBuyController();
                            var info = objCtrl.GetData(Convert.ToInt32(productitemid), EntityTypeCode + "LANG", EditLangCurrent);

                            var RazorTemplate = settings["razortemplate"];
                            if (RazorTemplate == "") RazorTemplate = "Admin_ProductOptionValues.cshtml";
                            strOut = NBrightBuyUtils.RazorTemplRender(RazorTemplate, 0, "", info, TemplateRelPath, "config", EditLangCurrent, passSettings);

                            NBrightBuyUtils.RemoveModCachePortalWide(prodData.Info.PortalId);
                        }
                    }
                }
                return strOut;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public String DeleteProduct(HttpContext context)
        {
            try
            {
                if (NBrightBuyUtils.CheckRights())
                {
                    var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);
                    var itemid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selecteditemid");
                    if (itemid > 0)
                    {
                        var prdData = new ProductData(itemid, EditLangCurrent, true, EntityTypeCode);
                        prdData.Delete();
                        ProductUtils.RemoveProductDataCache(PortalSettings.Current.PortalId, itemid);
                        NBrightBuyUtils.RemoveModCachePortalWide(ajaxInfo.PortalId);
                        return "OK";
                   }
                }
                return "";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }


        #region "fileupload"

        public string UpdateProductImages(HttpContext context)
        {
            var strOut = "";
            if (NBrightBuyUtils.CheckRights())
            {

                //get uploaded params
                var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);
                var productitemid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selecteditemid");
                var imguploadlist = ajaxInfo.GetXmlProperty("genxml/hidden/imguploadlist");

                if (Utils.IsNumeric(productitemid))
                {
                    var imgs = imguploadlist.Split(',');
                    foreach (var img in imgs)
                    {
                        if (ImgUtils.IsImageFile(Path.GetExtension(img)) && img != "")
                        {
                            string fullName = StoreSettings.Current.FolderTempMapPath + "\\" + img;
                            if (File.Exists(fullName))
                            {
                                var imgResize = StoreSettings.Current.GetInt(StoreSettingKeys.productimageresize);
                                if (imgResize == 0) imgResize = 960;
                                var imagepath = ResizeImage(fullName, imgResize);
                                var imageurl = StoreSettings.Current.FolderImages.TrimEnd('/') + "/" + Path.GetFileName(imagepath);
                                AddNewImage(Convert.ToInt32(productitemid), imageurl, imagepath);
                            }
                        }
                    }
                    // clear any cache for the product.
                    ProductUtils.RemoveProductDataCache(PortalSettings.Current.PortalId, Convert.ToInt32(productitemid));

                    var cachekey = "AjaxProductImgs*" + productitemid;
                    Utils.RemoveCache(cachekey);


                    var objCtrl = new NBrightBuyController();
                    var info = objCtrl.GetData(Convert.ToInt32(productitemid), EntityTypeCode + "LANG", EditLangCurrent);

                    if (RazorTemplate == "") RazorTemplate = ajaxInfo.GetXmlProperty("genxml/hidden/razortemplate");
                    if (RazorTemplate == "") RazorTemplate = "Admin_ProductImages.cshtml";
                    strOut = NBrightBuyUtils.RazorTemplRender(RazorTemplate, 0, "", info, TemplateRelPath, "config", EditLangCurrent, ajaxInfo.ToDictionary());

                }
            }
            return strOut;
        }


        public String ResizeImage(String fullName, int imgSize, string newFileName, bool useSizeFolder, bool deletefullname = true)
        {
            if (ImgUtils.IsImageFile(Path.GetExtension(fullName)))
            {
                if (newFileName == "")
                {
                    newFileName = Utils.GetUniqueKey();
                }
                var imgFolder = StoreSettings.Current.FolderImagesMapPath.TrimEnd(Convert.ToChar("\\")) + "\\";
                if (useSizeFolder)
                {
                    imgFolder = StoreSettings.Current.FolderImagesMapPath.TrimEnd(Convert.ToChar("\\")) + "\\" + imgSize;
                    if (!Directory.Exists(imgFolder))
                    {
                        Directory.CreateDirectory(imgFolder);
                    }
                    imgFolder += "\\";
                }
                var extension = Path.GetExtension(fullName);
                var newImageFileName = imgFolder + newFileName + extension;
                if (extension != null && extension.ToLower() == ".png")
                {
                    newImageFileName = ImgUtils.ResizeImageToPng(fullName, newImageFileName, imgSize);
                }
                else
                {
                    newImageFileName = ImgUtils.ResizeImageToJpg(fullName, newImageFileName, imgSize);
                }
                if (deletefullname)
                {
                    Utils.DeleteSysFile(fullName);
                }

                return newImageFileName;

            }
            return "";
        }

        public String ResizeImage(String fullName, int imgSize = 960)
        {
            return ResizeImage(fullName, imgSize, "", false);
        }


        public void AddNewImage(int itemId, String imageurl, String imagepath)
        {
            var objCtrl = new NBrightBuyController();
            var dataRecord = objCtrl.Get(itemId);
            if (dataRecord != null)
            {
                var strXml = "<genxml><imgs><genxml><hidden><imagepath>" + imagepath + "</imagepath><imageurl>" + imageurl + "</imageurl></hidden></genxml></imgs></genxml>";
                if (dataRecord.XMLDoc.SelectSingleNode("genxml/imgs") == null)
                {
                    dataRecord.AddXmlNode(strXml, "genxml/imgs", "genxml");
                }
                else
                {
                    dataRecord.AddXmlNode(strXml, "genxml/imgs/genxml", "genxml/imgs");
                }
                objCtrl.Update(dataRecord);
            }
        }


        public string FileUpload(HttpContext context, string itemid = "")
        {
            try
            {

                var strOut = "";
                switch (context.Request.HttpMethod)
                {
                    case "HEAD":
                    case "GET":
                        break;
                    case "POST":
                    case "PUT":
                        strOut = UploadFile(context, itemid);
                        break;
                    case "DELETE":
                        break;
                    case "OPTIONS":
                        break;

                    default:
                        context.Response.ClearHeaders();
                        context.Response.StatusCode = 405;
                        break;
                }

                return strOut;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }

        // Upload file to the server
        public String UploadFile(HttpContext context, string itemid = "")
        {
            var statuses = new List<FilesStatus>();
            var headers = context.Request.Headers;

            if (string.IsNullOrEmpty(headers["X-File-Name"]))
            {
                return UploadWholeFile(context, statuses, itemid);
            }
            else
            {
                return UploadPartialFile(headers["X-File-Name"], context, statuses, itemid);
            }
        }

        // Upload partial file
        public String UploadPartialFile(string fileName, HttpContext context, List<FilesStatus> statuses, string itemid = "")
        {
            Regex fexpr = new Regex(StoreSettings.Current.Get("fileregexpr"));
            if (fexpr.Match(fileName.ToLower()).Success)
            {

                if (itemid != "") itemid += "_";
                if (context.Request.Files.Count != 1) throw new HttpRequestValidationException("Attempt to upload chunked file containing more than one fragment per request");
                var inputStream = context.Request.Files[0].InputStream;
                var fullName = StoreSettings.Current.FolderTempMapPath + "\\" + itemid + fileName;

                using (var fs = new FileStream(fullName, FileMode.Append, FileAccess.Write))
                {
                    var buffer = new byte[1024];

                    var l = inputStream.Read(buffer, 0, 1024);
                    while (l > 0)
                    {
                        fs.Write(buffer, 0, l);
                        l = inputStream.Read(buffer, 0, 1024);
                    }
                    fs.Flush();
                    fs.Close();
                }
                statuses.Add(new FilesStatus(new FileInfo(fullName)));
            }
            return "";
        }

        // Upload entire file
        public String UploadWholeFile(HttpContext context, List<FilesStatus> statuses, string itemid = "")
        {
            if (itemid != "") itemid += "_";
            for (int i = 0; i < context.Request.Files.Count; i++)
            {
                var file = context.Request.Files[i];
                Regex fexpr = new Regex(StoreSettings.Current.Get("fileregexpr"));
                if (fexpr.Match(file.FileName.ToLower()).Success)
                {
                    file.SaveAs(StoreSettings.Current.FolderTempMapPath + "\\" + itemid + file.FileName);
                    statuses.Add(new FilesStatus(Path.GetFileName(itemid + file.FileName), file.ContentLength));
                }
            }
            return "";
        }



        #endregion

        #region "Docs"


        public string UpdateProductDocs(HttpContext context)
        {
            //get uploaded params
            var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
            var settings = ajaxInfo.ToDictionary();

            var strOut = "";

            if (!settings.ContainsKey("itemid")) settings.Add("itemid", "");
            var productitemid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selecteditemid");
            var docuploadlist = ajaxInfo.GetXmlProperty("genxml/hidden/docuploadlist");

            if (Utils.IsNumeric(productitemid))
            {
                var docs = docuploadlist.Split(',');
                foreach (var doc in docs)
                {
                    if (doc != "")
                    {
                        string fullName = StoreSettings.Current.FolderTempMapPath + "\\" + doc;
                        var extension = Path.GetExtension(fullName);
                        if ((extension.ToLower() == ".pdf" || extension.ToLower() == ".zip"))
                        {
                            if (File.Exists(fullName))
                            {
                                var newDocFileName = StoreSettings.Current.FolderDocumentsMapPath.TrimEnd(Convert.ToChar("\\")) + "\\" + Guid.NewGuid() + extension;
                                File.Copy(fullName, newDocFileName, true);
                                var docurl = StoreSettings.Current.FolderDocuments.TrimEnd('/') + "/" + Path.GetFileName(newDocFileName);
                                AddNewDoc(Convert.ToInt32(productitemid), newDocFileName, doc);
                            }
                        }
                    }
                }
                // clear any cache for the product.
                ProductUtils.RemoveProductDataCache(PortalSettings.Current.PortalId, Convert.ToInt32(productitemid));

                var objCtrl = new NBrightBuyController();
                var info = objCtrl.GetData(Convert.ToInt32(productitemid), EntityTypeCode + "LANG", EditLangCurrent);

                if (RazorTemplate == "") RazorTemplate = ajaxInfo.GetXmlProperty("genxml/hidden/razortemplate");
                if (RazorTemplate == "") RazorTemplate = "Admin_ProductDocs.cshtml";
                strOut = NBrightBuyUtils.RazorTemplRender(RazorTemplate, 0, "", info, TemplateRelPath, "config", EditLangCurrent, ajaxInfo.ToDictionary());

            }
            return strOut;
        }

        public void AddNewDoc(int itemId, String filepath, String orginalfilename)
        {
            var objCtrl = new NBrightBuyController();
            var dataRecord = objCtrl.Get(itemId);
            if (dataRecord != null)
            {
                var fileext = Path.GetExtension(orginalfilename);
                var strXml = "<genxml><docs><genxml><hidden><filepath>" + filepath + "</filepath><fileext>" + fileext + "</fileext></hidden><textbox><txtfilename>" + orginalfilename + "</txtfilename></textbox></genxml></docs></genxml>";
                if (dataRecord.XMLDoc.SelectSingleNode("genxml/docs") == null)
                {
                    dataRecord.AddXmlNode(strXml, "genxml/docs", "genxml");
                }
                else
                {
                    dataRecord.AddXmlNode(strXml, "genxml/docs/genxml", "genxml/docs");
                }
                objCtrl.Update(dataRecord);
            }
        }



        #endregion


        #region "Categories"

        public string AddProductCategory(HttpContext context)
        {
            try
            {
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                var parentitemid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selecteditemid");
                var xrefitemid = ajaxInfo.GetXmlProperty("genxml/hidden/selectedcatid");
                if (Utils.IsNumeric(xrefitemid) && Utils.IsNumeric(parentitemid))
                {
                    var prodData = ProductUtils.GetProductData(Convert.ToInt32(parentitemid), EditLangCurrent, false, EntityTypeCode);
                    prodData.AddCategory(Convert.ToInt32(xrefitemid));
                    NBrightBuyUtils.RemoveModCachePortalWide(prodData.Info.PortalId);
                    return GetProductCategories(context);
                }
                return "Invalid parentitemid or xrefitmeid";
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        public string SetDefaultCategory(HttpContext context)
        {
            try
            {
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                var parentitemid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selecteditemid");
                var xrefitemid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selectedcatid");
                if (xrefitemid > 0 && parentitemid > 0)
                {
                    var prodData = ProductUtils.GetProductData(Convert.ToInt32(parentitemid), EditLangCurrent, false, EntityTypeCode);
                    prodData.SetDefaultCategory(Convert.ToInt32(xrefitemid));
                    NBrightBuyUtils.RemoveModCachePortalWide(prodData.Info.PortalId);
                    return GetProductCategories(context);
                }
                return "Invalid parentitemid or xrefitmeid";
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }


        public string RemoveProductCategory(HttpContext context)
        {
            try
            {
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                var parentitemid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selecteditemid");
                var xrefitemid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selectedcatid");
                if (xrefitemid > 0 && parentitemid > 0)
                {
                    var prodData = ProductUtils.GetProductData(Convert.ToInt32(parentitemid), EditLangCurrent, false, EntityTypeCode);
                    prodData.RemoveCategory(Convert.ToInt32(xrefitemid));
                    NBrightBuyUtils.RemoveModCachePortalWide(prodData.Info.PortalId);
                    return GetProductCategories(context);
                }
                return "Invalid parentitemid or xrefitmeid";
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }



        public String GetProductCategories(HttpContext context)
        {
            try
            {
                //get uploaded params
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                var productitemid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selecteditemid");
                var strOut = "";
                var objCtrl = new NBrightBuyController();
                var info = objCtrl.GetData(Convert.ToInt32(productitemid), EntityTypeCode + "LANG", EditLangCurrent);

                if (RazorTemplate == "") RazorTemplate = ajaxInfo.GetXmlProperty("genxml/hidden/razortemplate");
                if (RazorTemplate == "") RazorTemplate = "Admin_ProductCategories.cshtml";
                strOut = NBrightBuyUtils.RazorTemplRender(RazorTemplate, 0, "", info, TemplateRelPath, "config", EditLangCurrent, ajaxInfo.ToDictionary());

                return strOut;

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }



        #endregion


        #region "Properties"

        public String GetPropertyListBox(HttpContext context)
        {
            var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
            ajaxInfo.Lang = Utils.GetCurrentCulture();
            if (RazorTemplate == "") RazorTemplate = ajaxInfo.GetXmlProperty("genxml/hidden/razortemplate");
            if (RazorTemplate == "") RazorTemplate = "Admin_ProductPropertySelect.cshtml";
            var strOut = NBrightBuyUtils.RazorTemplRender(RazorTemplate, 0, "", ajaxInfo, TemplateRelPath, "config", EditLangCurrent, ajaxInfo.ToDictionary());

            return strOut;
        }

        public string AddProperty(HttpContext context)
        {
            try
            {
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                var parentitemid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selecteditemid");
                var xrefitemid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selectedcatid");
                if (xrefitemid > 0 && parentitemid > 0)
                {
                    var prodData = ProductUtils.GetProductData(parentitemid, EditLangCurrent, false, EntityTypeCode);
                    prodData.AddCategory(Convert.ToInt32(xrefitemid));
                    NBrightBuyUtils.RemoveModCachePortalWide(prodData.Info.PortalId);
                    return GetProperties(context);
                }
                return "Invalid parentitemid or xrefitmeid";
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        public string RemoveProperty(HttpContext context)
        {
            try
            {
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                var parentitemid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selecteditemid");
                var xrefitemid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selectedcatid");
                if (xrefitemid > 0 && parentitemid > 0)
                {
                    var prodData = ProductUtils.GetProductData(parentitemid, EditLangCurrent, false);
                    prodData.RemoveCategory(Convert.ToInt32(xrefitemid));
                    NBrightBuyUtils.RemoveModCachePortalWide(prodData.Info.PortalId);
                    return GetProperties(context);
                }
                return "Invalid parentitemid or xrefitmeid";
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        public String GetProperties(HttpContext context)
        {
            try
            {
                //get uploaded params
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                var productitemid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selecteditemid");
                var strOut = "";
                var objCtrl = new NBrightBuyController();
                var info = objCtrl.GetData(Convert.ToInt32(productitemid), EntityTypeCode + "LANG", EditLangCurrent);

                if (RazorTemplate == "") RazorTemplate = ajaxInfo.GetXmlProperty("genxml/hidden/razortemplate");
                if (RazorTemplate == "") RazorTemplate = "Admin_ProductProperties.cshtml";
                strOut = NBrightBuyUtils.RazorTemplRender(RazorTemplate, 0, "", info, TemplateRelPath, "config", EditLangCurrent, ajaxInfo.ToDictionary());

                return strOut;

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }


        #endregion

        #region "related products"

        public string RemoveRelatedProduct(HttpContext context)
        {
            try
            {
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                var productid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selecteditemid");
                var selectedrelatedid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selectedrelatedid");
                if (productid > 0 && selectedrelatedid > 0)
                {
                    var prodData = ProductUtils.GetProductData(Convert.ToInt32(productid), EditLangCurrent, false, EntityTypeCode);
                    prodData.RemoveRelatedProduct(Convert.ToInt32(selectedrelatedid));
                    NBrightBuyUtils.RemoveModCachePortalWide(prodData.Info.PortalId);
                    return GetProductRelated(context);
                }
                return "Invalid itemid or selectedrelatedid";
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        public string AddRelatedProduct(HttpContext context)
        {
            try
            {
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                var productid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selecteditemid");
                var selectedrelatedid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selectedrelatedid");
                if (selectedrelatedid > 0 && productid > 0)
                {
                    var prodData = ProductUtils.GetProductData(Convert.ToInt32(productid), EditLangCurrent, false, EntityTypeCode);
                    if (prodData.Exists) prodData.AddRelatedProduct(Convert.ToInt32(selectedrelatedid));

                    // do bi-direction
                    var prodData2 = ProductUtils.GetProductData(Convert.ToInt32(selectedrelatedid), EditLangCurrent, false, EntityTypeCode);
                    if (prodData2.Exists) prodData2.AddRelatedProduct(Convert.ToInt32(productid));

                    NBrightBuyUtils.RemoveModCachePortalWide(prodData.Info.PortalId);
                    return GetProductRelated(context);
                }
                return "Invalid itemid or selectedrelatedid";
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        public String GetProductRelated(HttpContext context)
        {
            try
            {

                //get uploaded params
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                var productitemid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selecteditemid");
                var strOut = "";
                var objCtrl = new NBrightBuyController();
                var info = objCtrl.GetData(Convert.ToInt32(productitemid), EntityTypeCode + "LANG", EditLangCurrent);

                if (RazorTemplate == "") RazorTemplate = ajaxInfo.GetXmlProperty("genxml/hidden/razortemplate");
                if (RazorTemplate == "") RazorTemplate = "Admin_ProductRelated.cshtml";
                strOut = NBrightBuyUtils.RazorTemplRender(RazorTemplate, 0, "", info, TemplateRelPath, "config", EditLangCurrent, ajaxInfo.ToDictionary());

                return strOut;

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }

        #endregion


        #region "Clients"

        public string RemoveProductClient(HttpContext context)
        {
            try
            {
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                var productid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selecteditemid");
                var selecteduserid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selecteduserid");
                if (selecteduserid > 0 && productid > 0)
                {
                    var prodData = ProductUtils.GetProductData(Convert.ToInt32(productid), EditLangCurrent, false, EntityTypeCode);
                    if (!(NBrightBuyUtils.IsClientOnly() && (Convert.ToInt32(selecteduserid) == UserController.Instance.GetCurrentUserInfo().UserID)))
                    {
                        // ClientEditor role cannot remove themselves.
                        prodData.RemoveClient(Convert.ToInt32(selecteduserid));
                    }
                    NBrightBuyUtils.RemoveModCachePortalWide(prodData.Info.PortalId);
                    return GetProductClients(context);
                }
                return "Invalid itemid or selectedrelatedid";
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        public string AddProductClient(HttpContext context)
        {
            try
            {
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                var productid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selecteditemid");
                var selecteduserid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selecteduserid");
                if (selecteduserid > 0 && productid > 0)
                {
                    var prodData = ProductUtils.GetProductData(Convert.ToInt32(productid), EditLangCurrent, false, EntityTypeCode);
                    if (prodData.Exists) prodData.AddClient(Convert.ToInt32(selecteduserid));

                    NBrightBuyUtils.RemoveModCachePortalWide(prodData.Info.PortalId);
                    return GetProductClients(context);
                }
                return "Invalid itemid or selecteduserid";
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        public string GetClientSelectList(HttpContext context)
        {
            try
            {
                //get uploaded params
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                var productitemid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selecteditemid");
                var searchtext = ajaxInfo.GetXmlProperty("genxml/hidden/searchtext");

                //get data
                var prodData = ProductUtils.GetProductData(productitemid, EditLangCurrent);
                var objCtrl = new NBrightBuyController();
                var userlist = objCtrl.GetDnnUsers(prodData.Info.PortalId, "%" + searchtext + "%", 0, 1, 20, 20);
                var strOut = "";
                if (userlist.Count > 0)
                {
                    if (RazorTemplate == "") RazorTemplate = ajaxInfo.GetXmlProperty("genxml/hidden/razortemplate");
                    if (RazorTemplate == "") RazorTemplate = "Admin_ProductClientSelect.cshtml";
                    strOut = NBrightBuyUtils.RazorTemplRenderList(RazorTemplate, 0, "", userlist, TemplateRelPath, "config", EditLangCurrent, ajaxInfo.ToDictionary());
                }
                return  strOut;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }

        public string GetProductClients(HttpContext context)
        {
            try
            {
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                var productitemid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selecteditemid");
                var strOut = "";
                var objCtrl = new NBrightBuyController();
                var info = objCtrl.GetData(Convert.ToInt32(productitemid), EntityTypeCode + "LANG", EditLangCurrent);

                if (RazorTemplate == "") RazorTemplate = ajaxInfo.GetXmlProperty("genxml/hidden/razortemplate");
                if (RazorTemplate == "") RazorTemplate = "Admin_ProductClients.cshtml";
                strOut = NBrightBuyUtils.RazorTemplRender(RazorTemplate, 0, "", info, TemplateRelPath, "config", EditLangCurrent, ajaxInfo.ToDictionary());

                return strOut;

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }



        #endregion


        #endregion


        #region Ajax ProductList
        public String ProductAjaxViewList(HttpContext context)
        {
            var retval = "";
            var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);

            // get the moduleid, tabid
            var moduleid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/moduleid");
            var tabid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/tabid");

            var ps = PortalSettings.Current;


            var settings = new Hashtable();

            // then add the NBrightBuy settings
            var ModSettings = new ModSettings(moduleid, settings);
            //get Model Level Settings
            var ModuleKey = ModSettings.Get("modref");
            if (String.IsNullOrEmpty(ModuleKey))
                ModuleKey = ModSettings.Get("modulekey"); // keep backward compatiblity with ProductView.

            #region render template stuff

            #region variables
            var _eid = "";
            var _ename = "";
            var _catid = "";
            var _catname = "";
            var _modkey = "";
            var _pagemid = "";
            var _pagenum = "1";
            var _pagesize = "";
            var _templD = "";
            var _displayentrypage = false;
            var _orderbyindex = "";
            var _propertyfilter = "";
            NavigationData _navigationdata;
            var EntityTypeCode = ajaxInfo.GetXmlProperty("genxml/hidden/entitytypecode");
            var EntityTypeCodeLang = ajaxInfo.GetXmlProperty("genxml/hidden/entitytypecode") + "LANG";
            var _itemListName = "";
            var _guidkey = "";
            var _404code = false;
            var returnlimit = 0;
            var _filterTypeInsideProp = "AND";
            var _filterTypeOutsideProp = "AND";

            _catid = ajaxInfo.GetXmlProperty("genxml/hidden/catid");
            _catname = ajaxInfo.GetXmlProperty("genxml/hidden/catref");
            _modkey = ajaxInfo.GetXmlProperty("genxml/hidden/modkey");
            _pagemid = ajaxInfo.GetXmlProperty("genxml/hidden/pagemid");
            _pagenum = ajaxInfo.GetXmlProperty("genxml/hidden/page");
            _pagesize = ajaxInfo.GetXmlProperty("genxml/hidden/pagesize");
            _orderbyindex = ajaxInfo.GetXmlProperty("genxml/hidden/orderby");
            _propertyfilter = ajaxInfo.GetXmlProperty("genxml/hidden/propertyfilter");

            _templD = ModSettings.Get("razorlisttemplate");

            // we're making sure here, that this thing can only be AND or OR to prevent SQL Injection in any case
            if (ajaxInfo.GetXmlProperty("genxml/hidden/propertyfiltertypeinside").ToUpper() == "OR") _filterTypeInsideProp = "OR";
            if (ajaxInfo.GetXmlProperty("genxml/hidden/propertyfiltertypeoutside").ToUpper() == "OR") _filterTypeOutsideProp = "OR";

            //Get returnlimt from module settings
            var strreturnlimit = ModSettings.Get("returnlimit");
            if (Utils.IsNumeric(strreturnlimit)) returnlimit = Convert.ToInt32(strreturnlimit);

            var ModCtrl = new NBrightBuyController();

            // Get meta data from template
            // TODO: dat moeten we hier eigenlijk niet nodig hebben
            // voor nu even handig om die parameters erbij te kunnen halen en ze later om te zetten naar client side rommel

            _navigationdata = new NavigationData(ps.PortalId, ModuleKey);
            var metaTokens = NBrightBuyUtils.RazorPreProcessTempl(_templD, "/DesktopModules/NBright/NBrightBuy", ModSettings.ThemeFolder, Utils.GetCurrentCulture(), ModSettings.Settings(), moduleid.ToString());

            #endregion

            #region "Order BY"

            ////////////////////////////////////////////
            // get ORDERBY SORT 
            ////////////////////////////////////////////
            if (_orderbyindex != "") // if we have orderby set in url, find the meta tags
            {
                if (metaTokens.ContainsKey("orderby" + _orderbyindex))
                {
                    if (metaTokens["orderby" + _orderbyindex].Contains("{") ||
                        metaTokens["orderby" + _orderbyindex].ToLower().Contains("order by"))
                    {
                        _navigationdata.OrderBy = metaTokens["orderby" + _orderbyindex];
                        _navigationdata.OrderByIdx = _orderbyindex;
                    }
                    else
                    {
                        _navigationdata.OrderBy = " Order by " + metaTokens["orderby" + _orderbyindex];
                        _navigationdata.OrderByIdx = _orderbyindex;
                    }
                    _navigationdata.Save();
                }
            }
            else
            {
                if (String.IsNullOrEmpty(_navigationdata.OrderBy) && metaTokens.ContainsKey("orderby"))
                {
                    if (metaTokens["orderby"].Contains("{") || metaTokens["orderby"].ToLower().Contains("order by"))
                    {
                        _navigationdata.OrderBy = metaTokens["orderby"];
                    }
                    else
                    {
                        _navigationdata.OrderBy = " Order by " + metaTokens["orderby"];
                    }
                    _navigationdata.OrderByIdx = "";
                    _navigationdata.Save();
                }
            }


            #endregion

            #region "Get Paging setup"

            //See if we have a pagesize, uses the "searchpagesize" tag token.
            // : This can be overwritten by the cookie value if we need user selection of pagesize.

            #region "Get pagesize, from best place"
            //TODO SK pagesize is already in _pagesize
            var pageSize = 0;
            if (Utils.IsNumeric(_pagesize)) pageSize = Convert.ToInt32(_pagesize);
            //if (Utils.IsNumeric(ModSettings.Get("pagesize"))) pageSize = Convert.ToInt32(ModSettings.Get("pagesize"));
            //// overwrite default module pagesize , if we have a pagesize control in the template
            // TODO SK Don't think I need to know if there's a pagesize selector in the template. it just asks the right pagesize
            // TODO SK Maybe I should know to make it impossible to request more items than configured
            //if (metaTokens.ContainsKey("selectpagesize") && Utils.IsNumeric(_navigationdata.PageSize))
            //{
            //    pageSize = Convert.ToInt32(_navigationdata.PageSize);
            //}
            ////check for url param page size
            //if (Utils.IsNumeric(_pagesize) &&
            //    (_pagemid == "" | _pagemid == moduleid.ToString(CultureInfo.InvariantCulture)))
            //    pageSize = Convert.ToInt32(_pagesize);
            //if (pageSize == 0)
            //{
            //    var strPgSize = "";
            //    if (metaTokens.ContainsKey("searchpagesize")) strPgSize = metaTokens["searchpagesize"];
            //    if (metaTokens.ContainsKey("pagesize") && strPgSize == "") strPgSize = metaTokens["pagesize"];
            //    if (Utils.IsNumeric(strPgSize)) pageSize = Convert.ToInt32(strPgSize);
            //}

            _navigationdata.PageSize = pageSize.ToString("");

            #endregion

            #endregion

            #region "Get filter setup"

            // check the display header to see if we have a sqlfilter defined.
            var strFilter = "";
            var sqlTemplateFilter = "";
            if (metaTokens.ContainsKey("sqlfilter")) sqlTemplateFilter = GenXmlFunctions.StripSqlCommands(metaTokens["sqlfilter"]);

            if (_navigationdata.HasCriteria)
            {
                var paramcatid = _catid;
                if (Utils.IsNumeric(paramcatid))
                {
                    if (_navigationdata.CategoryId != Convert.ToInt32(paramcatid)) // filter mode DOES NOT persist catid (stop confusion when user selects a category)
                    {
                        _navigationdata.ResetSearch();
                    }
                }

                // if navdata is not deleted then get filter from navdata, created by productsearch module.
                strFilter = _navigationdata.Criteria;
                if (!strFilter.Contains(sqlTemplateFilter)) strFilter += " " + sqlTemplateFilter;

                if (_navigationdata.Mode.ToLower() == "s") _navigationdata.ResetSearch(); // single search so clear after
            }
            else
            {
                // reset search if category selected 
                // NOTE: keeping search across categories is VERY confusing for cleint, although it works logically.
                _navigationdata.ResetSearch();
                strFilter = sqlTemplateFilter;
            }

            var pageNumber = 1;
            //check for url param paging
            if (Utils.IsNumeric(_pagenum) && (_pagemid == "" | _pagemid == moduleid.ToString(CultureInfo.InvariantCulture)))
            {
                pageNumber = Convert.ToInt32(_pagenum);
            }

            #endregion

            #region "Get Category select setup"

            var objQual = DotNetNuke.Data.DataProvider.Instance().ObjectQualifier;
            var dbOwner = DotNetNuke.Data.DataProvider.Instance().DatabaseOwner;

            //get default catid.
            var catseo = _catid;
            var defcatid = ModSettings.Get("defaultcatid");
            if (defcatid == "") defcatid = "0";
            if (Utils.IsNumeric(defcatid) && Convert.ToInt32(defcatid) > 0)
            {
                // if we have no filter use the default category
                if (_catid == "" && strFilter.Trim() == "") _catid = defcatid;
            }
            else
            {
                defcatid = ModSettings.Get("defaultpropertyid");
                if (defcatid == "") defcatid = "0";
                if (Utils.IsNumeric(defcatid))
                {
                    // if we have no filter use the default category
                    if (_catid == "" && strFilter.Trim() == "") _catid = defcatid;
                }
            }

            // If we have a list,then always display the default category
            if (ModSettings.Get("staticlist") == "True")
            {
                if (catseo == "") catseo = _catid;
                _catid = defcatid;
                if (ModSettings.Get("chkcascaderesults").ToLower() == "true")
                    strFilter = strFilter + " and NB1.[ItemId] in (select parentitemid from " + dbOwner + "[" + objQual + "NBrightBuy] where (typecode = 'CATCASCADE' or typecode = 'CATXREF') and XrefItemId = " + _catid + ") ";
                else
                    strFilter = strFilter + " and NB1.[ItemId] in (select parentitemid from " + dbOwner + "[" + objQual + "NBrightBuy] where typecode = 'CATXREF' and XrefItemId = " + _catid + ") ";

                if (ModSettings.Get("caturlfilter") == "True" && catseo != "" && catseo != _catid)
                {
                    // add aditional filter for catid filter on url (catseo holds catid from url)
                    if (ModSettings.Get("chkcascaderesults").ToLower() == "true")
                        strFilter = strFilter + " and NB1.[ItemId] in (select parentitemid from " + dbOwner + "[" + objQual + "NBrightBuy] where (typecode = 'CATCASCADE' or typecode = 'CATXREF') and XrefItemId = " + catseo + ") ";
                    else
                        strFilter = strFilter + " and NB1.[ItemId] in (select parentitemid from " + dbOwner + "[" + objQual + "NBrightBuy] where typecode = 'CATXREF' and XrefItemId = " + catseo + ") ";
                }
                // do special custom sort in each cateogry, this passes the catid to the SQL SPROC, whcih process the '{bycategoryproduct}' and orders by product/category seq. 
                if (_navigationdata.OrderBy.Contains("{bycategoryproduct}")) _navigationdata.OrderBy = "{bycategoryproduct}" + _catid;
            }
            else
            {
                #region "use url to get category to display"
                //check if we are display categories 
                // get category list data
                if (_catname != "")
                {
                    _catid = CategoryUtils.GetCatIdFromName(_catname);
                    catseo = _catid;
                }

                if (Utils.IsNumeric(_catid))
                {

                    if (ModSettings.Get("chkcascaderesults").ToLower() == "true")
                        strFilter = strFilter + " and NB1.[ItemId] in (select parentitemid from " + dbOwner + "[" + objQual + "NBrightBuy] where (typecode = 'CATCASCADE' or typecode = 'CATXREF') and XrefItemId = " + _catid + ") ";
                    else
                        strFilter = strFilter + " and NB1.[ItemId] in (select parentitemid from " + dbOwner + "[" + objQual + "NBrightBuy] where typecode = 'CATXREF' and XrefItemId = " + _catid + ") ";

                    if (Utils.IsNumeric(catseo))
                    {
                        var objSEOCat = ModCtrl.GetData(Convert.ToInt32(catseo), "CATEGORYLANG", Utils.GetCurrentCulture());
                        if (objSEOCat != null && _eid == "") // we may have a detail page and listonly module, in which can we need the product detail as page title
                        {
                            //TODO: Should remain in ascx.cs perhaps?
                            //Page Title
                            //var seoname = objSEOCat.GetXmlProperty("genxml/lang/genxml/textbox/txtseoname");
                            //if (seoname == "") seoname = objSEOCat.GetXmlProperty("genxml/lang/genxml/textbox/txtcategoryname");

                            //var newBaseTitle = objSEOCat.GetXmlProperty("genxml/lang/genxml/textbox/txtseopagetitle");
                            //if (newBaseTitle == "") newBaseTitle = objSEOCat.GetXmlProperty("genxml/lang/genxml/textbox/txtseoname");
                            //if (newBaseTitle == "") newBaseTitle = objSEOCat.GetXmlProperty("genxml/lang/genxml/textbox/txtcategoryname");
                            //if (newBaseTitle != "") BasePage.Title = newBaseTitle;
                            ////Page KeyWords
                            //var newBaseKeyWords = objSEOCat.GetXmlProperty("genxml/lang/genxml/textbox/txtmetakeywords");
                            //if (newBaseKeyWords != "") BasePage.KeyWords = newBaseKeyWords;
                            ////Page Description
                            //var newBaseDescription = objSEOCat.GetXmlProperty("genxml/lang/genxml/textbox/txtmetadescription");
                            //if (newBaseDescription == "") newBaseDescription = objSEOCat.GetXmlProperty("genxml/lang/genxml/textbox/txtcategorydesc");
                            //if (newBaseDescription != "") BasePage.Description = newBaseDescription;


                            // Remove canonical link for list.  The Open URL Rewriter (OUR) will create a url that is different to the default SEO url in NBS. 
                            // So to stop clashes it's been disable by default.  The requirment for a canonical link on a category list is more ticking the box than of being any SEO help (might even be causing confusion to Search Engines). 
                            // ** If your a SEO nutcases (or SEO companies pushing for it) then you can uncomment the code below, and you can implement the Open URL Rewriter and canonical link.

                            //if (PortalSettings.HomeTabId == TabId)
                            //    PageIncludes.IncludeCanonicalLink(Page, Globals.AddHTTP(PortalSettings.PortalAlias.HTTPAlias)); //home page always default of site.
                            //else
                            //{
                            //    PageIncludes.IncludeCanonicalLink(Page, NBrightBuyUtils.GetListUrl(PortalId, TabId, objSEOCat.ItemID, seoname, Utils.GetCurrentCulture()));
                            //    // Code required for OUR (if used, test to ensure it works correctly!!)
                            //    //PageIncludes.IncludeCanonicalLink(Page, NBrightBuyUtils.GetListUrl(PortalId, TabId, objSEOCat.ItemID, "", Utils.GetCurrentCulture()));
                            //}
                        }
                    }

                    // do special custom sort in each cateogry, this passes the catid to the SQL SPROC, whcih process the '{bycategoryproduct}' and orders by product/category seq. 
                    if (_navigationdata.OrderBy.Contains("{bycategoryproduct}")) _navigationdata.OrderBy = "{bycategoryproduct}" + _catid;

                }
                else
                {
                    if (!_navigationdata.FilterMode) _navigationdata.CategoryId = 0; // filter mode persist catid
                    if (_navigationdata.OrderBy.Contains("{bycategoryproduct}")) _navigationdata.OrderBy = " Order by ModifiedDate DESC  ";
                }

                #endregion
            }

            // This allows the return to the same category after a returning from a entry view. + Gives support for current category in razor tokens
            if (Utils.IsNumeric(_catid)) _navigationdata.CategoryId = Convert.ToInt32(_catid);

            #endregion

            #region "Apply provider product filter"

            // Special filtering can be done, by using the ProductFilter interface.
            var productfilterkey = "";
            if (metaTokens.ContainsKey("providerfilterkey")) productfilterkey = metaTokens["providerfilterkey"];
            if (productfilterkey != "")
            {
                var provfilter = FilterInterface.Instance(productfilterkey);
                if (provfilter != null) strFilter = provfilter.GetFilter(strFilter, _navigationdata, ModSettings, context);
            }

            #endregion

            #region "itemlists (wishlist)"

            // if we have a itemListName field then get the itemlist cookie.
            if (ModSettings.Get("displaytype") == "2") // displaytype 2 = "selected list"
            {

                var cw = new ItemListData(PortalSettings.Current.PortalId,UserController.Instance.GetCurrentUserInfo().UserID);
                if (cw.Exists && cw.ItemCount > 0)
                {
                    strFilter = " and (";
                    foreach (var i in cw.GetItemList())
                    {
                        strFilter += " NB1.itemid = '" + i + "' or";
                    }
                    strFilter = strFilter.Substring(0, (strFilter.Length - 3)) + ") ";
                    // remove the last "or"                    
                }
                else
                {
                    //no data in list so select false itemid to stop anything displaying
                    strFilter += " and (NB1.itemid = '-1') ";
                }

            }

            #endregion

            #region apply ajax property filter

            if (!string.IsNullOrEmpty(_propertyfilter))
            {
                var propIds = new List<string>();
                var groupPropIds = new Dictionary<string, List<string>>();
                foreach (string grpPropId in _propertyfilter.Split(','))
                {
                    if (!String.IsNullOrEmpty(grpPropId) && grpPropId.Contains("-"))
                    {
                        var groupId = grpPropId.Split('-')[0];
                        var propId = grpPropId.Split('-')[1];

                        if (!groupPropIds.ContainsKey(groupId)) groupPropIds.Add(groupId, new List<string>());

                        groupPropIds[groupId].Add(propId);
                    }
                }

                var sqlGroupFilter = "";
                foreach (var groupPropId in groupPropIds.Keys)
                {
                    if (!String.IsNullOrEmpty(sqlGroupFilter))
                    {
                        sqlGroupFilter += $" {_filterTypeOutsideProp} ";
                    }

                    var sqlPropFilter = "";
                    foreach (var propId in groupPropIds[groupPropId])
                    {
                        if (!String.IsNullOrEmpty(sqlPropFilter))
                        {
                            sqlPropFilter += $" {_filterTypeInsideProp} ";
                        }
                        sqlPropFilter += $"NB1.[ItemId] in (select parentitemid from {dbOwner}[{objQual}NBrightBuy] where typecode = 'CATXREF' and XrefItemId = {propId}) ";
                    }
                    sqlGroupFilter += $" ({sqlPropFilter}) ";
                }

                //foreach (var propId in propIds)
                //{
                //    if (!String.IsNullOrEmpty(sqlPropFilter))
                //    {
                //        sqlPropFilter += " AND ";
                //    }
                //    sqlPropFilter += $"NB1.[ItemId] in (select parentitemid from {dbOwner}[{objQual}NBrightBuy] where typecode = 'CATXREF' and XrefItemId = {propId}) ";
                //}
                if (!String.IsNullOrEmpty(sqlGroupFilter))
                {
                    strFilter += $" AND ({sqlGroupFilter}) ";
                }
            }

            #endregion

            // save navigation data
            _navigationdata.PageModuleId = Utils.RequestParam(context, "pagemid");
            _navigationdata.PageNumber = Utils.RequestParam(context, "page");
            if (Utils.IsNumeric(_catid)) _navigationdata.PageName = NBrightBuyUtils.GetCurrentPageName(Convert.ToInt32(_catid));

            // save the last active modulekey to a cookie, so it can be used by the "NBrightBuyUtils.GetReturnUrl" function
            NBrightCore.common.Cookie.SetCookieValue(ps.PortalId, "NBrigthBuyLastActive", "ModuleKey", ModuleKey, 1);

            if (strFilter.Trim() == "")
            {
                //TODO: Check, but this should not be possible
                // if at this point we have no filter, then assume we're using urlrewriter and a 404 url has been entered.
                // rather than display all visible products in a list with no default.
                // redirect to the product display function, so we can display a 404 and product not found.
                //RazorDisplayDataEntry(_eid);
            }
            else
            {

                strFilter += " and (NB3.Visible = 1) "; // get only visible products

                var recordCount = ModCtrl.GetDataListCount(ps.PortalId, moduleid, EntityTypeCode, strFilter, EntityTypeCodeLang, Utils.GetCurrentCulture(), DebugMode);

                _navigationdata.RecordCount = recordCount.ToString("");
                _navigationdata.Save();

                if (returnlimit > 0 && returnlimit < recordCount) recordCount = returnlimit;

                // **** check if we already have the template cached, if so no need for DB call or razor call ****
                // get same cachekey used for DB return, and use for razor.
                var razorcachekey = ModCtrl.GetDataListCacheKey(ps.PortalId, moduleid, EntityTypeCode, EntityTypeCodeLang, Utils.GetCurrentCulture(), strFilter, _navigationdata.OrderBy, DebugMode, "", returnlimit, pageNumber, pageSize, recordCount);
                var cachekey = "NBrightBuyRazorOutput" + _templD + "*" + razorcachekey + ps.PortalId.ToString();
                retval = (String)NBrightBuyUtils.GetModCache(cachekey);
                if (retval == null || DebugMode)
                {
                    var l = ModCtrl.GetDataList(ps.PortalId, moduleid, EntityTypeCode, EntityTypeCodeLang, Utils.GetCurrentCulture(), strFilter, _navigationdata.OrderBy, DebugMode, "", returnlimit, pageNumber, pageSize, recordCount);
                    if (!ModSettings.Settings().ContainsKey("recordcount")) ModSettings.Settings().Add("recordcount", "");
                    ModSettings.Settings()["recordcount"] = recordCount.ToString();
                    retval = NBrightBuyUtils.RazorTemplRenderList(_templD, moduleid, razorcachekey, l, "/DesktopModules/NBright/NBrightBuy", ModSettings.ThemeFolder, Utils.GetCurrentCulture(), ModSettings.Settings());
                }

                //SK phData.Controls.Add(lit);

                // add paging if needed
                if (recordCount > pageSize)
                {
                    if (pageSize == 0) pageSize = 12;
                    var pgdata = NBrightBuyUtils.GetPagingData(recordCount, pageSize, pageNumber);
                    var strPg = NBrightBuyUtils.RazorTemplRenderList(Path.GetFileNameWithoutExtension(_templD) + "_paging" + Path.GetExtension(_templD), moduleid, razorcachekey + "PG", pgdata, "/DesktopModules/NBright/NBrightBuy", ModSettings.ThemeFolder, Utils.GetCurrentCulture(), ModSettings.Settings());
                    retval += strPg;
                }

                if (_navigationdata.SingleSearchMode) _navigationdata.ResetSearch();
            }

            #endregion


            return retval;
        }

        public String ProductAjaxFilter(HttpContext context)
        {
            var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);

            // get the moduleid, tabid
            var moduleid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/moduleid");
            if (!Utils.IsNumeric(moduleid))
            {
                return "No moduleid passed to server";
            }
            var settings = new Hashtable();

            // then add the NBrightBuy settings
            var ModSettings = new ModSettings(moduleid, settings);

            var RazorTemplate = ModSettings.Get("razortemplate");
            var providercontrolpath = "/DesktopModules/NBright/" + ModSettings.Get("providercontrolpath") + "/";
            var ThemeFolder = ModSettings.Get("themefolder");
            var catname = ajaxInfo.GetXmlProperty("genxml/hidden/catref");
            if (catname != "" && ajaxInfo.GetXmlPropertyInt("genxml/hidden/catid") == 0)
            {
                ajaxInfo.SetXmlProperty("genxml/hidden/catid", CategoryUtils.GetCatIdFromName(catname));
            }

            var strOut = NBrightBuyUtils.RazorTemplRender(RazorTemplate, -1, "", ajaxInfo, providercontrolpath, ThemeFolder, UiLang, ModSettings.Settings());

            return strOut;
        }
        #endregion


    }
}
