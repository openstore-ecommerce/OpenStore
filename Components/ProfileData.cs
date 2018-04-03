using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using System.Xml;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.FileSystem;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;

namespace Nevoweb.DNN.NBrightBuy.Components
{
    public class ProfileData
    {
        private UserData _uData;

        public ProfileData(int userid,Repeater rpData, Boolean debugMode = false)
        {
            Exists = true;
            PopulateData(userid.ToString(""));
            UpdateProfile(rpData, debugMode);
        }

        public ProfileData()
        {
            PopulateData("");
        }

        public ProfileData(String userId)
        {
            PopulateData(userId);
        }

        #region "base methods"


        public void UpdateProfile(Repeater rpData, Boolean debugMode = false)
        {
            const string profileupload = "NBStore\\profileupload";
            Utils.CreateFolder(PortalSettings.Current.HomeDirectoryMapPath + profileupload);
            var strXml = GenXmlFunctions.GetGenXml(rpData, "", PortalSettings.Current.HomeDirectoryMapPath + profileupload);
            Save(strXml, debugMode);
        }

        public void UpdateProfileAjax(String profileXml, Boolean debugMode = false)
        {
            Save(profileXml, debugMode);
        }

        public NBrightInfo GetProfile()
        {
            var pInfo = new NBrightInfo(true);
            if (_uData.Exists)
            {
                var xmlNode = _uData.Info.XMLDoc.SelectSingleNode("genxml/profile/genxml");
                if (xmlNode != null)
                {
                    pInfo.PortalId = _uData.Info.PortalId;
                    pInfo.Lang = _uData.Info.Lang;
                    pInfo.XMLData = xmlNode.OuterXml;
                }
            }
            return pInfo;
        }

        /// <summary>
        /// Set to true if cart exists
        /// </summary>
        public bool Exists { get; private set; }

        #endregion

        #region "private functions"

        /// <summary>
        /// Save to USERDATA and DNN profile
        /// </summary>
        private void Save(String profileXml, Boolean debugMode = false)
        {
            if (_uData.Exists)
            {
                //save cart
                var strXml = "<profile>";
                strXml += profileXml;
                strXml += "</profile>";
                _uData.Info.RemoveXmlNode("genxml/profile");
                _uData.Info.AddXmlNode(strXml, "profile", "genxml");
                _uData.Save(debugMode);

                var pInfo = GetProfile();
                UpdateDnnProfile(pInfo);

                Exists = true;
            }
        }

        /// <summary>
        /// Save Data to DNN profile
        /// </summary>
        /// <param name="profile"></param>
        private void UpdateDnnProfile(NBrightInfo profile)
        {
            var flag = false;
            var prop1 = DnnUtils.GetUserProfileProperties(_uData.Info.UserId.ToString(""));
            var prop2 = DnnUtils.GetUserProfileProperties(_uData.Info.UserId.ToString(""));
            foreach (var p in prop1)
            {
                var n = profile.XMLDoc.SelectSingleNode("genxml/textbox/" + p.Key.ToLower());
                if (n != null)
                {
                    prop2[p.Key] = n.InnerText;
                    flag = true;
                }
                n = profile.XMLDoc.SelectSingleNode("genxml/dropdownlist/" + p.Key.ToLower());
                if (n != null)
                {
                    prop2[p.Key] = n.InnerText;
                    flag = true;
                }
                n = profile.XMLDoc.SelectSingleNode("genxml/radiobuttonlist/" + p.Key.ToLower());
                if (n != null)
                {
                    prop2[p.Key] = n.InnerText;
                    flag = true;
                }
            }
            if (flag) DnnUtils.SetUserProfileProperties(_uData.Info.UserId.ToString(""), prop2);

            // update email
            var email = profile.GetXmlProperty("genxml/textbox/email");
            if (email != "" && email != _uData.GetEmail()) _uData.UpdateEmail(email);

        }

        /// <summary>
        /// Get data from DNN profile
        /// </summary>
        /// <param name="userId"></param>
        private void PopulateData(String userId)
        {
            Exists = false;
            _uData = new UserData(userId);
            //Get DNN profile
            if (_uData.Exists)
            {
                var newDefault = GetProfile();
                var prop = DnnUtils.GetUserProfileProperties(_uData.Info.UserId.ToString(""));
                foreach (var p in prop)
                {
                    newDefault.SetXmlProperty("genxml/textbox/" + p.Key.ToLower(), p.Value);
                }
                // get email
                newDefault.SetXmlProperty("genxml/textbox/email", _uData.GetEmail());
                Save(newDefault.XMLData);
            }
        }

        #endregion


    }
}
