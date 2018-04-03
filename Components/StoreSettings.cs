using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;
using DotNetNuke.Entities.Content.Common;
using DotNetNuke.Entities.Portals;
using NBrightCore.common;
using NBrightDNN;

namespace Nevoweb.DNN.NBrightBuy.Components
{
    public class StoreSettings
    {
        private readonly Dictionary<string, string> _settingDic;

        public const String ManagerRole = "Manager";
        public const String EditorRole = "Editor";
        public const String ClientEditorRole = "ClientEditor";
        public String DealerRole { get; private set; }

        #region Constructors
        public StoreSettings(int portalId)
        {
            DebugMode = false;
            DebugModeFileOut = false;

            _settingDic = new Dictionary<string, string>();

            //Get NBrightBuy Portal Settings.
            var modCtrl = new NBrightBuyController();
            SettingsInfo = modCtrl.GetByGuidKey(portalId, -1, "SETTINGS", "NBrightBuySettings");
            if (SettingsInfo != null)
            {
                AddToSettingDic(SettingsInfo, "genxml/hidden/*");
                AddToSettingDic(SettingsInfo, "genxml/textbox/*");
                AddToSettingDic(SettingsInfo, "genxml/checkbox/*");
                AddToSettingDic(SettingsInfo, "genxml/dropdownlist/*");
                AddToSettingDic(SettingsInfo, "genxml/radiobuttonlist/*");
                AddToSettingDicSelectedTextAttr(SettingsInfo, "genxml/dropdownlist/*");
                AddToSettingDicSelectedTextAttr(SettingsInfo, "genxml/radiobuttonlist/*");
            }

            //add DNN Portalsettings
            if (!_settingDic.ContainsKey("portalid")) _settingDic.Add("portalid", portalId.ToString(""));
            if (PortalSettings.Current == null)
            {
                var portalsettings = NBrightDNN.DnnUtils.GetPortalSettings(portalId);
                if (!_settingDic.ContainsKey("portalname")) _settingDic.Add("portalname", portalsettings.PortalName);
                if (!_settingDic.ContainsKey("homedirectory")) _settingDic.Add("homedirectory", portalsettings.HomeDirectory);
                if (!_settingDic.ContainsKey("homedirectorymappath")) _settingDic.Add("homedirectorymappath", portalsettings.HomeDirectoryMapPath);                
            }
            else
            {
                if (!_settingDic.ContainsKey("portalname")) _settingDic.Add("portalname", PortalSettings.Current.PortalName);
                if (!_settingDic.ContainsKey("homedirectory")) _settingDic.Add("homedirectory", PortalSettings.Current.HomeDirectory);
                if (!_settingDic.ContainsKey("homedirectorymappath")) _settingDic.Add("homedirectorymappath", PortalSettings.Current.HomeDirectoryMapPath);                
            }
            if (!_settingDic.ContainsKey("culturecode")) _settingDic.Add("culturecode", Utils.GetCurrentCulture());


            ThemeFolder = Get("themefolder");

            if (_settingDic.ContainsKey("debug.mode") && _settingDic["debug.mode"] == "True") DebugMode = true;  // set debug mmode
            if (_settingDic.ContainsKey("debugfileout") && _settingDic["debugfileout"] == "True") DebugModeFileOut = true;  // set debug mmode
            if (_settingDic.ContainsKey("enablefilelogging") && _settingDic["enablefilelogging"] == "True") EnableFileLogging = true;  // set File Logging
            StorageTypeClient = DataStorageType.Cookie;
            
            AdminEmail = Get("adminemail");
            ManagerEmail = Get("manageremail");
            FolderDocumentsMapPath = Get("homedirectorymappath").TrimEnd('\\') + "\\" + Get("folderdocs");
            FolderImagesMapPath = Get("homedirectorymappath").TrimEnd('\\') + "\\" + Get("folderimages");
            FolderUploadsMapPath = Get("homedirectorymappath").TrimEnd('\\') + "\\" + Get("folderuploads");
            FolderClientUploadsMapPath = Get("homedirectorymappath").TrimEnd('\\') + "\\" + Get("folderclientuploads");
            FolderTempMapPath = Get("homedirectorymappath").TrimEnd('\\') + "\\NBSTemp";
            FolderNBStoreMapPath = Get("homedirectorymappath").TrimEnd('\\') + "\\NBStore";

            FolderDocuments = Get("homedirectory").TrimEnd('/') + "/" + Get("folderdocs").Replace("\\", "/");
            FolderImages =  Get("homedirectory").TrimEnd('/') + "/" + Get("folderimages").Replace("\\", "/");
            FolderUploads = Get("homedirectory").TrimEnd('/') + "/" + Get("folderuploads").Replace("\\", "/");
            FolderClientUploads = Get("homedirectory").TrimEnd('/') + "/" + Get("folderclientuploads").Replace("\\", "/");
            FolderTemp = Get("homedirectory").TrimEnd('/') + "/NBSTemp";
            FolderNBStore = Get("homedirectory").TrimEnd('/') + "/NBStore";

            if (!_settingDic.ContainsKey("FolderDocumentsMapPath")) _settingDic.Add("FolderDocumentsMapPath",FolderDocumentsMapPath );
            if (!_settingDic.ContainsKey("FolderImagesMapPath")) _settingDic.Add("FolderImagesMapPath",FolderImagesMapPath );
            if (!_settingDic.ContainsKey("FolderUploadsMapPath")) _settingDic.Add("FolderUploadsMapPath",FolderUploadsMapPath );
            if (!_settingDic.ContainsKey("FolderDocuments")) _settingDic.Add("FolderDocuments", FolderDocuments);
            if (!_settingDic.ContainsKey("FolderImages")) _settingDic.Add("FolderImages",FolderImages );
            if (!_settingDic.ContainsKey("FolderUploads")) _settingDic.Add("FolderUploads", FolderUploads);

            if (!_settingDic.ContainsKey("NBrightBuyPath")) _settingDic.Add("NBrightBuyPath", NBrightBuyPath());

            DealerRole = Get("dealerrole");
            if (DealerRole == "") DealerRole = "Dealer";
        }

