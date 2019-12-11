using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.UI.WebControls;
using System.Xml;
using DotNetNuke.Entities.Portals;
using NBrightCore.TemplateEngine;
using NBrightCore.render;
using NBrightDNN;
using NBrightCore.common;
using NBrightCore.providers;
using DotNetNuke.Entities.Users;
using System.Web;

namespace Nevoweb.DNN.NBrightBuy.Components
{
    public class PluginUtils
    {

        public static List<NBrightInfo> GetPluginList()
        {
            return GetPluginList(PortalSettings.Current.PortalId);
        }

        public static List<NBrightInfo> GetPluginList(int portalId)
        {
            var objCtrl = new NBrightBuyController();
            var rtnList = objCtrl.GetList(portalId, -1, "PLUGIN","", "order by nb1.xmldata.value('(genxml/hidden/index)[1]','float')");
            if (rtnList.Count == 0)
            {
                rtnList = CreatePortalPlugins(portalId);
            }
            return rtnList;
        }

        public static void CreateSystemPlugins()
        {
            CreateSystemPlugins(PortalSettings.Current.PortalId);
        }

        public static void CreateSystemPlugins(int portalId)
        {
            var cachekey = "pluginlistsystem";
            var pList = NBrightBuyUtils.GetCache(cachekey);
            if (pList == null)
            {
                var objCtrl = new NBrightBuyController();
                var rtnList = objCtrl.GetList(99999, -1, "PLUGIN", "", "order by nb1.xmldata.value('(genxml/hidden/index)[1]','float')");
                if (rtnList.Count == 0)
                {
                    var pluginList = new List<NBrightInfo>();
                    var info = new NBrightInfo();
                    // no menuplugin.xml exists, so must be new install, get new config
                    var pluginfoldermappath = System.Web.Hosting.HostingEnvironment.MapPath(StoreSettings.NBrightBuyPath() + "/Plugins");
                    if (pluginfoldermappath != null && Directory.Exists(pluginfoldermappath) && File.Exists(pluginfoldermappath + "\\menu.config"))
                    {
                        var menuconfig = Utils.ReadFile(pluginfoldermappath + "\\menu.config");
                        if (menuconfig != "")
                        {
                            info.XMLData = menuconfig;
                            info.PortalId = 99999;
                            pluginList = CalcSystemPluginList(info);
                            CreateDBrecords(pluginList, 99999);
                            CreatePortalPlugins(portalId);
                        }
                    }
                }
                NBrightBuyUtils.SetCache(cachekey, "True");
            }
        }

        private static List<NBrightInfo> CreatePortalPlugins(int portalId)
        {
            var pluginList = new List<NBrightInfo>();

            var info = new NBrightInfo();
            info.PortalId = portalId;
            
            var templCtrl = NBrightBuyUtils.GetTemplateGetter(portalId, "config");
            var menuplugin = templCtrl.GetTemplateData("menuplugin.xml", Utils.GetCurrentCulture(), true, true, true, StoreSettings.Current.Settings());
            if (menuplugin != "")
            {
                // menuplugin.xml exists, which is legacy data for this portal.
                info.XMLData = menuplugin;
                pluginList = CalcPortalPluginList(info);
                if (pluginList.Any())
                {
                    CreateDBrecords(pluginList, portalId);
                }
                else
                {
                    // we may not have a menuplugin.xml, this only exists on upgrade.
                    CopySystemPluginsToPortal();
                }
            }
            else
            {
                CopySystemPluginsToPortal();
            }

            return pluginList;
        }


        /// <summary>
        /// Build list of plugins from XML config or legacy files.
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        private static List<NBrightInfo> CalcSystemPluginList(NBrightInfo info)
        {
            var rtnList = new List<NBrightInfo>();
            var xmlNodeList = info.XMLDoc.SelectNodes("genxml/plugin/*");
            if (xmlNodeList != null)
            {
                foreach (XmlNode carNod in xmlNodeList)
                {
                    var newInfo = new NBrightInfo { XMLData = carNod.OuterXml };
                    newInfo.ItemID = rtnList.Count;
                    newInfo.PortalId = 99999;
                    newInfo.SetXmlProperty("genxml/hidden/index", rtnList.Count.ToString(""),TypeCode.Double);
                    newInfo.GUIDKey = newInfo.GetXmlProperty("genxml/textbox/ctrl");
                    rtnList.Add(newInfo);
                }
            }

            return rtnList;
        }

