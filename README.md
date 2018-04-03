# NBrightBuy
NBrightStore - E-Commerce for DNN (NBSv3)

Developer please read this to get started:

http://nbsdocs.nbrightproject.org/Documentation/Developerguide/DevSetup.aspx

To load MSBuild NuGet use Package Manager:
PM> Install-Package MSBuildTasks -Version 1.5.0.235 

v3.6.6
- fix option text not going to multi-langauge data area on save.
- Fix redirect loop of url on langauge change.
- Unify Processing Icon
- Split shared product/category option.
- Add Custom Action provider.

v3.6.5
- Allow shipping provider to adjust all shipping cost by a percentage.
- Fix template to display document download name.
- Fix bug on category sort

v3.6.3
- update JS file.

v3.6.2
- Fix bug on Category removal and default.

v3.6.1
- Fix to category Product Select.
- Fix Tax Drop Down to display description, not value.
- Add apply tax to all products in a category.
- Redisplay missing add button for products.
- Activate Richtext for bespoke model fields.

v3.6.0
- Convert Product Admin to Razor.
- BREAKING CHANGE TO CUSTOM PRODUCT FIELD - The file "producfield.html" is used to create bespoke field in the product admin.  If you have used this file it will need to be converted to razor as  "productfield.cshtml"
- Allow custom field on the model, using the file called "modelfields.cshtml"
- Convert Client Admin to Razor.
- Add custom fields to Client Admin "clientfields.cshtml"
- REQUIRED min. v8.2.0.0 on NBrightTS  (Templating system)

v3.6.7
- fix bug on multiple langauge save.
- Close possible SQL hole in typecode lang record select for GetDataLang SPROC
- Add Tag Word js. 
- Add Ajax Product list
- Add userbased favorite lists
- Add CKEditor param
- Enable shared products on import.
- Option to Ignore Shared records on Export