        #endregion

        /// <summary>
        /// get relitive patyh of NBrightBuy Module (NBS is NOT compatiblity with DNN running in a virtual directory)
        /// </summary>
        /// <returns></returns>
        public static String NBrightBuyPath()
        {
            //if (HttpContext.Current.Request.ApplicationPath != null) return HttpContext.Current.Request.ApplicationPath.TrimEnd('/') + "/DesktopModules/NBright/NBrightBuy";
            return "/DesktopModules/NBright/NBrightBuy";
        }

        public static StoreSettings Current
        {
            get { return NBrightBuyController.GetCurrentPortalData(); }
        }

        public static void Refresh()
        {
            HttpContext.Current.Items.Remove("NBBStoreSettings" + PortalSettings.Current.PortalId.ToString(""));
            Utils.RemoveCache("NBBStoreSettings" + PortalSettings.Current.PortalId.ToString(""));
            DnnUtils.ClearPortalCache(PortalSettings.Current.PortalId);
        }


        public Dictionary<string, string> Settings()
        {
            // redo the edit langauge for backoffice.
            if (_settingDic != null)
            {
                if (_settingDic.ContainsKey("editlanguage"))
                    _settingDic["editlanguage"] = EditLanguage;
                else
                    _settingDic.Add("editlanguage", EditLanguage);
            }
            return _settingDic;
        }

        public String EditLanguage
        {
            get
            {
                var editlang = "";
                // need to test if HttpContext.Current is null, because webservice calling storesettings will raise exception. 
                if (HttpContext.Current != null && HttpContext.Current.Session != null && HttpContext.Current.Session["NBrightBuy_EditLanguage"] != null) editlang = (String)HttpContext.Current.Session["NBrightBuy_EditLanguage"];
                if (editlang == "")
                {
                    // no session, when call from webservice, so take setting dictionary if there.
                    if (_settingDic.ContainsKey("editlanguage"))
                    {
                        return _settingDic["editlanguage"];
                    }
                }
                if (editlang == "") return Utils.GetCurrentCulture();
                return editlang;
            }
            set
            {
                // need to test if HttpContext.Current is null, because webservice calling storesettings will raise exception. 
                if (HttpContext.Current != null && HttpContext.Current.Session != null)
                {
                    HttpContext.Current.Session["NBrightBuy_EditLanguage"] = value;
                }


            }
        }


        #region "properties"

