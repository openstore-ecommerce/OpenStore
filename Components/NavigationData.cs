using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;

namespace Nevoweb.DNN.NBrightBuy.Components
{
    /// <summary>
    /// Class to deal with search cookie data.
    /// </summary>
    public class NavigationData
    {

        private int _portalId;
        private string _cookieName;
        private DataStorageType _storageType;
        private string _criteria;

        /// <summary>
        /// Populate class with cookie data
        /// </summary>
        /// <param name="portalId"> </param>
        /// <param name="moduleKey"> </param>
        /// <param name="storageType"> Select data storgae type "SessionMemory" or "Cookie" (Default Cookie) </param>
        /// <param name="nameAppendix">specifiy Unique key for search data</param>
        public NavigationData(int portalId, String moduleKey, string nameAppendix = "")
        {
            Exists = false;
            _portalId = portalId;
            _cookieName = "NBrightBuyNav" + "_" + moduleKey.Trim() + nameAppendix.Trim();
            _storageType = DataStorageType.Cookie; // force cookie, some issues with session memory.
            Get();
        }

        /// <summary>
        /// Build the SQL criteria form the xml field input and the template meta data
        /// </summary>

        public void Build(String xmlData, GenXmlTemplate templ)
        {
            Build(xmlData, templ.MetaTags);
        }

