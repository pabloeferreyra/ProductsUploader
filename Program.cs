using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace ProductsUploader
{
    class Program
    {
        const string bar = "------------------------------------------";
        public class Product
        {
            public string Id { get; set; }
            public string sku { get; set; }
            public string regular_price { get; set; }
            public int stock_quantity { get; set; }
        }

        public class ProductOne
        {
            public string sku { get; set; }
            public string regular_price { get; set; }
            public int stock_quantity { get; set; }
        }

        static async Task<int> UpdateProductAsync(Product product)
        {
            ProductOne p = new ProductOne
            {
                sku = product.sku,
                regular_price = product.regular_price,
                stock_quantity = product.stock_quantity
            };

            string url = String.Format("https://alonsoinformatica.com.ar/wp-json/wc/v2/products/");

            HttpMessageHandler handler = new HttpClientHandler()
            {
            };

            HttpClient client = new HttpClient(handler)
            {
                BaseAddress = new Uri(url),
                Timeout = new TimeSpan(0, 2, 0)
            };


            var plainTextBytes = Encoding.UTF8.GetBytes("test:test");
            string val = Convert.ToBase64String(plainTextBytes);
            client.DefaultRequestHeaders.Add("Authorization", "Basic " + val);
            StringContent content = new StringContent(JsonConvert.SerializeObject(p), Encoding.UTF8, "application/json");
            await client.PostAsync(url + product.Id, content);
            
            return int.Parse(product.Id);
        }

        static async Task Main(string[] args)
        {
            Header();
            List<Product> lines = new List<Product>();
            try
            {
                string path = Directory.GetCurrentDirectory();
                string filename = path + @"\Alonso Informatica - CSV.csv";
                Console.Write("Archivo a leer:\n");
                Console.Write(filename+ "\n");
                Console.Write("Press <Enter> to start... ");
                while (Console.ReadKey().Key != ConsoleKey.Enter) { }
                using (var fs = new StreamReader(filename))
                {
                    // I just need this one line to load the records from the file in my List<CsvLine>
                    lines = new CsvReader((IParser)fs).GetRecords<Product>().ToList();
                }
                foreach (var product in lines)
                {
                    Console.WriteLine(await UpdateProductAsync(product));
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
