using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Nest;
using TsElasticCommon;
using TsElasticCommon.Models;

namespace ElasticCurl
{
    class Program
    {
        static void Main(string[] args)
        {
            var connector = new ElasticConnector();

            var client = connector.GetClient();

            Console.WriteLine("Search Templates (T) or Suggestions (S)?");

            var searchFor = Console.ReadLine();

            Console.WriteLine("What to search for?");

            var query = Console.ReadLine();

            if (string.Equals("S", searchFor, StringComparison.OrdinalIgnoreCase))
            {
                var response = connector.GetSuggestions(client, new TsSearchRequest
                {
                    Query = query,
                    PageSize = 10,
                    MinScore = 0.5
                }).Result;

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
                var response = connector.GetTemplates(client, new TsSearchRequest
                {
                    Query = query,
                    PageSize = 10,
                    MinScore = 0.5
                }).Result;

                foreach (var data in response.Results.OrderByDescending(x=>x.Score))
                {
                    Console.WriteLine(data);
                    Console.WriteLine();
                }

                Console.WriteLine("Time taken to search Mill Seconds {0}", TimeSpan.FromTicks(response.Ticks).Milliseconds);
            }

            Console.ReadLine();
        }
    }
}
