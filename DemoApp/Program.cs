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
            Console.WriteLine("What to search for?");

            var query = Console.ReadLine();

            var connector = new ElasticConnector();

            var client = connector.GetClient();

            var response = connector.GetSuggestions(client, new TsSearchRequest
            {
                Query = query,
                PageSize = 10,
                MinScore = 0.5
            }).Result;

            foreach (var groups in response.Results.GroupBy(x=>x.ValueType))
            {
                foreach (var data in groups.OrderByDescending(x=>x.Score))
                {
                    Console.WriteLine(data.ValueType + "   " + data.Value + "   " + data.Score);
                }

                Console.WriteLine();
            }

            Console.WriteLine("Time taken to search Mill Seconds {0}", TimeSpan.FromTicks(response.Ticks).Milliseconds);

            Console.ReadLine();
        }
    }
}
