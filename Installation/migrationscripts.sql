/************************************************************/
/*****              SqlDataProvider                     *****/
/*****    Provide Data Migration for NBS                *****/
/*****                                                  *****/
/***** Note: To manually execute this script you must   *****/
/*****       perform a search and replace operation     *****/
/*****       for {databaseOwner} and {objectQualifier}  *****/
/*****                                                  *****/
/*****                                                  *****/
/*****                                                  *****/
/************************************************************/


/****** Object:  StoredProcedure {databaseOwner}[{objectQualifier}NBrightBuy_MigrateCategoryData]    Script Date: 21/11/2014 16:35:24 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE {databaseOwner}[{objectQualifier}NBrightBuy_MigrateCategoryData] 
AS
BEGIN
	SET NOCOUNT ON;

-------------------------------------------------------------------------------------
------------------------- base category data --------------------------------------------
-------------------------------------------------------------------------------------
declare @dml nvarchar(max)
declare @xml XML
DECLARE @CategoryID int
DECLARE @PortalID int
DECLARE @Archived bit
DECLARE @ParentCategoryID int
DECLARE @ListOrder  int
DECLARE @ProductTemplate nvarchar(50)
DECLARE @ListItemTemplate nvarchar(50)
DECLARE @ListAltItemTemplate nvarchar(50)
DECLARE @hide bit
DECLARE @ImageURL nvarchar(256)
DECLARE @CategoryRef nvarchar(20)
DECLARE @GroupTypeRef nvarchar(20)

DECLARE @NewCategoryID int

DECLARE @Lang nvarchar(max)
DECLARE @CategoryName nvarchar(max)
DECLARE @CategoryDesc nvarchar(max)
DECLARE @Message nvarchar(max)
DECLARE @SEOName nvarchar(max)
DECLARE @MetaDescription nvarchar(max)
DECLARE @MetaKeywords nvarchar(max)
DECLARE @SEOPageTitle nvarchar(max)


DECLARE ins_cursor CURSOR FOR select [CategoryID],	[PortalID],	[Archived],	[ParentCategoryID],	[ListOrder],	[ProductTemplate],	[ListItemTemplate],	[ListAltItemTemplate],	[hide],	[ImageURL],	[CategoryRef],[GroupTypeRef] FROM {databaseOwner}{objectQualifier}[NB_Store_Categories]
OPEN ins_cursor
FETCH NEXT FROM ins_cursor INTO @CategoryID, @PortalID,  @Archived,  @ParentCategoryID, @ListOrder ,@ProductTemplate,  @ListItemTemplate,  @ListAltItemTemplate,  @hide,  @ImageURL, @CategoryRef, @GroupTypeRef
WHILE @@FETCH_STATUS = 0
BEGIN

SET @xml = '
<genxml>
  <files />
  <hidden>
    <recordsortorder><![CDATA[' + Convert(nvarchar(max),isnull(@ListOrder,'')) + ']]></recordsortorder>
    <imageurl><![CDATA[' + Convert(nvarchar(max),isnull(@ImageURL,'')) + ']]></imageurl>
  </hidden>
  <textbox>
	<txtcategoryref><![CDATA[' + Convert(nvarchar(max),isnull(@CategoryRef,'')) + ']]></txtcategoryref>
  </textbox>
  <checkbox>
    <chkarchived><![CDATA[' + replace(replace(Convert(nvarchar(max),isnull(@Archived,'0')),'1','True'),'0','False') + ']]></chkarchived>
    <chkishidden><![CDATA[' + replace(replace(Convert(nvarchar(max),isnull(@hide,'0')),'1','True'),'0','False') + ']]></chkishidden>
  </checkbox>
  <dropdownlist>
    <ddlgrouptype><![CDATA[' + Convert(nvarchar(max),isnull(@GroupTypeRef,''))  + ']]></ddlgrouptype>
    <ddlparentcatid></ddlparentcatid>
    <producttemplate><![CDATA[' + Convert(nvarchar(max),isnull(@ProductTemplate,''))  + ']]></producttemplate>
    <listitemtemplate><![CDATA[' + Convert(nvarchar(max),isnull(@ListItemTemplate,''))  + ']]></listitemtemplate>
    <listaltitemtemplate><![CDATA[' + Convert(nvarchar(max),isnull(@ListAltItemTemplate,'')) + ']]></listaltitemtemplate>
  </dropdownlist>  
  <checkboxlist />
  <radiobuttonlist />
  <edt />
</genxml>
'


	--******* do insert of CATEGORY record ********--
	-- NOTE: Use legacy @ParentCategoryID for the parentid, so we can replace it after the full insert has been done.
	INSERT INTO {databaseOwner}{objectQualifier}NBrightBuy ([PortalId],[ModuleId],[TypeCode],[XMLData],[GUIDKey],[ModifiedDate],[TextData],[XrefItemId],[ParentItemId],[Lang],[UserId],[LegacyItemId])
		VALUES(@PortalID ,-1,'CATEGORY',@xml,'',CURRENT_TIMESTAMP,'',0,@ParentCategoryID,'',0,@CategoryID)



-------------------------------------------------------------------------------------
------------------------- Do categorylang data --------------------------------------
-------------------------------------------------------------------------------------
SET @NewCategoryID = SCOPE_IDENTITY()

DECLARE insl_cursor CURSOR FOR select [Lang],[CategoryName],[CategoryDesc],[Message],[SEOName],[MetaDescription],[MetaKeywords],[SEOPageTitle] FROM {databaseOwner}{objectQualifier}[NB_Store_CategoryLang] where [CategoryID] = @CategoryID 
OPEN insl_cursor
FETCH NEXT FROM insl_cursor INTO @Lang, @CategoryName, @CategoryDesc, @Message, @SEOName, @MetaDescription, @MetaKeywords, @SEOPageTitle 
WHILE @@FETCH_STATUS = 0
BEGIN

SET @xml = '
<genxml>
  <files />
  <hidden />
  <textbox>
	<txtcategoryname><![CDATA[' + Convert(nvarchar(max),isnull(@CategoryName,'')) + ']]></txtcategoryname>
	<txtcategorydesc><![CDATA[' + Convert(nvarchar(max),isnull(@CategoryDesc,'')) + ']]></txtcategorydesc>
	<txtseoname><![CDATA[' + Convert(nvarchar(max),isnull(@SEOName,'')) + ']]></txtseoname>
	<txtmetadescription><![CDATA[' + Convert(nvarchar(max),isnull(@MetaDescription,'')) + ']]></txtmetadescription>
	<txtmetakeywords><![CDATA[' + Convert(nvarchar(max),isnull(@MetaKeywords,'')) + ']]></txtmetakeywords>
	<txtseopagetitle><![CDATA[' + Convert(nvarchar(max),isnull(@SEOPageTitle,'')) + ']]></txtseopagetitle>
  </textbox>  
  <checkbox />
  <dropdownlist />
  <checkboxlist />
  <radiobuttonlist />
  <edt>
	<message><![CDATA[' + isnull(@Message,'') + ']]></message>
  </edt>
</genxml>
'

		INSERT INTO {databaseOwner}{objectQualifier}NBrightBuy ([PortalId],[ModuleId],[TypeCode],[XMLData],[GUIDKey],[ModifiedDate],[TextData],[XrefItemId],[ParentItemId],[Lang],[UserId],[LegacyItemId])
			 VALUES (@PortalID,-1,'CATEGORYLANG',@xml,'',CURRENT_TIMESTAMP,'',0,@NewCategoryID,@Lang,0,@CategoryID)



FETCH NEXT FROM insl_cursor INTO @Lang, @CategoryName, @CategoryDesc, @Message, @SEOName, @MetaDescription, @MetaKeywords, @SEOPageTitle 
END 
CLOSE insl_cursor;
DEALLOCATE insl_cursor;




FETCH NEXT FROM ins_cursor INTO @CategoryID, @PortalID,  @Archived,  @ParentCategoryID, @ListOrder ,@ProductTemplate,  @ListItemTemplate,  @ListAltItemTemplate,  @hide,  @ImageURL, @CategoryRef, @GroupTypeRef
END 
CLOSE ins_cursor;
DEALLOCATE ins_cursor;

-------------------------------------------------------------------------------------
------------------------- Update parentid      --------------------------------------
-------------------------------------------------------------------------------------
-- START iteration to get all parent category ids
declare @TableVar table (CategoryID int, ParentID int, lvl int NOT NULL)
DECLARE @ParentID INT;
DECLARE @lvl INT;
DECLARE @catid int	


DECLARE @ItemId int
DECLARE @LegacyItemId int
DECLARE @ParentItemId int

--- use parentitemid for legacytemid, previous inserted to create the link
DECLARE ins_cursor CURSOR FOR select [ItemId],[ParentItemId] from {databaseOwner}{objectQualifier}[NBrightBuy] where isnull([ParentItemId],0) > 0 and TypeCode = 'CATEGORY'
OPEN ins_cursor
FETCH NEXT FROM ins_cursor INTO @ItemId,@LegacyItemId
WHILE @@FETCH_STATUS = 0
BEGIN

	Update NBrightBuy 
	Set ParentItemId = (select top 1 Itemid from NBrightBuy where LegacyItemId = @LegacyItemId and TypeCode = 'CATEGORY')
	where ItemId = @ItemId

FETCH NEXT FROM ins_cursor INTO @ItemId,@LegacyItemId
END 
CLOSE ins_cursor;
DEALLOCATE ins_cursor;


DECLARE ins_cursor CURSOR FOR select [ItemId],[XMLData],[ParentItemId] from {databaseOwner}{objectQualifier}[NBrightBuy] where isnull([ParentItemId],0) > 0 and TypeCode = 'CATEGORY'
OPEN ins_cursor
FETCH NEXT FROM ins_cursor INTO @ItemId,@xml,@ParentItemId
WHILE @@FETCH_STATUS = 0
BEGIN

	set @xml.modify('replace value of (genxml/dropdownlist/ddlparentcatid/text())[1] with sql:variable("@ParentItemId") ')
	set @xml.modify('insert text{sql:variable("@ParentItemId")} into (genxml/dropdownlist/ddlparentcatid[not(text())])[1]')

	UPDATE {databaseOwner}{objectQualifier}NBrightBuy
	SET [XMLData] = @xml
	,[ModifiedDate] = CURRENT_TIMESTAMP
	where ItemId = @ItemId

FETCH NEXT FROM ins_cursor INTO @ItemId,@xml,@ParentItemId
END 
CLOSE ins_cursor;
DEALLOCATE ins_cursor;


-------------------------------------------------------------------------------------
------------------------- create XREF data   --------------------------------------
-------------------------------------------------------------------------------------

DECLARE @NewProductID int
DECLARE @ProductID int

DECLARE insp_cursor CURSOR FOR select [itemId],[LegacyItemId]  FROM {databaseOwner}{objectQualifier}[NBrightBuy] where typecode = 'PRD'
OPEN insp_cursor
FETCH NEXT FROM insp_cursor INTO @NewProductID,@ProductID
WHILE @@FETCH_STATUS = 0
BEGIN

begin transaction

DECLARE ins_cursor CURSOR FOR select [ProductID],[CategoryID]  FROM {databaseOwner}{objectQualifier}[NB_Store_ProductCategory] where [ProductID] = @ProductID
OPEN ins_cursor
FETCH NEXT FROM ins_cursor INTO @ProductID,@CategoryID
WHILE @@FETCH_STATUS = 0
BEGIN

		SET @NewCategoryID = (select top 1 itemid from NBrightBuy where LegacyItemId = @CategoryID and typecode = 'CATEGORY')

		INSERT INTO {databaseOwner}{objectQualifier}NBrightBuy ([PortalId],[ModuleId],[TypeCode],[XMLData],[GUIDKey],[ModifiedDate],[TextData],[XrefItemId],[ParentItemId],[Lang],[UserId],[LegacyItemId])
			 VALUES (@PortalID,-1,'CATXREF','',NULL,CURRENT_TIMESTAMP,'',@NewCategoryID,@NewProductID,'',0,NULL)

FETCH NEXT FROM ins_cursor INTO @ProductID,@CategoryID
END 
CLOSE ins_cursor;
DEALLOCATE ins_cursor;



	------------------------------------------------------
	--- Get category list of all possible product cats ---
	------------------------------------------------------

	delete from @TableVar

	DECLARE cat_cursor CURSOR FOR select ParentItemId as ProductId, xrefitemid as CategoryId from {databaseOwner}{objectQualifier}[NBrightBuy] where typecode = 'CATXREF' and ParentItemId = @NewProductID
	OPEN cat_cursor
	FETCH NEXT FROM cat_cursor  INTO @productid,@categoryid
	WHILE @@FETCH_STATUS = 0
	BEGIN

		SET @lvl = 0; 	
		INSERT INTO @TableVar(CategoryID,ParentID, lvl)
			SELECT ItemId, ParentItemId  ,@lvl FROM {databaseOwner}{objectQualifier}[NBrightBuy] WHERE ItemId = @categoryid;

		WHILE @@rowcount > 0 and @lvl <=20        
		BEGIN
			SET @lvl = @lvl + 1;   
			IF NOT EXISTS(
				SELECT C.ItemId FROM @TableVar as TV 
				INNER JOIN {databaseOwner}{objectQualifier}[NBrightBuy]  as C ON TV.lvl = (@lvl - 1)  AND C.ItemID = TV.ParentID
				WHERE C.ItemId = CategoryID and C.ParentItemId = ParentID
			)
			BEGIN
				INSERT INTO @TableVar(CategoryID,ParentID, lvl)
				SELECT C.ItemId, C.ParentItemId  ,@lvl FROM @TableVar as TV          
				INNER JOIN {databaseOwner}{objectQualifier}[NBrightBuy]  as C ON TV.lvl = (@lvl - 1)  AND C.ItemID = TV.ParentID;
			END
		END
		FETCH NEXT FROM cat_cursor INTO @productid,@categoryid
	END
	CLOSE cat_cursor
	DEALLOCATE cat_cursor

---------------------------------------------------------
--- Create all CASCADE records for category list      ---
---------------------------------------------------------
	DECLARE cascadelist1 CURSOR FOR select distinct CategoryId from @TableVar
	OPEN cascadelist1
	FETCH NEXT FROM cascadelist1 INTO @catid
	WHILE @@FETCH_STATUS = 0
	BEGIN
			-- Let make sure it's not there, duplicates can exist
			if NOT EXISTS(select itemid from {databaseOwner}{objectQualifier}NBrightBuy where (TYPECODE = 'CATCASCADE' or TYPECODE = 'CATXREF') and [ParentItemId] = @NewProductID and [XrefItemId] = @catid)
			BEGIN
				INSERT INTO {databaseOwner}{objectQualifier}NBrightBuy
					([PortalId]
					,[ModuleId]
					,[TypeCode]
					,[XMLData]
					,[GUIDKey]
					,[ModifiedDate]
					,[TextData]
					,[XrefItemId]
					,[ParentItemId]
					,[Lang]
					,[UserId]
					,[LegacyItemId])
				VALUES
					(@portalid
					,-1
					,'CATCASCADE'
					,''
					,NULL 
					,CURRENT_TIMESTAMP
					,''
					,@catid
					,@NewProductID
					,''
					,0
					,NULL
					)
			END

	FETCH NEXT FROM cascadelist1 INTO @catid
	END
	CLOSE cascadelist1
	DEALLOCATE cascadelist1  

	commit

FETCH NEXT FROM insp_cursor INTO @NewProductID,@ProductID
END 
CLOSE insp_cursor;
DEALLOCATE insp_cursor;




END
GO



/****** Object:  StoredProcedure {databaseOwner}[{objectQualifier}NBrightBuy_MigrateProductData]    Script Date: 21/11/2014 16:35:30 ******/

