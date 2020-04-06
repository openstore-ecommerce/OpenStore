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
using System.Web.UI.WebControls;
using System.Xml;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;

using Nevoweb.DNN.NBrightBuy.Base;
using Nevoweb.DNN.NBrightBuy.Components;
using DataProvider = DotNetNuke.Data.DataProvider;

namespace Nevoweb.DNN.NBrightBuy.Admin
{

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ViewNBrightGen class displays the content
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class DashSummary : NBrightBuyAdminBase
    {


        #region Event Handlers


        override protected void OnInit(EventArgs e)
        {
            base.OnInit(e);

            try
            {
                // Get Display Header
                var rpDashTempl = ModCtrl.GetTemplateData(ModSettings, "dashboard.html", Utils.GetCurrentCulture(), DebugMode);
                rpDash.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(rpDashTempl, ModSettings.Settings(), PortalSettings.HomeDirectory);

                var rpDataHTempl = ModCtrl.GetTemplateData(ModSettings, "dashordersheader.html", Utils.GetCurrentCulture(), DebugMode);
                rpDataH.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(rpDataHTempl, ModSettings.Settings(), PortalSettings.HomeDirectory);

                var rpDataTempl = ModCtrl.GetTemplateData(ModSettings, "dashordersbody.html", Utils.GetCurrentCulture(), DebugMode);
                rpData.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(rpDataTempl, ModSettings.Settings(), PortalSettings.HomeDirectory);

                var rpDataFTempl = ModCtrl.GetTemplateData(ModSettings, "dashordersfooter.html", Utils.GetCurrentCulture(), DebugMode);
                rpDataF.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(rpDataFTempl, ModSettings.Settings(), PortalSettings.HomeDirectory);

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
            #region "get Dashboard"

            bool forceRefresh = Utils.RequestParam(Context, "refresh") == "1";
            var statsInfo = GetStats(PortalId, forceRefresh);
            var statsData = new NBrightInfo(true);

            #endregion

            #region "get order list"

            var orderList = new List<NBrightInfo>();
            var nodList = statsInfo.XMLDoc.SelectNodes("root/orders/*");
            if (nodList != null)
            {
                foreach (XmlNode nod in nodList)
                {
                    var nbi = new NBrightInfo();
                    nbi.FromXmlItem(nod.OuterXml);
                    var xmlData = nod.SelectSingleNode("genxml/genxml");
                    if (xmlData != null) nbi.XMLData = xmlData.OuterXml;
                    orderList.Add(nbi);
                }
            }

            #endregion

            DoDetail(rpDash, statsInfo); // dashboard

            DoDetail(rpDataH, statsInfo); // orders header

            rpData.DataSource = orderList; // orders body
            rpData.DataBind();

            DoDetail(rpDataF, statsInfo); // orders footer



        }

        #endregion

        #region  "Events "

        protected void CtrlItemCommand(object source, RepeaterCommandEventArgs e)
        {
            var cArg = e.CommandArgument.ToString();
            var param = new string[3];

            switch (e.CommandName.ToLower())
            {
                case "refresh":
                    param[0] = "refresh=1";
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "editorder":
                    param[0] = "";
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
            }

        }

        #endregion


        private NBrightInfo GetStats(int portalId, bool forceRefresh = false)
        {
            var cachekey = "nbrightbuydashboard*" + PortalId.ToString("");
            var statsInfo = (NBrightInfo)CacheUtils.GetCache(cachekey);

            if (statsInfo == null || StoreSettings.Current.DebugMode || forceRefresh)
            {
                var objCtrl = new NBrightBuyController();
                statsInfo = new NBrightInfo(true);
                
                var objQual = DotNetNuke.Data.DataProvider.Instance().ObjectQualifier;
                var dbOwner = DotNetNuke.Data.DataProvider.Instance().DatabaseOwner;

                var statsXml = objCtrl.GetSqlxml("exec " + dbOwner + objQual + "NBrightBuy_DashboardStats " + portalId);
                statsInfo.XMLData = statsXml;
                CacheUtils.SetCache(cachekey, statsInfo);
            }
            return statsInfo;
        }


    }

}
