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

namespace NLocalizer
{

    /// <summary>
    /// Text finder static class.
    /// </summary>
    public static class Finder
    {
        /// <summary>
        /// Find searchText in text from begin pos.
        /// </summary>
        /// <param name="text">Searched text.</param>
        /// <param name="searchText">Text to find.</param>
        /// <param name="begin">Start cursor positon.</param>
        /// <param name="forward">Forward search (true) or reverse (false).</param>
        /// <returns></returns>
        public static int IndexOf(string text, string searchText, int begin, bool forward)
        {
            if (forward)
                return text.IndexOf(searchText, begin, StringComparison.Ordinal);
            while (begin >= 0)
            {
                if (text.Length >= begin + searchText.Length &&
                    searchText == text.Substring(begin, searchText.Length))
                    return begin;
                begin--;
            }
            return -1;
        }

        /// <summary>
        /// Remove all string from text beetween before and after.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="before"></param>
        /// <param name="after"></param>
        /// <returns></returns>
        public static string RemoveBeetweenWithout(string text, string before, string after)
        {
            int begin = 0;
            if (text != "")
            {
                if (before != "")
                {
                    begin = text.IndexOf(before, begin, StringComparison.Ordinal);
                    if (begin >= 0)
                        begin = begin + before.Length;
                    else
                        begin = text.Length;
                }
            }

            int end = text.Length;
            if (after != "")
            {
                end = text.IndexOf(after, Math.Min(begin + 1, text.Length - 1), StringComparison.Ordinal);
                if (end < 0)
                    end = text.Length;
            }
            string result = text.Remove(begin, end - begin);
            return result;
        }

        /// <summary>
        /// Remove all string from text beetween before and after.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="before"></param>
        /// <param name="after"></param>
        /// <returns></returns>
        public static string RemoveBeetweenWithAll(string text, string before, string after)
        {
            string result = text;
            int len;
            do
            {
                len = result.Length;
                result = RemoveBeetweenWith(result, before, after);
            }
            while (len > result.Length);
            return result;
        }

        /// <summary>
        /// Remove all string from text beetween before and after.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="before"></param>
        /// <param name="after"></param>
        /// <returns></returns>
        public static string RemoveBeetweenWith(string text, string before, string after)
        {
            int begin = 0;
            if (text != "")
            {
                if (before != "")
                {
                    begin = text.IndexOf(before, begin, StringComparison.Ordinal);
                    if (begin < 0)
                        begin = text.Length;
                }
            }

            int end = text.Length;
            if (after != "")
            {
                end = text.IndexOf(after, Math.Min(begin + 1, text.Length - 1), StringComparison.Ordinal);
                if (end < 0)
                    end = text.Length;
            }
            if (begin != text.Length || end != text.Length)
                return text.Remove(begin, end - begin + after.Length);
            return text;
        }

        /// <summary>
        /// Find string beetween before and after in text.
        /// </summary>
        /// <param name="text">Searched text.</param>
        /// <param name="before"></param>
        /// <param name="after"></param>
        /// <param name="forward">Forward search (true) or reverse (false).</param>
        /// <returns></returns>
        public static string FindBeetween(string text, string before, string after, bool forward)
        {
            int begin = 0;
            return FindBeetween(text, before, after, ref begin, forward);
        }

        /// <summary>
        /// Find string beetween before and after in text.
        /// </summary>
        /// <param name="text">Searched text.</param>
        /// <param name="before"></param>
        /// <param name="after"></param>
        /// <returns></returns>
        public static string FindBeetween(string text, string before, string after)
        {
            int begin = 0;
            return FindBeetween(text, before, after, ref begin, true);
        }

        /// <summary>
        /// Find string beetween before and after in text.
        /// </summary>
        /// <param name="text">Searched text.</param>
        /// <param name="before"></param>
        /// <param name="after"></param>
        /// <param name="begin">Start cursor positon.</param>
        /// <returns></returns>
        public static string FindBeetween(string text, string before, string after, ref int begin)
        {
            return FindBeetween(text, before, after, ref begin, true);
        }

        /// <summary>
        /// Find string beetween before and after in text.
        /// </summary>
        /// <param name="text">Searched text.</param>
        /// <param name="before"></param>
        /// <param name="after"></param>
        /// <param name="begin">Start cursor positon.</param>
        /// <param name="forward">Forward search (true) or reverse (false).</param>
        /// <returns></returns>
        public static string FindBeetween(string text, string before, string after, ref int begin, bool forward)
        {
            if (text != "")
            {
                if (before != "")
                {
                    begin = IndexOf(text, before, Math.Min(begin, text.Length - 1), forward);
                    if (begin >= 0)
                        begin = begin + before.Length;
                    else
                        begin = text.Length;
                }
            }

            int end = text.Length;
            if (after != "")
            {
                end = IndexOf(text, after, Math.Min(begin, text.Length - 1), forward);
                if (end < 0)
                    end = text.Length;
            }
            string result = text.Substring(begin, end - begin);
            begin += result.Length;
            return result;
        }

        /// <summary>
        /// Find string beetween before and after in text.
        /// </summary>
        /// <param name="text">Searched text.</param>
        /// <param name="before"></param>
        /// <param name="after"></param>
        /// <returns></returns>
        public static string FindBeetween(string text, string[] before, string after)
        {
            int begin = 0;
            return FindBeetween(text, before, after, ref begin, true);
        }

        /// <summary>
        /// Find string beetween before and after in text.
        /// </summary>
        /// <param name="text">Searched text.</param>
        /// <param name="before"></param>
        /// <param name="after"></param>
        /// <param name="forward">Forward search (true) or reverse (false).</param>
        /// <returns></returns>
        public static string FindBeetween(string text, string[] before, string after, bool forward)
        {
            int begin = 0;
            return FindBeetween(text, before, after, ref begin, forward);
        }

        /// <summary>
        /// Find string beetween before and after in text.
        /// </summary>
        /// <param name="text">Searched text.</param>
        /// <param name="before"></param>
        /// <param name="after"></param>
        /// <param name="begin">Start cursor positon.</param>
        /// <param name="forward">Forward search (true) or reverse (false).</param>
        /// <returns></returns>
        public static string FindBeetween(string text, string[] before, string after, ref int begin, bool forward)
        {
            int minPos = text.Length;
            string result = "";
            for (int i = 0; i < before.Length; i++)
            {
                int pos = begin;
                string tempResult = FindBeetween(text, before[i], after, ref pos, forward);
                if (pos < minPos)
                {
                    result = tempResult;
                    minPos = pos;
                }
            }
            begin = minPos;
            return result;
        }
    }
}
