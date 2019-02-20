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
using System.Xml;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
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
    public partial class Import : NBrightBuyAdminBase
    {

        private Dictionary<int, int> _recordXref;
        private Dictionary<int,string> _productList;

        override protected void OnInit(EventArgs e)
        {
            base.OnInit(e);

            try
            {


                #region "load templates"

                var t1 = "import.html";

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
                base.DoDetail(rpData);
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
                case "import":
                    param[0] = "";
                    var importXML = GenXmlFunctions.GetGenXml(rpData, "", StoreSettings.Current.FolderUploadsMapPath);
                    var nbi = new NBrightInfo(false);
                    nbi.XMLData = importXML;
                    _recordXref = new Dictionary<int, int>();
                    _productList = new Dictionary<int,string>();
                    DoImport(nbi);
                    DoImportImages(nbi);
                    DoImportDocs(nbi);

                    /*
                     * Validation removed as it impact the performance of the import.
                     * A validation can be done by the store manager through the UI Admin Tools after the import.
                     */
//                    Validate();

                    NBrightBuyUtils.RemoveModCachePortalWide(PortalId);
                    NBrightBuyUtils.SetNotfiyMessage(ModuleId, "completed", NotifyCode.ok);
                    Response.Redirect(NBrightBuyUtils.AdminUrl(TabId, param), true);
                    break;
                case "cancel":
                    param[0] = "";
                    Response.Redirect(NBrightBuyUtils.AdminUrl(TabId, param), true);
                    break;
            }

        }

        #endregion

        private void DoImport(NBrightInfo nbi)
        {
            var fname = StoreSettings.Current.FolderUploadsMapPath + "\\" + nbi.GetXmlProperty("genxml/hidden/hiddatafile");
            if (System.IO.File.Exists(fname))
            {

                var xmlFile = new XmlDocument();
                xmlFile.Load(fname);

                if (GenXmlFunctions.GetField(rpData, "importproducts") == "True")
                {
                    ImportRecord(xmlFile,"PRD");
                    ImportRecord(xmlFile, "PRDLANG");

                    // import any entitytype provider data
                    // This is data created by plugins into the NBS data tables.
                    var pluginData = new PluginData(PortalSettings.Current.PortalId);
                    var provList = pluginData.GetEntityTypeProviders();
                    foreach (var prov in provList)
                    {
                        var entityprov = EntityTypeInterface.Instance(prov.Key);
                        if (entityprov != null)
                        {
                            ImportRecord(xmlFile, entityprov.GetEntityTypeCode());
                            ImportRecord(xmlFile, entityprov.GetEntityTypeCodeLang());
                        }
                    }

                    ImportRecord(xmlFile, "PRDXREF");
                    ImportRecord(xmlFile, "USERPRDXREF");
                }

                if (GenXmlFunctions.GetField(rpData, "importcategories") == "True")
                {
                    ImportRecord(xmlFile, "CATEGORY");
                    ImportRecord(xmlFile, "CATEGORYLANG");
                }

                if (GenXmlFunctions.GetField(rpData, "importcategories") == "True" && GenXmlFunctions.GetField(rpData, "importproducts") == "True")
                {
                    ImportRecord(xmlFile, "CATCASCADE");
                    ImportRecord(xmlFile, "CATXREF");
                }

                if (GenXmlFunctions.GetField(rpData, "importproperties") == "True")
                {
                    ImportRecord(xmlFile, "GROUP");
                    ImportRecord(xmlFile, "GROUPLANG");
                }

                if (GenXmlFunctions.GetField(rpData, "importsettings") == "True")
                {
                    ImportRecord(xmlFile, "SETTINGS");
                }

                if (GenXmlFunctions.GetField(rpData, "importorders") == "True")
                {
                    ImportRecord(xmlFile, "ORDER");
                }

                RelinkNewIds();
            }
        }

        private void DoImportImages(NBrightInfo nbi)
        {
            var fname = StoreSettings.Current.FolderUploadsMapPath + "\\" + nbi.GetXmlProperty("genxml/hidden/hidimagefile");
            if (System.IO.File.Exists(fname)) DnnUtils.UnZip(fname, StoreSettings.Current.FolderImagesMapPath);
            Utils.DeleteSysFile(fname);
        }

        private void DoImportDocs(NBrightInfo nbi)
        {
            var fname = StoreSettings.Current.FolderUploadsMapPath + "\\" + nbi.GetXmlProperty("genxml/hidden/hiddocsfile");
            if (System.IO.File.Exists(fname)) DnnUtils.UnZip(fname, StoreSettings.Current.FolderDocumentsMapPath);
            Utils.DeleteSysFile(fname);
        }

        private void Validate()
        {
            foreach (var r in _productList)
            {
                // if product then validate the data.
                var prodData = ProductUtils.GetProductData(r.Key, StoreSettings.Current.EditLanguage,true, r.Value);
                if (prodData.Exists)
                {
                    prodData.Validate();
                    prodData.Save();
                }
            }
        }

        private void ImportRecord(XmlDocument xmlFile, String typeCode, Boolean updaterecordsbyref = true)
        {
            var nodList = xmlFile.SelectNodes("root/item[./typecode='" + typeCode + "']");
            if (nodList != null)
            {
                foreach (XmlNode nod in nodList)
                {
                    var nbi = new NBrightInfo();
                    nbi.FromXmlItem(nod.OuterXml);
                    var olditemid = nbi.ItemID;

                    // check to see if we have a new record or updating a existing one, use the ref field to find out.
                    nbi.ItemID = -1;
                    if (nbi.PortalId >= 0) nbi.PortalId = PortalId; // shared products have -1 portalid

                    if (typeCode == "PRD" && updaterecordsbyref)
                    {
                        var itemref = nbi.GetXmlProperty("genxml/textbox/txtproductref");
                        if (itemref != "")
                        {
                            var l = ModCtrl.GetList(nbi.PortalId, -1, "PRD", " and NB1.XmlData.value('(genxml/textbox/txtproductref)[1]','nvarchar(max)') = '" + itemref.Replace("'", "''") + "' ");
                            if (l.Count > 0) nbi.ItemID = l[0].ItemID;
                        }
                    }
                    if (typeCode == "PRDLANG")
                    {
                        if (_recordXref.ContainsKey(nbi.ParentItemId))
                        {
                            var l = ModCtrl.GetList(nbi.PortalId, -1, "PRDLANG", " and NB1.parentitemid = '" + _recordXref[nbi.ParentItemId].ToString("") + "' and NB1.Lang = '" + nbi.Lang + "'");
                            if (l.Count > 0) nbi.ItemID = l[0].ItemID;
                            nbi.ParentItemId = _recordXref[nbi.ParentItemId];
                        }
                    }



                    // import any entitytype provider data
                    // This is data created by plugins into the NBS data tables.
                    var pluginData = new PluginData(PortalSettings.Current.PortalId);
                    var provList = pluginData.GetEntityTypeProviders();
                    foreach (var prov in provList)
                    {
                        var entityprov = EntityTypeInterface.Instance(prov.Key);
                        if (entityprov != null)
                        {
                            if (typeCode == entityprov.GetEntityTypeCode() && updaterecordsbyref)
                            {
                                var itemref = nbi.GetXmlProperty("genxml/textbox/txtproductref");
                                if (itemref != "")
                                {
                                    var l = ModCtrl.GetList(nbi.PortalId, -1, entityprov.GetEntityTypeCode(), " and NB3.ProductRef = '" + itemref.Replace("'", "''") + "' ");
                                    if (l.Count > 0) nbi.ItemID = l[0].ItemID;
                                }
                            }
                            if (typeCode == entityprov.GetEntityTypeCodeLang())
                            {
                                if (_recordXref.ContainsKey(nbi.ParentItemId))
                                {
                                    var l = ModCtrl.GetList(nbi.PortalId, -1, entityprov.GetEntityTypeCodeLang(), " and NB1.parentitemid = '" + _recordXref[nbi.ParentItemId].ToString("") + "' and NB1.Lang = '" + nbi.Lang + "'");
                                    if (l.Count > 0) nbi.ItemID = l[0].ItemID;
                                    nbi.ParentItemId = _recordXref[nbi.ParentItemId];
                                }
                            }

                        }
                    }


                    if ((typeCode == "PRDXREF" || typeCode == "CATXREF" || typeCode == "CATCASCADE"))
                    {
                        if (_recordXref.ContainsKey(nbi.XrefItemId)) nbi.XrefItemId = _recordXref[nbi.XrefItemId];
                        if (_recordXref.ContainsKey(nbi.ParentItemId)) nbi.ParentItemId = _recordXref[nbi.ParentItemId];

                        var l = ModCtrl.GetList(nbi.PortalId, -1, typeCode, " AND nb1.XrefItemId = " + nbi.XrefItemId + " AND nb1.ParentItemId=" + nbi.ParentItemId);
                        if (l.Count > 0) return;
                    }

                    if (typeCode == "USERPRDXREF")
                    {
                        var u = UserController.GetUserByName(nbi.PortalId, nbi.TextData);
                        if (u != null)
                        {
                            if (_recordXref.ContainsKey(nbi.ParentItemId)) nbi.ParentItemId = _recordXref[nbi.ParentItemId];
                            if (_recordXref.ContainsKey(nbi.XrefItemId)) nbi.UserId = u.UserID;

                            var l = ModCtrl.GetList(nbi.PortalId, -1, "USERPRDXREF", " AND nb1.UserId = " + nbi.UserId + " AND nb1.ParentItemId=" + nbi.ParentItemId);
                            if (l.Count > 0) return;
                        }
                    }


                    if (typeCode == "CATEGORY" && updaterecordsbyref)
                    {
                        var itemref = nbi.GetXmlProperty("genxml/textbox/txtcategoryref");
                        if (itemref != "")
                        {
                            var l = ModCtrl.GetList(nbi.PortalId, -1, "CATEGORY", " and [XMLData].value('(genxml/textbox/txtcategoryref)[1]','nvarchar(max)') = '" + itemref.Replace("'", "''") + "' ");
                            if (l.Count > 0) nbi.ItemID = l[0].ItemID;
                        }
                    }
                    if (typeCode == "CATEGORYLANG")
                    {
                        if (_recordXref.ContainsKey(nbi.ParentItemId))
                        {
                            var l = ModCtrl.GetList(nbi.PortalId, -1, "CATEGORYLANG", " and NB1.parentitemid = '" + _recordXref[nbi.ParentItemId].ToString("") + "' and NB1.Lang = '" + nbi.Lang + "'");
                            if (l.Count > 0) nbi.ItemID = l[0].ItemID;
                            nbi.ParentItemId = _recordXref[nbi.ParentItemId];
                        }
                    }
                    if (typeCode == "GROUP" && updaterecordsbyref)
                    {
                        var itemref = nbi.GetXmlProperty("genxml/textbox/groupref");
                        if (itemref != "")
                        {
                            var l = ModCtrl.GetList(nbi.PortalId, -1, "GROUP", " and [XMLData].value('(genxml/textbox/groupref)[1]','nvarchar(max)') = '" + itemref.Replace("'", "''") + "' ");
                            if (l.Count > 0) nbi.ItemID = l[0].ItemID;
                        }
                    }
                    if (typeCode == "GROUPLANG")
                    {
                        if (_recordXref.ContainsKey(nbi.ParentItemId))
                        {
                            var l = ModCtrl.GetList(nbi.PortalId, -1, "GROUPLANG", " and NB1.parentitemid = '" + _recordXref[nbi.ParentItemId].ToString("") + "' and NB1.Lang = '" + nbi.Lang + "'");
                            if (l.Count > 0) nbi.ItemID = l[0].ItemID;
                            nbi.ParentItemId = _recordXref[nbi.ParentItemId];
                        }
                    }
                    if (typeCode == "SETTINGS") // the setting exported are only the portal settings, not module.  So always update and don;t create new.
                    {
                        var l = ModCtrl.GetList(nbi.PortalId, 0, "SETTINGS", " and NB1.GUIDKey = 'NBrightBuySettings' ");
                        if (l.Count > 0) nbi.ItemID = l[0].ItemID;
                    }
                    //NOTE: if ORDERS are imported, we expect those to ALWAYS be new records, we don't want to delete any validate orders in this import.

                    // NOTE: we expect the records to be done in typecode order so we know parent and xref itemids.

                    var newitemid = ModCtrl.Update(nbi);
                    if (newitemid > 0) _recordXref.Add(olditemid, newitemid);
                    if (typeCode == "PRD" && !_productList.ContainsKey(newitemid)) _productList.Add(newitemid, typeCode);

                    // Add any provider data types
                    foreach (var prov in provList)
                    {
                        var entityprov = EntityTypeInterface.Instance(prov.Key);
                        if (entityprov != null)
                        {
                            if (typeCode == entityprov.GetEntityTypeCode()) _productList.Add(newitemid, typeCode);
                        }
                    }

                }


            }
        }

        private void RelinkNewIds()
        {
            var l = ModCtrl.GetList(PortalId, -1, "CATEGORY");
            foreach (var i in l)
            {
                if (_recordXref.ContainsKey(i.ParentItemId))
                {
                    i.ParentItemId = _recordXref[i.ParentItemId];
                    ModCtrl.Update(i);
                }
                var pcatid = i.GetXmlProperty("genxml/dropdownlist/ddlparentcatid");
                if (Utils.IsNumeric(pcatid) && pcatid != "0")
                {
                    if (_recordXref.ContainsKey(Convert.ToInt32(pcatid)))
                    {
                        i.SetXmlProperty("genxml/dropdownlist/ddlparentcatid", _recordXref[Convert.ToInt32(pcatid)].ToString());
                        ModCtrl.Update(i);
                    }                    
                }
            }

            l = ModCtrl.GetList(PortalId, -1, "CATCASCADE");
            foreach (var i in l)
            {
                UpdateXrefRecords(i);
            }

            l = ModCtrl.GetList(PortalId, -1, "CATXREF");
            foreach (var i in l)
            {
                UpdateXrefRecords(i);
            }

            l = ModCtrl.GetList(PortalId, -1, "PRDXREF");
            foreach (var i in l)
            {
                UpdateXrefRecords(i);
            }

        }


        private void UpdateXrefRecords(NBrightInfo nbi)
        {
            // Get new parentitemid  
            if (_recordXref.ContainsKey(nbi.ParentItemId)) nbi.ParentItemId = _recordXref[nbi.ParentItemId];
            // Get new xrefitemid  
            if (_recordXref.ContainsKey(nbi.XrefItemId)) nbi.XrefItemId = _recordXref[nbi.XrefItemId];
            // if we have a xref record update the guidkey
            if (nbi.ParentItemId > 0 && nbi.XrefItemId > 0)
            {
                nbi.GUIDKey = nbi.XrefItemId.ToString("") + "x" + nbi.ParentItemId.ToString("");
                //if we already have a record with this xref guid then we don;t need to update
                ModCtrl.Update(nbi);
            }

        }
    }

}