CREATE PROCEDURE {databaseOwner}[{objectQualifier}NBrightBuy_MigrateProductData] 
AS
BEGIN

	SET NOCOUNT ON;


-- select all products
declare @dml nvarchar(max)
declare @xml XML
DECLARE @ProductID int
DECLARE @PortalID int
DECLARE @TaxCategoryID int
DECLARE @Featured bit
DECLARE @Archived bit
DECLARE @IsDeleted bit
DECLARE @ProductRef nvarchar(20)
DECLARE @IsHidden bit

	DECLARE ins_cursor CURSOR FOR select i.[ProductID],i.[PortalID],i.[TaxCategoryID],i.[Featured],i.[Archived],i.[IsDeleted],i.[ProductRef],i.[IsHidden] FROM {databaseOwner}{objectQualifier}[NB_Store_Products] as i
	OPEN ins_cursor
	FETCH NEXT FROM ins_cursor INTO @ProductID,@PortalID,@TaxCategoryID,@Featured,@Archived,@IsDeleted,@ProductRef,@IsHidden
	WHILE @@FETCH_STATUS = 0
	BEGIN

--create base product xml

SET @xml = '<genxml>
  <files />
  <hidden />
  <textbox>
    <taxcategoryid><![CDATA[' + convert(nvarchar(max),@TaxCategoryID) + ']]></taxcategoryid>
    <txtproductref><![CDATA[' + @ProductRef + ']]></txtproductref>
  </textbox>  
  <checkbox>
    <chkfeatured><![CDATA[' + replace(replace(Convert(nvarchar(5),isnull(@Featured,'0')),'1','True'),'0','False') + ']]></chkfeatured>
    <chkarchived><![CDATA[' + replace(replace(Convert(nvarchar(5),isnull(@Archived,'0')),'1','True'),'0','False') + ']]></chkarchived>
    <chkisdeleted><![CDATA[' + replace(replace(Convert(nvarchar(5),isnull(@IsDeleted,'0')),'1','True'),'0','False') + ']]></chkisdeleted>
    <chkishidden><![CDATA[' + replace(replace(Convert(nvarchar(5),isnull(@IsHidden,'0')),'1','True'),'0','False') + ']]></chkishidden>
  </checkbox>
  <dropdownlist />
  <checkboxlist />
  <radiobuttonlist />
  <edt />
