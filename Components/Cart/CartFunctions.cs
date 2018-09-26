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
using DotNetNuke.Services.Exceptions;
using NBrightCore.common;
using NBrightCore.images;
using NBrightCore.render;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Admin;
using Nevoweb.DNN.NBrightBuy.Components.Interfaces;

namespace Nevoweb.DNN.NBrightBuy.Components.Cart
{
    public static class CartFunctions
    {
        #region "Admin Methods"

        public static string TemplateRelPath = "/DesktopModules/NBright/NBrightBuy";

        public static string ProcessCommand(string paramCmd, HttpContext context, string editlang = "")
        {
            var strOut = "CART - ERROR!! - No Security rights or function command.";
            switch (paramCmd)
            {
                case "cart_render":
                    strOut = RenderCart(context);
                    break;
                case "cart_rendercartlist":
                    strOut = RenderCart(context);
                    break;
                case "cart_rendersummary":
                    strOut = RenderCart(context);
                    break;
                case "cart_rendershipmethod":
                    strOut = RenderCart(context);
                    break;
                case "cart_rendercartaddress":
                    strOut = RenderCart(context);
                    break;
                case "cart_renderminicart":
                    strOut = RenderCart(context);
                    break;                    
                case "cart_recalculatecart":
                    RecalculateCart(context);
                    break;
                case "cart_clearcart":
                    var currentcart = new CartData(PortalSettings.Current.PortalId);
                    currentcart.DeleteCart();
                    break;
                case "cart_removefromcart":
                    RemoveFromCart(context);
                    break;
                case "cart_updatebilladdress":
                    strOut = UpdateCartAddress(context, "bill");
                    break;
                case "cart_updateshipaddress":
                    strOut = UpdateCartAddress(context, "ship");
                    break;
                case "cart_updateshipoption":
                    strOut = UpdateCartAddress(context, "shipoption");
                    break;
                case "cart_redirecttocheckout":
                    RecalculateCart(context);
                    break;
                case "cart_addtobasket":
                    AddToBasket(context);
                    break;
                case "cart_addalltobasket":
                    AddAllToBasket(context);
                    break;
                case "cart_shippingprovidertemplate":
                    strOut = GetShippingProviderTemplates(context);
                    break;
                case "cart_recalculatesummary":
                    RecalculateSummary(context);
                    break;
                case "cart_recalculatesummary2":
                    RecalculateSummary(context);
                    break;
            }
            return strOut;
        }


        #endregion

        public static string RenderCart(HttpContext context, string carttemplate = "")
        {
            var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);

            if (ajaxInfo.GetXmlProperty("genxml/hidden/carttemplate") != "")
            {
                carttemplate = ajaxInfo.GetXmlProperty("genxml/hidden/carttemplate");
            }
            if (carttemplate == "") carttemplate = ajaxInfo.GetXmlProperty("genxml/hidden/minicarttemplate");

            var theme = ajaxInfo.GetXmlProperty("genxml/hidden/carttheme");
            if (theme == "") theme = ajaxInfo.GetXmlProperty("genxml/hidden/minicarttheme");
            if (theme == "")
            {
                theme = StoreSettings.Current.ThemeFolder;
            }

            var lang = ajaxInfo.GetXmlProperty("genxml/hidden/lang");
            var controlpath = ajaxInfo.GetXmlProperty("genxml/hidden/controlpath");
            if (controlpath == "") controlpath = "/DesktopModules/NBright/NBrightBuy";
            var razorTempl = "";
            if (carttemplate != "")
            {
                if (lang == "") lang = Utils.GetCurrentCulture();
                var currentcart = new CartData(PortalSettings.Current.PortalId);
                if (currentcart.UserId != UserController.Instance.GetCurrentUserInfo().UserID && currentcart.EditMode == "")
                {
                    //do save to update userid, so addrees checkout can get addresses. (Even if no user "-1")
                    currentcart.Save();
                }

                razorTempl = NBrightBuyUtils.RazorTemplRender(carttemplate, 0, "", currentcart, controlpath, theme, lang, StoreSettings.Current.Settings());
            }
            return razorTempl;
        }

        private static void RecalculateCart(HttpContext context)
        {
            var ajaxInfoList = NBrightBuyUtils.GetAjaxInfoList(context);
            var currentcart = new CartData(PortalSettings.Current.PortalId);
            foreach (var ajaxInfo in ajaxInfoList)
            {
                currentcart.MergeCartInputData(currentcart.GetItemIndex(ajaxInfo.GetXmlProperty("genxml/hidden/itemcode")), ajaxInfo);
            }
            currentcart.Save(StoreSettings.Current.DebugMode, true);
        }

