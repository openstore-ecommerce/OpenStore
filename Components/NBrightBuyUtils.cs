using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Resources;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;
using NBrightBuy.render;
using NBrightCore.TemplateEngine;
using NBrightCore.common;
using NBrightCore.providers;
using NBrightCore.render;
using NBrightDNN;
using NBrightDNN.render;
using Nevoweb.DNN.NBrightBuy.Components.Interfaces;
using RazorEngine;
using RazorEngine.Configuration;
using MailPriority = DotNetNuke.Services.Mail.MailPriority;
using RazorEngine.Templating;
using Encoding = System.Text.Encoding;

namespace Nevoweb.DNN.NBrightBuy.Components
{
    public static class NBrightBuyUtils
    {

        /// <summary>
        /// Used to get the template system getter control.
        /// </summary>
        /// <returns></returns>
        public static TemplateGetter GetTemplateGetter(int portalId, string themeFolder)
        {
            var controlMapPath = System.Web.Hosting.HostingEnvironment.MapPath(StoreSettings.NBrightBuyPath());
            themeFolder = "Themes\\" + themeFolder;
            var map = "";
            var storeThemeFolder = "";
            if (PortalSettings.Current == null) // might be ran from scheduler
            {
                var portalsettings = NBrightDNN.DnnUtils.GetPortalSettings(portalId);
                map = portalsettings.HomeDirectoryMapPath;
                var storeset = new StoreSettings(portalId);
                storeThemeFolder = storeset.ThemeFolder;
            }
            else
            {
                map = PortalSettings.Current.HomeDirectoryMapPath;
                storeThemeFolder = StoreSettings.Current.ThemeFolder;
            }
            var templCtrl = new NBrightCore.TemplateEngine.TemplateGetter(map, controlMapPath, "Themes\\config", themeFolder, "Themes\\" + storeThemeFolder);
            return templCtrl;
        }

        public static TemplateGetter GetTemplateGetter(string themeFolder)
        {
            return GetTemplateGetter(PortalSettings.Current.PortalId, themeFolder);
        }

        public static NBrightInfo GetSettings(int portalId, int moduleId, string ctrlTypeCode = "", bool useCache = true)
        {
            var obj = (NBrightInfo) GetModCache("NBright_NBsettings" + portalId.ToString("") + "_" + moduleId.ToString(""));
            if (obj == null | !useCache)
            {
                // single record for EntityTypeCode settings, so get record directly.
                var objCtrl = new NBrightBuyController();
                obj = objCtrl.GetByType(portalId, moduleId, "SETTINGS");
                if (obj == null)
                {
                    obj = new NBrightInfo {ItemID = -1};
                    obj.TypeCode = "SETTINGS";
                    obj.ModuleId = moduleId;
                    obj.PortalId = portalId;
                    obj.XMLData = "<genxml><hidden></hidden><textbox></textbox></genxml>";
                    obj.UserId = -1;
                    obj.GUIDKey = ctrlTypeCode;
                }
                SetModCache(moduleId, "NBright_NBsettings" + portalId.ToString("") + "_" + moduleId.ToString(""), obj);
            }
            return obj;
        }

        public static Dictionary<string,string> GetPassSettings(NBrightInfo ajaxInfo)
        {
            var passSettings = ajaxInfo.ToDictionary();
            foreach (var s in StoreSettings.Current.Settings()) // copy store setting, otherwise we get a byRef assignement
            {
                if (passSettings.ContainsKey(s.Key))
                    passSettings[s.Key] = s.Value;
                else
                    passSettings.Add(s.Key, s.Value);
            }
            foreach (var s in StoreSettings.Current.Settings()) // copy store setting, otherwise we get a byRef assignement
            {
                if (passSettings.ContainsKey(s.Key))
                    passSettings[s.Key] = s.Value;
                else
                    passSettings.Add(s.Key, s.Value);
            }
            return passSettings;
        }



        public static string FormatListtoXml(IEnumerable<NBrightInfo> objList)
        {
            var xmlOut = "";
            foreach (var obj in objList)
            {
                if (obj.XMLData.StartsWith("<root>"))
                {
                    // Data is Xref, so read actual data required.
                    var xrefId = GenXmlFunctions.GetGenXmlValue(obj.XMLData, "root/xref");
                    if (Utils.IsNumeric(xrefId))
                    {
                        var objCtrl = new Components.NBrightBuyController();
                        var obj2 = objCtrl.Get(Convert.ToInt32(xrefId));
                        if (obj2 != null)
                        {
                            var linedxrefId = GenXmlFunctions.GetGenXmlValue(obj.XMLData, "root/linkedxref");
                            xmlOut += "<item id=\"" + obj.ItemID.ToString("") + "\" idxref=\"" + linedxrefId + "\" modifieddate=\"" + obj.ModifiedDate.ToString("s") + "\">" + obj2.XMLData + "</item>";
                        }
                    }
                }
                else
                {
                    xmlOut += "<item id=\"" + obj.ItemID.ToString("") + "\" modifieddate=\"" + obj.ModifiedDate.ToString("s") + "\">" + obj.XMLData + "</item>";
                }
            }
            return xmlOut;
        }

        public static string DoXslTransOnTemplate(string strTemplateText, NBrightInfo objInfo)
        {
            if (strTemplateText.ToLower().Contains("<xsl:stylesheet"))
            {
                var xmlOut = "<root>";
                var l = new List<NBrightInfo> {objInfo};
                xmlOut += NBrightBuyUtils.FormatListtoXml(l);
                xmlOut += "</root>";
                return XslUtils.XslTransInMemory(xmlOut, strTemplateText);
            }
            return strTemplateText;
        }

        public static string GetUniqueKeyRef(int PortalId, int ModuleId, string KeyRef, int LoopCount)
        {
            var rtnKeyRef = KeyRef;
            if (LoopCount > 0) rtnKeyRef += LoopCount.ToString("");

            var strFilter = " and ModuleId != " + ModuleId + " and GUIDKey = '" + rtnKeyRef + "' ";

            var l = CBO.FillCollection<NBrightInfo>(DataProvider.Instance().GetList(PortalId, -1, "SETTINGS", strFilter, "", 1, 1, 1, 1, "", ""));
            if (l.Count >= 1)
            {
                rtnKeyRef = GetUniqueKeyRef(PortalId, ModuleId, KeyRef, LoopCount + 1);
            }

            return rtnKeyRef;
        }


        #region "Build Urls"

        public static string GetEntryUrl(int portalId, string entryid, string modulekey, string seoname, string tabid, string catid = "", string catref = "")
        {

            var rdTabid = -1;
            var objTabInfo = new TabInfo();

            if (Utils.IsNumeric(tabid) && Convert.ToInt32(tabid) > 0) rdTabid = Convert.ToInt32(tabid);
            if (!Utils.IsNumeric(tabid)) rdTabid = StoreSettings.Current.ProductDetailTabId;
            if ((!Utils.IsNumeric(rdTabid) || rdTabid == -1) && PortalSettings.Current != null)
            {
                objTabInfo = PortalSettings.Current.ActiveTab;
                rdTabid = objTabInfo.TabID;
            }

            var cachekey = "entryurl*" + entryid + "*" + catid + "*" + catref + "*" + modulekey + "*" + rdTabid + "*" + Utils.GetCurrentCulture();
            var urldata = "";
            var chacheData = Utils.GetCache(cachekey);
            if (chacheData != null) return (string) chacheData;


            if (objTabInfo.TabID != rdTabid)
            {
                var portalTabs = NBrightDNN.DnnUtils.GetPortalTabs(portalId);
                if (portalTabs != null && portalTabs.ContainsKey(Convert.ToInt32(rdTabid)))
                {
                    var rTab = portalTabs[Convert.ToInt32(rdTabid)];
                    if (rTab != null) objTabInfo = rTab;
                }
            }

            // make sure we have the correct culturecode, fo OpenUrlrewriter
            objTabInfo.CultureCode = Utils.GetCurrentCulture();

            var rdModId = "";
            if (modulekey != "") rdModId = "&modkey=" + modulekey;

            var strurl = "~/Default.aspx?tabid=" + rdTabid + rdModId;

            if (Utils.IsNumeric(catid))
            {
                if (!strurl.EndsWith("?")) strurl += "&";
                strurl += "catid=" + catid;
            }

            if (Utils.IsNumeric(entryid))
            {
                if (!strurl.Contains("catid="))
                {
                    var prdData = ProductUtils.GetProductData(Convert.ToInt32(entryid), Utils.GetCurrentCulture());
                    var defcat = prdData.GetDefaultCategory();
                    if (defcat != null && defcat.categoryid > 0 && !strurl.EndsWith("?"))
                    {
                        strurl += "&";
                        strurl += "catid=" + defcat.categoryid;
                    }
                }
                if (!strurl.EndsWith("?")) strurl += "&";
                strurl += "eid=" + entryid;
            }
            seoname = Utils.UrlFriendly(seoname);

            urldata = DotNetNuke.Services.Url.FriendlyUrl.FriendlyUrlProvider.Instance().FriendlyUrl(objTabInfo, strurl, seoname);

            Utils.SetCache(cachekey, urldata);

            return urldata;
        }


        public static string GetListUrl(int portalId, int tabId, int catId, string seoName, string lang)
        {
            seoName = Utils.CleanInput(seoName);
            if (seoName == "") seoName = "page";
            if (catId == -1) return GetSEOLink(portalId, tabId, "", seoName + ".aspx", "language=" + lang);
            return GetSEOLink(portalId, tabId, "", seoName + ".aspx", "CatID=" + catId.ToString(""), "language=" + lang);
        }

        public static string GetSEOLink(int portalId, int tabId, string controlKey, string title, params string[] additionalParameters)
        {

            DotNetNuke.Entities.Tabs.TabInfo tabInfo = (new DotNetNuke.Entities.Tabs.TabController()).GetTab(tabId, portalId, false);
            var langList = DnnUtils.GetCultureCodeList(portalId);


            if ((tabInfo != null))
            {
                string Path = "~/default.aspx?tabid=" + tabInfo.TabID;

                foreach (string p in additionalParameters)
                {
                    if (langList.Count > 1)
                    {
                        Path += "&" + p;
                    }
                    else
                    {
                        //only one langauge so don't add the langauge param.
                        if (!p.ToLower().StartsWith("language"))
                        {
                            Path += "&" + p;
                        }
                    }
                }
                if (string.IsNullOrEmpty(title)) title = "Default.aspx";

                // make sure we have the correct culturecode, fo OpenUrlrewriter
                tabInfo.CultureCode = Utils.GetCurrentCulture();

                var url = DotNetNuke.Services.Url.FriendlyUrl.FriendlyUrlProvider.Instance().FriendlyUrl(tabInfo, "~/Default.aspx?tabid=" + tabInfo.TabID.ToString(""), title);
                return url;
            }
            return "";
        }


        public static string GetReturnUrl(string tabid)
        {

            var redirectTab = DotNetNuke.Entities.Portals.PortalSettings.Current.ActiveTab;
            var rdTabid = redirectTab.TabID;

            if (Utils.IsNumeric(tabid))
            {
                if (Convert.ToInt32(tabid) > 0)
                {
                    rdTabid = Convert.ToInt32(tabid);
                }
            }

            if (Utils.IsNumeric(rdTabid))
            {
                if (Convert.ToInt32(rdTabid) != redirectTab.TabID)
                {
                    var portalTabs = NBrightDNN.DnnUtils.GetPortalTabs(PortalSettings.Current.PortalId);
                    if (portalTabs != null)
                    {
                        var rTab = portalTabs[Convert.ToInt32(rdTabid)];
                        if (rTab != null)
                        {
                            redirectTab = rTab;
                        }
                    }
                }
            }

            //get last active modulekey from cookie.
            var moduleKey = NBrightCore.common.Cookie.GetCookieValue(PortalSettings.Current.PortalId, "NBrigthBuyLastActive", "ModuleKey");

            var pagename = redirectTab.TabName + ".aspx";
            var page = "";
            var pagemid = "";
            var catid = "";
            var navigationdata = new NavigationData(PortalSettings.Current.PortalId, moduleKey);
            if (navigationdata.PageNumber != "") page = "&page=" + navigationdata.PageNumber;
            if (navigationdata.PageModuleId != "") pagemid = "&pagemid=" + navigationdata.PageModuleId;
            if (navigationdata.CategoryId > 0) catid = "&catid=" + navigationdata.CategoryId;
            if (navigationdata.PageName != "") pagename = navigationdata.PageName + ".aspx";
            var url = DotNetNuke.Services.Url.FriendlyUrl.FriendlyUrlProvider.Instance().FriendlyUrl(redirectTab, "~/Default.aspx?tabid=" + redirectTab.TabID.ToString("") + page + pagemid + catid, pagename);

            return url;

        }