</genxml>
'
-------------------------------------------------------------------------------------
------------------------- add model data --------------------------------------------
-------------------------------------------------------------------------------------
DECLARE @ModelID int,@ListOrder int,@UnitCost money,@Barcode nvarchar(20),@ModelRef nvarchar(20),@QtyRemaining int,@QtyTrans int,@QtyTransDate datetime,@Deleted bit,@QtyStockSet int,@DealerCost money,@PurchaseCost money,@DealerOnly bit,@Allow int
declare @modelxml XML
declare @modeldata nvarchar(max)
SET @modeldata = ''

DECLARE model_cursor CURSOR FOR 
SELECT  [ModelID],[ListOrder],[UnitCost],[Barcode],[ModelRef],[QtyRemaining],[QtyTrans],[QtyTransDate],[Deleted],[QtyStockSet],[DealerCost],[PurchaseCost],[DealerOnly],[Allow]
	FROM {databaseOwner}{objectQualifier}[NB_Store_Model] as M WHERE M.ProductID = @ProductId ORDER BY ListOrder;

OPEN model_cursor

FETCH NEXT FROM model_cursor
INTO @ModelID,@ListOrder,@UnitCost,@Barcode,@ModelRef,@QtyRemaining,@QtyTrans,@QtyTransDate,@Deleted,@QtyStockSet,@DealerCost,@PurchaseCost,@DealerOnly,@Allow

