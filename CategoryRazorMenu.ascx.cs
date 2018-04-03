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
using System.ComponentModel.Design;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using DotNetNuke.Common;
using DotNetNuke.Entities.Content.Common;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;
using NBrightDNN.render;
using Nevoweb.DNN.NBrightBuy.Base;
using Nevoweb.DNN.NBrightBuy.Components;
using Nevoweb.DNN.NBrightBuy.Components.Interfaces;
using RazorEngine;
using DataProvider = DotNetNuke.Data.DataProvider;

namespace Nevoweb.DNN.NBrightBuy
{
    /// <summary>
    /// Deals with display of all cart modules that use Razor templates
    /// </summary>
    public partial class CategoryRazorMenu : NBrightBuyFrontOfficeBase
    {
        private string _controlPath = "";

        #region Event Handlers

        override protected void OnInit(EventArgs e)
        {           
            base.OnInit(e);

            _controlPath = ControlPath;

            if (ModuleKey == "")  // if we don't have module setting jump out
            {
                var lit = new Literal();
                lit.Text = "NO MODULE SETTINGS";
                phData.Controls.Add(lit);
                return;
            }


            if (ModSettings != null)
            {
                // check if we're using a provider controlpath for the templates.
                var providercontrolpath = ModSettings.Get("providercontrolpath");
                if (providercontrolpath != "")
                {
                    _controlPath = "/DesktopModules/NBright/" + providercontrolpath + "/";
                }
            }

        }

        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);
                if (Page.IsPostBack == false)
                {
                    // do razor code
                    RazorPageLoad();
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

        private void RazorPageLoad()
        {
            var obj = new NBrightInfo();
            var strOut = NBrightBuyUtils.RazorTemplRender(RazorTemplate,-1,"", obj,_controlPath, ThemeFolder, Utils.GetCurrentCulture(), ModSettings.Settings());
            var lit = new Literal();
            lit.Text = strOut;
            phData.Controls.Add(lit);
        }

        #endregion

    }

}
