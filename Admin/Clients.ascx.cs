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
using System.Web.UI.WebControls;
using DotNetNuke.Common;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Base;
using Nevoweb.DNN.NBrightBuy.Components;

namespace Nevoweb.DNN.NBrightBuy.Admin
{

    public partial class Clients : NBrightBuyAdminBase
    {
        #region Event Handlers

        override protected void OnInit(EventArgs e)
        {
            base.OnInit(e);
            // inject any pageheader we need
            var nbi = new NBrightInfo();
            nbi.Lang = Utils.GetCurrentCulture();
            nbi.PortalId = PortalId;
            var pageheaderTempl = NBrightBuyUtils.RazorTemplRender("Admin_Clients_head.cshtml", 0, "", nbi, "/DesktopModules/NBright/NBrightBuy", "config", Utils.GetCurrentCulture(), StoreSettings.Current.Settings());
            PageIncludes.IncludeTextInHeader(Page, pageheaderTempl);

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
                //display the error on the template (don't want to log it here, prefer to deal with errors directly.)
                var l = new Literal();
                l.Text = exc.ToString();
                Controls.Add(l);
            }
        }

        private void PageLoad()
        {

            if (NBrightBuyUtils.CheckRights()) // limit module data to NBS security roles
            {
                RazorTemplate = "Admin_Clients.cshtml";

                // new data record so set defaults.
                var obj = new NBrightInfo(true);
                obj.PortalId = PortalId;
                obj.ModuleId = 0;
                obj.Lang = Utils.GetCurrentCulture();
                obj.GUIDKey = RazorTemplate;
                obj.ItemID = -1;

                var strOut = NBrightBuyUtils.RazorTemplRender(RazorTemplate, 0, "", obj, "/DesktopModules/NBright/NBrightBuy", "config", Utils.GetCurrentCulture(), StoreSettings.Current.Settings());
                var lit = new Literal();
                lit.Text = strOut;
                phData.Controls.Add(lit);

            }

        }

        #endregion

    }

}
