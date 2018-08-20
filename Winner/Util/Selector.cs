using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;

namespace Winner
{
    class CssSelector
    {
        internal static IWebElement FindElement(IWebDriver driver, string selector)
        {
            IWebElement element = null;

            try
            {
                element = driver.FindElement(By.CssSelector(selector));
            }
            catch (Exception e)
            {
                return null;
            }
            return element;
        }
    }
}
