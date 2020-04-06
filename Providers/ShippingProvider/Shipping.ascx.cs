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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;

using Nevoweb.DNN.NBrightBuy.Base;
using Nevoweb.DNN.NBrightBuy.Components;
using DataProvider = DotNetNuke.Data.DataProvider;

namespace Nevoweb.DNN.NBrightBuy.Providers
{

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ViewNBrightGen class displays the content
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class Shipping : NBrightBuyAdminBase
    {


        #region Event Handlers

        private String _ctrlkey = ""; 

        override protected void OnInit(EventArgs e)
        {
            base.OnInit(e);

            try
            {
                _ctrlkey = (String)HttpContext.Current.Session["nbrightbackofficectrl"];

                #region "load templates"

                var t1 = "shippingheader.html";
                var t2 = "shippingbody.html";
                var t3 = "shippingfooter.html";


                // Get Display Header
                var rpDataHTempl = GetTemplateData(t1);
                rpDataH.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(rpDataHTempl, StoreSettings.Current.Settings(), PortalSettings.HomeDirectory);
                // Get Display Body
                var rpDataTempl = GetTemplateData(t2);
                rpData.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(rpDataTempl, StoreSettings.Current.Settings(), PortalSettings.HomeDirectory);
                // Get Display Footer
                var rpDataFTempl = GetTemplateData(t3);
                rpDataF.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(rpDataFTempl, StoreSettings.Current.Settings(), PortalSettings.HomeDirectory);

                #endregion


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
            if (UserId > 0) // only logged in users can see data on this module.
            {
                var shipping = new ShippingData(_ctrlkey);
                rpData.DataSource = shipping.GetRuleList();
                rpData.DataBind();
                
                // display header
                base.DoDetail(rpDataH,shipping.Info);

                // display footer
                base.DoDetail(rpDataF);
            }
        }

        #endregion

        #region  "Events "

        protected void CtrlItemCommand(object source, RepeaterCommandEventArgs e)
        {
            var cArg = e.CommandArgument.ToString();
            var param = new string[3];

            switch (e.CommandName.ToLower())
            {
                case "addnew":
                    var shipping = new ShippingData(_ctrlkey);
                    shipping.AddNewRule();
                    shipping.Save();
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "delete":
                    if (Utils.IsNumeric(cArg))
                    {
                        var shipping2 = new ShippingData(_ctrlkey);
                        shipping2.RemoveRule(Convert.ToInt32(cArg));
                        shipping2.Save();                        
                    }
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "saveall":
                    Update();
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "cancel":
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "alterpercent":
                    AlterCost();
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
            }

        }

        #endregion

        private String GetTemplateData(String templatename)
        {
            var controlMapPath = HttpContext.Current.Server.MapPath("/DesktopModules/NBright/NBrightBuy/Providers/ShippingProvider");
            var templCtrl = new NBrightCore.TemplateEngine.TemplateGetter(PortalSettings.Current.HomeDirectoryMapPath, controlMapPath, "Themes\\config", "");
            var templ = templCtrl.GetTemplateData(templatename, Utils.GetCurrentCulture());
            templ = Utils.ReplaceSettingTokens(templ, StoreSettings.Current.Settings());
            templ = Utils.ReplaceUrlTokens(templ);
            return templ;
        }

        private void Update()
        {
            var shipping = new ShippingData(_ctrlkey);

            shipping.Update(rpDataH);
            shipping.UpdateRule(rpData);
            shipping.Save();

            if (StoreSettings.Current.DebugMode) shipping.Info.XMLDoc.Save(PortalSettings.HomeDirectoryMapPath + "\\debug_Shipping.xml");

            //remove current setting from cache for reload
            CacheUtils.RemoveCache("NBrightBuyShipping" + PortalSettings.Current.PortalId.ToString(""));

        }

        private void AlterCost()
        {
            var info = new NBrightInfo();
            var shipping = new ShippingData(_ctrlkey);
            info.XMLData = GenXmlFunctions.GetGenXml(rpDataH);
            var percentValue = info.GetXmlPropertyDouble("genxml/textbox/alterpercent");
            shipping.UpdateCost(percentValue);
            shipping.Save();

            //remove current setting from cache for reload
            CacheUtils.RemoveCache("NBrightBuyShipping" + PortalSettings.Current.PortalId.ToString(""));

        }



    }

}