        public void Build(String xmlData, List<String> metaTags)
        {
            _criteria = "";
            var obj = new NBrightInfo();
            try
            {
                obj.XMLData = xmlData;
            }
            catch
            {
                //Just jump out without search.
            }

            // using a client side list of disabled token caused some issues with ajax. The new prefered method is to calc server side.
            var disabledtokens = GetDisabledTokens(obj);

            //Get only search tags
            var searchTags = new List<String>();
            foreach (var mta in metaTags)
            {
                var orderId = GenXmlFunctions.GetGenXmlValue(mta, "tag/@id");
                var active = GenXmlFunctions.GetGenXmlValue(mta, "tag/@active");
                if (active != "False" && orderId.ToLower().StartsWith("search") && !disabledtokens.Contains(orderId + ";"))
                {
                    searchTags.Add(mta);
                }
            }

            if (searchTags.Count > 0)
            {
                _criteria += ""; 
                var lp = 0;
                foreach (var mt in searchTags)
                {
                    lp += 1;
                    var action = GenXmlFunctions.GetGenXmlValue(mt, "tag/@action");
                    var search = GenXmlFunctions.GetGenXmlValue(mt, "tag/@search");
                    var sqlfield = GenXmlFunctions.GetGenXmlValue(mt, "tag/@sqlfield");
                    var sqlcol = GenXmlFunctions.GetGenXmlValue(mt, "tag/@sqlcol");
                    var searchfrom = GenXmlFunctions.GetGenXmlValue(mt, "tag/@searchfrom");
                    var searchto = GenXmlFunctions.GetGenXmlValue(mt, "tag/@searchto");
                    var sqltype = GenXmlFunctions.GetGenXmlValue(mt, "tag/@sqltype");
                    var sqloperator = GenXmlFunctions.GetGenXmlValue(mt, "tag/@sqloperator");
                    var sqlinject = GenXmlFunctions.GetGenXmlValue(mt, "tag/@sqlinject");

                    if (sqlfield == "") sqlfield = GenXmlFunctions.GetGenXmlValue(mt, "tag/@xpath"); //check is xpath of node ha been used.

                    if (lp == 1) sqloperator = ""; // use the "and" sepcified above for the first criteria.

                    // the < sign cannot be used in a XML attribute (it's illegal), so to do a xpath minimum value, we use a {LessThan} token.
                    // Ummm!!!. http://msdn.microsoft.com/en-us/library/ms748250(v=vs.110).aspx
                    if (sqlfield.Contains("{LessThan}")) sqlfield = sqlfield.Replace("{LessThan}", "<");
                    if (sqlfield.Contains("{GreaterThan}")) sqlfield = sqlfield.Replace("{GreaterThan}", ">"); // to keep it consistant

                    if (sqltype == "") sqltype = "nvarchar(max)";

                    if (sqlcol == "") sqlcol = "XMLData";

                    var searchVal = obj.GetXmlProperty(search);
                    if (searchVal == "") searchVal = GenXmlFunctions.GetGenXmlValue(mt, "tag/@static");

                    var searchValFrom = obj.GetXmlPropertyRaw(searchfrom);
                    var searchValTo = obj.GetXmlPropertyRaw(searchto);

                    if (sqltype.ToLower() == "datetime")
                    {
                        if (Utils.IsDate(searchValFrom))
                            searchValFrom = Convert.ToDateTime(searchValFrom).ToString("yyyy-MM-dd HH:mm:ss");
                        else
                            searchValFrom = "";
                        if (Utils.IsDate(searchValTo))
                        {
                            var searchToDate = Convert.ToDateTime(searchValTo);
                            searchToDate = searchToDate.AddDays(1);
                            var strippedToDate = searchToDate.Date;
                            searchValTo = strippedToDate.ToString("yyyy-MM-dd HH:mm:ss");                            
                        }
                        else
                            searchValTo = "";
                        if (Utils.IsDate(searchVal))
                            searchVal = Convert.ToDateTime(searchVal).ToString("yyyy-MM-dd HH:mm:ss");
                        else
                            searchVal = "";
                    }

                    switch (action.ToLower())
                    {
                        case "sqloperator":
                            _criteria += " " + sqloperator + " ";
                            break;
                        case "sqlinject":
                            _criteria += " " + sqlinject + " ";
                            break;
                        case "open":
                            _criteria += sqloperator + " ( ";
                            break;
                        case "close":
                            _criteria += " ) ";
                            break;
                        case "equal":
                            _criteria += " " + sqloperator + " " +
                                         GenXmlFunctions.GetSqlFilterText(sqlfield, sqltype, searchVal, sqlcol);
                            break;
                        case "not":
                            _criteria += " " + sqloperator + " " +
                                         GenXmlFunctions.GetSqlFilterText(sqlfield, sqltype, searchVal, sqlcol,"!=");
                            break;
                        case "like":
                            if (searchVal == "") searchVal = "NORESULTSnbright";
                            // for "like", build the sql so we have valid value, but add a fake search so the result is nothing for no selection values
                            _criteria += " " + sqloperator + " " + GenXmlFunctions.GetSqlFilterLikeText(sqlfield, sqltype, searchVal, sqlcol);

                            break;
                        case "range":
                            if (searchValFrom != "") // don't include search if we have no value input.
                            {
                                // We always need to return a value, otherwise we get an error, so range select cannot be empty. (we'll default here to 9999999)
                                if (searchValFrom == "")
                                {
                                    if (sqltype.ToLower() == "datetime")
                                        searchValFrom = "1800-01-01";
                                    else
                                        searchValFrom = "0";
                                }
                                if (searchValTo == "")
                                {
                                    if (sqltype.ToLower() == "datetime")
                                        searchValTo = "3000-12-30";
                                    else
                                        searchValTo = "999999999";
                                }

                                _criteria += " " + sqloperator + " " +
                                             GenXmlFunctions.GetSqlFilterRange(sqlfield, sqltype, searchValFrom, searchValTo, sqlcol);
                            }
                            else
                            {
                                _criteria += " 1 = 1 "; // add a dummy test, so we get a valid SQL structure.
                            }
                            break;
                        case "cats":
                            _criteria += " " + sqloperator + " ";
                            var selectoperator = GenXmlFunctions.GetGenXmlValue(mt, "tag/@selectoperator");
                            _criteria += BuildCategorySearch(search, obj, selectoperator);
                            break;
                        case "cat":
                            _criteria += " {criteriacatid} "; // add token for catergory search ()
                            break;
                    }
                }
            }
        }

