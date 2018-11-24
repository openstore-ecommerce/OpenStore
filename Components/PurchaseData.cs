using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using System.Xml;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Components.Interfaces;


namespace Nevoweb.DNN.NBrightBuy.Components
{
    public class PurchaseData
    {
        private int _entryId;
        public NBrightInfo PurchaseInfo;
        private List<NBrightInfo> _itemList;
        private int _userId = -1;

        public String PurchaseTypeCode;
        public int PortalId;


        /// <summary>
        /// Save Purchase record
        /// </summary>
        /// <param name="copyrecord">Copy this data as a new record in the DB with a new id</param>
        /// <returns></returns>
        public int SavePurchaseData(Boolean copyrecord = false )
        {
            if (copyrecord)
            {
                _entryId = -1;
                PurchaseInfo.SetXmlProperty("genxml/audit", ""); // remove audit
            }

            // langauge
            if (Lang == "")
            {
                Lang = Utils.GetCurrentCulture();
            }
            ClientLang = Lang;

            var strXml = "<items>";
            foreach (var info in _itemList)
            {
                // remove injected group header record for the product
                if (!info.GetXmlPropertyBool("genxml/groupheader")) strXml += info.XMLData;
            }
            strXml += "</items>";
            PurchaseInfo.RemoveXmlNode("genxml/items");
            PurchaseInfo.AddXmlNode(strXml, "items", "genxml");

            var modCtrl = new NBrightBuyController();
            PurchaseInfo.ItemID = _entryId;
            PurchaseInfo.PortalId = PortalId;
            PurchaseInfo.ModuleId = -1;
            PurchaseInfo.TypeCode = PurchaseTypeCode;
            if (UserController.Instance.GetCurrentUserInfo().UserID != -1)  // This might be updated from out of context (payment provider)
            {
                if (UserId != UserController.Instance.GetCurrentUserInfo().UserID && EditMode == "") UserId = UserController.Instance.GetCurrentUserInfo().UserID;
                PurchaseInfo.UserId = UserId;
                if (PurchaseTypeCode=="CART") PurchaseInfo.GUIDKey = UserId.ToString("");
                if (EditMode == "" && !string.IsNullOrEmpty(UserController.Instance.GetCurrentUserInfo().Profile.PreferredLocale))
                {
                    ClientLang = UserController.Instance.GetCurrentUserInfo().Profile.PreferredLocale;
                }
            }

            // save the product refs of the order to an XML node, so we can search for product ref in the BO Order Admin.
            var productrefs = "";
            foreach (var i in GetCartItemList())
            {
                productrefs += i.GetXmlProperty("genxml/productxml/genxml/textbox/txtproductref") + ",";
            }
            PurchaseInfo.SetXmlProperty("genxml/productrefs", productrefs);

            if (PurchaseInfo.TypeCode != null) // if we're using this class to build cart in memory for procesisng only, don;t save to DB.
            {
                AddPurchasedDocs(); // only update the userdata if we're saving data.
                _entryId = modCtrl.Update(PurchaseInfo);
                NBrightBuyUtils.ProcessEventProvider(EventActions.AfterSavePurchaseData, PurchaseInfo);
            }

            return _entryId;
        }

        public void TurnOffEditMode()
        {
            var modCtrl = new NBrightBuyController();
            EditMode = "X"; // make X so we keep any userid
            _entryId = modCtrl.Update(PurchaseInfo);
        }

        /// <summary>
        /// EditMode is a flag to indicate the update process of the cart/order R=Re-order, C=Create New Order for CLient, E=Edit order for client, {Empty}=Normal front office cart
        /// </summary>
        public String EditMode
        {
            get
            {
                return PurchaseInfo.GetXmlProperty("genxml/carteditmode");
            }
            set
            {
                PurchaseInfo.SetXmlProperty("genxml/carteditmode", value);
            }
        }

        public String Lang
        {
            get
            {
                return PurchaseInfo.GetXmlProperty("genxml/lang");
            }
            set
            {
                PurchaseInfo.SetXmlProperty("genxml/lang", value);
            }
        }

        public String ClientLang
        {
            get
            {
                if (PurchaseInfo.GetXmlProperty("genxml/clientlang") == "") return Lang;
                return PurchaseInfo.GetXmlProperty("genxml/clientlang");
            }
            set
            {
                PurchaseInfo.SetXmlProperty("genxml/clientlang", value);
            }
        }

        public int UserId
        {
            get { return _userId; }
            set { _userId = value; }
        }

        #region "Audit data"

        public void AddAuditMessage(String msg,String type, String username,String showtouser,String emailsubject = "")
        {
            if (msg != "" || emailsubject != "")
            {
                if (PurchaseInfo.XMLDoc.SelectSingleNode("genxml/audit") == null) PurchaseInfo.AddSingleNode("audit", "", "genxml");
                var strXml = "<genxml><date>" + DateTime.Now.ToString("s") + "</date>";
                strXml += "<showtouser>" + showtouser + "</showtouser>";
                strXml += "<type>" + type + "</type>";
                if (type == "email")
                {
                    strXml += "<emailsubject>" + emailsubject + "</emailsubject>";                    
                }
                strXml += "<username>" + username + "</username>";
                strXml += "<msg><![CDATA[" + msg + "]]></msg></genxml>";
                PurchaseInfo.AddXmlNode(strXml, "genxml", "genxml/audit");
            }

        }

        public void AddAuditStatusChange(String newstatuscode, String username)
        {
            if (newstatuscode != "")
            {
                if (PurchaseInfo.XMLDoc.SelectSingleNode("genxml/audit") == null) PurchaseInfo.AddSingleNode("audit", "", "genxml");
                var strXml = "<genxml><date>" + DateTime.Now.ToString("s") + "</date><status>" + newstatuscode + "</status>";
                strXml += "<username>" + username + "</username>";
                strXml += "<showtouser>True</showtouser>";
                strXml += "</genxml>";
                PurchaseInfo.AddXmlNode(strXml, "genxml", "genxml/audit");
            }
        }

