using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DotNetNuke.Entities.Portals;
using NBrightCore.common;
using NBrightDNN;

namespace Nevoweb.DNN.NBrightBuy.Components
{
    public class CategoryUtils
    {

        public static String GetCatIdFromName(String catname)
        {
            var catid = "0";
            if (catname != "")
                {
                    var objCtrl = new NBrightBuyController();
                    var objCat = objCtrl.GetByGuidKey(PortalSettings.Current.PortalId, -1, "CATEGORYLANG", catname);
                    if (objCat == null)
                    {
                        // check it's not just a single language
                        objCat = objCtrl.GetByGuidKey(PortalSettings.Current.PortalId, -1, "CATEGORY", catname);
                        if (objCat != null) catid = objCat.ItemID.ToString("");
                    }
                    else
                    {
                        catid = objCat.ParentItemId.ToString("");
                    }
                }


            return catid;
        }

        #region "Cacheing"

        /// <summary>
        /// Get ProductData class with cacheing
        /// </summary>
        /// <param name="categoryId"></param>
        /// <param name="lang"></param>
        /// <returns></returns>
        public static CategoryData GetCategoryData(int categoryId, String lang)
        {
            CategoryData catData;
            var cacheKey = "NBSCategoryData*" + categoryId.ToString("") + "*" + lang;
            catData = (CategoryData)NBrightBuyUtils.GetModCache(cacheKey);
            if (catData == null)
            {
                catData = new CategoryData(categoryId, lang);
                NBrightBuyUtils.SetModCache(-1, cacheKey, catData); // use module cache, so recussive changes are release from cache on update.
            }
            return catData;
        }

        public static CategoryData GetCategoryData(String categoryId, String lang)
        {
            if (Utils.IsNumeric(categoryId))
            {
                return GetCategoryData(Convert.ToInt32(categoryId), lang);
            }
            return null;
        }

        public static Boolean ValidateLangaugeRef(int portalId, int categoryId)
        {
            var updaterequired = false;
            foreach (var lang in DnnUtils.GetCultureCodeList(portalId))
            {
                var objCtrl = new NBrightBuyController();
                var parentCatData = GetCategoryData(categoryId, lang);
                var grpCatCtrl = new GrpCatController(lang);
                var newGuidKey = grpCatCtrl.GetBreadCrumb(categoryId, 0, "-", false,true);
                if (newGuidKey != "") newGuidKey = GetUniqueGuidKey(portalId, categoryId, Utils.UrlFriendly(newGuidKey)).ToLower();
                if (parentCatData.DataLangRecord.GUIDKey != newGuidKey)
                {
                    parentCatData.DataLangRecord.SetXmlProperty("genxml/textbox/txtcategoryref", newGuidKey);
                    parentCatData.DataLangRecord.GUIDKey = newGuidKey;
                    objCtrl.Update(parentCatData.DataLangRecord);
                    updaterequired = true;
                    // need to update all children, so call validate recursive.
                    foreach (var ch in parentCatData.GetDirectChildren())
                    {
                        if (ch.ItemID != categoryId) ValidateLangaugeRef(portalId, ch.ItemID);
                    }
                }
            }
            return updaterequired;
        }

        public static string GetUniqueGuidKey(int portalId, int categoryId, string newGUIDKey)
        {
            // make sure we have a unique guidkey
            var objCtrl = new NBrightBuyController();
            var doloop = true;
            var lp = 1;
            var testGUIDKey = newGUIDKey.ToLower();
            while (doloop)
            {
                var obj = objCtrl.GetByGuidKey(portalId, -1, "CATEGORY", testGUIDKey);
                if (obj != null && obj.ItemID != categoryId)
                {
                    testGUIDKey = newGUIDKey + lp;
                }
                else
                    doloop = false;

                lp += 1;
                if (lp > 999) doloop = false; // make sure we never get a infinate loop
            }
            return testGUIDKey;
        }

        #endregion
    }
}
