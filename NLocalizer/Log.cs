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
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace NLocalizer
{
    /// <summary>
    /// Helper which write information about process into log.
    /// </summary>
    public static class Log
    {

        /// <summary>
        /// Append row in log file.
        /// </summary>
        /// <param name="translation">The translation</param>
        /// <param name="message"></param>
        public static void Append(Translation translation, string message)
        {
            if (translation.LogFileName != "")
                File.AppendAllText(translation.LogFileName, message);
        }

        /// <summary>
        /// Append title and value into log file.
        /// </summary>
        /// <param name="translation">the translation.</param>
        /// <param name="title">the title.</param>
        /// <param name="value">value.</param>
        /// <param name="ifEmpty">message text if value empty, default: none.</param>
        public static void Append(Translation translation, string title, string value, string ifEmpty = "not defined")
        {
            Append(translation, $"{title}: {value}\r\n");
        }

        /// <summary>
        /// Append title and list of values into log file.
        /// </summary>
        /// <param name="translation">the translation.</param>
        /// <param name="title">title.</param>
        /// <param name="values">list of values.</param>
        /// <param name="ifEmpty">message text if list empty, default: none.</param>
        /// <param name="oneLine">display in one line</param>
        public static void Append(Translation translation, string title, List<string> values, string ifEmpty = "none", bool oneLine = false)
        {
            if (values.Count == 0)
                Append(translation, title, ifEmpty);
            else if (oneLine)
            {
                string items = "";
                foreach (string item in values)
                    items += item + ", ";
                if (items.Length > 2)
                    items = items.Substring(0, items.Length - 2);
                Append(translation, title, items, ifEmpty);
            }
            else
                for (int index = 0; index < values.Count; index++)
                    Append(translation, $"{title} {index + 1} of {values.Count}", 
                        values[index]);
        }

        /// <summary>
        /// Append initial information about the translation into log file.
        /// </summary>
        /// <param name="translation">the translation.</param>
        public static void Init(Translation translation)
        {
            Append(translation, "\r\n\r\n\r\n=== Start translation ===\r\n");
            Append(translation, "Date and time", DateTime.Now.ToString());

            Append(translation, "NLocalizer framework", Assembly.GetExecutingAssembly().ImageRuntimeVersion);
            Append(translation, "NLocalizer full name", Assembly.GetExecutingAssembly().FullName);
            Append(translation, "NLocalizer file", Assembly.GetExecutingAssembly().CodeBase);
            Append(translation, "NLocalizer version", Assembly.GetExecutingAssembly().GetName().Version.ToString());

            Append(translation, "Program framework", Assembly.GetEntryAssembly().ImageRuntimeVersion);
            Append(translation, "Program full name", Assembly.GetEntryAssembly().FullName);
            Append(translation, "Program file", Assembly.GetEntryAssembly().CodeBase);
            Append(translation, "Program version", Assembly.GetEntryAssembly().GetName().Version.ToString());

            Append(translation, "Translation framework", translation.FrameworkVersion);
            Append(translation, "Translation code template", RuntimeCompiler.CodeTemplatesFileName);
            Append(translation, "Translation debug", translation.DebugFileName);
            Append(translation, "Translation log", translation.LogFileName);

            Append(translation, "Translation dll", translation.CodeDlls);
            Append(translation, "Translation using", translation.CodeUsings);
            Append(translation, "Translation static", translation.StaticClasses);
            Append(translation, "Translation macro", translation.GetMacros());

            foreach (KeyValuePair<string, TranslationClasses> lang in translation)
            {
                Append(translation, "Translation " + lang.Key, 
                    translation.Locales.GetLocales(lang.Key), "not defined", true);
            }
            Append(translation, "Translation english language name", translation.GetEnglishName());
            //Append(translation, "Translation locales", translation.Locales.GetAllLocales());
        }
    }
}
