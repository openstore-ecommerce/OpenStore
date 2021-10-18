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
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Localization;
using NBrightCore.common;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Base;
using Nevoweb.DNN.NBrightBuy.Components;


namespace Nevoweb.DNN.NBrightBuy.Admin
{

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ViewNBrightGen class displays the content
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class EditLanguage : NBrightBuyBase
    {
        private String _entryid = "";
        private String _ctrl = "";

        public String Size { get; set; }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            _entryid = Utils.RequestParam(Context, "eid");
            _ctrl = Utils.RequestParam(Context, "ctrl");

        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (Size != "16" & Size != "24" & Size != "32") Size = "32";

            //NOTE: We need to recreate dynamically created controls on postback for them to pickup the event. 
                var enabledlanguages = LocaleController.Instance.GetLocales(PortalId);
                Controls.Add(new LiteralControl("<ul class='editlanguage'>"));
                foreach (var l in enabledlanguages)
                {
                    Controls.Add(new LiteralControl("<li>"));
                    var cmd = new LinkButton();
                    cmd.Text = "<img 'langflag' src='/images/flags/" + l.Value.Code + ".gif' alt='" + l.Value.EnglishName + "' />";
                    cmd.CommandArgument = l.Value.Code;
                    cmd.CommandName = "selectlang";
                    cmd.Command += (s, cmde) =>
                                       {
                                           var param = new string[2];
                                           if (_entryid != "")
                                           {
                                               param[0] = "eid=" + _entryid;
                                           }
                                           if (_ctrl != "") param[1] = "ctrl=" + _ctrl;

                                           //remove all cahce setting from cache for reload
                                           //DNN is sticky with some stuff (had some issues with email addresses not updating), so to be sure clear it all. 
                                           DataCache.ClearCache();

                                           StoreSettings.Current.EditLanguage = cmde.CommandArgument.ToString();
                                           Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                                       };
                    Controls.Add(cmd);
                    Controls.Add(new LiteralControl("</li>"));
                }
                Controls.Add(new LiteralControl("</ul>"));
        }

    }
}
