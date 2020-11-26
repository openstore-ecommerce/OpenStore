using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Web;
using System.Web.Handlers;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using System.Xml;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.UI.UserControls;
using DotNetNuke.UI.WebControls;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Components.Interfaces;

namespace Nevoweb.DNN.NBrightBuy.Components
{
    public class PluginRecord
    {
        private NBrightInfo _pluginInfo;
        public PluginRecord(NBrightInfo pluginInfo)
        {
            _pluginInfo = pluginInfo;
        }

        public NBrightInfo Info()
        {
            return _pluginInfo;
        }

        public List<NBrightInfo> GetInterfaces()
        {
            var rtnList = new List<NBrightInfo>();

            var interfaces = _pluginInfo.XMLDoc.SelectNodes("genxml/interfaces/*");
            foreach (XmlNode i in interfaces)
            {
                var nbi = new NBrightInfo();
                nbi.XMLData = i.OuterXml;
                nbi.TypeCode = "PLUGININTERFACE";
                rtnList.Add(nbi);
            }
            return rtnList;
        }

        public void AddInterface()
        {
            var objCtrl = new NBrightBuyController();
            _pluginInfo.AddXmlNode("<genxml></genxml>", "genxml","genxml/interfaces");
            objCtrl.Update(_pluginInfo);
        }

        public void UpdateModels(String xmlAjaxData, string editlang)
        {
            var rtnstatuscode = "";
            var modelList = NBrightBuyUtils.GetGenXmlListByAjax(xmlAjaxData, "", editlang);

            var basefields = "";

            // build xml for data records
            var strXml = "<genxml><interfaces>";
            foreach (var modelInfo in modelList)
            {

                // build list of xpath fields that need processing.
                var filedList = NBrightBuyUtils.GetAllFieldxPaths(modelInfo);
                foreach (var xpath in filedList)
                {
                        basefields += xpath + ",";
                }

                var objInfo = new NBrightInfo(true);

                var fields = basefields.Split(',');
                foreach (var f in fields.Where(f => f != ""))
                {
                    var datatype = modelInfo.GetXmlProperty(f + "/@datatype");
                    if (datatype == "date")
                        objInfo.SetXmlProperty(f, modelInfo.GetXmlProperty(f), TypeCode.DateTime);
                    else if (datatype == "double")
                        objInfo.SetXmlPropertyDouble(f, modelInfo.GetXmlProperty(f));
                    else if (datatype == "html")
                        objInfo.SetXmlProperty(f, modelInfo.GetXmlPropertyRaw(f));
                    else
                        objInfo.SetXmlProperty(f, modelInfo.GetXmlProperty(f));
                }
                strXml += objInfo.XMLData;
            }
            strXml += "</interfaces></genxml>";

            // replace models xml 
            Info().ReplaceXmlNode(strXml, "genxml/interfaces", "genxml");

        }

    }

    public class PluginData
    {
        private List<NBrightInfo> _pluginList;
        private String _cachekey;

        public PluginData(int portalId, bool usecache = true)
        {
            _cachekey = "pluginlist" + portalId;
            if (usecache)
            {
                var pList = NBrightBuyUtils.GetCache(_cachekey);
                if (pList != null)
                {
                    _pluginList = (List<NBrightInfo>)pList;
                }
                // if we have zero plugins, try and reload 
                if (pList == null || !_pluginList.Any())
                {
                    _pluginList = PluginUtils.GetPluginList(portalId);
                    NBrightBuyUtils.SetCache(_cachekey, _pluginList);
                }
            }
            else
            {
                _pluginList = PluginUtils.GetPluginList(portalId);
            }

        }

        #region "base methods"

        public NBrightInfo GetShippingProviderDefault()
        {
            var l = GetShippingProviders();
            if (l.Count > 0) return l.First().Value;
            return null;
        }

        public Dictionary<String,NBrightInfo> GetShippingProviders(Boolean activeOnly = true)
        {
            return GetProviders("02", activeOnly);
        }

