using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Web.UI.WebControls;
using System.Xml;
using DotNetNuke.Entities.Portals;
using NBrightCore.TemplateEngine;
using NBrightCore.render;
using NBrightDNN;
using NBrightCore.common;
using NBrightCore.images;
using NBrightCore.providers;
using Nevoweb.DNN.NBrightBuy.Components.Products;
using System.Text.RegularExpressions;

namespace Nevoweb.DNN.NBrightBuy.Components
{
	public class ProductUtils
	{



        #region "Entities"
        /// <summary>
        /// Entities are XML strucutred objects addtached the to the product data. ("models" on products,"optionval" on options ) 
        /// 
        /// NOTE: This is a little complicated using our XML data strucutre, but the pain here should be repaid when we come to a running system.
        ///       i.e. The entities(e.g. models) can become part of the order and cart xml and hence be independant of the product data,
        ///       so we have a single DB record to deal with for export/display, plus when we need to remove a model from a product
        ///       there is no need to check the foreign key for model usage elsewhere in the system and hence models can be removed from products without error.
        /// </summary>
        public static List<NBrightInfo> GetEntityList(NBrightInfo objInfo, String entityName)
		{
			var l = new List<NBrightInfo>();

            var xmlNodList = objInfo.XMLDoc.SelectNodes("genxml/" + entityName + "/genxml");
			if (xmlNodList != null)
			{
				//--------------------------------------------------
				//do non-langauge display
				//--------------------------------------------------
				// build generic list to bind to rpModels List
				foreach (XmlNode xNod in xmlNodList)
				{
					var obj = new NBrightInfo();
					// add the models data to this temp obj
					obj.XMLData = xNod.OuterXml;
					obj.ItemID = objInfo.ItemID;
					l.Add(obj);
				}
			}
			return l;
		}

        public static List<NBrightInfo> GetEntityLangList(NBrightInfo objInfo, NBrightInfo objLangInfo, String entityName)
		{

			var l = new List<NBrightInfo>();
			//--------------------------------------------------
			//do langauge display
			//--------------------------------------------------
			if (objLangInfo != null && objInfo != null)
			{
				var xmlNodList = objInfo.XMLDoc.SelectNodes("genxml/" + entityName + "/genxml");
				// build generic list to bind to rpModelsLang List
                var xmlNodListLang = objLangInfo.XMLDoc.SelectNodes("genxml/" + entityName + "/genxml");
				if (xmlNodList != null && xmlNodList.Count > 0)
				{
					var lp = 0;
					foreach (XmlNode xNod in xmlNodList)
					{
						var obj = new NBrightInfo();
						// add the models data to this temp obj
						if (xmlNodListLang != null && xmlNodListLang[lp] != null)
						{
							obj.XMLData = xmlNodListLang[lp].OuterXml;
						}
						obj.ItemID = objLangInfo.ParentItemId;
						l.Add(obj);
						lp = lp + 1;
					}
				}
			}
			return l;
		}


        public static NBrightInfo AddEntity(NBrightInfo objInfo, String entityName, int numberToAdd = 1, String genxmlData = "<genxml></genxml>")
        {
            var xNod = objInfo.XMLDoc.SelectSingleNode("genxml/" + entityName.ToLower());
            if (xNod != null)
            {
                var strModelXml = "";
                for (int i = 0; i < numberToAdd; i++)
                {
                    strModelXml += genxmlData;
                }
                // Create a document fragment to contain the XML to be inserted. 
                var docFrag = objInfo.XMLDoc.CreateDocumentFragment();
                // Set the contents of the document fragment. 
                docFrag.InnerXml = strModelXml;
                //Add new model data
                xNod.AppendChild(docFrag);
                objInfo.XMLData = objInfo.XMLDoc.OuterXml;
            }
            return objInfo;
        }