        private static List<NBrightInfo> CalcPortalPluginList(NBrightInfo info)
        {
            var templCtrl = NBrightBuyUtils.GetTemplateGetter(PortalSettings.Current.PortalId, "config");

            var rtnList = new List<NBrightInfo>();

            // get the systemlevel, incase this is an update and we have new system level provider that needs to be added
            // Some systems create their own portal specific menu we assume they don't require new updates from NBS core, so take that if we have one.
            var menupluginsys = templCtrl.GetTemplateData("menuplugin" + PortalSettings.Current.PortalId + ".xml", Utils.GetCurrentCulture(), true, true, false, StoreSettings.Current.Settings());
            // if no portal specific menus exist, take the default
            if (menupluginsys == "") menupluginsys = templCtrl.GetTemplateData("menuplugin.xml", Utils.GetCurrentCulture(), true, true, false, StoreSettings.Current.Settings());
            var infosys = new NBrightInfo();
            infosys.XMLData = menupluginsys;
            if (infosys.XMLDoc != null)
            {
                var xmlNodeList2 = infosys.XMLDoc.SelectNodes("genxml/plugin/*");
                if (xmlNodeList2 != null)
                {
                    foreach (XmlNode carNod in xmlNodeList2)
                    {
                        var newInfo = new NBrightInfo { XMLData = carNod.OuterXml };
                        newInfo.GUIDKey = newInfo.GetXmlProperty("genxml/textbox/ctrl");
                        var resultsys = rtnList.Where(p => p.GUIDKey == newInfo.GUIDKey);
                        if (!resultsys.Any())
                        {
                            // add the missing plugin to the active list
                            newInfo.ItemID = rtnList.Count;
                            newInfo.PortalId = PortalSettings.Current.PortalId;
                            newInfo.SetXmlProperty("genxml/hidden/index", rtnList.Count.ToString(""), TypeCode.Double);
                            newInfo.GUIDKey = newInfo.GetXmlProperty("genxml/textbox/ctrl");
                            rtnList.Add(newInfo);
                        }
                    }
                }
            }

            return rtnList;
        }

        private static void CreateDBrecords(List<NBrightInfo> pluginList,int portalId)
        {
            var objCtrl = new NBrightBuyController();
            foreach (var p in pluginList)
            {
                p.ItemID = -1;
                p.GUIDKey = p.GetXmlProperty("genxml/textbox/ctrl");
                p.PortalId = portalId;
                p.Lang = "";
                p.ParentItemId = 0;
                p.ModuleId = -1;
                p.XrefItemId = 0;
                p.UserId = 0;
                p.TypeCode = "PLUGIN";

                p.SetXmlProperty("genxml/hidden/index", p.GetXmlPropertyDouble("genxml/hidden/index").ToString("00.0"), TypeCode.Double);

                var interfaces = p.XMLDoc.SelectNodes("genxml/interfaces/*");
                if (interfaces.Count == 0)
                {
                    // possible legacy format, change.
                    p.SetXmlProperty("genxml/interfaces", "");
                    p.SetXmlProperty("genxml/interfaces/genxml", "");
                    p.SetXmlProperty("genxml/interfaces/genxml/dropdownlist", "");
                    p.SetXmlProperty("genxml/interfaces/genxml/checkbox", "");
                    p.SetXmlProperty("genxml/interfaces/genxml/textbox", "");
                    p.SetXmlProperty("genxml/interfaces/genxml/dropdownlist/providertype", p.GetXmlProperty("genxml/dropdownlist/providertype"));
                    p.SetXmlProperty("genxml/interfaces/genxml/checkbox/active", p.GetXmlProperty("genxml/checkbox/active"));
                    p.SetXmlProperty("genxml/interfaces/genxml/textbox/namespaceclass", p.GetXmlProperty("genxml/textbox/namespaceclass"));
                    p.SetXmlProperty("genxml/interfaces/genxml/textbox/assembly", p.GetXmlProperty("genxml/textbox/assembly"));
                }

                // check if record exists (should NOT) but lets replace if it does.
                var existingrecord = objCtrl.GetByGuidKey(portalId, -1, "PLUGIN", p.GUIDKey);
                if (existingrecord != null)
                {
                    p.ItemID = existingrecord.ItemID;
                }
                objCtrl.Update(p);
            }

        }

        public static void CopySystemPluginsToPortal()
        {
            var objCtrl = new NBrightBuyController();
            var rtnList = objCtrl.GetList(99999, -1, "PLUGIN", "", "order by nb1.xmldata.value('(genxml/hidden/index)[1]','float')");
            foreach (var p in rtnList)
            {
                var existingrecord = objCtrl.GetByGuidKey(PortalSettings.Current.PortalId, -1, "PLUGIN", p.GUIDKey);
                if (existingrecord == null)
                {
                    var filepath = HttpContext.Current.Server.MapPath(p.GetXmlProperty("genxml/textbox/path"));
                    if (File.Exists(filepath))
                    {
                        p.ItemID = -1;
                        p.PortalId = PortalSettings.Current.PortalId;
                        objCtrl.Update(p);
                    }
                    else
                    {
                        objCtrl.Delete(p.ItemID);
                    }
                }
            }
            PluginUtils.ResequenceRecords();
        }

