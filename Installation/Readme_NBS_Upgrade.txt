Upgrading NBrightStore (NBStore, NBS) to Open Store
---------------------------------------------------

ONLY UPGRADE IF YOU NEED TOO.

To those that don't know, Open-Store is a upgrade to an e-commerce called NBStore.

If you run NBS and want to upgrade to OpenStore, this is possible but not as simple as installing the new module.
Backward compatibly is broken in some areas.  These areas need special attention on upgrade.

MAKE A BACKUP OF THE LIVE SYSTEM AND RESTORE AS A TEST SYSTEM.  
BEFORE YOU TRY THIS ON A LIVE SYSTEM, TRY IT ON A TEST SYSTEM FIRST.

BEFORE UPGRADE ENSURE YOU HAVE A WORKING PAYMENT GATEWAY FOR OPEN STORE (NBS gateway plugins are NOT compatible with OS gateways plugins.)

Open-Store needs a minimum DNN version of DNN8.  Upgrade DNN if you are on a lower version of DNN.


Method 1 (RECOMENDED)
--------

The easiest method to upgrade is to create a new store and migrate the data.  
Export from NBS and import into Open Store, the import and export of data is compatible.


Method 2
--------

If method 1 is impossible, install the latest version of Open Source.  

Because the module names and templates have changed, you will get some issues with modules.
You should add the new module "OS*" with settings and delete the old modules from the page (NBS*).

In BO>Admin>Settings change the default template folder to "Default", for both display and emails.

(Optional) After replacing all old NBS modules you can remove the old NBS* module definitions from DNN, do NOT remove the files.
IMPORTANT: Remove the "Uninstall.SqlDataProvider" from the "\DesktopModules\NBright\NBrightBuy" folder. Before you remove the definition.
IF YOU DO NOT REOMOVE "Uninstall.SqlDataProvider" THEN IT WILL DELETE THE OPEN STORE DATABASE TABLES.

Check the BO>Admin>Plugins have the correct data, the plugin method has altered, it's been designed to upgrade, but you should check.

