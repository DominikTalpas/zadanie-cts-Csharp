using System;
using System.IO;
using System.Net;
using System.Text;
using HtmlAgilityPack;

class Program
{
    static void Main()
    {
        // URL of the main page
        string url = "https://www.cts-tradeit.cz/kariera/";

        // Fetch the main page content
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

        // Find all job links
        var jobLinks = mainPageDoc.DocumentNode.SelectNodes("//a[@href]");
        if (jobLinks != null)
        {
            foreach (HtmlNode link in jobLinks)
            {
                string href = link.GetAttributeValue("href", "");
                if (href.Contains("/kariera/"))
                {
                    string fullLink = "https://www.cts-tradeit.cz" + href;
                    string jobTitle = href.TrimEnd('/').Split('/')[^1].Replace(" ", "_");

                    // Fetch job page content
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

                    // Extract the "Co konkrétně Vás čeká?" section
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
                        string fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{jobTitle}.txt");

                        // Save the requirementsText to a text file
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