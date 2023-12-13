using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Net;
using System.IO;
using static System.Net.Mime.MediaTypeNames;

namespace Translater
{
    internal class Program
    {

        static void Main(string[] args)
        {
       
            //Console.WriteLine(translate(input, languageFrom, languageTo));
            Console.WriteLine("");

            TranslateSubtitles();
            Console.ReadKey();

            //Работать в StringBuilder, но когда передовать в функцию переводчика, то преобразовать в string
        }

        public static string translate(string input, string from, string to)
        {
            var fromLanguage = from;
            var toLanguage = to;
            input = input.Replace("\r\n", " ");
            Console.WriteLine("Входная строка:");
            Console.WriteLine(input);
            Console.WriteLine();
            var url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl={fromLanguage}&tl={toLanguage}&dt=t&q={HttpUtility.UrlEncode(input)}";
            var webclient = new WebClient
            {
                Encoding = System.Text.Encoding.UTF8
            };
            var result = webclient.DownloadString(url);

           // Console.WriteLine(result.Substring(4, 15));
           // Console.WriteLine();
            //Console.WriteLine("Строка на выходе:");
            //Console.WriteLine(result);
           // Console.WriteLine();
            string res = "";

            result = result.Substring(4);

            while (result.Contains(",[\""))
                {
                try
                {
                    //Console.WriteLine("Подстрока:");
                    int start = result.IndexOf("\",\"");
                    //Console.WriteLine("start:" + start);
               
                    int end = result.IndexOf("],[\"");
                    //Console.WriteLine("end:" + end);
                    //Console.WriteLine("");
                    //Console.WriteLine(result.Substring(start, end - start));
                    //Console.WriteLine();
                    result = result.Replace(result.Substring(start, end - start + 4), " ");//, 4
                    //Console.WriteLine("Результат после удаления подстроки:");                                                                                   //
                    //Console.WriteLine(result);
                    //Console.WriteLine();
                }
                catch (Exception ex)
                {
                    return ex.ToString();
                }
                
            }

            int startPos = result.IndexOf("\",\"");
            result = result.Replace(result.Substring(result.IndexOf("\",\"")), " ");//, 4

            return result;
            

        }


        public static void TranslateSubtitles()
        {
            string languageFrom = "en";
            string languageTo = "ru";

            string srtText = "";
            List<(string, string, string)> srtFile = GetSRTFile(ref srtText);

            Console.WriteLine(srtText);
            string textForTranslate = GetTextFromSRTFile(srtFile);


            string translatedText = translate(textForTranslate, languageFrom, languageTo);
            Console.WriteLine("Перевод:");
            Console.WriteLine(translatedText);
            Console.WriteLine();

            List<(string, string, string)> newSrtFile = ConvertTextToSubtitleFile(translatedText, srtFile);

            WriteSRTFile(newSrtFile);
        }

        public static List<(string, string, string)> ConvertTextToSubtitleFile(string translatedText, List<(string, string, string)> srtFile)
        {
            int indexStart = 0;
            int indexEnd = 0;
            List<(string, string, string)> newSrtFile = new List<(string, string, string)>();

            foreach ((string, string, string) blockSrt in srtFile)
            {
                indexEnd = indexStart + blockSrt.Item3.Length;
                string item3 = translatedText.Substring(indexStart, indexEnd - indexStart);
                newSrtFile.Add((blockSrt.Item1, blockSrt.Item2, item3));
                indexStart = indexEnd;
            }

            return newSrtFile;
        }


        public static string GetTextFromSRTFile(List<(string, string, string)> srtFile)
        {
            string fullTextForTranslate = "";
            foreach ((string, string, string) blockSrt in srtFile)
            {
                fullTextForTranslate += blockSrt.Item3;
            }
            
            Console.WriteLine("Полный текст для перевода:");
            Console.WriteLine(fullTextForTranslate);
            Console.WriteLine();
            return fullTextForTranslate;
        }


        public static List<(string, string, string)> GetSRTFile(ref string srtText)
        {
            List<(string, string, string)> srtFileTupleList = new List<(string, string, string)>();
            string path = "D:\\Projects\\srtTranslator\\Translater\\1.srt";
            StreamReader f = new StreamReader(path);
            string text = "";
            int i = 0;
            (string, string, string) srtFileTuple = ("", "", "");
            while (!f.EndOfStream)
            {
                string line = f.ReadLine();
                srtText += line + "\r\n";
                switch (i)
                {
                    case 0:
                        srtFileTuple.Item1 = line;
                        i++;
                        break;
                    case 1:
                        srtFileTuple.Item2 = line;
                        i++;
                        break;
                    case 2:
                        srtFileTuple.Item3 = line;
                        i++;
                        break;
                    case 3:
                        srtFileTupleList.Add(srtFileTuple);
                        i = 0;
                        break;
                }
                // что-нибудь делаем с прочитанной строкой s
            }
            f.Close();
            return srtFileTupleList;

            /*
            //D:\Projects\srtTranslator\Translater
            string path = "D:\\Projects\\srtTranslator\\Translater\\1.srt";
            string text = "";

            if (File.Exists(path))
            {
                text = File.ReadAllText(path);
               
            }
         
            return text;*/
        }
        
        public static void WriteSRTFile(List<(string, string, string)> newSrtFile)
        {
            string text = "";
            foreach ((string, string, string) blockSrt in newSrtFile)
            {
                text += blockSrt.Item1 + "\r\n";
                text += blockSrt.Item2 + "\r\n";
                text += blockSrt.Item3 + "\r\n";
                text += "\r\n";
            }
            string path = "D:\\Projects\\srtTranslator\\Translater\\2.srt";
            File.WriteAllText(path, text);
        }
    }
}



 