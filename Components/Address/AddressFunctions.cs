using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;

namespace Nevoweb.DNN.NBrightBuy.Components.Address
{
    public static class AddressAdminFunctions
    {
        #region "AddressAdmin Admin Methods"

        public static string ProcessCommand(string paramCmd,HttpContext context)
        {
            try
            {
                var strOut = "AddressAdmin - ERROR!! - No Security rights for current user!";
                if (UserController.Instance.GetCurrentUserInfo().UserID > 0)
                {
                    NBrightBuyUtils.SetContextLangauge(context);
                    var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);
                    var userId = ajaxInfo.GetXmlPropertyInt("genxml/hidden/userid");
                    var selecteditemid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selecteditemid");

                    switch (paramCmd)
                    {
                        case "addressadmin_getlist":
                            strOut = GetAddressList(context);
                            break;
                        case "addressadmin_saveaddress":
                            SaveAddress(context);
                            break;
                        case "addressadmin_deleteaddress":
                            strOut = DeleteAddress(context);
                            break;
                        case "addressadmin_editaddress":
                            strOut = GetAddress(context);
                            break;
                        case "addressadmin_newaddress":
                            strOut = NewAddress(context);
                            break;
                    }
                }
                return strOut;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }




#endregion

        private static String GetAddressList(HttpContext context)
        {
            var addressData = new AddressData();
            var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);
            var themeFolder = ajaxInfo.GetXmlProperty("genxml/hidden/themefolder");
            var razortemplate = ajaxInfo.GetXmlProperty("genxml/hidden/razortemplate");

            var passSettings = ajaxInfo.ToDictionary();
            foreach (var s in StoreSettings.Current.Settings()) // copy store setting, otherwise we get a byRef assignement
            {
                if (passSettings.ContainsKey(s.Key))
                    passSettings[s.Key] = s.Value;
                else
                    passSettings.Add(s.Key, s.Value);
            }

            var l = addressData.GetAddressList();
            var strOut = NBrightBuyUtils.RazorTemplRenderList(razortemplate, 0, "", l, "/DesktopModules/NBright/NBrightBuy", themeFolder, Utils.GetCurrentCulture(), passSettings);
            return strOut;
        }


        private static String GetAddress(HttpContext context)
        {
            var addressData = new AddressData();
            var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);
            var themeFolder = ajaxInfo.GetXmlProperty("genxml/hidden/themefolder");
            var razortemplate = ajaxInfo.GetXmlProperty("genxml/hidden/razortemplate");
            var selectedindex = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selectedindex");

            var passSettings = ajaxInfo.ToDictionary();
            foreach (var s in StoreSettings.Current.Settings()) // copy store setting, otherwise we get a byRef assignement
            {
                if (passSettings.ContainsKey(s.Key))
                    passSettings[s.Key] = s.Value;
                else
                    passSettings.Add(s.Key, s.Value);
            }

            var obj = addressData.GetAddress(selectedindex);

            obj.SetXmlProperty("genxml/selectedindex", selectedindex.ToString());

            var strOut = NBrightBuyUtils.RazorTemplRender(razortemplate, 0, "", obj, "/DesktopModules/NBright/NBrightBuy", themeFolder, Utils.GetCurrentCulture(), passSettings);
            return strOut;
        }


        private static String SaveAddress(HttpContext context)
        {
            var addressData = new AddressData();
            var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);
            var selectedindex = ajaxInfo.GetXmlPropertyInt("genxml/hidden/addrindex");

            addressData.UpdateAddress(ajaxInfo.XMLData, selectedindex);
            return "";
        }

        private static String NewAddress(HttpContext context)
        {
            var addressData = new AddressData();
            var dummyaddr = new NBrightInfo(true);
            dummyaddr.SetXmlProperty("genxml/textbox/dummy","XXXXXXX"); // set dummy value so we activate a unknown address.
            addressData.AddAddress(dummyaddr, -1);
            return "";
        }

        private static String DeleteAddress(HttpContext context)
        {
            var addressData = new AddressData();
            var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);
            var selectedindex = ajaxInfo.GetXmlPropertyInt("genxml/hidden/selectedindex");
            addressData.RemoveAddress(selectedindex);
            return "";
        }

    }
}
