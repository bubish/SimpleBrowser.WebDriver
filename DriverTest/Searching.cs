﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenQA.Selenium;
using SimpleBrowser.WebDriver;
using Moq;
using SimpleBrowser;
using System.Reflection;
using System.IO;
using OpenQA.Selenium.Support.UI;

namespace DriverTest
{
    [TestFixture]
    public class Searching
    {
        [Test]
        public void UsingFindElements_Should_Convert_To_Correct_Jquery_Selector_Call()
        {
            Mock<IHtmlResult> mockHtmlRoot;
            SetupElementSearch(By.ClassName("test"), out mockHtmlRoot);
            mockHtmlRoot.Verify(r => r.Select(".test"));

            SetupElementSearch(By.Id("test"), out mockHtmlRoot);
            mockHtmlRoot.Verify(r => r.Select("#test"));

            SetupElementSearch(By.CssSelector("div.blah>myid"), out mockHtmlRoot);
            mockHtmlRoot.Verify(r => r.Select("div.blah>myid"));

            SetupElementSearch(By.LinkText("test"), out mockHtmlRoot);
            mockHtmlRoot.Verify(r => r.Select("a"));
        }
        [Test]
        public void UsingSelectBoxes()
        {
            Browser b = new Browser();
            b.SetContent(GetFromResources("DriverTest.GitHub.htm"));
            IWebDriver driver = new SimpleBrowserDriver(new BrowserWrapper(b));
            var selectbox = driver.FindElement(By.Name("sel"));
            var box = new SelectElement(selectbox);
            Assert.That(box.SelectedOption.Text == "two");
            box.SelectByValue("3");
            Assert.That(box.SelectedOption.Text == "three");
            box.SelectByText("one");
            Assert.That(box.SelectedOption.Text == "one");

			selectbox = driver.FindElement(By.Name("sel_multi"));
			box = new SelectElement(selectbox);
			Assert.That(box.IsMultiple);
			box.SelectByValue("3");
			box.SelectByText("one");
			Assert.That(box.AllSelectedOptions.Count == 3);

		}
		[Test]
		public void UsingCheckboxes()
		{
			Browser b = new Browser();
			b.SetContent(GetFromResources("DriverTest.GitHub.htm"));
			IWebDriver driver = new SimpleBrowserDriver(new BrowserWrapper(b));
			var checkbox1 = driver.FindElement(By.CssSelector(".cb-container #first-checkbox"));
			var checkbox2 = driver.FindElement(By.CssSelector(".cb-container #second-checkbox"));
			Assert.That(checkbox1.Selected, "Checkbox 1 should be selected");
			Assert.That(!checkbox2.Selected, "Checkbox 2 should not be selected");
			checkbox2.Click();
			Assert.That(checkbox1.Selected, "Checkbox 1 should still be selected");
			Assert.That(checkbox2.Selected, "Checkbox 2 should be selected");
			var checkbox1Label = driver.FindElement(By.CssSelector("label[for=first-checkbox]"));
			Assert.NotNull(checkbox1Label, "Label not found");
			checkbox1Label.Click();
			Assert.That(checkbox2.Selected, "Checkbox 2 should still be selected");
			Assert.That(!checkbox1.Selected, "Checkbox 1 should be not selected");
			
		}
		[Test]
		public void UsingRadioButtons()
		{
			Browser b = new Browser();
			b.SetContent(GetFromResources("DriverTest.GitHub.htm"));
			IWebDriver driver = new SimpleBrowserDriver(new BrowserWrapper(b));
			var radio1 = driver.FindElement(By.CssSelector(".rb-container #first-radio"));
			var radio2 = driver.FindElement(By.CssSelector(".rb-container #second-radio"));
			var radio1Label = driver.FindElement(By.CssSelector("label[for=first-radio]"));
			Assert.That(radio1.Selected, "Radiobutton 1 should be selected");
			Assert.That(!radio2.Selected, "Radiobutton 2 should not be selected");
			radio2.Click();
			Assert.That(!radio1.Selected, "Radiobutton 1 should not be selected");
			Assert.That(radio2.Selected, "Radiobutton 2 should be selected");
			Assert.NotNull(radio1Label, "Label not found");
			radio1Label.Click();
			Assert.That(radio1.Selected, "Radiobutton 1 should be selected");
			Assert.That(!radio2.Selected, "Radiobutton 2 should be not selected");

		}
		[Test]
		public void UsingTextboxes()
		{
			Browser b = new Browser();
			b.SetContent(GetFromResources("DriverTest.GitHub.htm"));
			IWebDriver driver = new SimpleBrowserDriver(new BrowserWrapper(b));
			var textbox = driver.FindElement(By.CssSelector("#your-repos-filter"));
			Assert.NotNull(textbox, "Couldn't find textbox");
			Assert.That(textbox.Text == String.Empty, "Textbox without a value attribute should have empty text");
			textbox.SendKeys("test text");
			Assert.That(textbox.Text == "test text", "Textbox did not pick up sent keys");
			textbox.SendKeys(" more");
			Assert.That(textbox.Text == "test text more", "Textbox did not append second text");
			textbox.Clear();
			Assert.That(textbox.Text == String.Empty, "Textbox after Clear should have empty text");

		}
		[Test]
        public void SearchingInKnownDocument()
        {
            Browser b = new Browser();
            b.SetContent(GetFromResources("DriverTest.GitHub.htm"));
            IWebDriver driver = new SimpleBrowserDriver(new BrowserWrapper(b));

            var iconSpans = driver.FindElements(By.CssSelector("span.icon"));
            Assert.That(iconSpans.Count == 4, "There should be 4 spans with class icon");

            var accountSettings = driver.FindElements(By.CssSelector("*[title~= Account]"));
            Assert.That(accountSettings.Count == 1 && accountSettings[0].Text == "Account Settings", "There should be 1 element with title containing the word Account");

            var topStuff = driver.FindElements(By.CssSelector("*[class|=top]"));
            Assert.That(topStuff.Count == 3 , "There should be 3 elements with class starting with top-");

            var h2s = driver.FindElements(By.CssSelector("h2"));
            Assert.That(h2s.Count == 8, "There should be 8 h2 elements");

            var titleContainingTeun = driver.FindElements(By.CssSelector("*[title*=Teun]"));
            Assert.That(titleContainingTeun.Count == 3, "There should be 3 elements with 'Teun' somewhere in the title attrbute");
        }
        [Test]
        public void UsingBrowserDirect()
        {
            Browser b = new Browser();
            b.SetContent(GetFromResources("DriverTest.GitHub.htm"));
            var found = b.Find("div", FindBy.Class, "issues_closed");
            Assert.That(found.TotalElementsFound == 3);
        }
        internal string GetFromResources(string resourceName)
        {
            Assembly assem = this.GetType().Assembly;
            using (Stream stream = assem.GetManifestResourceStream(resourceName))
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        private void SetupElementSearch(By by, out Mock<IHtmlResult> mock)
        {
            var browserMock = new Mock<IBrowser>();
            mock = new Mock<IHtmlResult>();
            var foundElement = new Mock<IHtmlResult>();
            var elmEnumerator = new Mock<IEnumerator<IHtmlResult>>();

            foundElement.Setup(h => h.TotalElementsFound).Returns(1);
            foundElement.Setup(h => h.GetEnumerator()).Returns(elmEnumerator.Object);
            elmEnumerator.Setup(e => e.Current).Returns(foundElement.Object);
            elmEnumerator.SetupSequence(e => e.MoveNext()).Returns(true).Returns(false);
            mock.Setup(h => h.TotalElementsFound).Returns(1);
            mock.Setup(h => h.Select(It.IsAny<string>())).Returns(foundElement.Object);
            mock.Setup(root => root.Select(It.IsAny<string>())).Returns(foundElement.Object);
            browserMock.Setup(browser => browser.Find("html", It.IsAny<object>())).Returns(mock.Object);



            string url = "http://testweb.tst";
            SimpleBrowserDriver driver = new SimpleBrowserDriver(browserMock.Object);
            driver.Navigate().GoToUrl(url);
            driver.FindElements(by);

            browserMock.Verify(b => b.Navigate(url));
            browserMock.Verify(b => b.Find("html", It.IsAny<object>()));
        }

    }
}
