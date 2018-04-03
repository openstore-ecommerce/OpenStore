using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using ManualPaymentProvider;
using NBrightCore.common;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Components;

namespace Nevoweb.DNN.NBrightBuy.Providers
{
    public class ManualPaymentProvider :Components.Interfaces.PaymentsInterface 
    {
        public override string Paymentskey { get; set; }

        public override string GetTemplate(NBrightInfo cartInfo)
        {
            var templ = "";

            var objCtrl = new NBrightBuyController();
            var info = objCtrl.GetPluginSinglePageData("manualpayment", "MANUALPAYMENT", Utils.GetCurrentCulture());
            var templateName = info.GetXmlProperty("genxml/textbox/checkouttemplate");
            templ = NBrightBuyUtils.RazorTemplRender(templateName, 0, "",info, "/DesktopModules/NBright/NBrightBuy/Providers/ManualPaymentProvider", "config", Utils.GetCurrentCulture(), StoreSettings.Current.Settings());
            return templ;
        }

        public override string RedirectForPayment(OrderData orderData)
        {
            var neworderstatus = "020";
            orderData.OrderStatus = neworderstatus;
            orderData.SavePurchaseData();
            var param = new string[3];
            param[0] = "orderid=" + orderData.PurchaseInfo.ItemID.ToString("D");
            param[1] = "status=1";
            param[2] = "language=" + Utils.GetCurrentCulture();
            return Globals.NavigateURL(StoreSettings.Current.PaymentTabId, "", param);
        }

        public override string ProcessPaymentReturn(HttpContext context)
        {
            var orderid = Utils.RequestQueryStringParam(context, "orderid");
            if (Utils.IsNumeric(orderid))
            {
                var orderData = new OrderData(Convert.ToInt32(orderid));
                var objCtrl = new NBrightBuyController();
                var info = objCtrl.GetPluginSinglePageData("manualpayment", "MANUALPAYMENT", orderData.Lang);

                var neworderstatus = "060";
                var settings = info.ToDictionary();
                if (settings.ContainsKey("orderstatus")) neworderstatus = settings["orderstatus"];
                if (neworderstatus == "") neworderstatus = "060";
                orderData.PaymentOk(neworderstatus);
                return GetReturnTemplate(orderData, true, "");
            }
            return "";
        }

        private string GetReturnTemplate(OrderData orderData, bool paymentok, string paymenterror)
        {
            var displaytemplate = "payment_ok.cshtml";
            if (!paymentok)
            {
                displaytemplate = "payment_fail.cshtml";
            }
            var templ = "";
            var objCtrl = new NBrightBuyController();
            var info = objCtrl.GetPluginSinglePageData("manualpayment", "MANUALPAYMENT", Utils.GetCurrentCulture());
            var passSettings = info.ToDictionary();
            foreach (var s in StoreSettings.Current.Settings()) // copy store setting, otherwise we get a byRef assignement
            {
                if (passSettings.ContainsKey(s.Key))
                    passSettings[s.Key] = s.Value;
                else
                    passSettings.Add(s.Key, s.Value);
            }
            if (passSettings.ContainsKey("paymenterror"))
            {
                passSettings.Add("paymenterror", paymenterror);
            }
            info.UserId = UserController.Instance.GetCurrentUserInfo().UserID;
            info.SetXmlProperty("genxml/ordernumber", orderData.OrderNumber);
            templ = NBrightBuyUtils.RazorTemplRender(displaytemplate, 0, "", info, "/DesktopModules/NBright/NBrightBuy/Providers/ManualPaymentProvider", "config", Utils.GetCurrentCulture(), passSettings);

            return templ;
        }

    }
}