        public static NBrightInfo InsertEntityData(NBrightInfo objInfo, Repeater rpEntity, String entityName, String folderMapPath = "")
		{
			var strModelXml = "<" + entityName + ">";
            foreach (RepeaterItem i in rpEntity.Items)
			{
                if (GenXmlFunctions.GetField(rpEntity, "chkDelete", i.ItemIndex) != "True")
                {
                    GenXmlFunctions.SetField(rpEntity, "entityindex", i.ItemIndex.ToString(CultureInfo.InvariantCulture), i.ItemIndex);
					strModelXml += GenXmlFunctions.GetGenXml(i, "", folderMapPath);
                }
			}
            strModelXml += "</" + entityName + ">";

			// Create a document fragment to contain the XML to be inserted. 
			var docFrag = objInfo.XMLDoc.CreateDocumentFragment();
			// Set the contents of the document fragment. 
			docFrag.InnerXml = strModelXml;
			//Add new data
			if (objInfo.XMLDoc.DocumentElement != null) objInfo.XMLDoc.DocumentElement.AppendChild(docFrag);
			objInfo.XMLData = objInfo.XMLDoc.OuterXml;
			return objInfo;
		}

        public static NBrightInfo InsertEntityLangData(NBrightInfo objLangInfo, Repeater rpEntity, Repeater rpEntityLang, String entityName, String folderMapPath = "")
		{

            var strModelXML = "<" + entityName + ">";
            foreach (RepeaterItem i in rpEntityLang.Items)
			{
                if (GenXmlFunctions.GetField(rpEntity, "chkDelete", i.ItemIndex) != "True")
                {
                    GenXmlFunctions.SetField(rpEntityLang, "entityindex", i.ItemIndex.ToString(CultureInfo.InvariantCulture), i.ItemIndex);
                    strModelXML += GenXmlFunctions.GetGenXml(i, "", folderMapPath);
                }
            }
            strModelXML += "</" + entityName + ">";

			// Create a document fragment to contain the XML to be inserted. 
			var docFrag = objLangInfo.XMLDoc.CreateDocumentFragment();
			// Set the contents of the document fragment. 
			docFrag.InnerXml = strModelXML;
			//Add new model data
			if (objLangInfo.XMLDoc.DocumentElement != null) objLangInfo.XMLDoc.DocumentElement.AppendChild(docFrag);
			objLangInfo.XMLData = objLangInfo.XMLDoc.OuterXml;

			return objLangInfo;
		}

		#endregion

        #region "xref links to product"

        public static string GetRelatedProducts(int portalId, string parentItemId, string lang, string templatePrefix, string controlMapPath)
        {
            return GetRelatedXref(portalId, parentItemId, lang, templatePrefix, "prdxref", "PRDLANG", controlMapPath);
        }

        public static string GetProductImgs(int portalId, string parentItemId, string lang, string templatePrefix, string controlMapPath)
        {
            return GetRelatedXref(portalId, parentItemId, lang, templatePrefix, "prdimg", "IMGLANG", controlMapPath);
        }

        public static string GetProductDocs(int portalId, string parentItemId, string lang, string templatePrefix, string controlMapPath)
        {
            return GetRelatedXref(portalId, parentItemId, lang, templatePrefix, "prddoc", "DOCLANG", controlMapPath);
        }

        public static string GetProductOpts(int portalId, string parentItemId, string lang, string templatePrefix, string controlMapPath)
        {
            return GetRelatedXref(portalId, parentItemId, lang, templatePrefix, "prdopt", "OPTLANG", controlMapPath);
        }

