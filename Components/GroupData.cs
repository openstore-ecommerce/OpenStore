using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication.ExtendedProtection.Configuration;
using System.Text;
using System.Xml;
using DotNetNuke.Entities.Portals;
using NBrightCore.common;
using NBrightDNN;

namespace Nevoweb.DNN.NBrightBuy.Components
{
    public class GroupData
    {
        public NBrightInfo Info;
        public NBrightInfo DataRecord;
        public NBrightInfo DataLangRecord;

        private String _lang = ""; // needed for webservice

        /// <summary>
        /// Populate the DATA in this class
        /// </summary>
        /// <param name="groupId"> </param>
        /// <param name="lang"> </param>
        public GroupData(int groupId, String lang)
        {
            _lang = lang;
            if (_lang == "") _lang = StoreSettings.Current.EditLanguage;
            LoadData(groupId);
        }

        #region "public functions/interface"

        /// <summary>
        /// Set to true if product exists
        /// </summary>
        public bool Exists { get; private set; }

        public String Name
        {
            get
            {
                if (Exists) return Info.GetXmlProperty("genxml/lang/genxml/textbox/groupname");
                return "";
            }
            set {if (Exists) DataLangRecord.SetXmlProperty("genxml/textbox/groupname", value);}
        }

        public String Ref
        {
            get
            {
                if (Exists) return Info.GetXmlProperty("genxml/textbox/groupref");
                return "";
            }
            set 
            {
                if (Exists)
                {
                    DataRecord.GUIDKey = value;
                    DataRecord.SetXmlProperty("genxml/textbox/groupref", value);
                } 
            }
        }

        public String Type
        {
            get
            {
                if (Exists) return Info.GetXmlProperty("genxml/dropdownlist/grouptype");
                return "";
            }
            set
            {
                if (Exists)
                {
                    DataRecord.SetXmlProperty("genxml/dropdownlist/grouptype", value);
                }
            }
        }

        public string AddSearchBox
        {
            get
            {
                if (Exists) return Info.GetXmlPropertyBool("genxml/checkbox/addsearchbox").ToString();
                return false.ToString();
            }
            set
            {
                if (Exists)
                {
                    bool b;
                    if (bool.TryParse(value, out b))
                    {
                        DataRecord.SetXmlProperty("genxml/checkbox/addsearchbox", b.ToString());
                    }
                    else
                    {
                        DataRecord.SetXmlProperty("genxml/checkbox/addsearchbox", false.ToString());
                    }
                }
            }
        }


        public void Save()
        {
            var objCtrl = new NBrightBuyController();
            var groupId = objCtrl.Update(DataRecord);
            objCtrl.Update(DataLangRecord);
            Info = objCtrl.Get(groupId, "GROUPLANG", _lang);
        }

