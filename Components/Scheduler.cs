using System;
using System.IO;
using System.Runtime.Remoting;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Components.Interfaces;

namespace Nevoweb.DNN.NBrightBuy.Components
{
    public class NBrightBuyScheduler : DotNetNuke.Services.Scheduling.SchedulerClient
    {
        private string providername;
        private string portalname;

        public NBrightBuyScheduler(DotNetNuke.Services.Scheduling.ScheduleHistoryItem objScheduleHistoryItem) : base()
        {
            this.ScheduleHistoryItem = objScheduleHistoryItem;
        }



        public override void DoWork()
        {
            try
            {
                providername = "";
                portalname = "";

                var portallist = DnnUtils.GetAllPortals();

                foreach (var portal in portallist)
                {
                    portalname = portal.PortalName;

                    var storeSettings = new StoreSettings(portal.PortalID);
                    if (Directory.Exists(storeSettings.FolderTempMapPath))
                    {
                        // clear old carts
                        var objCtrl = new NBrightBuyController();
                        var objQual = DotNetNuke.Data.DataProvider.Instance().ObjectQualifier;
                        var dbOwner = DotNetNuke.Data.DataProvider.Instance().DatabaseOwner;
                        var days = 60;
                        var d = DateTime.Now.AddDays(days * -1);
                        var strDate = d.ToString("s");
                        var stmt = "";
                        stmt = "delete from " + dbOwner + "[" + objQual + "NBrightBuy] where PortalId = " + portal.PortalID.ToString("") + " and typecode = 'CART' and ModifiedDate < '" + strDate + "' ";
                        objCtrl.ExecSql(stmt);


                        // clear down NBStore temp folder
                        string[] files = Directory.GetFiles(storeSettings.FolderTempMapPath);

                        foreach (string file in files)
                        {
                            FileInfo fi = new FileInfo(file);
                            if (fi.LastAccessTime < DateTime.Now.AddHours(-1)) fi.Delete();
                        }

                        // DO Scheduler Jobs
                        var pluginData = new PluginData(portal.PortalID);
                        var l = pluginData.GetSchedulerProviders();

                        foreach (var p in l)
                        {
                            var prov = p.Value;
                            providername = prov.GetXmlProperty("genxml/textbox/assembly") + " " + prov.GetXmlProperty("genxml/textbox/namespaceclass");
                            ObjectHandle handle = null;
                            handle = Activator.CreateInstance(prov.GetXmlProperty("genxml/textbox/assembly"), prov.GetXmlProperty("genxml/textbox/namespaceclass"));
                            if (handle != null)
                            {
                                var objProvider = (SchedulerInterface)handle.Unwrap();
                                var strMsg = objProvider.DoWork(portal.PortalID);
                                if (strMsg != "")
                                {
                                    this.ScheduleHistoryItem.AddLogNote(strMsg);
                                }
                            }

                        }
                    }
                }

                this.ScheduleHistoryItem.Succeeded = true;

            }
            catch (Exception Ex)
            {
                //--intimate the schedule mechanism to write log note in schedule history
                this.ScheduleHistoryItem.Succeeded = false;
                this.ScheduleHistoryItem.AddLogNote(" Service Start. Failed. providername: " + providername + " Portal:" + portalname + " Error:" + Ex.ToString());
                this.Errored(ref Ex);
            }
        }


    }


}
