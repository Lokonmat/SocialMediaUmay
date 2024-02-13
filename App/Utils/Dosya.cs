using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using UmayAppNew;
using Xamarin.Essentials;
using Xamarin.Forms;
using System.Drawing;

namespace App.Utils
{
    public class Dosya
    {
        public Dosya(string DosyaAdı, string klasorkod, string tableName)
        {
            Gonder(DosyaAdı, klasorkod, tableName);
            PaylasimGonder(DosyaAdı, klasorkod);
        }
        public static bool Gonder(string DosyaAdı, string klasorkod, string tableName)
        {
            FtpWebRequest ftpIstegi;
            try
            {
                string ftpAdresi = "ftp://" + "www.destek88.com" + "/" + "public_html" + "/" + "umayapp" + "/" + "ProfilPicture";
                FileInfo DosyaBilgisi = new FileInfo(DosyaAdı);
                string dosyaYolu = klasorkod + "_" + DosyaBilgisi.Name;

                ftpIstegi = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpAdresi + "/" + tableName + "/" + dosyaYolu));
                ftpIstegi.Credentials = new NetworkCredential("destek88", "w2LBhF27");
                ftpIstegi.Method = WebRequestMethods.Ftp.UploadFile;

                // Bağlantıyı sürekli açık tutma
                ftpIstegi.KeepAlive = false;

                // Yapılacak işlem (Upload)
                ftpIstegi.Method = WebRequestMethods.Ftp.UploadFile;

                // Verinin gönderim şekli
                ftpIstegi.UseBinary = true;

                // Sunucuya gönderilecek dosya uzunluğu bilgisi
                ftpIstegi.ContentLength = DosyaBilgisi.Length;

                // Buffer uzunluğu 2048 byte olarak belirlenmiş
                int bufferUzunlugu = 2048;
                byte[] buff = new byte[10000000];
                int sayi;
                Thread.Sleep(500);
                FileStream stream = DosyaBilgisi.OpenRead();

                // Veriyi göndermek için istemci akışı alınıyor
                Stream str = ftpIstegi.GetRequestStream();

                sayi = stream.Read(buff, 0, bufferUzunlugu);

                // Dosya verileri akış aracılığıyla gönderiliyor
                while (sayi != 0)
                {
                    str.Write(buff, 0, sayi);
                    sayi = stream.Read(buff, 0, bufferUzunlugu);
                }

