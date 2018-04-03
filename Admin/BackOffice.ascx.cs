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
using System.Web;
using System.Web.UI.WebControls;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using NBrightCore.common;
using NBrightCore.render;
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
    public partial class BackOffice : NBrightBuyAdminBase
    {


        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            try
            {

                // check for new plugins
                PluginUtils.UpdateSystemPlugins();

                #region "load templates"

                var t1 = "backoffice.html";

                // Get Display Body
                var dataTempl = ModCtrl.GetTemplateData(ModSettings, t1, Utils.GetCurrentCulture(), DebugMode);
                // insert page header text
                var headerTempl = NBrightBuyUtils.GetGenXmlTemplate(dataTempl, ModSettings.Settings(), PortalSettings.HomeDirectory);
                NBrightBuyUtils.IncludePageHeaders(ModCtrl, ModuleId, Page, headerTempl, ModSettings.Settings(), null, DebugMode);

                // remove any DNN modules from the BO page
                var tabInfo = PortalSettings.Current.ActiveTab;
                var modCount = tabInfo.Modules.Count;
                for (int i = 0; i < modCount; i++)
                {
                    tabInfo.Modules.RemoveAt(0);                    
                }


                var aryTempl = Utils.ParseTemplateText(dataTempl);

                foreach (var s in aryTempl)
                {
                    var htmlDecode = System.Web.HttpUtility.HtmlDecode(s);
                    if (htmlDecode != null)
                    {
                        switch (htmlDecode.ToLower())
                        {
                            case "<tag:menu>":
                                var c1 = LoadControl(ControlPath + "/Menu.ascx");
                                phData.Controls.Add(c1);
                                break;
                            case "<tag:container>":
                                var c2 = LoadControl(ControlPath +  "/Container.ascx");
                                phData.Controls.Add(c2);
                                break;
                            default:
                                var lc = new Literal {Text = s};
                                phData.Controls.Add(lc);
                                break;
                        }
                    }
                }


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

    }

}
