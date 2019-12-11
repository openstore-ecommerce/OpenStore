Files
=====

menu.config 
-----------
 Main default system menu (will be used for new systems and should include all system menu options)


/Themes/config/default/menuplugin.xml (LEGACY)
--------------------------------------
 Sytsem level menu which will be used for each portal, unless a portal has it's own version of the menu at portal level.
 (A manual update may be required if the portal level menu requires any new options.)


DB records
----------
In version 4 then plugin menu has been converted to store data about plugins in the DB.  The typecode for these records is "PLUGIN" in the NBrightBuy table.

Plugin Update
-------------
Any XML file in the NBrightBuy\plugins folder will be merged in the system level (portalid = -1) menu.  It will them be copied into the portal level menu.
   

Remove system level
-------------------
To remove system level pugins you can add an xml to the plugins folder with a delete flag and ctrl ref.

<genxml>
	<delete>True</delete>
	<textbox>
		<ctrl>nameofctrl</ctrl>
	</textbox>
</genxml>