        #endregion

        #region "Stock Control"
        /// <summary>
        /// Save transent model qty to cache.
        /// </summary>
        public void SaveModelTransQty()
        {
            //update trans stock levels.
            var itemList = GetCartItemList();
            foreach (var cartItemInfo in itemList)
            {
                var modelid = cartItemInfo.GetXmlProperty("genxml/modelid");
                var qty = cartItemInfo.GetXmlPropertyDouble("genxml/qty");
                var prdid = cartItemInfo.GetXmlPropertyInt("genxml/productid");
                var prd = ProductUtils.GetProductData(prdid, Utils.GetCurrentCulture());
                if (prd.Exists)
                {
                    var model = prd.GetModel(modelid);
                    if (model != null && model.GetXmlPropertyBool("genxml/checkbox/chkstockon")) prd.UpdateModelTransQty(modelid, _entryId, qty);
                }
            }
        }

        /// <summary>
        /// Release transient qty for this cart
        /// </summary>
        public void ReleaseModelTransQty()
        {
            //update trans stock levels.
            var itemList = GetCartItemList();
            foreach (var cartItemInfo in itemList)
            {
                var modelid = cartItemInfo.GetXmlProperty("genxml/modelid");
                var qty = cartItemInfo.GetXmlPropertyDouble("genxml/qty");
                var prdid = cartItemInfo.GetXmlPropertyInt("genxml/productid");

                var prd = ProductUtils.GetProductData(prdid, Utils.GetCurrentCulture());
                if (prd.Exists)
                {
                    var model = prd.GetModel(modelid);
                    if (model.GetXmlPropertyBool("genxml/checkbox/chkstockon")) prd.ReleaseModelTransQty(modelid, _entryId, qty);
                }
            }
        }
        /// <summary>
        /// Apply Transient qty for this cart onto the model
        /// </summary>
        public void ApplyModelTransQty()
        {
            //update trans stock levels.
            var itemList = GetCartItemList();
            foreach (var cartItemInfo in itemList)
            {
                var modelid = cartItemInfo.GetXmlProperty("genxml/modelid");
                var qty = cartItemInfo.GetXmlPropertyDouble("genxml/qty");
                var prdid = cartItemInfo.GetXmlPropertyInt("genxml/productid");

                var prd = ProductUtils.GetProductData(prdid, Utils.GetCurrentCulture());
                if (prd.Exists)
                {
                    var model = prd.GetModel(modelid);
                    if (model.GetXmlPropertyBool("genxml/checkbox/chkstockon")) prd.ApplyModelTransQty(modelid, _entryId, qty);
                }
            }
        }

        #endregion

        #region "base methods"

        public String AddAjaxItem(NBrightInfo ajaxInfo, NBrightInfo nbSettings, Boolean debugMode = false)
        {
            Lang = NBrightBuyUtils.SetContextLangauge(ajaxInfo);
            var strproductid = ajaxInfo.GetXmlProperty("genxml/hidden/productid");
            // Get ModelID
            var modelidlist = new List<String>();
            // *************************************
            // Do Model List return
            var qtylist = new Dictionary<String, String>();
            var nodList = ajaxInfo.XMLDoc.SelectNodes("genxml/textbox/*");
            if (nodList != null && nodList.Count > 0)
            {
                foreach (XmlNode nod in nodList)
                {
                    if (nod.Name.StartsWith("selectedmodelqty")) // model list qty textbox has modelid appedix.
                    {
                        if (Utils.IsNumeric(nod.InnerText))
                        {
                            var fieldid = nod.Name.Replace("selectedmodelqty", "selectedmodelid"); //link to modelid in hidden field
                            var strmodelId = ajaxInfo.GetXmlProperty("genxml/hidden/" + fieldid);
                            var strqtyId = nod.InnerText;
                            if (Utils.IsNumeric(strqtyId))
                            {
                                modelidlist.Add(strmodelId);
                                qtylist.Add(strmodelId, strqtyId);
                            }
                        }
                    }
                }
            }
            // *************************************
            // do dropdown and radio return
            if (qtylist.Count == 0)
            {
                var strmodelId = ajaxInfo.GetXmlProperty("genxml/radiobuttonlist/rblmodelsel");
                if (strmodelId == "") strmodelId = ajaxInfo.GetXmlProperty("genxml/dropdownlist/ddlmodelsel");
                if (strmodelId == "") strmodelId = ajaxInfo.GetXmlProperty("genxml/hidden/modeldefault");
                if (strmodelId == "")
                {
                    var p = ProductUtils.GetProductData(strproductid,Lang);
                    strmodelId = p.Models[0].GetXmlProperty("genxml/hidden/modelid");
                }
                modelidlist.Add(strmodelId);
                var strqtyId = ajaxInfo.GetXmlProperty("genxml/textbox/selectedaddqty");
                if (!Utils.IsNumeric(strqtyId)) strqtyId = "1";
                qtylist.Add(strmodelId, strqtyId);
            }

            var strRtn = "";
            foreach (var m in modelidlist)
            {
                var numberofmodels = 0; // use additemqty field to add multiple items
                var additemqty = ajaxInfo.GetXmlProperty("genxml/textbox/additemqty");
                if (Utils.IsNumeric(additemqty))
                    numberofmodels = Convert.ToInt32(additemqty); // zero should be allowed on add all to basket option.
                else
                    numberofmodels = 1; // if we have no numeric, assume we need to add it

                var replaceIndex = -1;
                if (nbSettings.GetXmlPropertyBool("genxml/newcartitems")) // always create new items for cart. (Do not replace)
                {
                    replaceIndex = -2;
                }
                for (var i = 1; i <= numberofmodels; i++)
                {
                    strRtn += AddSingleItem(strproductid, m, qtylist[m], ajaxInfo, debugMode, replaceIndex);
                }
            }
            return strRtn;
        }


