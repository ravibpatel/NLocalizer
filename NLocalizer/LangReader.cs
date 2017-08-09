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
using System.Windows.Forms;

namespace NLocalizer
{
    /// <summary>
    /// Helper which read language files from application directory
    /// </summary>
    public static class LangReader
    {
        /// <summary>
        /// Reads language files from application directory into the specified translation.
        /// </summary>
        /// <param name="translation">The translation.</param>
        public static void Read(Translation translation)
        {
            Read(Path.GetDirectoryName(Application.ExecutablePath), translation);
        }

        /// <summary>
        /// Reads language files from specified directory into specified translation.
        /// </summary>
        /// <param name="directoryName">Name of the directory.</param>
        /// <param name="translation">The translation.</param>
        public static void Read(string directoryName, Translation translation)
        {
            try
            {
                foreach (string fileName in GetFiles(directoryName, "*.lang"))
                    Read(Path.GetFileNameWithoutExtension(fileName),
                        File.ReadAllLines(fileName), translation);
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        /// <summary>
        /// Get all the files with searchPattern from the directory.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="searchPattern"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static List<string> GetFiles(string path, string searchPattern)
        {
            if (path == null)
                throw new ArgumentNullException("path");
            if (searchPattern == null)
                throw new ArgumentNullException("searchPattern");
            List<string> files = new List<string>();
            foreach (var file in Directory.GetFiles(path, searchPattern))
            {
                files.Add(file);
            }
            foreach (var directory in Directory.GetDirectories(path, "Languages"))
            {
                try
                {
                    files.AddRange(GetFiles(directory, searchPattern));
                }
                catch (UnauthorizedAccessException)
                {
                }
            }
            return files;
        }

        /// <summary>
        /// Reads language from lines into the specified translation.
        /// </summary>
        /// <param name="language">The language.</param>
        /// <param name="lines">The lines.</param>
        /// <param name="translation">The translation.</param>
        public static void Read(string language, string[] lines, Translation translation)
        {
            for (int pos = 0; pos < lines.Length; pos++)
            {
                string line = lines[pos].Trim();
                if (line.ToLower().StartsWith("debug"))
                {
                    translation.DebugFileName = line.Length > "debug".Length ? line.Substring("debug ".Length).Trim() : "NLocalizer.cs";
                }
                else if (line.ToLower().StartsWith("log"))
                {
                    translation.LogFileName = line.Length > "log".Length ? line.Substring("log ".Length).Trim() : "NLocalizer.log";
                }
                else if (line.ToLower().StartsWith("template"))
                {
                    string fileName;
                    fileName = line.Length > "template".Length ? line.Substring("template ".Length).Trim() : "NLocalizer.tpl";
                    if (File.Exists(fileName))
                        RuntimeCompiler.CodeTemplatesFileName = fileName;
                }
                else if (line.ToLower().StartsWith("framework "))
                {
                    translation.FrameworkVersion = line.Substring("framework ".Length).Trim();
                }
                else if (line.ToLower().StartsWith("locale "))
                {
                    line = line.Substring("locale ".Length).Trim();
                    string[] items = line.Split(',');
                    foreach (string item in items)
                        if (item.Trim() != "")
                            translation.Locales[item.Trim()] = language;
                }
                else if (line.ToLower().StartsWith("module "))
                {
                    line = line.Substring("module ".Length).Trim();
                    string[] items = line.Split(',');
                    foreach (string item in items)
                        if (item.Trim() != "" && translation.CodeDlls.Contains(item.Trim()) == false)
                            translation.CodeDlls.Add(item.Trim());
                }
                else if (line.ToLower().StartsWith("dll "))
                {
                    line = line.Substring("dll ".Length).Trim();
                    string[] items = line.Split(',');
                    foreach (string item in items)
                        if (item.Trim() != "" && translation.CodeDlls.Contains(item.Trim()) == false)
                            translation.CodeDlls.Add(item.Trim());
                }
                else if (line.ToLower().StartsWith("using "))
                {
                    line = line.Substring("using ".Length).Trim();
                    string[] items = line.Split(',');
                    foreach (string item in items)
                        if (item.Trim() != "" && translation.CodeUsings.Contains(item.Trim()) == false)
                            translation.CodeUsings.Add(item.Trim());
                }
                else if (line.ToLower().StartsWith("imports "))
                {
                    line = line.Substring("imports ".Length).Trim();
                    string[] items = line.Split(',');
                    foreach (string item in items)
                        if (item.Trim() != "" && translation.CodeUsings.Contains(item.Trim()) == false)
                            translation.CodeUsings.Add(item.Trim());
                }
                else if (line.ToLower().StartsWith("static "))
                {
                    line = line.Substring("static ".Length).Trim();
                    string[] items = line.Split(',');
                    foreach (string item in items)
                        if (item.Trim() != "" && translation.StaticClasses.Contains(item.Trim()) == false)
                            translation.StaticClasses.Add(item.Trim());
                }
                else if (line.Trim() != "" && line.StartsWith(";") == false)
                {
                    string className = "";
                    string propertyName = "";
                    string valueText = "";
                    bool isStatic;
                    Decode(line, ref className, ref propertyName, ref valueText, out isStatic);
                    translation.SetProperty(language, className, propertyName, valueText, isStatic);
                }
            }
        }

        /// <summary>
        /// Replace special chars \r \n \t \\ \"
        /// </summary>
        /// <param name="message">input text</param>
        /// <returns></returns>
        public static string ReplaceSpecialChars(string message)
        {
            List<string> macros = Macro.GetMacros(message);
            message = Macro.NamesToNumbers(message, macros);
            message = message.Replace(@"\r", "\r").
                Replace(@"\n", "\n").
                Replace(@"\t", "\t").
                Replace("\\\"", "\"").
                Replace(@"\\", "\\");
            message = Macro.NumbersToNames(message, macros);
            return message;
        }

        /// <summary>
        /// Decode the specified line of language.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="className">Name of the class.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="valueText">The value text.</param>
        /// <param name="isStatic">Is the property static?</param>
        public static void Decode(string line,
            ref string className, ref string propertyName, ref string valueText, out bool isStatic)
        {
            isStatic = line.StartsWith("!");
            if (isStatic)
                line = line.Substring(1).Trim();

            if (line.StartsWith("(") == false)
                throw new Exception(String.Format("Expected ( in line: {0}", line));

            line = line.Substring(1).Trim();
            if (line.IndexOf(")", StringComparison.Ordinal) < 0)
                throw new Exception(String.Format("Expected ) in line: {0}", line));
            className = line.Substring(0, line.IndexOf(")", StringComparison.Ordinal)).Trim();

            line = line.Substring(line.IndexOf(")", StringComparison.Ordinal) + 1).Trim();
            if (line.IndexOf("=", StringComparison.Ordinal) < 0)
                throw new Exception(String.Format("Expected = in line: {0}", line));
            propertyName = line.Substring(0, line.IndexOf("=", StringComparison.Ordinal)).Trim();
            line = line.Substring(line.IndexOf("=", StringComparison.Ordinal) + 1).Trim();
            valueText = ReplaceSpecialChars(line);
        }
    }
}