        public static string GetRelatedXref(int portalId, string parentItemId, string lang, string templatePrefix, string nodeName, string entityTypeCodeLang, string controlMapPath)
        {
            var strOut = "";
            if (Utils.IsNumeric(parentItemId))
            {

                var objCtrl = new NBrightBuyController();

                var templCtrl = new TemplateController(controlMapPath);

                var hTempl = templCtrl.GetTemplateData(templatePrefix + "_" + ModuleEventCodes.selectedheader + ".html", Utils.GetCurrentCulture());
                var bTempl = templCtrl.GetTemplateData(templatePrefix + "_" + ModuleEventCodes.selectedbody + ".html", Utils.GetCurrentCulture());
                var fTempl = templCtrl.GetTemplateData(templatePrefix + "_" + ModuleEventCodes.selectedfooter + ".html", Utils.GetCurrentCulture());

                // replace tags for ajax to work.
                hTempl = Utils.ReplaceUrlTokens(hTempl);
                bTempl = Utils.ReplaceUrlTokens(bTempl);
                fTempl = Utils.ReplaceUrlTokens(fTempl);

                var objPInfo = objCtrl.Get(Convert.ToInt32(parentItemId));
                if (objPInfo != null)
                {
                    var nodList = objPInfo.XMLDoc.SelectNodes("genxml/" + nodeName + "/id");
                    var objList = new List<NBrightInfo>();

                    foreach (XmlNode xNod in nodList)
                    {
                        if (xNod != null && Utils.IsNumeric(xNod.InnerText))
                        {
                            var o = objCtrl.Get(Convert.ToInt32(xNod.InnerText), lang, entityTypeCodeLang);
                            if (o != null)
                            {
                                objList.Add(o);
                            }
                        }
                    }

                    var obj = new NBrightInfo();
                    strOut += GenXmlFunctions.RenderRepeater(obj, hTempl);
                    strOut += GenXmlFunctions.RenderRepeater(objList, bTempl);
                    strOut += GenXmlFunctions.RenderRepeater(obj, fTempl);
                }
            }

            return strOut;
        }

        public static void RemoveAllXref(string itemId)
        {
            if (Utils.IsNumeric(itemId))
            {
                var xrefList = new List<string>();
                xrefList.Add("prdimg");
                xrefList.Add("prdxref");
                xrefList.Add("prddoc");
                xrefList.Add("prdopt");

                foreach (var xrefName in xrefList)
                {
                    var objCtrl = new NBrightBuyController();
                    var objPInfo = objCtrl.Get(Convert.ToInt32(itemId));
                    if (objPInfo != null)
                    {
                        var xrefIdList = objPInfo.GetXrefList(xrefName);
                        foreach (var xrefid in xrefIdList)
                        {
                            if (Utils.IsNumeric(xrefid))
                            {
                                var objRef = objCtrl.Get(Convert.ToInt32(xrefid));
                                if (objRef != null)
                                {
                                    objRef.RemoveXref(xrefName, itemId);
                                    objCtrl.Update(objRef);
                                }
                            }
                            objPInfo.RemoveXref(xrefName, xrefid);
                            objCtrl.Update(objPInfo);
                        }

                    }
                }
            }
        }
        #endregion


        public static string GetRelatedCats(int portalId, string parentItemId, string cultureCode, string templatePrefix, string controlMapPath, Boolean AllowCache = true)
        {
            var strOut = "";
            if (Utils.IsNumeric(parentItemId))
            {
                if (!AllowCache)
                {
                    //Remove any cache for the module -1, we don't want any cache in BO
                    //All xref records are portal wide, hence -1 in cahce key.
                    NBrightBuyUtils.RemoveModCache(-1);
                }

                var objCtrl = new NBrightBuyController();

                var templCtrl = new TemplateController(controlMapPath);

                var hTempl = templCtrl.GetTemplateData(templatePrefix + "_" + ModuleEventCodes.selectedheader + ".html", Utils.GetCurrentCulture());
                var bTempl = templCtrl.GetTemplateData(templatePrefix + "_" + ModuleEventCodes.selectedbody + ".html", Utils.GetCurrentCulture());
                var fTempl = templCtrl.GetTemplateData(templatePrefix + "_" + ModuleEventCodes.selectedfooter + ".html", Utils.GetCurrentCulture());

                // replace Settings tags for ajax to work.
                hTempl = Utils.ReplaceUrlTokens(hTempl);
                bTempl = Utils.ReplaceUrlTokens(bTempl);
                fTempl = Utils.ReplaceUrlTokens(fTempl);

                var strFilter = " and parentitemid = " + parentItemId;
                var strOrderBy = GenXmlFunctions.GetSqlOrderBy(hTempl);
                if (strOrderBy == "")
                {
                    strOrderBy = GenXmlFunctions.GetSqlOrderBy(bTempl);
                }

                var l = objCtrl.GetList(portalId, -1, "CATXREF", strFilter, strOrderBy);
                var objList = new List<NBrightInfo>();

                foreach (var objXref in l)
                {
                    var o = objCtrl.Get(objXref.XrefItemId, "CATEGORYLANG", cultureCode);
                    if (o != null)
                    {
                        if (objXref.GetXmlProperty("genxml/hidden/defaultcat") != "")
                        {
                            // set the default flag in the category, for display in the entry only.
                            o.SetXmlProperty("genxml/hidden/defaultcat", "True");
                        }
                        o.GUIDKey = objXref.ItemID.ToString(); // overwrite with xref itemid for delete ajax action.
                        o.TextData = o.GetXmlProperty("genxml/lang/genxml/textbox/txtname"); // set for sort
                        o.Lang = cultureCode; // set lang so the GenXmlTemplateExt can pickup the edit langauge.
                        objList.Add(o);
                        objList.Sort(delegate(NBrightInfo p1, NBrightInfo p2) { return p1.TextData.CompareTo(p2.TextData); });
                    }
                }

                var obj = new NBrightInfo();
                strOut += GenXmlFunctions.RenderRepeater(obj, hTempl);
                strOut += GenXmlFunctions.RenderRepeater(objList, bTempl);
                strOut += GenXmlFunctions.RenderRepeater(obj, fTempl);

            }

            return strOut;
        }

