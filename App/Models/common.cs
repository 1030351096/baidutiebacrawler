using AppModul;
using HtmlAgilityPack;
using PetaPoco;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace App.Models
{
    public class common
    {
        private static readonly Stopwatch watch = new Stopwatch();

        /// <summary>
        /// 异步获取json内容
        /// </summary>
        /// <param name="typeid"></param>
        /// <param name="pagecount"></param>
        /// <returns></returns>
        public static async Task<string> Getpagecontext(int? typeid, int pagecount = 1)
        {
            string url = $"http://101.201.42.62:5664/api/ShopViews/GetShopView?pagecount={pagecount}&pageshu=200&typeid=";
            var client = new RestClient(url);
            var request = new RestRequest(Method.GET);
            request.AddHeader("postman-token", "91a964e1-7e0f-c05e-13a8-410abdd4253a");
            request.AddHeader("cache-control", "no-cache");
            IRestResponse response = await client.ExecuteGetTaskAsync(request);
            return response?.Content;
        }
        /// <summary>
        /// 异步写入json文件内容
        /// </summary>
        /// <param name="page"></param>
        /// <param name="jsoncontext"></param>
        public static async void writejsonfile(int page, Task<string> jsoncontext)
        {
            string path = @"C:\Users\ASUS\Desktop\App\App\bin\Debug\json";
            string newpath = System.IO.Path.Combine(path, page.ToString());
            System.IO.Directory.CreateDirectory(newpath);
            await Task.Run(() => writefile(newpath, jsoncontext));
        }
        /// <summary>
        /// 写入json文件内容
        /// </summary>
        /// <param name="newpath"></param>
        /// <param name="jsoncontext"></param>
        public static void writefile(string newpath, Task<string> jsoncontext)
        {
            System.IO.File.WriteAllText(System.IO.Path.Combine(newpath, "json.json"), jsoncontext.Result, Encoding.UTF8);
        }

        public static async void GetTieba(int page = 0)
        {
            var httpResult = Task.Run(() => new HttpHelper().GetHtml(new HttpItem()
            {
                URL = $"https://tieba.baidu.com/f?kw=%E6%8A%97%E5%8E%8B&ie=utf-8&pn={page}"
            })
            );
            if (httpResult.Result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                int pagecount = (page / 50 + 1);
                await Task.Run(() => clearHtmlattrbute(httpResult.Result.Html));
                Console.ForegroundColor = ConsoleColor.Green;
                if (page == 0)
                {
                    Console.WriteLine($"已添加完第1页数据");
                }
                else
                {
                    Console.WriteLine($"已添加完第{pagecount}页数据");
                }
                Thread.Sleep(2000);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("休息两秒。。。");
                watch.Start();
                GetTieba(page + 50);
                Console.WriteLine($"已爬取完第{pagecount}页数据耗时为，{watch.ElapsedMilliseconds}");
                watch.Reset();
            }
            else
            {
                Console.WriteLine($"请求失败，请检查接口状态码...：{httpResult.Result.StatusCode}");
            }
        }

        public static void clearHtmlattrbute(string html)
        {
            HtmlDocument hd = new HtmlDocument();
            hd.LoadHtml(html);
            var list = hd.DocumentNode.SelectNodes(".//div[@class='col2_right j_threadlist_li_right ']");
            foreach (var item in list)
            {
                var Tid = item.SelectSingleNode("./div[1]/div[1]/a[1]").Attributes["href"].Value;
                Tid = Tid.Substring(3, Tid.Length - 3);//贴吧ID
                if (Tid.Length == 10)
                {
                    using (DbcontextDB db = new DbcontextDB())
                    {
                        var IsNew = db.FirstOrDefault<Tieba>("where Tid=@Tid", new { Tid = Tid });
                        if (IsNew == null)
                        {
                            var modul = Task.Run(() => GetTieba(item));
                            modul.Result.Tid = Tid;
                            var IsAdd = Task.Run(() => AddTieba(modul.Result));
                            if (!IsAdd.Result)
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine($"标题：{modul.Result.title}添加失败!");

                            }
                        }
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Tid 有误，请查看");
                }
            }
        }

        public static bool AddTieba(Tieba T)
        {
            DbcontextDB db = new DbcontextDB();
            return Convert.ToInt32(db.Insert(T)) > 0;
        }

        public static Tieba GetTieba(HtmlNode item)
        {
            var username = item.SelectSingleNode("./div[1]/div[2]/span").Attributes["title"].Value;
            username = username.Substring(6, username.Length - 6);//用户名
            var userid = item.SelectSingleNode("./div[1]/div[2]/span").Attributes["data-field"].Value;
            userid = userid.Substring(21, userid.Length - 22);//用户id
            var title = item.SelectSingleNode("./div[1]/div[1]/a[1]").InnerText;//标题
            return new Tieba()
            {
                title = title,
                userid = userid,
                username = username
            };
        }

        public static IEnumerable<Tieba> tieba(string title,string name)
        {
            DbcontextDB db = new DbcontextDB();
            Sql sql= Sql.Builder.Select("*");
            sql.From("Tieba");
            if (!string.IsNullOrEmpty(name))
            {
                sql.Where("username=@0", name);
            }
            if (!string.IsNullOrEmpty(title))
            {
                sql.Where("title like @0","%"+title+"%");
            }
            string Tsql=sql.ToString();


            return db.Query<Tieba>(sql);;
        }


    }
}