WHILE @@FETCH_STATUS = 0
BEGIN

SET @modeldata = @modeldata + '
<genxml>
	<files />
	<hidden>
	<modelid>' + convert(nvarchar(255),isnull(@ModelID,'')) + '</modelid>
	<allow>' + convert(nvarchar(255),isnull(@Allow,'')) + '</allow>
	<qtytrans>' + convert(nvarchar(255),isnull(@QtyTrans,'')) + '</qtytrans>
	<qtytransdate>' + convert(nvarchar(255),isnull(@QtyTransDate,''),126) + '</qtytransdate>
	</hidden>
	<textbox>
	<txtlistorder>' + convert(nvarchar(255),isnull(@ListOrder,'')) + '</txtlistorder>
	<txtunitcost>' + convert(nvarchar(255),isnull(@UnitCost,'')) + '</txtunitcost>
	<txtbarcode>' + convert(nvarchar(255),isnull(@Barcode,'')) + '</txtbarcode>
	<txtmodelref>' + convert(nvarchar(255),isnull(@ModelRef,'')) + '</txtmodelref>
	<txtqtyremaining>' + convert(nvarchar(255),isnull(@QtyRemaining,'')) + '</txtqtyremaining>
	<txtqtystockset>' + convert(nvarchar(255),isnull(@QtyStockSet,'')) + '</txtqtystockset>
	<txtdealercost>' + convert(nvarchar(255),isnull(@DealerCost,'')) + '</txtdealercost>
	<txtpurchasecost>' + convert(nvarchar(255),isnull(@PurchaseCost,'')) + '</txtpurchasecost>
	</textbox>  
	<checkbox>
	<chkdeleted>' + replace(replace(Convert(nvarchar(5),isnull(@Deleted,'0')),'1','True'),'0','False') +'</chkdeleted>
	<chkdealeronly>' + replace(replace(Convert(nvarchar(5),isnull(@DealerOnly,'0')),'1','True'),'0','False') +'</chkdealeronly>
	</checkbox>
	<dropdownlist />
	<checkboxlist />
	<radiobuttonlist />
	<edt/>
</genxml>
'

FETCH NEXT FROM model_cursor 
INTO @ModelID,@ListOrder,@UnitCost,@Barcode,@ModelRef,@QtyRemaining,@QtyTrans,@QtyTransDate,@Deleted,@QtyStockSet,@DealerCost,@PurchaseCost,@DealerOnly,@Allow
END 

