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
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using NBrightCore.common;
using NBrightCore.providers;
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
    public partial class Settings : NBrightBuyAdminBase
    {


        #region Event Handlers


        override protected void OnInit(EventArgs e)
        {
            base.OnInit(e);

            try
            {

                #region "load templates"

                var t1 = "settingsheader.html";
                var t2 = "settingsbody.html";
                var t3 = "settingsfooter.html";

                // Get Display Header
                var rpDataHTempl = ModCtrl.GetTemplateData(ModSettings, t1, Utils.GetCurrentCulture(), DebugMode);
                rpDataH.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(rpDataHTempl, ModSettings.Settings(), PortalSettings.HomeDirectory);
                // Get Display Body
                var rpDataTempl = ModCtrl.GetTemplateData(ModSettings, t2, Utils.GetCurrentCulture(), DebugMode);
                rpData.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(rpDataTempl, ModSettings.Settings(), PortalSettings.HomeDirectory);
                // Get Display Footer
                var rpDataFTempl = ModCtrl.GetTemplateData(ModSettings, t3, Utils.GetCurrentCulture(), DebugMode);
                rpDataF.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(rpDataFTempl, ModSettings.Settings(), PortalSettings.HomeDirectory);

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

            #region "Data Repeater"
            if (UserId > 0) // only logged in users can see data on this module.
            {
                DisplayDataEntryRepeater();
            }

            #endregion

            // display header (Do header after the data return so the productcount works)
            base.DoDetail(rpDataH);

            // display footer
            base.DoDetail(rpDataF);

        }

        #endregion

        #region  "Events "

        protected void CtrlItemCommand(object source, RepeaterCommandEventArgs e)
        {
            var cArg = e.CommandArgument.ToString();
            var param = new string[3];

            switch (e.CommandName.ToLower())
            {
                case "save":
                    Update();
                    ShareProducts();
                    NBrightBuyUtils.AddRoleToDNN(PortalSettings.Current.PortalId,"Manager", "Manager");
                    NBrightBuyUtils.AddRoleToDNN(PortalSettings.Current.PortalId, "Editor", "Editor");
                    NBrightBuyUtils.AddRoleToDNN(PortalSettings.Current.PortalId, "ClientEditor", "ClientEditor");

                    param[0] = "ctrl=settings";
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "removelogo":
                    var settings = ModCtrl.GetByGuidKey(PortalSettings.Current.PortalId, 0, "SETTINGS", "NBrightBuySettings");
                    if (settings != null && settings.GetXmlProperty("genxml/hidden/hidemaillogo") != "")
                    {
                        settings.SetXmlProperty("genxml/hidden/hidemaillogo", "");
                        settings.SetXmlProperty("genxml/hidden/emaillogourl", "");
                        settings.SetXmlProperty("genxml/hidden/emaillogopath", "");
                        ModCtrl.Update(settings);
                    }
                    param[0] = "";
                    Response.Redirect(NBrightBuyUtils.AdminUrl(TabId, param), true);
                    break;
                case "cancel":
                    param[0] = "";
                    Response.Redirect(NBrightBuyUtils.AdminUrl(TabId, param), true);
                    break;
            }

        }

        #endregion

        private void ShareProducts()
        {
            var settings = ModCtrl.GetByGuidKey(PortalSettings.Current.PortalId, 0, "SETTINGS", "NBrightBuySettings");
            if (settings != null)
            {
                StoreSettings.Refresh(); // make sure we pickup changes.
                var shareproducts = StoreSettings.Current.GetBool("shareproducts");
                var sharedproductsflag = StoreSettings.Current.GetBool("sharedproductsflag");
                if (shareproducts) 
                {
                    // we only want to do this if the shareproducts has changed, so use a flag.
                    if (!sharedproductsflag) 
                    {
                        var l = ModCtrl.GetList(PortalId, -1, "PRD");
                        foreach (var i in l)
                        {
                            SharedRecord(i);
                        }
                        l = ModCtrl.GetList(PortalId, -1, "PRDLANG");
                        foreach (var i in l)
                        {
                            SharedRecord(i);
                        }
                        l = ModCtrl.GetList(PortalId, -1, "CATEGORY");
                        foreach (var i in l)
                        {
                            SharedRecord(i);
                        }
                        l = ModCtrl.GetList(PortalId, -1, "CATEGORYLANG");
                        foreach (var i in l)
                        {
                            SharedRecord(i);
                        }

                        settings.SetXmlProperty("genxml/checkbox/sharedproductsflag", "True"); // set flag
                        ModCtrl.Update(settings);
                    }
                }
                else
                {
                    // test if want to reverse the share products, by using the flag.
                    if (sharedproductsflag)
                    {
                        var l = ModCtrl.GetList(PortalId, -1, "PRD");
                        foreach (var i in l)
                        {
                            UnSharedRecord(i);
                        }
                        l = ModCtrl.GetList(PortalId, -1, "PRDLANG");
                        foreach (var i in l)
                        {
                            UnSharedRecord(i);
                        }
                        l = ModCtrl.GetList(PortalId, -1, "CATEGORY");
                        foreach (var i in l)
                        {
                            UnSharedRecord(i);
                        }
                        l = ModCtrl.GetList(PortalId, -1, "CATEGORYLANG");
                        foreach (var i in l)
                        {
                            UnSharedRecord(i);
                        }

                        settings.SetXmlProperty("genxml/checkbox/sharedproductsflag", "False"); // set flag
                        ModCtrl.Update(settings);
                    }
                }
            }
        }

        private void SharedRecord(NBrightInfo i)
        {
            var createdportalid = i.PortalId;
            if (createdportalid == -1) createdportalid = PortalSettings.Current.PortalId; // previously shared record, so defualt to current.
            i.SetXmlProperty("genxml/createdportalid", createdportalid.ToString(""));
            i.PortalId = -1;
            ModCtrl.Update(i);
        }
        private void UnSharedRecord(NBrightInfo i)
        {
            var createdportalid = PortalSettings.Current.PortalId; // default previously shared record to this portal.
            if (Utils.IsNumeric(i.GetXmlProperty("genxml/createdportalid"))) createdportalid = i.GetXmlPropertyInt("genxml/createdportalid"); 
            i.PortalId = createdportalid;
            ModCtrl.Update(i);
        }

        private void Update()
        {
            var settings = ModCtrl.GetByGuidKey(PortalSettings.Current.PortalId, 0, "SETTINGS", "NBrightBuySettings");
            if (settings == null)
            {
                settings = new NBrightInfo(true);
                settings.PortalId = PortalId;
                // use zero as moduleid so it's not picked up by the modules for their settings.
                // The normal GetList will get all moduleid OR moduleid=-1 
                settings.ModuleId = 0; 
                settings.ItemID = -1;
                settings.TypeCode = "SETTINGS";
                settings.GUIDKey = "NBrightBuySettings";
            }

            var sharedflag = settings.GetXmlProperty("genxml/checkbox/sharedproductsflag"); //maintain shared flag

            settings.XMLData = GenXmlFunctions.GetGenXml(rpData,"",StoreSettings.Current.FolderImagesMapPath);

            if (settings.GetXmlProperty("genxml/hidden/hidemaillogo") != "")
            {
                settings.SetXmlProperty("genxml/hidden/emaillogourl", StoreSettings.Current.FolderImages + "/" + settings.GetXmlProperty("genxml/hidden/hidemaillogo"));
                settings.SetXmlProperty("genxml/hidden/emaillogopath", StoreSettings.Current.FolderImagesMapPath + "\\" + settings.GetXmlProperty("genxml/hidden/hidemaillogo"));                
            }


            settings.SetXmlProperty("genxml/hidden/backofficetabid", PortalSettings.Current.ActiveTab.TabID.ToString(""));

            settings.SetXmlProperty("genxml/checkbox/sharedproductsflag", sharedflag); //maintain shared flag

            // store root mappath of website, so we can use it in the scheudler.
            settings.SetXmlProperty("genxml/hidden/rootmappath", HttpContext.Current.Server.MapPath("/"));
            
            ModCtrl.Update(settings);

            if (StoreSettings.Current.DebugModeFileOut) settings.XMLDoc.Save(PortalSettings.HomeDirectoryMapPath + "\\debug_Settings.xml");

            // create upload folders
            var folder = StoreSettings.Current.FolderNBStoreMapPath;
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            folder = StoreSettings.Current.FolderImagesMapPath;
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            folder = StoreSettings.Current.FolderDocumentsMapPath;
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            folder = StoreSettings.Current.FolderUploadsMapPath ;
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            folder = StoreSettings.Current.FolderClientUploadsMapPath;
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            folder = StoreSettings.Current.FolderTempMapPath;
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            //Create default category grouptype
            var l = NBrightBuyUtils.GetCategoryGroups(EditLanguage, true);
            var g = from i in l where i.GetXmlProperty("genxml/textbox/groupref") == "cat" select i;
            if (!g.Any()) CreateGroup("cat", "Categories","2");
            if (l.Count == 0)
            {
                // read init xml config file.
                string relPath = "/DesktopModules/NBright/NBrightBuy/Themes/config/default";
                var fullpath = HttpContext.Current.Server.MapPath(relPath);
                var strXml = Utils.ReadFile(fullpath + "\\setup.config");
                var nbi = new NBrightInfo();
                nbi.XMLData = strXml;

                var xmlNodes = nbi.XMLDoc?.SelectNodes("root/group");
                if (xmlNodes != null)
                {
                    foreach (XmlNode xmlNod in xmlNodes)
                    {
                        if (xmlNod.Attributes?["groupref"] != null)
                        {
                            g = from i in l where i.GetXmlProperty("genxml/textbox/groupref") == xmlNod.Attributes["groupref"].Value select i;
                            if (!g.Any()) CreateGroup(xmlNod.Attributes["groupref"].Value, xmlNod.Attributes["groupname"].Value, xmlNod.Attributes["grouptype"].Value);
                        }
                    }
                }
            }

            //update resx fields
            var resxDic = GenXmlFunctions.GetGenXmlResx(rpData);
            var genTempl = (GenXmlTemplate)rpData.ItemTemplate;
            var resxfolders = genTempl.GetResxFolders();  // ideally we'd create the settings resx at the portal level, but can't easily get that to work.
            var resxUpdate = NBrightBuyUtils.UpdateResxFields(resxDic, resxfolders, StoreSettings.Current.EditLanguage,true);

            //remove all cahce setting from cache for reload
            //DNN is sticky with some stuff (had some issues with email addresses not updating), so to be sure clear it all. 
            DataCache.ClearCache();

        }

        private void CreateGroup(String groupref, String name,String groupType)
        {
            var n = new GroupData(-1, StoreSettings.Current.EditLanguage);
            n.Ref = groupref;
            n.Name = name;
            n.Type = groupType;
            n.DataRecord.GUIDKey = groupref;
            n.Save();
            n.Validate();
        }


        private void DisplayDataEntryRepeater()
        {
                //render the detail page
                var settings = ModCtrl.GetByGuidKey(PortalSettings.Current.PortalId, 0, "SETTINGS", "NBrightBuySettings");
                if (settings == null) settings = new NBrightInfo(true);
                base.DoDetail(rpData, settings);
        }


    }

}