        public String AddSingleItem(String strproductid, String strmodelId, String strqtyId, NBrightInfo objInfoIn, Boolean debugMode = false, int replaceIndex = -1)
        {
            Lang = NBrightBuyUtils.SetContextLangauge(objInfoIn);

            if (!Utils.IsNumeric(strqtyId) || Convert.ToInt32(strqtyId) <= 0) return "";

            if (StoreSettings.Current.DebugModeFileOut) objInfoIn.XMLDoc.Save(PortalSettings.Current.HomeDirectoryMapPath + "debug_addtobasket.xml");

            var objInfo = new NBrightInfo();
            objInfo.XMLData = "<genxml></genxml>";

            // get productid
            if (Utils.IsNumeric(strproductid))
            {
                var itemcode = ""; // The itemcode var is used to decide if a cart item is new or already existing in the cart.
                var productData = ProductUtils.GetProductData(Convert.ToInt32(strproductid), Lang);

                if (productData.Info == null) return ""; // we may have a invalid productid that has been saved by a cookie, but since has been deleted.

                var modelInfo = productData.GetModel(strmodelId);
                if (modelInfo == null) return ""; // no valid model

                objInfo.AddSingleNode("productid", strproductid, "genxml");
                itemcode += strproductid + "-";

                objInfo.AddSingleNode("modelid", strmodelId, "genxml");
                itemcode += strmodelId + "-";

                // Get Qty
                objInfo.AddSingleNode("qty", strqtyId, "genxml");

                #region "Get model and product data"

                objInfo.AddSingleNode("productname", productData.Info.GetXmlPropertyRaw("genxml/lang/genxml/textbox/txtproductname"), "genxml");
                objInfo.AddSingleNode("summary", productData.Info.GetXmlPropertyRaw("genxml/lang/genxml/textbox/txtsummary"), "genxml");

                objInfo.AddSingleNode("modelref", modelInfo.GetXmlPropertyRaw("genxml/textbox/txtmodelref"), "genxml");
                objInfo.AddSingleNode("modeldesc", modelInfo.GetXmlPropertyRaw("genxml/lang/genxml/textbox/txtmodelname"), "genxml");
                objInfo.AddSingleNode("modelextra", modelInfo.GetXmlPropertyRaw("genxml/lang/genxml/textbox/txtextra"), "genxml");
                objInfo.AddSingleNode("unitcost", modelInfo.GetXmlPropertyRaw("genxml/textbox/txtunitcost"), "genxml");
                objInfo.AddSingleNode("dealercost", modelInfo.GetXmlPropertyRaw("genxml/textbox/txtdealercost"), "genxml");
                objInfo.AddSingleNode("taxratecode", modelInfo.GetXmlPropertyRaw("genxml/dropdownlist/taxrate"), "genxml");
                objInfo.AddSingleNode("saleprice", modelInfo.GetXmlPropertyRaw("genxml/textbox/txtsaleprice"), "genxml");
                objInfo.AddSingleNode("basecost", modelInfo.GetXmlPropertyRaw("genxml/textbox/txtunitcost"), "genxml");

                // flag if dealer
                if (NBrightBuyUtils.IsDealer())
                    objInfo.SetXmlProperty("genxml/isdealer", "True");
                else
                    objInfo.SetXmlProperty("genxml/isdealer", "False");

                // add entitytype for validation methods
                productData.Info.SetXmlProperty("genxml/entitytypecode", productData.DataRecord.TypeCode);

                //move all product and model data into cart item, so we can display bespoke fields.
                objInfo.AddSingleNode("productxml", productData.Info.XMLData, "genxml");

                #endregion


                #region "Get option Data"

                //build option data for cart
                Double additionalCosts = 0;
                var strXmlIn = "";
                var optionDataList = new Dictionary<String, String>();
                if (objInfoIn.XMLDoc != null)
                {
                    var nodList = objInfoIn.XMLDoc.SelectNodes("genxml/textbox/*[starts-with(name(), 'optiontxt')]");
                    if (nodList != null)
                        foreach (XmlNode nod in nodList)
                        {
                            strXmlIn = "<option>";
                            var idx = nod.Name.Replace("optiontxt", "");
                            var optionid = objInfoIn.GetXmlProperty("genxml/hidden/optionid" + idx);
                            var optionInfo = productData.GetOption(optionid);
                            var optvaltext = nod.InnerText;
                            strXmlIn += "<optid>" + optionid + "</optid>";
                            strXmlIn += "<optvaltext>" + optvaltext + "</optvaltext>";
                            itemcode += optionid + "-" + Utils.GetUniqueKey() + "-";
                            strXmlIn += "<optname>" + optionInfo.GetXmlProperty("genxml/lang/genxml/textbox/txtoptiondesc") + "</optname>";
                            strXmlIn += "</option>";
                            if (!optionDataList.ContainsKey(idx)) optionDataList.Add(idx, strXmlIn);
                        }
                    nodList = objInfoIn.XMLDoc.SelectNodes("genxml/dropdownlist/*[starts-with(name(), 'optionddl')]");
                    if (nodList != null)
                        foreach (XmlNode nod in nodList)
                        {
                            strXmlIn = "<option>";
                            var idx = nod.Name.Replace("optionddl", "");
                            var optionid = objInfoIn.GetXmlProperty("genxml/hidden/optionid" + idx);
                            var optionvalueid = nod.InnerText;
                            var optionValueInfo = productData.GetOptionValue(optionid, optionvalueid);
                            var optionInfo = productData.GetOption(optionid);
                            strXmlIn += "<optid>" + optionid + "</optid>";
                            strXmlIn += "<optvalueid>" + optionvalueid + "</optvalueid>";
                            itemcode += optionid + ":" + optionvalueid + "-";
                            strXmlIn += "<optname>" + optionInfo.GetXmlProperty("genxml/lang/genxml/textbox/txtoptiondesc") + "</optname>";
                            strXmlIn += "<optvalcost>" + optionValueInfo.GetXmlPropertyRaw("genxml/textbox/txtaddedcost") + "</optvalcost>";
                            strXmlIn += "<optvaltext>" + optionValueInfo.GetXmlProperty("genxml/lang/genxml/textbox/txtoptionvaluedesc") + "</optvaltext>";
                            strXmlIn += "</option>";
                            additionalCosts += optionValueInfo.GetXmlPropertyDouble("genxml/textbox/txtaddedcost");
                            if (!optionDataList.ContainsKey(idx)) optionDataList.Add(idx, strXmlIn);
                        }
                    nodList = objInfoIn.XMLDoc.SelectNodes("genxml/checkbox/*[starts-with(name(), 'optionchk')]");
                    if (nodList != null)
                        foreach (XmlNode nod in nodList)
                        {
                                strXmlIn = "<option>";
                                var idx = nod.Name.Replace("optionchk", "");
                                var optionid = objInfoIn.GetXmlProperty("genxml/hidden/optionid" + idx);
                                var optionvalueid = nod.InnerText;
                                var optionValueInfo = productData.GetOptionValue(optionid, ""); // checkbox does not have optionvalueid
                                var optionInfo = productData.GetOption(optionid);
                                strXmlIn += "<optid>" + optionid + "</optid>";
                                strXmlIn += "<optvalueid>" + optionvalueid + "</optvalueid>";
                                itemcode += optionid + ":" + optionvalueid + "-";
                                strXmlIn += "<optname>" + optionInfo.GetXmlProperty("genxml/lang/genxml/textbox/txtoptiondesc") + "</optname>";
                                strXmlIn += "<optvalcost>" + optionValueInfo.GetXmlPropertyRaw("genxml/textbox/txtaddedcost") + "</optvalcost>";
                                strXmlIn += "<optvaltext>" + optionValueInfo.GetXmlProperty("genxml/lang/genxml/textbox/txtoptionvaluedesc") + "</optvaltext>";
                                strXmlIn += "</option>";
                                if (nod.InnerText.ToLower() == "true") additionalCosts += optionValueInfo.GetXmlPropertyDouble("genxml/textbox/txtaddedcost");
                                if (!optionDataList.ContainsKey(idx)) optionDataList.Add(idx, strXmlIn);
                        }
                }

                // we need to save the options in the same order as in product, so index works correct on the template tokens.
                var strXmlOpt = "<options>";
                for (int i = 1; i <= optionDataList.Count; i++)
			    {
			        if (optionDataList.ContainsKey(i.ToString("")))
			        {
			            strXmlOpt += optionDataList[i.ToString("")];
			        }
			    }
                strXmlOpt += "</options>";

                objInfo.AddXmlNode(strXmlOpt, "options", "genxml");

                #endregion

                //add additional costs from optionvalues (Add to both dealer and unit cost)
                if (additionalCosts > 0)
                {
                    objInfo.SetXmlPropertyDouble("genxml/additionalcosts", additionalCosts);
                    var uc = objInfo.GetXmlPropertyDouble("genxml/unitcost");
                    var dc = objInfo.GetXmlPropertyDouble("genxml/dealercost");
                    uc += additionalCosts;
                    if (dc > 0) dc += additionalCosts; // only calc dealer cost if it's > zero (active)
                    objInfo.SetXmlPropertyDouble("genxml/unitcost", uc);
                    objInfo.SetXmlPropertyDouble("genxml/dealercost", dc);
                }


                objInfo.AddSingleNode("itemcode", itemcode.TrimEnd('-'), "genxml");

                // check if we have a client file upload
                var clientfileuopload = objInfoIn.GetXmlProperty("genxml/textbox/optionfilelist") != "";

                //replace the item if it's already in the list.
                var nodItem = PurchaseInfo.XMLDoc.SelectSingleNode("genxml/items/genxml[itemcode='" + itemcode.TrimEnd('-') + "']");
                if (nodItem == null || clientfileuopload || replaceIndex == -2)
                {
                    if (replaceIndex == -2)
                    {
                        // make itemcode unique, so we create new item each time.
                        objInfo.SetXmlProperty("genxml/itemcode", objInfo.GetXmlProperty("genxml/itemcode") + Utils.GetUniqueKey());  // make sure itemcode is unique
                    }

                    #region "Client Files"

                    if (clientfileuopload)
                    {
                        // client has uploaded files, so save these to client upload folder and create link in cart data.
                        var flist = objInfoIn.GetXmlProperty("genxml/textbox/optionfilelist").TrimEnd(',');
                        var fprefix = objInfoIn.GetXmlProperty("genxml/hidden/optionfileprefix") + "_";
                        var fileXml = "<clientfiles>";
                        foreach (var f in flist.Split(','))
                        {
                            var fullName = StoreSettings.Current.FolderTempMapPath.TrimEnd(Convert.ToChar("\\")) + "\\" + fprefix + f;
                            var extension = Path.GetExtension(fullName);
                            if (File.Exists(fullName))
                            {
                                var newDocFileName = StoreSettings.Current.FolderClientUploadsMapPath.TrimEnd(Convert.ToChar("\\")) + "\\" + Guid.NewGuid() + extension;
                                File.Copy(fullName, newDocFileName, true);
                                File.Delete(fullName);
                                var docurl = StoreSettings.Current.FolderClientUploads.TrimEnd('/') + "/" + Path.GetFileName(newDocFileName);
                                fileXml += "<file>";
                                fileXml += "<mappath>" + newDocFileName + "</mappath>";
                                fileXml += "<url>" + docurl + "</url>";
                                fileXml += "<name>" + f + "</name>";
                                fileXml += "<prefix>" + fprefix + "</prefix>";
                                fileXml += "</file>";
                            }
                        }
                        fileXml += "</clientfiles>";
                        objInfo.AddXmlNode(fileXml, "clientfiles", "genxml");
                    }

                    #endregion

                    if (replaceIndex >= 0 && replaceIndex < _itemList.Count)
                        _itemList[replaceIndex] = objInfo; //replace
                    else
                        _itemList.Add(objInfo); //add as new item
                }
                else
                {
                    //replace item
                    var qty = nodItem.SelectSingleNode("qty");
                    if (qty != null && Utils.IsNumeric(qty.InnerText) && Utils.IsNumeric(strqtyId))
                    {
                        var userqtylimit = objInfoIn.GetXmlPropertyInt("genxml/hidden/userqtylimit");
                        if (userqtylimit > 0 && Convert.ToInt32(qty.InnerText) >= userqtylimit) return "";

                        //add new qty and replace item
                        PurchaseInfo.RemoveXmlNode("genxml/items/genxml[itemcode='" + itemcode.TrimEnd('-') + "']");
                        _itemList = GetCartItemList();
                        var newQty = Convert.ToString(Convert.ToInt32(qty.InnerText) + Convert.ToInt32(strqtyId));
                        objInfo.SetXmlProperty("genxml/qty", newQty, TypeCode.String, false);
                        _itemList.Add(objInfo);
                    }
                }

                //add nodes for any fields that might exist in cart template
                objInfo.AddSingleNode("textbox", "", "genxml");
                objInfo.AddSingleNode("dropdownlist", "", "genxml");
                objInfo.AddSingleNode("radiobuttonlist", "", "genxml");
                objInfo.AddSingleNode("checkbox", "", "genxml");

                SavePurchaseData(); // need to save after each add, so it exists in data when we check it already exists for updating.

                // return the message status code in textData, non-critical (usually empty)
                return objInfo.TextData;
            }
            return "";
        }

