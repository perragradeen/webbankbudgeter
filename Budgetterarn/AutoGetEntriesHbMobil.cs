using System.Collections.Generic;
using System.Windows.Forms;

namespace Budgetterarn
{
    public delegate void DoneNavigationAction();

    public class AutoGetEntriesHbMobil
    {
        // Ev. byta denna mot en klass med innehåll och nyckel, för att behålla orginalordningen på posterna. Sorteras med nyaste först
        private readonly Stack<DoneNavigationAction> navigatedNextActionIsStack;
        private readonly BrowserNavigating browserNavigator;
        private readonly DoneNavigationAction LoadCurrentEntriesFromBrowser;

        public AutoGetEntriesHbMobil(DoneNavigationAction loadCurrentEntriesFromBrowser, WebBrowser webBrowser)
        {
            navigatedNextActionIsStack = new Stack<DoneNavigationAction>();
            browserNavigator = new BrowserNavigating(webBrowser);
            this.LoadCurrentEntriesFromBrowser = loadCurrentEntriesFromBrowser;
        }

        public BrowserNavigating BrowserNavigator { get { return browserNavigator; } }

        public void LoadingCompleted()
        {
            if (navigatedNextActionIsStack.Count > 0)
            {
                // SetLoginUserEtc();
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

            // navigatedNextActionIsSatck.Push(NavigateToFirstItemInVisibleList);

            // Allkonto
            navigatedNextActionIsStack.Push(LoadEntriesAndGoToFirst);
            navigatedNextActionIsStack.Push(browserNavigator.NavigateToAllKonto);

            // navigatedNextActionIsSatck.Push(BrowserGoBack);

            // Löne
            navigatedNextActionIsStack.Push(LoadEntriesAndGoBack);
            navigatedNextActionIsStack.Push(browserNavigator.NavigateToLöneKonto);

            // Inlogg
            navigatedNextActionIsStack.Push(browserNavigator.NavigateToFirstItemInVisibleList);
            navigatedNextActionIsStack.Push(browserNavigator.SetLoginUserEtc);
            navigatedNextActionIsStack.Push(browserNavigator.NavigateToFirstItemInVisibleList);
        }

        private void LoadEntriesAndGoBack()
        {
            LoadCurrentEntriesFromBrowser();
            browserNavigator.BrowserGoBack();
        }

        private void LoadEntriesAndGoToFirst()
        {
            LoadCurrentEntriesFromBrowser();
            browserNavigator.NavigateToFirstItemInVisibleList();
        }

        private void LoadEntriesAndVisaFler()
        {
            LoadCurrentEntriesFromBrowser();
            browserNavigator.NavigateTo3rdinsideEgVisaFler();
        }
    }
}
