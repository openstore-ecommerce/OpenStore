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
    public class PropertyData : CategoryData
    {
        public PropertyData(string proprtyId, string lang) : base(proprtyId, lang)
        {
        }

        public PropertyData(int proprtyId, string lang) : base(proprtyId, lang)
        {
        }
    }
}