        public void RemoveItem(int index)
        {
            if (index < _itemList.Count)
            {
                _itemList.RemoveAt(index);
            }
            SavePurchaseData();
        }

        public void UpdateItemQty(int index, int qty)
        {
            var itemqty = _itemList[index].GetXmlPropertyInt("genxml/qty");
            itemqty += qty;
            if (itemqty <= 0)
                RemoveItem(index);
            else
                _itemList[index].SetXmlProperty("genxml/qty", itemqty.ToString(""), TypeCode.String, false);
            SavePurchaseData();
        }


        public void DeleteCart()
        {
            //remove DB record
            var modCtrl = new NBrightBuyController();
            modCtrl.Delete(_entryId);
        }

        /// <summary>
        /// Get Current Cart
        /// </summary>
        /// <returns></returns>
        public NBrightInfo GetInfo()
        {
            return PurchaseInfo;
        }

        /// <summary>
        /// Get Current Cart Item List
        /// </summary>
        /// <returns></returns>
        public List<NBrightInfo> GetCartItemList(Boolean groupByProduct = false)
        {
            var rtnList = new List<NBrightInfo>();
            var xmlNodeList = PurchaseInfo.XMLDoc.SelectNodes("genxml/items/*");
            if (xmlNodeList != null)
            {
                foreach (XmlNode carNod in xmlNodeList)
                {
                    var newInfo = new NBrightInfo {XMLData = carNod.OuterXml};
                    newInfo.GUIDKey = newInfo.GetXmlProperty("genxml/itemcode");
                    newInfo.PortalId = PortalId;
                    newInfo.ItemID = newInfo.GetXmlPropertyInt("genxml/productid");
                    rtnList.Add(newInfo);
                }
            }

            if (groupByProduct)
            {

                var grouped = from p in rtnList group p by p.GetXmlProperty("genxml/productid") into g select new {g.Key,Value = g};
                rtnList = new List<NBrightInfo>();
                foreach (var group in grouped)
                {
                    // inject header record for the product
                    var itemheader = (NBrightInfo)group.Value.First().Clone();
                    itemheader.SetXmlProperty("genxml/groupheader","True");
                    itemheader.SetXmlProperty("genxml/groupcount", group.Value.Count().ToString(""));
                    itemheader.SetXmlProperty("genxml/seeditemcode", itemheader.GUIDKey);
                    itemheader.GUIDKey = "";
                    // remove option data, so we get a clear item 
                    itemheader.RemoveXmlNode("genxml/options");
                    rtnList.Add(itemheader);

                    foreach (var item in group.Value)
                    {
                        rtnList.Add(item);
                    }
                }
            }

            return rtnList;
        }

