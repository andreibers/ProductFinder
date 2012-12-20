using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Globalization;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace ProductFinder
{
    class Program
    {
        static void Main(string[] args)
        {
            //this is where the products will be stored
            Dictionary<string, List<string>> products = new Dictionary<string, List<string>>();
            //this is for recalling the product name later
            Dictionary<string, string> recall = new Dictionary<string, string>();
            //this is associating a product and the found listings
            Dictionary<string, List<string>> results = new Dictionary<string, List<string>>();

            if (!File.Exists("listings.txt") || !File.Exists("products.txt"))
            {
                Console.WriteLine("listings.txt or products.txt not found. Quitting program.");
                Console.ReadLine();
                return;
            }

            string line;
            Product product;

            //builds the dictionaries per manufacturer and product names
            System.IO.StreamReader fileProducts = new System.IO.StreamReader("products.txt");
            while ((line = fileProducts.ReadLine()) != null)
            {
                product = JsonConvert.DeserializeObject<Product>(line);
                             
                product.model = product.model.ToLower().Trim();

                //save the original product name, used for the key in the dictionary
                string productNameOriginal = product.product_name;

                //process product names replace all instances of -_/ with empty strings, and put it to lower cased
                product.product_name = product.product_name.ToLower().Replace("-", string.Empty).Replace(" ", string.Empty).Replace("_", string.Empty).Replace("/", string.Empty);
                product.manufacturer = product.manufacturer.ToLower();
                
                List<string> list;
                //build up the products dictionary
                if (!products.ContainsKey(product.manufacturer))
                {
                    list = new List<string>();
                    list.Add(product.product_name);                   
                    recall.Add(product.product_name, productNameOriginal);
                    products.Add(product.manufacturer, list);
                }
                else
                {
                    list = products[product.manufacturer];

                    if (!list.Contains(product.product_name))
                    {
                        list.Add(product.product_name);
                        recall.Add(product.product_name, productNameOriginal);
                    }
                }
            }

            //now we start processing the product listings
            TextWriter tw = new StreamWriter("output.txt");

            System.IO.StreamReader fileListings = new System.IO.StreamReader("listings.txt");

            string line2 = string.Empty;

            while ((line2 = fileListings.ReadLine()) != null)
            {
                Listing listing = JsonConvert.DeserializeObject<Listing>(line2);
                listing.manufacturer = listing.manufacturer.ToLower();

                //first we see if the manufacturer in the listing we have in our products dictionary
                if (products.ContainsKey(listing.manufacturer))
                {
                    //do the same thing with the listing as we did with product name, strip -_/ we don't need these characters
                    string productListing = listing.title.ToLower().Replace("-", "").Replace(" ", "").Replace("_", "").Replace("/", string.Empty);

                    //get the list of products associated with this manufacturer
                    List<string> products2 = products[listing.manufacturer];

                    //loop through each product and see if we find this key in the listing
                    foreach (string p in products2)
                    {
                        if (productListing.Contains(p))
                        {
                            //we found the key, add this liting to the result
                            List<string> matches;

                            if (!results.ContainsKey(p))
                            {
                                matches = new List<string>();
                                matches.Add(line2);

                                results.Add(p, matches);
                                break;
                            }
                            else
                            {
                                matches = results[p];
                                matches.Add(line2);
                                break;
                            }
                        }
                    }
                }
            }
            
            //write out the json file here
            foreach (string key in results.Keys)
            {
                List<string> list = results[key];

                string results2 = string.Empty;

                foreach (string s in list)
                {                    
                    if (results2 != string.Empty)
                        results2 += ",";

                    results2 += s;
                }

                tw.Write("{\"product_name\": \"" + recall[key] + "\", \"listings\": [" + results2 + "]} \n");
            }

            tw.Close();

            Console.WriteLine("Processing done! Results written to: output.txt");
            Console.ReadLine();
        }
    }
}