        public static string GetCurrentPageName(int catid)
        {
            var newBaseName = PortalSettings.Current.ActiveTab.TabName;
            var objCtrl = new NBrightBuyController();
            var catInfo = objCtrl.GetData(catid, "CATEGORYLANG");
            if (catInfo != null)
            {
                newBaseName = catInfo.GetXmlProperty("genxml/lang/genxml/textbox/txtseoname");
                if (newBaseName == "") newBaseName = catInfo.GetXmlProperty("genxml/lang/genxml/textbox/txtcategoryname");
                if (newBaseName == "") newBaseName = PortalSettings.Current.ActiveTab.TabName;
            }
            return newBaseName;
        }

        #endregion

        #region "Cacheing"

        /// <summary>
        /// Get Module level cache, is same as normal GetCache.  Created to stop confusion.
        /// </summary>
        /// <param name="CacheKey"></param>
        public static object GetModCache(string CacheKey)
        {
            return NBrightCore.common.Utils.GetCache(CacheKey);
        }
        public static object GetCache(string CacheKey)
        {
            return NBrightCore.common.Utils.GetCache(CacheKey);
        }

        /// <summary>
        /// Save into normal cache, but keep a list on the moduleid, so we can remove it at module level
        ///  </summary>
        /// <param name="moduleid">Moduleid use to store in the cache list, (not added to the cachekey)</param>
        /// <param name="CacheKey"></param>
        /// <param name="objObject"></param>
        public static void SetModCache(int moduleid, string CacheKey, object objObject, DateTime AbsoluteExpiration)
        {
            var cList = (List<string>) NBrightCore.common.Utils.GetCache("keylist:" + moduleid.ToString(CultureInfo.InvariantCulture));
            if (cList == null) cList = new List<string>();
            if (!cList.Contains(CacheKey))
            {
                cList.Add(CacheKey);
                NBrightCore.common.Utils.SetCache("keylist:" + moduleid.ToString(CultureInfo.InvariantCulture), cList);
                NBrightCore.common.Utils.SetCache(CacheKey, objObject, AbsoluteExpiration);
            }
        }

        public static void SetModCache(int moduleid, string CacheKey, object objObject)
        {
            SetModCache(moduleid, CacheKey, objObject, DateTime.Now + new TimeSpan(2, 0, 0, 0));
        }

        public static void SetCache(string cacheKey, object objObject)
        {
            NBrightCore.common.Utils.SetCache(cacheKey, objObject, DateTime.Now + new TimeSpan(1, 0, 0, 0));
        }

        public static void RemoveCache(string cacheKey)
        {
            NBrightCore.common.Utils.RemoveCache(cacheKey);
        }

        public static void RemoveModCache(int moduleid)
        {
            var cList = (List<string>) NBrightCore.common.Utils.GetCache("keylist:" + moduleid.ToString(CultureInfo.InvariantCulture));
            if (cList != null)
            {
                foreach (var s in cList)
                {
                    NBrightCore.common.Utils.RemoveCache(s);
                }
            }
            NBrightCore.common.Utils.RemoveCache("keylist:" + moduleid.ToString(CultureInfo.InvariantCulture));
        }

        public static void RemoveModCachePortalWide(int portalid)
        {
            var mCtrl = new NBrightBuyController();
            var l = mCtrl.GetList(portalid, -1, "SETTINGS");
            foreach (var obj in l)
            {
                RemoveModCache(obj.ModuleId);
            }
            RemoveModCache(-1);
        }

        #endregion

        /// <summary>
        /// Include and template data into header, if specified in meta tag token (includeinheader).  
        /// </summary>
        /// <param name="modCtrl"></param>
        /// <param name="moduleId"></param>
        /// <param name="page"></param>
        /// <param name="template"></param>
        /// <param name="settings"></param>
        /// <param name="objInfo"></param>
        /// <param name="debugMode"></param>
        /// <returns></returns>
        public static string IncludePageHeaders(NBrightBuyController modCtrl, int moduleId, Page page, GenXmlTemplate template, Dictionary<string, string> settings, NBrightInfo objInfo = null, bool debugMode = false)
        {
            foreach (var mt in template.MetaTags)
            {
                var id = GenXmlFunctions.GetGenXmlValue(mt, "tag/@id");
                if (id == "includeinheader")
                {
                    var templ = GenXmlFunctions.GetGenXmlValue(mt, "tag/@value");
                    if (templ != "")
                    {

                        var includetext = modCtrl.GetTemplateData(moduleId, templ, Utils.GetCurrentCulture(), settings, debugMode);
                        if (objInfo == null) objInfo = new NBrightInfo(); //create a object so we process the tag values (resourcekey)
                        includetext = GenXmlFunctions.RenderRepeater(objInfo, includetext);
                        if (includetext != "")
                        {
                            if (!page.Items.Contains("nbrightbuyinject")) page.Items.Add("nbrightbuyinject", "");
                            if (templ != "" && !page.Items["nbrightbuyinject"].ToString().Contains(templ + ","))
                            {
                                PageIncludes.IncludeTextInHeader(page, includetext);
                                page.Items["nbrightbuyinject"] = page.Items["nbrightbuyinject"] + templ + ",";
                            }
                        }
                    }
                }
            }
            return "";
        }

        public static string IncludePageHeaderDefault(NBrightBuyController modCtrl, Page page, string templatename, string themeFolder, bool debugMode = false)
        {

            if (!page.Items.Contains("nbrightbuyinject")) page.Items.Add("nbrightbuyinject", "");
            if (templatename != "" && !page.Items["nbrightbuyinject"].ToString().Contains(templatename + ","))
            {
                var includetext = modCtrl.GetTemplate(templatename,Utils.GetCurrentCulture(),themeFolder, debugMode);
                var objInfo = new NBrightInfo(); //create a object so we process the tag values (resourcekey)
                includetext = GenXmlFunctions.RenderRepeater(objInfo, includetext,"","XMLData","",StoreSettings.Current.Settings());
                if (includetext != "")
                {
                    PageIncludes.IncludeTextInHeader(page, includetext);
                    page.Items["nbrightbuyinject"] = page.Items["nbrightbuyinject"] + templatename + ",";
                }
            }
            return "";
        }

        public static GenXmlTemplate GetGenXmlTemplate(string templateData, Dictionary<string, string> settingsDic, string portalHomeDirectory)
        {
            return GetGenXmlTemplate(templateData, settingsDic, portalHomeDirectory,null);
        }

        /// <summary>
        /// Get the GenXmltemplate class and assign required resx files.
        /// </summary>
        /// <param name="templateData"></param>
        /// <param name="settingsDic"></param>
        /// <param name="portalHomeDirectory"></param>
        /// <param name="visibleStatusIn">List of the visible staus used for nested if</param>
        /// <returns></returns>
        public static GenXmlTemplate GetGenXmlTemplate(string templateData, Dictionary<string, string> settingsDic, string portalHomeDirectory, ConcurrentStack<Boolean> visibleStatusIn)
        {
            if (templateData.Trim() != "") templateData = "[<tag type='tokennamespace' value='nbs' />]" + templateData; // add token namespoace for nbs (no need if empty)

            var itemTempl = new GenXmlTemplate(templateData, settingsDic,visibleStatusIn);
            // add default resx folder to template
            itemTempl.AddResxFolder(StoreSettings.NBrightBuyPath() + "/App_LocalResources/");
            if (settingsDic.ContainsKey("themefolder") && settingsDic["themefolder"] != "")
            {
                itemTempl.AddResxFolder(StoreSettings.NBrightBuyPath() + "/Themes/" + settingsDic["themefolder"] + "/resx/");
            }

            return itemTempl;
        }

        /// <summary>
        /// Save temnpoary form data into memory.  This save data we want to repopulate a formwith after pstback into memory, rather than the DB.
        /// </summary>
        /// <param name="moduleId"></param>
        /// <param name="msgcode"></param>
        /// <param name="result"></param>
        public static void SetFormTempData(int moduleId, string xmlData)
        {
            if (xmlData != "")
            {
                var sessionkey = "NBrightBuyForm*" + moduleId.ToString("");
                HttpContext.Current.Session[sessionkey] = xmlData;
            }
        }
        public static string GetFormTempData(int moduleId)
        {
            var xmlData = "";
            var sessionkey = "NBrightBuyForm*" + moduleId.ToString("");
            if (HttpContext.Current.Session[sessionkey] != null) xmlData = (string)HttpContext.Current.Session[sessionkey];
            if (xmlData != "")
            {
                HttpContext.Current.Session.Remove(sessionkey);
                return xmlData;
            }
            return "";
        }

        /// <summary>
        /// Set Notify Message
        /// </summary>
        /// <param name="moduleId">modelid</param>
        /// <param name="msgcode">message code to pick in resx</param>
        /// <param name="result">result code</param>
        /// <param name="resxpath">resx folder path of the message</param>
        public static void SetNotfiyMessage(int moduleId, string msgcode, NotifyCode result, string resxpath = "")
        {
            if (resxpath == "") resxpath = StoreSettings.NBrightBuyPath() + "/App_LocalResources/Notification.ascx.resx";
            if (msgcode != "")
            {
                var sessionkey = "NBrightBuyNotify*" + moduleId.ToString("");
                HttpContext.Current.Session[sessionkey] = msgcode + "_" + result;
                sessionkey = "NBrightBuyNotify*" + moduleId.ToString("") + "*resxpath";
                HttpContext.Current.Session[sessionkey] = resxpath;
            }
        }

        public static string GetNotfiyMessage(int moduleId)
        {
            var msgcode = "";
            var sessionkey = "NBrightBuyNotify*" + moduleId.ToString("");
            if (HttpContext.Current.Session[sessionkey] != null) msgcode = (string) HttpContext.Current.Session[sessionkey];
            if (msgcode != "")
            {
                var sessionkeyresx = "NBrightBuyNotify*" + moduleId.ToString("") + "*resxpath";
                var resxmsgpath = (string)HttpContext.Current.Session[sessionkeyresx];
                HttpContext.Current.Session.Remove(sessionkey);
                HttpContext.Current.Session.Remove(sessionkeyresx);

                var msgtempl = GetResxMessage(msgcode, resxmsgpath);
                return msgtempl;
            }
            return "";
        }

        public static string GetResxMessage(string msgcode = "general_ok", string resxmsgpath = "")
        {
            var resxpath = StoreSettings.NBrightBuyPath() + "/App_LocalResources/Notification.ascx.resx";
            if (resxmsgpath == "") resxmsgpath = resxpath;
            var msg = DnnUtils.GetLocalizedString(msgcode, resxmsgpath, Utils.GetCurrentCulture());
            var level = "ok";
            if (msgcode.EndsWith("_" + NotifyCode.fail.ToString())) level = NotifyCode.fail.ToString();
            if (msgcode.EndsWith("_" + NotifyCode.warning.ToString())) level = NotifyCode.warning.ToString();
            if (msgcode.EndsWith("_" + NotifyCode.error.ToString())) level = NotifyCode.error.ToString();
            var msgtempl = DnnUtils.GetLocalizedString("notifytemplate_" + level, resxpath, Utils.GetCurrentCulture());
            if (msgtempl == null) msgtempl = msg;
            msgtempl = msgtempl.Replace("{message}", msg);
            return msgtempl;
        }

        public static void SendEmailToManager(string templateName, NBrightInfo dataObj, string emailsubjectresxkey = "", string fromEmail = "")
        {
            NBrightBuyUtils.SendEmail(StoreSettings.Current.ManagerEmail, templateName, dataObj, emailsubjectresxkey, fromEmail, StoreSettings.Current.Get("merchantculturecode"));
        }

