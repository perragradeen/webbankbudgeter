using System.Collections.Generic;
using System.Windows.Forms;

namespace Budgetterarn.AutoNavigateBrowser
{
    public class AutoGetEntriesHbMobil
    {
        // Ev. byta denna mot en klass med innehåll och nyckel, för att behålla orginalordningen på posterna. Sorteras med nyaste först
        private readonly Stack<DoneNavigationAction> navigatedNextActionIsStack;
        private readonly DoneNavigationAction LoadCurrentEntriesFromBrowser;

        public AutoGetEntriesHbMobil(
            DoneNavigationAction loadCurrentEntriesFromBrowser,
            WebBrowser webBrowser)
        {
            navigatedNextActionIsStack = new Stack<DoneNavigationAction>();
            BrowserNavigator = new BrowserNavigating(webBrowser);
            LoadCurrentEntriesFromBrowser = loadCurrentEntriesFromBrowser;
        }

        public BrowserNavigating BrowserNavigator { get; }

        public void LoadingCompleted()
        {
            if (navigatedNextActionIsStack.Count > 0)
            {
                var navigatedNextActionIs = navigatedNextActionIsStack.Pop();
                navigatedNextActionIs.Invoke();
            }
        }

        /// <summary>
        /// Måste alltid avsluta med navigering el. likn. för att browsern ska lämna ett laddat-klar-event så att nästa sak i stacken kan köras
        /// </summary>
        public void AutoNavigateToKontonEtc()
        {
            // Korttransaktioner
            navigatedNextActionIsStack.Push(LoadEntriesAndVisaFler);
            navigatedNextActionIsStack.Push(LoadEntriesAndVisaFler);
            navigatedNextActionIsStack.Push(LoadEntriesAndVisaFler);
            navigatedNextActionIsStack.Push(LoadEntriesAndVisaFler);
            navigatedNextActionIsStack.Push(LoadEntriesAndVisaFler);
            navigatedNextActionIsStack.Push(LoadEntriesAndVisaFler);
            navigatedNextActionIsStack.Push(LoadEntriesAndVisaFler);
            navigatedNextActionIsStack.Push(LoadEntriesAndVisaFler);
            navigatedNextActionIsStack.Push(LoadEntriesAndVisaFler);
            navigatedNextActionIsStack.Push(LoadEntriesAndVisaFler);
            navigatedNextActionIsStack.Push(LoadEntriesAndVisaFler);
            navigatedNextActionIsStack.Push(LoadEntriesAndVisaFler);
            navigatedNextActionIsStack.Push(LoadEntriesAndVisaFler);

            // Allkonto
            navigatedNextActionIsStack.Push(LoadEntriesAndGoToFirst);
            navigatedNextActionIsStack.Push(BrowserNavigator.NavigateToAllKonto);

            // Löne
            navigatedNextActionIsStack.Push(LoadEntriesAndGoBack);
            navigatedNextActionIsStack.Push(BrowserNavigator.NavigateToLöneKonto);

            // Inlogg
            navigatedNextActionIsStack.Push(BrowserNavigator.NavigateToFirstItemInVisibleList);
            navigatedNextActionIsStack.Push(BrowserNavigator.SetLoginUserEtc);
            navigatedNextActionIsStack.Push(BrowserNavigator.NavigateToFirstItemInVisibleList);
        }

        private void LoadEntriesAndGoBack()
        {
            LoadCurrentEntriesFromBrowser();
            BrowserNavigator.BrowserGoBack();
        }

        private void LoadEntriesAndGoToFirst()
        {
            LoadCurrentEntriesFromBrowser();
            BrowserNavigator.NavigateToFirstItemInVisibleList();
        }

        private void LoadEntriesAndVisaFler()
        {
            LoadCurrentEntriesFromBrowser();
            BrowserNavigator.NavigateTo3rdinsideEgVisaFler();
        }
    }
}