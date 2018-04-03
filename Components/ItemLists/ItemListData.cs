using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Web;
using System.Windows.Forms.VisualStyles;
using System.Xml;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;

namespace Nevoweb.DNN.NBrightBuy.Components
{

    /// <summary>
    /// Class to deal with itemlist cookie data.
    /// </summary>
    public class ItemListData
    {

        private HttpCookie _cookie;
        private ClientData _clientData;

        public Dictionary<string, string> listnames;
        public string products;
        public Dictionary<string, string> productsInList;
        public string listkeys;

        public int UserId;

        /// <summary>
        /// Populate class with cookie data
        /// </summary>
        /// <param name="listName"></param>
        public ItemListData(int portalId, int userId)
        {
            UserId = userId;
            CookieName = "NBSShoppingList";
            Exists = false;
            ItemCount = 0;
            listnames = new Dictionary<string, string>();
            products = "";
            productsInList = new Dictionary<string, string>();

            if (UserId > 0)
            {
                _clientData = new ClientData(portalId, UserId);
                if (_clientData.Exists)
                {
                    Get();
                    SaveCookie();
                }
            }
        }

        /// <summary>
        /// Save cookie to client
        /// </summary>
        public void SaveAll()
        {
            // if user then get from clientdata
            if (UserId > 0)
            {
                if (_clientData.Exists)
                {
                    products = "";
                    listkeys = "";
                    foreach (var lname in listnames)
                    {
                        listkeys += lname.Key + "*";
                        _clientData.UpdateItemList(lname.Key, productsInList[lname.Key],lname.Value);
                        var l = _clientData.GetItemList(lname.Key);
                        products += l;
                    }
                    _clientData.Save();
                    SaveCookie();
                }
            }
        }

        public void SaveList(string listkey,string listname)
        {
            listkey = CleanListKey(listkey);
            // if user then get from clientdata
            if (UserId > 0)
            {
                if (_clientData.Exists)
                {
                    _clientData.UpdateItemList(listkey, productsInList[listkey], listname);
                    _clientData.Save();
                    products = "";
                    listkeys = "";
                    foreach (var list in listnames)
                    {
                        listkeys += list.Key + "*";
                        var l = _clientData.GetItemList(list.Key);
                        products += l;
                    }
                    SaveCookie();
                }
            }
        }

        public void SaveCookie()
        {
            if (products.Length > 0)
            {
                Exists = true;
            }

            _cookie = HttpContext.Current.Request.Cookies[CookieName];
            if (_cookie == null)
            {
                _cookie = new HttpCookie(CookieName);
            }
            _cookie[CookieName] = products;
            _cookie.Expires = DateTime.MinValue;
            HttpContext.Current.Response.Cookies.Add(_cookie);
        }

        /// <summary>
        /// Get the cookie data from the client.
        /// </summary>
        /// <returns></returns>
        public ItemListData Get()
        {
                // if user then get from clientdata
            if (UserId > 0)
            {
                products = "";
                if (_clientData.Exists)
                {
                    listkeys = "";
                    listnames = _clientData.GetItemListNames();
                    foreach (var list in listnames)
                    {
                        listkeys += list.Key + "*";
                        var l = _clientData.GetItemList(list.Key);
                        productsInList.Add(list.Key, l);
                        products += l;
                    }
                }
            }


            if (products.Length == 0)
            {
                Exists = false;
            }
            else
            {
                Exists = true;
                ItemCount = products.Length;
            }

            return this;
        }

        /// <summary>
        /// Delete cookie from client
        /// </summary>
        public void DeleteList(string listkey)
        {
            listkey = CleanListKey(listkey);
            if (productsInList.ContainsKey(listkey))
            {
                if (UserId > 0)
                {
                    if (_clientData.Exists)
                    {
                        _clientData.DataRecord.RemoveXmlNode("genxml/itemlists/" + listkey);
                        _clientData.Save();
                    }
                }
                listnames.Remove(listkey);
                productsInList.Remove(listkey);
                SaveAll();
            }
        }

        public void Add(string listkey, string itemId)
        {
            var listkey1 = listkey;
            listkey = CleanListKey(listkey);
            if (productsInList.ContainsKey(listkey))
            {
                productsInList[listkey] = productsInList[listkey] + itemId + "*";
            }
            else
            {
                productsInList.Add(listkey, itemId + "*");
                listnames.Add(listkey, listkey1);
            }
            SaveList(listkey, listkey1);
        }

        /// <summary>
        /// remove item from wishlist
        /// </summary>
        /// <param name="itemId"></param>
        public void Remove(string listkey, string itemId)
        {
            if (listkey == "")
            {
                foreach (var lname in listnames)
                {
                    if (productsInList.ContainsKey(lname.Key))
                    {
                        productsInList[lname.Key] = productsInList[lname.Key].Replace(itemId + "*", "");
                        SaveList(lname.Key, "");
                    }
                }
            }
            else
            {
                listkey = CleanListKey(listkey);
                if (productsInList.ContainsKey(listkey))
                {
                    productsInList[listkey] = productsInList[listkey].Replace(itemId + "*", "");
                    SaveList(listkey, "");
                }
            }
        }

        /// <summary>
        /// Return a generic list of itemid in the cookie
        /// </summary>
        /// <returns></returns>
        public List<String> GetItemList()
        {
            if (products == "") return null;
            var l = products.Split('*');
            var gl = new List<String>();
            foreach (var s in l)
            {
                if (s != "") gl.Add(s);
            }
            if (gl.Count == 0) return null;
            return gl;
        }
        public List<String> GetItemList(string listkey)
        {
            var gl = new List<String>();
            listkey = CleanListKey(listkey);
            if (products == "" || !productsInList.ContainsKey(listkey) || productsInList[listkey] == "") return gl;
            var l = productsInList[listkey].Split('*');
            foreach (var s in l)
            {
                if (s != "") gl.Add(s);
            }
            return gl;
        }

        public Boolean IsInList(String itemid)
        {
            if (products.Length == 0) return false;
            return GetItemList().Contains(itemid);
        }

        /// <summary>
        /// Cookie name
        /// </summary>
        public string CookieName { get; private set; }

        /// <summary>
        /// Set to true if cookie exists
        /// </summary>
        public bool Exists { get; private set; }

        /// <summary>
        /// Count of itemids to be included in the list
        /// </summary>
        public int ItemCount { get; private set; }



        private string CleanListKey(string listkey)
        {
            listkey = listkey.Replace(" ", "-");
            if (Regex.IsMatch(listkey, @"^\d"))
            {
                listkey = "lst" + listkey;  //elements that start with numeric are not allow in XML
            }
            return Regex.Replace(listkey, "[^a-zA-Z0-9]+", "", RegexOptions.Compiled);
        }

    }


}