        public Dictionary<String, NBrightInfo> GetTaxProviders(Boolean activeOnly = true)
        {
            return GetProviders("03", activeOnly);
        }

        public Dictionary<String, NBrightInfo> GetPromoProviders(Boolean activeOnly = true)
        {
            return GetProviders("04", activeOnly);
        }
        public Dictionary<String, NBrightInfo> GetSchedulerProviders(Boolean activeOnly = true)
        {
            return GetProviders("05", activeOnly);
        }
        public Dictionary<String, NBrightInfo> GetEventsProviders(Boolean activeOnly = true)
        {
            return GetProviders("06", activeOnly);
        }

        public Dictionary<String, NBrightInfo> GetPaymentProviders(Boolean activeOnly = true)
        {
            return GetProviders("07", activeOnly);
        }

        public Dictionary<String, NBrightInfo> GetDiscountCodeProviders(Boolean activeOnly = true)
        {
            return GetProviders("08", activeOnly);
        }

        public Dictionary<String, NBrightInfo> GetFilterProviders(Boolean activeOnly = true)
        {
            return GetProviders("09", activeOnly);
        }

        public Dictionary<String, NBrightInfo> GetTemplateExtProviders(Boolean activeOnly = true)
        {
            return GetProviders("10", activeOnly);
        }

        public Dictionary<String, NBrightInfo> GetEntityTypeProviders(Boolean activeOnly = true)
        {
            return GetProviders("11", activeOnly);
        }

        public Dictionary<String, NBrightInfo> GetAjaxProviders(Boolean activeOnly = true)
        {
            return GetProviders("12", activeOnly);
        }

        public Dictionary<String, NBrightInfo> GetImageProviders(Boolean activeOnly = true)
        {
            return GetProviders("14", activeOnly);
        }


        public Dictionary<String, NBrightInfo> GetOtherProviders(Boolean activeOnly = true)
        {
            return GetProviders("99", activeOnly);
        }


        private Dictionary<String, NBrightInfo> GetProviders(String providerType, Boolean activeOnly = true)
        {
            var pList = new Dictionary<String, NBrightInfo>();
            foreach (var p in _pluginList)
            {
                var pr = new PluginRecord(p);
                foreach (var i in pr.GetInterfaces())
                {
                    if (i.GetXmlProperty("genxml/dropdownlist/providertype") == providerType && (i.GetXmlProperty("genxml/checkbox/active") == "True" || !activeOnly))
                    {
                        var ctrlkey = pr.Info().GetXmlProperty("genxml/textbox/ctrl");
                        var lp = 1;
                        while (pList.ContainsKey(ctrlkey))
                        {
                            ctrlkey = p.GetXmlProperty("genxml/textbox/assembly") + lp.ToString("");
                            lp += 1;
                        }
                        i.SetXmlProperty("genxml/textbox/ctrl",ctrlkey); // add key for multiple interfacxes have ctrlkey data
                        if (!pList.ContainsKey(ctrlkey)) pList.Add(ctrlkey, i);
                    }
                }
            }
            return pList;
        }


        public NBrightInfo GetPaymentProviderDefault()
        {
            var l = GetPaymentProviders();
            if (l.Count > 0) return l.First().Value;
            return null;
        }



        public List<NBrightInfo> GetSubList(String groupname)
        {
            var rtnList = new List<NBrightInfo>();
            if (groupname != "")
            {
                foreach (var p in _pluginList)
                {
                    if (p.GetXmlProperty("genxml/textbox/group") == groupname)
                    {
                        rtnList.Add(p);
                    }
                }
            }
            return rtnList;
        }

        public NBrightInfo GetPluginByCtrl(String ctrlname)
        {
            var ctrllist = from i in _pluginList where i.GetXmlProperty("genxml/textbox/ctrl") == ctrlname select i;
            if (ctrllist.Any()) return ctrllist.First(); 
            return null;
        }

        #endregion



    }
}