        public int Validate()
        {
            var errorcount = 0;
            var objCtrl = new NBrightBuyController();

            DataRecord.ValidateXmlFormat();
            if (DataLangRecord == null)
            {
                // we have no datalang record for this language, so get an existing one and save it.
                var l = objCtrl.GetList(PortalSettings.Current.PortalId, -1, "GROUPLANG", " and NB1.ParentItemId = " + Info.ItemID.ToString(""));
                if (l.Count > 0)
                {
                    DataLangRecord = (NBrightInfo)l[0].Clone();
                    DataLangRecord.ItemID = -1;
                    DataLangRecord.Lang = _lang;
                    DataLangRecord.SetXmlProperty("genxml/textbox/groupname", DnnUtils.GetResourceString("/DesktopModules/NBright/NBrightBuy/App_LocalResources/", "General." + DataRecord.GUIDKey));
                    DataLangRecord.ValidateXmlFormat();
                    objCtrl.Update(DataLangRecord);
                }
            }

            var defaultname = Name;
            if (defaultname == "")
            {
                // find a valid default name
                foreach (var lang in DnnUtils.GetCultureCodeList(PortalSettings.Current.PortalId))
                {
                    var l = objCtrl.GetList(PortalSettings.Current.PortalId, -1, "GROUPLANG", " and NB1.ParentItemId = " + Info.ItemID.ToString("") + " and NB1.Lang = '" + lang + "'");
                    if (l.Count == 1)
                    {
                        var nbi2 = (NBrightInfo) l[0];
                        if (nbi2.GetXmlProperty("genxml/textbox/groupname") != "")
                        {
                            defaultname = nbi2.GetXmlProperty("genxml/textbox/groupname");
                            Name = defaultname;
                            Save();
                            break;
                        }
                    }
                }
            }

            // fix langauge records
            foreach (var lang in DnnUtils.GetCultureCodeList(PortalSettings.Current.PortalId))
            {

                var l = objCtrl.GetList(PortalSettings.Current.PortalId, -1, "GROUPLANG", " and NB1.ParentItemId = " + Info.ItemID.ToString("") + " and NB1.Lang = '" + lang + "'");
                if (l.Count == 0 && DataLangRecord != null)
                {
                    var nbi = (NBrightInfo)DataLangRecord.Clone();
                    nbi.ItemID = -1;
                    nbi.Lang = lang;
                    if (nbi.GetXmlProperty("genxml/textbox/groupname") == "")
                    {
                        nbi.SetXmlProperty("genxml/textbox/groupname", DnnUtils.GetResourceString("/DesktopModules/NBright/NBrightBuy/App_LocalResources/", "General." + DataRecord.GUIDKey));
                    }
                    objCtrl.Update(nbi);
                    errorcount += 1;
                }

                if (l.Count == 1)
                {
                    var nbi2 = (NBrightInfo) l[0];
                    if (nbi2.GetXmlProperty("genxml/textbox/groupname") == "")
                    {
                        // if we have no name, use the default name we found early to update.
                        nbi2.SetXmlProperty("genxml/textbox/groupname", DnnUtils.GetResourceString("/DesktopModules/NBright/NBrightBuy/App_LocalResources/", "General." + DataRecord.GUIDKey));
                        objCtrl.Update(nbi2);
                    }
                }

                if (l.Count > 1)
                {
                    // we have more records than should exists, remove any old ones.
                    var l2 = objCtrl.GetList(PortalSettings.Current.PortalId, -1, "GROUPLANG", " and NB1.ParentItemId = " + Info.ItemID.ToString("") + " and NB1.Lang = '" + lang + "'", "order by Modifieddate desc");
                    var lp = 1;
                    foreach (var i in l2)
                    {
                        if (lp >= 2) objCtrl.Delete(i.ItemID);
                        lp += 1;
                    }
                }
            }

            if (Ref == "")
            {
                Ref = DataRecord.GUIDKey;
                objCtrl.Update(DataRecord);
            }

            // add required field values to make getting group easier.
            if (Ref != "" && DataRecord.GUIDKey != Ref)
            {
                DataRecord.GUIDKey = Ref;
                objCtrl.Update(DataRecord);
            }

            // check we have a groupt type, it might be missing if upgraded.
            if (Type == "")
            {
                if (Ref == "cat")
                {
                    Type = "2";
                }
                else
                {
                    Type = "1";
                }
                objCtrl.Update(DataRecord);
            }


            return errorcount;
        }


        #endregion



        #region " private functions"

        private void LoadData(int groupId)
        {
            Exists = false;
            if (groupId == -1) AddNew(); // add new record if -1 is used as id.
            var objCtrl = new NBrightBuyController();
            Info = objCtrl.Get(groupId, "GROUPLANG", _lang);
            if (Info != null)
            {
                Exists = true;
                DataRecord = objCtrl.GetData(groupId);
                DataLangRecord = objCtrl.GetDataLang(groupId, _lang);
            }
        }

        private void AddNew()
        {
            var nbi = new NBrightInfo(true);
            nbi.PortalId = PortalSettings.Current.PortalId; ;
            nbi.TypeCode = "GROUP";
            nbi.ModuleId = -1;
            nbi.ItemID = -1;
            var objCtrl = new NBrightBuyController();
            var itemId = objCtrl.Update(nbi);

            foreach (var lang in DnnUtils.GetCultureCodeList(PortalSettings.Current.PortalId))
            {
                nbi = new NBrightInfo(true);
                nbi.PortalId = PortalSettings.Current.PortalId;
                nbi.TypeCode = "GROUPLANG";
                nbi.ModuleId = -1;
                nbi.ItemID = -1;
                nbi.Lang = lang;
                nbi.ParentItemId = itemId;
                objCtrl.Update(nbi);
            }

            LoadData(itemId);

        }


        #endregion
    }
}
