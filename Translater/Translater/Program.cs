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
       
            ////Console.WriteLine(translate(input, languageFrom, languageTo));
            Console.WriteLine("Начало перевода");

            string pathOut = "..\\..\\..\\Out\\";
            string pathIn = "..\\..\\..\\In\\";

            //Сортировка по дате создания файлов от мдм и взятие 20 первых файлов.
            string[] allFilesInExchangeFolder = Directory.EnumerateFiles(pathOut).OrderBy(d => new FileInfo(d).CreationTime).Take(20).ToArray();

            if (allFilesInExchangeFolder.Length > 0) //если обнаружен хоть один файл
            {
                foreach (string file in allFilesInExchangeFolder)
                {
                    string fileName = System.IO.Path.GetFileNameWithoutExtension(file); //сохранить имя файла без полного пути 
                    Console.Write("Файл: " + fileName + " - ");
                    TranslateSubtitles(file, pathIn, fileName);
                }   
            }

            //TranslateSubtitles();
            Console.WriteLine("Выполнение программы завершено. Нажмите любую кнопку для выхода...");
            Console.ReadKey();

            //Работать в StringBuilder, но когда передовать в функцию переводчика, то преобразовать в string
        }

        public static string TranslateText(List<string> inputList, string from, string to)
        {
            var fromLanguage = from;
            var toLanguage = to;
            string resText = "";
            foreach (var input in inputList)
            {
                string textWithoutBreak = input.Replace("\r\n", " ");
                //Console.WriteLine("Входная строка:");
                //Console.WriteLine(input);
                //Console.WriteLine();
                var url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl={fromLanguage}&tl={toLanguage}&dt=t&q={HttpUtility.UrlEncode(textWithoutBreak)}";
                var webclient = new WebClient
                {
                    Encoding = System.Text.Encoding.UTF8
                };
                var result = webclient.DownloadString(url);

                // //Console.WriteLine(result.Substring(4, 15));
                // //Console.WriteLine();
                ////Console.WriteLine("Строка на выходе:");
                ////Console.WriteLine(result);
                // //Console.WriteLine();

                result = result.Substring(4);

                while (result.Contains(",[\""))
                {
                    try
                    {
                        ////Console.WriteLine("Подстрока:");
                        int start = result.IndexOf("\",\"");
                        ////Console.WriteLine("start:" + start);

                        int end = result.IndexOf("],[\"");
                        ////Console.WriteLine("end:" + end);
                        ////Console.WriteLine("");
                        ////Console.WriteLine(result.Substring(start, end - start));
                        ////Console.WriteLine();
                        result = result.Replace(result.Substring(start, end - start + 4), " ");//, 4
                                                                                               ////Console.WriteLine("Результат после удаления подстроки:");                                                                                   //
                                                                                               ////Console.WriteLine(result);
                                                                                               ////Console.WriteLine();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Ошибка в преобразовании текста");
                        //return ex.ToString();
                    }

                }

                int startPos = result.IndexOf("\",\"");
                result = result.Replace(result.Substring(result.IndexOf("\",\"")), " ");//, 4
                resText += result;
            }


            return resText;
            

        }


        public static void TranslateSubtitles(string pathOut, string pathIn, string fileName)
        {
            try
            {
                string languageFrom = "en";
                string languageTo = "ru";

                string srtText = "";
                List<(string, string, string)> srtFile = GetSRTFile(pathOut, ref srtText);

                //Console.WriteLine(srtText);
                string textForTranslate = GetTextFromSRTFile(srtFile);

                List<string> listForTranslate = GetTextList(textForTranslate);

                string translatedText = TranslateText(listForTranslate, languageFrom, languageTo);
                Console.WriteLine("Перевод:");
                Console.WriteLine(translatedText);
                Console.WriteLine();

                List<(string, string, string)> newSrtFile = CreateNewSubtitle(translatedText, srtFile);
                //List <(string, string, string)> newSrtFile = ConvertTextToSubtitleFile(translatedText, srtFile);

                WriteSRTFile(newSrtFile, pathIn + fileName);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Готово");
                Console.ResetColor();
                
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }
           
        }


        public static List<string> GetTextList(string textForTranslate)
        {
            List<string> textList = new List<string>();
            if (textForTranslate.Length >= 10000) 
            { 
                for (int i = 0; i < textForTranslate.Length; i += 10000)
                {
                    if (textForTranslate.Length - 1 < i+10000)
                    {
                        textList.Add(textForTranslate.Substring(i, textForTranslate.Length - i - 1));
                    }
                    else
                    {
                        textList.Add(textForTranslate.Substring(i, 10000));
                    }
                }
            }
            else
            {
                textList.Add(textForTranslate);
            }

            return textList;
        }

        public static List<(string, string, string)> ConvertTextToSubtitleFile(string translatedText, List<(string, string, string)> srtFile)
        {
            int indexStart = 0;
            int indexEnd = 0;
            List<(string, string, string)> newSrtFile = new List<(string, string, string)>();


            foreach ((string, string, string) blockSrt in srtFile)
            {
                indexEnd = indexStart + blockSrt.Item3.Length;
                if (indexEnd < translatedText.Length)
                {
                    while (translatedText[indexEnd] != ' ')
                    {
                        indexEnd++;
                    }
                    indexEnd++;
                }
                else
                {
                    indexEnd = translatedText.Length;
                }

                if (blockSrt == srtFile[srtFile.Count - 1] && indexEnd != translatedText.Length)
                {
                    indexEnd = translatedText.Length;
                }


                string item3 = translatedText.Substring(indexStart, indexEnd - indexStart);
                newSrtFile.Add((blockSrt.Item1, blockSrt.Item2, item3));
                indexStart = indexEnd;
            }

            return newSrtFile;
        }

        public static List<(string, string, string)> CreateNewSubtitle(string translatedText, List<(string, string, string)> srtFile)
        {
            int indexStart = 0;
            int indexEnd = 0;
            List<(string, string, string)> newSrtFile = new List<(string, string, string)>();

            int countSubLines = srtFile.Count;
            double totalSeconds = GetTotalTimeSubtitles(countSubLines, srtFile);

            Console.WriteLine("Общее время: " + totalSeconds);

            double timeForOneLine = (totalSeconds / countSubLines);
            Console.WriteLine("Время одной линии: " + timeForOneLine);

            TimeSpan time;
            string strTime = "";
            double startTime = 0;
            double endTime = timeForOneLine;

            for (int i=1; i<=countSubLines; i++)
            {
                time = TimeSpan.FromSeconds(startTime);
                strTime = time.ToString(@"hh\:mm\:ss\,fff");
                time = TimeSpan.FromSeconds(endTime);
                strTime += " --> " + time.ToString(@"hh\:mm\:ss\,fff");
                Console.WriteLine(strTime);
                startTime += timeForOneLine;
                endTime += timeForOneLine;

               //string item3 = translatedText.Substring(indexStart, indexEnd - indexStart);
                newSrtFile.Add((i.ToString(), strTime, ""));
            }


            /*
            TimeSpan time = TimeSpan.FromSeconds(timeForOneLine);
            string str = time.ToString(@"hh\:mm\:ss\,fff");
            Console.WriteLine(str);

            timeForOneLine += timeForOneLine;

            time = TimeSpan.FromSeconds(timeForOneLine);
            str = time.ToString(@"hh\:mm\:ss\,fff");
            Console.WriteLine(str);
            */
            /*
            foreach ((string, string, string) blockSrt in srtFile)
            {
                indexEnd = indexStart + blockSrt.Item3.Length;
                if (indexEnd < translatedText.Length)
                {
                    while (translatedText[indexEnd] != ' ')
                    {
                        indexEnd++;
                    }
                    indexEnd++;
                }
                else
                {
                    indexEnd = translatedText.Length;
                }

                if (blockSrt == srtFile[srtFile.Count - 1] && indexEnd != translatedText.Length)
                {
                    indexEnd = translatedText.Length;
                }


                string item3 = translatedText.Substring(indexStart, indexEnd - indexStart);
                newSrtFile.Add((blockSrt.Item1, blockSrt.Item2, item3));
                indexStart = indexEnd;
            }
            */
            return newSrtFile;
        }


        public static double GetTotalTimeSubtitles(int countSubLines, List<(string, string, string)> srtFile)
        {
            string timeStr = srtFile[countSubLines - 1].Item2;
            int timeIndex = timeStr.IndexOf("-->") + 4;
            timeStr = timeStr.Substring(timeIndex);

            string hourSub = timeStr.Substring(0, 2); ;
            string minuteSub = timeStr.Substring(3, 2);
            string secundSub = timeStr.Substring(6, 2);

            double totalSeconds = Convert.ToDouble(hourSub) * 3600 + Convert.ToDouble(minuteSub) * 60 + Convert.ToDouble(secundSub);

            return totalSeconds;

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


        public static List<(string, string, string)> GetSRTFile(string path, ref string srtText)
        {
            List<(string, string, string)> srtFileTupleList = new List<(string, string, string)>();
            //string path = "D:\\Projects\\srtTranslator\\Translater\\1.srt";
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
                        if (line != "")
                        {
                            srtFileTuple.Item3 += "\r\n" + line;
                        }
                        else
                        {
                            srtFileTupleList.Add(srtFileTuple);
                            i = 0;
                        }
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
        
        public static void WriteSRTFile(List<(string, string, string)> newSrtFile, string pathIn)
        {
            string text = "";
            foreach ((string, string, string) blockSrt in newSrtFile)
            {
                text += blockSrt.Item1 + "\r\n";
                text += blockSrt.Item2 + "\r\n";
                text += blockSrt.Item3 + "\r\n";
                text += "\r\n";
            }
            //string path = "D:\\Projects\\srtTranslator\\Translater\\2.srt";
            File.WriteAllText(pathIn + ".srt", text);
        }
    }
}



 