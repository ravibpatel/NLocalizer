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
    /// Sorted dictionary of translation locales. Name of locale = name of language
    /// </summary>
    /// <example>
    /// pl_PL = Polski
    /// en_US = English
    /// en_UK = English
    /// </example>
    public class TranslationLocales : SortedDictionary<string, string>
    {
        /// <inheritdoc />
        public TranslationLocales() : this(StringComparer.InvariantCulture)
        {
        }

        private TranslationLocales(IComparer<string> comparer) : base(comparer)
        {
        }

        /// <summary>
        /// Get list of locales names.
        /// </summary>
        /// <returns></returns>
        public List<string> GetAllLocales()
        {
            var list = new List<string>();
            foreach (KeyValuePair<string, string> item in this)
                if (list.Contains(item.Key) == false)
                    list.Add(item.Key);
            return list;
        }

        /// <summary>
        /// Get list of locales names for language.
        /// </summary>
        /// <param name="languageName">Name of language</param>
        /// <returns></returns>
        public List<string> GetLocales(string languageName)
        {
            var list = new List<string>();
            foreach (KeyValuePair<string, string> item in this)
                if (list.Contains(item.Key) == false &&
                    String.Compare(item.Value, languageName, StringComparison.OrdinalIgnoreCase) == 0)
                    list.Add(item.Key);
            return list;
        }
    }
}
