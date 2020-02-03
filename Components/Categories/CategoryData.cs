using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Web.UI.WebControls;
using System.Xml;
using DotNetNuke.Entities.Portals;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;

namespace Nevoweb.DNN.NBrightBuy.Components
{
    public class CategoryData
    {
        public NBrightInfo Info;
        public NBrightInfo DataRecord;
        public NBrightInfo DataLangRecord;

        private Boolean _doCascadeIndex;
        private int _oldcatcascadeid = 0;
        private String _lang = ""; // needed for webservice
        private int _portalId = -1; // for shared products.

        private NBrightBuyController _objCtrl = null;

        /// <summary>
        /// Populate the ProductData in this class
        /// </summary>
        /// <param name="categoryId">categoryId (use -1 to create new record)</param>
        /// <param name="lang"> </param>
        public CategoryData(String categoryId,String lang)
        {
            if (Utils.IsNumeric(categoryId))
            {
                LoadCatData(Convert.ToInt32(categoryId), lang);
            }
        }

        /// <summary>
        /// Populate the CategoryData in this class
        /// </summary>
        /// <param name="categoryId">categoryId (use -1 to create new record)</param>
        /// <param name="lang"> </param>
        public CategoryData(int categoryId, String lang)
        {
            LoadCatData(categoryId, lang);
        }

        private void LoadCatData(int categoryId, String lang)
        {
            _objCtrl = new NBrightBuyController();
            _lang = lang;
            if (_lang == "") _lang = Utils.GetCurrentCulture();
            LoadData(categoryId);
        }

        #region "public functions/interface"

        /// <summary>
        /// Set to true if product exists
        /// </summary>
        public bool Exists { get; private set; }

        public String Name
        {
            get
            {
                return DataLangRecord.GetXmlProperty("genxml/textbox/txtcategoryname");
            }
            set
            {
                DataLangRecord.SetXmlProperty("genxml/textbox/txtcategoryname", value);
            }
        }

        public String GroupType {
            get
            {
                return DataRecord.GetXmlProperty("genxml/dropdownlist/ddlgrouptype");
            }
            set
            {
                DataRecord.SetXmlProperty("genxml/dropdownlist/ddlgrouptype", value);
            }
        }

        public int ParentItemId
        {
            get
            {
                return DataRecord.ParentItemId;
            }
            set
            {
                DataRecord.ParentItemId = value;
                DataRecord.SetXmlProperty("genxml/dropdownlist/ddlparentcatid", value.ToString(""));
            }
        }


        public String SEOName
        {
            get
            {
                if (Exists)
                {
                    var seoname = Info.GetXmlProperty("genxml/lang/genxml/textbox/txtseoname");
                    if (seoname == "") seoname = Info.GetXmlProperty("genxml/lang/genxml/textbox/txtcategoryname");
                    return seoname;                                    
                }
                return "";
            }
        }

        public String SEOTitle
        {
            get
            {
                if (Exists) return Info.GetXmlProperty("genxml/lang/genxml/textbox/txtseopagetitle");
            return "";
            }
        }

        public String SEOTagwords
        {
            get
            {
                if (Exists) return Info.GetXmlProperty("genxml/lang/genxml/textbox/txtmetakeywords");
            return "";
            }
        }

        public String SEODescription
        {
            get
            {
                if (Exists) return Info.GetXmlProperty("genxml/lang/genxml/textbox/txtmetadescription");
                return "";
            }
        }

        public String CategoryRef => DataRecord.GetXmlProperty("genxml/textbox/txtcategoryref");

        public int CategoryId => DataRecord.ItemID;


        public Boolean IsHidden
        {
            get
            {
                return DataRecord.GetXmlPropertyBool("genxml/checkbox/chkishidden");
            }
            set
            {
                DataRecord.SetXmlProperty("genxml/checkbox/chkishidden", value.ToString());
            }
        }

        public void Delete()
        {
            _objCtrl.Delete(DataRecord.ItemID);
        }

