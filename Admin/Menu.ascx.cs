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
using System.Web.UI.WebControls;
using DotNetNuke.Common;
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
    public partial class Menu : NBrightBuyAdminBase
    {
        private string _resxpath = "";

        protected override void OnLoad(EventArgs e)
        {
            try
            {
                if (UserId > 0) //do nothing if user not logged on
                {
                    _resxpath = StoreSettings.NBrightBuyPath() + "/App_LocalResources/Plugins.ascx.resx";

                    base.OnLoad(e);

                    var rpDataTemplH = ModCtrl.GetTemplateData(ModSettings, "menuheader.html",
                                                               Utils.GetCurrentCulture(), DebugMode);
                    var l = new Literal();
                    l.Text = rpDataTemplH;
                    phMenuH.Controls.Add(l);

                    l = new Literal();
                    l.Text = GetMenu();
                    phMenuF.Controls.Add(l);

                    var rpDataTemplF = ModCtrl.GetTemplateData(ModSettings, "menufooter.html",
                                                               Utils.GetCurrentCulture(), DebugMode);
                    l = new Literal();
                    l.Text = rpDataTemplF;
                    phMenuF.Controls.Add(l);
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

        private String GetMenu()
        {
            var strCacheKey = "bomenuhtml*" + Utils.GetCurrentCulture() + "*" + PortalId.ToString("") + "*" + UserId.ToString("");

            var strOut = "";
            var obj = CacheUtils.GetCache(strCacheKey);
            if (obj != null) strOut = (String) obj;

            if (StoreSettings.Current.DebugMode || strOut == "")
            {
                var pluginData = new PluginData(PortalId);

                var bomenuattributes = DnnUtils.GetLocalizedString("bomenuattributes", _resxpath, Utils.GetCurrentCulture());
                var bosubmenuattributes = DnnUtils.GetLocalizedString("bosubmenuattributes", _resxpath, Utils.GetCurrentCulture());

                //get group list (these are the sections/first level of the menu)
                var rootList = new Dictionary<String, String>();

                var pluginList = PluginUtils.GetPluginList();
                foreach (var p in pluginList)
                {
                    if (PluginUtils.CheckSecurity(p))
                    {
                        var grpname = p.GetXmlProperty("genxml/textbox/group");
                        if (p.GetXmlPropertyBool("genxml/checkbox/hidden") == false)
                        {
                            var rootname = grpname;
                            if (rootname == "") rootname = p.GetXmlProperty("genxml/textbox/ctrl");
                            if (!rootList.ContainsKey(rootname))
                            {
                                var resxname = DnnUtils.GetLocalizedString(rootname.ToLower(), _resxpath, Utils.GetCurrentCulture());
                                if (resxname == "") resxname = rootname;
                                rootList.Add(rootname, resxname);
                            }
                        }
                    }
                }

                strOut = "<ul " + bomenuattributes + ">";

                // clientEditor roles can only access products, so only add the exit button to the menu.
                // the security restriuction on product ctrl is applied in the container.ascx.cs
                //if (!NBrightBuyUtils.IsClientOnly()) 
                //{
                    foreach (var rootname in rootList)
                    {
                        var rtnlist = pluginData.GetSubList(rootname.Key);
                        var sublist = new List<NBrightInfo>();
                        // check security
                        foreach (var p in rtnlist)
                        {
                            if (PluginUtils.CheckSecurity(p)) sublist.Add(p);
                        }


                        var href = "#";
                        var ctrl = "";
                        var name = "unknown";
                        var icon = "";
                        var hrefclass = "";
                        var securityrootcheck = true;
                        if (sublist.Count > 0)
                        {
                            // has sub menus
                            ctrl = rootname.Key;
                            name = rootname.Value;
                            hrefclass = "class='dropdown-toggle'";
                            icon = DnnUtils.GetLocalizedString(ctrl.ToLower() + "_icon", _resxpath, Utils.GetCurrentCulture());
                            strOut += "<li class='dropdown'>";
                        }
                        else
                        {
                            // clickable root menu
                            var rootp = pluginData.GetPluginByCtrl(rootname.Key);
                            if (rootp != null)
                            {
                                ctrl = rootp.GetXmlProperty("genxml/textbox/ctrl");
                                name = rootp.GetXmlProperty("genxml/textbox/name");
                                icon = rootp.GetXmlProperty("genxml/textbox/icon");

                                securityrootcheck = PluginUtils.CheckSecurity(rootp);
                                if (securityrootcheck)
                                {
                                    strOut += "<li>";
                                    var param = new string[1];
                                    param[0] = "ctrl=" + ctrl;
                                    href = Globals.NavigateURL(TabId, "", param);
                                }
                            }
                            else
                            {
                                securityrootcheck = false;
                            }
                        }
                        if (securityrootcheck) strOut += GetRootLinkNode(name, ctrl, icon, href, hrefclass);

                        if (sublist.Count > 0)
                        {
                            strOut += "<ul " + bosubmenuattributes + ">";
                            foreach (var p in sublist)
                            {
                                if (p.GetXmlPropertyBool("genxml/checkbox/hidden") == false)
                                {

                                    ctrl = p.GetXmlProperty("genxml/textbox/ctrl");
                                    name = p.GetXmlProperty("genxml/textbox/name");
                                    icon = p.GetXmlProperty("genxml/textbox/icon");
                                    var param = new string[1];
                                    param[0] = "ctrl=" + ctrl;
                                    href = Globals.NavigateURL(TabId, "", param);
                                    strOut += "<li>" + GetSubLinkNode(name, ctrl, icon, href) + "</li>";
                                }
                            }
                            strOut += "</ul>";
                        }
                        if (securityrootcheck) strOut += "</li>";
                    }

               // }

                // add exit button
                strOut += "<li>";
                var tabid = StoreSettings.Current.Get("exittab");
                var exithref = "/";
                if (Utils.IsNumeric(tabid)) exithref = Globals.NavigateURL(Convert.ToInt32(tabid));
                strOut += GetRootLinkNode("Exit", "exit", DnnUtils.GetLocalizedString("exit_icon", _resxpath, Utils.GetCurrentCulture()), exithref, "");
                strOut += "</li>";

                strOut += "</ul>";

                CacheUtils.SetCache(strCacheKey, strOut);

                if (StoreSettings.Current.DebugModeFileOut) Utils.SaveFile(PortalSettings.HomeDirectoryMapPath + "\\debug_menu.html", strOut);
            }

            return strOut;
        }


        private String GetRootLinkNode(String name,String ctrl,String icon,String href,String hrefclass)
        {
            var strOutSub = "";
            var dispname = DnnUtils.GetLocalizedString(ctrl.ToLower(), _resxpath, Utils.GetCurrentCulture());
            if (string.IsNullOrEmpty(dispname)) dispname = name;
            strOutSub += "<a " + hrefclass + " href='" + href + "'>" + icon + "<span class='hidden-xs'>" + dispname + "</span></a>";
            return strOutSub;
        }

        private String GetSubLinkNode(String name, String ctrl, String icon, String href)
        {
            var strOutSub = "";
            var dispname = DnnUtils.GetLocalizedString(ctrl.ToLower(), _resxpath, Utils.GetCurrentCulture());
            if (string.IsNullOrEmpty(dispname)) dispname = name;
            strOutSub += "<a href='" + href + "'>" + icon + dispname + "</a>";
            return strOutSub;
        }

        private Boolean IsInRoles(String roleCSV)
        {
            if (roleCSV == "") return true;
            var s = roleCSV.Split(',');
            foreach (var r in s)
            {
                if (UserInfo.IsInRole(r)) return true;
            }
            return false;
        }

        #region  "Events "

        protected void CtrlItemCommand(object source, RepeaterCommandEventArgs e)
        {
            var cArg = e.CommandArgument.ToString();
            var param = new string[3];

            switch (e.CommandName.ToLower())
            {
                case "link":
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
            }

        }

        #endregion


    }

}
