// --- Copyright (c) notice NevoWeb ---
//  Copyright (c) 2014 SARL NevoWeb.  www.nevoweb.com. The MIT License (MIT).
// Author: D.C.Lee
// ------------------------------------------------------------------------
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// ------------------------------------------------------------------------
// This copyright notice may NOT be removed, obscured or modified without written consent from the author.
// --- End copyright notice --- 

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Web.UI.WebControls;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;

using Nevoweb.DNN.NBrightBuy.Base;
using Nevoweb.DNN.NBrightBuy.Components;
using Nevoweb.DNN.NBrightBuy.Components.Interfaces;
using DataProvider = DotNetNuke.Data.DataProvider;

namespace Nevoweb.DNN.NBrightBuy
{

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ViewNBrightGen class displays the content
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class Payment : NBrightBuyFrontOfficeBase
    {


        #region Event Handlers


        override protected void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (ModuleKey == "")  // if we don't have module setting jump out
            {
                var lit = new Literal();
                lit.Text = "NO MODULE SETTINGS";
                phData.Controls.Add(lit);
                return;
            }

        }

        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                if (Page.IsPostBack == false)
                {
                    var providerkey = Utils.RequestParam(Context, "provider");
                    if (providerkey != "")
                    {
                        var cartInfo = new CartData(PortalSettings.Current.PortalId);
                        if (cartInfo != null)
                        {
                            cartInfo.SaveModelTransQty(); // move qty into trans
                            var orderData = cartInfo.ConvertToOrder(StoreSettings.Current.DebugMode);
                            if (orderData != null)
                            {
                                orderData.PaymentProviderKey = providerkey.ToLower(); // provider keys should always be lowecase
                                orderData.SavePurchaseData();
                                PaymentsInterface.Instance(orderData.PaymentProviderKey).RedirectForPayment(orderData);
                            }
                        }
                    }
                    else
                    {
                        PageLoad();
                    }
                }
            }
            catch (Exception exc) //Module failed to load
            {
                //display the error on the template (don't want to log it here, prefer to deal with errors directly.)
                var l = new Literal();
                l.Text = exc.ToString();
                Controls.Add(l);
            }
        }

        private void PageLoad()
        {
            var strOut = "";

            var orderid = Utils.RequestParam(Context, "orderid");
            if (Utils.IsNumeric(orderid))
            {
                // orderid exists, so must be return from bank; Process it!!
                var orderData = new OrderData(PortalId, Convert.ToInt32(orderid));
                var prov = PaymentsInterface.Instance(orderData.PaymentProviderKey);


                strOut = prov.ProcessPaymentReturn(Context);
                if (strOut == "")
                {
                    orderData = new OrderData(PortalId, Convert.ToInt32(orderid)); // reload the order, becuase the status and typecode may have changed by the payment provider.
                    var status = Utils.RequestQueryStringParam(Context, "status");
                    if (status == "0")
                    {
                        var rtnerr = orderData.PurchaseInfo.GetXmlProperty("genxml/paymenterror");
                        orderData.AddAuditMessage(rtnerr, "paymsg", "payment.ascx", "False");
                        orderData.Save();
                        if (strOut == "")
                        {
                            strOut = NBrightBuyUtils.RazorTemplRender("payment_fail.cshtml", 0, "", orderData.PurchaseInfo, ControlPath, "config", Utils.GetCurrentCulture(), StoreSettings.Current.Settings());
                        }
                    }
                    else
                    {
                        orderData.PaymentOk("050");
                        if (strOut == "")
                        {
                            strOut = NBrightBuyUtils.RazorTemplRender("payment_ok.cshtml", 0, "", orderData.PurchaseInfo, ControlPath, "config", Utils.GetCurrentCulture(), StoreSettings.Current.Settings());
                        }
                    }
                }
            }
            else
            {
                var cartInfo = new CartData(PortalSettings.Current.PortalId);
                // not returning from bank, so display list of payment providers.
                strOut = NBrightBuyUtils.RazorTemplRender(RazorTemplate, 0, "", cartInfo, ControlPath, ThemeFolder, Utils.GetCurrentCulture(), StoreSettings.Current.Settings());
            }

            var lit = new Literal();
            lit.Text = strOut;
            phData.Controls.Add(lit);

        }

        #endregion

    }

}
