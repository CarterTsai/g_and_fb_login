using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using g_and_fb.Models;
using System.Net;
using System.Net.Http;
using System.IO;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;

namespace g_and_fb.Controllers
{
   
    public class HomeController : Controller
    {
        private string clientid = "";
        public HomeController(IConfiguration configuration) 
        {
            clientid = configuration["GooglePlus:client_id"];
        }
        public IActionResult Index()
        {
            return View();
        }

        public class ReturnObj {
            public int StatusCode {get; set;}
            public string Message {get; set;}
        }

        public string GetWebResponse(string uri, HttpMethod method, string authorization = null)
        {
            WebRequest request = WebRequest.Create(uri);
            request.Method = method.Method;

            if (authorization != null)
            {
                request.Headers.Add("Authorization", authorization);
            }

            try
            {
                using (var response = request.GetResponse())
                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    return sr.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e);
                return "";
            }
        }

        public IActionResult LoginWithFacebook(string accessToken)
        {
            var jsonResult = this.GetWebResponse("https://graph.facebook.com/v3.0/me?access_token=" + accessToken + "&fields=id%2Cname%2Cemail%2Cgender%2Cbirthday%2Cpicture&format=json&method=get&pretty=0&suppress_http_code=1", HttpMethod.Get);
            if (!String.IsNullOrEmpty(jsonResult))
            {
                var response = JValue.Parse(jsonResult);
                var error = response.Value<JObject>("error");

                if (error != null)
                {
                    this.ViewBag.Message = error.Value<string>("message");
                    return this.View();
                }
                else 
                {
                     Dictionary<string, string> dc = new Dictionary<string, string>();

                    var userId = response.Value<string>("id");
                    dc.Add("id", userId);

                    var userName = response.Value<string>("name");
                    dc.Add("name", userName);

                    var userEmail = response.Value<string>("email");
                    var userBirthday = response.Value<string>("birthday");

                }
            }
            return View();
        }

        public JsonResult LoginWithGooglePlus(string accessToken)
        {
            // {
            //     "emails": [
            //         {
            //             "value": "",
            //             "type": "account"
            //         }
            //     ],
            //     "id": "",
            //     "displayName": ""
            // }
            var resultData = new ReturnObj{};
            
            var clientId =  clientid;
            var jsonResult = this.GetWebResponse("https://www.googleapis.com/plus/v1/people/me?fields=displayName%2Cemails%2Cid%2Cbirthday%2Cgender&key=" + clientId, HttpMethod.Get, "Bearer " + accessToken);
            System.Console.WriteLine(jsonResult);
            if (!String.IsNullOrEmpty(jsonResult))
            {
                var response = JValue.Parse(jsonResult);
                var error = response.Value<JObject>("error");

                if (error != null)
                {
                    resultData.StatusCode = 400;
                    resultData.Message = error.Value<string>("message");
                }
                else
                {
                    Dictionary<string, string> dc = new Dictionary<string, string>();

                    var userId = response.Value<string>("id");
                    dc.Add("id", userId);

                    var userName = response.Value<string>("displayName");
                    dc.Add("name", userName);

                    var userEmail = response.Value<JArray>("emails").First().Value<string>("value");
                    var userBirthday = response.Value<string>("birthday");
                    System.Console.WriteLine($"userEmail {userEmail}");
                    System.Console.WriteLine($"userEmail {userEmail}");
                    string userBirthYear = string.Empty;
                    string userBirthMonth = string.Empty;
                    string userBirthDay = string.Empty;

                    if (!string.IsNullOrEmpty(userBirthday))
                    {
                        DateTime dt = Convert.ToDateTime(userBirthday);

                        userBirthYear = dt.Year.ToString();
                        userBirthMonth = dt.Month.ToString();
                        userBirthDay = dt.Day.ToString();
                        System.Console.WriteLine($"Birthday {userBirthYear} {userBirthMonth} {userBirthDay}");
                    }
                }
            }


            return this.Json(resultData);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
