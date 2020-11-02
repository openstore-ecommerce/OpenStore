using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using System.Xml;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Membership;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Mail;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;


namespace Nevoweb.DNN.NBrightBuy.Components
{
    public class ClientData
    {
        private NBrightInfo _clientInfo;
        public NBrightInfo DataRecord;
        public int PortalId;
        private UserInfo _userInfo;
        private readonly string _cacheKey;

        public Boolean Exists;

        public List<NBrightInfo> DiscountCodes;
        public List<NBrightInfo> VoucherCodes;

        public ClientData(int portalId, int userid) : this(portalId, userid, false) { }

        public ClientData(int portalId, int userid, bool debugmode)
        {
            _cacheKey = "ClientData" + userid + "_" + portalId;
            Exists = false;
            PortalId = portalId;
            PopulateClientData(userid, debugmode);
        }


        #region "base methods"


        /// <summary>
        /// Get Client Cart
        /// </summary>
        /// <returns></returns>
        public NBrightInfo GetInfo()
        {
            return _clientInfo;
        }

        public void AddClientEditorRole()
        {
            if (_userInfo != null)
            {
                if (!_userInfo.IsInRole(StoreSettings.ClientEditorRole))
                {
                    var rc = new DotNetNuke.Security.Roles.RoleController();
                    var ri = rc.GetRoleByName(PortalId, StoreSettings.ClientEditorRole);
                    if (ri != null) rc.AddUserRole(PortalId, _userInfo.UserID, ri.RoleID, Null.NullDate, Null.NullDate);
                    if (StoreSettings.Current.Get("sendclientroleemail") == "True")
                    {
                        var emailBody = "";
                        emailBody = NBrightBuyUtils.RazorTemplRender("AddClientRole.cshtml", 0, "", _clientInfo, "/DesktopModules/NBright/NBrightBuy", StoreSettings.Current.Get("themefolder"), _userInfo.Profile.PreferredLocale, StoreSettings.Current.Settings());
                        NBrightBuyUtils.SendEmail(emailBody, _userInfo.Email, "ClientRole", _clientInfo, StoreSettings.Current.SettingsInfo.GetXmlProperty("genxml/textbox/storecompany"), StoreSettings.Current.ManagerEmail, _userInfo.Profile.PreferredLocale);
                    }
                }
            }
        }

        public void RemoveClientEditorRole()
        {
            if (_userInfo != null)
            {
                if (_userInfo.IsInRole(StoreSettings.ClientEditorRole))
                {
                    var rc = new DotNetNuke.Security.Roles.RoleController();
                    var ri = rc.GetRoleByName(PortalId, StoreSettings.ClientEditorRole);
                    var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
                    if (ri != null) RoleController.DeleteUserRole(_userInfo, ri, portalSettings, false);
                }
            }
        }

        public void ResetPassword()
        {
            if (_userInfo != null)
            {
                _userInfo.PasswordResetExpiration = DateTime.Now.AddMinutes(1200);
                _userInfo.PasswordResetToken = Guid.NewGuid();
                _userInfo.Membership.UpdatePassword = true;
                UserController.UpdateUser(_userInfo.PortalID, _userInfo);
                var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
                Mail.SendMail(_userInfo, MessageType.PasswordReminder, portalSettings);               
            }
        }

        public void UpdateEmail(String email)
        {
            // update email
            if (_userInfo != null && Utils.IsEmail(email) && (_userInfo.Email != email))
            {
                _userInfo.Email = email;
                UserController.UpdateUser(PortalSettings.Current.PortalId, _userInfo);
            }
        }

        public void Update(NBrightInfo updateInfo)
        {
            // update email
            var email = updateInfo.GetXmlProperty("genxml/textbox/email");
            if (_userInfo != null && Utils.IsEmail(email) && (_userInfo.Email != email))
            {
                _userInfo.Email = email;
                UserController.UpdateUser(PortalSettings.Current.PortalId, _userInfo);
            }

            // ClientEditorRole
            var clientEditorRole = updateInfo.GetXmlProperty("genxml/checkbox/clienteditorrole");
            if (clientEditorRole == "True")
            {
                AddClientEditorRole();
            }
            else
            {
                RemoveClientEditorRole();
            }

            // save client data fields.
            var objCtrl = new NBrightBuyController();
            DataRecord = objCtrl.GetByType(PortalId, -1, "CLIENT", _userInfo.UserID.ToString(""));
            DataRecord.XMLData = updateInfo.XMLData;
            DataRecord.RemoveXmlNode("genxml/hidden/xmlupdatediscountcodedata");
            DataRecord.RemoveXmlNode("genxml/hidden/xmlupdatevouchercodedata");
            objCtrl.Update(DataRecord);

            // update Discount codes
            var strXml = Utils.UnCode(updateInfo.GetXmlProperty("genxml/hidden/xmlupdatediscountcodedata"));
            UpdateDiscountCodes(strXml);
            strXml = Utils.UnCode(updateInfo.GetXmlProperty("genxml/hidden/xmlupdatevouchercodedata"));
            UpdateVoucherCodes(strXml);

        }

