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
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Reflection;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace NLocalizer
{
    /// <summary>
    /// Helper to compile translation class in runtime mode.
    /// </summary>
    public static class RuntimeCompiler
    {
        private const string MethodTranslate = "Translate";
        private const string MethodRestore = "Restore";
        private const string MethodCreate = "Create";
        private const string MethodShowDialog = "ShowDialog";

        private static Dictionary<string, string> codeTemplates = new Dictionary<string, string>();

        /// <summary>
        /// Gets the templates (C# code).
        /// </summary>
        /// <value>The templates.</value>
        public static Dictionary<string, string> CodeTemplates
        {
            get
            {
                if (codeTemplates.Count == 0)
                    InitCodeTemplates(RuntimeTemplates.Code);

                return codeTemplates;
            }
        }

        /// <summary>
        /// Gets or sets the code templates as text.
        /// </summary>
        /// <value>The code templates as text.</value>
        public static string CodeTemplatesAsText
        {
            get
            {
                if (codeTemplates.Count == 0)
                    InitCodeTemplates(RuntimeTemplates.Code);

                var str = new StringBuilder();
                foreach (KeyValuePair<string, string> item in codeTemplates)
                {
                    str.AppendLine("@" + item.Key);
                    str.AppendLine(item.Value);
                }
                return str.ToString();
            }
            set => InitCodeTemplates(value);
        }

        private static string _codeTemplatesFileName = "";

        /// <summary>
        /// Name of file with templates to generate C# code.
        /// </summary>
        public static string CodeTemplatesFileName
        {
            get => _codeTemplatesFileName;
            set
            {
                if (File.Exists(value))
                {
                    InitCodeTemplates(File.ReadAllText(value));
                    _codeTemplatesFileName = value;
                }
            }
        }

        /// <summary>
        /// Inits the code templates from text.
        /// </summary>
        /// <param name="text">The text.</param>
        public static void InitCodeTemplates(string text)
        {
            string name = "";
            foreach (string line in text.Split(new[] {"\r\n"}, StringSplitOptions.None))
            {
                if (line.Trim().StartsWith("@"))
                {
                    name = line.Trim().Substring(1).Trim();
                    codeTemplates[name] = "";
                }
                else if (name != "" && line.Trim().StartsWith(";") == false)
                {
                    if (codeTemplates.ContainsKey(name))
                        codeTemplates[name] += line + "\r\n";
                    else
                        codeTemplates[name] = line + "\r\n";
                }
            }
        }

        /// <summary>
        /// Gets the template, code C#.
        /// </summary>
        /// <param name="name">The name of template.</param>
        /// <returns></returns>
        public static string GetTemplate(string name)
        {
            if (CodeTemplates.ContainsKey(name) == false)
            {
                throw new Exception($"NLocalizer: Template '{name}' not found. Please correct .lang files.");
            }
            return CodeTemplates[name];
        }

        /// <summary>
        /// Convert list of strings to one string separated by comma.
        /// </summary>
        /// <param name="list">list of strings</param>
        /// <returns></returns>
        public static string ToString(List<string> list)
        {
            string result = "";
            foreach (string item in list)
                if (result == "")
                    result = item.Trim();
                else
                    result += ", " + item.Trim();
            return result;
        }

        /// <summary>
        /// Creates StringBuilder with C# code of translation.
        /// </summary>
        /// <param name="translation">The translation.</param>
        /// <param name="usedUsings">List of used namespaces.</param>
        /// <param name="macros">List of macros.</param>
        /// <returns></returns>
        public static StringBuilder ToCSharpCode(Translation translation, List<string> usedUsings, List<string> macros)
        {
            List<string> allClasses = translation.GetAllClasses();

            string code = GetTemplate("Code").Replace("$(Locales)", ToString(translation.Locales.GetAllLocales())).
                Replace("$(DateTime)", DateTime.Now.ToShortDateString());

            var usings = new StringBuilder();
            foreach (string usingName in usedUsings)
                usings.Append(GetTemplate("Using").Replace("${NamespaceName)", usingName));
            code = code.Replace("$(Using)", usings.ToString());

            var translate = new StringBuilder();
            foreach (string className in allClasses)
                if (translation.StaticClasses.Contains(className) ||
                    (translation.GetEnglishClasses().ContainsKey(className) &&
                     translation.GetEnglishClasses()[className].StaticPropertyExists()))
                    translate.Append(GetTemplate("TranslateClass").Replace("$(ClassName)", className));
            code = code.Replace("$(TranslateClass)", translate.ToString());

            var remember = new StringBuilder();
            foreach (string className in allClasses)
                if (translation.StaticClasses.Contains(className) ||
                    (translation.GetEnglishClasses().ContainsKey(className) &&
                     translation.GetEnglishClasses()[className].StaticPropertyExists()))
                    remember.Append(GetTemplate("RememberClass").Replace("$(ClassName)", className));
            code = code.Replace("$(RememberClass)", remember.ToString());

            var restore = new StringBuilder();
            foreach (string className in allClasses)
                if (translation.StaticClasses.Contains(className) ||
                    (translation.GetEnglishClasses().ContainsKey(className) &&
                     translation.GetEnglishClasses()[className].StaticPropertyExists()))
                    restore.Append(GetTemplate("RestoreClass").Replace("$(ClassName)", className));
            code = code.Replace("$(RestoreClass)", restore.ToString());

            var classTranslation = new StringBuilder();
            foreach (string item in allClasses)
                classTranslation.Append(ToCSharpCode(item, translation));
            code = code.Replace("$(ClassTranslation)", classTranslation.ToString());

            var macroMethod = new StringBuilder();
            foreach (string item in macros)
                macroMethod.Append(GetTemplate("Macro").
                    Replace("$(MacroName)", item.Replace("\"", "\\\"")).
                    Replace("$(MacroCode)", item));
            code = code.Replace("$(Macro)", macroMethod.ToString());

            return new StringBuilder(code);
        }

        /// <summary>
        /// Creates StringBuilder with C# code of translation.
        /// </summary>
        /// <param name="translation">The translation.</param>
        /// <returns></returns>
        public static StringBuilder ToCSharpCode(Translation translation)
        {
            return ToCSharpCode(translation, GetCompileUsings(translation), translation.GetMacros());
        }

        /// <summary>
        /// Creates StringBuilder with C# code of translation class.
        /// </summary>
        /// <param name="className">Name of the class.</param>
        /// <param name="translation">The translation.</param>
        /// <returns></returns>
        public static StringBuilder ToCSharpCode(string className, Translation translation)
        {
            string code = "";
            TranslationClasses classes = translation.GetEnglishClasses();
            Type type = FindType(className);
            if (type == null)
            {
                if(Translator.ReportErrors)
                    throw new Exception($"NLocalizer: Class {className} not found in your project to translate");
                return new StringBuilder(0);
            }

            if (classes.ContainsKey(className))
            {
                if (translation.StaticClasses.Contains(className) == false)
                {
                    code = GetTemplate("DynamicClass").Replace("$(ClassName)", className);
                    var remember = new StringBuilder();
                    foreach (KeyValuePair<string, TranslationProperty> item in classes[className])
                    {
                        if (PropertyExists(item.Key, type))
                        {
                            if (item.Value.IsStatic == false)
                                remember.Append(
                                    GetTemplate("DynamicClassRemember")
                                        .Replace("$(ClassName)", className)
                                        .Replace("$(PropertyName)", item.Key));
                        }
                        else
                        {
                            if(Translator.ReportErrors)
                                throw new Exception($"NLocalizer: compile error - '{className}' does not contain a definition for '{item.Key}'. Please check - is property marked as public?");
                        }
                    }
                    code = code.Replace("$(DynamicClassRemember)", remember.ToString());

                    var translate = new StringBuilder();
                    foreach (KeyValuePair<string, TranslationProperty> item in classes[className])
                    {
                        if (PropertyExists(item.Key, type))
                        {
                            if (item.Value.IsStatic == false)
                                translate.Append(
                                    GetTemplate("DynamicClassTranslate")
                                        .Replace("$(ClassName)", className)
                                        .Replace("$(PropertyName)", item.Key));
                        }
                        else
                        {
                            if (Translator.ReportErrors)
                                throw new Exception($"NLocalizer: compile error - '{className}' does not contain a definition for '{item.Key}'. Please check - is property marked as public?");
                        }
                    }
                    code = code.Replace("$(DynamicClassTranslate)", translate.ToString());
                }

                if (translation.StaticClasses.Contains(className) || classes[className].StaticPropertyExists())
                {
                    code = GetTemplate("StaticClass").Replace("$(ClassName)", className);

                    var remember = new StringBuilder();
                    foreach (KeyValuePair<string, TranslationProperty> item in classes[className])
                    {
                        if (PropertyExists(item.Key, type))
                        {
                            if (item.Value.IsStatic)
                                remember.Append(
                                    GetTemplate("StaticClassRemember")
                                        .Replace("$(ClassName)", className)
                                        .Replace("$(PropertyName)", item.Key));
                        }
                        else
                        {
                            if (Translator.ReportErrors)
                                throw new Exception($"NLocalizer: compile error - '{className}' does not contain a definition for '{item.Key}'. Please check - is property marked as public?");
                        }
                    }
                    code = code.Replace("$(StaticClassRemember)", remember.ToString());

                    var translate = new StringBuilder();
                    foreach (KeyValuePair<string, TranslationProperty> item in classes[className])
                    {
                        if (PropertyExists(item.Key, type))
                        {
                            if (item.Value.IsStatic)
                                translate.Append(
                                    GetTemplate("StaticClassTranslate")
                                        .Replace("$(ClassName)", className)
                                        .Replace("$(PropertyName)", item.Key));
                        }
                        else
                        {
                            if (Translator.ReportErrors)
                                throw new Exception($"NLocalizer: compile error - '{className}' does not contain a definition for '{item.Key}'. Please check - is property marked as public?");
                        }
                    }
                    code = code.Replace("$(StaticClassTranslate)", translate.ToString());
                }
            }
            else
            {
                if (Translator.ReportErrors)
                    throw new Exception($"NLocalizer: Class {className} not found in your English language file.");
            }
            return new StringBuilder(code);
        }

        private static bool PropertyExists(string property, Type type)
        {
            if (property.Contains("."))
            {
                var splitProperty = property.Split('.');
                var field = type.GetField(splitProperty[0].Trim(), BindingFlags.Public | BindingFlags.Instance);
                if (field != null)
                {
                    if (field.FieldType.GetProperty(splitProperty[1].Trim(), BindingFlags.Public | BindingFlags.Instance) != null)
                    {
                        return true;
                    }
                    if (field.FieldType.GetField(splitProperty[1].Trim(), BindingFlags.Public | BindingFlags.Instance) != null)
                    {
                        return true;
                    }
                }
                var propertyInfo = type.GetProperty(splitProperty[0].Trim(), BindingFlags.Public | BindingFlags.Instance);
                if (propertyInfo != null)
                {
                    if (propertyInfo.PropertyType.GetProperty(splitProperty[1].Trim(), BindingFlags.Public | BindingFlags.Instance) != null)
                    {
                        return true;
                    }
                }
            }
            else
            {
                var propertyInfo = type.GetProperty(property, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                if (propertyInfo != null)
                {
                    return true;
                }
                var fieldInfo = type.GetField(property, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                if (fieldInfo != null)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Get file name location of Assembly.
        /// </summary>
        /// <param name="assembly">Assembly.</param>
        /// <returns></returns>
        public static string GetAssemblyFileName(Assembly assembly)
        {
            return assembly.CodeBase.Replace("file:///", "");
        }

        /// <summary>
        /// Gets the default compile directory (current assembly directory).
        /// </summary>
        /// <returns></returns>
        public static string GetDefaultCompileDirectory(Assembly assembly)
        {
            if (assembly != null)
                return Path.GetDirectoryName(GetAssemblyFileName(assembly));
            return Directory.GetCurrentDirectory();
        }

        /// <summary>
        /// Gets the default compile directory (current assembly directory).
        /// </summary>
        /// <returns></returns>
        public static string GetDefaultCompileDirectory()
        {
            // TODO: Test działania w katalogu Program Files
            return GetDefaultCompileDirectory(Assembly.GetEntryAssembly());
        }

        /// <summary>
        /// Search all assemblies to find type className
        /// </summary>
        /// <param name="className">name of type to find</param>
        /// <returns></returns>
        public static Type FindType(string className)
        {
            return FindType(className, Assembly.GetEntryAssembly());
        }

        /// <summary>
        /// Search all assemblies to find type className
        /// </summary>
        /// <param name="className">Name of class type to find.</param>
        /// <param name="assembly">Assembly with types.</param>
        /// <returns></returns>
        public static Type FindType(string className, Assembly assembly)
        {
            if (assembly == null)
                return null;

            // TODO: Sprawdzić wielkie i małe znaki w VB.NET
            foreach (Type type in assembly.GetTypes())
                if (type.IsClass && (type.Name == className || type.FullName == className) && type.IsPublic)
                    return type;

            foreach (Assembly assemblyItem in AppDomain.CurrentDomain.GetAssemblies())
                if (!Equals(assembly, assemblyItem))
                    foreach (Type type in assemblyItem.GetTypes())
                        if (type.IsClass && (type.Name == className || type.FullName == className) && type.IsPublic)
                            return type;

            return null;
        }

        /// <summary>
        /// Get name of DLL or EXE file name
        /// </summary>
        /// <param name="type">Type to examine</param>
        /// <returns></returns>
        public static string GetDllName(Type type)
        {
            if (type.Assembly.CodeBase != "")
                return GetAssemblyFileName(type.Assembly);
            return type.Assembly.FullName;
        }

        /// <summary>
        /// Get all namespaces in translation used during compilation.
        /// </summary>
        /// <param name="translation">Translation.</param>
        /// <returns></returns>
        public static List<string> GetCompileUsings(Translation translation)
        {
            return GetCompileUsings(translation, Assembly.GetEntryAssembly());
        }

        /// <summary>
        /// Get all namespaces in translation used during compilation.
        /// </summary>
        /// <param name="translation">Translation.</param>
        /// <param name="assembly">Assembly.</param>
        /// <returns></returns>
        public static List<string> GetCompileUsings(Translation translation, Assembly assembly)
        {
            var usings = new List<string>();

            foreach (string className in translation.GetAllClasses())
            {
                Type type = FindType(className, assembly);
                if (type != null && usings.Contains(type.Namespace) == false)
                    usings.Add(type.Namespace);
            }
            foreach (string usingName in translation.CodeUsings)
                if (usings.Contains(usingName) == false)
                    usings.Add(usingName);

            if (usings.Contains("System.Windows.Forms") == false)
                usings.Add("System.Windows.Forms");

            if (usings.Contains("System") == false)
                usings.Add("System");

            return usings;
        }

        /// <summary>
        /// Get list of .dll and .exe file names to compile
        /// </summary>
        /// <param name="translation">The translation.</param>
        public static List<string> GetCompileDlls(Translation translation)
        {
            return GetCompileDlls(translation, Assembly.GetEntryAssembly());
        }

        /// <summary>
        /// Get list of .dll and .exe file names to compile
        /// </summary>
        /// <param name="translation">The translation.</param>
        /// <param name="assembly">The assembly of translation.</param>
        public static List<string> GetCompileDlls(Translation translation, Assembly assembly)
        {
            var dlls = new List<string>();

            if (assembly != null)
            {
                if (!dlls.Contains(assembly.Location))
                    dlls.Add(assembly.Location);

                foreach (AssemblyName assemblyName in assembly.GetReferencedAssemblies())
                {
                    Assembly assemblyLoad = Assembly.Load(assemblyName);
                    if (!dlls.Contains(assemblyName.Name.ToLower() + ".dll"))
                    {
                        dlls.Add(assemblyLoad.Location);
                    }
                }
            }
            else
            {
                dlls.Add("system.windows.forms.dll");
                dlls.Add("nlocalizer.dll");
                dlls.Add("nlocalizer.tests.dll");
            }

            foreach (string dll in translation.CodeDlls)
                if (dlls.Contains(dll.ToLower()) == false)
                    dlls.Add(dll.ToLower());

            return dlls;
        }

        /// <summary>
        /// Compiles the specified translation.
        /// </summary>
        /// <param name="translation">The translation.</param>
        /// <returns></returns>
        public static CompilerResults Compile(Translation translation)
        {
            return Compile(translation, GetDefaultCompileDirectory());
        }

        /// <summary>
        /// Compiles the specified translation.
        /// </summary>
        /// <param name="translation">The translation.</param>
        /// <param name="directoryName">Name of the directory.</param>
        /// <param name="assembly">Translation assembly.</param>
        /// <returns></returns>
        public static CompilerResults Compile(Translation translation, string directoryName, Assembly assembly)
        {
            return Compile(translation, directoryName,
                GetCompileDlls(translation, assembly), GetCompileUsings(translation, assembly), translation.GetMacros());
        }

        /// <summary>
        /// Compiles the specified translation.
        /// </summary>
        /// <param name="translation">The translation.</param>
        /// <param name="directoryName">Name of the directory.</param>
        /// <returns></returns>
        public static CompilerResults Compile(Translation translation, string directoryName)
        {
            return Compile(translation, directoryName,
                GetCompileDlls(translation), GetCompileUsings(translation), translation.GetMacros());
        }

        /// <summary>
        /// Auto detect current version of framework or sets defaultVersion if exists.
        /// </summary>
        /// <param name="defaultVersion"></param>
        /// <returns></returns>
        public static string GetFrameworkVersion(string defaultVersion)
        {
            string programVersion = Assembly.GetEntryAssembly().ImageRuntimeVersion.Trim();
            if (programVersion.StartsWith("v") && programVersion.Length > 1)
                programVersion = programVersion.Substring(1);
            string version = defaultVersion.Trim().ToLower();
            if (version.StartsWith("v") && version.Length > 1)
                version = version.Substring(1);

            if (version == "")
                version = programVersion;

            if (new Version(version).Major > new Version(programVersion).Major)
                version = programVersion;
            if (new Version(version).Major == new Version(programVersion).Major &&
                new Version(version).Minor > new Version(programVersion).Minor)
                version = programVersion;

            return $"v{new Version(version).Major}.{new Version(version).Minor}";
        }

        /// <summary>
        /// Compiles the specified translation.
        /// </summary>
        /// <param name="translation">The translation.</param>
        /// <param name="directoryName">Name of the directory.</param>
        /// <param name="compileDlls">List of dll file names used in translation project.</param>
        /// <param name="compileMacros">List of macros used in translation project.</param>
        /// <param name="compileUsings">List of namespaces used in translation project.</param>
        /// <returns></returns>
        public static CompilerResults Compile(Translation translation, string directoryName,
            List<string> compileDlls, List<string> compileUsings, List<string> compileMacros)
        {
            Log.Init(translation);
            Log.Append(translation, "Compile framework version", GetFrameworkVersion(translation.FrameworkVersion));
            var provider = new CSharpCodeProvider(new Dictionary<string, string>
                {{"CompilerVersion", GetFrameworkVersion(translation.FrameworkVersion)}});

            var parameters = new CompilerParameters
            {
                GenerateInMemory = true,
                TreatWarningsAsErrors = false,
                GenerateExecutable = false,
                IncludeDebugInformation = false,
                CompilerOptions = "/optimize"
            };
            parameters.ReferencedAssemblies.AddRange(compileDlls.ToArray());

            Log.Append(translation, "Compile directory", directoryName);
            Log.Append(translation, "Compile dll", compileDlls);
            Log.Append(translation, "Compile using", compileUsings);
            Log.Append(translation, "Compile macro", compileMacros);

            string lastDirectory = Directory.GetCurrentDirectory();
            CompilerResults compiler;
            try
            {
                var uri = new Uri(directoryName.Replace("\\", "/").Replace("file:/", "file://"));
                Directory.SetCurrentDirectory((uri.LocalPath));
                string csharpCode = ToCSharpCode(translation, compileUsings, compileMacros).ToString();
                if (translation.DebugFileName != "")
                    File.WriteAllText(translation.DebugFileName, csharpCode);
                compiler = provider.CompileAssemblyFromSource(parameters, csharpCode);
                if (compiler.Errors.HasErrors)
                {
                    bool disableDll = false;
                    foreach (CompilerError error in compiler.Errors)
                        if (error.ErrorNumber == "CS0006")
                        {
                            disableDll = true;
                            string dllName = error.ErrorText;
                            dllName = dllName.Substring(dllName.IndexOf("'", StringComparison.Ordinal) + 1);
                            dllName = dllName.Substring(0, dllName.IndexOf("'", StringComparison.Ordinal));
                            Log.Append(translation, "Disable dll", dllName);
                            int index = 0;
                            while (index < compileDlls.Count)
                                if (String.Compare(compileDlls[index], dllName, StringComparison.OrdinalIgnoreCase) == 0)
                                    compileDlls.RemoveAt(index);
                                else
                                    index++;
                        }
                    if (disableDll)
                    {
                        parameters.ReferencedAssemblies.Clear();
                        parameters.ReferencedAssemblies.AddRange(compileDlls.Distinct().ToArray());

                        compiler = provider.CompileAssemblyFromSource(parameters, csharpCode);
                    }
                }

                if (compiler.Errors.HasErrors)
                {
                    Log.Append(translation, "Compile", compiler.Errors.Count + " error(s)");
                    foreach (CompilerError error in compiler.Errors)
                        Log.Append(translation, "Error " + error.ErrorNumber + " in line " + error.Line,
                            error.ErrorText);

                    Log.Append(translation, "Compile", compiler.Errors.Count + " error(s)");
                    CompilerError firstError = compiler.Errors[0];
                    switch (firstError.ErrorNumber)
                    {
                        case "CS0117":
                            throw new Exception($"NLocalizer: {firstError.ErrorText}. Please check - is property marked as public?");
                        default:
                            throw new Exception(
                                $@"NLocalizer: compile error - {firstError.ErrorText}. Please correct .lang files. Please completely remove and then reinstall latest version of the application if you didn't alter the language files.");
                    }
                }
                else
                    Log.Append(translation, "Compile", "OK");
            }
            finally
            {
                Directory.SetCurrentDirectory(lastDirectory);
            }
            return compiler;
        }

        /// <summary>
        /// Gets the compiled class from compiler.
        /// </summary>
        /// <param name="compiler">The compiler.</param>
        /// <returns></returns>
        public static Type GetCompiledClass(CompilerResults compiler)
        {
            if (compiler == null || compiler.CompiledAssembly == null)
                throw new Exception("NLocalizer: Runtime translator not compiled");

            if (compiler.CompiledAssembly.GetModules().Length < 1)
                throw new Exception("NLocalizer: No modules in runtime translator");

            Module module = compiler.CompiledAssembly.GetModules()[0];
            if (module == null)
                throw new Exception("NLocalizer: No modules in runtime translator");

            Type type = module.GetType("NLocalizer.RuntimeTranslator");
            if (type == null)
                throw new Exception("NLocalizer: Type NLocalizer.RuntimeTranslator is not found in runtime translator");

            return type;
        }

        /// <summary>
        /// Invokes the specified method in type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="types">The types of parameters.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public static object Invoke(Type type, string methodName, Type[] types, object[] parameters)
        {
            if (type == null)
            {
                return null;
            }

            MethodInfo method = types != null ? type.GetMethod(methodName, types) : type.GetMethod(methodName);

            if (method == null)
            {
                string typeParams = "";
                if (types != null)
                    foreach (Type t in types)
                        typeParams += t.Name + ", ";
                if (typeParams != "")
                    typeParams = typeParams.Substring(0, typeParams.Length - ", ".Length);
                throw new Exception(
                    $"NLocalizer: Method {methodName}({typeParams}) is not found in runtime translator class {type.FullName}");
            }
            return method.Invoke(null, parameters);
        }

        /// <summary>
        /// Translates the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="translation">The translation.</param>
        public static void Translate(Type type, Translation translation)
        {
            try
            {
                Invoke(type, MethodTranslate,
                    new[] {translation.GetType()},
                    new object[] {translation});
            }
            catch (Exception ex)
            {
                Log.Append(translation, "Translate error", ex.Message);
                Log.Append(translation, "Error Stack", ex.StackTrace);
                throw;
            }
        }

        /// <summary>
        /// Translates all static classes into language.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="language">The language.</param>
        /// <param name="translation">The translation.</param>
        public static void Translate(Type type, string language, Translation translation)
        {
            try
            {
                Invoke(type, MethodTranslate,
                    new[] {language.GetType(), translation.GetType()},
                    new object[] {language, translation});
            }
            catch (Exception ex)
            {
                Log.Append(translation, "Translate error", ex.Message);
                Log.Append(translation, "Error Stack", ex.StackTrace);
                throw;
            }
        }

        /// <summary>
        /// Translates the specified obj into language.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="language">The language.</param>
        /// <param name="obj">The obj.</param>
        /// <param name="translation">The translation.</param>
        public static void Translate(Type type, string language, object obj, Translation translation)
        {
            try
            {
                Invoke(type, MethodTranslate,
                    new[] {language.GetType(), obj.GetType(), translation.GetType()},
                    new[] {language, obj, translation});
            }
            catch (Exception ex)
            {
                Log.Append(translation, "Translate error", ex.Message);
                Log.Append(translation, "Error Stack", ex.StackTrace);
                throw;
            }
        }

        /// <summary>
        /// Translates the specified obj into current language.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="obj">The obj.</param>
        /// <param name="translation">The translation.</param>
        public static void Translate(Type type, object obj, Translation translation)
        {
            Translate(type, translation.CurrentLanguage, obj, translation);
        }

        /// <summary>
        /// Restores all static classes into neutral language.
        /// </summary>
        /// <param name="type">The type.</param>
        public static void Restore(Type type)
        {
            Invoke(type, MethodRestore, null, null);
        }

        /// <summary>
        /// Restores all static classes into neutral language.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="translation">The translation.</param>
        public static void Restore(Type type, Translation translation)
        {
            try
            {
                Invoke(type, MethodRestore,
                    new[] {translation.GetType()},
                    new object[] {translation});
            }
            catch (Exception ex)
            {
                Log.Append(translation, "Translate error", ex.Message);
                Log.Append(translation, "Error Stack", ex.StackTrace);
                throw;
            }
        }

        /// <summary>
        /// Restores the specified obj into neutral language.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="obj">The obj.</param>
        /// <param name="translation">The translation.</param>
        public static void Restore(Type type, object obj, Translation translation)
        {
            try
            {
                Invoke(type, MethodRestore,
                    new[] {obj.GetType(), translation.GetType()},
                    new[] {obj, translation});
            }
            catch (Exception ex)
            {
                Log.Append(translation, "Translate error", ex.Message);
                Log.Append(translation, "Error Stack", ex.StackTrace);
                throw;
            }
        }

        /// <summary>
        /// Creates the specified className.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="className">Name of the class.</param>
        /// <returns></returns>
        public static object Create(Type type, string className)
        {
            return Invoke(type, MethodCreate + className, null, null);
        }

        /// <summary>
        /// Shows the dialog of specified className.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="className">Name of the class.</param>
        public static void ShowDialog(Type type, string className)
        {
            object obj = Create(type, className);
            if (obj is Form)
                Invoke(type, MethodShowDialog, null, null);
        }
    }
}