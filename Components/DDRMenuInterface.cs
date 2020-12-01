using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Web.DDRMenu;
using NBrightCore.common;

namespace Nevoweb.DNN.NBrightBuy.Components
{
    public class DdrMenuInterface : INodeManipulator
    {
        #region Implementation of INodeManipulator
		String _tabid = "";

        private static string lockobjectManipulateNodes = "lockit";
        public List<MenuNode> ManipulateNodes(List<MenuNode> nodes, DotNetNuke.Entities.Portals.PortalSettings portalSettings)
        {
            var parentcatref = "";
            // jump out if we don't have [CAT] token in nodes
            if (nodes.Count(x => x.Text.ToUpper() == "[CAT]") == 0 && nodes.Count(x => x.Text.ToUpper().StartsWith("[CAT:")) == 0)
            {
                return nodes;
            }

            // use cache ()
            var nodeTabList = "*";
            foreach (var n in nodes)
            {
                nodeTabList += n.Text + n.TabId + "*" + n.Breadcrumb + "*";
            }
            var cachekey = "NBrightPL*" + portalSettings.PortalId + "*" + Utils.GetCurrentCulture() + "*" + nodeTabList; // use nodeTablist incase the DDRMenu has a selector.
            var rtnnodes = (List<MenuNode>)Utils.GetCache(cachekey);
            if (rtnnodes != null) return rtnnodes;

            lock (lockobjectManipulateNodes)
            {
                var categoryInjectList = new Dictionary<int, int>();
                var idx1 = 0;
                var listCats = new List<int>();
                if (nodes.Count(x => x.Text.ToUpper().StartsWith("[CAT")) > 0)
                {
                    // find the selected node.
                    var nods = nodes.Where(x => (x.Text.ToUpper().StartsWith("[CAT"))).ToList();
                    foreach (var n in nods)
                    {
                        if (n.Text.ToUpper().StartsWith("[CAT:"))
                        {
                            var s = n.Text.Split(':');
                            if (s.Count() >= 2)
                            {
                                parentcatref = s[1].TrimEnd(']');
                                var objCtrl = new NBrightBuyController();
                                var parentcat = objCtrl.GetByGuidKey(PortalSettings.Current.PortalId, -1, "CATEGORY", parentcatref);
                                if (parentcat != null)
                                {
                                    listCats.Add(parentcat.ItemID);
                                    if (!categoryInjectList.ContainsKey(idx1)) categoryInjectList.Add(idx1, n.TabId);
                                    idx1 += 1;
                                }
                            }
                        }
                        else
                        {
                            if (!categoryInjectList.ContainsKey(idx1)) categoryInjectList.Add(0, n.TabId);
                            listCats.Add(0);
                        }
                    }
                }
                var lp = 0;
                foreach (var parentItemId in listCats)
                {

                    _tabid = PortalSettings.Current.ActiveTab.TabID.ToString("");

                    var defaultListPage = "";
                    defaultListPage = StoreSettings.Current.Get("productlisttab");

                    var catNodeList = GetCatNodeXml(_tabid, categoryInjectList[lp], parentItemId, true, 0, null, defaultListPage);

                    // see if we need to merge into the current pages, by searching for marker page [cat]
                    int idx = 0;
                    var catNods = new Dictionary<int, MenuNode>();
                    foreach (var n in nodes)
                    {
                        if (n.Text.ToLower() == "[cat]" || n.Text.ToLower().StartsWith("[cat:"))
                        {
                            if (!catNods.ContainsKey(idx)) catNods.Add(idx, n);
                            break;
                        }
                        idx += 1;
                    }


                    if (catNods.Count > 0)
                    {
                        foreach (var catNod in catNods)
                        {
                            // remove marker page [cat]
                            nodes.Remove(catNod.Value);
                        }
                        var insidx = idx;
                        foreach (var n2 in catNodeList)
                        {
                            if (n2.Parent == null)
                            {
                                nodes.Insert(insidx, n2);
                            }
                            insidx += 1;
                        }
                    }
                    lp += 1;
                }

                Utils.SetCacheList(cachekey, nodes, "category_cachelist");
            }
            return nodes;
        }


        private List<MenuNode> GetCatNodeXml(string currentTabId, int categoryInjectTabId, int parentItemId = 0, bool recursive = true, int depth = 0, MenuNode pnode = null, string defaultListPage = "")
        {
            
            var nodes = new List<MenuNode>();
            //[TODO: Add images onto DDRMenu]
            //var objS = objCtrl.GetByGuidKey(PortalSettings.Current.PortalId, -1, "SETTINGS", "NBrightBuySettings");
            //var imgFolder = objS.GetXmlProperty("genxml/textbox/txtuploadfolder");
            //var defimgsize = objS.GetXmlProperty("genxml/textbox/txtsmallimgsize");

            //var l = objCtrl.GetList(PortalSettings.Current.PortalId, -1, "CATEGORY", strFilter, strOrderBy, 0, 0, 0, 0, "CATEGORYLANG", Utils.GetCurrentCulture());

            var grpCatCtrl = new GrpCatController(Utils.GetCurrentCulture());

            var l = grpCatCtrl.GetCategories(parentItemId);

            foreach (var obj in l)
            {
                if (!obj.ishidden)
                {

                    var n = new MenuNode();

                    n.Parent = pnode;

                    n.TabId = categoryInjectTabId;
                    n.Text = obj.categoryname;
                    n.Title = obj.categorydesc;

                    var tabid = "";
                    if (Utils.IsNumeric(defaultListPage)) tabid = defaultListPage;
                    if (tabid == "") tabid = currentTabId;
                    if (Utils.IsNumeric(tabid)) n.Url = grpCatCtrl.GetCategoryUrl(obj, Convert.ToInt32((tabid)));

                    n.Enabled = true;
                    if (obj.disabled) n.Enabled = false;
                    n.Selected = false;
                    // redundant with caching
                    //if (_catid == obj.categoryid.ToString("")) n.Selected = true;
                    n.Breadcrumb = false;
                    //if (_catid == obj.categoryid.ToString("")) n.Breadcrumb = true;
                    n.Separator = false;
                    n.LargeImage = "";
                    n.Icon = "";
                    var img = obj.imageurl;
                    if (img != "")
                    {
                        n.LargeImage = img;
                        n.Icon = StoreSettings.NBrightBuyPath() + "/NBrightThumb.ashx?w=50&h=50&src=/" + img.TrimStart('/');
                    }
                    n.Keywords = obj.metakeywords;
                    n.Description = obj.metadescription;
                    n.CommandName = "";
                    //n.CommandArgument = string.Format("entrycount={0}|moduleid={1}", obj.GetXmlProperty("genxml/hidden/entrycount"), obj.ModuleId.ToString(""));
                    n.CommandArgument = obj.entrycount.ToString(""); // not used, so we use it to store the entry count

                    if (recursive && depth < 5) //stop infinate loop, only allow 50 sub levels
                    {
                        depth += 1;
                        var childrenNodes = GetCatNodeXml(tabid, categoryInjectTabId, obj.categoryid, true, depth, n, defaultListPage);
                        if (childrenNodes.Count > 0)
                        {
                            n.Children = childrenNodes;
                        }
                    }

                    nodes.Add(n);
                }

            }

            return nodes;

        }


        #endregion
    }

   
}
