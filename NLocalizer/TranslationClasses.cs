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
    /// SortedDictionary of classes: className = list of properties.
    /// </summary>
    public class TranslationClasses : SortedDictionary<string, TranslationProperties>
    {
        /// <inheritdoc />
        public TranslationClasses() : this(StringComparer.InvariantCulture)
        {
        }

        private TranslationClasses(IComparer<string> comparer) : base(comparer)
        {
        }
    }
}