CLOSE model_cursor;
DEALLOCATE model_cursor;


SET @modelxml = '<models>' + @modeldata + '</models>'

SET @xml.modify('insert sql:variable("@modelxml") as last into /genxml[1] ')

-------------------------------------------------------------------------------------
------------------------- add option data --------------------------------------------
-------------------------------------------------------------------------------------

declare @itemxml XML
declare @itemdata nvarchar(max)

-- Item Column vars -----------------
DECLARE @OptionID int, @oListOrder nvarchar(max)
--------------------------------------

SET @itemdata = ''

DECLARE item_cursor CURSOR FOR 
SELECT  [OptionID],[ListOrder]
	FROM {databaseOwner}{objectQualifier}[NB_Store_Option] as T WHERE T.ProductID = @ProductId ORDER BY ListOrder;

OPEN item_cursor

FETCH NEXT FROM item_cursor
INTO @OptionID,@oListOrder

WHILE @@FETCH_STATUS = 0
BEGIN

SET @itemdata = @itemdata + '
<genxml>
	<files />
	<hidden>
	<optionid>' + convert(nvarchar(255),isnull(@OptionID,'')) + '</optionid>
	</hidden>
	<textbox>
	<txtlistorder>' + isnull(@oListOrder,'') + '</txtlistorder>
	</textbox>  
	<checkbox/>
	<dropdownlist />
	<checkboxlist />
	<radiobuttonlist />
	<edt/>
</genxml>
'

-------------------------------------------------------------------------------------
------------------------- add option value data --------------------------------------------
-------------------------------------------------------------------------------------

-- Item Column vars -----------------
DECLARE @OptionValueID int,@AddedCost nvarchar(max),@ovListOrder nvarchar(max)
--------------------------------------
DECLARE @itemdataOV nvarchar(max)

SET @itemdataOV = ''

DECLARE item_cursorOV CURSOR FOR 
SELECT  [OptionValueID],[AddedCost],TV.[ListOrder]
	FROM {databaseOwner}{objectQualifier}[NB_Store_OptionValue] as TV
	inner join {databaseOwner}{objectQualifier}[NB_Store_Option] as T on TV.OptionID = T.OptionID 
	WHERE T.ProductID = @ProductId and TV.OptionID = @OptionId 
	ORDER BY TV.ListOrder;

OPEN item_cursorOV

FETCH NEXT FROM item_cursorOV
INTO @OptionValueID,@AddedCost,@ovListOrder

WHILE @@FETCH_STATUS = 0
BEGIN

SET @itemdataOV = @itemdataOV + '
<genxml>
	<files />
	<hidden>
	<optionvalueid>' + convert(nvarchar(255),isnull(@OptionValueID,'')) + '</optionvalueid>
	</hidden>
	<textbox>
	<txtlistorder>' + isnull(@ovListOrder,'') + '</txtlistorder>
	<txtaddedcost>' + isnull(@AddedCost,'') + '</txtaddedcost>
	</textbox>  
	<checkbox/>
	<dropdownlist />
	<checkboxlist />
	<radiobuttonlist />
	<edt/>
</genxml>
'

FETCH NEXT FROM item_cursorOV
INTO @OptionValueID,@AddedCost,@ovListOrder
END 

CLOSE item_cursorOV;
DEALLOCATE item_cursorOV;