        /// <summary>
        /// Get Audit Item List
        /// </summary>
        /// <returns></returns>
        public List<NBrightInfo> GetAuditItemList()
        {
            var rtnList = new List<NBrightInfo>();
            var xmlNodeList = PurchaseInfo.XMLDoc.SelectNodes("genxml/audit/*");
            if (xmlNodeList != null)
            {
                foreach (XmlNode carNod in xmlNodeList)
                {
                    var newInfo = new NBrightInfo { XMLData = carNod.OuterXml };
                    newInfo.PortalId = PortalId;
                    rtnList.Add(newInfo);
                }
            }
            return rtnList;
        }
        /// <summary>
        /// Get Current Cart Item List
        /// </summary>
        /// <returns></returns>
        public void RemoveItem(String itemCode)
        {
            var removeindex = GetItemIndex(itemCode);
            if (removeindex >= 0) RemoveItem(removeindex);
        }

        public void MergeCartInputData(String itemCode, NBrightInfo inputInfo)
        {
            var index = GetItemIndex(itemCode);
            if (index >= 0) MergeCartInputData(index,inputInfo);
        }

        /// <summary>
        /// Merges data entered in the cartview into the cart item
        /// </summary>
        /// <param name="index">index of cart item</param>
        /// <param name="inputInfo">genxml data of cartview item</param>
        public void MergeCartInputData(int index, NBrightInfo inputInfo)
        {
            //get cart item
            //_itemList = GetCartItemList();  // Don;t get get here, it resets previous altered itemlist records.
            if (_itemList[index] != null)
            {

                #region "merge option data"
                var nodList = inputInfo.XMLDoc.SelectNodes("genxml/hidden/*[starts-with(name(), 'optionid')]");
                if (nodList != null)
                    foreach (XmlNode nod in nodList)
                    {
                        var idx = nod.Name.Replace("optionid", "");
                        var optid = nod.InnerText;
                        var oldoptvalue = _itemList[index].GetXmlProperty("genxml/options/option[optid='" + optid + "']/optvalueid");
                        var newoptvalue = "";
                        if (inputInfo.GetXmlProperty("genxml/textbox/optiontxt" + idx) != "")
                        {
                            _itemList[index].SetXmlProperty("genxml/options/option[optid='" + optid + "']/optvaltext", inputInfo.GetXmlProperty("genxml/textbox/optiontxt" + idx));   
                        }
                        if (inputInfo.GetXmlProperty("genxml/dropdownlist/optionddl" + idx) != "")
                        {
                            newoptvalue = inputInfo.GetXmlProperty("genxml/dropdownlist/optionddl" + idx);
                            _itemList[index].SetXmlProperty("genxml/options/option[optid='" + optid + "']/optvalueid", inputInfo.GetXmlProperty("genxml/dropdownlist/optionddl" + idx));
                            _itemList[index].SetXmlProperty("genxml/options/option[optid='" + optid + "']/optvaltext", inputInfo.GetXmlProperty("genxml/dropdownlist/optionddl" + idx + "/@selectedtext"));
                            if (oldoptvalue != newoptvalue) //rebuild itemcode
                            {
                                var icode = _itemList[index].GetXmlProperty("genxml/itemcode");
                                _itemList[index].SetXmlProperty("genxml/itemcode", icode.Replace(optid + ":" + oldoptvalue, optid + ":" + newoptvalue));
                            }
                        }
                        if (inputInfo.GetXmlProperty("genxml/checkbox/optionchk" + idx) != "")
                        {
                            newoptvalue = inputInfo.GetXmlProperty("genxml/checkbox/optionchk" + idx);
                            _itemList[index].SetXmlProperty("genxml/options/option[optid='" + optid + "']/optvalueid", newoptvalue);
                            if (oldoptvalue != newoptvalue) //rebuild itemcode
                            {
                                var icode = _itemList[index].GetXmlProperty("genxml/itemcode");
                                _itemList[index].SetXmlProperty("genxml/itemcode", icode.Replace(optid + ":" + oldoptvalue, optid + ":" + newoptvalue));
                            }
                        }

                    }

                #endregion

                var nods = inputInfo.XMLDoc.SelectNodes("genxml/textbox/*");
                if (nods != null)
                {
                    foreach (XmlNode nod in nods)
                    {
                        if (nod.Name.ToLower() == "qty")
                            _itemList[index].SetXmlProperty("genxml/" + nod.Name.ToLower(), nod.InnerText, TypeCode.String, false); //don't want cdata on qty field
                        else
                            _itemList[index].SetXmlProperty("genxml/textbox/" + nod.Name.ToLower(), nod.InnerText);
                    }
                }
                nods = inputInfo.XMLDoc.SelectNodes("genxml/radiobuttonlist/*");
                if (nods != null)
                {
                    foreach (XmlNode nod in nods)
                    {
                        if (nod.Name.ToLower() == "qty")
                            _itemList[index].SetXmlProperty("genxml/" + nod.Name.ToLower(), nod.InnerText, TypeCode.String, false); //don't want cdata on qty field
                        else
                        {
                            _itemList[index].SetXmlProperty("genxml/radiobuttonlist/" + nod.Name.ToLower(), nod.InnerText);
                            if (nod.Attributes != null && nod.Attributes["selectedtext"] != null) _itemList[index].SetXmlProperty("genxml/radiobuttonlist/" + nod.Name + "text", nod.Attributes["selectedtext"].Value);
                        }
                    }
                }
                nods = inputInfo.XMLDoc.SelectNodes("genxml/checkbox/*");
                if (nods != null)
                {
                    foreach (XmlNode nod in nods)
                    {
                        _itemList[index].SetXmlProperty("genxml/checkbox/" + nod.Name.ToLower(), nod.InnerText);
                    }
                }

                nods = inputInfo.XMLDoc.SelectNodes("genxml/dropdownlist/*");
                if (nods != null)
                {
                    foreach (XmlNode nod in nods)
                    {
                        if (nod.Name.ToLower() == "qty")
                            _itemList[index].SetXmlProperty("genxml/" + nod.Name.ToLower(), nod.InnerText, TypeCode.String, false); //don't want cdata on qty field
                        else
                        {
                            _itemList[index].SetXmlProperty("genxml/dropdownlist/" + nod.Name.ToLower(), nod.InnerText);
                            if (nod.Attributes != null && nod.Attributes["selectedtext"] != null) _itemList[index].SetXmlProperty("genxml/dropdownlist/" + nod.Name + "text", nod.Attributes["selectedtext"].Value);

                            // see if we've changed the model, if so update required fields
                            if (nod.Name.ToLower() == "ddlmodelsel")
                            {
                                var oldmodelid = _itemList[index].GetXmlProperty("genxml/modelid");
                                var newmodelid = nod.InnerText;
                                if (oldmodelid != newmodelid)
                                {
                                    AddSingleItem(_itemList[index].GetXmlProperty("genxml/productid"), newmodelid, _itemList[index].GetXmlProperty("genxml/qty"), inputInfo,false,index);
                                }
                            }
                        }
                    }
                }

            }
        }

