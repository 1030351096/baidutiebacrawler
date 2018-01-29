using App.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace App
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //watch.Start();
            //int page = 1;
            //while (true)
            //{
            //    var jsoncontext = common.Getpagecontext(typeid: null, pagecount: page);
            //    if (jsoncontext.Result == "[]")
            //    {
            //        Console.WriteLine($"已存储全部数据。。。,耗时为{watch.ElapsedMilliseconds}");
            //        break;
            //    }
            //    else
            //    {
            //        common.writejsonfile(page, jsoncontext);
            //        page++;
            //    }
            //}
            common.GetTieba(50);
            Console.Read();
        }



    }
}