        private static void RecalculateSummary(HttpContext context)
        {
            var objCtrl = new NBrightBuyController();

            var currentcart = new CartData(PortalSettings.Current.PortalId);
            var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context, true);
            var shipoption = currentcart.GetShippingOption(); // ship option already set in address update.

            currentcart.AddExtraInfo(ajaxInfo);
            currentcart.SetShippingOption(shipoption);
            currentcart.PurchaseInfo.SetXmlProperty("genxml/currentcartstage", "cartsummary"); // (Legacy) we need to set this so the cart calcs shipping
            currentcart.PurchaseInfo.SetXmlProperty("genxml/extrainfo/genxml/radiobuttonlist/shippingprovider", ajaxInfo.GetXmlProperty("genxml/radiobuttonlist/shippingprovider"));

            var shipref = ajaxInfo.GetXmlProperty("genxml/radiobuttonlist/shippingprovider");
            var displayanme = "";
            var shipInfo = objCtrl.GetByGuidKey(PortalSettings.Current.PortalId, -1, "SHIPPING", shipref);
            if (shipInfo != null)
            {
                var shipprov = ShippingInterface.Instance(shipref);
                if (shipprov != null)
                { 
                    displayanme = shipprov.Name();
                }
            }
            if (displayanme == "") displayanme = shipref;
            currentcart.PurchaseInfo.SetXmlProperty("genxml/extrainfo/genxml/hidden/shippingdisplayanme", displayanme);

            currentcart.Lang = ajaxInfo.Lang;  // set lang so we can send emails in same language the order was made in.

