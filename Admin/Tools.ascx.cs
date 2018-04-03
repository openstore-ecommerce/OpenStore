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
using System.Runtime.Remoting;
using System.Web;
using System.Web.UI.WebControls;
using System.Xml;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Base;
using Nevoweb.DNN.NBrightBuy.Components;
using Nevoweb.DNN.NBrightBuy.Components.Interfaces;

namespace Nevoweb.DNN.NBrightBuy.Admin
{

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ViewNBrightGen class displays the content
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class Tools : NBrightBuyAdminBase
    {


        override protected void OnInit(EventArgs e)
        {
            base.OnInit(e);

            try
            {


                #region "load templates"

                var t1 = "tools.html";

                // Get Display Body
                var dataTempl = ModCtrl.GetTemplateData(ModSettings, t1, Utils.GetCurrentCulture(), DebugMode);
                // insert page header text
                rpData.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(dataTempl, ModSettings.Settings(), PortalSettings.HomeDirectory);

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
                var obj = new NBrightInfo(true);
                base.DoDetail(rpData,obj);
            }

            #endregion

        }

                #endregion

        #region  "Events "

        protected void CtrlItemCommand(object source, RepeaterCommandEventArgs e)
        {
            var cArg = e.CommandArgument.ToString();
            var param = new string[3];

            switch (e.CommandName.ToLower())
            {
                case "cleardownstore":
                    param[0] = "";
                    DoClearDown();
                    Response.Redirect(NBrightBuyUtils.AdminUrl(TabId, param), true);
                    break;
                case "validatestore":
                    param[0] = "";
                    NBrightBuyUtils.ValidateStore();
                    NBrightBuyUtils.SetNotfiyMessage(ModuleId, "validatecompleted", NotifyCode.ok);
                    Response.Redirect(NBrightBuyUtils.AdminUrl(TabId, param), true);
                    break;
                case "purgecarts":
                    param[0] = "";
                    PurgeCarts();
                    NBrightBuyUtils.SetNotfiyMessage(ModuleId, "completed", NotifyCode.ok);
                    Response.Redirect(NBrightBuyUtils.AdminUrl(TabId, param), true);
                    break;
                case "resetlangauge":
                    param[0] = "";
                    ResetLanguage();
                    Response.Redirect(NBrightBuyUtils.AdminUrl(TabId, param), true);
                    break;
                case "resetportalmenu":
                    param[0] = "";
                    ResetPortalMenu();
                    Response.Redirect(NBrightBuyUtils.AdminUrl(TabId, param), true);
                    break;
                case "purgeimages":
                    param[0] = "";
                    PurgeImages();
                    Response.Redirect(NBrightBuyUtils.AdminUrl(TabId, param), true);
                    break;                    
                case "cancel":
                    param[0] = "";
                    Response.Redirect(NBrightBuyUtils.AdminUrl(TabId, param), true);
                    break;
                case "runcustomprovider":
                    var msg = RunCustomProvider();
                    NBrightBuyUtils.SetNotfiyMessage(ModuleId, "completed", NotifyCode.ok);
                    param[0] = "";
                    Response.Redirect(NBrightBuyUtils.AdminUrl(TabId, param), true);
                    break;
            }

        }

        #endregion


        private void DoClearDown()
        {
            var pass = GenXmlFunctions.GetField(rpData, "txtclearpass");
            if (pass == StoreSettings.Current.Get("adminpin") && pass != "")
            {
                var done = false;
                var objCtrl = new NBrightBuyController();
                var objQual = DotNetNuke.Data.DataProvider.Instance().ObjectQualifier;
                var dbOwner = DotNetNuke.Data.DataProvider.Instance().DatabaseOwner;
                var stmt = "";
                if (GenXmlFunctions.GetField(rpData, "clearproducts") == "True")
                {
                    stmt = "delete from " + dbOwner + "[" + objQual + "NBrightBuy] where PortalId = " + PortalId.ToString("") + " and typecode = 'PRD' ";
                    objCtrl.ExecSql(stmt);
                    stmt = "delete from " + dbOwner + "[" + objQual + "NBrightBuy] where PortalId = " + PortalId.ToString("") + " and typecode = 'PRDLANG' ";
                    objCtrl.ExecSql(stmt);
                    stmt = "delete from " + dbOwner + "[" + objQual + "NBrightBuy] where PortalId = " + PortalId.ToString("") + " and typecode = 'AMY' ";
                    objCtrl.ExecSql(stmt);
                    stmt = "delete from " + dbOwner + "[" + objQual + "NBrightBuy] where PortalId = " + PortalId.ToString("") + " and typecode = 'AMYLANG' ";
                    objCtrl.ExecSql(stmt);
                    stmt = "delete from " + dbOwner + "[" + objQual + "NBrightBuy] where PortalId = " + PortalId.ToString("") + " and typecode = 'PRDXREF' ";
                    objCtrl.ExecSql(stmt);
                    stmt = "delete from " + dbOwner + "[" + objQual + "NBrightBuy] where PortalId = " + PortalId.ToString("") + " and typecode = 'USERPRDXREF' ";
                    objCtrl.ExecSql(stmt);
                    stmt = "delete from " + dbOwner + "[" + objQual + "NBrightBuy] where PortalId = " + PortalId.ToString("") + " and typecode = 'CATCASCADE' ";
                    objCtrl.ExecSql(stmt);
                    stmt = "delete from " + dbOwner + "[" + objQual + "NBrightBuy] where PortalId = " + PortalId.ToString("") + " and typecode = 'CATXREF' ";
                    objCtrl.ExecSql(stmt);
                    done = true;
                }

                if (GenXmlFunctions.GetField(rpData, "clearcategories") == "True")
                {
                    stmt = "delete from " + dbOwner + "[" + objQual + "NBrightBuy] where PortalId = " + PortalId.ToString("") + " and typecode = 'CATEGORY' and [XMLData].value('(genxml/dropdownlist/ddlgrouptype)[1]','nvarchar(max)') = 'cat' ";
                    objCtrl.ExecSql(stmt);
                    stmt = "delete from " + dbOwner + "[" + objQual + "NBrightBuy] where PortalId = " + PortalId.ToString("") + " and typecode = 'CATEGORYLANG' and ParentItemId not in (Select itemid from " + dbOwner + "[" + objQual + "NBrightBuy] where PortalId = " + PortalId.ToString("") + " and typecode = 'CATEGORY') ";
                    objCtrl.ExecSql(stmt);
                    done = true;
                }

                if (GenXmlFunctions.GetField(rpData, "clearpropertiesonly") == "True")
                {
                    stmt = "delete from " + dbOwner + "[" + objQual + "NBrightBuy] where PortalId = " + PortalId.ToString("") + " and typecode = 'CATEGORY' and [XMLData].value('(genxml/dropdownlist/ddlgrouptype)[1]','nvarchar(max)') != 'cat' ";
                    objCtrl.ExecSql(stmt);
                    stmt = "delete from " + dbOwner + "[" + objQual + "NBrightBuy] where PortalId = " + PortalId.ToString("") + " and typecode = 'CATEGORYLANG' and ParentItemId not in (Select itemid from " + dbOwner + "[" + objQual + "NBrightBuy] where PortalId = " + PortalId.ToString("") + " and typecode = 'CATEGORY') ";
                    objCtrl.ExecSql(stmt);
                    done = true;
                }


                if (GenXmlFunctions.GetField(rpData, "clearproperties") == "True")
                {
                    stmt = "delete from " + dbOwner + "[" + objQual + "NBrightBuy] where PortalId = " + PortalId.ToString("") + " and typecode = 'CATEGORY' and [XMLData].value('(genxml/dropdownlist/ddlgrouptype)[1]','nvarchar(max)') != 'cat' ";
                    objCtrl.ExecSql(stmt);
                    stmt = "delete from " + dbOwner + "[" + objQual + "NBrightBuy] where PortalId = " + PortalId.ToString("") + " and typecode = 'CATEGORYLANG' and ParentItemId not in (Select itemid from " + dbOwner + "[" + objQual + "NBrightBuy] where PortalId = " + PortalId.ToString("") + " and typecode = 'CATEGORY') ";
                    objCtrl.ExecSql(stmt);

                    stmt = "delete from " + dbOwner + "[" + objQual + "NBrightBuy] where PortalId = " + PortalId.ToString("") + " and typecode = 'GROUP' ";
                    objCtrl.ExecSql(stmt);
                    stmt = "delete from " + dbOwner + "[" + objQual + "NBrightBuy] where PortalId = " + PortalId.ToString("") + " and typecode = 'GROUPLANG' ";
                    objCtrl.ExecSql(stmt);

                    done = true;
                }

                if (GenXmlFunctions.GetField(rpData, "clearorders") == "True")
                {
                    stmt = "delete from " + dbOwner + "[" + objQual + "NBrightBuy] where PortalId = " + PortalId.ToString("") + " and typecode = 'ORDER' ";
                    objCtrl.ExecSql(stmt);
                    done = true;
                }

                DataCache.ClearCache();

                if (done) NBrightBuyUtils.SetNotfiyMessage(ModuleId, "deletecompleted", NotifyCode.ok);

            }
            else
            {
                NBrightBuyUtils.SetNotfiyMessage(ModuleId, "nopin", NotifyCode.fail);
            }

        }

        private void PurgeCarts()
        {
            var objCtrl = new NBrightBuyController();
            var objQual = DotNetNuke.Data.DataProvider.Instance().ObjectQualifier;
            var dbOwner = DotNetNuke.Data.DataProvider.Instance().DatabaseOwner;
            if (Utils.IsNumeric(GenXmlFunctions.GetField(rpData, "purgecartsdays")))
            {
                var days = Convert.ToInt32(GenXmlFunctions.GetField(rpData, "purgecartsdays"));
                var d = DateTime.Now.AddDays(days * -1);
                var strDate = d.ToString("s");
                var stmt = "";
                stmt = "delete from " + dbOwner + "[" + objQual + "NBrightBuy] where PortalId = " + PortalId.ToString("") + " and typecode = 'CART' and ModifiedDate < '" + strDate + "' ";
                objCtrl.ExecSql(stmt);
            }
        }

        private string RunCustomProvider()
        {
            try
            {
                var assembly = GenXmlFunctions.GetField(rpData, "providerassembly");
                var namespaceclass = GenXmlFunctions.GetField(rpData, "providernamespaceclass");
                var paramdata = GenXmlFunctions.GetField(rpData, "paramdata");
                ObjectHandle handle = null;
                handle = Activator.CreateInstance(assembly, namespaceclass);
                if (handle != null)
                {
                    var prov = (CustomActionInterface)handle.Unwrap();
                    if (prov != null)
                    {
                        prov.Run(paramdata);
                    }
                }
            }
            catch (Exception e)
            {
                return "ERROR";
            }
            return "";
        }

        private void ResetLanguage()
        {
            var pass = GenXmlFunctions.GetField(rpData, "txtclearpass");
            if (pass == StoreSettings.Current.Get("adminpin") && pass != "")
            {
            var languagetoreset = GenXmlFunctions.GetField(rpData, "languagetoreset");
            var languageresetto = GenXmlFunctions.GetField(rpData, "languageresetto");
                if (languagetoreset != "" && languageresetto != languagetoreset)
                {
                    var objCtrl = new NBrightBuyController();

                    var l = objCtrl.GetDataList(PortalId, -1, "PRD", "", Utils.GetCurrentCulture(), "", "");
                    foreach (var i in l)
                    {
                        var prdData = ProductUtils.GetProductData(i.ItemID, languagetoreset);
                        prdData.ResetLanguage(languageresetto);
                    }

                    l = objCtrl.GetDataList(PortalId, -1, "CATEGORY", "", Utils.GetCurrentCulture(), "", "");
                    foreach (var i in l)
                    {
                        var catData = CategoryUtils.GetCategoryData(i.ItemID, languagetoreset);
                        catData.ResetLanguage(languageresetto);
                    }

                    DataCache.ClearCache();

                    NBrightBuyUtils.SetNotfiyMessage(ModuleId, "completed", NotifyCode.ok);

                }
            }
            else
            {
                NBrightBuyUtils.SetNotfiyMessage(ModuleId, "nopin", NotifyCode.fail);
            }
        }


        private void ResetPortalMenu()
        {
            var pass = GenXmlFunctions.GetField(rpData, "txtclearpass");
            if (pass == StoreSettings.Current.Get("adminpin") && pass != "")
            {
                DataCache.ClearCache();

                //var pi = new PluginData(PortalId);
                //pi.RemovePortalLevel();

                NBrightBuyUtils.SetNotfiyMessage(ModuleId, "completed", NotifyCode.ok);
            }
            else
            {
                NBrightBuyUtils.SetNotfiyMessage(ModuleId, "nopin", NotifyCode.fail);
            }
        }

        /// <summary>
        /// Remove all unsed images.  (DELETE any NBS images NOT linked to Categories or Products)
        /// </summary>
        private void PurgeImages()
        {
            var pass = GenXmlFunctions.GetField(rpData, "txtclearpass");
            if (pass == StoreSettings.Current.Get("adminpin") && pass != "")
            {

                var objCtrl = new NBrightBuyController();
                var imgdblist = new List<string>();

                // get DB filenames
                var prdlist = objCtrl.GetList(PortalId, -1, "PRD");
                foreach (var nbi in prdlist)
                {
                    var nodlist = nbi.XMLDoc.SelectNodes("genxml/imgs/*");
                    if (nodlist != null)
                    {
                        foreach (XmlNode nod in nodlist)
                        {
                            var pnod = nod.SelectSingleNode("hidden/imagepath");
                            if (pnod != null)
                            {
                                var fname = Path.GetFileName(pnod.InnerText);
                                if (fname != "" && !imgdblist.Contains(fname)) imgdblist.Add(fname);
                            }
                        }
                    }
                }
                var itemlist = objCtrl.GetList(PortalId, -1, "CATEGORY");
                foreach (var nbi in itemlist)
                {
                    var p = nbi.GetXmlProperty("genxml/hidden/imagepath");
                    if (p != "")
                    {
                        imgdblist.Add(Path.GetFileName(p));
                    }
                }

                // get fileystem filenames and remove if not in DB.
                var filelist = Directory.GetFiles(StoreSettings.Current.FolderImagesMapPath);
                foreach (var f in filelist)
                {
                    if (!imgdblist.Contains(Path.GetFileName(f)))
                    {
                        // delete img
                        File.Delete(f);
                    }
                }
                NBrightBuyUtils.SetNotfiyMessage(ModuleId, "completed", NotifyCode.ok);
            }
            else
            {
                NBrightBuyUtils.SetNotfiyMessage(ModuleId, "nopin", NotifyCode.fail);
            }

        }

    }

}
