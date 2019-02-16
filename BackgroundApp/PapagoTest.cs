﻿using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Polly;
using System;
using System.Text.RegularExpressions;

namespace BackgroundApp
{
    /// <summary>
    /// Papago ui test for fetching translated text.
    /// </summary>
    public class PapagoTest
    {
        private IWebDriver _webDriver;
        private By _translatedTextArea = By.Id("txtTarget");
        private const int _waitTime = 20;

        public PapagoTest()
        {
            this._webDriver = createWebDriver();
        }

        private IWebDriver createWebDriver()
        {
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("window-size=1920,1080");
            options.AddArgument("headless");
            IWebDriver webDriver = new ChromeDriver(options);
            return webDriver;
        }

        /// <summary>
        /// Quit if web driver is initialized.
        /// </summary>
        public void Quit()
        {
            if (_webDriver != null)
            {
                _webDriver.Quit();
            }
        }

        /// <summary>
        /// Translate the text from source language to target language.
        /// </summary>
        /// <param name="text">Text to translate</param>
        /// <param name="sourceLanguage">Source language</param>
        /// <param name="targetLanguage">Target language</param>
        /// <returns>Translated text</returns>
        public string Translate(string text, LanguageCode sourceLanguage, LanguageCode targetLanguage)
        {
            // TODO: if request is too big, should run this several times - 2,083 characters

            _webDriver.Url = GetUrl(RemoveIllegalCharacters(text), sourceLanguage, targetLanguage);
            //return GetTranslatedText();
            string result = GetTranslatedText();
            Console.WriteLine(result);
            return result;
        }

        private string GetTranslatedText()
        {
            WebDriverWait wait = new WebDriverWait(_webDriver, new TimeSpan(0, 0, _waitTime));
            IWebElement webElement = null;
            Policy
                .Handle<NoSuchElementException>()
                .WaitAndRetry(3, retryAttempt => TimeSpan.FromSeconds(3))
                .Execute(() => {
                    wait.Until(driver => driver.FindElement(_translatedTextArea));
                    webElement = _webDriver.FindElement(_translatedTextArea);
                });
            
            if (webElement == null)
            {
                throw new Exception("Failed to get translated text...");
            } else
            {
                return webElement.Text;
            }
        }

        private string GetUrl(string text, LanguageCode sourceLanguage, LanguageCode targetLanguage)
        {
            string url = "https://papago.naver.com/?sk=[SOURCE]&tk=[TARGET]&st=[TEXT]"
                .Replace("[SOURCE]", sourceLanguage.ToLanguageCode())
                .Replace("[TARGET]", targetLanguage.ToLanguageCode())
                .Replace("[TEXT]", Uri.EscapeUriString(text));

            if (targetLanguage.Equals(LanguageCode.KOREAN))
            {
                url.Replace("&st", "&hn=0&");
            }

            return url;
        }

        private string RemoveIllegalCharacters(string text)
        {
            return new Regex("[{}^]").Replace(text, "");
        }

        static void Main(String[] args)
        {

        }

        ~PapagoTest()
        {
            this.Quit();
        }
    }
}