        // calc any 
        private string GetDisabledTokens(NBrightInfo objInfo)
        {
            var searchData = new List<NavigationSearchData>();
            var disabledList = "";
            var hiddenList = objInfo.XMLDoc.SelectNodes("genxml/hidden/*");
            if (hiddenList == null) return "";
            foreach (XmlNode nod in hiddenList)
            {
                var sd = new NavigationSearchData(nod.InnerText);
                if (!String.IsNullOrEmpty(sd.Id) && sd.Id.ToLower().StartsWith("search"))
                {
                   searchData.Add(sd); 
                }
            }

            // checkboxes
            var ctrlList = objInfo.XMLDoc.SelectNodes("genxml/checkbox/*");
            if (ctrlList != null)
            {
                foreach (XmlNode nod in ctrlList)
                {
                    // add unchecked to the disabled list
                    if (!Convert.ToBoolean(nod.InnerText))
                    {
                        var f = searchData.FirstOrDefault(a => a.FieldId == nod.Name);
                        if (f != null) disabledList += f.Id + ";" + f.Dependency + ";";
                    }
                }
            }

            // textboxes
            ctrlList = objInfo.XMLDoc.SelectNodes("genxml/textbox/*");
            if (ctrlList != null)
            {
                foreach (XmlNode nod in ctrlList)
                {
                    // add empty to the disabled list
                    if (nod.InnerText == "")
                    {
                        var f = searchData.FirstOrDefault(a => a.FieldId == nod.Name);
                        if (f != null) disabledList += f.Id + ";" + f.Dependency + ";";
                    }
                }
            }

            // dropdownlist
            ctrlList = objInfo.XMLDoc.SelectNodes("genxml/dropdownlist/*");
            if (ctrlList != null)
            {
                foreach (XmlNode nod in ctrlList)
                {
                    // add empty to the disabled list
                    if (nod.InnerText == "")
                    {
                        var f = searchData.FirstOrDefault(a => a.FieldId == nod.Name);
                        if (f != null) disabledList += f.Id + ";" + f.Dependency + ";";
                    }
                }
            }


            return disabledList;
        }

        private String BuildCriteriaCatId()
        {
            var criteriacatid = "";
            if (CategoryId > 0)
            {
                var objQual = DotNetNuke.Data.DataProvider.Instance().ObjectQualifier;
                var dbOwner = DotNetNuke.Data.DataProvider.Instance().DatabaseOwner;
                criteriacatid += "and (NB1.[ItemId] in (select parentitemid from " + dbOwner + "[" + objQual + "NBrightBuy] where (typecode = 'CATCASCADE' or typecode = 'CATXREF') and (";
                criteriacatid += "XrefItemId = " + CategoryId;
                criteriacatid += " )))";
            }
            return criteriacatid;
        }

        private String BuildCategorySearch(String search, NBrightInfo searchData, String selectoperator)
        {
            var objQual = DotNetNuke.Data.DataProvider.Instance().ObjectQualifier;
            var dbOwner = DotNetNuke.Data.DataProvider.Instance().DatabaseOwner;

            // get list of selected categories.
            var catlist = new List<string>();
            var xmlNod = GenXmlFunctions.GetGenXmLnode(searchData.XMLData, search);
            if (xmlNod != null)
            {
                var xmlNodeList = xmlNod.SelectNodes("./chk");
                if (xmlNodeList != null)
                {
                    if (xmlNodeList.Count == 0)
                    {//dropdown list
                        catlist.Add(xmlNod.InnerText);
                    }
                    else
                    {// checkbox list
                        foreach (XmlNode xmlNoda in xmlNodeList)
                        {
                            if (xmlNoda.Attributes != null && xmlNoda.Attributes["value"] != null && xmlNoda.Attributes["data"] != null)
                            {
                                if (xmlNoda.Attributes["value"].Value.ToLower() == "true")
                                {
                                    catlist.Add(xmlNoda.Attributes["data"].Value);
                                }
                            }
                        }
                    }
                }
            }
            //build SQL
            var strRtn = "";
            if (catlist.Count > 0)
            {
                var categorylist = "";
                for (int i = 0; i < catlist.Count; i++)
                {
                    categorylist += catlist[i] + ",";
                }
                categorylist = categorylist.TrimEnd(',');

                if (selectoperator.ToLower() == "and")
                {
                    strRtn += " (select count(parentitemid) from " + dbOwner + "[" + objQual + "NBrightBuy] where typecode = 'CATXREF' and parentitemid = NB1.[ItemId] and XrefItemId in (" + categorylist + ")) = " + catlist.Count + " ";
                }
                else
                {
                    if (selectoperator.ToLower() == "cascade")
                    {
                        strRtn += "NB1.[ItemId] in (select parentitemid from " + dbOwner + "[" + objQual + "NBrightBuy] where (typecode = 'CATCASCADE' or typecode = 'CATXREF') and XrefItemId in (" + categorylist + ")) ";
                    }
                    else
                    {
                        strRtn += "NB1.[ItemId] in (select parentitemid from " + dbOwner + "[" + objQual + "NBrightBuy] where typecode = 'CATXREF' and XrefItemId in (" + categorylist + ")) ";
                    }
                }
            }
            else
            {
                // no categories selected, so add sql to stop display 
                strRtn += "NB1.[ItemId] = -1";
            }

            return strRtn;
        }

