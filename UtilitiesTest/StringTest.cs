using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefLesses;

namespace UtilitiesTest
{
    [TestClass]
    public class StringTest
    {
        [TestMethod]
        public void TestGetTextBetweenStartAndEndText()
        {
            var result = StringFunctions.GetTextBetweenStartAndEndText(TestText, "LÖNEKONTO - 629 010 552",
                "Att tänka på inför årsskiftet");

            var splitResult = result.Substring(0, result.Length / 2);

            Assert.AreEqual("163421,75", splitResult);
        }

        private static string TestText =>
            @"









Gå till
InnehållOmrådesmenyHuvudmeny






Per GradeenPersonliga inställningar


Min profil

Logga ut






Göteborg Lindholmen



Sök bankärende: Minst två bokstäver för sökning. Sökresultatet visas direkt efter sökfältet.






StartStart

Konton och kortKonton och kort



Betala och överföraBetala och överföra



LånLån



Spara och placeraSpara och placera



Pension och försäkringPension och försäkring











Öppna meny - alla val inom Konton och kort Navigera med tabb, skift-tabb i menynStäng meny







Öppna meny - alla val inom Betala och överföra Navigera med tabb, skift-tabb i menynStäng meny







Öppna meny - alla val inom Lån Navigera med tabb, skift-tabb i menynStäng meny







Öppna meny - alla val inom Spara och placera Navigera med tabb, skift-tabb i menynStäng meny







Öppna meny - alla val inom Pension och försäkring Navigera med tabb, skift-tabb i menynStäng meny


















Start








Giltighetstiden på ditt inloggningskort går snart ut


Du behöver välja PIN-kod till ett nytt inloggningskort. När du valt kod får du ditt nya inloggningskort hemskickat.
Läs mer om ditt nya inloggningskort.Öppnas i nytt fönster

Välj kod (sex siffror)




Du kan välja samma kod som du har idag eller en ny kod.


*

Koden ska bestå av sex siffror. Den får inte bestå av siffror i följd eller sex likadana siffror.

Upprepa kod




OBS! Kom ihåg koden du valt. Du kommer inte kunna se den igen eller ändra den.

*Koderna överensstämmer inte


Skicka









Konton och kort



Modulmeny Konton och kort



Informationom Konton och kort, öppnas i ett nytt fönster

MinimeraKonton och kort

UppdateraKonton och kort










Namn


Saldo


Disponibelt belopp




Allkortskonto - 629 011 192
58 250,8356 270,45


LÖNEKONTO - 629 010 552
163 421,75163 421,75









Att tänka på inför årsskiftet


Gör dina betalningar i god tid
Senast den 29 december ska du signera dina betalningar, inom Sverige, för att de ska vara mottagaren tillhanda före årsskiftet.



Se över ditt sparande
Har du sålt fonder, aktier eller andra värdepapper under året? Då kan du ha kapitalvinster och kapitalförluster att kvitta som minskar årets skatt. 
Tips inför årsskiftet











	
Vanliga ärenden

Betala


Överföra


Kommande transaktioner


Utförda transaktioner


Fondinnehav






E-brevlåda (18 olästa)


Skicka nytt meddelande





Ekonomisk översikt


Internet, mobil och BankID


Din kundbonus








Säkerhet
Vi arbetar ständigt med att skapa en säker internetmiljö för våra tjänster. Men du har själv ansvar för att skydda din dator och dina uppgifter.

Skydda din dator









Villkor | Ansvarsbegränsning | För dig som bor utanför Sverige | Prislista 



Ekonomisk översikt
        ";
    }
}