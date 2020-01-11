using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net.Http;
using System.Threading;
using System.IO;
using System.Diagnostics;

namespace PLCorp.WebUtils
{
    public class NetworkObject
    {
        Socket clientListener { get; set; }
        public static int i = 1;
        public static string MyThreadName = Thread.CurrentThread.Name;
        public Socket ClientListener { get { return clientListener; } }
        public string SocketTxt = "Socket " + i.ToString() + ": Running on thread: " + MyThreadName;
       
        public string MyAddress = "";
        public string myIPAddress { get; set; }

        public NetworkObject() { }

        public NetworkObject(string host)
        {
            MyAddress = host;
        }

        
        public async Task<string> GetHttpPage() //Function to read from given url
        {
            HttpClient client = new HttpClient();
            string url = "http://" + MyAddress;
            HttpResponseMessage response = await client.GetAsync(url);
            HttpResponseMessage v = new HttpResponseMessage();
            return await response.Content.ReadAsStringAsync();
        }


        public Socket GetSocketOnHomePage()
        {
            // create socket
            clientListener = new Socket(SocketType.Stream, ProtocolType.IP);

           

            try
            {
                ClientListener.Connect(MyAddress, 80);
                myIPAddress = "IP Address = " + ClientListener.RemoteEndPoint.ToString();
                i++;
                

            }
            catch (Exception ex)
            {
                Debug.WriteLine("Could not obtain socket: " + ex.Message);
            }
            
            return ClientListener;

        }


        public List<string> GetPage(Socket source, string RelativeUrl)
        {
            StringBuilder sb = new StringBuilder();
            string buffer;
            List<string> result = new List<string>();
            var sbuilder = new StringBuilder();


            var rStream = new NetworkStream(source);

            NetRequest.Send(rStream, new[] { "GET " + RelativeUrl + " HTTP/1.1", "Host: " + MyAddress });
            try
            {
                do
                {
                    buffer = null;
                    buffer = NetRequest.ReadLine(rStream);
                    if (buffer != null && buffer.Length > 0)
                        sb.Append(buffer);
                } while (buffer != null && buffer.Length > 0);
                result.Add(SocketTxt);
                result.Add("Socket connected to: " + MyAddress);
                result.Add(sb.ToString());
                return result;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                result.Add(e.Message);
                return result;
            }


        }



    }
    public static class NetRequest
    {
        private static readonly Encoding DefaultEncoding = Encoding.ASCII;
        private static readonly byte[] LineTerminator = new byte[] { 13, 10 };



        public static void Send(Stream s, IEnumerable<string> msg)
        {
            foreach (var seg in msg)
            {
                var data = DefaultEncoding.GetBytes(seg);
                s.Write(data, 0, data.Length);
                s.Write(LineTerminator, 0, 2);

            }
            s.Write(LineTerminator, 0, 2);


            var response = ReadLine(s);


        }

        public static string ReadLine(Stream s)
        {
            var LineBuffer = new List<byte>();
            try
            {
                while (true)
                {
                    int b = s.ReadByte();
                    // slow read dos
             //       Thread.Sleep(200);

                    if (b == -1)
                        return null;
                    if (b == 10)
                        break;
                    if (b != 13)
                        LineBuffer.Add((byte)b);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
            return DefaultEncoding.GetString(LineBuffer.ToArray());


        }

    }
}