        public void Save()
        {
            _objCtrl.Update(DataRecord);
            _objCtrl.Update(DataLangRecord);
            
            //do reindex of cascade records.
            if (_doCascadeIndex)
            {
                var objGrpCtrl = new GrpCatController(_lang);
                objGrpCtrl.ReIndexCascade(_oldcatcascadeid); // reindex form parnet and parents
                objGrpCtrl.ReIndexCascade(DataRecord.ItemID); // reindex self
                objGrpCtrl.Reload();
            }

            NBrightBuyUtils.ProcessEventProvider(EventActions.AfterCategorySave, DataRecord);
            // reload data so if event has altered data we use that.
            DataRecord = _objCtrl.Get(DataRecord.ItemID);
            DataLangRecord = _objCtrl.Get(DataLangRecord.ItemID);

            Utils.RemoveCacheList("category_cachelist");
        }

        public void Update(NBrightInfo info)
        {

            // build list of xpath fields that need processing.
            var updatefields = new List<String>();
            var fieldList = NBrightBuyUtils.GetAllFieldxPaths(info);
            foreach (var xpath in fieldList)
            {
                if (info.GetXmlProperty(xpath + "/@update") == "lang")
                {
                    updatefields.Add(xpath);
                }
            }

            foreach (var f in updatefields)
            {
                if (f.EndsWith("/message"))
                {
                    // special processing for editor, to place code in standard place.
                    if (DataLangRecord.XMLDoc.SelectSingleNode("genxml/edt") == null) DataLangRecord.AddSingleNode("edt", "", "genxml");
                    if (info.GetXmlProperty("genxml/textbox/message") == "")
                        DataLangRecord.SetXmlProperty("genxml/edt/message", info.GetXmlPropertyRaw("genxml/edt/message"));
                    else
                        DataLangRecord.SetXmlProperty("genxml/edt/message", info.GetXmlPropertyRaw("genxml/textbox/message")); // ajax on ckeditor (Ajax diesn't work for telrik)
                }
                else
                {
                    DataLangRecord.RemoveXmlNode(f);
                    var xpathDest = f.Split('/');
                    if (xpathDest.Count() >= 2) DataLangRecord.AddXmlNode(info.XMLData, f, xpathDest[0] + "/" + xpathDest[1]);
                }

                var datatype = info.GetXmlProperty(f + "/@datatype");
                if (datatype == "date")
                    DataLangRecord.SetXmlProperty(f, info.GetXmlProperty(f), TypeCode.DateTime);
                else if (datatype == "double")
                    DataLangRecord.SetXmlPropertyDouble(f, info.GetXmlProperty(f));
                else if (datatype == "html")
                    DataLangRecord.SetXmlProperty(f, info.GetXmlPropertyRaw(f));
                else
                    DataLangRecord.SetXmlProperty(f, info.GetXmlProperty(f).Trim());


                DataRecord.RemoveXmlNode(f);
            }


            updatefields = new List<String>();
            fieldList = NBrightBuyUtils.GetAllFieldxPaths(info);
            foreach (var xpath in fieldList)
            {
                var id = xpath.Split('/').Last();
                if (info.GetXmlProperty(xpath + "/@update") == "save")
                {
                    updatefields.Add(xpath);
                }
            }

            foreach (var f in updatefields)
            {
                var datatype = info.GetXmlProperty(f + "/@datatype");
                if (datatype == "date")
                    DataRecord.SetXmlProperty(f, info.GetXmlProperty(f), TypeCode.DateTime);
                else if (datatype == "double")
                    DataRecord.SetXmlPropertyDouble(f, info.GetXmlProperty(f));
                else if (datatype == "html")
                    DataRecord.SetXmlProperty(f, info.GetXmlPropertyRaw(f));
                else
                    DataRecord.SetXmlProperty(f, info.GetXmlProperty(f));

                // if we have a image field then we need to create the imageurl field
                if (info.GetXmlProperty(f.Replace("textbox/", "hidden/hidinfo")) == "Img=True")
                {
                    DataRecord.SetXmlProperty(f.Replace("textbox/", "hidden/") + "url", StoreSettings.Current.FolderImages + "/" + info.GetXmlProperty(f.Replace("textbox/", "hidden/hid")));
                    DataRecord.SetXmlProperty(f.Replace("textbox/", "hidden/") + "path", StoreSettings.Current.FolderImagesMapPath  + "\\" + info.GetXmlProperty(f.Replace("textbox/", "hidden/hid")));
                }
                if (f == "genxml/dropdownlist/ddlparentcatid")
                {
                    var parentitemid = info.GetXmlProperty(f);
                    if (!Utils.IsNumeric(parentitemid)) parentitemid = "0";
                    if (DataRecord.ParentItemId != Convert.ToInt32(parentitemid))
                    {
                        _oldcatcascadeid = DataRecord.ParentItemId;
                        _doCascadeIndex = true;
                        DataRecord.ParentItemId = Convert.ToInt32(parentitemid);
                    }
                }
                DataLangRecord.RemoveXmlNode(f);
                
            }
        }

