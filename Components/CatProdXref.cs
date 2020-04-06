using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Content.Common;
using DotNetNuke.Entities.Portals;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;

namespace Nevoweb.DNN.NBrightBuy.Components
{
    public class CatProdXref
    {
        private String _lang = "";
        public List<String> CatRefProdList;
        private String strCacheKey;

        public CatProdXref()
        {
            Load();
        }

        #region "base methods"

        public void Reload()
        {
            ClearCache();
            Load();
        }

        public void ClearCache()
        {
            strCacheKey = "CarProdXrefList_" + PortalSettings.Current.PortalId;
            CacheUtils.RemoveCache(strCacheKey);
        }

        public Boolean IsProductInCategory(int productid, String categoryRef)
        {
            var s = categoryRef + "-" + productid.ToString("");
            if (CatRefProdList.Contains(s)) return true;
            return false;
        }

        #endregion


        private void Load()
        {
            strCacheKey = "CarProdXrefList_" + PortalSettings.Current.PortalId;
            CatRefProdList = (List<String>)CacheUtils.GetCache(strCacheKey);
            if (CatRefProdList == null)
            {
                CatRefProdList = new List<string>();

                var objQual = DotNetNuke.Data.DataProvider.Instance().ObjectQualifier;
                var dbOwner = DotNetNuke.Data.DataProvider.Instance().DatabaseOwner;

                var sql = "SELECT NB1.ParentItemId as productid ,NB1.XrefItemId as categoryid,nb2.GUIDKey as categoryref FROM " + dbOwner + "[" + objQual + "NBrightBuy] as NB1 inner join " + dbOwner + "[" + objQual + "NBrightBuy] as NB2 on nb2.ItemId = nb1.XrefItemId where nb1.typecode = 'CATCASCADE' or nb1.typecode = 'CATXREF' for xml path('item'), root('root')";
                var objCtrl = new NBrightBuyController();
                var strOut = objCtrl.GetSqlxml(sql);
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(strOut);
                var nodList = xmlDoc.SelectNodes("root/item");
                if (nodList != null)
                {
                    foreach (XmlNode nod in nodList)
                    {
                        if (nod.SelectSingleNode("categoryref") != null && nod.SelectSingleNode("productid") != null)
                        {
                            var s = nod.SelectSingleNode("categoryref").InnerText + "-" + nod.SelectSingleNode("productid").InnerText;
                            CatRefProdList.Add(s);
                        }
                    }
                }
                CacheUtils.SetCache(strCacheKey, CatRefProdList);
            }

        }

    }
}