        public void AddBillingAddress(NBrightInfo info)
        {
            var strXml = "<billaddress>";
            strXml += info.XMLData;
            strXml += "</billaddress>";
            PurchaseInfo.RemoveXmlNode("genxml/billaddress");
            PurchaseInfo.AddXmlNode(strXml, "billaddress", "genxml");
        }

        public NBrightInfo GetBillingAddress()
        {
            var rtnInfo = new NBrightInfo();
            rtnInfo.PortalId = PortalId;
            var xmlNode = PurchaseInfo.XMLDoc.SelectSingleNode("genxml/billaddress");
            if (xmlNode != null) rtnInfo.XMLData = xmlNode.InnerXml;
            return rtnInfo;
        }

        public void AddShippingAddress(NBrightInfo info)
        {
            var strXml = "<shipaddress>";
            strXml += info.XMLData;
            strXml += "</shipaddress>";
            PurchaseInfo.RemoveXmlNode("genxml/shipaddress");
            PurchaseInfo.AddXmlNode(strXml, "shipaddress", "genxml");
            SetShippingOption("2");
        }

        public NBrightInfo GetShippingAddress()
        {
            var rtnInfo = new NBrightInfo();
            rtnInfo.PortalId = PortalId;
            var xmlNode = PurchaseInfo.XMLDoc.SelectSingleNode("genxml/shipaddress");
            if (xmlNode != null)
            {
                rtnInfo.XMLData = xmlNode.InnerXml;
                return rtnInfo;
            }
            return GetBillingAddress();
        }