        public void ResetLanguage(String resetToLang)
        {
            if (resetToLang != DataLangRecord.Lang)
            {
                var resetToLangData = CategoryUtils.GetCategoryData(DataRecord.ItemID, resetToLang);
                DataLangRecord.XMLData = resetToLangData.DataLangRecord.XMLData;
                _objCtrl.Update(DataLangRecord);
            }
        }

        public List<NBrightInfo> GetDirectChildren()
        {
            var l = _objCtrl.GetList(_portalId, -1, "CATEGORY", " and NB1.ParentItemId = " + Info.ItemID.ToString(""));
            return l;
        }

        public List<NBrightInfo> GetAllArticles()
        {
            var l = _objCtrl.GetList(_portalId, -1, "CATXREF", " and NB1.XRefItemId = " + Info.ItemID.ToString(""));
            var l2 = _objCtrl.GetList(_portalId, -1, "CATCASCADE", " and NB1.XRefItemId = " + Info.ItemID.ToString(""));
            l.AddRange(l2);
            return l;
        }

        public List<NBrightInfo> GetDirectArticles()
        {
            var l = _objCtrl.GetList(_portalId, -1, "CATXREF", " and NB1.XRefItemId = " + Info.ItemID.ToString(""));
            return l;
        }

        public List<NBrightInfo> GetCascadeArticles()
        {
            var l = _objCtrl.GetList(_portalId, -1, "CATCASCADE", " and NB1.XRefItemId = " + Info.ItemID.ToString(""));
            return l;
        }

        public List<GroupData> GetFilterGroups(string lang)
        {
            var l = _objCtrl.GetList(_portalId, -1, "CATGRPXREF", " and NB1.ParentItemId = " + Info.ItemID.ToString(""));
            var rtnlist = new List<GroupData>();
            foreach (var nbi in l)
            {
                var c = new GroupData(nbi.XrefItemId, lang);
                if (c.Exists)
                {
                    rtnlist.Add(c);
                }
            }
            return rtnlist;
        }

        public int AddFilterGroup(int groupid)
        {
            var l = _objCtrl.GetList(_portalId, -1, "CATGRPXREF", " and NB1.ParentItemId = " + Info.ItemID.ToString("") + " and NB1.XrefItemId = " + groupid.ToString(""));
            if (l.Any())
            {
                var nbi = l.First();
                return nbi.ItemID;
            }
            else
            {
                var nbi = new NBrightInfo();
                nbi.XMLData = "<genxml><sort>1</sort><typecode>CAT</typecode></genxml>";
                nbi.ParentItemId = Info.ItemID;
                nbi.XrefItemId = groupid;
                nbi.PortalId = Info.PortalId;
                nbi.ModuleId = Info.ModuleId;
                nbi.TypeCode = "CATGRPXREF";
                nbi.ItemID = -1;
                return _objCtrl.Update(nbi);
            }
        }

        public void RemoveFilterGroup(int groupid)
        {
            var l = _objCtrl.GetList(_portalId, -1, "CATGRPXREF", " and NB1.ParentItemId = " + Info.ItemID.ToString("") + " and NB1.XrefItemId = " + groupid.ToString(""));
            if (l.Any())
            {
                var nbi = l.First();
                _objCtrl.Delete(nbi.ItemID);
            }
        }

