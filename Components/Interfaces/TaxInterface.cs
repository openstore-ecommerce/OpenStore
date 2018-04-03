
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

namespace Nevoweb.DNN.NBrightBuy.Components.Interfaces
{


	public abstract class TaxInterface
	{

		#region "Shared/Static Methods"

        // constructor
        static TaxInterface()
		{
			CreateProvider();
		}

        //private static ShippingInterface objProvider = null;
        private static Dictionary<String, TaxInterface> _providerList; 

        // dynamically create provider
        private static void CreateProvider()
        {

            _providerList = new Dictionary<string, TaxInterface>();

            var pluginData = new PluginData(PortalSettings.Current.PortalId);
            var l = pluginData.GetTaxProviders(false);

            foreach (var p in l)
            {
                var prov = p.Value;
                ObjectHandle handle = null;
                handle = Activator.CreateInstance(prov.GetXmlProperty("genxml/textbox/assembly"),
                prov.GetXmlProperty("genxml/textbox/namespaceclass"));
                var objProvider = (TaxInterface)handle.Unwrap();
                var ctrlkey = prov.GetXmlProperty("genxml/textbox/ctrl");
                if (!_providerList.ContainsKey(ctrlkey))
                {
                    _providerList.Add(ctrlkey, objProvider);
                }
            }

        }

        // return the provider
        public static TaxInterface Instance(String ctrlkey)
        {
            if (_providerList.ContainsKey(ctrlkey)) return _providerList[ctrlkey];
            if (_providerList.Count > 0) return _providerList.Values.First();
            return null;
        }

		#endregion

        public abstract NBrightInfo Calculate(NBrightInfo cartInfo);
	    public abstract Double CalculateItemTax(NBrightInfo cartItemInfo);
        public abstract Dictionary<String, Double> GetRates();
        public abstract Dictionary<String, String> GetName();


	}

}