                // İşlem tamamlandı, akışları kapat
                str.Close();
                stream.Close();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Hata Mesajı: " + ex.Message);
                Console.WriteLine("Stack Trace: " + ex.StackTrace);
                return false;
            }
        }
        public static bool PaylasimGonder(string DosyaAdı, string klasorkod)
        {
            FtpWebRequest ftpIstegi;
            try
            {
                string ftpAdresi = "ftp://" + "www.destek88.com" + "/" + "public_html" + "/" + "umayapp" + "/" + "Post" + "/";
                FileInfo DosyaBilgisi = new FileInfo(DosyaAdı);


                // Alt klasör adını kullanıcı kimliğine veya UUID'ye göre oluşturun
                string altKlasorYolu = klasorkod;
                string tamAd = $"{klasorkod}_{DosyaBilgisi.Name}";
                string dosyaYolu = altKlasorYolu + "_" + DosyaBilgisi.Name;

                ftpIstegi = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpAdresi + dosyaYolu));
                ftpIstegi.Credentials = new NetworkCredential("destek88", "w2LBhF27");
                ftpIstegi.Method = WebRequestMethods.Ftp.UploadFile;
                ftpIstegi.KeepAlive = true;  // Bağlantıyı sürekli açık tutma
                ftpIstegi.UseBinary = true;
                ftpIstegi.ContentLength = DosyaBilgisi.Length;

                int bufferUzunlugu = 2048;
                byte[] buff = new byte[bufferUzunlugu];
                int sayi;

                using (FileStream stream = DosyaBilgisi.OpenRead())
                using (Stream str = ftpIstegi.GetRequestStream())
                {
                    sayi = stream.Read(buff, 0, bufferUzunlugu);

                    while (sayi != 0)
                    {
                        str.Write(buff, 0, sayi);
                        sayi = stream.Read(buff, 0, bufferUzunlugu);
                    }
                }

                ftpIstegi.Abort(); // Bağlantıyı kapat

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Hata Mesajı: " + ex.Message);
                Console.WriteLine("Stack Trace: " + ex.StackTrace);
                return false;
            }
        }
        public static bool VideoGonder(string DosyaAdı, string klasorkod)
        {
            FtpWebRequest ftpIstegi;
            try
            {
                string ftpAdresi = "ftp://" + "www.destek88.com" + "/" + "public_html" + "/" + "umayapp" + "/" + "Post" + "/";
                FileInfo DosyaBilgisi = new FileInfo(DosyaAdı);


                // Alt klasör adını kullanıcı kimliğine veya UUID'ye göre oluşturun
                string altKlasorYolu = klasorkod;
                string tamAd = $"{klasorkod}_{DosyaBilgisi.Name}";
                string dosyaYolu = altKlasorYolu + "_" + DosyaBilgisi.Name;

                // Create a temporary file to store the first 10 seconds of the video
                string tempFilePath = Path.GetTempFileName();
                using (var inputFileStream = new FileStream(DosyaAdı, FileMode.Open, FileAccess.Read))
                using (var tempFileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write))
                {
                    // Calculate the number of bytes for the first 10 seconds (adjust accordingly)
                    int bytesToRead = (int)(10 * inputFileStream.Length / inputFileStream.Length);
                    byte[] buffer = new byte[bytesToRead];

                    // Read the first 10 seconds into the buffer
                    inputFileStream.Read(buffer, 0, bytesToRead);

                    // Write the buffer to the temporary file
                    tempFileStream.Write(buffer, 0, bytesToRead);
                }

                ftpIstegi = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpAdresi + dosyaYolu));
                ftpIstegi.Credentials = new NetworkCredential("destek88", "w2LBhF27");
                ftpIstegi.Method = WebRequestMethods.Ftp.UploadFile;
                ftpIstegi.KeepAlive = true;  // Bağlantıyı sürekli açık tutma
                ftpIstegi.UseBinary = true;
                using (var fileStream = File.OpenRead(tempFilePath))
                using (var ftpStream = ftpIstegi.GetRequestStream())
                {
                    fileStream.CopyTo(ftpStream);
                }

                // Delete the temporary file
                File.Delete(tempFilePath);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Hata Mesajı: " + ex.Message);
                Console.WriteLine("Stack Trace: " + ex.StackTrace);
                return false;
            }
        }
        public static bool PdfGonder(string DosyaAdı, string klasorkod)
        {
            FtpWebRequest ftpIstegi;
            try
            {
                string ftpAdresi = "ftp://" + "www.destek88.com" + "/" + "public_html" + "/" + "umayapp" + "/" + "Post" + "/";
                FileInfo DosyaBilgisi = new FileInfo(DosyaAdı);
                long dosyaUzunlugu = DosyaAdı.Length;


                // Alt klasör adını kullanıcı kimliğine veya UUID'ye göre oluşturun
                string altKlasorYolu = klasorkod;
                string tamAd = $"{klasorkod}_{DosyaBilgisi.Name}";
                string dosyaYolu = altKlasorYolu + "_" + DosyaBilgisi.Name;

                ftpIstegi = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpAdresi + dosyaYolu));
                ftpIstegi.Credentials = new NetworkCredential("destek88", "w2LBhF27");
                ftpIstegi.Method = WebRequestMethods.Ftp.UploadFile;

                // Bağlantıyı sürekli açık tutma
                ftpIstegi.KeepAlive = false;

                // Yapılacak işlem (Upload)
                ftpIstegi.Method = WebRequestMethods.Ftp.UploadFile;

                // Verinin gönderim şekli
                ftpIstegi.UseBinary = true;

                // Sunucuya gönderilecek dosya uzunluğu bilgisi
                ftpIstegi.ContentLength = dosyaUzunlugu;

                // Buffer uzunluğu 2048 byte olarak belirlenmiş
                int bufferUzunlugu = 2048;
                byte[] buff = new byte[10000000];
                int sayi;
                Thread.Sleep(500);
                FileStream stream = DosyaBilgisi.OpenRead();

                // Veriyi göndermek için istemci akışı alınıyor
                Stream str = ftpIstegi.GetRequestStream();

                sayi = stream.Read(buff, 0, bufferUzunlugu);

                // Dosya verileri akış aracılığıyla gönderiliyor
                while (sayi != 0)
                {
                    str.Write(buff, 0, sayi);
                    sayi = stream.Read(buff, 0, bufferUzunlugu);
                }

                // İşlem tamamlandı, akışları kapat
                str.Close();
                stream.Close();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Hata Mesajı: " + ex.Message);
                Console.WriteLine("Stack Trace: " + ex.StackTrace);
                return false;
            }
        }
        public static bool CheckAndCreateDirectory(string ftpServerIP, string klasorYolu, string kullaniciAdi, string sifre)
        {
            FtpWebRequest ftpIstegi;
            try
            {
                ftpIstegi = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpServerIP + klasorYolu));
                ftpIstegi.Credentials = new NetworkCredential(kullaniciAdi, sifre);
                ftpIstegi.Method = WebRequestMethods.Ftp.ListDirectory;
                FtpWebResponse response = (FtpWebResponse)ftpIstegi.GetResponse();
                response.Close();
                return true; // Alt klasör zaten var
            }
            catch (WebException ex)
            {
                // Hata durumunda alt klasör yok demektir, dolayısıyla oluşturun
                if (ex.Response != null && ((FtpWebResponse)ex.Response).StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                {
                    ftpIstegi = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpServerIP + klasorYolu));
                    ftpIstegi.Credentials = new NetworkCredential(kullaniciAdi, sifre);
                    ftpIstegi.Method = WebRequestMethods.Ftp.MakeDirectory;
                    FtpWebResponse response = (FtpWebResponse)ftpIstegi.GetResponse();
                    response.Close();
                    return true; // Alt klasör başarıyla oluşturuldu
                }
                else
                {
                    // Başka bir hata durumunda alt klasör oluşturulamadı
                    return false;
                }
            }
        }
    }
}