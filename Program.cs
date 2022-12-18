using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Collections.ObjectModel;
using System.Globalization;
using CsvHelper;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Text;

namespace Web_Scraper
{
    public class Video
    {
        public string Link { get; set; }
        public string Title { get; set; }
        public string Uploader { get; set; }
        public string Views { get; set; }
    }
    public class Short
    {
        public string Link { get; set; }
        public string Title { get; set; }
        public string Views { get; set; }
    }
    public class Job
    {
        public string Title { get; set; }
        public string Company { get; set; }
        public string Location { get; set; }
        public string keywords { get; set; }
        public string Link { get; set; }
    }
    public class Player
    {
        public string Nationality { get; set; }
        public string Name { get; set; }
        public string InGameName { get; set; }
        public string Link { get; set;}
    }
    public class Team
    {
        public string Name { get; set; }
        public string Link { get; set; }
    }
    public class Event
    {
        public string Name { get; set; }
        public string Link { get; set; }
    }
    public class Article
    {
        public string Region { get; set; }
        public string Title { get; set; }
        public string PublishDate { get; set; }
        public string Author { get; set; }
        public string Link { get; set; }
    }
    public class HLTVObject
    { 
        public string Type { get; set; }
        public string Name { get; set; }
        public string Link { get; set; }
        public string Title { get; set; }
        public string Nationality { get; set; }
        public string Region { get; set; }
        public string InGameName { get; set; }
        public string PublishDate { get; set; }
        public string Author { get; set; }
    }
    class Program
    {
        private static readonly JsonSerializerOptions _options =
        new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
        public static void PrettyWrite(object obj, string fileName)
        {
            var options = new JsonSerializerOptions(_options)
            {
                WriteIndented = true
            };
            var jsonString = JsonSerializer.Serialize(obj, options);
            File.WriteAllText(fileName, jsonString);
        }
        
