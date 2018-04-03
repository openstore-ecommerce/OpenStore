using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using System.Xml;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.FileSystem;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;


namespace Nevoweb.DNN.NBrightBuy.Components
{
    public class UserData
    {
        public NBrightInfo Info;
        private UserInfo _userInfo;
        public Dictionary<string,NBrightInfo> DocList;
        private Dictionary<string, string> _fileKeyXref;


        public UserData()
        {
            PopulateData("");
        }

        public UserData(String userId)
        {
            PopulateData(userId);
        }

        public void AddNewPurchasedDoc(string key, string filename, string productId)
        {
            if (!_fileKeyXref.ContainsKey(filename))
            {
                var strXml = "<genxml><key>" + key + "</key><productid>" + productId + "</productid><filename>" + filename + "</filename></genxml>";
                var nbi = new NBrightInfo();
                nbi.GUIDKey = key;
                nbi.XMLData = strXml;
                DocList.Add(key, nbi);
                _fileKeyXref.Add(filename, nbi.GUIDKey);
            }
        }

        public void RemovePurchasedDoc(string key)
        {
            if (DocList != null && DocList.ContainsKey(key)) DocList.Remove(key);
        }
        public bool HasPurchasedDocByKey(string key)
        {
            if (DocList != null && DocList.ContainsKey(key)) return true;
            return false;
        }

        public bool HasPurchasedDocByFileName(string filename)
        {
            if (_fileKeyXref != null && _fileKeyXref.ContainsKey(filename)) return true;
            return false;
        }
        public NBrightInfo GetPurchasedInfoByKey(string key)
        {
            if (DocList != null && DocList.ContainsKey(key))
            {
                return DocList[key];
            }
            return new NBrightInfo();
        }
        public NBrightInfo GetPurchasedInfoByFileName(string filename)
        {
            if (_fileKeyXref != null && _fileKeyXref.ContainsKey(filename))
            {
                var key = _fileKeyXref[filename];
                return GetPurchasedInfoByKey(key);
            }
            return new NBrightInfo();
        }

        public string GetPurchasedKey(string filename)
        {
            if (_fileKeyXref != null && _fileKeyXref.ContainsKey(filename))
            {
                return _fileKeyXref[filename];
            }
            return "";
        }

        public string GetPurchasedFileName(string key)
        {
            if (DocList != null && DocList.ContainsKey(key))
            {
                return DocList[key].GetXmlProperty("genxml/filename");
            }
            return "";
        }


        /// <summary>
        /// Save cart
        /// </summary>
        public void Save(Boolean debugMode = false)
        {
            if (Info != null)
            {
                Info.RemoveXmlNode("genxml/docs");
                var strDocs = "<docs>";
                foreach (var d in DocList)
                {
                    var nbi = d.Value;
                    if (nbi.XMLData != "") strDocs += nbi.XMLData;
                }
                strDocs += "</docs>";
                Info.AddXmlNode(strDocs,"docs","genxml");

                var modCtrl = new NBrightBuyController();
                Info.ItemID = modCtrl.Update(Info);
                if (StoreSettings.Current.DebugModeFileOut) Info.XMLDoc.Save(PortalSettings.Current.HomeDirectoryMapPath + "debug_userdata.xml");
                Exists = true;
            }
        }

        public void DeleteUserData()
        {
            //remove DB record
            var modCtrl = new NBrightBuyController();
            modCtrl.Delete(Info.ItemID);
            Exists = false;
        }

        /// <summary>
        /// Get NBright UserData
        /// </summary>
        /// <returns></returns>
        public NBrightInfo GetUserData()
        {
            return Info;
        }

        /// <summary>
        /// Set to true if usedata exists
        /// </summary>
        public bool Exists { get; private set; }

        public void UpdateEmail(String email)
        {
            if (_userInfo != null && Utils.IsEmail(email))
            {
                _userInfo.Email = email;
                UserController.UpdateUser(PortalSettings.Current.PortalId, _userInfo);
            }
        }

        public String GetEmail()
        {
            if (_userInfo != null)
            {
                return _userInfo.Email;
            }
            return "";
        }


        private void PopulateData(String userId)
        {
            DocList = new Dictionary<string, NBrightInfo>();
            _fileKeyXref = new Dictionary<string, string>();

            Exists = false;
            if (Utils.IsNumeric(userId))
                _userInfo = UserController.GetUserById(PortalSettings.Current.PortalId, Convert.ToInt32(userId));
            else
                _userInfo = UserController.Instance.GetCurrentUserInfo();

            if (_userInfo != null && _userInfo.UserID != -1) // only create userdata if we have a user logged in.
            {
                var modCtrl = new NBrightBuyController();
                Info = modCtrl.GetByType(_userInfo.PortalID, -1, "USERDATA", _userInfo.UserID.ToString(""));
                if (Info == null && _userInfo.UserID != -1)
                {
                    Info = new NBrightInfo();
                    Info.ItemID = -1;
                    Info.UserId = _userInfo.UserID;
                    Info.PortalId = _userInfo.PortalID;
                    Info.ModuleId = -1;
                    Info.TypeCode = "USERDATA";
                    Info.XMLData = "<genxml></genxml>";
                    Save();
                }
                else
                    Exists = true;

                var nodlist = Info.XMLDoc.SelectNodes("genxml/docs/*");
                if (nodlist != null)
                {
                    foreach (XmlNode nod in nodlist)
                    {
                        if (nod.SelectSingleNode("filename") != null && !_fileKeyXref.ContainsKey(nod.SelectSingleNode("filename").InnerXml))
                        {
                            var nbi = new NBrightInfo();
                            nbi.XMLData = nod.OuterXml;
                            nbi.GUIDKey = nbi.GetXmlProperty("genxml/key");
                            DocList.Add(nbi.GetXmlProperty("genxml/key"), nbi);
                            _fileKeyXref.Add(nbi.GetXmlProperty("genxml/filename"), nbi.GUIDKey);
                        }
                    }
                }

            }
        }

    }
}
