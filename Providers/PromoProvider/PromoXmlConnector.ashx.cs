using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Web;
using System.Xml;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using NBrightCore;
using NBrightCore.common;
using NBrightCore.images;
using NBrightCore.render;
using NBrightDNN;
using DataProvider = DotNetNuke.Data.DataProvider;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Services.Exceptions;
using Nevoweb.DNN.NBrightBuy.Components;

namespace Nevoweb.DNN.NBrightBuy.Providers.PromoProvider
{
    /// <summary>
    /// Summary description for XMLconnector
    /// </summary>
    public class PromoXmlConnector : IHttpHandler
    {
        private String _lang = "";
        private String _itemid = "";

        public void ProcessRequest(HttpContext context)
        {
            #region "Initialize"

            var strOut = "";
            try
            {

                var paramCmd = Utils.RequestQueryStringParam(context, "cmd");
                _itemid = Utils.RequestQueryStringParam(context, "itemid");

                #region "setup language"

                SetContextLangauge(context);

                #endregion

                #endregion

                #region "Do processing of command"

                strOut = "ERROR!! - No Security rights for current user!";
                if (CheckRights())
                {
                    switch (paramCmd)
                    {
                        case "test":
                            strOut = "<root>" + UserController.Instance.GetCurrentUserInfo().Username + "</root>";
                            break;
                        case "getdata":
                            strOut = GetData(context);
                            break;
                        case "addnew":
                            strOut = GetData(context, true);
                            break;
                        case "deleterecord":
                            strOut = DeleteData(context);
                            break;
                        case "savedata":
                            strOut = SaveData(context);
                            break;
                        case "selectlang":
                            strOut = SaveData(context);
                            break;
                    }
                }

                #endregion

            }
            catch (Exception ex)
            {
                strOut = ex.ToString();
                Logging.LogException(ex);
            }


            #region "return results"

            //send back xml as plain text
            context.Response.Clear();
            context.Response.ContentType = "text/plain";
            context.Response.Write(strOut);
            context.Response.End();

            #endregion

        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        private void SetContextLangauge(HttpContext context)
        {
            var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);
            SetContextLangauge(ajaxInfo); // Ajax breaks context with DNN, so reset the context language to match the client.
        }

        private void SetContextLangauge(NBrightInfo ajaxInfo = null)
        {
            // NOTE: "genxml/hidden/lang" should be set in the template for langauge to work OK.
            // set langauge if we have it passed.
            if (ajaxInfo == null) ajaxInfo = new NBrightInfo(true);
            var lang = ajaxInfo.GetXmlProperty("genxml/hidden/currentlang");
            if (lang == "") lang = Utils.RequestParam(HttpContext.Current, "langauge"); // fallbacl
            if (lang == "") lang = ajaxInfo.GetXmlProperty("genxml/hidden/lang"); // fallbacl
            if (lang == "") lang = Utils.GetCurrentCulture(); // fallback, but very often en-US on ajax call
            // set the context  culturecode, so any DNN functions use the correct culture 
            if (lang != "" && lang != System.Threading.Thread.CurrentThread.CurrentCulture.ToString()) System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo(lang);

        }

        #region "Methods"

        private String GetData(HttpContext context,bool clearCache = false)
        {

                var objCtrl = new NBrightBuyController();
                var strOut = "";
                //get uploaded params
                var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);
                SetContextLangauge(ajaxInfo); // Ajax breaks context with DNN, so reset the context language to match the client.

                var itemid = ajaxInfo.GetXmlProperty("genxml/hidden/itemid");
                var typeCode = ajaxInfo.GetXmlProperty("genxml/hidden/typecode");
                var newitem = ajaxInfo.GetXmlProperty("genxml/hidden/newitem");                
                var selecteditemid = ajaxInfo.GetXmlProperty("genxml/hidden/selecteditemid");
                var moduleid = ajaxInfo.GetXmlProperty("genxml/hidden/moduleid");
                var editlang = ajaxInfo.GetXmlProperty("genxml/hidden/editlang");
                if (editlang == "") editlang = _lang;

                if (!Utils.IsNumeric(moduleid)) moduleid = "-2"; // use moduleid -2 for razor

                if (clearCache) NBrightBuyUtils.RemoveModCache(Convert.ToInt32(moduleid));

                if (newitem == "new") selecteditemid = AddNew(moduleid, typeCode);

                var templateControl = "/DesktopModules/NBright/NBrightBuy/Providers/PromoProvider";

                if (Utils.IsNumeric(selecteditemid))
                {
                    // do edit field data if a itemid has been selected
                    var obj = objCtrl.Get(Convert.ToInt32(selecteditemid), "",editlang);
                    strOut = NBrightBuyUtils.RazorTemplRender(typeCode.ToLower() + "fields.cshtml", Convert.ToInt32(moduleid), _lang + itemid + editlang + selecteditemid, obj, templateControl, "config", _lang, StoreSettings.Current.Settings());
                }
                else
                {
                    // Return list of items
                    var l = objCtrl.GetList(PortalSettings.Current.PortalId, Convert.ToInt32(moduleid), typeCode, "", " order by [XMLData].value('(genxml/textbox/validuntil)[1]','nvarchar(50)'), ModifiedDate desc", 0, 0, 0, 0, editlang);
                    strOut = NBrightBuyUtils.RazorTemplRenderList(typeCode.ToLower() + "list.cshtml", Convert.ToInt32(moduleid), _lang + editlang, l, templateControl, "config",_lang,StoreSettings.Current.Settings());
                }