        public static NBrightInfo CalculateModels(NBrightInfo objInfo,String controlMapPath)
        {
            var objCtrl = new NBrightBuyController();
            var optList = new List<NBrightInfo>();

            // get list of active options for product models
            var xmlOptList = objInfo.XMLDoc.SelectNodes("genxml/prdopt/id");
            if (xmlOptList != null)
            {
                foreach (XmlNode xNod in xmlOptList)
                {
                    if (Utils.IsNumeric(xNod.InnerText))
                    {
                        var objOpt = objCtrl.Get(Convert.ToInt32(xNod.InnerText));
                        if (objOpt != null) optList.Add(objOpt);
                    }
                }                
            }

            //sort into ItemId order so we get the same modelcode created.
            optList.Sort(delegate(NBrightInfo p1, NBrightInfo p2)
            {
                return p1.ItemID.CompareTo(p2.ItemID);
            });

            //Build modelCode list
            int lp1 = 0;
            var mcList = new List<string>();
            lp1 = 0;
            if (optList.Count == 1)
            {
                // only 1 option with stock, so no need to do a recursive build.
                var xmlNodList2 = optList[0].XMLDoc.SelectNodes("genxml/optionval/genxml");
                if (xmlNodList2 != null)
                    foreach (XmlNode xNod2 in xmlNodList2)
                    {
                        var xNod = xNod2.SelectSingleNode("textbox/txtoptionvalue");
                        if (xNod != null) mcList.Add(xNod.InnerText);
                    }
            }
            else
            {
                // do recursive build on options.
                while (lp1 < (optList.Count - 1))
                {
                    mcList = BuildModelCodes(optList, lp1, "", "", mcList);
                    lp1++;
                }
            }


            //Merge with existing models
            var templCtrl = new TemplateGetter(controlMapPath,controlMapPath);
            Repeater rpEntity;
            var strTemplate = templCtrl.GetTemplateData("AdminProducts_Models.html", Utils.GetCurrentCulture(), true, true, true, StoreSettings.Current.Settings());
            
            // remove models no longer needed
            XmlNodeList nodes = objInfo.XMLDoc.SelectNodes("genxml/models/genxml");
            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                var mCode = nodes[i].SelectSingleNode("hidden/modelcode");
                if (mCode != null)
                {
                    if (!mcList.Contains(mCode.InnerText))
                    {
                        var parentNode = nodes[i].ParentNode;
                        if (parentNode != null) parentNode.RemoveChild(nodes[i]);
                    }
                }
                else
                {
                    // no modelcode, invalid, so remove
                    var parentNode = nodes[i].ParentNode;
                    if (parentNode != null) parentNode.RemoveChild(nodes[i]);
                }
            }
            
