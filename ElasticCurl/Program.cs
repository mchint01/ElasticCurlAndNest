using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Nest;

namespace ElasticCurl
{
    class Program
    {
        static void Main(string[] args)
        {
            //if (!CheckIfIndexExists("customer").Result)
            //{
            //    CreateIndex("customer").Wait();
            //}

            Console.WriteLine("What to search for?");
            var query = Console.ReadLine();

            var connector = new ElasticConnector();

            var client = connector.GetClient();

            var results = connector.GetSuggestions(client, new Models.SuggestionRequest {Query = query }).Result;

            foreach (var r in results)
            {
                Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(r));
            }

            Console.ReadLine();
        }

        private static async Task<bool> CheckIfIndexExists(string indexName)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://113121f8664ccf8c8a10dccd10132cd7.us-east-1.aws.found.io:9200/");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await client.GetAsync(indexName);

                return response.IsSuccessStatusCode;
            }
        }

        private static async Task CreateIndex(string indexName)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://113121f8664ccf8c8a10dccd10132cd7.us-east-1.aws.found.io:9200/");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await client.PutAsync(indexName + "?pretty", null);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Index creation failed.");
                }
            }

        }
    }
}
