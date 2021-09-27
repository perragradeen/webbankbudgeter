using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Budgetterarn")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Per Gradeen")]
[assembly: AssemblyProduct("Budgetterarn")]
[assembly: AssemblyCopyright("Use freely just mention Per Gradeen")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("d13664a5-09e0-4561-a100-92af834bfc29")]

// Version information for an assembly consists of the following four values:
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.1.16")]
[assembly: AssemblyFileVersion("1.0.1.16")]

// IF CHANGED VERSION. DOCUMENT CHANGES!!!AND COMMIT See Changes summary below.
// 1.0.1.16 Swedbank funkar nu med senaste. Lite refaktor lämnad etc
// 1.0.1.15 Möjligt att Ladda från pdf-fil med gamla kredit-rader. Allkortsfaktura SHB
// 1.0.1.14 Anpassning till nyare Handelsbanken inlgoggning via dosa. Även kredit-kontotinfo etc. Stor Refaktor
// 1.0.1.12 Anpassning till nyare Handelsbanken inlgoggning via dosa
// 1.0.1.11 Stor refaktor av kod. Sparar datum som år, månad och dag. Fler klick bakåt i historiken på allkortsköp.
// 1.0.1.10 Stor refaktor av kod. Introducerat en ny bugg vid körning...
// 1.0.1.9 Gjort anpassningar till Excel engelsk version map datumformat etc. Nu loggas celler som objekt istället för strängar. Div omstrukturering i testProjekt och tillagda projekt.
// 1.0.1.8 För handelsbanken mobil. Autonavigera med inloggning etc. Så allt nytt laddas in automatiskt. + Snabbkanapp Ctrl+L för att ladda entries.
// 1.0.1.7 Kan nu ladda entries från Handelsbanken mobilsida. Som har enklare inloggning. +Buggfix med sortering av nya entries.
// 1.0.1.6 Enklare sparning utan prompt. Autosave som val när man lagt till nya entries. Även ändrat för funktioner som sparar och laddar. Fixat så det går att anv designervyn i VS.
// 1.0.1.5 Fixed new saldos for SHB. Double-mbox clearified, better handling of uniques and double-entries.
// 1.0.1.4 Fixed autocat set, so it is less unneccesary popups to user. Started to Add functionality for Swedbank.
// 1.0.1.3 Addad exception catch att Exclel close. Now user selects if autocat shold overwrite existing choices. Added info about how to set several cats at the same time. Added sorting on listviews. Columns from excel should now be correct in listviews.
// 1.0.1.2 Fixed so tag is also set when just selecting cat on new entries from web, also a halfsmart (not full proof, bu probably never gonna err...) doublechecker added.
// 1.0.1.1 Changed way Version number is set
// 1.0.1.0 PopupComboboxOfCaytegories had a bugg with wrong colwidth added when checking postion, only noticable if not all columns have same length. Nicer set autocat and popup. användaren kan sätta autokat.
// 1.0.0.1 Nothing new yet, Later singleclick in newlist etc.
// 1.0.0.0 Everything before, see Svn. Even Added mulitiselect etc.