            // save changes back to the product object
            objInfo.XMLData = objInfo.XMLDoc.OuterXml;

            // add new models
            var idx = 0;
            foreach (var modelCode in mcList)
            {
                if (objInfo.XMLDoc.SelectSingleNode("genxml/models/genxml/hidden/modelcode[.='" + modelCode + "']") == null)
                {
                    var obj = new NBrightInfo();
                    rpEntity = GenXmlFunctions.InitRepeater(obj, strTemplate);
                    GenXmlFunctions.SetHiddenField(rpEntity.Items[0], "modelcode", modelCode);
                    GenXmlFunctions.SetHiddenField(rpEntity.Items[0], "entityindex", idx.ToString(CultureInfo.InvariantCulture));
                    var strXml = GenXmlFunctions.GetGenXml(rpEntity, 0);
                    objInfo = AddEntity(objInfo, "models", 1, strXml);
                    idx += 1;
                }
            }

            return objInfo;
        }

        private static List<string> BuildModelCodes(List<NBrightInfo> optList, int lpPos, String pmodelCode, String modelCode, List<string> mcList)
        {
            // COMMENT(Dave Lee): Dave, if your looking at this in the future... 
            // YES!!! you did stuggle to pop this out of your tiny mad mind...
            // and YES!! your still just as stupid now as then! 
            var xmlNodList2 = optList[lpPos].XMLDoc.SelectNodes("genxml/optionval/genxml");
            if (xmlNodList2 != null)
            {
                foreach (XmlNode xNod2 in xmlNodList2)
                {
                    if ((pmodelCode == "") | (pmodelCode != modelCode)) // only want to add the first value to the modelcode
                    {
                        var selectSingleNode = xNod2.SelectSingleNode("textbox/txtoptionvalue");
                        if (selectSingleNode != null) modelCode = pmodelCode + selectSingleNode.InnerText + "-";
                        if (lpPos == (optList.Count - 1)) // only add the last child of the tree
                        {
                            mcList.Add(modelCode.TrimEnd('-'));
                        }
                        if (lpPos < (optList.Count - 1)) // end recussive loop on last child.
                        {
                            mcList = BuildModelCodes(optList, lpPos + 1, modelCode, "", mcList);
                        }
                    }
                }
            }
            return mcList;
        }

        /// <summary>
        /// Get ProductData class with cacheing
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="portalId"></param>
        /// <param name="lang"></param>
        /// <param name="hydrateLists"></param>
        /// <param name="typeCode">Typecode of record default "PRD"</param>
        /// <param name="typeLangCode">Langauge Typecode of record default "PRDLANG"</param>
        /// <returns></returns>
        public static ProductData GetProductData(int productId, int portalId, String lang, Boolean hydrateLists = true, String typeCode = "PRD")
        {
            ProductData prdData;
            var cacheKey = "NBSProductData*" + productId.ToString("") + "*" + lang;
            prdData = (ProductData)Utils.GetCache(cacheKey);
            if ((prdData == null) || (productId == -1))
            {
                prdData = new ProductData(productId, portalId, lang, hydrateLists, typeCode);
                Utils.SetCache(cacheKey, prdData);
            }
            return prdData;
        }


        /// <summary>
        /// Get ProductData class with cacheing
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="lang"></param>
        /// <param name="hydrateLists"></param>
        /// <param name="typeCode">Typecode of record default "PRD"</param>
        /// <param name="typeLangCode">Langauge Typecode of record default "PRDLANG"</param>
        /// <returns></returns>
        public static ProductData GetProductData(int productId, String lang, Boolean hydrateLists = true, String typeCode = "PRD")
        {
            if (Utils.IsNumeric(productId))
            {
                return GetProductData(Convert.ToInt32(productId), PortalSettings.Current.PortalId, lang, hydrateLists, typeCode);
            }
            return null;
        }