        public static void SendOrderEmail(string emailtype, int orderId, string emailsubjectresxkey = "", string fromEmail = "", string emailmsg = "")
        {
            SendOrderEmail(emailtype, orderId, emailsubjectresxkey, fromEmail, emailmsg, false);
        }
        public static void SendOrderEmail(string emailtype, int orderId, string emailsubjectresxkey, string fromEmail, string emailmsg, bool onlyUserManagerOnly)
        {
            var ordData = new OrderData(orderId);
            var lang = ordData.ClientLang;
            if (ordData.GetInfo().UserId > 0)
            {
                // this order is linked to a DNN user, so get the order email from the DNN profile (so if it's updated since the order, we pickup the new one)
                var createdportalid = PortalSettings.Current.PortalId; // May be a shared order, but default to this portal.
                if (Utils.IsNumeric(ordData.PurchaseInfo.GetXmlProperty("genxml/createdportalid"))) createdportalid = ordData.PurchaseInfo.GetXmlPropertyInt("genxml/createdportalid");
                var objUser = UserController.GetUserById(createdportalid, ordData.GetInfo().UserId);
                if (objUser != null)
                {
                    if (ordData.EmailAddress != objUser.Email)
                    {
                        ordData.EmailAddress = objUser.Email;
                        ordData.SavePurchaseData();
                    }
                    lang = ordData.ClientLang; // pickup is the client prefered local has changed.
                }
            }
            if (lang == "") lang = Utils.GetCurrentCulture();
            // we're going to send email to all email addreses linked to the order.
            var emailList = ordData.EmailAddress;
            if (!emailList.Contains(ordData.EmailShippingAddress) && Utils.IsEmail(ordData.EmailShippingAddress)) emailList += "," + ordData.EmailShippingAddress;
            if (!emailList.Contains(ordData.EmailBillingAddress) && Utils.IsEmail(ordData.EmailBillingAddress)) emailList += "," + ordData.EmailBillingAddress;

            if (!onlyUserManagerOnly || UserController.Instance.GetCurrentUserInfo().UserID > 0)
            {

                var passSettings = new Dictionary<string, string>();
                passSettings.Add("printtype", emailtype);
                passSettings.Add("emailmessage", emailmsg);
                foreach (var s in StoreSettings.Current.Settings()) // copy store setting, otherwise we get a byRef assignement
                {
                    if (passSettings.ContainsKey(s.Key))
                        passSettings[s.Key] = s.Value;
                    else
                        passSettings.Add(s.Key, s.Value);
                }

                // check for user or manager.
                var sendEmailFlag = true;
                if (onlyUserManagerOnly && UserController.Instance.GetCurrentUserInfo().UserID != ordData.UserId)
                {
                    if (!NBrightBuyUtils.CheckRights())
                    {
                        sendEmailFlag = false;
                    }
                }

                if (sendEmailFlag)
                {
                    var emailBody = "";
                    emailBody = NBrightBuyUtils.RazorTemplRender("OrderHtmlOutput.cshtml", 0, "", ordData, "/DesktopModules/NBright/NBrightBuy", StoreSettings.Current.Get("themefolder"), lang, passSettings);
                    if (StoreSettings.Current.DebugModeFileOut) Utils.SaveFile(PortalSettings.Current.HomeDirectoryMapPath + "\\testemail.html", emailBody);

                    SendEmail(emailBody, emailList, emailtype, ordData.GetInfo(), emailsubjectresxkey, fromEmail, lang);

                    // also send to manager is created. 
                    if (emailtype == "OrderCreatedClient")
                    {
                        emailBody = NBrightBuyUtils.RazorTemplRender("OrderHtmlOutput.cshtml", 0, "", ordData, "/DesktopModules/NBright/NBrightBuy", StoreSettings.Current.Get("themefolder"), StoreSettings.Current.Get("merchantculturecode"), passSettings);
                        SendEmail(emailBody, StoreSettings.Current.ManagerEmail, "OrderCreatedManager", ordData.GetInfo(), emailsubjectresxkey, fromEmail, StoreSettings.Current.Get("merchantculturecode"));
                    }


                    ordData.AddAuditMessage(emailmsg, "email", UserController.Instance.GetCurrentUserInfo().Username, "False", NBrightBuyUtils.ResourceKey("Notification." + emailsubjectresxkey, StoreSettings.Current.EditLanguage));
                    ordData.SavePurchaseData();
                }
            }

        }

        /// <summary>
        /// legacy function for sending email using tagtoken system
        /// </summary>
        /// <param name="toEmail"></param>
        /// <param name="templateName"></param>
        /// <param name="dataObj"></param>
        /// <param name="emailsubjectresxkey"></param>
        /// <param name="fromEmail"></param>
        /// <param name="lang"></param>
        public static void SendEmail(string toEmail, string templateName, NBrightInfo dataObj, string emailsubjectresxkey, string fromEmail, string lang)
        {
            dataObj = ProcessEventProvider(EventActions.BeforeSendEmail, dataObj, templateName);

            if (!dataObj.GetXmlPropertyBool("genxml/stopprocess"))
            {

                if (lang == "") lang = Utils.GetCurrentCulture();
                var emaillist = toEmail;
                if (emaillist != "")
                {
                    var emailsubject = "";
                    if (emailsubjectresxkey != "")
                    {
                        var resxpath = StoreSettings.NBrightBuyPath() + "/App_LocalResources/Notification.ascx.resx";
                        emailsubject = DnnUtils.GetLocalizedString(emailsubjectresxkey, resxpath, lang);
                        if (emailsubject == null) emailsubject = emailsubjectresxkey;
                    }

                    // we can't use StoreSettings.Current.Settings(), becuase of external calls from providers to this function, so load in the settings directly  
                    var modCtrl = new NBrightBuyController();
                    var storeSettings = modCtrl.GetStoreSettings(dataObj.PortalId);

                    var strTempl = modCtrl.GetTemplateData(-1, templateName, lang, storeSettings.Settings(), storeSettings.DebugMode);

                    var emailbody = GenXmlFunctions.RenderRepeater(dataObj, strTempl, "", "XMLData", lang, storeSettings.Settings());
                    if (templateName.EndsWith(".xsl")) emailbody = XslUtils.XslTransInMemory(dataObj.XMLData, emailbody);
                    if (fromEmail == "") fromEmail = storeSettings.AdminEmail;
                    var emailarray = emaillist.Split(',');
                    emailsubject = storeSettings.Get("storename") + " : " + emailsubject;
                    foreach (var email in emailarray)
                    {
                        if (!string.IsNullOrEmpty(email.Trim()) && Utils.IsEmail(fromEmail.Trim()) && Utils.IsEmail(email.Trim()))
                        {
                            // multiple attachments as csv with "|" seperator
                            DotNetNuke.Services.Mail.Mail.SendMail(fromEmail.Trim(), email.Trim(), "", emailsubject, emailbody, dataObj.GetXmlProperty("genxml/emailattachment"), "HTML", "", "", "", "");
                        }
                    }
                }
            }

            ProcessEventProvider(EventActions.AfterSendEmail, dataObj, templateName);


        }

        /// <summary>
        /// Send email as a body of text. (Used for new Order razor API call to printview)
        /// </summary>
        /// <param name="emailbody"></param>
        /// <param name="toEmail"></param>
        /// <param name="emailtype"></param>
        /// <param name="dataObj"></param>
        /// <param name="emailsubjectresxkey"></param>
        /// <param name="fromEmail"></param>
        /// <param name="lang"></param>
        public static void SendEmail(string emailbody, string toEmail, string emailtype, NBrightInfo dataObj, string emailsubjectresxkey, string fromEmail, string lang)
        {
            dataObj = ProcessEventProvider(EventActions.BeforeSendEmail, dataObj, emailtype);

            if (!dataObj.GetXmlPropertyBool("genxml/stopprocess"))
            {

                if (lang == "") lang = Utils.GetCurrentCulture();
                var emaillist = toEmail;
                if (emaillist != "")
                {
                    var emailsubject = "";
                    if (emailsubjectresxkey != "")
                    {
                        var resxpath = StoreSettings.NBrightBuyPath() + "/App_LocalResources/Notification.ascx.resx";
                        emailsubject = DnnUtils.GetLocalizedString(emailsubjectresxkey, resxpath, lang);
                        if (string.IsNullOrWhiteSpace(emailsubject)) emailsubject = emailsubjectresxkey;
                    }

                    // we can't use StoreSettings.Current.Settings(), becuase of external calls from providers to this function, so load in the settings directly  
                    var modCtrl = new NBrightBuyController();
                    var storeSettings = modCtrl.GetStoreSettings(dataObj.PortalId);

                    if (fromEmail == "") fromEmail = storeSettings.AdminEmail;
                    var emailarray = emaillist.Split(',');
                    emailsubject = storeSettings.Get("storename") + " : " + emailsubject;
                    foreach (var email in emailarray)
                    {
                        if (!string.IsNullOrEmpty(email.Trim()) && Utils.IsEmail(fromEmail.Trim()) && Utils.IsEmail(email.Trim()))
                        {
                            // multiple attachments as csv with "|" seperator
                            DotNetNuke.Services.Mail.Mail.SendMail(fromEmail.Trim(), email.Trim(), "", emailsubject, emailbody, dataObj.GetXmlProperty("genxml/emailattachment"), "HTML", "", "", "", "");
                        }
                    }
                }
            }

            ProcessEventProvider(EventActions.AfterSendEmail, dataObj, emailtype);


        }


        public static List<NBrightInfo> GetCategoryGroups(string lang, Boolean debugMode = false, string groupType = "")
        {
            return GetCategoryGroups(PortalSettings.Current.PortalId,lang,debugMode,groupType);
        }

        public static List<NBrightInfo> GetCategoryGroups(int portalId, string lang, Boolean debugMode = false, string groupType = "")
        {
            var filter = "";
            if (groupType != "") filter = " and [XMLData].value('(genxml/dropdownlist/grouptype)[1]','nvarchar(max)') = '" + groupType + "' ";
            var objCtrl = new NBrightBuyController();
            var levelList = objCtrl.GetDataList(portalId, -1, "GROUP", "GROUPLANG", lang, filter, " order by [XMLData].value('(genxml/hidden/recordsortorder)[1]','decimal(10,2)') ", debugMode);
            return levelList;
        }