        /// <summary>
        /// Save cookie to client
        /// </summary>
        public void Save()
        {
            #region "Get temp filename"

            var tempfilename = "";


            if (_storageType == DataStorageType.SessionMemory)
            {
                if (HttpContext.Current.Session[_cookieName + "tempname"] != null) tempfilename = (String) HttpContext.Current.Session[_cookieName + "tempname"];
            }
            else
            {
                tempfilename = Cookie.GetCookieValue(_portalId, _cookieName, "tempname", "");
            }

            if (tempfilename == "") tempfilename = Utils.GetUniqueKey(12);

            if (_storageType == DataStorageType.SessionMemory)
            {
                HttpContext.Current.Session[_cookieName + "tempname"] = tempfilename;
            }
            else
            {
                Cookie.SetCookieValue(_portalId, _cookieName, "tempname", tempfilename, 1, "");
            }

            #endregion

            var nbi = new NBrightInfo(true);
            if (XmlData != "") nbi.XMLData = XmlData;

            nbi.SetXmlProperty("genxml/Criteria", _criteria);
            nbi.SetXmlProperty("genxml/PageModuleId", PageModuleId);
            nbi.SetXmlProperty("genxml/PageNumber", PageNumber);
            nbi.SetXmlProperty("genxml/PageName", PageName);
            nbi.SetXmlProperty("genxml/PageSize", PageSize);
            nbi.SetXmlProperty("genxml/OrderBy", OrderBy);
            nbi.SetXmlProperty("genxml/CategoryId", CategoryId.ToString("D"));
            nbi.SetXmlProperty("genxml/RecordCount", RecordCount);
            nbi.SetXmlProperty("genxml/Mode", Mode);
            nbi.SetXmlProperty("genxml/OrderByIdx", OrderByIdx);

            if (!String.IsNullOrEmpty(SearchFormData))
            {
                nbi.RemoveXmlNode("genxml/SearchFormData");
                nbi.SetXmlProperty("genxml/SearchFormData", "",TypeCode.String,false);
                nbi.AddXmlNode(SearchFormData,"genxml", "genxml/SearchFormData");
            }

            var filePath = StoreSettings.Current.FolderTempMapPath + "\\" + tempfilename;
            try
            {
                Utils.SaveFile(filePath, nbi.XMLData);

            }
            catch (Exception e)
            {
                // this is because the file's in use sometimes
                // we call it bad luck in those cases
            }

            Exists = true;
        }