        /// <summary>
        /// Search filesystem for any new plugins that have been added. Removed any deleted ones.
        /// </summary>
        public static void UpdateSystemPlugins()
        {
                // Add new plugins
                var updated = false;
                var pluginfoldermappath = System.Web.Hosting.HostingEnvironment.MapPath(StoreSettings.NBrightBuyPath() + "/Plugins");
            if (pluginfoldermappath != null && Directory.Exists(pluginfoldermappath))
            {
                var objCtrl = new NBrightBuyController();
                var flist = Directory.GetFiles(pluginfoldermappath,"*.xml");
                foreach (var f in flist)
                {
                    if (f.EndsWith(".xml"))
                    {
                        var datain = File.ReadAllText(f);
                        try
                        {
                            var nbi = new NBrightInfo();
                            nbi.XMLData = datain;
                            // check if we are injecting multiple
                            var nodlist = nbi.XMLDoc.SelectNodes("genxml");
                            if (nodlist != null && nodlist.Count > 0)
                            {
                                foreach (XmlNode nod in nodlist)
                                {
                                    var nbi2 = new NBrightInfo();
                                    nbi2.XMLData = nod.OuterXml;
                                    nbi2.ItemID = -1;
                                    nbi2.GUIDKey = nbi.GetXmlProperty("genxml/textbox/ctrl");
                                    nbi2.PortalId = 99999;
                                    nbi2.Lang = "";
                                    nbi2.ParentItemId = 0;
                                    nbi2.ModuleId = -1;
                                    nbi2.XrefItemId = 0;
                                    nbi2.UserId = 0;
                                    nbi2.TypeCode = "PLUGIN";

                                    // check if record exists (should NOT) but lets replace if it does.
                                    var existingrecord = objCtrl.GetByGuidKey(-1, -1, "PLUGIN", nbi2.GUIDKey);
                                    if (existingrecord != null)
                                    {
                                        nbi2.ItemID = existingrecord.ItemID;
                                        if (nbi2.GetXmlPropertyBool("genxml/delete"))
                                        {
                                            objCtrl.Delete(existingrecord.ItemID);
                                            File.Delete(f);
                                            ClearPluginCache(PortalSettings.Current.PortalId);
                                        }
                                        else
                                        {
                                            objCtrl.Update(nbi2);
                                            updated = true;
                                        }
                                    }
                                    else
                                    {
                                        objCtrl.Update(nbi2);
                                        updated = true;
                                    }
                                }
                            }
                            if (updated)
                            {
                                File.Delete(f);
                                //load entity typecode to DB idx settings.
                                NBrightBuyUtils.RegisterEnityTypeToDataBase();
                            }
                        }
                        catch (Exception)
                        {
                            // data might not be XML complient (ignore)
                        }
                    }
                }
            }

            if (updated)
            {
                CopySystemPluginsToPortal();
                ClearPluginCache(PortalSettings.Current.PortalId);
            }

        }

        public static void ClearPluginCache(int portalId)
        {
            var cachekey = "pluginlist" + portalId;
            NBrightBuyUtils.RemoveCache(cachekey);
        }

        public static void ResequenceRecords()
        {
            var objCtrl = new NBrightBuyController();
            var pdata = PluginUtils.GetPluginList();
            var lp = 0;
            foreach (var p in pdata)
            {
                p.SetXmlProperty("genxml/hidden/index", lp.ToString(), TypeCode.Double);
                objCtrl.Update(p);
                lp += 1;
            }

        }

        public static bool CheckSecurity(NBrightInfo pluginXml)
        {
            var currentuser = UserController.Instance.GetCurrentUserInfo();
            if (currentuser != null && pluginXml != null)
            {
                if (currentuser.IsInRole("Administrators"))
                {
                    return true;
                }
                if (pluginXml.GetXmlPropertyBool("genxml/checkbox/hidden")) return false;
                if (pluginXml.GetXmlPropertyBool("genxml/checkboxlist/securityroles/chk[@data='" + StoreSettings.ManagerRole + "']/@value") && currentuser.IsInRole(StoreSettings.ManagerRole))
                {
                    return true;
                }
                if (pluginXml.GetXmlPropertyBool("genxml/checkboxlist/securityroles/chk[@data='" + StoreSettings.EditorRole + "']/@value") && currentuser.IsInRole(StoreSettings.EditorRole))
                {
                    return true;
                }
                if (pluginXml.GetXmlPropertyBool("genxml/checkboxlist/securityroles/chk[@data='" + StoreSettings.ClientEditorRole + "']/@value") && currentuser.IsInRole(StoreSettings.ClientEditorRole))
                {
                    return true;
                }
            }
            return false;
        }

        public static Boolean CheckPluginSecurity(int portalId, String ctrl)
        {
            var currentuser = UserController.Instance.GetCurrentUserInfo();
            if (currentuser.IsSuperUser) return true;
            if (currentuser.IsInRole("Administrators")) return true;

            var pluginData = new PluginData(portalId);
            var p = pluginData.GetPluginByCtrl(ctrl);
            return CheckSecurity(p);
        }


    }
}