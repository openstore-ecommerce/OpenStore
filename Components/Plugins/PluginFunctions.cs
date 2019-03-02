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

namespace Nevoweb.DNN.NBrightBuy.Components.Plugins
{
    public static class PluginFunctions
    {
        #region "Admin Methods"

        public static string TemplateRelPath = "/DesktopModules/NBright/NBrightBuy";

        public static string ProcessCommand(string paramCmd, HttpContext context, string editlang = "")
        {
            var strOut = "PLUGIN - ERROR!! - No Security rights or function command.";
            var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);

            switch (paramCmd)
            {
                case "plugins_admin_getlist":
                    if (!NBrightBuyUtils.CheckRights()) break;
                    strOut = PluginAdminList(context);
                    break;
                case "plugins_admin_getdetail":
                    if (!NBrightBuyUtils.CheckRights()) break;
                    strOut = PluginAdminDetail(context);
                    break;
                case "plugins_adminaddnew":
                    if (!NBrightBuyUtils.CheckRights()) break;
                    strOut = PluginAddNew(context);
                    break; 
                case "plugins_addpluginsmodels":
                    if (!NBrightBuyUtils.CheckRights()) break;
                    PluginAddInterface(context);
                    strOut = PluginAdminDetail(context);
                    break;
                case "plugins_admin_save":
                    if (!NBrightBuyUtils.CheckRights()) break;
                    PluginSave(context);
                    strOut = PluginAdminDetail(context);
                    break;
                case "plugins_admin_delete":
                    if (!NBrightBuyUtils.CheckRights()) break;
                    PluginDelete(context);
                    strOut = PluginAdminList(context);
                    break;
                case "plugins_movepluginsadmin":
                    if (!NBrightBuyUtils.CheckRights()) break;
                    PluginMove(context);
                    strOut = PluginAdminList(context);
                    break;                    
            }
            return strOut;
        }


        #endregion


        public static string PluginAdminList(HttpContext context)
        {

            try
            {
                if (NBrightBuyUtils.CheckRights())
                {
                    var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                    var list =  PluginUtils.GetPluginList();
                    return RenderPluginAdminList(list, ajaxInfo, 0);

                }
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                return ex.ToString();
            }
            return "";
        }

