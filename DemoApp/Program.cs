using ElasticCommon;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using SearchRequest = ElasticCommon.Models.SearchRequest;

namespace ElasticCurl
{
    class Program
    {
        private static readonly string TemplateIndexName = ConfigurationManager.AppSettings["TemplateIndexName"];
        private static readonly string SuggestIndexName = ConfigurationManager.AppSettings["SuggestIndexName"];
        private static readonly string ElasticClusterUri = ConfigurationManager.AppSettings["ElasticClusterUri"];
        private static readonly string ElasticAdminUserName = ConfigurationManager.AppSettings["ElasticAdminUserName"];
        private static readonly string ElasticAdminPassword = ConfigurationManager.AppSettings["ElasticAdminPassword"];

        static void Main(string[] args)
        {
            var connector = new ElasticConnector(TemplateIndexName, SuggestIndexName);

            var client = connector.GetClient(new[] { ElasticClusterUri }, ElasticAdminUserName, ElasticAdminPassword);

            Console.WriteLine("Search Templates (T){pattern : [query],[filterField],[filterString]} or Suggestions (S){pattern ; [query]}?");

            // sample query
            // 3rd grade multiplication activity,tagSubjectsPrefixed,Math: Basic Operations

            var searchFor = "T"; // Console.ReadLine();

            Console.WriteLine(searchFor);

            Console.WriteLine("What to search for?");

            var query = "3rd grade multiplication activity,tagSubjectsPrefixed,Math: Basic Operations"; // Console.ReadLine();
            Console.WriteLine(query);

            if (string.Equals("S", searchFor, StringComparison.OrdinalIgnoreCase))
            {

                var filters = new Dictionary<string, List<string>>();

                var response = connector.GetSuggestions(client, new SearchRequest
                {
                    Query = query,
                    Filters = filters,
                    PageSize = 20,
                    MinScore = 0.5,
		    Order = ElasticCommon.Models.SearchOrdering.popular
                }).Result;

                Console.WriteLine("Total records {0}", response.Count);

                foreach (var groups in response.Results.GroupBy(x => x.ValueType))
                {
                    foreach (var data in groups.OrderByDescending(x => x.Score))
                    {
                        Console.WriteLine(data.ValueType + "   " + data.Value + "   " + data.Score);
                    }

                    Console.WriteLine();
                }

                Console.WriteLine("Time taken to search Mill Seconds {0}", TimeSpan.FromTicks(response.Ticks).Milliseconds);
            }

            if (string.Equals("T", searchFor, StringComparison.OrdinalIgnoreCase))
            {
                var filters = new Dictionary<string, List<string>>();
                var parts = query.Split(',');
                var filterField = parts[1];
                var filterString = parts[2];

                filters.Add(filterField, new List<string> { filterString });

                var response = connector.GetTemplates(client, new SearchRequest
                {
                    Query = parts[0],
                    Filters = filters,
                    PageSize = 18,
                    MinScore = 0.1,
                    CurrentPage = 0,
		    Order = ElasticCommon.Models.SearchOrdering.relevant
                }).Result;

                Console.WriteLine("Total records {0}", response.Count);

                foreach (var data in response.Results.OrderByDescending(x => x.Score))
                {
                    Console.WriteLine("\n------------------------------------------\n");
                    Console.WriteLine("Title    {0}", data.Title);
                    Console.WriteLine("Desc     {0}", data.Desc);
                    Console.WriteLine("Authr    {0}", data.By);
                    //Console.WriteLine("TmplCode {0}", data.TmplCode);
                    Console.WriteLine("TmplTags {0}", string.Join(", ", data.TmplTags));
                    Console.WriteLine("TmplCcss {0}", string.Join(", ", data.TmplCcss));
                    //Console.WriteLine("TmplTyps {0}", string.Join(", ", data.TmplTypes));
                    //Console.WriteLine("InsAuthr {0}", data.InsAuthor);
                    Console.WriteLine("Score    {0}", data.Score);
                    Console.WriteLine("Smily    {0}", data.ClonedCnt + data.DownloadCnt);
                    Console.WriteLine("Created  {0}", data.LstDt);
                    //Console.WriteLine("Featured {0}", data.IsFeatured);
                    //Console.WriteLine("Deleted  {0}", data.Deleted);

                    //Console.WriteLine(JsonConvert.SerializeObject(data));
                    Console.WriteLine();
                }

                Console.WriteLine("Time taken to search Mill Seconds {0}", TimeSpan.FromTicks(response.Ticks).Milliseconds);
            }

            Console.ReadLine();
        }
    }
}