        public void DeleteShippingAddress()
        {
            PurchaseInfo.RemoveXmlNode("genxml/shipaddress");
        }

        public void AddExtraInfo(NBrightInfo info)
        {
            var strXml = "<extrainfo>";
            strXml += info.XMLData;
            strXml += "</extrainfo>";
            PurchaseInfo.RemoveXmlNode("genxml/extrainfo");
            PurchaseInfo.AddXmlNode(strXml, "extrainfo", "genxml");
        }

        public NBrightInfo GetExtraInfo()
        {
            var rtnInfo = new NBrightInfo(true);
            rtnInfo.PortalId = PortalId;
            var xmlNode = PurchaseInfo.XMLDoc.SelectSingleNode("genxml/extrainfo");
            if (xmlNode != null) rtnInfo.XMLData = xmlNode.InnerXml;

            var nodList = PurchaseInfo.XMLDoc.SelectNodes("genxml/*");
            if (nodList != null)
                foreach (XmlNode nod in nodList)
                {
                    if (nod.FirstChild != null && nod.FirstChild.Name != "genxml") rtnInfo.SetXmlProperty("genxml/" + nod.Name, nod.InnerText);                
                }

            return rtnInfo;
        }

        public void AddShipData(NBrightInfo info)
        {
            var strXml = "<shipdata>";
            strXml += info.XMLData;
            strXml += "</shipdata>";
            PurchaseInfo.RemoveXmlNode("genxml/shipdata");
            PurchaseInfo.AddXmlNode(strXml, "shipdata", "genxml");
        }

        public NBrightInfo GetShipData()
        {
            var rtnInfo = new NBrightInfo();
            rtnInfo.PortalId = PortalId;
            var xmlNode = PurchaseInfo.XMLDoc.SelectSingleNode("genxml/shipdata");
            if (xmlNode != null) rtnInfo.XMLData = xmlNode.InnerXml;
            return rtnInfo;
        }

        /// <summary>
        /// Get the shipping option: 1 = use billing, 2=shipping, 3 = collection
        /// </summary>
        public String GetShippingOption()
        {
            return PurchaseInfo.GetXmlProperty("genxml/extrainfo/genxml/radiobuttonlist/rblshippingoptions");
        }

        /// <summary>
        /// Set the shipping option:
        /// </summary>
        /// <param name="value"> 1 = use billing, 2=shipping, 3 = collection</param>
        public void SetShippingOption(String value)
        {
            if (PurchaseInfo.XMLDoc.SelectSingleNode("genxml/extrainfo/genxml") == null)
            {
                var nbi = new NBrightInfo(true);
                AddExtraInfo(nbi);
            }

            PurchaseInfo.SetXmlProperty("genxml/extrainfo/genxml/radiobuttonlist/rblshippingoptions", value, TypeCode.String, false);
        }

        /// <summary>
        /// Get the IsValidated (A cart is validated by the cart process and can only be converted to an ORDER when it has been validated)
        /// </summary>
        public Boolean IsValidated()
        {
            if (PurchaseInfo.GetXmlProperty("genxml/isvalidated") == "True") return true;
            return false;
        }

        public Boolean IsNotPaid()
        {
            return !IsPaid();
        }

        public Boolean IsPaid()
        {
            var orderstatus = PurchaseInfo.GetXmlProperty("genxml/dropdownlist/orderstatus");
            if (orderstatus == "010" || orderstatus == "020" || orderstatus == "030")
            {
                return false;
            }
            return true;
        }

