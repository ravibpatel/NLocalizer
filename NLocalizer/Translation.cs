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
using System.Text;
using System.Threading;
using System.CodeDom.Compiler;
using System.Globalization;
using System.Reflection;

namespace NLocalizer
{
    /// <summary>
    /// Main translation object with all translation strings of application.
    /// </summary>
    public class Translation : SortedDictionary<string, TranslationClasses>
    {
        private string _currentLanguage = "";
        /// <summary>
        /// Gets or sets the current language of application.
        /// </summary>
        /// <value>The current language.</value>
        public string CurrentLanguage
        {
            get
            {
                if (_currentLanguage != "" && Exists(_currentLanguage))
                    return _currentLanguage;
                if (locales.ContainsKey(Thread.CurrentThread.CurrentUICulture.Name) &&
                    Exists(locales[Thread.CurrentThread.CurrentUICulture.Name]))
                    return locales[Thread.CurrentThread.CurrentUICulture.Name];
                return "";
            }
            set
            {
                _currentLanguage = value;
            }
        }

        /// <summary>
        /// Name of log file. If exists then result .log will be written.
        /// </summary>
        public string LogFileName { get; set; } = "";

        /// <summary>
        /// Name of file to debug. If exists then result .cs will be written.
        /// </summary>
        public string DebugFileName { get; set; } = "";

        /// <summary>
        /// Gets or sets the current framework version to compile runtime translation.
        /// </summary>
        public string FrameworkVersion { get; set; } = "";

        private TranslationLocales locales = new TranslationLocales();
        /// <summary>
        /// Gets the locales of translations: LocaleName = LanguageName.
        /// </summary>
        /// <value>The locales.</value>
        public TranslationLocales Locales => locales;

        private List<string> codeUsings = new List<string>();
        /// <summary>
        /// Gets the code usings of application to translate.
        /// </summary>
        /// <value>The code usings.</value>
        public List<string> CodeUsings => codeUsings;

        private List<string> codeDlls = new List<string>();
        /// <summary>
        /// Gets the dll and exe files with code to translate.
        /// </summary>
        /// <value>The code modules.</value>
        public List<string> CodeDlls => codeDlls;

        private List<string> staticClasses = new List<string>();
        /// <summary>
        /// Gets the static classes of application to translate.
        /// </summary>
        /// <value>The static classes.</value>
        public List<string> StaticClasses => staticClasses;