        /// <summary>
        /// Get the cookie data from the client.
        /// </summary>
        /// <returns></returns>
        public NavigationData Get(string tempfilename = "")
        {
            ClearData();

            if (tempfilename == "")
            {
                if (_storageType == DataStorageType.SessionMemory)
                {
                    if (HttpContext.Current.Session[_cookieName + "tempname"] != null) tempfilename = (String)HttpContext.Current.Session[_cookieName + "tempname"];
                }
                else
                {
                    tempfilename = Cookie.GetCookieValue(_portalId, _cookieName, "tempname", "");
                }
            }

            XmlData = "";

            if (tempfilename != "")
            {
                var filePath = StoreSettings.Current.FolderTempMapPath + "\\" + tempfilename;
                if (File.Exists(filePath)) XmlData = Utils.ReadFile(filePath);
                var nbi = new NBrightInfo();
                nbi.XMLData = XmlData;

                _criteria = nbi.GetXmlProperty("genxml/Criteria");
                PageModuleId = nbi.GetXmlProperty("genxml/PageModuleId");
                PageNumber = nbi.GetXmlProperty("genxml/PageNumber");
                PageName = nbi.GetXmlProperty("genxml/PageName");
                PageSize = nbi.GetXmlProperty("genxml/PageSize");
                OrderBy = nbi.GetXmlProperty("genxml/OrderBy");
                CategoryId = Convert.ToInt32(nbi.GetXmlPropertyDouble("genxml/CategoryId"));
                RecordCount = nbi.GetXmlProperty("genxml/RecordCount");
                Mode = nbi.GetXmlProperty("genxml/Mode");
                OrderByIdx = nbi.GetXmlProperty("genxml/OrderByIdx");
                SearchFormData = nbi.GetXmlNode("genxml/SearchFormData").ToString();

            }

            if (_criteria == "" && XmlData == "") // "Exist" property not used for paging data
                Exists = false;
            else
                Exists = true;

            return this;
        }

        public string GetTempFileName()
        {
            ClearData();
            var tempfilename = "";
            if (_storageType == DataStorageType.SessionMemory)
            {
                if (HttpContext.Current.Session[_cookieName + "tempname"] != null) tempfilename = (String)HttpContext.Current.Session[_cookieName + "tempname"];
            }
            else
            {
                tempfilename = Cookie.GetCookieValue(_portalId, _cookieName, "tempname", "");
            }
            return tempfilename;
        }

        /// <summary>
        /// Delete cookie from client
        /// </summary>
        public void Delete()
        {
            ClearData();

            var tempfilename = "";

            if (_storageType == DataStorageType.SessionMemory)
            {
                if (HttpContext.Current.Session[_cookieName + "tempname"] != null) tempfilename = (String) HttpContext.Current.Session[_cookieName + "tempname"];
                if (HttpContext.Current.Session[_cookieName + "tempname"] != null) HttpContext.Current.Session.Remove(_cookieName + "Criteria");
            }
            else
            {
                tempfilename = Cookie.GetCookieValue(_portalId, _cookieName, "tempname", "");
                Cookie.RemoveCookie(_portalId, _cookieName);
            }

            Utils.DeleteSysFile(StoreSettings.Current.FolderTempMapPath + tempfilename);
            Exists = false;
        }

        public void ResetSearch()
        {
            _criteria = "";
            XmlData = "";
            SearchFormData = "";
            Save();
        }


        private void ClearData()
        {
            _criteria = "";
            PageModuleId = "";
            PageNumber = "";
            PageName = "";
            OrderBy = "";
            XmlData = "";
            CategoryId = 0;
            PageSize = "";
            RecordCount = "";
            Mode = "";
            OrderByIdx = "";
        }

        /// <summary>
        /// Set to true if cookie exists
        /// </summary>
        public bool Exists { get; private set; }

        /// <summary>
        /// Search Criteria, partial SQL String
        /// </summary>
        public string Criteria
        {
            get
            {
                // categoryid is sometimes known/ or diofferent when crteria is first built.
                // so we use a token and replace when we know the categoryid have been assigned.
                // Us HadCriteria property to test if criteria has been assigned.
                var criteria = _criteria.Replace("{criteriacatid}", BuildCriteriaCatId());
                if (criteria.Trim() == "") return "";
                if (!criteria.Trim().ToLower().StartsWith("and")) criteria = " and ( " + criteria + " )"; //wrap criteria into a AND, if not already.
                return criteria; 
            }
        }

