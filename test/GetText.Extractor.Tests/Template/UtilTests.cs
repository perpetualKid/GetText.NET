using System;
using System.Globalization;

using GetText.Extractor.Template;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GetText.Extractor.Tests.Template
{
    [TestClass]
    public class DateTimeExtensionTests
    {
        [TestMethod]
        public void ToRfc822FormatTest()
        {
            DateTime now = DateTime.Now;
            //2020-05-26 20:01:14+0200
            string rfcDate = now.ToRfc822Format();
            Assert.AreEqual(24, rfcDate.Length);
            Assert.AreEqual(now.Date,
                new DateTime(int.Parse(rfcDate[..4], CultureInfo.InvariantCulture),
                int.Parse(rfcDate.Substring(5, 2), CultureInfo.InvariantCulture),
                int.Parse(rfcDate.Substring(8, 2), CultureInfo.InvariantCulture)));

            TimeSpan timeOfDay = now.TimeOfDay;
            timeOfDay = new TimeSpan(timeOfDay.Hours, timeOfDay.Minutes, timeOfDay.Seconds);
            Assert.AreEqual(timeOfDay,
                new TimeSpan(int.Parse(rfcDate.Substring(11, 2), CultureInfo.InvariantCulture),
                int.Parse(rfcDate.Substring(14, 2), CultureInfo.InvariantCulture),
                int.Parse(rfcDate.Substring(17, 2), CultureInfo.InvariantCulture)));
        }

        [TestMethod]
        public void FromRfc822FormatTest()
        {
            DateTime now = DateTime.Now;
            now = new DateTime(now.Ticks - now.Ticks % (10000 * 1000)); //cut off milliseconds and ticks
            string rfcFormat = now.ToRfc822Format();

            DateTime result = DateTimeExtension.FromRfc822Format(rfcFormat);
            Assert.AreEqual(now, result);
        }
    }
}
