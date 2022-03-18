
using System.Linq;
using System.Web;
using DotNetNuke.Entities.Portals;
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;


using System.Runtime.Remoting;
using NBrightDNN;

namespace Nevoweb.DNN.NBrightBuy.Components.Interfaces
{


	public abstract class FilterInterface
	{

		#region "Shared/Static Methods"

		// singleton reference to the instantiated object 

        //private static ShippingInterface objProvider = null;
        private static Dictionary<String, FilterInterface> _providerList; 
        // constructor
        static FilterInterface()
		{
			CreateProvider();
		}

		// dynamically create provider
		private static void CreateProvider()
		{

            _providerList = new Dictionary<string, FilterInterface>();

            var pluginData = new PluginData(PortalSettings.Current.PortalId);
            var l = pluginData.GetFilterProviders(false).Where(x => x.Value.GetXmlPropertyBool("genxml/checkbox/active"));

            foreach (var p in l)
            {
                var prov = p.Value;
                ObjectHandle handle = null;
                handle = Activator.CreateInstance(prov.GetXmlProperty("genxml/textbox/assembly"),
                    prov.GetXmlProperty("genxml/textbox/namespaceclass"));
                var objProvider = (FilterInterface)handle.Unwrap();
                var ctrlkey = prov.GetXmlProperty("genxml/textbox/ctrl");
                if (!_providerList.ContainsKey(ctrlkey))
                {
                    if (!_providerList.ContainsKey(ctrlkey)) _providerList.Add(ctrlkey, objProvider);
                }
            }

		}


		// return the provider
        public static FilterInterface Instance(String ctrlkey)
		{
            if (_providerList.ContainsKey(ctrlkey)) return _providerList[ctrlkey];
            if (_providerList.Count > 0) return _providerList.Values.First();
            return null;
		}

		#endregion

        public abstract String GetFilter(String currentFilter, NavigationData navigationData, ModSettings setting, NBrightInfo ajaxInfo);

    }

}