        public void UnlockUser()
        {
            if (_userInfo != null) UserController.UnLockUser(_userInfo);
        }

        public void AuthoriseClient()
        {
            if (_userInfo != null)
            {
                _userInfo.Membership.Approved = true;
                UserController.UpdateUser(PortalSettings.Current.PortalId, _userInfo);

                if (_userInfo.IsInRole("Unverified Users"))
                {
                    var rc = new DotNetNuke.Security.Roles.RoleController();
                    var ri = rc.GetRoleByName(PortalId, "Unverified Users");
                    var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
                    if (ri != null) RoleController.DeleteUserRole(_userInfo, ri, portalSettings, false);

                    ri = rc.GetRoleByName(PortalId, "Registered Users");                    
                    if (ri != null) RoleController.AddUserRole(_userInfo, ri, portalSettings, RoleStatus.Approved, DateTime.Now, DateTime.Now.AddYears(99),false,false);

                    ri = rc.GetRoleByName(PortalId, "Subscribers");
                    if (ri != null) RoleController.AddUserRole(_userInfo, ri, portalSettings, RoleStatus.Approved, DateTime.Now, DateTime.Now.AddYears(99), false, false);

                }
            }
        }

        public void UnAuthoriseClient()
        {
            if (_userInfo != null)
            {
                _userInfo.Membership.Approved = false;
                UserController.UpdateUser(PortalSettings.Current.PortalId, _userInfo);
            }
        }

        public void DeleteUser()
        {
            if (_userInfo != null)
            {
                var usrInfo = UserController.GetUserById(PortalSettings.Current.PortalId,_userInfo.UserID);
                UserController.DeleteUser(ref usrInfo, false, false);
            }
        }

        public void RestoreUser()
        {
            if (_userInfo != null)
            {
                var usrInfo = UserController.GetUserById(PortalSettings.Current.PortalId, _userInfo.UserID);
                UserController.RestoreUser(ref usrInfo);
            }
        }

        public Boolean RemoveUser()
        {
            if (_userInfo != null)
            {
                var objCtrl = new NBrightBuyController();
                var strFilter = " and UserId = " + _userInfo.UserID.ToString("") + " ";
                var recordcount = objCtrl.GetListCount(PortalId, -1, "ORDER", strFilter);
                if (recordcount == 0) // don't remove if we have orders
                {
                    var usrInfo = UserController.GetUserById(PortalSettings.Current.PortalId, _userInfo.UserID);
                    UserController.RemoveUser(usrInfo);
                    return true;
                }
            }
            return false;
        }

        public void OutputDebugFile(String fileName)
        {
            if (StoreSettings.Current.DebugModeFileOut) _clientInfo.XMLDoc.Save(PortalSettings.Current.HomeDirectoryMapPath + fileName);
        }

        public void UpdateDiscountCodes(String xmlAjaxData)
        {
            xmlAjaxData = GenXmlFunctions.DecodeCDataTag(xmlAjaxData);
            var discountcodesList = NBrightBuyUtils.GetGenXmlListByAjax(xmlAjaxData, "");
            // build xml for data records
            var strXml = "<genxml><discountcodes>";
            foreach (var discountcodesInfo in discountcodesList)
            {
                strXml += discountcodesInfo.XMLData;
            }
            strXml += "</discountcodes></genxml>";

            // replace models xml 
            DataRecord.ReplaceXmlNode(strXml, "genxml/discountcodes", "genxml");
            DiscountCodes = GetEntityList("discountcodes");
        }

        public void UpdateDiscountCodeList(List<NBrightInfo> discountcodesList)
        {
            // build xml for data records
            var strXml = "<genxml><discountcodes>";
            foreach (var discountcodesInfo in discountcodesList)
            {
                strXml += discountcodesInfo.XMLData;
            }
            strXml += "</discountcodes></genxml>";

            // replace models xml 
            DataRecord.ReplaceXmlNode(strXml, "genxml/discountcodes", "genxml");
            DiscountCodes = GetEntityList("discountcodes");
        }

        public void Save()
        {
            var objCtrl = new NBrightBuyController();
            objCtrl.Update(DataRecord);
            Utils.RemoveCache(_cacheKey);
        }

