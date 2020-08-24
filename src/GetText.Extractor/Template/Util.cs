using System;
using System.Globalization;

namespace GetText.Extractor.Template
{
    public static class DateTimeExtension
    {
        public static string ToRfc822Format(this DateTime dateTime) => dateTime.ToString("yyyy-MM-dd HH':'mm':'sszz00", CultureInfo.InvariantCulture); //rfc822 format

        public static DateTime FromRfc822Format(string dateTime)
        {
            if (DateTime.TryParse(dateTime, out DateTime result))
            {
                return result;
            }
            return DateTime.MinValue;
        }
    }
}
