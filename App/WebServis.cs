using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace UmayAppNew
{
    public class WebServis
    {
        public static string SifrelenmisURL { get; private set; }

        public static string IzinYeni(String kurumkodu, string program = "anonsterminalisql")
        {
            string TamURL;
            try
            {
                string sonuc;
                string url = "http://www.sekizdesekiz.com/versiyonguncelleme/izinyeni.php";
                TamURL = url + "?kurumkodu=" + kurumkodu + "&program=" + program;
                WebRequest client = WebRequest.Create(TamURL);

                Stream objStream;
                objStream = client.GetResponse().GetResponseStream();
                StreamReader objReader = new StreamReader(objStream, Encoding.GetEncoding("windows-1254"));
                WebResponse wr = client.GetResponse();
                Stream receiveStream = wr.GetResponseStream();
                StreamReader reader = new StreamReader(receiveStream, Encoding.GetEncoding("windows-1254"));
                sonuc = reader.ReadToEnd();
                objStream.Close();
                client.Abort();
                return sonuc.TrimEnd('\n');
            }
            catch { return ""; }
        }
        public static string TestKBM(String sql)
        {
            try
            {
                string sonuc = "";
                string egParametre = "";
                egParametre = sql.Substring(0, 6) == "select" ? "1" : "0";
                string url = "http://www.destek88.com/umayapp/sqlNet.php";
                string sifrelitext = Sifrele(sql);
                sifrelitext = "C" + sifrelitext + "=";
                SifrelenmisURL = Sifrele(sifrelitext);
                SifrelenmisURL = url + "?parametre=" + egParametre + "&sql=" + SifrelenmisURL;
                WebRequest client = WebRequest.Create(SifrelenmisURL);

                Stream objStream;
                objStream = client.GetResponse().GetResponseStream();
                StreamReader objReader = new StreamReader(objStream, Encoding.GetEncoding("windows-1254"));
                WebResponse wr = client.GetResponse();
                Stream receiveStream = wr.GetResponseStream();
                StreamReader reader = new StreamReader(receiveStream, Encoding.GetEncoding("windows-1254"));
                sonuc = reader.ReadToEnd().TrimEnd('\n');
                objStream.Close();
                client.Abort();
                return sonuc.TrimEnd('\n').TrimEnd('\r');
            }
            catch (Exception EX) { return ""; }
        }

        public static string Sifrele(string text)
        {
            try
            {
                text = Encode64(Convert.ToString(text));
                return text;
            }
            catch { return ""; }
        }
        public static string Encode64(string Text)
        {
            var plainTextBytes = Encoding.GetEncoding("windows-1254").GetBytes(Text);
            return Convert.ToBase64String(plainTextBytes);
        }
    }
}
