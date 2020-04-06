
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
	public abstract class ImageUploadInterface
    {
        public abstract NBrightInfo ProductImage(NBrightInfo nbrightInfo);
        public abstract NBrightInfo CategoryImage(NBrightInfo nbrightInfo);
	}
}