	    public static ProductData GetProductData(String productId, String lang, Boolean hydrateLists = true)
	    {
	        if (Utils.IsNumeric(productId))
	        {
	            return GetProductData(Convert.ToInt32(productId),lang,hydrateLists);
	        }
	        return null;
	    }

        [Obsolete("Deprecated, please use RemoveProductDataCache(int portalid, int productId) instead.", true)]
        public static void RemoveProductDataCache(String productId, String lang)
        {
            if (Utils.IsNumeric(productId)) RemoveProductDataCache(Convert.ToInt32(productId), lang);
        }

        [Obsolete("Deprecated, please use RemoveProductDataCache(int portalid, int productId) instead.", true)]
        public static void RemoveProductDataCache(int productId, String lang)
        {
            var cacheKey = "NBSProductData*" + productId.ToString("") + "*" + lang;
            Utils.RemoveCache(cacheKey);
        }

        /// <summary>
        /// Remove product data cache for all languages in portal
        /// </summary>
        /// <param name="portalid"></param>
        /// <param name="productId"></param>
        public static void RemoveProductDataCache(int portalid, int productId)
        {
            foreach (var lang in DnnUtils.GetCultureCodeList(portalid))
            {
                var cacheKey = "NBSProductData*" + productId.ToString("") + "*" + lang;
                Utils.RemoveCache(cacheKey);
            }
        }

        public static void RemoveProductDataCache(ProductData product)
        {
            RemoveProductDataCache(product.Info.PortalId, product.Info.ItemID);
        }