                return strOut;

        }

        private String AddNew(String moduleid,String typeCode)
        {
            if (!Utils.IsNumeric(moduleid)) moduleid = "-2"; // -2 for razor

            var objCtrl = new NBrightBuyController();
            var nbi = new NBrightInfo(true);
            nbi.PortalId = PortalSettings.Current.PortalId;
            nbi.TypeCode = typeCode;
            nbi.ModuleId = Convert.ToInt32(moduleid);
            nbi.ItemID = -1;
            if (typeCode == "DISCOUNTCODE")
            {
                nbi.SetXmlProperty("genxml/textbox/code", Utils.GetUniqueKey().ToUpper());
                nbi.GUIDKey = nbi.GetXmlProperty("genxml/textbox/code");
            }
            var itemId = objCtrl.Update(nbi);
            nbi.ItemID = itemId;

            foreach (var lang in DnnUtils.GetCultureCodeList(PortalSettings.Current.PortalId))
            {
                var nbi2 = new NBrightInfo(true);
                nbi2.PortalId = PortalSettings.Current.PortalId;
                nbi2.TypeCode = typeCode + "LANG";
                nbi2.ModuleId = Convert.ToInt32(moduleid);
                nbi2.ItemID = -1;
                nbi2.Lang = lang;
                nbi2.ParentItemId = itemId;
                nbi2.GUIDKey = "";
                nbi2.ItemID = objCtrl.Update(nbi2);
            }

            NBrightBuyUtils.RemoveModCache(nbi.ModuleId);

            return nbi.ItemID.ToString("");
        }

        private String SaveData(HttpContext context)
        {
                var objCtrl = new NBrightBuyController();

                //get uploaded params
                var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);
                SetContextLangauge(ajaxInfo); // Ajax breaks context with DNN, so reset the context language to match the client.

                var itemid = ajaxInfo.GetXmlProperty("genxml/hidden/itemid");
                var lang = ajaxInfo.GetXmlProperty("genxml/hidden/editlang");
                if (lang == "") lang = ajaxInfo.GetXmlProperty("genxml/hidden/lang");
                if (lang == "") lang = _lang;

                if (Utils.IsNumeric(itemid))
                {
                    // get DB record
                    var nbi = objCtrl.Get(Convert.ToInt32(itemid));
                    if (nbi != null)
                    {
                        var typecode = nbi.TypeCode;

                        // get data passed back by ajax
                        var strIn = HttpUtility.UrlDecode(Utils.RequestParam(context, "inputxml"));
                        // update record with ajax data
                        nbi.UpdateAjax(strIn);
                        nbi.GUIDKey = nbi.GetXmlProperty("genxml/textbox/code");
                        objCtrl.Update(nbi);

                        // do langauge record
                        var nbi2 = objCtrl.GetDataLang(Convert.ToInt32(itemid), lang);
                        nbi2.UpdateAjax(strIn);
                        objCtrl.Update(nbi2);

                        DataCache.ClearCache(); // clear ALL cache.

                        // run the promo now.
                        if (typecode == "CATEGORYPROMO")
                        {
                            PromoUtils.CalcGroupPromoItem(nbi);
                        }
                        if (typecode == "MULTIBUYPROMO")
                        {
                            PromoUtils.CalcMultiBuyPromoItem(nbi);
                        }

                    }
                }
                return "";
        }

        private String DeleteData(HttpContext context)
        {
            var objCtrl = new NBrightBuyController();

            //get uploaded params
            var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);
            var itemid = ajaxInfo.GetXmlProperty("genxml/hidden/itemid");
            if (Utils.IsNumeric(itemid))
            {
                var nbi = objCtrl.Get(Convert.ToInt32(itemid));
                if (nbi != null)
                {
                    var typecode = nbi.TypeCode;

                    // run the promo before delete, so we remove any promo data that may exist.
                    if (typecode == "CATEGORYPROMO")
                    {
                        PromoUtils.RemoveGroupProductPromo(PortalSettings.Current.PortalId, nbi.ItemID);
                    }
                    if (typecode == "MULTIBUYPROMO")
                    {
                        PromoUtils.RemoveMultiBuyProductPromo(PortalSettings.Current.PortalId, nbi.ItemID);
                    }

                    // delete DB record
                    objCtrl.Delete(nbi.ItemID);

                }

                NBrightBuyUtils.RemoveModCache(-2);

            }
            return "";
        }

        #endregion


        private Boolean CheckRights()
        {
            if (UserController.Instance.GetCurrentUserInfo().IsInRole(StoreSettings.ManagerRole) || UserController.Instance.GetCurrentUserInfo().IsInRole(StoreSettings.EditorRole) || UserController.Instance.GetCurrentUserInfo().IsInRole("Administrators"))
            {
                return true;
            }
            return false;
        }



    }
}