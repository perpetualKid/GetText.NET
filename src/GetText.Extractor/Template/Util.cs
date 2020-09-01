using System;
using System.Globalization;

namespace GetText.Extractor.Template
{

    /// <summary>
    /// provides extension methods to convert DateTime to/from different string formats
    /// </summary>
    public static class DateTimeExtension
    {
        /// <summary>
        /// Converts DateTime instance o RFC822 compatible format (yyyy-MM-dd HH':'mm':'sszz00)
        /// </summary>
        public static string ToRfc822Format(this DateTime dateTime) => dateTime.ToString("yyyy-MM-dd HH':'mm':'sszz00", CultureInfo.InvariantCulture); //rfc822 format

        /// <summary>
        /// Converts RFC822 date format to DateTime instance. Returns DateTime.MinValue if invalid input provided 
        /// </summary>
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
