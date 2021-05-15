using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Common.Elastic.Interfaces;
using Common.Elastic.Models;
using Newtonsoft.Json;

namespace Common.Elastic
{
    internal class JsonHelper : IJsonHelper
    {
        public async Task<string> GetJsonContentAsync(string fileName)
        {
            var assembly = typeof(JsonHelper).Assembly;

            var stream = assembly.GetManifestResourceStream(fileName);

            if (stream == null)
                return null;

            var reader = new StreamReader(stream);

            return await reader.ReadToEndAsync();
        }

        public async Task<string> CreateBulkJsonContentAsync(List<SuggestionIndex> list)
        {
            using (var ms = new MemoryStream())
            {
                var sw = new StreamWriter(ms);

                foreach (var item in list)
                {
                    await sw.WriteLineAsync(JsonConvert.SerializeObject(new Bulk.Index(item.Id)));
                    await sw.WriteLineAsync(JsonConvert.SerializeObject(item));
                }

                await sw.FlushAsync();

                return Encoding.ASCII.GetString(ms.ToArray());
            }
        }

        public async Task<string> CreateBulkJsonContentAsync(List<SearchIndex> list)
        {
            using (var ms = new MemoryStream())
            {

                var sw = new StreamWriter(ms);

                foreach (var item in list)
                {
                    await sw.WriteLineAsync(JsonConvert.SerializeObject(new Bulk.Index(item.Id)));
                    await sw.WriteLineAsync(JsonConvert.SerializeObject(item));
                }

                await sw.FlushAsync();

                return Encoding.ASCII.GetString(ms.ToArray());
            }
        }
    }
}