        /// <summary>
        /// Get two digits locale.
        /// </summary>
        /// <param name="language"></param>
        public CultureInfo FindCultureInfo(string language)
        {

            language = language.Trim();
            foreach (KeyValuePair<string, string> item in locales)
                if (String.Compare(item.Key, language, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    language = item.Key;
                    break;
                }
            foreach (CultureInfo info in CultureInfo.GetCultures(CultureTypes.AllCultures))
                {
                    if (String.Compare(info.TwoLetterISOLanguageName, language, StringComparison.OrdinalIgnoreCase) == 0 ||
                        String.Compare(info.EnglishName, language, StringComparison.OrdinalIgnoreCase) == 0 ||
                        String.Compare(info.Name, language, StringComparison.OrdinalIgnoreCase) == 0 ||
                        String.Compare(info.NativeName, language, StringComparison.OrdinalIgnoreCase) == 0 ||
                        String.Compare(info.DisplayName, language, StringComparison.OrdinalIgnoreCase) == 0 ||
                        String.Compare(info.ThreeLetterISOLanguageName, language, StringComparison.OrdinalIgnoreCase) == 0 ||
                        String.Compare(info.ThreeLetterWindowsLanguageName, language, StringComparison.OrdinalIgnoreCase) == 0)
                        return info;
                }
            return null;
        }

        /// <summary>
        /// Add new translation language.
        /// </summary>
        /// <param name="key">Name of language to add.</param>
        /// <param name="autoTranslate">Auto translate by GoogleTranslator?</param>
        /// <param name="fromLanguage">Name of language from auto translate.</param>
        /// <param name="progress">Delegate execute by each translation item.</param>
        public void Add(string key, bool autoTranslate, string fromLanguage = "", ProgressDelegate progress = null)
        {
            Add(key, new TranslationClasses());
            if (fromLanguage.Trim() == "")
                fromLanguage = GetEnglishName();

            if (Exists(fromLanguage))
            {
                int maxValue = 0;
                foreach (KeyValuePair<string, TranslationProperties> classItem in this[fromLanguage])
                    maxValue += classItem.Value.Count;
                int value = 0;
                foreach (KeyValuePair<string, TranslationProperties> classItem in this[fromLanguage])
                    foreach (KeyValuePair<string, TranslationProperty> propertyItem in this[fromLanguage][classItem.Key])
                    {
                        string message = "";
                        if (autoTranslate && Exists(fromLanguage, classItem.Key, propertyItem.Key))
                            try
                            {
                                message = GoogleTranslator.Translate(propertyItem.Value.Message,
                                    FindCultureInfo(fromLanguage).TwoLetterISOLanguageName,
                                    FindCultureInfo(key).TwoLetterISOLanguageName);
                            }
                            catch
                            {
                                message = "";
                            }
                        SetProperty(key, classItem.Key, propertyItem.Key, message, propertyItem.Value.IsStatic);
                        progress?.Invoke(value, maxValue);
                        value++;
                    }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Translation"/> class and Init() this default values.
        /// </summary>
        public Translation() : this(StringComparer.InvariantCulture)
        {
            Init();
        }

        private Translation(IComparer<string> comparer) : base(comparer)
        {
        }

        /// <summary>
        /// Inits this instance default values.
        /// </summary>
        public void Init()
        {
            Clear();
            foreach (CultureInfo culture in CultureInfo.GetCultures(CultureTypes.AllCultures))
            {
                string[] name = culture.Name.Split('-');
                if (name.Length == 1 && name[0] != "")
                    locales[culture.Name] = culture.EnglishName;
            }

            foreach (CultureInfo culture in CultureInfo.GetCultures(CultureTypes.AllCultures))
            {
                string[] name = culture.Name.Split('-');
                if (name.Length > 1 && locales.ContainsKey(name[0]))
                    locales[culture.Name] = locales[name[0]];
            }
        }

        /// <summary>
        /// Gets the name of the english translation (locale en, en-US or en-GB).
        /// </summary>
        /// <returns>name of the english translation</returns>
        public string GetEnglishName()
        {
            if (locales.ContainsKey("en"))
                return locales["en"];
            if (locales.ContainsKey("en-US"))
                return locales["en-US"];
            if (locales.ContainsKey("en-GB"))
                return locales["en-GB"];
            throw new Exception("NLocalizer: Please create translation with locale en, en-US or en-GB");
        }

        /// <summary>
        /// Gets the english classes.
        /// </summary>
        /// <returns></returns>
        public TranslationClasses GetEnglishClasses()
        {
            if (ContainsKey(GetEnglishName()) == false)
                throw new Exception("NLocalizer: Please create translation with locale en, en-US or en-GB");

            return this[GetEnglishName()];
        }

        /// <summary>
        /// Clears all translation.
        /// </summary>
        public void ClearAll()
        {
            Clear();
            codeDlls.Clear();
            codeUsings.Clear();
            staticClasses.Clear();
            locales.Clear();
            _currentLanguage = "";
        }

        /// <summary>
        /// Find language in translation by language or locale. 
        /// </summary>
        /// <param name="language">Language to find.</param>
        /// <returns></returns>
        public string FindLanguage(string language)
        {
            if (ContainsKey(language.Trim()))
                return language.Trim();
            if (locales.ContainsKey(language) && ContainsKey(locales[language]))
                return locales[language];
            return language;
        }

        /// <summary>
        /// Check if exists the specified language.
        /// </summary>
        /// <param name="language">The language.</param>
        /// <returns></returns>
        public bool Exists(string language)
        {
            return ContainsKey(language) || (locales.ContainsKey(language) && ContainsKey(locales[language]));
        }

        /// <summary>
        /// Check if exists the specified class in language.
        /// </summary>
        /// <param name="language">The language.</param>
        /// <param name="className">Name of the class.</param>
        /// <returns></returns>
        public bool Exists(string language, string className)
        {
            return Exists(language) && this[FindLanguage(language)].ContainsKey(className);
        }

        /// <summary>
        /// Check if exists the specified property of class and language.
        /// </summary>
        /// <param name="language">The language.</param>
        /// <param name="className">Name of the class.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        public bool Exists(string language, string className, string propertyName)
        {
            return Exists(language, className) && this[FindLanguage(language)][className].ContainsKey(propertyName);
        }

        /// <summary>
        /// Sets the translation text of class and property in specified language.
        /// </summary>
        /// <param name="language">The language.</param>
        /// <param name="className">Name of the class.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="message">The value text.</param>
        /// <param name="isStatic">is property static?</param>
        public void SetProperty(string language,
            string className, string propertyName, string message, bool isStatic)
        {
            language = FindLanguage(language);
            if (ContainsKey(language) == false)
                Add(language, new TranslationClasses());
            if (this[language].ContainsKey(className) == false)
                this[language].Add(className, new TranslationProperties());
            this[language][className][propertyName] = new TranslationProperty(message, isStatic);
        }

        /// <summary>
        /// Gets the translation property of class and property in current language.
        /// </summary>
        /// <param name="className">Name of the class.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        public TranslationProperty GetProperty(string className, string propertyName)
        {
            return GetProperty(CurrentLanguage, className, propertyName);
        }

        /// <summary>
        /// Gets the translation text of class and property in specified language.
        /// </summary>
        /// <param name="language">The language.</param>
        /// <param name="className">Name of the class.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        public TranslationProperty GetProperty(string language, string className, string propertyName)
        {
            try
            {
                if (Exists(language, className, propertyName))
                    return this[FindLanguage(language)][className][propertyName];
                if (Exists(GetEnglishName(), className, propertyName))
                    return this[GetEnglishName()][className][propertyName];
                if (Exists("Neutral", className, propertyName))
                    return this["Neutral"][className][propertyName];
                return new TranslationProperty("", false);
            }
            catch
            {
                return new TranslationProperty("", false);
            }
        }

        /// <summary>
        /// Gets the translation text of class and property in specified language.
        /// </summary>
        /// <param name="language">The language.</param>
        /// <param name="className">Name of the class.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="macroDelegate">Delegate executed to each string macro.</param>
        /// <returns></returns>
        public TranslationProperty GetProperty(string language, string className, string propertyName, MacroDelegate macroDelegate)
        {
            TranslationProperty property = GetProperty(language, className, propertyName);
            property.Message = ReplaceMacros(property.Message, macroDelegate);
            return property;
        }

        /// <summary>
        /// Replace all macros in message.
        /// </summary>
        /// <param name="message">Message to replace.</param>
        /// <param name="macroDelegate">Delegate executed to each string macro.</param>
        /// <returns></returns>
        public string ReplaceMacros(string message, MacroDelegate macroDelegate)
        {
            return Macro.Replace(message, GetMacros(), macroDelegate);
        }

        /// <summary>
        /// Get list of macros from all translation.
        /// </summary>
        /// <returns></returns>
        public List<string> GetMacros()
        {
            var macros = new List<string>();
            foreach (KeyValuePair<string, TranslationClasses> classes in this)
                foreach (KeyValuePair<string, TranslationProperties> properties in this[classes.Key])
                    foreach (KeyValuePair<string, TranslationProperty> property in this[classes.Key][properties.Key])
                        foreach (string macro in Macro.GetMacros(this[classes.Key][properties.Key][property.Key].Message))
                            if (macros.Contains(macro) == false)
                                macros.Add(macro);
            return macros;
        }

        /// <summary>
        /// Writes the all translation into files in specified directory name.
        /// </summary>
        /// <param name="directoryName">Name of the directory.</param>
        public void Write(string directoryName)
        {
            LangWriter.Write(directoryName, this);
        }

        /// <summary>
        /// Writes all this translation into files in application directory.
        /// </summary>
        public void Write()
        {
            LangWriter.Write(this);
        }

        /// <summary>
        /// Reads all translation from files in application directory.
        /// </summary>
        public void Read()
        {
            LangReader.Read(this);
        }

        /// <summary>
        /// Reads all translation from files in specified directory.
        /// </summary>
        /// <param name="directoryName">Name of the directory.</param>
        public void Read(string directoryName)
        {
            LangReader.Read(directoryName, this);
        }

        /// <summary>
        /// Reads the specified language from lines.
        /// </summary>
        /// <param name="language">The language.</param>
        /// <param name="lines">The lines.</param>
        public void Read(string language, string[] lines)
        {
            LangReader.Read(language, lines, this);
        }

        /// <summary>
        /// Generates the translation code in C# into StringBuilder.
        /// </summary>
        /// <returns></returns>
        public StringBuilder GenerateCode()
        {
            return GenerateCode(Assembly.GetEntryAssembly());
        }

        /// <summary>
        /// Generates the translation code in C# into StringBuilder.
        /// </summary>
        /// <returns></returns>
        public StringBuilder GenerateCode(Assembly assembly)
        {
            return RuntimeCompiler.ToCSharpCode(this, RuntimeCompiler.GetCompileUsings(this, assembly), GetMacros());
        }

        /// <summary>
        /// Compiles the translation.
        /// </summary>
        /// <returns></returns>
        public CompilerResults Compile()
        {
            return RuntimeCompiler.Compile(this);
        }

        /// <summary>
        /// Compiles the translation.
        /// </summary>
        /// <param name="directoryName">Name of the directory.</param>
        /// <returns></returns>
        public CompilerResults Compile(string directoryName)
        {
            return RuntimeCompiler.Compile(this, directoryName);
        }

        /// <summary>
        /// Gets all classes.
        /// </summary>
        /// <returns></returns>
        public List<string> GetAllClasses()
        {
            var list = new List<string>();
            foreach(KeyValuePair<string, TranslationClasses> classes in this)
                foreach (KeyValuePair<string, TranslationProperties> item in this[classes.Key])
                    if (list.Contains(item.Key) == false)
                        list.Add(item.Key);
            return list;
        }
    }
}
