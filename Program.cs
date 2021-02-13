using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using CsvHelper.Configuration;
using System.Globalization;

namespace ProductsUploader
{
    class Program
    {
        const string bar = "------------------------------------------";
        public class Product
        {
            [JsonPropertyName("id")]
            public string Id { get; set; }
            [JsonPropertyName("sku")]
            public string sku { get; set; }
            [JsonPropertyName("regular_price")]
            public string regular_price { get; set; }
            [JsonPropertyName("stock_quantity")]
            public int stock_quantity { get; set; }
        }

        public class ProductOne
        {
            public string SKU { get; set; }
            public int STOCK { get; set; }
            public string PRECIO { get; set; }
        }

            static async Task<int> UpdateProductAsync(Product product, string val, string url)
            {
            HttpMessageHandler handler = new HttpClientHandler()
            {
            };

            HttpClient client = new HttpClient(handler)
            {
                BaseAddress = new Uri(url),
                Timeout = new TimeSpan(0, 2, 0)
            };

            client.DefaultRequestHeaders.Add("Authorization", "Basic " + val);
            StringContent content = new StringContent(JsonConvert.SerializeObject(product), Encoding.UTF8, "application/json");
            await client.PostAsync(url + product.Id, content);
            
            return int.Parse(product.Id);
        }

        static async Task<dynamic> GetProductsAsync(string sku, string val, string url)
        {

            

            HttpMessageHandler handler = new HttpClientHandler()
            {
            };

            HttpClient client = new HttpClient(handler)
            {
                BaseAddress = new Uri(url),
                Timeout = new TimeSpan(0, 2, 0)
            };

            client.DefaultRequestHeaders.Add("Authorization", "Basic " + val);
            string s = await client.GetStringAsync(url+ "?sku="+sku);
            var p = JsonConvert.DeserializeObject<dynamic>(s);

            return p;
        }

        static async Task Main(string[] args)
        {
            Header();
            try
            {
                string path = Directory.GetCurrentDirectory();
                string filename = path + @"\Alonso Informatica - CSV.csv";
                Console.Write("Archivo a leer:\n");
                Console.Write(filename+ "\n");
                Console.Write("Press <Enter> to start... ");
                while (Console.ReadKey().Key != ConsoleKey.Enter) { }
                string url = String.Format("https://alonsoinformatica.com.ar/wp-json/wc/v3/products/");
                var plainTextBytes = Encoding.UTF8.GetBytes("test:test");
                string val = Convert.ToBase64String(plainTextBytes);
                using (var stream = new MemoryStream())
                using (var fs = new StreamReader(filename))
                {
                    // I just need this one line to load the records from the file in my List<CsvLine>
                    using (var csvReader = new CsvReader(fs, CultureInfo.InvariantCulture))
                    {
                        while (csvReader.Read())
                        {
                            try
                            {
                                var records = csvReader.GetRecord<ProductOne>();
                                var prod = await GetProductsAsync(records.SKU, val, url);
                                var value = ((Newtonsoft.Json.Linq.JValue)((Newtonsoft.Json.Linq.JContainer)((Newtonsoft.Json.Linq.JContainer)((Newtonsoft.Json.Linq.JContainer)prod).First).First).First).Value;
                                Console.WriteLine("id Producto " + value);
                                Product productFinished = new Product
                                {
                                    Id = value.ToString(),
                                    regular_price = records.PRECIO,
                                    sku = records.SKU,
                                    stock_quantity = records.STOCK
                                };

                                Console.WriteLine(await UpdateProductAsync(productFinished, val, url));
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                                Console.Write("Press <Enter> to continue... ");
                                while (Console.ReadKey().Key != ConsoleKey.Enter) { }
                            }
                        }
                    }
                }
                

                Footer();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Footer();
                Console.Write("Press <Enter> to exit... ");
                while (Console.ReadKey().Key != ConsoleKey.Enter) { }
            }
            
        }

        private static void Header()
        {
            Console.WriteLine(bar);
            Console.WriteLine("[Uploader] A simple CSV Reader And ProductUploader");
            Console.WriteLine(bar);
        }

        private static void Footer()
        {
            Console.WriteLine("\r\n" + bar);
            Console.WriteLine("Bye!");
            Console.WriteLine(bar);
            Console.Write("Press <Enter> to exit... ");
            while (Console.ReadKey().Key != ConsoleKey.Enter) { }
        }
    }
}