        public Boolean IsArchived()
        {
            var orderstatus = PurchaseInfo.GetXmlProperty("genxml/dropdownlist/orderstatus");
            if (orderstatus == "110")
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get the IsClientOrderMode (If the cart is being edited/created by a manager then this flag is set to true.)
        /// </summary>
        public Boolean IsClientOrderMode()
        {
            if (PurchaseInfo.GetXmlProperty("genxml/clientmode") == "True")
            {
                if (!UserController.Instance.GetCurrentUserInfo().IsInRole(StoreSettings.ManagerRole) && !UserController.Instance.GetCurrentUserInfo().IsInRole(StoreSettings.EditorRole) && !UserController.Instance.GetCurrentUserInfo().IsInRole("Administrators")) // user not editor, so stop edit mode.
                {                    
                    PurchaseInfo.SetXmlProperty("genxml/clientmode", "False");
                    SavePurchaseData();
                    return false;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Set IsValidated:
        /// </summary>
        /// <param name="value"> </param>
        public void SetValidated(Boolean value)
        {
            if (value)
                PurchaseInfo.SetXmlProperty("genxml/isvalidated", "True", TypeCode.String, false);
            else
                PurchaseInfo.SetXmlProperty("genxml/isvalidated", "False", TypeCode.String, false);
        }

        public String GetClientEmail()
        {
            if (Utils.IsEmail(EmailBillingAddress)) return EmailBillingAddress;
            if (Utils.IsEmail(EmailAddress)) return EmailAddress;
            if (Utils.IsEmail(EmailShippingAddress)) return EmailShippingAddress;
            return "";
        }

        public String EmailAddress
        {
            get
            {
                return PurchaseInfo.GetXmlProperty("genxml/extrainfo/genxml/textbox/cartemailaddress");
            }
            set
            {
                PurchaseInfo.SetXmlProperty("genxml/extrainfo/genxml/textbox/cartemailaddress", value);
            }
        }
        public String EmailBillingAddress
        {
            get
            {
                return PurchaseInfo.GetXmlProperty("genxml/billaddress/genxml/textbox/email");
            }
            set
            {
                PurchaseInfo.SetXmlProperty("genxml/billaddress/genxml/textbox/email", value);
            }
        }
        public String EmailShippingAddress
        {
            get
            {
                return PurchaseInfo.GetXmlProperty("genxml/shipaddress/genxml/textbox/deliveryemail");
            }
            set
            {
                PurchaseInfo.SetXmlProperty("genxml/shipaddress/genxml/textbox/deliveryemail", value);
            }
        }


        public void OutputDebugFile(String fileName)
        {
            PurchaseInfo.XMLDoc.Save(PortalSettings.Current.HomeDirectoryMapPath + fileName);
        }



        public List<NBrightInfo> GetPurchaseDocs()
        {
            var rtnList = new List<NBrightInfo>();
            var xmlNodeList = PurchaseInfo.XMLDoc.SelectNodes("genxml/items/*");
            if (xmlNodeList != null)
            {
                foreach (XmlNode carNod in xmlNodeList)
                {
                    var xmlNodeList2 = carNod.SelectNodes("productxml/genxml/docs/*");
                    if (xmlNodeList2 != null)
                    {
                        foreach (XmlNode docNod in xmlNodeList2)
                        {
                            var newDocInfo = new NBrightInfo { XMLData = docNod.OuterXml };
                            if (newDocInfo.GetXmlPropertyBool("genxml/checkbox/chkpurchase"))
                            {
                                if (carNod.SelectSingleNode("productid") != null)
                                {
                                    newDocInfo.SetXmlProperty("genxml/productid", carNod.SelectSingleNode("productid").InnerText);
                                }
                                rtnList.Add(newDocInfo);
                            }
                        }
                    }

                }
            }
            return rtnList;
        }


        #endregion

        #region "private methods/functions"


        /// <summary>
        /// Check for any purchased download documents in cartitem and add any to USERDATA record.
        /// </summary>
        private void AddPurchasedDocs()
        {
            if (PurchaseInfo.GetXmlProperty("genxml/dropdownlist/orderstatus") == "040") // only add docs when payment OK.
            {
                // add purchased docs to user.
                var udata = new UserData(PurchaseInfo.UserId.ToString());
                var upd = false;
                foreach (var docitem in GetPurchaseDocs())
                {
                    if (!udata.HasPurchasedDocByFileName(docitem.GetXmlProperty("genxml/hidden/filename")))
                    {
                        udata.AddNewPurchasedDoc(Utils.GetUniqueKey(20), docitem.GetXmlProperty("genxml/hidden/filename"), docitem.GetXmlProperty("genxml/productid"));
                        upd = true;
                    }
                }
                if (upd) udata.Save();

            }
        }


        /// <summary>
        /// Get CartID from cookie or session
        /// </summary>
        /// <returns></returns>
        protected int PopulatePurchaseData(int entryId)
        {
            _entryId = entryId;
            //populate cart data
            var modCtrl = new NBrightBuyController();
            PurchaseInfo = modCtrl.Get(Convert.ToInt32(_entryId));
            if (PurchaseInfo == null)
            {
                PurchaseInfo = new NBrightInfo(true);
                PurchaseInfo.TypeCode = PurchaseTypeCode;
                //add items node so we can add items
                PurchaseInfo.AddSingleNode("items","","genxml");
                
                if (entryId == -1)
                {
                    PurchaseInfo.UserId = UserController.Instance.GetCurrentUserInfo().UserID; // new cart created from front office, so give current userid.
                    PurchaseInfo.GUIDKey = UserController.Instance.GetCurrentUserInfo().UserID.ToString(""); // new cart created from front office, so give current userid.
                    EditMode = "";
                }
            }
            else
            {
                EditMode = PurchaseInfo.GetXmlProperty("genxml/carteditmode"); // keep edit mode, so user doesn't get changed.
            }
            PurchaseTypeCode = PurchaseInfo.TypeCode;
            UserId = PurchaseInfo.UserId; //retain theuserid, if created by a manager for a client.
            var currentuserInfo = UserController.Instance.GetCurrentUserInfo();
            if (UserId > 0 && EditMode != "") // 0 is default userid for new cart
            {
                PurchaseInfo.SetXmlProperty("genxml/clientmode", "True", TypeCode.String, false);
                PurchaseInfo.SetXmlProperty("genxml/clientdisplayname", currentuserInfo.DisplayName);
            }
            else
            {
                PurchaseInfo.SetXmlProperty("genxml/clientmode", "False", TypeCode.String, false);
                PurchaseInfo.SetXmlProperty("genxml/clientdisplayname", "");
            }


            //build item list
            PopulateItemList();

            return Convert.ToInt32(_entryId);
        }


        public void PopulateItemList()
        {
            _itemList = GetCartItemList();
        }

        public int GetItemIndex(String itemCode)
        {
            var xmlNodeList = PurchaseInfo.XMLDoc.SelectNodes("genxml/items/*");
            if (xmlNodeList != null)
            {
                var lp = 0;
                foreach (XmlNode carNod in xmlNodeList)
                {
                    var newInfo = new NBrightInfo { XMLData = carNod.OuterXml };
                    if (newInfo.GetXmlProperty("genxml/itemcode") == itemCode)
                    {
                        return lp;
                    }
                    lp += 1;
                }
            }
            return -1;
        }

        #endregion


    }
}
