using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RazorEngine;
using System.Globalization;
using System.IO;
using System.Reflection;



namespace ConsoleApplication2
{
    class Program
    {
        static void Main(string[] args)
        {

            var model = new
            {
                Id = 10,
                Name = "Name1",
                Items = new List<string> { "Item1", "Item2", "Item3", "Item4", "Item5" }
            };

            var templateService = new TemplatesService(new FileSystemService(), new RazorTemplateEngine());

            var result = templateService.Parse("first", model);

            Console.WriteLine(templateService._templatesDirectoryFullName);
            


            Console.ReadKey();
            
           
        }
    }

    public interface ITemplateEngine
    {
        string Parse(string template, dynamic model);
    }
    
    public class RazorTemplateEngine : ITemplateEngine
    {
        public string Parse(string template, dynamic model)
        {
            return Razor.Parse(template, model);
        }
    }
    
    public interface ITemplatesService
    {
        string Parse(string templateName, dynamic model, CultureInfo cultureInfo = null);
    }
    
    public interface IFileSystemService
    {
        string ReadAllText(string fileName);
        bool FileExists(string fileName);
        string GetCurrentDirectory();
    }
    
    public class FileSystemService : IFileSystemService
    {
        public string ReadAllText(string fileName)
        {
            return File.ReadAllText(fileName);
        }

        public bool FileExists(string fileName)
        {
            return File.Exists(fileName);
        }

        public string GetCurrentDirectory()
        {
           return new System.Uri(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase)).
        LocalPath;

        }

        
       
        
       

   }

    public class TemplatesService : ITemplatesService
    {
        private const string DefaultLanguage = "en";
        private const string TemplatesDirectoryName = "Templates";
        private const string TemplateFileNameWithCultureTemplate = "{0}.{1}.template";
        private const string TemplateFileNameWithoutCultureTemplate = "{0}.template";

        private readonly IFileSystemService _fileSystemService;
        private readonly ITemplateEngine _templateEngine;
        public readonly string _templatesDirectoryFullName;

        public TemplatesService(IFileSystemService fileSystemService, ITemplateEngine templateEngine)
        {
            _fileSystemService = fileSystemService;
            _templateEngine = templateEngine;
            _templatesDirectoryFullName = Path.Combine(_fileSystemService.GetCurrentDirectory(), TemplatesDirectoryName);
        }

        // rest of the code

        public string Parse(string templateName, dynamic model, CultureInfo cultureInfo = null)
        {
            var templateContent = GetContent(templateName, cultureInfo);

            return _templateEngine.Parse(templateContent, model);
        }
        private string GetContent(string templateName, CultureInfo cultureInfo)
        {
            var templateFileName = TryGetFileName(templateName, cultureInfo);
            if (string.IsNullOrEmpty(templateFileName))
            {
                throw new FileNotFoundException(string.Format("Template file not found for template '{0}' in '{1}'", templateName, _templatesDirectoryFullName));
            }

            return _fileSystemService.ReadAllText(templateFileName);
        }

        private string TryGetFileName(string templateName, CultureInfo cultureInfo)
        {
            var language = GetLanguageName(cultureInfo);

            // check file for current culture
            var fullFileName = GetFullFileName(templateName, language);
            if (_fileSystemService.FileExists(fullFileName))
            {
                return fullFileName;
            }

            // check file for default culture
            if (language != DefaultLanguage)
            {
                fullFileName = GetFullFileName(templateName, DefaultLanguage);
                if (_fileSystemService.FileExists(fullFileName))
                {
                    return fullFileName;
                }
            }

            // check file without culture
            fullFileName = GetFullFileName(templateName, string.Empty);
            if (_fileSystemService.FileExists(fullFileName))
            {
                return fullFileName;
            }

            return string.Empty;
        }
        private static string GetLanguageName(CultureInfo cultureInfo)
        {
            return cultureInfo != null ? cultureInfo.TwoLetterISOLanguageName.ToLower() : DefaultLanguage;
        }

        private string GetFullFileName(string templateName, string language)
        {
            var fileNameTemplate = string.IsNullOrEmpty(language) ? TemplateFileNameWithoutCultureTemplate : TemplateFileNameWithCultureTemplate;

            var templateFileName = string.Format(fileNameTemplate, templateName, language);

            return Path.Combine(_templatesDirectoryFullName, templateFileName);
        }


    }
    
   
   
 
   

}