        public bool HasCriteria
        {
            get
            {
                if (_criteria != "") return true;
                return false;
            }
        }

        /// <summary>
        /// selected page
        /// </summary>
        public string PageNumber { get; set; }

        /// <summary>
        /// selected pagemid
        /// </summary>
        public string PageModuleId { get; set; }

        /// <summary>
        /// Page Name, used to return to page with correct page name 
        /// </summary>
        public string PageName { get; set; }

        /// <summary>
        /// Page Size 
        /// </summary>
        public string PageSize { get; set; }

        /// <summary>
        /// Save the sort order of the last required
        /// </summary>
        public string OrderBy { get; set; }

        /// <summary>
        /// Save the sort order index key 
        /// </summary>
        public string OrderByIdx { get; set; }

        /// <summary>
        /// Save form xml data (this could be large, be careful on the cookie size)
        /// </summary>
        public string XmlData { get; set; }

        /// <summary>
        /// CategoryId Selected
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// Count of records returned on last Display
        /// </summary>
        public string RecordCount { get; set; }

        /// <summary>
        /// Mode:  "F" = filter will persist past category selection, "S" = SingleSearchMode (The filter will only exist for 1 search) 
        /// </summary>
        public string Mode { get; set; }

        /// <summary>
        /// Search Criteria, partial SQL String
        /// </summary>
        public bool FilterMode
        {
            get
            {
                return Mode.ToLower() == "f";
            }
        }

        public bool SingleSearchMode
        {
            get
            {
                return Mode.ToLower() == "s";
            }
        }
        /// <summary>
        /// Store search form data so wee can redisplay correct settings on filter form.
        /// </summary>
        public string SearchFormData { get; set; }

    }

    public class NavigationSearchData
    {
        public NavigationSearchData(string xmltag)
        {
            Id = "";
            FieldId = "";
            Action = "";
            SqlOperator = "";
            SearchFrom = "";
            SearchTo = "";
            SqlType = "";
            SearchField = "";
            SqlField = "";
            ControlType = "";
            Static = "";
            Dependency = "";
            if (xmltag.Trim().StartsWith("<tag"))
            {
                var xdoc = new XmlDataDocument();
                xdoc.LoadXml(xmltag);
                var nodtag = xdoc.SelectSingleNode("tag");
                if (nodtag != null)
                {
                    if (nodtag.Attributes["search"] != null)
                    {
                        SearchField = nodtag.Attributes["search"].Value;
                        var s = SearchField.Split('/');
                        FieldId = s[s.Count() - 1];
                        ControlType = s[s.Count() - 2];
                    }
                    if (nodtag.Attributes["id"] != null) Id = nodtag.Attributes["id"].Value;
                    if (nodtag.Attributes["action"] != null) Action = nodtag.Attributes["action"].Value;
                    if (nodtag.Attributes["sqloperator"] != null) SqlOperator = nodtag.Attributes["sqloperator"].Value;
                    if (nodtag.Attributes["searchfrom"] != null) SearchFrom = nodtag.Attributes["searchfrom"].Value;
                    if (nodtag.Attributes["searchto"] != null) SearchTo = nodtag.Attributes["searchto"].Value;
                    if (nodtag.Attributes["sqltype"] != null) SqlType = nodtag.Attributes["sqltype"].Value;
                    if (nodtag.Attributes["sqlfield"] != null) SqlField = nodtag.Attributes["sqlfield"].Value;
                    if (nodtag.Attributes["static"] != null) Static = nodtag.Attributes["static"].Value;
                    if (nodtag.Attributes["dependency"] != null) Dependency = nodtag.Attributes["dependency"].Value;
                    

                }
            }
        }

        public string Id { get; set; }
        public string FieldId { get; set; }
        public string Action { get; set; }
        public string SqlOperator { get; set; }
        public string SearchFrom { get; set; }
        public string SearchTo { get; set; }
        public string SqlType { get; set; }
        public string SearchField { get; set; }
        public string SqlField { get; set; }
        public string ControlType { get; set; }
        public string Static { get; set; }
        public string Dependency { get; set; }

    }
}