	    public static void CreateFriendlyImages(int productid, string lang)
	    {
            var objCtrl = new NBrightBuyController();
            var imgList = new List<string>();
            var productData = new ProductData(productid, lang);
	        var productImgFolder = StoreSettings.Current.FolderImagesMapPath.TrimEnd('\\') + "\\" + productData.DataRecord.ItemID + "\\" + lang;
            Utils.CreateFolder(productImgFolder);

	        foreach (var i in productData.Imgs)
	        {
	            //becuase of updates to sort order and alt text we NEED to delete the existing files.
	            Utils.DeleteSysFile(i.GetXmlProperty("genxml/lang/genxml/hidden/fimagepath"));
	        }

	        var lp = 1;
	        foreach (var i in productData.Imgs)
	        {
                if (StoreSettings.Current.SEOimages)
                {

                    // use imageref to link langauges
                    var imageref = i.GetXmlProperty("genxml/hidden/imageref");
                    if (imageref == "")
                    {
                        imageref = Utils.GetUniqueKey();
                        productData.DataRecord.SetXmlProperty("genxml/imgs/genxml[" + lp + "]/hidden/imageref", imageref);
                    }

                    var imgname = i.GetXmlProperty("genxml/lang/genxml/textbox/txtimagedesc");
                    if (imgname == "")
                    {
                        imgname = productData.ProductName;
                    }
                    if (imgname == "")
                    {
                        imgname = productData.ProductRef;
                    }
                    if (imgname != "")
                    {
                        var fullName = i.GetXmlProperty("genxml/hidden/imagepath");
                        var extension = Path.GetExtension(fullName);
                        imgname = AlphaNumeric(CleanFileName(imgname.Replace(" ", "-")));
                        var imgnameext = imgname + extension;
                        var newImageFileName = productImgFolder + "\\" + imgnameext;
                        var lp2 = 1;
                        while (File.Exists(newImageFileName))
                        {
                            imgnameext = imgname + "-" + lp2 + extension;
                            newImageFileName = productImgFolder + "\\" + imgnameext;
                            lp2++;
                        }
                        var imgSize = StoreSettings.Current.GetInt(StoreSettingKeys.productimageresize);
                        if (imgSize == 0) imgSize = 960;
                        if (extension != null && extension.ToLower() == ".png")
                        {
                            newImageFileName = ImgUtils.ResizeImageToPng(fullName, newImageFileName, imgSize);
                        }
                        else
                        {
                            newImageFileName = ImgUtils.ResizeImageToJpg(fullName, newImageFileName, imgSize);
                        }
                        var newimageurl = StoreSettings.Current.FolderImages.TrimEnd('/') + "/" + productData.DataRecord.ItemID + "/" + lang + "/" + imgnameext;
                        productData.DataLangRecord.SetXmlProperty("genxml/imgs/genxml[" + lp + "]/hidden/fimageurl", newimageurl);
                        productData.DataLangRecord.SetXmlProperty("genxml/imgs/genxml[" + lp + "]/hidden/fimagepath", newImageFileName);
                        productData.DataLangRecord.SetXmlProperty("genxml/imgs/genxml[" + lp + "]/hidden/fimageref", imageref);
                        imgList.Add(newImageFileName);
                    }

                }
                else
                {
                    productData.DataLangRecord.SetXmlProperty("genxml/imgs/genxml[" + lp + "]/hidden/fimageurl", "");
                    productData.DataLangRecord.SetXmlProperty("genxml/imgs/genxml[" + lp + "]/hidden/fimagepath", "");
                    productData.DataLangRecord.SetXmlProperty("genxml/imgs/genxml[" + lp + "]/hidden/fimageref", "");
                }

                lp += 1;
	        }

            objCtrl.Update(productData.DataLangRecord);
            objCtrl.Update(productData.DataRecord);

            // remove any deleted images.
            var fl = Directory.GetFiles(productImgFolder);
            foreach (var f in fl)
            {
                if (!imgList.Contains(f))
                {
                    Utils.DeleteSysFile(f);
                }
            }

            // sort other language
            foreach (var l in DnnUtils.GetCultureCodeList())
            {
                if (l != lang)
                {
                    var strXml = "";
                    var pData = new ProductData(productid, l);
                    var lp3 = 1;
                    foreach (var langimg in pData.Imgs)
                    {
                        var imgref = langimg.GetXmlProperty("genxml/hidden/imageref");
                        var strNode = pData.DataLangRecord.GetXmlNode("genxml/imgs/genxml[./hidden/fimageref='" + imgref + "']");
                        if (strNode == "")
                        {
                            // create missing ref and get xml. (May misalign images alts)
                            pData.DataLangRecord.SetXmlProperty("genxml/imgs/genxml[" + lp3 + "]/hidden/fimageref", imgref);
                            strNode = pData.DataLangRecord.GetXmlNode("genxml/imgs/genxml[./hidden/fimageref='" + imgref + "']");
                        }
                        strXml += "<genxml>" + strNode + "</genxml>";
                        lp3 += 1;
                    }
                    strXml = "<imgs>" + strXml + "</imgs>";
                    pData.DataLangRecord.RemoveXmlNode("genxml/imgs");
                    pData.DataLangRecord.AddXmlNode(strXml,"imgs","genxml");
                    objCtrl.Update(pData.DataLangRecord);
                }
            }

            RemoveProductDataCache(PortalSettings.Current.PortalId, productid);

        }
        public static string CleanFileName(string filename)
        {
            string file = filename;
            file = string.Concat(file.Split(System.IO.Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));

            file = file.Replace("&", "-");
            file = file.Replace("?", "-");
            file = file.Replace("+", "-");

            if (file.Length > 250)
            {
                file = file.Substring(0, 250);
            }
            return file;
        }
        public static string AlphaNumeric(string strIn)
        {
            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            var str = rgx.Replace(strIn, "");
            return str.Replace(" ", "");
        }


        public static void DeleteFriendlyImages(int productid)
        {
            var objCtrl = new NBrightBuyController();
            var productImgFolder = StoreSettings.Current.FolderImagesMapPath.TrimEnd('\\') + "\\" + productid;
            Utils.DeleteFolder(productImgFolder,true);
            var list = objCtrl.GetList(PortalSettings.Current.PortalId,-1,"PRDLANG"," and NB1.ParentItemId = " + productid);
            foreach (var i in list)
            {
                i.RemoveXmlNode("genxml/hidden/imageurl");
                i.RemoveXmlNode("genxml/hidden/imagepath");
                objCtrl.Update(i);
            }
        }

    }
}