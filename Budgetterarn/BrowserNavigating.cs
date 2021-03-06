﻿using System;
using System.Windows.Forms;

namespace Budgetterarn
{
    public class BrowserNavigating
    {
        private WebBrowser webBrowser1;

        public BrowserNavigating(WebBrowser webBrowser)
        {
            webBrowser1 = webBrowser;
        }

        public void NavigateToFirstItemInVisibleList()
        {
            if (webBrowser1.Document != null)
            {
                if (webBrowser1.Document.Body != null)
                {
                    // ReSharper disable PossibleNullReferenceException
                    if (webBrowser1.Document.Body.FirstChild.FirstChild.FirstChild.FirstChild.NextSibling.NextSibling
                        != null)
                    {
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

                        var logginElem = baseElem.FirstChild;

                        NavigateToAsHref(logginElem);
                    }
                }
            }
        }

        public void NavigateTo3rdinsideEgVisaFler()
        {
            NavigateToAsHref(webBrowser1.Document.Body.FirstChild.FirstChild.FirstChild.FirstChild.NextSibling.NextSibling.FirstChild.NextSibling.NextSibling.FirstChild.FirstChild);
        }

        private void NavigateToAsHref(HtmlElement navigateAElem)
        {
            var href = navigateAElem.GetAttribute("href");

            var url = href;
            webBrowser1.Navigate(url);
        }

        // private void webBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        // {

        // }
        public void SetLoginUserEtc()
        {
            if (webBrowser1.Document != null && webBrowser1.Document.Body != null)
            {
                var baselogginElem =

                    // ReSharper disable PossibleNullReferenceException
                    webBrowser1.Document.Body.FirstChild.FirstChild.FirstChild.FirstChild.NextSibling.NextSibling
                               .FirstChild.FirstChild.NextSibling;

                // ReSharper restore PossibleNullReferenceException
                if (baselogginElem != null)
                {
                    var userNameElem = baselogginElem.FirstChild;

                    // ReSharper disable PossibleNullReferenceException
                    var passElem = baselogginElem.NextSibling.NextSibling.FirstChild;

                    // ReSharper restore PossibleNullReferenceException

                    // System.Web.UI.HtmlControls.HtmlInputControl;

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

                // (webBrowser1.FindForm() as HtmlElement)
                // .InvokeMember("submit");
                Submit(webBrowser1);
            }
        }

        private void Submit(WebBrowser inWebBrowserControl)
        {
            if (inWebBrowserControl.Document != null)
            {
                var elements = inWebBrowserControl.Document.GetElementsByTagName("Form");

                foreach (HtmlElement currentElement in elements)
                {
                    currentElement.InvokeMember("submit");
                }
            }
        }

        public void NavigateToAllKonto()
        {
            if (webBrowser1.Document != null)
            {
                var oneElem = FindChildWithId(webBrowser1.Document.Body, "item_1").FirstChild;
                if (oneElem == null)
                {
                    throw new ArgumentNullException("one" + "Elem");
                }

                NavigateToAsHref(oneElem);
            }
        }

        public void NavigateToLöneKonto()
        {
            // item_2

            // webBrowser1

            // var Dd = webBrowser1.Document.Body.FirstChild;
            // Dd = FindChildWithId(webBrowser1.Document.Body, "item_2");
            if (webBrowser1.Document == null)
            {
                return;
            }

            var oneElem = FindChildWithId(webBrowser1.Document.Body, "item_2").FirstChild;
            NavigateToAsHref(oneElem);

            // var href = elem.FirstChild.GetAttribute("href");

            // var url = href;
            // webBrowser1.Navigate(url);
        }

        public void BrowserGoBack()
        {
            if (webBrowser1.CanGoBack)
            {
                webBrowser1.GoBack();
            }
        }

        public HtmlElement FindChildWithId(HtmlElement htmlElement, string idToFind)
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

            if (returnHtmlElement != null)
            {
                return returnHtmlElement;
            }

            return null;
        }

    }
}
