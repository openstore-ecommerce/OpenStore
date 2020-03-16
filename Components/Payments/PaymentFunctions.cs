using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Components.Interfaces;

namespace Nevoweb.DNN.NBrightBuy.Components.Payments
{
    public static class PaymentFunctions
    {

        public static string ProcessCommand(string paramCmd, HttpContext context)
        {
            var strOut = "ORDER - ERROR!! - No Security rights for current user!";
            var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);
            switch (paramCmd)
            {
                case "payment_manualpayment":
                    strOut = "";
                    var cartInfo = new CartData(PortalSettings.Current.PortalId);
                    if (cartInfo != null)
                    {
                        cartInfo.SaveModelTransQty(); // move qty into trans
                        var orderData = cartInfo.ConvertToOrder(StoreSettings.Current.DebugMode);
                        orderData.PaymentProviderKey = ajaxInfo.GetXmlProperty("genxml/hidden/paymentproviderkey").ToLower(); // provider keys should always be lowecase
                        orderData.SavePurchaseData();
                        strOut = PaymentsInterface.Instance(orderData.PaymentProviderKey).RedirectForPayment(orderData);
                    }                    
                    break;
                case "payment_getlist":
                    strOut = GetPaymentList(context);
                    break;
            }

            return strOut;
        }


        private static String GetPaymentList(HttpContext context)
        {
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

            var cartInfo = new CartData(PortalSettings.Current.PortalId);
            if (cartInfo != null)
            {
            }

            var strOut = NBrightBuyUtils.RazorTemplRender(razortemplate, 0, "", cartInfo, "/DesktopModules/NBright/NBrightBuy", themeFolder, Utils.GetCurrentCulture(), passSettings);
            return strOut;
        }



    }
}