        public static ChromeOptions SetBrowserLanguage()
        {
            var languages = new Dictionary<string, string>()
            {
            {"English", "--lang=en"},
            {"Russian", "--lang=ru"},
            {"Spanish", "--lang=es"},
            {"French", "--lang=fr"},
            {"German", "--lang=de"},
            {"Japanese", "--lang=ja"},
            {"Chinese Simplified", "--lang=zh-Hans"},
            {"Chinese Traditional", "--lang=zh-Hant"},
            {"Dutch", "--lang=nl"},
            };
            Console.WriteLine($"Choose browser language:");
            foreach (var language in languages.Select((Entry, Index) => new { Entry, Index }))
            {
                Console.WriteLine($"{language.Index + 1}. {language.Entry.Key}");
            }
            int language_choice = Convert.ToInt32(Console.ReadLine());
            ChromeOptions opt = new ChromeOptions();
            opt.AddArguments(languages.ElementAt(language_choice -1).Value);
            return opt;
        }
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("Choose an option to look for:\n1. Search for videos/shorts on youtube\n2. Search for jobs on ICT-jobs\n3. Search for results on HLTV\n");
            int choice = Convert.ToInt32(Console.ReadLine());
            if (choice == 1)
            {
                ChromeOptions language = SetBrowserLanguage();
                Console.WriteLine("Enter a search term for YouTube videos: ");
                string searchTerm = Console.ReadLine();
                Console.WriteLine("How many videos would you like to return: ");
                int numberVideos = Convert.ToInt32(Console.ReadLine());
                string url_youtube = "https://www.youtube.com/results?search_query=" + searchTerm + "&sp=CAI%253D";
                IWebDriver driver = new ChromeDriver(language);
                driver.Navigate().GoToUrl(url_youtube);
                var timeout = 10000; 
                var wait = new WebDriverWait(driver, TimeSpan.FromMilliseconds(timeout));
                wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));
                var videoObjects = new List<Video>();
                Thread.Sleep(5000);
                try
                {
                    Console.WriteLine($"{numberVideos} most recently uploaded videos when looking for \"{searchTerm}\"");
                    for (int i = 1; i < numberVideos + 1; i++)
                    {
                        Video video = new Video();
                        video.Link = driver.FindElement(By.XPath($"//*[@id=\"contents\"]/ytd-video-renderer[{i}]/div[1]/ytd-thumbnail/a")).GetAttribute("href");
                        video.Title = driver.FindElement(By.XPath($"//*[@id=\"contents\"]/ytd-video-renderer[{i}]/div[1]/div/div[1]/div/h3/a/yt-formatted-string")).Text;
                        video.Uploader = driver.FindElement(By.XPath($"//*[@id=\"contents\"]/ytd-video-renderer[{i}]/div[1]/div/div[2]/ytd-channel-name/div/div/yt-formatted-string/a")).Text;
                        video.Views = driver.FindElement(By.XPath($"//*[@id=\"contents\"]/ytd-video-renderer[{i}]/div[1]/div/div[1]/ytd-video-meta-block/div[1]/div[2]/span[1]")).Text;
                        videoObjects.Add(video);
                        Console.WriteLine($"-------------------------------------------------------Video{i}\nLink: {videoObjects[i - 1].Link}" +
                            $"\nTitle: {videoObjects[i - 1].Title}\nUploader: {videoObjects[i - 1].Uploader}\nViews: {videoObjects[i - 1].Views}\n");
                    }
                    using (var writer = new StreamWriter("videos.csv"))
                    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {
                        csv.WriteRecords(videoObjects);
                    }
                    PrettyWrite(videoObjects, "videos.json");
                }
                catch
                {
                    Console.WriteLine("---------------------------------------------------------------\n" +
                        "No results found for that search term, try another search term.\n" +
                        "---------------------------------------------------------------");
                }
                finally
                {
                    driver.Quit();
                }
                Console.WriteLine("Action finished");
            }
            else if (choice == 2)
            {
                var jobObjects = new List<Job>();
                Console.WriteLine("Enter a search term for jobs on ICT-jobs: ");
                string searchTerm = Console.ReadLine();
                Console.WriteLine("How many jobs would you like to return: ");
                int numberJobs = Convert.ToInt32(Console.ReadLine());
                string url_ict_jobs = "https://www.ictjob.be/en/search-it-jobs?keywords=" + searchTerm;
                IWebDriver driver = new ChromeDriver();
                driver.Navigate().GoToUrl(url_ict_jobs);
                var timeout = 10000; 
                var wait = new WebDriverWait(driver, TimeSpan.FromMilliseconds(timeout));
                wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));
                Thread.Sleep(5000);
                driver.FindElement(By.Id("sort-by-date")).Click();
                Int64 last_height = (Int64)(((IJavaScriptExecutor)driver).ExecuteScript("return document.documentElement.scrollHeight"));
                while (true)
                {
                    ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollTo(0, document.documentElement.scrollHeight);");
                    Thread.Sleep(5000);
                    /* Calculate new scroll height and compare with last scroll height */
                    Int64 new_height = (Int64)((IJavaScriptExecutor)driver).ExecuteScript("return document.documentElement.scrollHeight");
                    if (new_height == last_height)
                        /* If heights are the same it will exit the function */
                        break;
                    last_height = new_height;
                }
                try
                {
                    ReadOnlyCollection<IWebElement> jobs = driver.FindElements(By.CssSelector("li[itemtype=\"http://schema.org/JobPosting\"]"));
                    Console.WriteLine($"{numberJobs} most recently uploaded jobs when looking for \"{searchTerm}\"");
                    for (int i = 0; i < numberJobs; i++)
                    {
                        Job job = new Job();
                        job.Title = jobs[i].FindElement(By.CssSelector(".job-title")).Text;
                        job.Company = jobs[i].FindElement(By.CssSelector(".job-company")).Text;
                        job.Location = jobs[i].FindElement(By.XPath("//*[@id=\"search-result-body\"]/div/ul/li/span[2]/span[2]/span[2]/span/span")).Text.Replace(", ", "/");
                        job.keywords = jobs[i].FindElement(By.CssSelector(".job-keywords")).Text.Replace(", ", "/");
                        job.Link = jobs[i].FindElement(By.CssSelector(".search-item-link")).GetAttribute("href");
                        jobObjects.Add(job);
                        Console.WriteLine($"-------------------------------------------------------Job {i + 1}\nTitle: {job.Title}" +
                           $"\nCompany: {job.Company}\nLocation: {job.Location}\nKeywords: {job.keywords}\nLink: {job.Link}\n");
                    }
                    using (var writer = new StreamWriter("jobs.csv"))
                    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {
                        csv.WriteRecords(jobObjects);
                    }
                    PrettyWrite(jobObjects, "jobs.json");
                }
                catch
                {
                    Console.WriteLine("---------------------------------------------------------------\n" +
                        "No results found for that search term, try another search term.\n" +
                        "---------------------------------------------------------------");
                }
                finally
                {
                    driver.Quit();
                }
                Console.WriteLine("Action finished");
            }
            else if(choice == 3)
            {
                Console.WriteLine("Enter a search term for players/teams/events/articles in HLTV: ");
                string searchTerm = Console.ReadLine();
                string url_hltv = "https://www.hltv.org/search?query=" + searchTerm;
                IWebDriver driver = new ChromeDriver();
                driver.Navigate().GoToUrl(url_hltv);
                var timeout = 10000; /* Maximum wait time of 10 seconds */
                var wait = new WebDriverWait(driver, TimeSpan.FromMilliseconds(timeout));
                wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));

                Thread.Sleep(5000);

                Int64 last_height = (Int64)(((IJavaScriptExecutor)driver).ExecuteScript("return document.documentElement.scrollHeight"));
                while (true)
                {
                    ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollTo(0, document.documentElement.scrollHeight);");
                    Thread.Sleep(5000);
                    /* Calculate new scroll height and compare with last scroll height */
                    Int64 new_height = (Int64)((IJavaScriptExecutor)driver).ExecuteScript("return document.documentElement.scrollHeight");
                    if (new_height == last_height)
                        /* If heights are the same it will exit the function */
                        break;
                    last_height = new_height;
                }
                try
                {
                    ReadOnlyCollection<IWebElement> elements = driver.FindElements(By.CssSelector("table[class=\"table\"]"));
                    if (elements.Count == 0)
                    {
                        Console.WriteLine("---------------------------------------------------------------\n" +
                       "No results found for that search term, try another search term.\n" +
                       "---------------------------------------------------------------");
                    }
                    else
                    {
                        var hltvObjectList = new List<HLTVObject>();
                        foreach (IWebElement element in elements)
                        {
                            Console.WriteLine(element.FindElement(By.CssSelector("td[class=\"table-header\"]")).Text + "(s)\n");
                            if (element.FindElement(By.CssSelector("td[class=\"table-header\"]")).Text == "Player")
                            {
                                ReadOnlyCollection<IWebElement> players = element.FindElements(By.CssSelector("tbody tr:not(:first-child)"));
                                int counter = 1;
                                foreach (IWebElement player_elem in players)
                                {
                                    Player player = new Player();
                                    HLTVObject hltvObject = new HLTVObject();
                                    hltvObject.Type = "player";
                                    player.Nationality = hltvObject.Nationality = player_elem.FindElement(By.CssSelector("img[class=\"flag\"]")).GetAttribute("title");
                                    string fullName = player_elem.FindElement(By.CssSelector("a[href^=\"/player\"]")).Text;
                                    player.InGameName = hltvObject.InGameName = Regex.Replace(fullName, @"\w*\s'|'\s\w*.", "");
                                    player.Name = hltvObject.Name = Regex.Replace(fullName, @"\s'\w*'\s", " ");
                                    player.Link = hltvObject.Link = player_elem.FindElement(By.CssSelector("a[href^=\"/player\"]")).GetAttribute("href");
                                    Console.WriteLine($"Player {counter}---------------------------------------------------------------\n" +
                                        $"Nationality: {player.Nationality}\nName: {player.Name}\nIn Game Name: {player.InGameName}\nLink: {player.Link}");
                                    counter++;
                                    hltvObjectList.Add(hltvObject);
                                }
                                counter = 1;
                                Console.WriteLine("\n");
                            }
                            else if (element.FindElement(By.CssSelector("td[class=\"table-header\"]")).Text == "Team")
                            {
                                ReadOnlyCollection<IWebElement> teams = element.FindElements(By.CssSelector("tbody tr:not(:first-child)"));
                                int counter = 1;
                                foreach (IWebElement team_elem in teams)
                                {
                                    Team team = new Team();
                                    HLTVObject hltvObject = new HLTVObject();
                                    hltvObject.Type = "Team";
                                    team.Name = hltvObject.Name = team_elem.FindElement(By.CssSelector("a[href^=\"/team\"]")).Text;
                                    team.Link = hltvObject.Link = team_elem.FindElement(By.CssSelector("a[href^=\"/team\"]")).GetAttribute("href");
                                    Console.WriteLine($"Team {counter}---------------------------------------------------------------\n" +
                                        $"Team: {team.Name}\nLink: {team.Link}");
                                    counter++;
                                    hltvObjectList.Add(hltvObject);
                                }
                                counter = 1;
                                Console.WriteLine("\n");
                            }
                            else if (element.FindElement(By.CssSelector("td[class=\"table-header\"]")).Text == "Event")
                            {
                                ReadOnlyCollection<IWebElement> events = element.FindElements(By.CssSelector("tbody tr:not(:first-child)"));
                                int counter = 1;
                                foreach (IWebElement event_elem in events)
                                {
                                    Event event_ = new Event();
                                    HLTVObject hltvObject = new HLTVObject();
                                    hltvObject.Type = "Event";
                                    event_.Name = hltvObject.Name = event_elem.FindElement(By.CssSelector("a[href^=\"/events\"]")).Text;
                                    event_.Link = hltvObject.Link = event_elem.FindElement(By.CssSelector("a[href^=\"/events\"]")).GetAttribute("href");
                                    Console.WriteLine($"Event {counter}---------------------------------------------------------------\n" +
                                        $"Event: {event_.Name}\nLink: {event_.Link}");
                                    counter++;
                                    hltvObjectList.Add(hltvObject);
                                }
                                counter = 1;
                                Console.WriteLine("\n");
                            }
                            else if (element.FindElement(By.CssSelector("td[class=\"table-header\"]")).Text == "Article")
                            {
                                ReadOnlyCollection<IWebElement> articles = element.FindElements(By.CssSelector("tbody tr:not(:first-child)"));
                                int counter = 1;
                                foreach (IWebElement article_elem in articles)
                                {
                                    Article article = new Article();
                                    HLTVObject hltvObject = new HLTVObject();
                                    hltvObject.Type = "Article";
                                    article.Region = hltvObject.Region = article_elem.FindElement(By.CssSelector("img[class=\"flag\"]")).GetAttribute("title");
                                    article.Title = hltvObject.Title = article_elem.FindElement(By.CssSelector("a[href^=\"/news\"]")).Text;
                                    article.PublishDate = hltvObject.PublishDate = article_elem.FindElement(By.CssSelector("span[data-time-format=\"y-MM-dd\"]")).Text;
                                    article.Author = hltvObject.Author = article_elem.FindElement(By.CssSelector("a[href^=\"/profile\"]")).Text;
                                    article.Link = hltvObject.Link = article_elem.FindElement(By.CssSelector("a[href^=\"/news\"]")).GetAttribute("href");
                                    Console.WriteLine($"Article {counter}---------------------------------------------------------------\n" +
                                        $"Region: {article.Region}\nTitle: {article.Title}\nPublish date: {article.PublishDate}\nAuthor: {article.Author}\nLink: {article.Link}\n");
                                    counter++;
                                    hltvObjectList.Add(hltvObject);
                                }
                                counter = 1;
                                Console.WriteLine("\n");
                            }
                            Console.WriteLine(hltvObjectList.Count);
                        }
                        using (var writer = new StreamWriter("hltv.csv"))
                        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                        {
                            csv.WriteRecords(hltvObjectList);
                        }
                        PrettyWrite(hltvObjectList, "hltv.json");
                    }
                }
                catch
                {
                    Console.WriteLine("---------------------------------------------------------------\n" +
                       "Something went wrong.\n" +
                       "---------------------------------------------------------------");
                }
                finally
                {
                    driver.Quit();
                }
                Console.WriteLine("Action finished");
            }
        }
    }
}