SET @itemxml = '<optionvalues optionid=''' + convert(nvarchar(max),@OptionId) + '''>' + @itemdataOV + '</optionvalues>'

SET @xml.modify('insert sql:variable("@itemxml") as last into /genxml[1] ')

-------------------------------------------------------------------------------------
------------------------- END option value data --------------------------------------------
-------------------------------------------------------------------------------------


FETCH NEXT FROM item_cursor 
INTO @OptionID,@oListOrder
END 

CLOSE item_cursor;
DEALLOCATE item_cursor;


SET @itemxml = '<options>' + @itemdata + '</options>'

SET @xml.modify('insert sql:variable("@itemxml") as last into /genxml[1] ')


-------------------------------------------------------------------------------------
-------------------------productdoc data --------------------------------------------
-------------------------------------------------------------------------------------


-- Item Column vars -----------------
DECLARE @DocID int,@DocPath nvarchar(max),@dListOrder nvarchar(max),@Hidden nvarchar(max),@FileName nvarchar(max),@FileExt nvarchar(max),@Purchase nvarchar(max)
--------------------------------------

SET @itemdata = ''

DECLARE item_cursor CURSOR FOR 
SELECT  [DocID],[DocPath],[ListOrder],[Hidden],[FileName],[FileExt],[Purchase]
	FROM {databaseOwner}{objectQualifier}[NB_Store_ProductDoc] as T WHERE T.ProductID = @ProductId ORDER BY ListOrder;

OPEN item_cursor

FETCH NEXT FROM item_cursor
INTO @DocID,@DocPath,@ListOrder,@Hidden,@FileName,@FileExt,@Purchase

WHILE @@FETCH_STATUS = 0
BEGIN

SET @itemdata = @itemdata + '
<genxml>
	<files />
	<hidden>
	<docid>' + convert(nvarchar(255),isnull(@DocID,'')) + '</docid>
	<docpath>' + convert(nvarchar(255),isnull(@DocPath,'')) + '</docpath>
	<fileext>' + convert(nvarchar(255),isnull(@FileExt,'')) + '</fileext>
	</hidden>
	<textbox>
	<txtlistorder>' + isnull(@dListOrder,'') + '</txtlistorder>
	<txtfilename>' + convert(nvarchar(255),isnull(@FileName,'')) + '</txtfilename>
	</textbox>  
	<checkbox>
	<chkhidden>' + replace(replace(Convert(nvarchar(5),isnull(@Hidden,'0')),'1','True'),'0','False') +'</chkhidden>
	<chkpurchase>' + replace(replace(Convert(nvarchar(5),isnull(@Purchase,'0')),'1','True'),'0','False') +'</chkpurchase>
	</checkbox>
	<dropdownlist />
	<checkboxlist />
	<radiobuttonlist />
	<edt/>
</genxml>
'

FETCH NEXT FROM item_cursor 
INTO @DocID,@DocPath,@ListOrder,@Hidden,@FileName,@FileExt,@Purchase
END 

CLOSE item_cursor;
DEALLOCATE item_cursor;


SET @itemxml = '<docs>' + @itemdata + '</docs>'

SET @xml.modify('insert sql:variable("@itemxml") as last into /genxml[1] ')


-------------------------------------------------------------------------------------
-------------------------product image data --------------------------------------------
-------------------------------------------------------------------------------------

-- Item Column vars -----------------
DECLARE @ImageID int,@ImagePath nvarchar(max),@iListOrder nvarchar(max),@iHidden nvarchar(max),@ImageURL nvarchar(max)
--------------------------------------

SET @itemdata = ''

DECLARE item_cursor CURSOR FOR 
SELECT  [ImageID],[ImagePath],[ListOrder],[Hidden],[ImageURL]
	FROM {databaseOwner}{objectQualifier}[NB_Store_ProductImage] as T WHERE T.ProductID = @ProductId ORDER BY ListOrder;

OPEN item_cursor

FETCH NEXT FROM item_cursor
INTO @ImageID,@ImagePath,@iListOrder,@iHidden,@ImageURL

WHILE @@FETCH_STATUS = 0
BEGIN

SET @itemdata = @itemdata + '
<genxml>
	<files />
	<hidden>
	<imageid>' + convert(nvarchar(255),isnull(@ImageID,'')) + '</imageid>
	<imagepath>' + convert(nvarchar(255),isnull(@ImagePath,'')) + '</imagepath>
	<imageurl>' + convert(nvarchar(255),isnull(@ImageURL,'')) + '</imageurl>
	</hidden>
	<textbox>
	<txtlistorder>' + isnull(@iListOrder,'') + '</txtlistorder>
	</textbox>  
	<checkbox>
	<chkhidden>' + replace(replace(Convert(nvarchar(5),isnull(@iHidden,'0')),'1','True'),'0','False') +'</chkhidden>
	</checkbox>
	<dropdownlist />
	<checkboxlist />
	<radiobuttonlist />
	<edt/>
</genxml>
'

FETCH NEXT FROM item_cursor 
INTO @ImageID,@ImagePath,@iListOrder,@iHidden,@ImageURL
END 

CLOSE item_cursor;
DEALLOCATE item_cursor;

SET @itemxml = '<imgs>' + @itemdata + '</imgs>'

SET @xml.modify('insert sql:variable("@itemxml") as last into /genxml[1] ')



-- do insert of record
INSERT INTO {databaseOwner}{objectQualifier}NBrightBuy ([PortalId],[ModuleId],[TypeCode],[XMLData],[GUIDKey],[ModifiedDate],[TextData],[XrefItemId],[ParentItemId],[Lang],[UserId],[LegacyItemId])
		VALUES(@PortalID ,-1,'PRD',@xml,'',CURRENT_TIMESTAMP,'',0,0,'',0,@ProductID)


FETCH NEXT FROM ins_cursor INTO @ProductID,@PortalID,@TaxCategoryID,@Featured,@Archived,@IsDeleted,@ProductRef,@IsHidden
END 
CLOSE ins_cursor;
DEALLOCATE ins_cursor;


-------------------------------------------------------------------------------------
-------------------------  add langauge data ----------------------------------------
-------------------------------------------------------------------------------------
DECLARE @Lang char(5)
DECLARE @Summary nvarchar(1000)
DECLARE @Description nvarchar(max)
DECLARE @Manufacturer nvarchar(50)
DECLARE @ProductName nvarchar(150)
DECLARE @XMLData xml
DECLARE @SEOName nvarchar(150)
DECLARE @TagWords nvarchar(255)
DECLARE @SEOPageTitle nvarchar(150)



	DECLARE ins_cursor CURSOR FOR select i.[ProductID],i.[Lang],i.[Summary],i.[Description],i.[Manufacturer],i.[ProductName],i.[XMLData],i.[SEOName],i.[TagWords],i.[SEOPageTitle] FROM {databaseOwner}{objectQualifier}[NB_Store_ProductLang] as i
	OPEN ins_cursor
	FETCH NEXT FROM ins_cursor INTO @ProductID,@Lang,@Summary,@Description,@Manufacturer,@ProductName,@XMLData,@SEOName,@TagWords,@SEOPageTitle
	WHILE @@FETCH_STATUS = 0
	BEGIN

--create base product lang xml

SET @xml = '
<genxml>
  <files />
  <hidden />
  <textbox>
	<txtsummary><![CDATA[' + Convert(nvarchar(max),isnull(@Summary,'')) + ']]></txtsummary>
	<txtmanufacturer><![CDATA[' + Convert(nvarchar(50),isnull(@Manufacturer,'')) + ']]></txtmanufacturer>
	<txtproductname><![CDATA[' + Convert(nvarchar(150),isnull(@ProductName,'')) + ']]></txtproductname>
	<txtseoname><![CDATA[' + Convert(nvarchar(150),isnull(@SEOName,'')) + ']]></txtseoname>
	<txttagwords><![CDATA[' + Convert(nvarchar(255),isnull(@TagWords,'')) + ']]></txttagwords>
	<txtseopagetitle><![CDATA[' + Convert(nvarchar(150),isnull(@SEOPageTitle,'')) + ']]></txtseopagetitle>
  </textbox>  
  <checkbox />
  <dropdownlist />
  <checkboxlist />
  <radiobuttonlist />
  <edt>
	<description><![CDATA[' + isnull(@Description,'') + ']]></description>
  </edt>
</genxml>
'

-------------------------------------------------------------------------------------
------------------------- add CUSTOM data --------------------------------------------
-------------------------------------------------------------------------------------
declare @customxml XML
SET @customxml = '<customxml>' + convert(nvarchar(max),@XMLData) + '</customxml>'
SET @xml.modify('insert sql:variable("@customxml") as last into /genxml[1] ')

-------------------------------------------------------------------------------------
------------------------- add model data --------------------------------------------
-------------------------------------------------------------------------------------

declare @ModelName nvarchar(max),@Extra nvarchar(max)

SET @modeldata = ''

DECLARE model_cursor CURSOR FOR 
SELECT  ML.[ModelId],ML.[ModelName],ML.[Extra]
    FROM {databaseOwner}{objectQualifier}[NB_Store_ModelLang] as ML 
	inner join {databaseOwner}{objectQualifier}[NB_Store_Model] as M on ML.ModelID = M.ModelID 
	WHERE ML.Lang = @Lang and M.ProductID = @ProductId

OPEN model_cursor

FETCH NEXT FROM model_cursor INTO @ModelId,@ModelName,@Extra
WHILE @@FETCH_STATUS = 0
BEGIN

SET @modeldata = @modeldata + '
<genxml>
	<files />
	<hidden>
	<modelid>' + convert(nvarchar(255),isnull(@ModelId,'')) + '</modelid>
	</hidden>
	<textbox>
	<txtmodelname>' + convert(nvarchar(255),isnull(@ModelName,'')) + '</txtmodelname>
	<txtextra>' + convert(nvarchar(255),isnull(@Extra,'')) + '</txtextra>
	</textbox>  
	<checkbox/>
	<dropdownlist />
	<checkboxlist />
	<radiobuttonlist />
	<edt/>
</genxml>
'

FETCH NEXT FROM model_cursor INTO @ModelId,@ModelName,@Extra
END 

CLOSE model_cursor;
DEALLOCATE model_cursor;


SET @modelxml = '<models>' + @modeldata + '</models>'

SET @xml.modify('insert sql:variable("@modelxml") as last into /genxml[1] ')

-------------------------------------------------------------------------------------
------------------------- add option data --------------------------------------------
-------------------------------------------------------------------------------------

-- Item Column vars -----------------
declare @OptionDesc nvarchar(max)
-------------------------------------

SET @itemdata = ''

DECLARE item_cursor CURSOR FOR 
SELECT  TL.[OptionID],TL.[OptionDesc]
    FROM {databaseOwner}{objectQualifier}[NB_Store_OptionLang] as TL 
	inner join {databaseOwner}{objectQualifier}[NB_Store_Option] as T on TL.OptionID = T.OptionID 
	WHERE TL.Lang = @Lang and T.ProductID = @ProductId

OPEN item_cursor

FETCH NEXT FROM item_cursor INTO @OptionId,@OptionDesc
WHILE @@FETCH_STATUS = 0
BEGIN

SET @itemdata = @itemdata + '
<genxml>
	<files />
	<hidden>
	<optionid>' + convert(nvarchar(255),isnull(@OptionId,'')) + '</optionid>
	</hidden>
	<textbox>
	<txtoptiondesc>' + convert(nvarchar(255),isnull(@OptionDesc,'')) + '</txtoptiondesc>
	</textbox>  
	<checkbox/>
	<dropdownlist />
	<checkboxlist />
	<radiobuttonlist />
</genxml>
'

-------------------------------------------------------------------------------------
------------------------- add option value data --------------------------------------------
-------------------------------------------------------------------------------------

-- Item Column vars -----------------
declare @OptionValueDesc nvarchar(max)
-------------------------------------

SET @itemdataOV = ''

DECLARE item_cursorOV CURSOR FOR 
SELECT  TL.[OptionValueID],TL.[OptionValueDesc]
    FROM {databaseOwner}{objectQualifier}[NB_Store_OptionValueLang] as TL 
	inner join {databaseOwner}{objectQualifier}[NB_Store_OptionValue] as T on TL.OptionValueID = T.OptionValueID 
	inner join {databaseOwner}{objectQualifier}[NB_Store_Option] as O on O.OptionID = T.OptionID 
	WHERE TL.Lang = @Lang and O.ProductID = @ProductId and T.OptionID = @OptionId 

OPEN item_cursorOV

FETCH NEXT FROM item_cursorOV INTO @OptionValueId,@OptionValueDesc
WHILE @@FETCH_STATUS = 0
BEGIN

SET @itemdataOV = @itemdataOV + '
<genxml>
	<files />
	<hidden>
	<optionvalueid>' + convert(nvarchar(255),isnull(@OptionValueId,'')) + '</optionvalueid>
	</hidden>
	<textbox>
	<txtoptionvaluedesc>' + convert(nvarchar(255),isnull(@OptionValueDesc,'')) + '</txtoptionvaluedesc>
	</textbox>  
	<checkbox/>
	<dropdownlist />
	<checkboxlist />
	<radiobuttonlist />
</genxml>
'

FETCH NEXT FROM item_cursorOV INTO @OptionValueId,@OptionValueDesc
END 

CLOSE item_cursorOV;
DEALLOCATE item_cursorOV;


SET @itemxml = '<optionvalues optionid=''' + convert(nvarchar(max),@OptionId) + '''>' + @itemdataOV + '</optionvalues>'

SET @xml.modify('insert sql:variable("@itemxml") as last into /genxml[1] ')

-------------------------------------------------------------------------------------
------------------------- END option value data --------------------------------------------
-------------------------------------------------------------------------------------

FETCH NEXT FROM item_cursor INTO @OptionId,@OptionDesc
END 

CLOSE item_cursor;
DEALLOCATE item_cursor;


SET @itemxml = '<options>' + @itemdata + '</options>'

SET @xml.modify('insert sql:variable("@itemxml") as last into /genxml[1] ')


-------------------------------------------------------------------------------------
-------------------------productdoc data --------------------------------------------
-------------------------------------------------------------------------------------

-- Item Column vars -----------------
declare @DocDesc nvarchar(max)
-------------------------------------
SET @itemdata = ''

DECLARE item_cursor CURSOR FOR 
SELECT  TL.[DocID],TL.[DocDesc]
    FROM {databaseOwner}{objectQualifier}[NB_Store_ProductDocLang] as TL 
	inner join {databaseOwner}{objectQualifier}[NB_Store_ProductDoc] as T on TL.DocID = T.DocID 
	WHERE TL.Lang = @Lang and T.ProductID = @ProductId

OPEN item_cursor

FETCH NEXT FROM item_cursor INTO @DocId,@DocDesc
WHILE @@FETCH_STATUS = 0
BEGIN

SET @itemdata = @itemdata + '
<genxml>
	<files />
	<hidden>
	<docid>' + convert(nvarchar(255),isnull(@DocId,'')) + '</docid>
	</hidden>
	<textbox>
	<txtdocdesc>' + convert(nvarchar(255),isnull(@DocDesc,'')) + '</txtdocdesc>
	</textbox>  
	<checkbox/>
	<dropdownlist />
	<checkboxlist />
	<radiobuttonlist />
</genxml>
'

FETCH NEXT FROM item_cursor INTO @DocId,@DocDesc
END 

CLOSE item_cursor;
DEALLOCATE item_cursor;


SET @itemxml = '<docs>' + @itemdata + '</docs>'

SET @xml.modify('insert sql:variable("@itemxml") as last into /genxml[1] ')

-------------------------------------------------------------------------------------
-------------------------product image data --------------------------------------------
-------------------------------------------------------------------------------------

declare @ImageDesc nvarchar(max)
SET @itemdata = ''

DECLARE item_cursor CURSOR FOR 
SELECT  TL.[ImageID],TL.[ImageDesc]
    FROM {databaseOwner}{objectQualifier}[NB_Store_ProductImageLang] as TL 
	inner join {databaseOwner}{objectQualifier}[NB_Store_ProductImage] as T on TL.ImageID = T.ImageID 
	WHERE TL.Lang = @Lang and T.ProductID = @ProductId

OPEN item_cursor

FETCH NEXT FROM item_cursor INTO @ImageId,@ImageDesc
WHILE @@FETCH_STATUS = 0
BEGIN

SET @itemdata = @itemdata + '
<genxml>
	<files />
	<hidden>
	<imageid>' + convert(nvarchar(255),isnull(@ImageId,'')) + '</imageid>
	</hidden>
	<textbox>
	<txtimagedesc>' + convert(nvarchar(255),isnull(@ImageDesc,'')) + '</txtimagedesc>
	</textbox>  
	<checkbox/>
	<dropdownlist />
	<checkboxlist />
	<radiobuttonlist />
</genxml>
'

FETCH NEXT FROM item_cursor INTO @ImageId,@ImageDesc
END 

CLOSE item_cursor;
DEALLOCATE item_cursor;


SET @itemxml = '<imgs>' + @itemdata + '</imgs>'

SET @xml.modify('insert sql:variable("@itemxml") as last into /genxml[1] ')




-- do insert of record
INSERT INTO {databaseOwner}{objectQualifier}NBrightBuy ([PortalId],[ModuleId],[TypeCode],[XMLData],[GUIDKey],[ModifiedDate],[TextData],[XrefItemId],[ParentItemId],[Lang],[UserId],[LegacyItemId])
		VALUES(@PortalID ,-1,'PRDLANG',@xml,'',CURRENT_TIMESTAMP,'',0,(select top 1 itemid from NBrightBuy where [LegacyItemId] = @ProductID and isnull([Lang],'') = '') ,@lang,'',@ProductID)


FETCH NEXT FROM ins_cursor INTO @ProductID,@Lang,@Summary,@Description,@Manufacturer,@ProductName,@XMLData,@SEOName,@TagWords,@SEOPageTitle
END 
CLOSE ins_cursor;
DEALLOCATE ins_cursor;






END
GO



EXEC {databaseOwner}[{objectQualifier}NBrightBuy_MigrateCategoryData]
GO

EXEC {databaseOwner}[{objectQualifier}NBrightBuy_MigrateProductData] 
GO
