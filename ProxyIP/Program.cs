using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxyIP
{
    class Program
    {
        static void Main(string[] args)
        {
            //List<IPProxy> list= IpProxyGet.ParseProxy("127.0.0.1");

            HashSet<string> list = IpProxyGet.Grab_ProxyIp();

            //foreach (var item in list)
            //{
            //    Console.WriteLine("%s", item.IP);
            //    Console.ReadKey();
            //}
            Console.ReadKey();
        }
    }
}
