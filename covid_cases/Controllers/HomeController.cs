using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

using covid_cases.DTO;

namespace covid_cases.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            //Create request to site.
            string url = "https://api.covid19api.com/summary";
            WebRequest request = WebRequest.Create(url);
            request.Method= "GET";
            //Get and read response stream from request, storing value in string.
            string responseContent;
            using(HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            using(Stream responseStream = response.GetResponseStream())
            using(StreamReader reader = new StreamReader(responseStream)){
                responseContent = reader.ReadToEnd();
            }                                           // The using statements allows for an earlier garbage collection
                                                        // by making the data extraction from a response a separate scope 
                                                        // from the later code.

            //Get all countries from response into list of Country elements.
            List<Country> countries = fillCountryListFromResponse(ref responseContent);
            
            //Sort and extract 20 countries with highest total number of cases.
            countries.Sort((x,y) => y.TotalConfirmed.CompareTo(x.TotalConfirmed));
            List<Country> topTwenty = countries.Take(20).ToList();

            //Pass data to View and return view.
            ViewData["Countries"]= topTwenty;
            return View();
        }

        //Method using a string containing response to create and return a list of countries with total number of cases.
        private List<Country> fillCountryListFromResponse(ref string content)
        {
            List<Country> countries = new List<Country>();
            string searchString = "\"Countries\"";
            int cutIndex = content.IndexOf(searchString);
            content = content.Remove(0, cutIndex);
            while (true){
                cutIndex = content.IndexOf("}},");
                string countryStr;
                if(cutIndex>=0){
                    countryStr = content.Remove(cutIndex);
                    content = content.Remove(0,cutIndex+3);
                }else{
                    countryStr=content;
                }
                Country country = new Country();
                searchString="Country\":\"";
                country.Name = getInfoFromString(searchString, ref countryStr);
                if(country.Name.Equals("Viet Nam")){country.Name="Vietnam";}        //Some countries have incorrect names
                if(country.Name.Equals("Iran, Islamic Republic of")){country.Name="Islamic Republic of Iran";}
                searchString="TotalConfirmed\":";
                country.TotalConfirmed = Int64.Parse(getInfoFromString(searchString, ref countryStr, ",\""));
                countries.Add(country);
                if(cutIndex<0)
                    break;
            }  
            return countries;
        }
        //Method finding and returning a string sequence after the end of search string and until the ending is reached 
        //in a passed data string. Data contents up to the found sequence are also removed.
        private string getInfoFromString(string search, ref string data, string ending = "\",\"")
        {
            string result;
            int searchIndex = 0;
            searchIndex = data.IndexOf(search);
            if(searchIndex<-1){
                return null;
            }
            data = data.Remove(0,searchIndex+search.Length);
            searchIndex = data.IndexOf(ending);
            result = data.Remove(searchIndex);
            data = data.Remove(0,searchIndex);
            return result;
        }
    }
}