        public static List<NBrightInfo> GetGenXmlListByAjax(string xmlAjaxData, string originalXml, string lang = "en-US", string xmlRootName = "genxml")
        {
            var rtnList = new List<NBrightInfo>();
            if (!string.IsNullOrEmpty(xmlAjaxData))
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlAjaxData);
                var nodList = xmlDoc.SelectNodes("root/root");
                if (nodList != null)
                    foreach (XmlNode nod in nodList)
                    {
                        var xmlData = GenXmlFunctions.GetGenXmlByAjaxLang(nod.OuterXml, "","genxml",false,false,lang);
                        var objInfo = new NBrightInfo();
                        objInfo.ItemID = -1;
                        objInfo.TypeCode = "AJAXDATA";
                        objInfo.XMLData = xmlData;
                        rtnList.Add(objInfo);
                    }
            }
            return rtnList;
        }




        public static string FormatToStoreCurrency(double value)
        {


            var currencycode = StoreSettings.Current.Get("currencyculturecode");
            if (currencycode.StartsWith("\"")) return value.ToString(currencycode.Replace("\"", ""));
            if (currencycode == "") currencycode = StoreSettings.Current.Get("merchantculturecode");
            if (currencycode == "") currencycode = PortalSettings.Current.CultureCode;
            try
            {
                var currFormat = value.ToString("c", new CultureInfo(currencycode, false));
                if (currFormat.Length > 0 && Utils.IsNumeric(currFormat.Substring(0, 1))) return value.ToString("0.00", new CultureInfo(Utils.GetCurrentCulture(), false)) + StoreSettings.Current.Get("currencysymbol");
                return StoreSettings.Current.Get("currencysymbol") + value.ToString("0.00", new CultureInfo(Utils.GetCurrentCulture(), false));
                // NOTE: ATM we only have 1 currency.
            }
            catch (Exception)
            {
                return value.ToString("0.00", new CultureInfo(PortalSettings.Current.CultureCode, false));
            }
        }

        public static string GetCurrencyIsoCode()
        {
            var currencycode = StoreSettings.Current.Get("currencyculturecode");
            if (currencycode == "") currencycode = StoreSettings.Current.Get("merchantculturecode");
            if (currencycode == "") currencycode = PortalSettings.Current.CultureCode;
            try
            {
                return new RegionInfo(currencycode).ISOCurrencySymbol;
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static Dictionary<string, string> GetCountryList(string dnnlistname = "Country")
        {
            var resxpath = StoreSettings.NBrightBuyPath() + "/App_LocalResources/CountryNames.ascx.resx";

            var objCtrl = new DotNetNuke.Common.Lists.ListController();
            var tList = objCtrl.GetListEntryInfoDictionary(dnnlistname);
            var rtnDic = new Dictionary<string, string>();

            var xmlNodeList = StoreSettings.Current.SettingsInfo.XMLDoc.SelectNodes("genxml/checkboxlist/countrycodelist/chk[@value='True']");
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
                            if (tList.ContainsKey(datavalue))
                            {
                                var countryname = DnnUtils.GetLocalizedString(datavalue, resxpath, Utils.GetCurrentCulture());
                                if (string.IsNullOrEmpty(countryname)) countryname = tList[datavalue].Text;
                                rtnDic.Add(datavalue.Replace(dnnlistname + ":", ""),countryname);
                            }
                        }
                    }
                }
            }
            var sortlist = StoreSettings.Current.Get("countrysortorder");
            if (sortlist == "") return rtnDic;
            
            var rtnSort = new Dictionary<string, string>();
            var s = sortlist.Split(';');
            foreach (var c in s)
            {
                if (c != "")
                {
                    if (rtnDic.ContainsKey(c))
                    {
                        var d = rtnDic[c];
                        rtnSort.Add(c,d);
                        rtnDic.Remove(c);
                    }
                }
            }
            foreach (var c in rtnDic)
            {
                rtnSort.Add(c.Key,c.Value);    
            }

            return rtnSort;
        }

        public static Dictionary<string, string> GetRegionList(string countrycode, string dnnlistname = "Region")
        {
            var resxpath = StoreSettings.NBrightBuyPath() + "/App_LocalResources/RegionNames.ascx.resx";
            var parentkey = "Country." + countrycode;
            var objCtrl = new DotNetNuke.Common.Lists.ListController();
            var tList = objCtrl.GetListEntryInfoDictionary(dnnlistname, parentkey);
            var rtnDic = new Dictionary<string, string>();

            foreach (var r in tList)
            {
                var datavalue = r.Value;
                var regionname = DnnUtils.GetLocalizedString(datavalue.Text, resxpath, Utils.GetCurrentCulture());
                if (string.IsNullOrEmpty(regionname)) regionname = datavalue.Text;
                rtnDic.Add(datavalue.Key, regionname);                
            }

            return rtnDic;
        }

        public static Boolean UpdateResxFields(Dictionary<string, string> resxFields, List<string> resxFolders, string lang, Boolean createFile)
        {
            var fileList = new Dictionary<string, string>(); // use NBrightInfo to edit res file to make it easier.

            if (lang != "") lang = "." + lang;

            foreach (var d in resxFields)
            {
                var resxKeyArray = d.Key.Split('.');
                if (resxKeyArray.Length == 2)
                {
                    var fileName = resxKeyArray[0];
                    var resxKey = resxKeyArray[1];
                    if (!fileList.ContainsKey(fileName)) fileList.Add(fileName, "");
                }
            }

            var updDone = false;
            foreach (var r in resxFolders)
            {
                foreach (var f in fileList)
                {
                    var fpath = HttpContext.Current.Server.MapPath(r + f.Key + ".ascx.resx");
                    //var fpathlang = HttpContext.Current.Server.MapPath(r + f.Key + ".ascx" + lang + ".resx");
                    var fpathlangportal = "";
                    if (lang.EndsWith(Localization.SystemLocale))
                        fpathlangportal = HttpContext.Current.Server.MapPath(r + f.Key + ".ascx.Portal-" + PortalSettings.Current.PortalId.ToString("") + ".resx");
                    else
                        fpathlangportal = HttpContext.Current.Server.MapPath(r + f.Key + ".ascx" + lang + ".Portal-" + PortalSettings.Current.PortalId.ToString("") + ".resx");
                    if (File.Exists(fpath))
                    {
                        if (!File.Exists(fpathlangportal)) CreateResourceFile(fpathlangportal);
                        foreach (var d in resxFields)
                        {
                            var resxKeyArray = d.Key.Split('.');
                            if (resxKeyArray.Length == 2)
                            {
                                var fileName = resxKeyArray[0];
                                var resxKey = resxKeyArray[1];
                                if (fileName == f.Key)
                                {
                                    if (UpdateResourceFile(resxKey + ".Text", d.Value, fpathlangportal)) updDone = true;
                                }
                            }
                        }
                    }
                }
            }

            return updDone;
        }

        public static void CreateResourceFile(string path)
        {
            var pathString = Path.GetDirectoryName(path);
            if (!Directory.Exists(pathString)) System.IO.Directory.CreateDirectory(pathString);

            ResXResourceWriter resourceWriter = new ResXResourceWriter(path);
            resourceWriter.Generate();
            resourceWriter.Close();
        }

        public static Boolean UpdateResourceFile(string key, string value, string path)
        {
            Hashtable resourceEntries = new Hashtable();

            //Get existing resources
            if (File.Exists(path))
            {
                ResXResourceReader reader = new ResXResourceReader(path);
                if (reader != null)
                {
                    IDictionaryEnumerator id = reader.GetEnumerator();
                    foreach (DictionaryEntry d in reader)
                    {
                        if (d.Value == null)
                            resourceEntries.Add(d.Key.ToString(), "");
                        else
                            resourceEntries.Add(d.Key.ToString(), d.Value.ToString());
                    }
                    reader.Close();
                }
            }
            //Modify resources here...
            var updDone = false;
                if (resourceEntries.ContainsKey(key))
                {
                    if ((string) resourceEntries[key] != value)
                    {
                        resourceEntries[key] = value;
                        updDone = true;
                    }
                }
                else
                {
                    resourceEntries.Add(key,value);
                    updDone = true;
                }

            //Write the combined resource file
            if (updDone)
            {
                ResXResourceWriter resourceWriter = new ResXResourceWriter(path);

                foreach (string key2 in resourceEntries.Keys)
                {
                    resourceWriter.AddResource(key2, resourceEntries[key2]);
                }
                resourceWriter.Generate();
                resourceWriter.Close();                
            }
            return updDone;
        }

        public static string AdminUrl(int tabId, string[] param )
        {
            var newparam = new string[(param.Length + 1)];
            var foundCtrl = false;
            for (int i = 0; i < param.Length; i++)
            {
                if (param[i] != null && param[i].StartsWith("ctrl=")) foundCtrl = true;
                newparam[i] = param[i];
            }
            if (!foundCtrl) newparam[param.Length] = "ctrl=" + HttpContext.Current.Session["nbrightbackofficectrl"];
            return Globals.NavigateURL(tabId, "", newparam);
        }

        public static List<string> GetAllFieldxPaths(NBrightInfo nbi, Boolean ignoreHiddenFields = false)
        {
            var ignorefields = nbi.GetXmlProperty("genxml/hidden/ignorefields");
            ignorefields = ignorefields + ",ignorefields";
            var ignoreAry = ignorefields.Split(',');
            
            var rtnList = new List<string>();
            var nods = nbi.XMLDoc.SelectNodes("genxml/hidden/*");
            if (!ignoreHiddenFields)
            {
                if (nods != null)
                {
                    foreach (XmlNode xmlNod in nods)
                    {
                        if (!ignoreAry.Contains(xmlNod.Name)) rtnList.Add("genxml/hidden/" + xmlNod.Name);
                    }
                }                
            }
            nods = nbi.XMLDoc.SelectNodes("genxml/textbox/*");
            if (nods != null)
            {
                foreach (XmlNode xmlNod in nods)
                {
                    if (!ignoreAry.Contains(xmlNod.Name)) rtnList.Add("genxml/textbox/" + xmlNod.Name);
                }
            }
            nods = nbi.XMLDoc.SelectNodes("genxml/checkbox/*");
            if (nods != null)
            {
                foreach (XmlNode xmlNod in nods)
                {
                    if (!ignoreAry.Contains(xmlNod.Name)) rtnList.Add("genxml/checkbox/" + xmlNod.Name);
                }
            }
            nods = nbi.XMLDoc.SelectNodes("genxml/checkboxlist/*");
            if (nods != null)
            {
                foreach (XmlNode xmlNod in nods)
                {
                    if (!ignoreAry.Contains(xmlNod.Name)) rtnList.Add("genxml/checkboxlist/" + xmlNod.Name);
                }
            }
            nods = nbi.XMLDoc.SelectNodes("genxml/dropdownlist/*");
            if (nods != null)
            {
                foreach (XmlNode xmlNod in nods)
                {
                    if (!ignoreAry.Contains(xmlNod.Name)) rtnList.Add("genxml/dropdownlist/" + xmlNod.Name);
                }
            }
            nods = nbi.XMLDoc.SelectNodes("genxml/radiobuttonlist/*");
            if (nods != null)
            {
                foreach (XmlNode xmlNod in nods)
                {
                    if (!ignoreAry.Contains(xmlNod.Name)) rtnList.Add("genxml/radiobuttonlist/" + xmlNod.Name);
                }
            }
            nods = nbi.XMLDoc.SelectNodes("genxml/edt/*");
            if (nods != null)
            {
                foreach (XmlNode xmlNod in nods)
                {
                    if (!ignoreAry.Contains(xmlNod.Name)) rtnList.Add("genxml/edt/" + xmlNod.Name);
                }
            }

            return rtnList;
        }

        public static int ValidateStore()
        {
            // clear all cache to start.
            DataCache.ClearCache();

            var objCtrl = new NBrightBuyController();
            var errcount = 0;

            // Validate Products
            var prodList = objCtrl.GetList(DotNetNuke.Entities.Portals.PortalSettings.Current.PortalId, -1, "PRD");
            foreach (var p in prodList)
            {
                var enabledlanguages = LocaleController.Instance.GetLocales(PortalSettings.Current.PortalId);
                foreach (var lang in enabledlanguages)
                {
                    var prodData = ProductUtils.GetProductData(p.ItemID, lang.Key, true, "PRD");
                    errcount += prodData.Validate();
                }
                foreach (var lang in enabledlanguages)
                {
                    // do 2 loops, so we dont overwrite data.
                    ProductUtils.CreateFriendlyImages(p.ItemID, lang.Key);
                }
            }

            // Validate Categories
            var catList = objCtrl.GetList(DotNetNuke.Entities.Portals.PortalSettings.Current.PortalId, -1, "CATEGORY");
            foreach (var c in catList)
            {
                var catData = CategoryUtils.GetCategoryData(c.ItemID, StoreSettings.Current.EditLanguage);
                errcount += catData.Validate();
            }

            // Validate Groups
            var grpList = objCtrl.GetList(DotNetNuke.Entities.Portals.PortalSettings.Current.PortalId, -1, "GROUP");
            foreach (var c in grpList)
            {
                var grpData = new GroupData(c.ItemID, StoreSettings.Current.EditLanguage);
                errcount += grpData.Validate();
            }

            // reset catid from catref
            var l = objCtrl.GetDataList(PortalSettings.Current.PortalId, -1, "SETTINGS", "", Utils.GetCurrentCulture(), " and [XMLdata].value('(genxml/catref)[1]','nvarchar(max)') != ''","");
            foreach (var s in l)
            {
                var info = ModuleSettingsResetCatIdFromRef(s);
                objCtrl.Update(info);
            }

            // validate plugin data
            var pluginData = new PluginData(PortalSettings.Current.PortalId);
            var provList = pluginData.GetAjaxProviders();
            foreach (var d in provList)
            {
                    var ajaxprov = AjaxInterface.Instance(d.Key);
                    if (ajaxprov != null)
                    {
                        ajaxprov.Validate();
                    }
            }

            return errcount;
        }

        public static NBrightInfo ProcessEventProvider(EventActions eventaction, NBrightInfo nbrightInfo)
        {
            return ProcessEventProvider(eventaction, nbrightInfo, "");
        }

        public static NBrightInfo ProcessEventProvider(EventActions eventaction, NBrightInfo nbrightInfo, string eventinfo)
        {
            var rtnInfo = nbrightInfo;
            var portalId = nbrightInfo.PortalId; // can be -1 for categories and products (Shared on all portals)
            if (portalId < 0 && PortalSettings.Current != null) portalId = PortalSettings.Current.PortalId; 
            if (portalId < 0) return rtnInfo;

            var pluginData = new PluginData(portalId);
            var provList = pluginData.GetEventsProviders();

            foreach (var d in provList)
            {
                try
                {

                var prov = d.Value;
                ObjectHandle handle = null;
                var cachekey = prov.PortalId + "*" + prov.GetXmlProperty("genxml/textbox/assembly") + "*" + prov.GetXmlProperty("genxml/textbox/namespaceclass");
                handle = (ObjectHandle)Utils.GetCache(cachekey);
                if (handle == null) handle = Activator.CreateInstance(prov.GetXmlProperty("genxml/textbox/assembly"), prov.GetXmlProperty("genxml/textbox/namespaceclass"));
                    if (handle != null)
                    {
                        var eventprov = (EventInterface)handle.Unwrap();
                        if (eventprov != null)
                        {
                            if (eventaction == EventActions.ValidateCartBefore)
                            {
                                rtnInfo = eventprov.ValidateCartBefore(nbrightInfo);
                            }
                            else if (eventaction == EventActions.ValidateCartAfter)
                            {
                                rtnInfo = eventprov.ValidateCartAfter(nbrightInfo);
                            }
                            else if (eventaction == EventActions.ValidateCartItemBefore)
                            {
                                rtnInfo = eventprov.ValidateCartItemBefore(nbrightInfo);
                            }
                            else if (eventaction == EventActions.ValidateCartItemAfter)
                            {
                                rtnInfo = eventprov.ValidateCartItemAfter(nbrightInfo);
                            }
                            else if (eventaction == EventActions.AfterCartSave)
                            {
                                rtnInfo = eventprov.AfterCartSave(nbrightInfo);
                            }
                            else if (eventaction == EventActions.AfterCategorySave)
                            {
                                rtnInfo = eventprov.AfterCategorySave(nbrightInfo);
                            }
                            else if (eventaction == EventActions.AfterProductSave)
                            {
                                rtnInfo = eventprov.AfterProductSave(nbrightInfo);
                            }
                            else if (eventaction == EventActions.AfterSavePurchaseData)
                            {
                                rtnInfo = eventprov.AfterSavePurchaseData(nbrightInfo);
                            }
                            else if (eventaction == EventActions.BeforeOrderStatusChange)
                            {
                                rtnInfo = eventprov.BeforeOrderStatusChange(nbrightInfo);
                            }
                            else if (eventaction == EventActions.AfterOrderStatusChange)
                            {
                                rtnInfo = eventprov.AfterOrderStatusChange(nbrightInfo);
                            }
                            else if (eventaction == EventActions.BeforePaymentOK)
                            {
                                rtnInfo = eventprov.BeforePaymentOK(nbrightInfo);
                            }
                            else if (eventaction == EventActions.AfterPaymentOK)
                            {
                                rtnInfo = eventprov.AfterPaymentOK(nbrightInfo);
                            }
                            else if (eventaction == EventActions.BeforePaymentFail)
                            {
                                rtnInfo = eventprov.BeforePaymentFail(nbrightInfo);
                            }
                            else if (eventaction == EventActions.AfterPaymentFail)
                            {
                                rtnInfo = eventprov.AfterPaymentFail(nbrightInfo);
                            }
                            else if (eventaction == EventActions.BeforeSendEmail)
                            {
                                rtnInfo = eventprov.BeforeSendEmail(nbrightInfo, eventinfo);
                            }
                            else if (eventaction == EventActions.AfterSendEmail)
                            {
                                rtnInfo = eventprov.AfterSendEmail(nbrightInfo, eventinfo);
                            }

                        }
                    }

                }
                catch (Exception ex)
                {
                    // ignore and log provider erros
                    Logging.Debug(ex.Message);
                }
            }
            return rtnInfo;
        }

        /// <summary>
        /// Helper function to help plugins get a theme template from their local theme folder.
        /// This function will get the template, replace settings tokens, replace url tokens
        /// </summary>
        /// <param name="templatename">name of template</param>
        /// <param name="templateControlPath">Relative Control path of plugin e.g."/DesktopModules/NBright/NBrightBuyPluginTempl"</param>
        /// <param name="themeFolder">Theme folder to use e.g. "config"</param>
        /// <param name="settings">Settings to use, default to storesettings</param>
        /// <param name="lang">culture code</param>
        /// <returns></returns>
        public static string GetTemplateData(string templatename, string templateControlPath, string themeFolder = "config", Dictionary<string, string> settings = null, string lang = "")
        {
            themeFolder = "Themes\\" + themeFolder;
            if (settings == null) settings = StoreSettings.Current.Settings();
            var controlMapPath = HttpContext.Current.Server.MapPath(templateControlPath);
            var templCtrl = new TemplateGetter(PortalSettings.Current.HomeDirectoryMapPath, controlMapPath, themeFolder, StoreSettings.Current.ThemeFolder);
            if (lang == "") lang = Utils.GetCurrentCulture();
            var templ = templCtrl.GetTemplateData(templatename, lang);
            templ = Utils.ReplaceSettingTokens(templ, settings);
            templ = Utils.ReplaceUrlTokens(templ);
            return templ;
        }

        public static List<NBrightInfo> GetCatList(int parentid = 0, string groupref = "", string lang = "")
        {
            if (lang == "") lang = Utils.GetCurrentCulture();

            var strFilter = "";
            if (groupref == "" || groupref == "0") // Because we've introduced Properties (for non-category groups) we will only display these if cat is not selected.
                strFilter += " and [XMLData].value('(genxml/dropdownlist/ddlgrouptype)[1]','nvarchar(max)') != 'cat' ";
            else
            {
                if (groupref == "cat") strFilter = " and NB1.ParentItemId = " + parentid + " "; // only category have multipel levels.
                strFilter += " and [XMLData].value('(genxml/dropdownlist/ddlgrouptype)[1]','nvarchar(max)') = '" + groupref + "' ";
            }

            var objCtrl = new NBrightBuyController();
            var levelList = objCtrl.GetDataList(PortalSettings.Current.PortalId, -1, "CATEGORY", "CATEGORYLANG", lang, strFilter, " order by [XMLData].value('(genxml/hidden/recordsortorder)[1]','decimal(10,2)') ", true);

            var grpCtrl = new GrpCatController(lang);

            foreach (var c in levelList)
            {
                var g = grpCtrl.GetCategory(c.ItemID);
                if (g != null) c.SetXmlProperty("genxml/entrycount", g.entrycount.ToString(""));
            }

            return levelList;
        }

        public static NBrightInfo ModuleSettingsResetCatIdFromRef(NBrightInfo objInfo)
        {
            var ModCtrl = new NBrightBuyController();
            var catid = objInfo.GetXmlPropertyInt("genxml/dropdownlist/defaultcatid");
            var nbi = ModCtrl.Get(catid);
            if (nbi == null)
            {
                // categoryid no longer exists, see if we can get it back with the catref (might be lost due to cleardown and import)
                var catref = objInfo.GetXmlProperty("genxml/catref");
                nbi = ModCtrl.GetByGuidKey(objInfo.PortalId, -1, "CATEGORY", catref);
                if (nbi != null)
                {
                    objInfo.SetXmlProperty("genxml/dropdownlist/defaultcatid", nbi.ItemID.ToString(""));
                }
            }
            return objInfo;
        }

        public static NBrightInfo ModuleSettingsSaveCatRefFromId(NBrightInfo objInfo)
        {
            var catid = objInfo.GetXmlPropertyInt("genxml/dropdownlist/defaultcatid");
            var catData = CategoryUtils.GetCategoryData(catid, Utils.GetCurrentCulture());
            if (catData.Exists) objInfo.SetXmlProperty("genxml/catref", catData.CategoryRef);
            return objInfo;
        }

        public static int ModuleSettingsGetCatIdFromRef(NBrightInfo settingsInfo)
        {
            var ModCtrl = new NBrightBuyController();
            // categoryid no longer exists, see if we can get it back with the catref (might be lost due to cleardown and import)
            var catref = settingsInfo.GetXmlProperty("genxml/catref");
            var nbi = ModCtrl.GetByGuidKey(settingsInfo.PortalId, -1, "CATEGORY", catref);
            if (nbi != null) return nbi.ItemID;
            return -1;
        }


        public static int GetEntryIdFromUrl(int portalId, System.Web.HttpRequest request)
        {
            var entryId = 0;
            var qryitemid = Utils.RequestQueryStringParam(request, "eid");
            if (Utils.IsNumeric(qryitemid))
            {
                entryId = Convert.ToInt32(qryitemid);
            }
            else
            {
                var qryguidkey = Utils.RequestQueryStringParam(request, "guidkey");
                if (qryguidkey == "") qryguidkey = Utils.RequestQueryStringParam(request, "ref");
                if (qryguidkey != "")
                {
                    var objCtrl = new NBrightBuyController();
                    var guidData = objCtrl.GetByGuidKey(portalId, -1, "PRD", qryguidkey);
                    if (guidData != null) entryId = guidData.ItemID;
                }
            }
            return entryId;
        }

        public static int GetCategoryIdFromUrl(int portalId, System.Web.HttpRequest request)
        {
            var categoryid = -1;
            var grpCatCtrl = new GrpCatController(Utils.GetCurrentCulture());

            var qrycatid = Utils.RequestQueryStringParam(request, "catid");
            if (Utils.IsNumeric(qrycatid)) categoryid = Convert.ToInt32(qrycatid);
            if (categoryid == -1 )
            {
                var qrycatref = Utils.RequestQueryStringParam(request, "catref");
                if (qrycatref != "")
                {
                    var catrefData = grpCatCtrl.GetCategoryByRef(portalId, qrycatref);
                    if (catrefData != null) categoryid = catrefData.categoryid;
                }
            }
            return categoryid;
        }

        public static string ResourceKey(String resourceFileKey, String lang = "", String resourceExtension = "Text")
        {
            if (lang == "") lang = Utils.GetCurrentCulture();
            var strOut = "";
            strOut = DnnUtils.GetResourceString("/DesktopModules/NBright/NBrightBuy/App_LocalResources/", resourceFileKey, resourceExtension, lang);
            return strOut;
        }

        #region "Razor"

        public static string GetRazorTemplateData(string templatename, string templateControlPath, string themeFolder = "config", string lang = "")
        {
            var controlMapPath = HttpContext.Current.Server.MapPath(templateControlPath);
            var templCtrl = new TemplateGetter(PortalSettings.Current.HomeDirectoryMapPath, controlMapPath, "Themes\\" + StoreSettings.Current.ThemeFolder, "Themes\\" + themeFolder );
            if (lang == "") lang = Utils.GetCurrentCulture();
            var templ = templCtrl.GetTemplateData(templatename, lang);
            return templ;
        }

        public static bool FileExists(string relfilepathname)
        {
            try
            {
                var filepathname = HttpContext.Current.Server.MapPath(relfilepathname);
                return System.IO.File.Exists(filepathname);
            }
            catch (Exception e)
            {
                return false;
            }
        }


        public static string RazorTemplRenderList(string razorTemplName, int moduleid, string cacheKey, List<NBrightInfo> objList, string templateControlPath, string theme, string lang, Dictionary<string, string> settings)
        {
            // do razor template
            if (lang == "")
            {
                lang = Utils.GetCurrentCulture();
            }
            var ckey = "NBrightBuyRazorOutput" + theme + razorTemplName + "*" + cacheKey + PortalSettings.Current.PortalId.ToString() + lang;
            var razorTempl = (string)GetModCache(ckey);
            if (razorTempl == null || StoreSettings.Current.DebugMode)
            {
                razorTempl = GetRazorTemplateData(razorTemplName, templateControlPath, theme, lang);
                if (razorTempl != "")
                {
                    var nbRazor = new NBrightRazor(objList.Cast<object>().ToList(), settings, HttpContext.Current.Request.QueryString);
                    nbRazor.ModuleId = moduleid;
                    nbRazor.FullTemplateName = theme + "." + razorTemplName;
                    nbRazor.TemplateName = razorTemplName;
                    nbRazor.ThemeFolder = theme;
                    nbRazor.Lang = lang;

                    var razorTemplateKey = "NBrightBuyRazorKey" + theme + razorTemplName + PortalSettings.Current.PortalId.ToString();
                    razorTempl = RazorRender(nbRazor, razorTempl, razorTemplateKey, StoreSettings.Current.DebugMode);
                    if (cacheKey != "") SetModCache(moduleid, ckey, razorTempl); // only save to cache if we pass in a cache key.
                }
            }
            return razorTempl;
        }

        public static string RazorTemplRenderGroupCategoryList(string razorTemplName, int moduleid, string cacheKey, List<GroupCategoryData> objList, string templateControlPath, string theme, string lang, Dictionary<string, string> settings)
        {
            // do razor template
            var cachekey = "NBrightBuyRazorOutputGrp" + theme + razorTemplName + "*" + cacheKey + PortalSettings.Current.PortalId.ToString();
            var razorTempl = (string)GetModCache(cachekey);
            if (razorTempl == null || StoreSettings.Current.DebugMode)
            {
                razorTempl = GetRazorTemplateData(razorTemplName, templateControlPath, theme, lang);
                if (razorTempl != "")
                {
                    var nbRazor = new NBrightRazor(objList.Cast<object>().ToList(), settings, HttpContext.Current.Request.QueryString);
                    nbRazor.ModuleId = moduleid;
                    nbRazor.FullTemplateName = theme + "." + razorTemplName;
                    nbRazor.TemplateName = razorTemplName;
                    nbRazor.ThemeFolder = theme;
                    nbRazor.Lang = lang;

                    var razorTemplateKey = "NBrightBuyRazorKey" + theme + razorTemplName + PortalSettings.Current.PortalId.ToString();
                    razorTempl = RazorRender(nbRazor, razorTempl, razorTemplateKey, StoreSettings.Current.DebugMode);
                    if (cacheKey != "") SetModCache(moduleid, cachekey, razorTempl); // only save to cache if we pass in a cache key.
                }
            }
            return razorTempl;
        }

        public static Dictionary<string, string> RazorPreProcessTempl(string razorTemplName, string templateControlPath, string theme, string lang, Dictionary<string, string> settings, string moduleid = "")
        {
            var cachekey = "preprocessmetadata" + theme + "." + razorTemplName + moduleid;

            // get cached data if there
            var cachedlist = (Dictionary<string, string>)Utils.GetCache(cachekey);
            if (cachedlist != null) return cachedlist;
            
            // build cache data from template.
            cachedlist = new Dictionary<string, string>();
            var razorTemplate = GetRazorTemplateData(razorTemplName, templateControlPath, theme, lang);
            if (razorTemplate != "" && razorTemplate.Contains("AddPreProcessMetaData"))
            {
                var obj = new NBrightInfo(true);
                obj.Lang = lang;
                obj.ModuleId = -1;
                var l = new List<object>();
                l.Add(obj);
                var modRazor = new NBrightRazor(l, settings, HttpContext.Current.Request.QueryString);
                modRazor.FullTemplateName = theme + "." + razorTemplName;
                modRazor.TemplateName = razorTemplName;
                modRazor.ThemeFolder = theme;
                modRazor.Lang = lang;
                try
                {
                    // do razor and cache preprocessmetadata
                    razorTemplate = RazorRender(modRazor, razorTemplate, cachekey, false);
                }
                catch (Exception ex)
                {
                    // Only log exception, could be a error because of missing data.  The preprocessing doesn't care.
                }
                cachedlist = (Dictionary<string, string>) Utils.GetCache(cachekey);
                if (cachedlist == null) cachedlist = new Dictionary<string, string>();
                Utils.SetCache(cachekey, cachedlist);
            }
            else
            {
                cachedlist = new Dictionary<string, string>();
                Utils.SetCache(cachekey, cachedlist);
            }
            return cachedlist;
        }


        /// <summary>
        /// Render a razor template with an object, this method will include the object in the List of the NBrightRazor class
        /// </summary>
        /// <param name="razorTemplName"></param>
        /// <param name="moduleid"></param>
        /// <param name="cacheKey">If empty no cache done</param>
        /// <param name="obj"></param>
        /// <param name="templateControlPath"></param>
        /// <param name="theme"></param>
        /// <param name="lang"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static string RazorTemplRender(string razorTemplName, int moduleid, string cacheKey, object obj, string templateControlPath, string theme, string lang, Dictionary<string, string> settings)
        {
            // do razor template
            var ckey = "NBrightBuyRazorOutput" + razorTemplName + "*" + cacheKey + PortalSettings.Current.PortalId.ToString() + "*" + lang;
            var razorTempl = (string)GetModCache(ckey);
            if (razorTempl == null || cacheKey == "")
            {
                razorTempl = GetRazorTemplateData(razorTemplName, templateControlPath, theme, lang);
                if (razorTempl == "")
                {
                    // check for non-razor templates
                    razorTemplName = razorTemplName.ToLower().Replace(".cshtml", ".html");
                    ckey = "NBrightBuyRazorOutput" + razorTemplName + "*" + cacheKey + PortalSettings.Current.PortalId.ToString() + "*" + lang; // reset cachekey
                    razorTempl = (string)GetModCache(ckey);
                    if (razorTempl != null)
                    {
                        return razorTempl;
                    }
                    razorTempl = GetRazorTemplateData(razorTemplName, templateControlPath, theme, lang);
                }
                if (razorTempl != "" &&  razorTemplName.EndsWith(".cshtml"))
                {
                    if (obj == null) obj = new NBrightInfo(true);
                    var l = new List<object>();
                    l.Add(obj);
                    if (settings == null) settings = new Dictionary<string, string>();
                    if (!settings.ContainsKey("userid")) settings.Add("userid", UserController.Instance.GetCurrentUserInfo().UserID.ToString());
                    var nbRazor = new NBrightRazor(l, settings, HttpContext.Current.Request.QueryString);
                    nbRazor.FullTemplateName = theme + "." + razorTemplName;
                    nbRazor.TemplateName = razorTemplName;
                    nbRazor.ThemeFolder = theme;
                    nbRazor.Lang = lang;

                    var razorTemplateKey = "NBrightBuyRazorKey" + theme + razorTemplName + PortalSettings.Current.PortalId.ToString() + "*" + lang;
                    var debugMode = StoreSettings.Current.DebugMode;
                    if (cacheKey == "")
                    {
                        debugMode = true;
                    }
                    razorTempl = RazorRender(nbRazor, razorTempl, razorTemplateKey, debugMode);
                    if (!debugMode) SetModCache(moduleid, ckey, razorTempl); // only save to cache if we pass in a cache key.
                }
                else
                {
                    razorTempl = "ERROR - Razor Template Not Found: " + theme + "." + razorTemplName;
                }
            }
            return razorTempl;
        }

        public static string RazorTemplRenderNoCache(string razorTemplName, int moduleid, string cacheKey, object obj, string templateControlPath, string theme, string lang, Dictionary<string, string> settings)
        {
            // do razor template
                var razorTempl = GetRazorTemplateData(razorTemplName, templateControlPath, theme, lang);
                if (razorTempl != "")
                {
                    if (obj == null) obj = new NBrightInfo(true);
                    var l = new List<object>();
                    l.Add(obj);
                    if (settings == null) settings = new Dictionary<string, string>();
                    if (!settings.ContainsKey("userid")) settings.Add("userid", UserController.Instance.GetCurrentUserInfo().UserID.ToString());
                    var nbRazor = new NBrightRazor(l, settings, HttpContext.Current.Request.QueryString);
                    nbRazor.FullTemplateName = theme + "." + razorTemplName;
                    nbRazor.TemplateName = razorTemplName;
                    nbRazor.ThemeFolder = theme;
                    nbRazor.Lang = lang;

                    razorTempl = RazorRender(nbRazor, razorTempl, "", true);
                }
                else
                {
                    razorTempl = "ERROR - Razor Template Not Found: " + theme + "." + razorTemplName;
                }
            return razorTempl;
        }

        /// <summary>
        /// legacy method to render template using both tag tokens and razor tokens
        /// </summary>
        /// <param name="razorTemplName"></param>
        /// <param name="moduleid"></param>
        /// <param name="cacheKey"></param>
        /// <param name="objList"></param>
        /// <param name="templateControlPath"></param>
        /// <param name="theme"></param>
        /// <param name="lang"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static string RazorTemplRenderWithRepeater(string razorTemplName, int moduleid, string cacheKey, List<NBrightInfo> objList, string templateControlPath, string theme, string lang, Dictionary<string, string> settings)
        {
            // do razor template
            var cachekey = "NBrightBuyRazorKey" + razorTemplName + "*" + cacheKey + PortalSettings.Current.PortalId.ToString();
            var razorTempl = (string)GetModCache(cachekey);
            if (razorTempl == null || StoreSettings.Current.DebugMode)
            {
                razorTempl = GetTemplateData(razorTemplName, templateControlPath, theme, settings, lang);
                if (razorTempl != "")
                {
                    if (!objList.Any()) objList.Add(new NBrightInfo(true));
                    razorTempl = GenXmlFunctions.RenderRepeater(objList[0], razorTempl, "", "XMLData", "", settings, null);
                    var razorTemplateKey = "NBrightBuyRazorKey" + theme + razorTemplName + PortalSettings.Current.PortalId.ToString();
                    razorTempl = RazorRender(objList, razorTempl, razorTemplateKey, StoreSettings.Current.DebugMode);
                    SetModCache(moduleid, cachekey, razorTempl);
                }
            }
            return razorTempl;
        }

        /// <summary>
        /// legacy method to render template using both tag tokens and razor tokens
        /// </summary>
        /// <param name="razorTemplName"></param>
        /// <param name="moduleid"></param>
        /// <param name="cacheKey"></param>
        /// <param name="obj"></param>
        /// <param name="templateControlPath"></param>
        /// <param name="theme"></param>
        /// <param name="lang"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static string RazorTemplRenderWithRepeater(string razorTemplName, int moduleid, string cacheKey, NBrightInfo obj, string templateControlPath, string theme, string lang, Dictionary<string, string> settings)
        {
            // do razor template
            var cachekey = "NBrightBuyRazorKey" + razorTemplName + "*" + cacheKey + PortalSettings.Current.PortalId.ToString();
            var razorTempl = (string)GetModCache(cachekey);
            if (razorTempl == null)
            {
                razorTempl = GetTemplateData(razorTemplName, templateControlPath, theme, settings, lang);
                if (razorTempl != "")
                {
                    if (obj == null) obj = new NBrightInfo(true);
                    razorTempl = GenXmlFunctions.RenderRepeater(obj, razorTempl, "", "XMLData", "", settings, null);
                    var razorTemplateKey = "NBrightBuyRazorKey" + theme + razorTemplName + PortalSettings.Current.PortalId.ToString();
                    razorTempl = RazorRender(obj, razorTempl, razorTemplateKey, StoreSettings.Current.DebugMode);
                    SetModCache(moduleid, cachekey, razorTempl);
                }
            }
            return razorTempl;
        }


        public static string RazorRender(Object info, string razorTempl, string templateKey, Boolean debugMode = false)
        {
            var result = "";
            try
            {
                var service = (IRazorEngineService)HttpContext.Current.Application.Get("NBrightBuyIRazorEngineService");
                if (service == null)
                {
                    // do razor test
                    var config = new TemplateServiceConfiguration();
                    config.Debug = debugMode;
                    config.BaseTemplateType = typeof(NBrightBuyRazorTokens<>);
                    service = RazorEngineService.Create(config);
                    HttpContext.Current.Application.Set("NBrightBuyIRazorEngineService", service);
                }
                Engine.Razor = service;
                var israzorCached = Utils.GetCache("nbrightbuyrzcache_" + templateKey); // get a cache flag for razor compile.
                if (israzorCached == null || (string)israzorCached != razorTempl || debugMode)
                {
                    result = Engine.Razor.RunCompile(razorTempl, GetMd5Hash(razorTempl), null, info);
                    Utils.SetCache("nbrightbuyrzcache_" + templateKey, razorTempl);
                }
                else
                {
                    result = Engine.Razor.Run(GetMd5Hash(razorTempl), null, info);
                }

            }
            catch (Exception ex)
            {
                result = ex.ToString();
            }

            return result;
        }

        /// <summary>
        /// work arounf MD5 has for razorengine caching.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static string GetMd5Hash(string input)
        {
            var md5 = MD5.Create();
            var inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            var hash = md5.ComputeHash(inputBytes);
            var sb = new StringBuilder();
            foreach (byte t in hash)
            {
                sb.Append(t.ToString("X2"));
            }
            return sb.ToString();
        }

        public static void RazorIncludePageHeader(int moduleid, Page page, string razorTemplateName, string controlPath, string theme, Dictionary<string, string> settings, ProductData productdata = null)
        {
            if (!page.Items.Contains("nbrightinject")) page.Items.Add("nbrightinject", "");
            if (!page.Items["nbrightinject"].ToString().Contains(razorTemplateName + ","))
            {
                var razorTempl = "";
                if (productdata == null || !productdata.Exists)
                {
                    var nbi = new NBrightInfo();
                    nbi.Lang = Utils.GetCurrentCulture();
                    razorTempl = NBrightBuyUtils.RazorTemplRender(razorTemplateName, moduleid, "RazorIncludePageHeader", nbi, controlPath, theme, Utils.GetCurrentCulture(), settings);
                }
                else
                {
                    razorTempl = NBrightBuyUtils.RazorTemplRender(razorTemplateName, moduleid, "RazorIncludePageHeader" + productdata.Info.ItemID, productdata, controlPath, theme, Utils.GetCurrentCulture(), settings);
                }
                if (razorTempl != "" && !razorTempl.StartsWith("ERROR"))
                {
                    PageIncludes.IncludeTextInHeader(page, razorTempl);
                    page.Items["nbrightinject"] = page.Items["nbrightinject"] + razorTemplateName + ",";
                }
            }
        }

        public static void RazorIncludePageBody(int moduleid, Page page, string razorTemplateName, string controlPath, string theme, Dictionary<string, string> settings)
        {
            if (!page.Items.Contains("nbrightinject")) page.Items.Add("nbrightinject", "");
            if (!page.Items["nbrightinject"].ToString().Contains(razorTemplateName + ","))
            {
                var razorTempl = "";
                var nbi = new NBrightInfo();
                nbi.Lang = Utils.GetCurrentCulture();
                razorTempl = NBrightBuyUtils.RazorTemplRenderNoCache(razorTemplateName, moduleid, "RazorIncludePageBody", nbi, controlPath, theme, Utils.GetCurrentCulture(), settings);
                if (razorTempl != "" && !razorTempl.StartsWith("ERROR"))
                {
                    page.Form.Controls.Add(new LiteralControl(razorTempl));
                    page.Items["nbrightinject"] = page.Items["nbrightinject"] + razorTemplateName + ",";
                }
            }
        }


        public static void RazorIncludePageHeaderNoCache(int moduleid, Page page, string razorTemplateName, string controlPath, string theme, Dictionary<string, string> settings, ProductData productdata = null)
        {

            if (!page.Items.Contains("nbrightinject")) page.Items.Add("nbrightinject", "");
            if (!page.Items["nbrightinject"].ToString().Contains(razorTemplateName + ","))
            {
                var razorTempl = "";
                if (productdata == null)
                {
                    var nbi = new NBrightInfo();
                    nbi.Lang = Utils.GetCurrentCulture();
                    razorTempl = NBrightBuyUtils.RazorTemplRenderNoCache(razorTemplateName, moduleid, "RazorIncludePageHeader", nbi, controlPath, theme, Utils.GetCurrentCulture(), settings);
                }
                else
                {
                    razorTempl = NBrightBuyUtils.RazorTemplRenderNoCache(razorTemplateName, moduleid, "RazorIncludePageHeader", productdata, controlPath, theme, Utils.GetCurrentCulture(), settings);
                }
                if (razorTempl != "" && !razorTempl.StartsWith("ERROR"))
                {
                    PageIncludes.IncludeTextInHeader(page, razorTempl);
                    page.Items["nbrightinject"] = page.Items["nbrightinject"] + razorTemplateName + ",";
                }
            }
        }
        


        #endregion

        #region "Render Functions"

        public static List<NBrightInfo> BuildModelList(NBrightInfo dataItemObj, Boolean addSalePrices = false)
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
                var uInfo = addSalePrices == true ? UserController.Instance.GetCurrentUserInfo() : null;

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

        public static Double GetSalePriceDouble(NBrightInfo dataItemObj)
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

        public static string GetSalePrice(NBrightInfo dataItemObj)
        {
            var price = GetSalePriceDouble(dataItemObj);
            return price.ToString("");
        }

        public static string GetDealerPrice(NBrightInfo dataItemObj)
        {
            var dealprice = "-1";
            var l = BuildModelList(dataItemObj);
            foreach (var m in l)
            {
                var s = m.GetXmlPropertyRaw("genxml/textbox/txtdealercost");
                if (Utils.IsNumeric(s))
                {
                    if ((Convert.ToDouble(s, CultureInfo.GetCultureInfo("en-US")) > 0) && (Convert.ToDouble(s, CultureInfo.GetCultureInfo("en-US")) < Convert.ToDouble(dealprice, CultureInfo.GetCultureInfo("en-US"))) | (dealprice == "-1")) dealprice = s;
                }
            }
            return dealprice;
        }

        public static string GetFromPrice(NBrightInfo dataItemObj)
        {
            var price = "-1";
            var l = BuildModelList(dataItemObj);
            foreach (var m in l)
            {
                var s = m.GetXmlPropertyRaw("genxml/textbox/txtunitcost");
                if (Utils.IsNumeric(s))
                {
                    // NBrightBuy numeric always stored in en-US format.
                    if ((Convert.ToDouble(s, CultureInfo.GetCultureInfo("en-US")) < Convert.ToDouble(price, CultureInfo.GetCultureInfo("en-US"))) | (price == "-1")) price = s;
                }
            }
            return price;
        }

        public static string GetBestPrice(NBrightInfo dataItemObj)
        {
            var fromprice = Convert.ToDouble(GetFromPrice(dataItemObj), CultureInfo.GetCultureInfo("en-US"));
            if (fromprice < 0) fromprice = 0; // make sure we have a valid price
            var saleprice = GetSalePriceDouble(dataItemObj);
            if (saleprice < 0) saleprice = fromprice; // sale price might not exists.

            if (IsDealer())
            {
                var dealerprice = Convert.ToDouble(GetDealerPrice(dataItemObj), CultureInfo.GetCultureInfo("en-US"));
                if (dealerprice <= 0) dealerprice = fromprice; // check for valid dealer price.
                if (fromprice < dealerprice)
                {
                    if (fromprice < saleprice) return fromprice.ToString(CultureInfo.GetCultureInfo("en-US"));
                    return saleprice.ToString(CultureInfo.GetCultureInfo("en-US"));
                }
                if (dealerprice < saleprice) return dealerprice.ToString(CultureInfo.GetCultureInfo("en-US"));
                return saleprice.ToString(CultureInfo.GetCultureInfo("en-US"));
            }
            if (fromprice < saleprice) return fromprice.ToString(CultureInfo.GetCultureInfo("en-US"));
            return saleprice.ToString(CultureInfo.GetCultureInfo("en-US"));
        }

        public static Boolean HasDifferentPrices(NBrightInfo dataItemObj)
        {
            var saleprice = GetSalePriceDouble(dataItemObj);
            if (saleprice >= 0) return true;  // if it's on sale we can assume it has multiple prices
            var nodList = dataItemObj.XMLDoc.SelectNodes("genxml/models/*");
            if (nodList != null)
            {
                //check if we really need to add prices (don't if all the same)
                var holdPrice = "";
                var holdDealerPrice = "";
                var isDealer = IsDealer();
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
                        var mDealerSalePrice = nod.SelectSingleNode("textbox/txtdealersale");
                        if (mDealerSalePrice != null)
                        {
                            if (Utils.IsNumeric(mDealerSalePrice.InnerText))
                            {
                                if (Convert.ToDouble(mDealerSalePrice.InnerText) > 0) return true; // if we have a sale price assume different.
                            }
                        }
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

        public static Boolean IsInStock(NBrightInfo dataItem)
        {
            var nodList = BuildModelList(dataItem);
            foreach (var obj in nodList)
            {
                if (IsModelInStock(obj)) return true;
            }
            return false;
        }

        public static Boolean IsModelInStock(NBrightInfo dataItem)
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

        public static string GetItemDisplay(NBrightInfo obj, string templ, Boolean displayPrices)
        {
            var isDealer = IsDealer();
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
                    var saleprice = obj.GetXmlPropertyDouble("genxml/textbox/txtsaleprice");
                    var price = obj.GetXmlPropertyDouble("genxml/textbox/txtunitcost");
                    var bestprice = price;
                    if (saleprice > 0 && saleprice < price) bestprice = saleprice;

                    var strprice = NBrightBuyUtils.FormatToStoreCurrency(price);
                    var strbestprice = NBrightBuyUtils.FormatToStoreCurrency(bestprice);
                    var strsaleprice = NBrightBuyUtils.FormatToStoreCurrency(saleprice);

                    var strdealerprice = "";
                    var dealerprice = obj.GetXmlPropertyDouble("genxml/textbox/txtdealercost");
                    if (isDealer && dealerprice > 0)
                    {
                        var dealersaleprice = obj.GetXmlPropertyDouble("genxml/textbox/txtdealersale");
                        if (dealersaleprice > 0 && dealerprice > dealersaleprice) dealerprice = dealersaleprice;
                        strdealerprice = NBrightBuyUtils.FormatToStoreCurrency(dealerprice);
                        if (!outText.Contains("{dealerprice}") && (price > dealerprice)) strprice = strdealerprice;
                        if (bestprice <= 0 || dealerprice < bestprice)
                        {
                            bestprice = dealerprice;
                            strbestprice = NBrightBuyUtils.FormatToStoreCurrency(bestprice);
                        }
                    }

                    outText = outText.Replace("{price}", strprice);
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


        public static Dictionary<int, string> BuildCatList(int displaylevels = 20, Boolean showHidden = false, Boolean showArchived = false, int parentid = 0, string catreflist = "", string prefix = "", bool displayCount = false, bool showEmpty = true, string groupref = "", string breadcrumbseparator = ">", string lang = "")
        {
            if (lang == "") lang = Utils.GetCurrentCulture();

            var rtnDic = new Dictionary<int, string>();

            var strCacheKey = "NBrightBuy_BuildCatList" + PortalSettings.Current.PortalId + "*" + displaylevels + "*" + showHidden.ToString(CultureInfo.InvariantCulture) + "*" + showArchived.ToString(CultureInfo.InvariantCulture) + "*" + parentid + "*" + catreflist + "*" + prefix + "*" + Utils.GetCurrentCulture() + "*" + showEmpty + "*" + displayCount + "*" + groupref + "*" + lang;

            var objCache = NBrightBuyUtils.GetModCache(strCacheKey);

            if (objCache == null | StoreSettings.Current.DebugMode)
            {
                var grpCatCtrl = new GrpCatController(lang);
                var d = new Dictionary<int, string>();
                var rtnList = new List<GroupCategoryData>();
                rtnList = grpCatCtrl.GetTreeCategoryList(rtnList, 0, parentid, groupref, breadcrumbseparator);
                var strCount = "";
                foreach (var grpcat in rtnList)
                {
                    if (displayCount) strCount = " (" + grpcat.entrycount.ToString("") + ")";

                    if (grpcat.depth < displaylevels)
                    {
                        if (showEmpty || grpcat.entrycount > 0)
                        {
                            if (grpcat.ishidden == false || showHidden)
                            {
                                var addprefix = new string(' ', grpcat.depth).Replace(" ", prefix);
                                if (catreflist == "")
                                    rtnDic.Add(grpcat.categoryid, addprefix + grpcat.categoryname + strCount);
                                else
                                {
                                    if (grpcat.categoryref != "" &&
                                        (catreflist + ",").Contains(grpcat.categoryref + ","))
                                    {
                                        rtnDic.Add(grpcat.categoryid, addprefix + grpcat.categoryname + strCount);
                                    }
                                }
                            }
                        }
                    }
                }
                NBrightBuyUtils.SetModCache(-1, strCacheKey, rtnDic);

            }
            else
            {
                rtnDic = (Dictionary<int, string>)objCache;
            }
            return rtnDic;
        }

        public static Dictionary<int, string> BuildPropertyList(int displaylevels = 20, Boolean showHidden = false, Boolean showArchived = false, int parentid = 0, string catreflist = "", string prefix = "", bool displayCount = false, bool showEmpty = true, string groupref = "", string breadcrumbseparator = ">", string lang = "")
        {
            if (lang == "") lang = Utils.GetCurrentCulture();

            var rtnDic = new Dictionary<int, string>();

            var strCacheKey = "NBrightBuy_BuildPropertyList" + PortalSettings.Current.PortalId + "*" + displaylevels + "*" + showHidden.ToString(CultureInfo.InvariantCulture) + "*" + showArchived.ToString(CultureInfo.InvariantCulture) + "*" + parentid + "*" + catreflist + "*" + prefix + "*" + Utils.GetCurrentCulture() + "*" + showEmpty + "*" + displayCount + "*" + groupref + "*" + lang;

            var objCache = NBrightBuyUtils.GetModCache(strCacheKey);

            if (objCache == null | StoreSettings.Current.DebugMode)
            {
                var grpCatCtrl = new GrpCatController(lang);
                var d = new Dictionary<int, string>();
                var rtnList = new List<GroupCategoryData>();
                rtnList = grpCatCtrl.GetTreePropertyList(breadcrumbseparator);
                var strCount = "";
                foreach (var grpcat in rtnList)
                {
                    if (displayCount) strCount = " (" + grpcat.entrycount.ToString("") + ")";

                    if (grpcat.depth < displaylevels)
                    {
                        if (showEmpty || grpcat.entrycount > 0)
                        {
                            if (grpcat.ishidden == false || showHidden)
                            {
                                if (!rtnDic.ContainsKey(grpcat.categoryid))
                                {
                                    var addprefix = new string(' ', grpcat.depth).Replace(" ", prefix);
                                    if (catreflist == "")
                                        rtnDic.Add(grpcat.categoryid, addprefix + grpcat.categoryname + strCount);
                                    else
                                    {
                                        if (grpcat.categoryref != "" &&
                                            (catreflist + ",").Contains(grpcat.categoryref + ","))
                                        {
                                            rtnDic.Add(grpcat.categoryid, addprefix + grpcat.categoryname + strCount);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                NBrightBuyUtils.SetModCache(-1, strCacheKey, rtnDic);

            }
            else
            {
                rtnDic = (Dictionary<int, string>)objCache;
            }
            return rtnDic;
        }

        #endregion

        #region "security functions"

        public static Boolean IsClientOnly()
        {
            if (UserController.Instance.GetCurrentUserInfo().IsInRole(StoreSettings.ClientEditorRole) && (!UserController.Instance.GetCurrentUserInfo().IsInRole(StoreSettings.EditorRole) && !UserController.Instance.GetCurrentUserInfo().IsInRole(StoreSettings.ManagerRole) && !UserController.Instance.GetCurrentUserInfo().IsInRole("Administrators")))
            {
                return true;
            }
            return false;
        }


        public static Boolean CheckRights()
        {
            if (UserController.Instance.GetCurrentUserInfo().IsInRole(StoreSettings.ClientEditorRole) || UserController.Instance.GetCurrentUserInfo().IsInRole(StoreSettings.ManagerRole) || UserController.Instance.GetCurrentUserInfo().IsInRole(StoreSettings.EditorRole) || UserController.Instance.GetCurrentUserInfo().IsInRole("Administrators"))
            {
                return true;
            }
            return false;
        }

        public static Boolean CheckManagerRights()
        {
            if (UserController.Instance.GetCurrentUserInfo().IsInRole(StoreSettings.ManagerRole) || UserController.Instance.GetCurrentUserInfo().IsInRole("Administrators"))
            {
                return true;
            }
            return false;
        }

        public static Boolean IsDealer()
        {
            if (!StoreSettings.Current.GetBool("enabledealer")) return false;
            if (StoreSettings.Current.DealerRole == "*") return true; // * for all users
            if (CmsProviderManager.Default.IsInRole(StoreSettings.Current.DealerRole)) return true;
            return false;
        }


        #endregion

        #region "Razor functions"

        public static string RenderCart(string theme, string carttemplate, string controlPath = "/DesktopModules/NBright/NBrightBuy",int moduleid = -1)
        {
            var razorTempl = "";
            if (carttemplate != "")
            {
                var currentcart = new CartData(PortalSettings.Current.PortalId);
                var settings = GetSettings(PortalSettings.Current.PortalId, moduleid);
                var passSettings = GetPassSettings(settings);
                razorTempl = NBrightBuyUtils.RazorTemplRender(carttemplate, 0, "", currentcart, controlPath, theme, Utils.GetCurrentCulture(), passSettings);
            }
            return razorTempl;
        }

        public static string RenderProfile(string theme, string carttemplate, string controlPath = "/DesktopModules/NBright/NBrightBuy")
        {
            var razorTempl = "";
            if (carttemplate != "")
            {
                var profileData = new ProfileData();
                var objprof = profileData.GetProfile();
                razorTempl = NBrightBuyUtils.RazorTemplRender(carttemplate, 0, "", objprof, controlPath, theme, Utils.GetCurrentCulture(), StoreSettings.Current.Settings());
            }
            return razorTempl;
        }

        #endregion

        #region "http functions"

        public static string BaseSiteUrl()
        {
            if (HttpContext.Current.Request.ApplicationPath != null)
            {
                var baseUrl = string.Format("{0}://{1}{2}",
                    HttpContext.Current.Request.Url.Scheme,
                    HttpContext.Current.Request.ServerVariables["HTTP_HOST"],
                    (HttpContext.Current.Request.ApplicationPath.Equals("/")) ? string.Empty : HttpContext.Current.Request.ApplicationPath
                    );
                return baseUrl;
            }
            return "";
        }
        #endregion

        public static String CopyAllCatXref(int categoryId,int newCategoryId, Boolean moverecords = false)
        {
            var strOut = NBrightBuyUtils.GetResxMessage("general_fail");
            try
            {
                var strFilter = " and XrefItemId = " + categoryId.ToString("") + " ";

                if (newCategoryId >= 0 && categoryId >= 0)
                {
                    var objCtrl = new NBrightBuyController();
                    var objList = objCtrl.GetList(PortalSettings.Current.PortalId, -1, "CATXREF", strFilter);

                    foreach (var obj in objList)
                    {
                        var prdData = new ProductData(obj.ParentItemId, PortalSettings.Current.PortalId, Utils.GetCurrentCulture());

                        if (moverecords)
                        {
                            prdData.RemoveCategory(obj.XrefItemId);
                        }

                        prdData.AddCategory(newCategoryId);
                    }

                    strOut = NBrightBuyUtils.GetResxMessage();
                }

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
            return strOut;
        }

        public static int CalcPricePercentageDiff(double originalprice,double newprice,int fractionaldigits = 0)
        {
           return (int)Math.Round(((originalprice - newprice) / originalprice) *100, fractionaldigits);
        }


        /// <summary>
        /// Set thread langauge to correct langauge to match html page.
        /// uses ajaxInfo.GetXmlProperty("genxml/hidden/editlang"); param
        /// returns the edit culture code from the "editlang" param.
        /// </summary>
        /// <param name="context"></param>
        /// <returns>editlang</returns>
        public static string SetContextLangauge(HttpContext context)
        {
            var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);
            return SetContextLangauge(ajaxInfo); // Ajax breaks context with DNN, so reset the context language to match the client.
        }

        public static string SetContextLangauge(NBrightInfo ajaxInfo = null)
        {
            // NOTE: "genxml/hidden/editlang" and "genxml/hidden/uilang" should be set in the template for langauge to work OK.
            // set langauge if we have it passed.
            if (ajaxInfo == null) ajaxInfo = new NBrightInfo(true);
            // set the context  culturecode, so any DNN functions use the correct culture 
            var uilang = GetUILang(ajaxInfo); //UI Langauge
            if (uilang != System.Threading.Thread.CurrentThread.CurrentCulture.ToString())
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo(uilang);
            }
             return uilang;
        }


        public static List<NBrightInfo> GetPagingData(int TotalRecords, int PageSize, int CurrentPage, int pageLinksPerPage = 10)
        {
            var pageL = new List<NBrightInfo>();

            var lastPage = Convert.ToInt32(TotalRecords / PageSize);
            if (TotalRecords != (lastPage * PageSize)) lastPage = lastPage + 1;
            if (lastPage == 1) return pageL;            //if only one page, don;t process

            var rangebase = Convert.ToInt32((CurrentPage - 1) / pageLinksPerPage);
            var lowNum = (rangebase * pageLinksPerPage) + 1;
            var highNum = lowNum + (pageLinksPerPage - 1);
            if (highNum > Convert.ToInt32(lastPage)) highNum = Convert.ToInt32(lastPage);
            if (lowNum < 1) lowNum = 1;


            if ((lowNum > 1) && (CurrentPage > pageLinksPerPage))
            {
                var nbi = new NBrightInfo(true);
                nbi.SetXmlProperty("genxml/pagenumber", "1");
                nbi.SetXmlProperty("genxml/text", "<<");
                nbi.SetXmlProperty("genxml/type", "head1");
                pageL.Add(nbi);
            }

            if ((CurrentPage > 1))
            {
                var nbi = new NBrightInfo(true);
                nbi.SetXmlProperty("genxml/pagenumber", Convert.ToString(CurrentPage - 1));
                nbi.SetXmlProperty("genxml/text", "<");
                nbi.SetXmlProperty("genxml/type", "head2");
                pageL.Add(nbi);
            }

            if ((lowNum > 1) && (CurrentPage > pageLinksPerPage))
            {
                var nbi = new NBrightInfo(true);
                nbi.SetXmlProperty("genxml/pagenumber", Convert.ToString(lowNum - 1));
                nbi.SetXmlProperty("genxml/text", "...");
                nbi.SetXmlProperty("genxml/type", "headsection");
                pageL.Add(nbi);
            }


            for (int i = lowNum; i <= highNum; i++)
            {
                var nbi = new NBrightInfo(true);
                nbi.SetXmlProperty("genxml/pagenumber",i.ToString(""));
                nbi.SetXmlProperty("genxml/text", i.ToString(""));
                nbi.SetXmlProperty("genxml/type", "page");
                if (CurrentPage == i)
                {
                    nbi.SetXmlProperty("genxml/currentpage", "True");
                }
                else
                {
                    nbi.SetXmlProperty("genxml/currentpage", "False");
                }
                pageL.Add(nbi);
            }



            if ((lastPage > highNum))
            {
                var nbi = new NBrightInfo(true);
                nbi.SetXmlProperty("genxml/pagenumber", Convert.ToString(highNum + 1));
                nbi.SetXmlProperty("genxml/text", "...");
                nbi.SetXmlProperty("genxml/type", "foot1");
                pageL.Add(nbi);
            }


            if ((lastPage > CurrentPage))
            {
                var nbi = new NBrightInfo(true);
                nbi.SetXmlProperty("genxml/pagenumber", Convert.ToString(CurrentPage + 1));
                nbi.SetXmlProperty("genxml/text", ">");
                nbi.SetXmlProperty("genxml/type", "foot2");
                pageL.Add(nbi);
            }

            if ((lastPage != highNum) && (lastPage > CurrentPage))
            {
                var nbi = new NBrightInfo(true);
                nbi.SetXmlProperty("genxml/pagenumber", Convert.ToString(lastPage));
                nbi.SetXmlProperty("genxml/text", ">>");
                nbi.SetXmlProperty("genxml/type", "foot1");
                pageL.Add(nbi);
            }






            return pageL;
        }



        #region "AJAX functions"

        public static Dictionary<String, String> GetAjaxDictionary(HttpContext context)
        {
            var objInfo = GetAjaxInfo(context);
            var dic = objInfo.ToDictionary();
            return dic;
        }

        /// <summary>
        /// Put Ajax data into a NBrightInfo class for processing
        /// </summary>
        /// <param name="context">Http context</param>
        /// <param name="updatefields">If true only fields marked with update attribute are returned.</param>
        /// <returns></returns>
        public static NBrightInfo GetAjaxInfo(HttpContext context, Boolean updatefields = false)
        {
            var strIn = HttpUtility.UrlDecode(Utils.RequestParam(context, "inputxml"));

            var objInfo = new NBrightInfo();

            objInfo.ItemID = -1;
            objInfo.TypeCode = "AJAXDATA";
            objInfo.PortalId = PortalSettings.Current.PortalId;
            if (updatefields)
            {
                objInfo.UpdateAjax(strIn);
            }
            else
            {
                var xmlData = GenXmlFunctions.GetGenXmlByAjax(strIn, "");
                objInfo.XMLData = xmlData;
            }
            if (objInfo.Lang == "") objInfo.Lang = objInfo.GetXmlProperty("genxml/hidden/editlang");
            if (objInfo.Lang == "") objInfo.Lang = Utils.GetCurrentCulture(); // make sure we have the langauge in the object.
            return objInfo;
        }

        public static List<NBrightInfo> GetAjaxInfoList(HttpContext context)
        {
            var rtnList = new List<NBrightInfo>();
            var strIn = HttpUtility.UrlDecode(Utils.RequestParam(context, "inputxml"));
            var xmlDoc1 = new XmlDocument();
            if (!String.IsNullOrEmpty(strIn))
            {

                xmlDoc1.LoadXml(strIn);

                var xmlNodeList = xmlDoc1.SelectNodes("root/*");
                if (xmlNodeList != null)
                {
                    foreach (XmlNode nod in xmlNodeList)
                    {
                        var xmlData = GenXmlFunctions.GetGenXmlByAjax(nod.OuterXml, "");
                        var objInfo = new NBrightInfo();

                        objInfo.ItemID = -1;
                        objInfo.TypeCode = "AJAXDATA";
                        objInfo.PortalId = PortalSettings.Current.PortalId;
                        objInfo.XMLData = xmlData;
                        if (objInfo.Lang == "") objInfo.Lang = objInfo.GetXmlProperty("genxml/hidden/editlang");
                        if (objInfo.Lang == "") objInfo.Lang = Utils.GetCurrentCulture(); // make sure we have the langauge in the object.
                        rtnList.Add(objInfo);
                    }
                }
            }
            return rtnList;
        }

        /// <summary>
        /// Get a list of Ajax XML for each item posted as a list of ajax records. 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static List<string> GetAjaxXmlFieldsList(HttpContext context)
        {
            var rtnList = new List<string>();
            var xmlAjaxData = HttpUtility.UrlDecode(Utils.RequestParam(context, "inputxml"));
            // get each returned xml root node
            var xmlDoc1 = new XmlDocument();
            if (!string.IsNullOrEmpty(xmlAjaxData))
            {
                xmlDoc1.LoadXml(xmlAjaxData);
                var xmlNodeList = xmlDoc1.SelectNodes("root/root");
                if (xmlNodeList != null)
                {
                    foreach (XmlNode nod in xmlNodeList)
                    {
                        rtnList.Add(nod.OuterXml);
                    }
                }
            }
            return rtnList;
        }

        /// <summary>
        /// Return data from Ajax in ajax string format
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetAjaxXmlFields(HttpContext context)
        {
            var strIn = HttpUtility.UrlDecode(Utils.RequestParam(context, "inputxml"));
            return strIn;
        }

        public static NBrightInfo GetAjaxFields(HttpContext context)
        {
            var strIn = HttpUtility.UrlDecode(Utils.RequestParam(context, "inputxml"));
            var xmlData = GenXmlFunctions.GetGenXmlByAjax(strIn, "");
            var objInfo = new NBrightInfo();

            objInfo.ItemID = -1;
            objInfo.TypeCode = "AJAXDATA";
            objInfo.XMLData = xmlData;
            return objInfo;
        }

        public static String GetUILang(NBrightInfo ajaxInfo, string defaultlang = "")
        {
            var UserLang = ajaxInfo.GetXmlProperty("genxml/hidden/uilang");
            if (UserLang != "") return UserLang;
            UserLang = ajaxInfo.GetXmlProperty("genxml/hidden/uilang1");
            if (UserLang != "") return UserLang;
            UserLang = ajaxInfo.GetXmlProperty("genxml/hidden/uilang2");
            if (UserLang != "") return UserLang;
            UserLang = ajaxInfo.GetXmlProperty("genxml/hidden/uilang3");
            if (UserLang != "") return UserLang;
            UserLang = GetEditLang(ajaxInfo,defaultlang);
            return defaultlang;
        }

        public static String GetEditLang(NBrightInfo ajaxInfo, string defaultlang = "")
        {
            var editLang = ajaxInfo.GetXmlProperty("genxml/hidden/editlang");
            if (editLang != "") return editLang;
            editLang = ajaxInfo.GetXmlProperty("genxml/hidden/editlang1");
            if (editLang != "") return editLang;
            editLang = ajaxInfo.GetXmlProperty("genxml/hidden/editlang2");
            if (editLang != "") return editLang;
            editLang = ajaxInfo.GetXmlProperty("genxml/hidden/editlang3");
            if (editLang != "") return editLang;
            editLang = ajaxInfo.GetXmlProperty("genxml/hidden/editlanguage");
            if (editLang != "") return editLang;
            editLang = ajaxInfo.GetXmlProperty("genxml/hidden/lang");
            if (editLang != "") return editLang;
            editLang = ajaxInfo.GetXmlProperty("genxml/hidden/uilang");
            if (editLang != "") return editLang;
            editLang = ajaxInfo.GetXmlProperty("genxml/hidden/language");
            if (editLang != "") return editLang;
            if (defaultlang == "")
            {
                return Utils.GetCurrentCulture();
            }
            return defaultlang;
        }

        public static String GetNextLang(NBrightInfo ajaxInfo, string defaultlang = "")
        {
            var nextLang = ajaxInfo.GetXmlProperty("genxml/hidden/nextlang");
            if (nextLang != "") return nextLang;
            nextLang = ajaxInfo.GetXmlProperty("genxml/hidden/nextlang1");
            if (nextLang != "") return nextLang;
            nextLang = ajaxInfo.GetXmlProperty("genxml/hidden/nextlang2");
            if (nextLang != "") return nextLang;
            nextLang = ajaxInfo.GetXmlProperty("genxml/hidden/nextlang3");
            if (nextLang != "") return nextLang;
            if (defaultlang == "")
            {
                return Utils.GetCurrentCulture();
            }
            return defaultlang;
        }

        #endregion
    }
}

