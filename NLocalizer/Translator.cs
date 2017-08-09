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
using System.IO;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows.Forms;

namespace NLocalizer
{
    /// <summary>
    /// Main static class of application translation
    /// </summary>
    public static class Translator
    {
        // Language / (ClassName)ControlName.PropertyName = ValueText
        private static Translation translation = new Translation();
        /// <summary>
        /// Gets the translation of current application.
        /// </summary>
        /// <value>The translation.</value>
        public static Translation Translation => translation;
        
        /// <summary>
        /// Reports errors in your language files if set to true.
        /// </summary>
        public static bool ReportErrors { get; set; } = false;

        /// <summary>
        /// Name of directory with translation to read.
        /// </summary>
        public static string TranslationDirectory => GetTranslationDirectory(Application.ExecutablePath);

        /// <summary>
        /// Calculate translaction directory.
        /// </summary>
        /// <param name="exeFileName">Name of exe file name.</param>
        /// <returns></returns>
        public static string GetTranslationDirectory(string exeFileName)
        {
            try
            {
                var parentFolder = Path.GetDirectoryName(exeFileName);
                if (parentFolder != null)
                {
                    foreach (string fileName in LangReader.GetFiles(parentFolder, "*.lang"))
                        return Path.GetDirectoryName(fileName);
                }

                return Path.GetDirectoryName(exeFileName);
            }
            catch
            {
                return Path.GetDirectoryName(exeFileName);
            }
        }

        private static CompilerResults compilerResults;
        /// <summary>
        /// Gets the translation compile results.
        /// </summary>
        /// <value>The compiler results.</value>
        public static CompilerResults CompilerResults => compilerResults;

        /// <summary>
        /// Compiles if necessary.
        /// </summary>
        public static void CompileIfNecessary()
        {
            if (translation.Count == 0)
                translation.Read(TranslationDirectory);

            if (translation.Count == 0)
            {
                if(ReportErrors)
                    throw new Exception("NLocalizer: Translation is empty. Please enter any .lang file in Your program directory or any subdirectory.");
                return;
            }

            if (compilerResults == null || compilerResults.CompiledAssembly == null)
                compilerResults = RuntimeCompiler.Compile(translation);
        }

        /// <summary>
        /// Gets the runtime compiled class (compile if necessary).
        /// </summary>
        /// <returns></returns>
        public static Type GetRuntimeClass()
        {
            CompileIfNecessary();
            if (compilerResults != null)
            {
                return RuntimeCompiler.GetCompiledClass(compilerResults);
            }
            return null;
        }

        /// <summary>
        /// Translates the specified obj into language.
        /// </summary>
        /// <param name="language">The language.</param>
        /// <param name="obj">The obj.</param>
        public static void Translate(string language, object obj)
        {
            RuntimeCompiler.Translate(GetRuntimeClass(), language, obj, translation);
        }

        /// <summary>
        /// Translates the specified obj into current language.
        /// </summary>
        /// <param name="obj">The obj.</param>
        public static void Translate(object obj)
        {
            Translate();
            RuntimeCompiler.Translate(GetRuntimeClass(), obj, translation);
        }

        /// <summary>
        /// Translates all static classes into current language.
        /// </summary>
        public static void Translate()
        {
            RuntimeCompiler.Translate(GetRuntimeClass(), translation);
        }

        /// <summary>
        /// Translates all static classes into specified language.
        /// </summary>
        /// <param name="language">The language.</param>
        public static void Translate(string language)
        {
            RuntimeCompiler.Translate(GetRuntimeClass(), language, translation);
        }

        /// <summary>
        /// Restores all static classes into neutral language.
        /// </summary>
        public static void Restore()
        {
            RuntimeCompiler.Restore(GetRuntimeClass(), translation);
        }

        /// <summary>
        /// Restores the specified obj into neutral language.
        /// </summary>
        /// <param name="obj">The obj.</param>
        public static void Restore(object obj)
        {
            RuntimeCompiler.Restore(GetRuntimeClass(), obj, translation);
        }

        /// <summary>
        /// Shows the className as form.
        /// </summary>
        /// <param name="className">Name of the class.</param>
        public static void ShowDialog(string className)
        {
            RuntimeCompiler.ShowDialog(GetRuntimeClass(), className);
        }

        /// <summary>
        /// Creates the specified class name.
        /// </summary>
        /// <param name="className">Name of the class.</param>
        /// <returns></returns>
        public static object Create(string className)
        {
            return RuntimeCompiler.Create(GetRuntimeClass(), className);
        }
    }
}