        public int Validate()
        {
            var errorcount = 0;

            // default any undefined group type as category (I think quickcategory v1.0.0 plugin causes this)
            if (DataRecord.GetXmlProperty("genxml/dropdownlist/ddlgrouptype") == "")
            {
                DataRecord.SetXmlProperty("genxml/dropdownlist/ddlgrouptype", "cat");
                _objCtrl.Update(DataRecord);
            }

            if (GroupType == "cat")
            {
                // the base category ref cannot have language dependant refs, we therefore just use a unique key
                var catref = DataRecord.GetXmlProperty("genxml/textbox/txtcategoryref");
                if (catref == "" || DataRecord.GUIDKey == "")
                {
                    catref = Utils.GetUniqueKey().ToLower();
                    DataRecord.SetXmlProperty("genxml/textbox/txtcategoryref", catref);
                    DataRecord.GUIDKey = catref;
                    errorcount += 1;
                }

                if (DataRecord.GetXmlProperty("genxml/dropdownlist/ddlparentcatid") != DataRecord.ParentItemId.ToString())
                {
                    DataRecord.SetXmlProperty("genxml/dropdownlist/ddlparentcatid", DataRecord.ParentItemId.ToString());   
                }

            }

            DataRecord.ValidateXmlFormat();

            if (DataLangRecord == null)
            {
                // we have no datalang record for this language, so get an existing one and save it.
                var l = _objCtrl.GetList(_portalId, -1, "CATEGORYLANG", " and NB1.ParentItemId = " + Info.ItemID.ToString(""));
                if (l.Count > 0)
                {
                    DataLangRecord = (NBrightInfo)l[0].Clone();
                    DataLangRecord.ItemID = -1;
                    DataLangRecord.Lang = _lang;
                    DataLangRecord.ValidateXmlFormat();
                    _objCtrl.Update(DataLangRecord);
                }
            }

            // fix image
            var imgpath = DataRecord.GetXmlProperty("genxml/hidden/imagepath");
            var imgurl = DataRecord.GetXmlProperty("genxml/hidden/imageurl");
            var imagefilename = Path.GetFileName(imgpath);
            if (StoreSettings.Current != null) // current setting not valid in Scheduler.
            {
                if (!imgpath.StartsWith(StoreSettings.Current.FolderImagesMapPath))
                {
                    DataRecord.SetXmlProperty("genxml/hidden/imagepath", StoreSettings.Current.FolderImagesMapPath.TrimEnd('\\') + "\\" + imagefilename);
                    errorcount += 1;
                }
                if (imagefilename == "")
                {
                    DataRecord.SetXmlProperty("genxml/hidden/imagepath", "");
                    errorcount += 1;
                }
                if (!imgurl.StartsWith(StoreSettings.Current.FolderImages))
                {
                    DataRecord.SetXmlProperty("genxml/hidden/imageurl", StoreSettings.Current.FolderImages.TrimEnd('/') + "/" + imagefilename);
                    errorcount += 1;
                }
            }
            if (imagefilename == "")
            {
                DataRecord.SetXmlProperty("genxml/hidden/imageurl", "");
                errorcount += 1;
            }

            // check guidkey is correct
            if (DataRecord.GUIDKey != CategoryRef)
            {
                DataRecord.GUIDKey = CategoryRef;
                errorcount += 1;
            }

            if (errorcount > 0) _objCtrl.Update(DataRecord); // update if we find a error

            // fix langauge records
            foreach (var lang in DnnUtils.GetCultureCodeList(_portalId))
            {
                var l = _objCtrl.GetList(_portalId, -1, "CATEGORYLANG", " and NB1.ParentItemId = " + Info.ItemID.ToString("") + " and NB1.Lang = '" + lang + "'");
                if (l.Count == 0 && DataLangRecord != null)
                {
                    var nbi = (NBrightInfo)DataLangRecord.Clone();
                    nbi.ItemID = -1;
                    nbi.Lang = lang;
                    _objCtrl.Update(nbi);
                    errorcount += 1;
                }
                if (l.Count > 1)
                {
                    // we have more records than shoudl exists, remove any old ones.
                    var l2 = _objCtrl.GetList(_portalId, -1, "CATEGORYLANG", " and NB1.ParentItemId = " + Info.ItemID.ToString("") + " and NB1.Lang = '" + lang + "'", "order by Modifieddate desc");
                    var lp = 1;
                    foreach (var i in l2)
                    {
                      if (lp >=2) _objCtrl.Delete(i.ItemID);
                      lp += 1;
                    }
                }
            }

            // Build langauge refs
            if (GroupType == "cat")
            {
                var updaterequired = CategoryUtils.ValidateLangaugeRef(_portalId,CategoryId);
                if (updaterequired)
                {
                    // the catref has been updated, so reload the datarecord
                    DataLangRecord = _objCtrl.GetDataLang(CategoryId, _lang);
                }
            }

            // fix groups with mismatching ddlgrouptype
            if (GroupType != "cat")
            {
                var grp = _objCtrl.Get(DataRecord.ParentItemId, "GROUP");
                if (grp != null)
                {
                    if (grp.GUIDKey != GroupType)
                    {
                        DataRecord.SetXmlProperty("genxml/dropdownlist/ddlgrouptype", grp.GUIDKey);
                        _objCtrl.Update(DataRecord);
                        errorcount += 1;
                    }
                }
            }

            // update shared product if flagged
            // possible call from scheduler, no storesetting in that case.
            if ((StoreSettings.Current != null) && StoreSettings.Current.GetBool(StoreSettingKeys.sharecategories) && DataRecord.PortalId >= 0)
            {
                DataRecord.PortalId = -1;
                _objCtrl.Update(DataRecord);
                if (DataLangRecord != null)
                {
                    DataLangRecord.PortalId = -1;
                    _objCtrl.Update(DataLangRecord);
                }
            }


            return errorcount;
        }

