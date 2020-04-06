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
using System.Web;
using System.Web.UI.WebControls;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Admin;
using Nevoweb.DNN.NBrightBuy.Components;
using Nevoweb.DNN.NBrightBuy.Components.Interfaces;

namespace Nevoweb.DNN.NBrightBuy
{

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ViewNBrightGen class displays the content
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class PrintView : CDefault
    {

        private String _itemid = "";
        private String _template = "";
        private String _printcode = "";
        private String _theme = "";
        private String _printtype = "";
        private String _scode = "";

        #region Event Handlers


        override protected void OnInit(EventArgs e)
        {

            base.OnInit(e);

            try
            {
                _itemid = Utils.RequestParam(HttpContext.Current, "itemid");
                _template = Utils.RequestParam(HttpContext.Current, "template");
                _printcode = Utils.RequestParam(HttpContext.Current, "printcode");
                _theme = Utils.RequestParam(HttpContext.Current, "theme");
                _printtype = Utils.RequestParam(HttpContext.Current, "printtype");
                _scode = Utils.RequestParam(HttpContext.Current, "scode");
            }
            catch (Exception exc)
            {
                //display the error on the template (don;t want to log it here, prefer to deal with errors directly.)
                var l = new Literal();
                l.Text = exc.ToString();
                phData.Controls.Add(l);
            }

        }

        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);
                if (Page.IsPostBack == false)
                {
                    PageLoad();
                }
            }
            catch (Exception exc) //Module failed to load
            {
                //display the error on the template (don;t want to log it here, prefer to deal with errors directly.)
                var l = new Literal();
                l.Text = exc.ToString();
                phData.Controls.Add(l);
            }
        }

        private void PageLoad()
        {
            var portalId = PortalSettings.Current.PortalId;
            var objUserInfo = UserController.Instance.GetCurrentUserInfo();
            if (_theme == "") _theme = StoreSettings.Current.Get("emailthemefolder");
            if (_theme == "") _theme = "ClassicRazor";

            switch (_printcode)
            {
                case "printorder":
                {
                    DisplayOrderData(portalId, objUserInfo, _itemid);
                    break;
                }
                case "printproduct":
                {
                    DisplayProductData(portalId, _itemid);
                    break;
                }
            }
        }

        #endregion

        #region "print display functions"

        private void DisplayOrderData(int portalId, UserInfo userInfo, String entryId)
        {
            var strOut = "***ERROR***  Invalid Data";
            if (Utils.IsNumeric(entryId) && entryId != "0")
            {
                var orderData = new OrderData(portalId, Convert.ToInt32(entryId));
                if (orderData.PurchaseInfo.TypeCode == "ORDER")
                {
                    strOut = "***ERROR***  Invalid Security";
                    if (_scode == orderData.PurchaseInfo.GetXmlProperty("genxml/securitycode") || userInfo.UserID == orderData.UserId || userInfo.IsInRole(StoreSettings.ManagerRole) || userInfo.IsInRole(StoreSettings.EditorRole) || userInfo.IsInRole(StoreSettings.SalesRole))
                    {
                        var doProv = false;
                        //check the payment provider for a print url
                        var shippingprovider = orderData.PurchaseInfo.GetXmlProperty("genxml/extrainfo/genxml/radiobuttonlist/shippingprovider");
                        if (shippingprovider != "")
                        {
                            var shipprov = ShippingInterface.Instance(shippingprovider);
                            if (shipprov != null)
                            {
                                if (_printtype == "shiplabel")
                                {
                                    var printurl = shipprov.GetDeliveryLabelUrl(orderData.PurchaseInfo);
                                    if (printurl != "")
                                    {
                                        doProv = true;
                                        Response.Redirect(printurl);
                                    }
                                }
                            }
                        }
                        if (!doProv)
                        {
                            var printtemplate = "printorder.cshtml";
                            var obj = new NBrightInfo(true);
                            obj.PortalId = PortalSettings.Current.PortalId;
                            obj.ModuleId = 0;
                            obj.Lang = Utils.GetCurrentCulture();
                            obj.GUIDKey = _printtype;
                            obj.ItemID = -1;

                            strOut = NBrightBuyUtils.RazorTemplRender(printtemplate, 0, "", obj, "/DesktopModules/NBright/NBrightBuy", _theme, Utils.GetCurrentCulture(), StoreSettings.Current.Settings());
                        }
                    }
                }
            }
            var l = new Literal();
            l.Text = strOut;
            phData.Controls.Add(l);
        }

        private void DisplayProductData(int portalId, String entryId)
        {
            var strOut = "***ERROR***  Invalid Data";
            if (Utils.IsNumeric(entryId) && entryId != "0")
            {
                var prodData = ProductUtils.GetProductData(Convert.ToInt32(entryId),Utils.GetCurrentCulture());
                if (prodData.Exists)
                {
                    var templCtrl = NBrightBuyUtils.GetTemplateGetter(_theme);
                    var strTempl = templCtrl.GetTemplateData(_template, Utils.GetCurrentCulture(), true, true, true, StoreSettings.Current.Settings());

                    strOut = GenXmlFunctions.RenderRepeater(prodData.Info, strTempl, "", "XMLData", Utils.GetCurrentCulture(), StoreSettings.Current.Settings());
                    if (_template.EndsWith(".xsl")) strOut = XslUtils.XslTransInMemory(prodData.Info.XMLData, strOut);
                }
            }
            var l = new Literal();
            l.Text = strOut;
            phData.Controls.Add(l);
        }

        #endregion

    }

}
