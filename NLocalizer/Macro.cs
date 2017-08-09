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

namespace NLocalizer
{
    /// <summary>
    /// Manipulate macros.
    /// </summary>
    public static class Macro
    {
        /// <summary>
        /// Replace all macros in message.
        /// </summary>
        /// <param name="message">Message to replace.</param>
        /// <param name="macros">List of macros.</param>
        /// <param name="macroDelegate">Delegate executed to each string macro.</param>
        /// <returns></returns>
        public static string Replace(string message, List<string> macros, MacroDelegate macroDelegate)
        {
            foreach (string macro in macros)
                message = message.Replace("$(" + macro + ")", macroDelegate(macro));
            return message;
        }

        /// <summary>
        /// Replace all macros in message.
        /// </summary>
        /// <param name="message">Message to replace.</param>
        /// <param name="macroDelegate">Delegate executed to each string macro.</param>
        /// <returns></returns>
        public static string Replace(string message, MacroDelegate macroDelegate)
        {
            foreach (string macro in GetMacros(message))
                message = message.Replace("$(" + macro + ")", macroDelegate(macro));
            return message;
        }

        /// <summary>
        /// Replace all macros in message.
        /// </summary>
        /// <param name="message">Message to replace.</param>
        /// <param name="macros">List of macros and text to replace.</param>
        /// <returns></returns>
        public static string Replace(string message, Dictionary<string, string> macros)
        {
            foreach (KeyValuePair<string, string> macro in macros)
                message = message.Replace("$(" + macro.Key + ")", macro.Value);
            return message;
        }

        /// <summary>
        /// Replace macro names to numbers, for example text "$(First) $(Second)" will be replaced to "$(1) $(2)"
        /// </summary>
        /// <param name="message">Message to replace.</param>
        /// <returns></returns>
        public static string NamesToNumbers(string message)
        {
            return NamesToNumbers(message, GetMacros(message));
        }

        /// <summary>
        /// Replace macro names to numbers, for example text "$(First) $(Second)" will be replaced to "$(1) $(2)"
        /// </summary>
        /// <param name="message">Message to replace.</param>
        /// <param name="macros">List of macros.</param>
        /// <returns></returns>
        public static string NamesToNumbers(string message, List<string> macros)
        {
            for (int num = 0; num < macros.Count; num++)
                message = message.Replace("$(" + macros[num] + ")", "$(" + (num + 1) + ")");
            return message;
        }

        /// <summary>
        /// Replace macro numers to name, for example text "$(1) $(2)" will be replaced to "$(First) $(Second)".
        /// </summary>
        /// <param name="message">Message to replace.</param>
        /// <param name="macros">List of macros.</param>
        /// <returns></returns>
        public static string NumbersToNames(string message, List<string> macros)
        {
            for (int num = 0; num < macros.Count; num++)
                message = message.Replace("$(" + (num + 1) + ")", "$(" + macros[num] + ")");
            return message;
        }

        /// <summary>
        /// Get list of macros from message.
        /// </summary>
        /// <param name="message">Translation message.</param>
        /// <returns></returns>
        public static List<string> GetMacros(string message)
        {
            var macros = new List<string>();
            int pos = message.IndexOf("$(", StringComparison.Ordinal);
            while (pos >= 0 && pos < message.Length)
            {
                pos += 2;
                string macro = "";
                int brackets = 0;
                while (pos < message.Length)
                {
                    if (message.Substring(pos, 1) == "(")
                        brackets++;
                    else if (message.Substring(pos, 1) == ")")
                        brackets--;
                    if (brackets < 0)
                        break;
                    macro += message.Substring(pos, 1);
                    pos++;
                }
                if (macros.Contains(macro) == false)
                    macros.Add(macro);
                pos = message.IndexOf("$(", pos, StringComparison.Ordinal);
            }
            return macros;
        }
    }
}
