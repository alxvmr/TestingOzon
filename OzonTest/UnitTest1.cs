using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;

namespace OzonTest
{
    public class Tests
    {
        IWebDriver driver;
        public static String BASE_URL = "https://www.ozon.ru/";
        [SetUp]
        public void Setup()
        {
            driver = new ChromeDriver();
            driver.Manage().Window.Maximize();
            driver.Manage().Timeouts().ImplicitWait = System.TimeSpan.FromSeconds(7);
            driver.Navigate().GoToUrl(BASE_URL);
        }

        [Test]
        public void Filter()
        {
            driver.FindElement(By.XPath("//*[contains(@href , '/category/elektronika-15500/') and contains(@class, 'a3p5 g5e3')]")).Click();
            driver.FindElement(By.XPath("//*[@href = '/category/smartfony-15502/']")).Click();
            driver.FindElement(By.XPath("//div[contains(@class, 'ui-y4 ui-y7') and contains (.//p, 'от')]//input")).SendKeys(Keys.Control + Keys.Backspace);
            driver.FindElement(By.XPath("//div[contains(@class, 'ui-y4 ui-y7') and contains (.//p, 'от')]//input")).SendKeys("30000");
            driver.FindElement(By.XPath("//div[contains(@class, 'ui-y4 ui-y7') and contains (.//p, 'до')]//input")).SendKeys(Keys.Control + Keys.Backspace);
            driver.FindElement(By.XPath("//div[contains(@class, 'ui-y4 ui-y7') and contains (.//p, 'до')]//input")).SendKeys("100000");
            driver.FindElement(By.XPath("//div[contains(@class, 'ui-y4 ui-y7') and contains (.//p, 'до')]//input")).SendKeys(Keys.Enter);
            new WebDriverWait(driver, TimeSpan.FromSeconds(3))
                .Until(x => driver.FindElements(By.XPath("//div[@data-widget = 'megaPaginator']//div[@class = 'a3r3 a3r8']")).Any());
            new WebDriverWait(driver, TimeSpan.FromSeconds(30))
                .Until(x => driver.FindElements(By.XPath("//div[@data-widget = 'megaPaginator']//div[@class = 'a3r3 a3r8']")).Count == 0);

            string deleteSpace(string s)
            {
                string str = "";
                for (int i = 0; i < s.Length; i++)
                {
                    string t = s[i].ToString();
                    if (t != " ")
                        str = str + t;
                }

                return str;
            }

            string[] actualValuesStr = Array.ConvertAll(driver.FindElements(By.XPath("//div[@data-widget = 'searchResultsV2']//span[@class = 'ui-p5 ui-p8 ui-q0']"))
                .Select(webPrice => webPrice.Text.Trim()).ToArray<string>(), s => s.ToString().Substring(0, s.Length - 2)); //убраны рубли и пробел перед ними
            int[] actualValues = actualValuesStr.Select(x => int.Parse(deleteSpace(x))).ToArray();
            actualValues.ToList().ForEach(actualPrice => Assert.True(actualPrice >= 30000 && actualPrice <= 100000, "Price filter works wrong. Actual price is " + actualPrice + ". But should be more or equal than 30000 and less or equal than 100000"));
        }

        [Test]
        public void TestTooltipText()
        {
            new Actions(driver).MoveToElement(driver.FindElement(By.XPath("//div[@data-widget = 'profileMenuAnonymous']"))).Build().Perform();
            Assert.IsTrue(driver.FindElements(By.XPath("//div[@class = 'vue-portal-target']//span[@class = 'ui-e6' and contains(text(), 'Личный кабинет')]")).Any(),
                "Tooltip has not appeared.");
            Assert.AreEqual("Личный кабинет", driver.FindElement(By.XPath("//div[@class = 'vue-portal-target']//span[@class = 'ui-e6' and contains(text(), 'Личный кабинет')]")).Text.Trim(),
               "Tooltip has not appeared.");
        }

        [Test]
        public void NegativeSignUpTest()
        {
            driver.FindElement(By.XPath("//div[@data-widget='loginButton']")).Click();
            driver.SwitchTo().Frame("authFrame");
            driver.FindElement(By.XPath("//button[@type='submit']")).Click();
            driver.FindElement(By.XPath("//button[@type='submit']")).Click();
            Assert.IsTrue(driver.FindElements(By.XPath("//p[@class='ui-p2' and contains(text(), 'Заполните телефон')]")).Any(),
               "Phone number confirmation button is enabel when phone number input has no value.");
        }


        [TearDown]
        public void CleanUp()
        {
            driver.Quit();
        }
    }
}