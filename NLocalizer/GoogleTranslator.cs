/*******************************************************************************

  NLocalizer (C) 2010-2012 Chris Prusik (KAP1 Ltd www.kap1.net)
  The fast, simple solution for localizing .NET applications, by text files.
  Latest version: http://NLocalizer.codeplex.com/

  $Id$

  This library is free software; you can redistribute it and/or
  modify it under the terms of the GNU Lesser General Public
  License as published by the Free Software Foundation; either
  version 2.1 of the License, or (at your option) any later version.

  This library is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
  Lesser General Public License for more details.

*******************************************************************************/
using System;
using System.Net;
using System.Text;
using System.Globalization;
using System.Xml;

namespace NLocalizer
{
    /// <summary>
    /// Automatic translation by http://translator.google.com
    /// </summary>
    public static class GoogleTranslator
    {
        /// <summary>
        /// Translates the text.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="sourceLanguage">Source language or locale.</param>
        /// <param name="destinationlanguage">Destination language or locale.</param>
        /// <returns></returns>
        public static string Translate(string input, string sourceLanguage, string destinationlanguage)
        {
            string sourcePair = "";
            string destinationPair = "";
            foreach (CultureInfo info in CultureInfo.GetCultures(CultureTypes.AllCultures))
            {
                if (String.Compare(info.Name, sourceLanguage, StringComparison.OrdinalIgnoreCase) == 0 ||
                    String.Compare(info.NativeName, sourceLanguage, StringComparison.OrdinalIgnoreCase) == 0 ||
                    String.Compare(info.EnglishName, sourceLanguage, StringComparison.OrdinalIgnoreCase) == 0 ||
                    String.Compare(info.DisplayName, sourceLanguage, StringComparison.OrdinalIgnoreCase) == 0 ||
                    String.Compare(info.TwoLetterISOLanguageName, sourceLanguage, StringComparison.OrdinalIgnoreCase) == 0)
                    sourcePair = info.TwoLetterISOLanguageName;

                if (String.Compare(info.Name, destinationlanguage, StringComparison.OrdinalIgnoreCase) == 0 ||
                    String.Compare(info.NativeName, destinationlanguage, StringComparison.OrdinalIgnoreCase) == 0 ||
                    String.Compare(info.EnglishName, destinationlanguage, StringComparison.OrdinalIgnoreCase) == 0 ||
                    String.Compare(info.DisplayName, destinationlanguage, StringComparison.OrdinalIgnoreCase) == 0 ||
                    String.Compare(info.TwoLetterISOLanguageName, destinationlanguage, StringComparison.OrdinalIgnoreCase) == 0)
                    destinationPair = info.TwoLetterISOLanguageName;
                if (sourcePair != "" && destinationPair != "")
                    break;
            }
            return Translate(input, sourcePair + "|" + destinationPair);
        }

        /// <summary>
        /// Translate Text using Google Translate API's
        /// Google URL - http://translate.google.com
        /// </summary>
        /// <param name="input">Input string</param>
        /// <param name="languagePair">2 letter Language Pair, delimited by "|".
        /// E.g. "ar|en" language pair means to translate from Arabic to English</param>
        /// <returns>Translated to String</returns>
        public static string Translate(string input, string languagePair)
        {
            string url = $"http://www.google.com/translate_t?hl=en&ie=UTF8&text={input}&langpair={languagePair}";
            var web = new WebClient();
            string html = web.DownloadString(url);
            string charset = Finder.FindBeetween(html, "charset=", "\"").Trim();
            byte[] converted = Encoding.Convert(Encoding.GetEncoding(charset), Encoding.UTF8, web.DownloadData(url));
            html = Encoding.UTF8.GetString(converted);
            string result = html.Substring(html.IndexOf("<span title=\"", StringComparison.Ordinal) + "<span title=\"".Length);
            result = result.Substring(result.IndexOf(">", StringComparison.Ordinal) + 1);
            result = result.Substring(0, result.IndexOf("</span>", StringComparison.Ordinal));
            var str = new StringBuilder();
            str.AppendLine($"<?xml version=\"1.0\" encoding=\"{charset}\" ?>");
            str.AppendLine("<html>");
            str.AppendLine("<head>");
            str.AppendLine("<meta content=\"text/html; charset=utf-8\" http-equiv=\"Content-Type\" />");
            str.AppendLine("</head>");
            str.AppendLine("<body>");
            str.AppendLine(result);
            str.AppendLine("</body>");
            str.AppendLine("</html>");
            var xml = new XmlDocument();
            xml.LoadXml(str.ToString());
            var xmlNode = xml.SelectSingleNode("/html/body");
            if (xmlNode != null) return xmlNode.InnerText.Trim();
            return input;
        }
    }
}