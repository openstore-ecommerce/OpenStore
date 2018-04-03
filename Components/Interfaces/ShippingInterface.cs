
using System.Linq;
using DotNetNuke.Entities.Portals;
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;


using System.Runtime.Remoting;
using NBrightDNN;
using RazorEngine.Templating;

namespace Nevoweb.DNN.NBrightBuy.Components.Interfaces
{


	public abstract class ShippingInterface
	{

		#region "Shared/Static Methods"

		// singleton reference to the instantiated object 

	    private static Dictionary<String,ShippingInterface> _providerList;
	    private static ShippingInterface _defaultProvider;
        // constructor
        static ShippingInterface()
		{
			CreateProvider();
		}

		// dynamically create provider
		private static void CreateProvider()
		{

			string providerName = null;

		    _providerList = new Dictionary<string, ShippingInterface>();

            var pluginData = new PluginData(PortalSettings.Current.PortalId);
		    var l = pluginData.GetShippingProviders(false);

		    foreach (var p in l)
		    {
		        var prov = p.Value;
		        ObjectHandle handle = null;
		        handle = Activator.CreateInstance(prov.GetXmlProperty("genxml/textbox/assembly"), prov.GetXmlProperty("genxml/textbox/namespaceclass"));
		        var objProvider = (ShippingInterface) handle.Unwrap();
		        var ctrlkey = prov.GetXmlProperty("genxml/textbox/ctrl");
		        var lp = 1;
		        while (_providerList.ContainsKey(ctrlkey))
		        {
		            ctrlkey = ctrlkey + lp.ToString("");
		            lp += 1;
		        }
		        objProvider.Shippingkey = ctrlkey;
		        _providerList.Add(ctrlkey, objProvider);
		        if (prov.GetXmlPropertyBool("genxml/checkbox/default"))
		        {
		            _defaultProvider = objProvider;
		        }
		    }

		}


		// return the provider
        public static ShippingInterface Instance(String ctrlkey)
		{
            if (ctrlkey == "")
            {
                return _defaultProvider;
            }
            else
            {
                if (_providerList.ContainsKey(ctrlkey)) return _providerList[ctrlkey];
                if (_providerList.Count > 0) return _providerList.Values.First();
            }
            return null;
		}

		#endregion

        public abstract String Shippingkey { get; set; }

        public abstract NBrightInfo CalculateShipping(NBrightInfo cartInfo);

        public abstract String Name();

        public abstract String GetTemplate(NBrightInfo cartInfo);

        public abstract String GetDeliveryLabelUrl(NBrightInfo cartInfo);

        public abstract Boolean IsValid(NBrightInfo cartInfo);

    }

}