        public static String RenderPluginAdminList(List<NBrightInfo> list, NBrightInfo ajaxInfo, int recordCount)
        {

            try
            {
                if (NBrightBuyUtils.CheckRights())
                {
                    if (list == null) return "";
                    var strOut = "";

                    // select a specific entity data type for the product (used by plugins)
                    var themeFolder = ajaxInfo.GetXmlProperty("genxml/hidden/themefolder");
                    if (themeFolder == "") themeFolder = "config";
                    var razortemplate = ajaxInfo.GetXmlProperty("genxml/hidden/razortemplate");
                    if (razortemplate == "")
                    {
                        razortemplate = "Admin_pluginsList.cshtml";
                    }

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

                    strOut = NBrightBuyUtils.RazorTemplRenderList(razortemplate, 0, "", list, TemplateRelPath, themeFolder, Utils.GetCurrentCulture(), passSettings);

                    return strOut;

                }
                return "";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }

        public static String PluginAdminDetail(HttpContext context)
        {
            try
            {
                if (NBrightBuyUtils.CheckRights())
                {
                    var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);

                    var strOut = "";
                    var selecteditemid = ajaxInfo.GetXmlProperty("genxml/hidden/selecteditemid");
                    if (Utils.IsNumeric(selecteditemid))
                    {
                        var themeFolder = ajaxInfo.GetXmlProperty("genxml/hidden/themefolder");
                        if (themeFolder == "") themeFolder = "config";
                        var razortemplate = ajaxInfo.GetXmlProperty("genxml/hidden/razortemplate");

                        var passSettings =  NBrightBuyUtils.GetPassSettings(ajaxInfo);

                        var objCtrl = new NBrightBuyController();
                        var info = objCtrl.GetData(Convert.ToInt32(selecteditemid));
                        var pluginRecord = new PluginRecord(info);

                        strOut = NBrightBuyUtils.RazorTemplRender(razortemplate, 0, "", pluginRecord, TemplateRelPath, themeFolder, Utils.GetCurrentCulture(), passSettings);
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

        public static String PluginAddNew(HttpContext context)
        {
            try
            {
                if (NBrightBuyUtils.CheckRights())
                {
                    var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);

                    var strOut = "";
                    var themeFolder = ajaxInfo.GetXmlProperty("genxml/hidden/themefolder");
                    if (themeFolder == "") themeFolder = "config";
                    var razortemplate = ajaxInfo.GetXmlProperty("genxml/hidden/razortemplate");

                    var passSettings = NBrightBuyUtils.GetPassSettings(ajaxInfo);

                    var info = new NBrightInfo(true);
                    info.ItemID = -1;
                    info.Lang = Utils.GetCurrentCulture();
                    info.SetXmlProperty("genxml/hidden/index","99");
                    info.TypeCode = "PLUGIN";
                    info.GUIDKey = Utils.GetUniqueKey(12);
                    var objCtrl = new NBrightBuyController();
                    info.ItemID = objCtrl.Update(info);
                    var pluginRecord = new PluginRecord(info);

                    strOut = NBrightBuyUtils.RazorTemplRender(razortemplate, 0, "", pluginRecord, TemplateRelPath, themeFolder, Utils.GetCurrentCulture(), passSettings);
                    return strOut;
                }
                return "";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public static void PluginAddInterface(HttpContext context)
        {
            try
            {
                if (NBrightBuyUtils.CheckRights())
                {
                    var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                    var selecteditemid = ajaxInfo.GetXmlProperty("genxml/hidden/selecteditemid");
                    if (Utils.IsNumeric(selecteditemid))
                    {
                        var objCtrl = new NBrightBuyController();
                        var info = objCtrl.GetData(Convert.ToInt32(selecteditemid));
                        var pluginRecord = new PluginRecord(info);
                        pluginRecord.AddInterface();
                    }
                }
            }
            catch (Exception ex)
            {
                // ignore
            }
        }

        public static void PluginSave(HttpContext context)
        {
            if (NBrightBuyUtils.CheckRights())
            {
                var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);
                var itemid = ajaxInfo.GetXmlProperty("genxml/hidden/itemid");
                if (Utils.IsNumeric(itemid))
                {
                    var objCtrl = new NBrightBuyController();
                    var info = objCtrl.GetData(Convert.ToInt32(itemid));
                    var pluginRecord = new PluginRecord(info);
                    var modelXml = Utils.UnCode(ajaxInfo.GetXmlProperty("genxml/hidden/xmlupdatemodeldata"));

                    ajaxInfo.RemoveXmlNode("genxml/hidden/xmlupdatemodeldata");
                    pluginRecord.Info().XMLData = ajaxInfo.XMLData; 

                    // check for unique ctrl ref
                    var ctrlref = pluginRecord.Info().GetXmlProperty("genxml/textbox/ctrl");
                    var ctrltest = objCtrl.GetByGuidKey(PortalSettings.Current.PortalId, -1, "PLUGIN", ctrlref);
                    if (ctrltest != null)
                    {
                        if (ctrltest.ItemID != pluginRecord.Info().ItemID)
                        {
                            pluginRecord.Info().SetXmlProperty("genxml/textbox/ctrl", pluginRecord.Info().GetXmlProperty("genxml/textbox/ctrl") + Utils.GetUniqueKey());
                        }
                    }

                    // make sure index is in correct format, (FLOAT) for SQL
                    pluginRecord.Info().SetXmlProperty("genxml/hidden/index", (pluginRecord.Info().GetXmlPropertyInt("genxml/hidden/index").ToString()), TypeCode.Double);
                    pluginRecord.Info().RemoveXmlNode("genxml/hidden/itemid");
                    pluginRecord.Info().RemoveXmlNode("genxml/hidden/editlanguage");
                    pluginRecord.Info().RemoveXmlNode("genxml/hidden/uilang1");
                    pluginRecord.Info().GUIDKey = pluginRecord.Info().GetXmlProperty("genxml/textbox/ctrl");

                    pluginRecord.UpdateModels(modelXml, Utils.GetCurrentCulture());
                    objCtrl.Update(pluginRecord.Info());

                    // remove save GetData cache
                    DataCache.ClearCache();

                    //load entity typecode to DB idx settings.
                    NBrightBuyUtils.RegisterEnityTypeToDataBase();

                }
            }
        }

        public static void PluginDelete(HttpContext context)
        {
            if (NBrightBuyUtils.CheckRights())
            {
                var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);
                var itemid = ajaxInfo.GetXmlProperty("genxml/hidden/itemid");
                if (Utils.IsNumeric(itemid))
                {
                    var objCtrl = new NBrightBuyController();
                    objCtrl.Delete(Convert.ToInt32(itemid));

                    PluginUtils.CopySystemPluginsToPortal();

                    // remove save GetData cache
                    DataCache.ClearCache();

                }
            }
        }

        public static void PluginMove(HttpContext context)
        {
            if (NBrightBuyUtils.CheckRights())
            {
                var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);
                var movepluginsid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/movepluginsid");
                var movetopluginsid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/movetopluginsid");
                if (movepluginsid > 0 && movetopluginsid > 0)
                {
                    var objCtrl = new NBrightBuyController();
                    var infoTo = objCtrl.GetData(movetopluginsid);
                    var info = objCtrl.GetData(movepluginsid);

                    if (infoTo.GetXmlPropertyDouble("genxml/hidden/index") < info.GetXmlPropertyDouble("genxml/hidden/index"))
                    {
                        info.SetXmlProperty("genxml/hidden/index", (infoTo.GetXmlPropertyDouble("genxml/hidden/index") - 0.5).ToString("00.0"), TypeCode.Double);
                    }
                    else
                    {
                        info.SetXmlProperty("genxml/hidden/index", (infoTo.GetXmlPropertyDouble("genxml/hidden/index") + 0.5).ToString("00.0"), TypeCode.Double);
                    }
                    objCtrl.Update(info);

                    // remove save GetData cache so we read changed data
                    DataCache.ClearCache();

                    PluginUtils.ResequenceRecords();

                    // remove save GetData cache
                    DataCache.ClearCache();

                }
            }
        }

    }
}
