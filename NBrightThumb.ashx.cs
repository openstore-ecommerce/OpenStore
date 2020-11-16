using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Web;
using NBrightCore.common;
using NBrightCore.images;
using Nevoweb.DNN.NBrightBuy.Components;

namespace Nevoweb.DNN.NBrightBuy
{
    /// <summary>
    /// Summary description for NBrightThumb1
    /// </summary>
    public class NBrightThumb : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {

            var w = Utils.RequestQueryStringParam(context, "w");
            var h = Utils.RequestQueryStringParam(context, "h");
            var src = Utils.RequestQueryStringParam(context, "src");
            var imgtype = Utils.RequestQueryStringParam(context, "imgtype");

            if (h == "") h = "0";
            if (w == "") w = "0";

            if (Utils.IsNumeric(w) && Utils.IsNumeric(h))
            {
                src = HttpContext.Current.Server.MapPath(src);

                var strCacheKey = context.Request.Url.Host.ToLower() + "*" + src + "*" + Utils.GetCurrentCulture() + "*img:" + w + "*" + h + "*";
                var newImage = (Bitmap)Utils.GetCache(strCacheKey);

                if (newImage == null)
                {
                    newImage = ImgUtils.CreateThumbnail(src, Convert.ToInt32(w), Convert.ToInt32(h));
                    Utils.SetCache(strCacheKey, newImage);
                }

                if ((newImage != null))
                {
                    context.Response.Clear();

                    ImageCodecInfo useEncoder;

                    // due to issues on some servers not outputing the png format correctly from the thumbnailer.
                    // this thumbnailer will always output jpg, unless specifically told to do a png format.
                    if (imgtype.ToLower() == "png")
                    {
                        useEncoder = ImgUtils.GetEncoder(ImageFormat.Png);
                        context.Response.ContentType = "image/png";
                    }
                    else
                    {
                        useEncoder = ImgUtils.GetEncoder(ImageFormat.Jpeg);
                        context.Response.ContentType = "image/jpeg";
                    }

                    var encoderParameters = new EncoderParameters(1);
                    encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, 85L);

                    try
                    {
                        newImage.Save(context.Response.OutputStream, useEncoder, encoderParameters);
                    }
                    catch (Exception exc)
                    {
                        var outArray = Utils.StrToByteArray(exc.ToString());
                        context.Response.BinaryWrite(outArray);
                    }
                }
            }
        }


        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}