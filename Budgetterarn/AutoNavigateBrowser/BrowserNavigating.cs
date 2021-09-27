using System;
using System.Windows.Forms;

namespace Budgetterarn.AutoNavigateBrowser
{
    public class BrowserNavigating
    {
        private readonly WebBrowser webBrowser1;

        public BrowserNavigating(WebBrowser webBrowser)
        {
            webBrowser1 = webBrowser;
        }

        public void NavigateToFirstItemInVisibleList()
        {
            if (webBrowser1.Document == null) return;
            if (webBrowser1.Document.Body == null) return;

            // ReSharper disable PossibleNullReferenceException
            if (webBrowser1.Document.Body
                .FirstChild.FirstChild.FirstChild.FirstChild.NextSibling.NextSibling == null) return;

            var baseElem =
                webBrowser1.Document.Body.FirstChild.FirstChild.FirstChild.FirstChild.NextSibling.FirstChild
                    .FirstChild
                ?? webBrowser1.Document.Body.FirstChild.FirstChild.FirstChild.FirstChild.NextSibling
                    .NextSibling.FirstChild.FirstChild;

            // ReSharper restore PossibleNullReferenceException
            if (baseElem == null)
            {
                return;
            }

            var loginElem = baseElem.FirstChild;

            NavigateToAsHref(loginElem);
        }

        public void NavigateTo3rdinsideEgVisaFler()
        {
            NavigateToAsHref(webBrowser1.Document?.Body?.FirstChild?.FirstChild?.FirstChild?.FirstChild?
                .NextSibling?.NextSibling?.FirstChild?.NextSibling?.NextSibling?.FirstChild?.FirstChild);
        }

        private void NavigateToAsHref(HtmlElement navigateAElem)
        {
            var href = navigateAElem.GetAttribute("href");

            webBrowser1.Navigate(href);
        }

        public void SetLoginUserEtc()
        {
            if (webBrowser1.Document == null || webBrowser1.Document.Body == null) return;

            var baseloginElem =

                // ReSharper disable PossibleNullReferenceException
                webBrowser1.Document.Body.FirstChild.FirstChild.FirstChild.FirstChild.NextSibling.NextSibling
                    .FirstChild.FirstChild.NextSibling;

            // ReSharper restore PossibleNullReferenceException
            if (baseloginElem != null)
            {
                var userNameElem = baseloginElem.FirstChild;

                // ReSharper disable PossibleNullReferenceException
                var passElem = baseloginElem.NextSibling.NextSibling.FirstChild;

                // ReSharper restore PossibleNullReferenceException

                // Set attrib. Login
                if (userNameElem != null)
                {
                    userNameElem.SetAttribute("value", "7906072439");
                }

                if (passElem != null)
                {
                    passElem.SetAttribute("value", "2222");
                }
            }

            Submit(webBrowser1);
        }

        private static void Submit(WebBrowser inWebBrowserControl)
        {
            if (inWebBrowserControl.Document == null) return;

            var elements = inWebBrowserControl.Document.GetElementsByTagName("Form");

            foreach (HtmlElement currentElement in elements)
            {
                currentElement.InvokeMember("submit");
            }
        }

        public void NavigateToAllKonto()
        {
            if (webBrowser1.Document == null) return;
            var oneElem = FindChildWithId(webBrowser1.Document.Body, "item_1").FirstChild;
            if (oneElem == null)
            {
                throw new ArgumentNullException("one" + "Elem");
            }

            NavigateToAsHref(oneElem);
        }

        public void NavigateToLöneKonto()
        {
            if (webBrowser1.Document == null)
            {
                return;
            }

            var oneElem = FindChildWithId(webBrowser1.Document.Body, "item_2").FirstChild;
            NavigateToAsHref(oneElem);
        }

        public void BrowserGoBack()
        {
            if (webBrowser1.CanGoBack)
            {
                webBrowser1.GoBack();
            }
        }

        private static HtmlElement FindChildWithId(HtmlElement htmlElement, string idToFind)
        {
            if (htmlElement.Id != null && htmlElement.Id.Equals(idToFind))
            {
                return htmlElement;
            }

            HtmlElement returnHtmlElement = null;
            foreach (HtmlElement item in htmlElement.Children)
            {
                returnHtmlElement = FindChildWithId(item, idToFind);
            }

            if (returnHtmlElement != null)
            {
                return returnHtmlElement;
            }

            while ((htmlElement = htmlElement.NextSibling) != null)
            {
                returnHtmlElement = FindChildWithId(htmlElement, idToFind);
            }

            return returnHtmlElement;
        }
    }
}