        public void AddNewDiscountCode(String xmldata = "")
        {
            if (xmldata == "") xmldata = "<genxml><discountcodes><genxml><textbox><coderef>" + Utils.GetUniqueKey().ToUpper() + "</coderef></textbox></genxml></discountcodes></genxml>";
            if (!xmldata.StartsWith("<genxml><discountcodes>")) xmldata = "<genxml><discountcodes>" + xmldata + "</discountcodes></genxml>";
            if (DataRecord.XMLDoc.SelectSingleNode("genxml/discountcodes") == null)
            {
                DataRecord.AddXmlNode(xmldata, "genxml/discountcodes", "genxml");
            }
            else
            {
                DataRecord.AddXmlNode(xmldata, "genxml/discountcodes/genxml", "genxml/discountcodes");
            }
            DiscountCodes = GetEntityList("discountcodes");
        }

        public void UpdateVoucherCodes(String xmlAjaxData)
        {
            xmlAjaxData = GenXmlFunctions.DecodeCDataTag(xmlAjaxData);
            var vouchercodesList = NBrightBuyUtils.GetGenXmlListByAjax(xmlAjaxData, "");
            // build xml for data records
            var strXml = "<genxml><vouchercodes>";
            foreach (var vouchercodesInfo in vouchercodesList)
            {
                strXml += vouchercodesInfo.XMLData;
            }
            strXml += "</vouchercodes></genxml>";

            // replace models xml 
            DataRecord.ReplaceXmlNode(strXml, "genxml/vouchercodes", "genxml");
            VoucherCodes = GetEntityList("vouchercodes");

            // update any new vouchers with starting voucher value
            var upd = false;
            foreach (var v in VoucherCodes)
            {
                if (v.GetXmlPropertyDouble("genxml/hidden/vouchervalue") == 0)
                {
                    v.SetXmlPropertyDouble("genxml/hidden/vouchervalue", v.GetXmlPropertyDouble("genxml/textbox/amount"));
                    v.SetXmlPropertyDouble("genxml/hidden/amountused", "0");
                    upd = true;
                }
            }
            if (upd) UpdateVoucherCodeList(VoucherCodes);
        }

        public void UpdateVoucherAmount(String vouchercode,double amountapplied)
        {
            if (amountapplied > 0)
            {
                var upd = false;
                foreach (var v in VoucherCodes)
                {
                    if (v.GetXmlProperty("genxml/textbox/coderef") == vouchercode)
                    {
                        var vouchervalue = v.GetXmlPropertyDouble("genxml/hidden/vouchervalue");
                        var amount = v.GetXmlPropertyDouble("genxml/textbox/amount");
                        v.SetXmlPropertyDouble("genxml/textbox/amount", (amount - amountapplied));
                        v.SetXmlPropertyDouble("genxml/hidden/amountused", (vouchervalue - (amount - amountapplied)));
                        upd = true;
                    }
                }
                if (upd) UpdateVoucherCodeList(VoucherCodes);
            }
        }

        public void UpdateVoucherCodeList(List<NBrightInfo> vouchercodesList)
        {
            // build xml for data records
            var strXml = "<genxml><vouchercodes>";
            foreach (var vouchercodesInfo in vouchercodesList)
            {
                strXml += vouchercodesInfo.XMLData;
            }
            strXml += "</vouchercodes></genxml>";

            // replace models xml 
            DataRecord.ReplaceXmlNode(strXml, "genxml/vouchercodes", "genxml");
            VoucherCodes = GetEntityList("vouchercodes");
        }

        public void AddNewVoucherCode(String xmldata = "")
        {
            if (xmldata == "") xmldata = "<genxml><vouchercodes><genxml><textbox><coderef>" + Utils.GetUniqueKey().ToUpper() + "</coderef></textbox></genxml></vouchercodes></genxml>";
            if (!xmldata.StartsWith("<genxml><vouchercodes>")) xmldata = "<genxml><vouchercodes>" + xmldata + "</vouchercodes></genxml>";
            if (DataRecord.XMLDoc.SelectSingleNode("genxml/vouchercodes") == null)
            {
                DataRecord.AddXmlNode(xmldata, "genxml/vouchercodes", "genxml");
            }
            else
            {
                DataRecord.AddXmlNode(xmldata, "genxml/vouchercodes/genxml", "genxml/vouchercodes");
            }
            VoucherCodes = GetEntityList("vouchercodes");
        }


        #endregion

        #region "itemlists"

        public void UpdateItemList(string listkey, string listcsv,string listname)
        {
            if (DataRecord.XMLDoc.SelectSingleNode("genxml/itemlists") == null)
            {
                DataRecord.SetXmlProperty("genxml/itemlists", "");
            }
            DataRecord.SetXmlProperty("genxml/itemlists/" + listkey, listcsv);
            if (listname == "") listname = listkey;
            DataRecord.SetXmlProperty("genxml/itemlists/" + listkey + "/@name", listname);
        }