        #endregion



        #region " private functions"

        private void LoadData(int categoryId)
        {
            Exists = false;
            if (categoryId == -1) categoryId = AddNew(); // add new record if -1 is used as id.
            if (_lang == "") _lang = Utils.GetCurrentCulture();
            Info = _objCtrl.Get(categoryId, "CATEGORYLANG", _lang);
            if (Info != null)
            {
                Exists = true;
                _portalId = Info.PortalId;
                DataRecord = _objCtrl.GetData(categoryId);
                DataLangRecord = _objCtrl.GetDataLang(categoryId, _lang);
                if (DataLangRecord == null) // rebuild langauge if we have a missing lang record
                {
                    Validate();
                    DataLangRecord = _objCtrl.GetDataLang(categoryId, _lang);
                }
            }
        }

        private int AddNew()
        {
            var nbi = new NBrightInfo(true);
            if (StoreSettings.Current.GetBool(StoreSettingKeys.sharecategories)) // option in storesetting to share products created here across all portals.
                _portalId = -1;
            else
                _portalId = PortalSettings.Current.PortalId;
            nbi.PortalId = _portalId;
            nbi.TypeCode = "CATEGORY";
            nbi.ModuleId = -1;
            nbi.ItemID = -1;
            nbi.SetXmlProperty("genxml/dropdownlist/ddlgrouptype", "cat");
            nbi.SetXmlProperty("genxml/checkbox/chkishidden", "True");
            nbi.SetXmlPropertyDouble("genxml/hidden/recordsortorder", 99999);
            var itemId = _objCtrl.Update(nbi);

            // update again, so we get a valid sort order based on itemid
            nbi.ItemID = itemId;
            nbi.SetXmlPropertyDouble("genxml/hidden/recordsortorder", itemId);
            _objCtrl.Update(nbi);

            foreach (var lang in DnnUtils.GetCultureCodeList(_portalId))
            {
                nbi = new NBrightInfo(true)
                {
                    PortalId = _portalId,
                    TypeCode = "CATEGORYLANG",
                    ModuleId = -1,
                    ItemID = -1,
                    Lang = lang,
                    ParentItemId = itemId
                };
                _objCtrl.Update(nbi);
            }

            return itemId;
        }


        #endregion
    }
}
