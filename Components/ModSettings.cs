using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using DotNetNuke.Entities.Portals;
using NBrightDNN;

namespace Nevoweb.DNN.NBrightBuy.Components
{
    public class ModSettings
    {
        private Dictionary<string, string> _settingsDic;

        #region Constructors

        public ModSettings(int moduleid, System.Collections.Hashtable modSettings)
        {
            Moduleid = moduleid;
            BuildSettingsDic(modSettings);
            ThemeFolder = Get("themefolder");
        }

        #endregion

        #region "properties"

        /// <summary>
        /// Return setting using key value.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string Get(string key)
        {
            var value = _settingsDic.ContainsKey(key) ? _settingsDic[key] : "";
            if (value == "") return StoreSettings.Current.Get(key);
            return value;
        }

        public void Set(string key,string value)
        {
            if (!_settingsDic.ContainsKey(key))
                _settingsDic.Add(key, value);
            else
                _settingsDic[key] = value;
        }

        public Dictionary<string, string> Settings()
        {
            return _settingsDic;
        }

        public int Moduleid { get; private set; }
        public String ThemeFolder { get; private set; }

        #endregion

        #region "private methods"


        private void BuildSettingsDic(System.Collections.Hashtable modSettings)
        {
            var nbbSettings = NBrightBuyUtils.GetSettings(PortalSettings.Current.PortalId, Moduleid);

            _settingsDic = new Dictionary<string, string>();
            // this function will build the Settings that is passed to the templating system.
            var strCacheKey = PortalSettings.Current.PortalId.ToString("") + "*" + Moduleid.ToString("") + "*SettingsDic";
            var obj = NBrightBuyUtils.GetModCache(strCacheKey);
            if (obj != null) _settingsDic = (Dictionary<string, string>)obj;
            if (_settingsDic.Count == 0 || StoreSettings.Current.DebugMode)
            {

                if (!_settingsDic.ContainsKey("tabid")) _settingsDic.Add("tabid", PortalSettings.Current.ActiveTab.TabID.ToString(""));

                // add store settings (we keep store settings at module level, because these can be overwritten by the module)
                var storesettings = StoreSettings.Current.Settings(); // assign to var, so it doesn;t causes error if site settings change during loop.
                foreach (var item in storesettings)
                {
                    if (_settingsDic.ContainsKey(item.Key))
                        _settingsDic[item.Key] = item.Value;
                    else
                        _settingsDic.Add(item.Key,item.Value);                        
                }


                // add normal DNN Setting
                foreach (string name in modSettings.Keys)
                {
                    if (!_settingsDic.ContainsKey(name)) _settingsDic.Add(name, modSettings[name].ToString());
                }

                // add nbbSettings Settings 
                AddToSettingDic(nbbSettings, "genxml/hidden/*");
                AddToSettingDic(nbbSettings, "genxml/textbox/*");
                AddToSettingDic(nbbSettings, "genxml/checkbox/*");
                AddToSettingDic(nbbSettings, "genxml/dropdownlist/*");
                AddToSettingDic(nbbSettings, "genxml/radiobuttonlist/*");


                // redo the moduleid key, on imported module settings this could be wrong.
                if (_settingsDic.ContainsKey("moduleid"))
                    _settingsDic["moduleid"] = Moduleid.ToString("");
                else
                    _settingsDic.Add("moduleid", Moduleid.ToString(""));

                NBrightBuyUtils.SetModCache(Moduleid, strCacheKey, _settingsDic);
            }
            else
            {
                _settingsDic = (Dictionary<string, string>)obj;
            }

            // redo the edit langauge for backoffice.
            if (_settingsDic != null)
            {
                if (_settingsDic.ContainsKey("editlanguage"))
                    _settingsDic["editlanguage"] = StoreSettings.Current.EditLanguage;
                else
                    _settingsDic.Add("editlanguage", StoreSettings.Current.EditLanguage);                
            }


        }

        private void AddToSettingDic(NBrightInfo settings, string xpath)
        {
            if (settings.XMLDoc != null)
            {
                var nods = settings.XMLDoc.SelectNodes(xpath);
                if (nods != null)
                {
                    foreach (XmlNode nod in nods)
                    {
                        if (_settingsDic.ContainsKey(nod.Name))
                        {
                            _settingsDic[nod.Name] = nod.InnerText; // overwrite same name node
                        }
                        else
                        {
                            _settingsDic.Add(nod.Name, nod.InnerText);
                        }
                    }
                }
            }
        }


        #endregion
    }
}