        /// <summary>
        /// Return setting using key value.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string Get(String key)
        {
            return _settingDic.ContainsKey(key) ? _settingDic[key] : "";
        }

        public int GetInt(String key)
        {
            if (_settingDic.ContainsKey(key))
            {
                if (Utils.IsNumeric(_settingDic[key]))
                {
                    return Convert.ToInt32(_settingDic[key]);
                }
            }
            return 0;
        }

        public Boolean GetBool(String key)
        {
            if (_settingDic.ContainsKey(key))
            {
                if (_settingDic[key] == "True") return true;
            }
            return false;
        }

        //get properties
        public int CartTabId
        {
            get
            {
                var i = Get("carttab");
                if (Utils.IsNumeric(i)) return Convert.ToInt32(i);
                return PortalSettings.Current.ActiveTab.TabID;
            }
        }

        public int PaymentTabId
        {
            get
            {
                var i = Get("paymenttab");
                if (Utils.IsNumeric(i)) return Convert.ToInt32(i);
                return PortalSettings.Current.ActiveTab.TabID;
            }
        }
        public int CheckoutTabId
        {
            get
            {
                var i = Get("checkouttab");
                if (Utils.IsNumeric(i)) return Convert.ToInt32(i);
                return PortalSettings.Current.ActiveTab.TabID;
            }
        }

        public int ProductListTabId
        {
            get
            {
                var i = Get("productlisttab");
                if (Utils.IsNumeric(i)) return Convert.ToInt32(i);
                return PortalSettings.Current.ActiveTab.TabID;
            }
        }
        public int ProductDetailTabId
        {
            get
            {
                var i = Get("ddldetailtabid");
                if (Utils.IsNumeric(i)) return Convert.ToInt32(i);
                return PortalSettings.Current.ActiveTab.TabID;
            }
        }

        // this section contain a set of properties that are assign commanly used setting.

        public bool DebugMode { get; private set; }
        public bool DebugModeFileOut { get; private set; }
        public bool EnableFileLogging { get; private set; }
        /// <summary>
        /// Get Client StorageType type Cookie,SessionMemory
        /// </summary>
        public DataStorageType StorageTypeClient { get; private set; }
        public String AdminEmail { get; private set; }
        public String ManagerEmail { get; private set; }
        public NBrightInfo SettingsInfo { get; private set; }
        public String ThemeFolder { get; private set; }

        public String FolderImagesMapPath { get; private set; }
        public String FolderDocumentsMapPath { get; private set; }
        public String FolderUploadsMapPath { get; private set; }
        public String FolderClientUploadsMapPath { get; private set; }
        public String FolderImages { get; private set; }
        public String FolderDocuments { get; private set; }
        public String FolderUploads { get; private set; }
        public String FolderClientUploads { get; private set; }
        public String FolderTemp { get; private set; }
        public String FolderTempMapPath { get; private set; }
        public String FolderNBStore { get; private set; }
        public String FolderNBStoreMapPath { get; private set; }

        #endregion

        private void AddToSettingDic(NBrightInfo settings, string xpath)
        {
            if (settings.XMLDoc != null)
            {
                var nods = settings.XMLDoc.SelectNodes(xpath);
                if (nods != null)
                {
                    foreach (XmlNode nod in nods)
                    {
                        if (_settingDic.ContainsKey(nod.Name))
                        {
                            _settingDic[nod.Name] = nod.InnerText; // overwrite same name node
                        }
                        else
                        {
                            _settingDic.Add(nod.Name, nod.InnerText);
                        }
                    }
                }
            }
        }

        private void AddToSettingDicSelectedTextAttr(NBrightInfo settings, string xpath)
        {
            if (settings.XMLDoc != null)
            {
                var nods = settings.XMLDoc.SelectNodes(xpath);
                if (nods != null)
                {
                    foreach (XmlNode nod in nods)
                    {
                        if (_settingDic.ContainsKey(nod.Name + "text"))
                        {
                            if (nod.Attributes != null) _settingDic[nod.Name + "text"] = nod.Attributes["selectedtext"].InnerText;
                        }
                        else
                        {
                            if (nod.Attributes != null) _settingDic.Add(nod.Name + "text", nod.Attributes["selectedtext"].InnerText);
                        }
                    }
                }
            }
        }

    }
}
