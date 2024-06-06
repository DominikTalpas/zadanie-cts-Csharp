using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using HtmlAgilityPack;

class Program
{
    static void Main()
    {
        
        string url = "https://www.cts-tradeit.cz/kariera/";

        
        string mainPageContent = "";
        using (WebClient client = new WebClient())
        {
            try
            {
                mainPageContent = client.DownloadString(url);
            }
            catch (WebException ex)
            {
                Console.WriteLine($"Failed to fetch the main page. Status code: {((HttpWebResponse)ex.Response).StatusCode}");
                return;
            }
        }

        HtmlDocument mainPageDoc = new HtmlDocument();
        mainPageDoc.LoadHtml(mainPageContent);

        
        var jobLinks = mainPageDoc.DocumentNode.SelectNodes("//a[@href]");
        if (jobLinks != null)
        {
            foreach (HtmlNode link in jobLinks)
            {
                string href = link.GetAttributeValue("href", "");
                if (href.Contains("/kariera/"))
                {
                    string fullLink = "https://www.cts-tradeit.cz" + href;
                    string jobTitle = href.TrimEnd('/').Split('/')[^1];

                   
                    string jobPageContent = "";
                    using (WebClient jobClient = new WebClient())
                    {
                        try
                        {
                            jobPageContent = jobClient.DownloadString(fullLink);
                        }
                        catch (WebException ex)
                        {
                            Console.WriteLine($"Failed to fetch job page: {fullLink}. Status code: {((HttpWebResponse)ex.Response).StatusCode}");
                            continue;
                        }
                    }

                    HtmlDocument jobPageDoc = new HtmlDocument();
                    jobPageDoc.LoadHtml(jobPageContent);
                    
                   
                    var section = jobPageDoc.DocumentNode.SelectSingleNode("//h2[text()='Co Tě u nás čeká?']");
                    if (section != null)
                    {
                        var elementUl = section.SelectSingleNode("following-sibling::ul");
                        var elementP = section.SelectNodes("following-sibling::*");

                        StringBuilder requirementsText = new StringBuilder();
                        if (elementUl != null)
                            requirementsText.Append(elementUl.InnerText.Trim());

                        foreach (var elem in elementP)
                        {
                            if (elem.Name == "ul")
                                break;
                            if (elem.Name == "p")
                                requirementsText.Append(elem.InnerText.Trim());
                        }

                        string text = requirementsText.ToString().Trim();
                        
                        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                        DirectoryInfo binDirectory = Directory.GetParent(baseDirectory).Parent.Parent.Parent;
                        string projectPath  = binDirectory.FullName;
                        string fileName = Path.Combine(projectPath, $"{jobTitle}.txt");

                        
                        using (StreamWriter sw = new StreamWriter(fileName, false, Encoding.UTF8))
                        {
                            sw.WriteLine($"Co Tě u nás čeká? {text}");
                        }
                    }
                }
            }
            Console.WriteLine("Job details have been saved in the directory.");
        }
    }
}