            currentcart.Save(StoreSettings.Current.DebugMode, true);

        }

        private static void RemoveFromCart(HttpContext context)
        {
            var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
            var currentcart = new CartData(PortalSettings.Current.PortalId);
            currentcart.RemoveItem(ajaxInfo.GetXmlProperty("genxml/hidden/itemcode"));
            currentcart.Save(StoreSettings.Current.DebugMode);
        }

        private static string UpdateCartAddress(HttpContext context, String addresstype = "")
        {
            var currentcart = new CartData(PortalSettings.Current.PortalId);
            var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context, true);

            currentcart.PurchaseInfo.SetXmlProperty("genxml/currentcartstage", "cartsummary"); // (Legacy) we need to set this so the cart calcs shipping

            if (addresstype == "bill")
            {
                currentcart.AddBillingAddress(ajaxInfo);
                currentcart.Save();
            }

            if (addresstype == "ship")
            {
                currentcart.AddShippingAddress(ajaxInfo);
                currentcart.Save();
            }

            if (addresstype == "shipoption")
            {
                var shipoption = ajaxInfo.GetXmlProperty("genxml/radiobuttonlist/rblshippingoptions");
                currentcart.SetShippingOption(shipoption);
                currentcart.Save();
            }

            return addresstype;
        }

        private static void AddToBasket(HttpContext context)
        {
            try
            {
                var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                var settings = ajaxInfo.ToDictionary();

                if (settings.ContainsKey("productid"))
                {
                    if (!settings.ContainsKey("portalid")) settings.Add("portalid", PortalSettings.Current.PortalId.ToString("")); // aways make sure we have portalid in settings

                    var currentcart = new CartData(Convert.ToInt16(settings["portalid"]));
                    currentcart.AddAjaxItem(ajaxInfo, StoreSettings.Current.SettingsInfo, StoreSettings.Current.DebugMode);
                    currentcart.Save(StoreSettings.Current.DebugMode);
                }
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
            }
        }

        private static void AddAllToBasket(HttpContext context)
        {
            try
            {
                var ajaxInfoList = NBrightBuyUtils.GetAjaxInfoList(context);
                foreach (var ajaxInfo in ajaxInfoList)
                {
                    var settings = ajaxInfo.ToDictionary();

                    if (settings.ContainsKey("productid"))
                    {
                        if (!settings.ContainsKey("portalid")) settings.Add("portalid", PortalSettings.Current.PortalId.ToString("")); // aways make sure we have portalid in settings

                        var currentcart = new CartData(Convert.ToInt16(settings["portalid"]));
                        currentcart.AddAjaxItem(ajaxInfo, StoreSettings.Current.SettingsInfo, StoreSettings.Current.DebugMode);
                        currentcart.Save(StoreSettings.Current.DebugMode);
                    }

                }
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
            }
        }

        private static string GetShippingProviderTemplates(HttpContext context)
        {
            var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
            var activeprovider = ajaxInfo.GetXmlProperty("genxml/radiobuttonlist/shippingprovider");
            var currentcart = new CartData(PortalSettings.Current.PortalId);

            var shipoption = currentcart.GetShippingOption(); // we don't want to overwrite the selected shipping option.
            currentcart.AddExtraInfo(ajaxInfo);
            currentcart.SetShippingOption(shipoption);
            currentcart.Save();

            if (activeprovider == "") activeprovider = currentcart.PurchaseInfo.GetXmlProperty("genxml/extrainfo/genxml/radiobuttonlist/shippingprovider");


            var strRtn = "";
            var pluginData = new PluginData(PortalSettings.Current.PortalId);
            var provList = pluginData.GetShippingProviders();
            if (provList != null && provList.Count > 0)
            {
                if (activeprovider == "") activeprovider = provList.First().Key;
                foreach (var d in provList)
                {
                    if (activeprovider == d.Key)
                    {
                        var p = d.Value;
                        var shippingkey = p.GetXmlProperty("genxml/textbox/ctrl");
                        var shipprov = ShippingInterface.Instance(shippingkey);
                        if (shipprov != null)
                        {
                            var razorTempl = shipprov.GetTemplate(currentcart.PurchaseInfo);
                            if (razorTempl != "")
                            {
                                var objList = new List<NBrightInfo>();
                                objList.Add(currentcart.PurchaseInfo);

                                var nbRazor = new NBrightRazor(objList.Cast<object>().ToList(), NBrightBuyUtils.GetPassSettings(ajaxInfo), HttpContext.Current.Request.QueryString);
                                nbRazor.ModuleId = -1;
                                nbRazor.FullTemplateName = "";
                                nbRazor.TemplateName = "";
                                nbRazor.ThemeFolder = "";
                                nbRazor.Lang = Utils.GetCurrentCulture();

                                strRtn += NBrightBuyUtils.RazorRender(nbRazor, razorTempl, shippingkey + "shippingtemplate", true);
                            }
                        }
                    }
                }
            }
            return strRtn;
        }

        public static string GetPaymentUrl()
        {
            try
            {

                var currentcart = new CartData(PortalSettings.Current.PortalId);

                if (currentcart.GetCartItemList().Count > 0)
                {
                    currentcart.SetValidated(true);
                    if (currentcart.EditMode == "E") currentcart.ConvertToOrder();
                }
                else
                {
                    currentcart.SetValidated(true);
                }
                currentcart.Save();

                var rtnurl = Globals.NavigateURL(StoreSettings.Current.PaymentTabId);
                if (currentcart.EditMode == "E")
                {
                    // is order being edited, so return to order status after edit.
                    // ONLY if the cartsummry is being displayed to the manager.
                    currentcart.ConvertToOrder();
                    // redirect to back office
                    var param = new string[2];
                    param[0] = "ctrl=orders";
                    param[1] = "eid=" + currentcart.PurchaseInfo.ItemID.ToString("");
                    var strbackofficeTabId = StoreSettings.Current.Get("backofficetabid");
                    var backofficeTabId = PortalSettings.Current.ActiveTab.TabID;
                    if (Utils.IsNumeric(strbackofficeTabId)) backofficeTabId = Convert.ToInt32(strbackofficeTabId);
                    rtnurl = Globals.NavigateURL(backofficeTabId, "", param);
                }

                // get payment providers, if only 1 then return payment url.
                if (StoreSettings.Current.GetBool("singlepaymentoption"))
                {
                    var pluginData = new PluginData(PortalSettings.Current.PortalId);
                    var provList = pluginData.GetPaymentProviders();
                    if (provList.Count() == 1)
                    {
                        foreach (var d in provList)
                        {
                            var p = d.Value;
                            var key = p.GetXmlProperty("genxml/textbox/ctrl");
                            var prov = PaymentsInterface.Instance(key);
                            if (prov != null)
                            {
                                rtnurl += "?provider=" + prov.Paymentskey;
                            }
                        }

                    }
                }

                return rtnurl;

            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return "ERROR";
            }
        }

        public static string GetPaymentButtonText()
        {
            try
            {
                var rtn = DnnUtils.GetResourceString("/DesktopModules/NBright/NBrightBuy/App_LocalResources/", "CartView.Order");

                // get payment providers, if only 1 then return payment url.
                if (StoreSettings.Current.GetBool("singlepaymentoption"))
                {
                    var pluginData = new PluginData(PortalSettings.Current.PortalId);
                    var provList = pluginData.GetPaymentProviders();
                    if (provList.Count() == 1)
                    {
                        rtn = DnnUtils.GetResourceString("/DesktopModules/NBright/NBrightBuy/App_LocalResources/", "CartView.PaymentButton");
                    }
                }

                return rtn;

            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return "ERROR";
            }
        }

    }
}