        public string GetItemList(string listkey)
        {
            return DataRecord.GetXmlProperty("genxml/itemlists/" + listkey);
        }

        public Dictionary<string,string> GetItemListNames()
        {
            var l = new Dictionary<string, string>();
            var nodList = DataRecord.XMLDoc.SelectNodes("genxml/itemlists/*");
            foreach (XmlNode n in nodList)
            {
                var lname = n.Name;
                if (n.Attributes?["name"] != null)
                {
                    lname = n.Attributes["name"].InnerText;
                }
                l.Add(n.Name,lname);
            }
            return l;
        } 

        #endregion


        #region "private methods/functions"

        private void PopulateClientData(int userId,bool debugmode = false)
        {
            _clientInfo = new NBrightInfo(true);
            _clientInfo.ItemID = userId;
            _clientInfo.UserId = userId;
            _clientInfo.PortalId = PortalId;

            DataRecord = (NBrightInfo)NBrightBuyUtils.GetModCache(_cacheKey);

            if (DataRecord == null)
            {
                // get any datarecord on DB
                var objCtrl = new NBrightBuyController();
                DataRecord = objCtrl.GetByType(PortalId, -1, "CLIENT", userId.ToString(""), "", "", debugmode);
                if (DataRecord == null)
                {
                    DataRecord = new NBrightInfo(true);
                    DataRecord.ItemID = -1;
                    DataRecord.UserId = userId;
                    DataRecord.PortalId = PortalId;
                    DataRecord.ModuleId = -1;
                    DataRecord.TypeCode = "CLIENT";
                    objCtrl.Update(DataRecord);
                }
                NBrightBuyUtils.SetModCache(-1, _cacheKey, DataRecord);
            }
            else
            {
                _clientInfo.XMLData = DataRecord.XMLData;
            }


            _userInfo = UserController.GetUserById(PortalId, userId);
            if (_userInfo != null)
            {
                Exists = true;

                _clientInfo.ModifiedDate = _userInfo.Membership.CreatedDate;

                foreach (var propertyInfo in _userInfo.GetType().GetProperties())
                {
                    if (propertyInfo.CanRead)
                    {
                        object pv = null;
                        try
                        {
                            pv = propertyInfo.GetValue(_userInfo, null);
                            if (pv == null) pv = "";
                        }
                        catch (Exception)
                        {
                            pv = "";
                        }
                        _clientInfo.SetXmlProperty("genxml/textbox/" + propertyInfo.Name.ToLower(), pv.ToString());
                    }
                }

                foreach (DotNetNuke.Entities.Profile.ProfilePropertyDefinition p in _userInfo.Profile.ProfileProperties)
                {
                    _clientInfo.SetXmlProperty("genxml/textbox/" + p.PropertyName.ToLower(), p.PropertyValue);
                }

                _clientInfo.AddSingleNode("membership", "", "genxml");
                foreach (var propertyInfo in _userInfo.Membership.GetType().GetProperties())
                {
                    if (propertyInfo.CanRead)
                    {
                        var pv = propertyInfo.GetValue(_userInfo.Membership, null);
                        if (pv != null) _clientInfo.SetXmlProperty("genxml/membership/" + propertyInfo.Name.ToLower(), pv.ToString());
                    }
                }

                if (_userInfo.IsInRole(StoreSettings.ClientEditorRole))
                {
                    _clientInfo.SetXmlProperty("genxml/checkbox/clienteditorrole", "True");
                }
                else
                {
                    _clientInfo.SetXmlProperty("genxml/checkbox/clienteditorrole", "False");
                }

                DiscountCodes = GetEntityList("discountcodes");
                VoucherCodes = GetEntityList("vouchercodes");

            }
        }

        private List<NBrightInfo> GetEntityList(String entityName)
        {
            var l = new List<NBrightInfo>();
            if (DataRecord != null)
            {
                var xmlNodList = DataRecord.XMLDoc.SelectNodes("genxml/" + entityName + "/*");
                if (xmlNodList != null && xmlNodList.Count > 0)
                {
                    var lp = 1;
                    foreach (XmlNode xNod in xmlNodList)
                    {
                        var obj = new NBrightInfo();
                        obj.XMLData = xNod.OuterXml;
                        obj.ItemID = lp;
                        obj.Lang = DataRecord.Lang;
                        obj.ParentItemId = DataRecord.ItemID;
                        l.Add(obj);
                        lp += 1;
                    }
                }
            }
            return l;
        }


        #endregion


    }
}
