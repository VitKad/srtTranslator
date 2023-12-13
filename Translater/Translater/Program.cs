﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Net;
using System.IO;

namespace Translater
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string input = @"
Равным образом консультация с профессионалами из IT напрямую зависит от модели развития? Разнообразный и богатый опыт дальнейшее развитие различных форм деятельности требует определения и уточнения системы обучения кадров, соответствующей насущным потребностям? Дорогие друзья, постоянный количественный рост и сфера нашей активности позволяет оценить значение дальнейших направлений развитая системы массового участия. Значимость этих проблем настолько очевидна, что рамки и место обучения кадров способствует повышению актуальности существующих финансовых и административных условий. Практический опыт показывает, что рамки и место обучения кадров влечет за собой процесс внедрения и модернизации системы обучения кадров, соответствующей насущным потребностям.

Задача организации, в особенности же консультация с профессионалами из IT играет важную роль в формировании новых предложений. Таким образом, новая модель организационной деятельности напрямую зависит от системы обучения кадров, соответствующей насущным потребностям. Значимость этих проблем настолько очевидна, что постоянный количественный рост и сфера нашей активности напрямую зависит от экономической целесообразности принимаемых решений. Соображения высшего порядка, а также рамки и место обучения кадров напрямую зависит от дальнейших направлений развитая системы массового участия? Равным образом курс на социально-ориентированный национальный проект требует от нас системного анализа всесторонне сбалансированных нововведений. Не следует, однако, забывать о том, что повышение уровня гражданского сознания представляет собой интересный эксперимент проверки системы масштабного изменения ряда параметров?

Соображения высшего порядка, а также выбранный нами инновационный путь играет важную роль в формировании экономической целесообразности принимаемых решений? Таким образом, сложившаяся структура организации требует от нас системного анализа позиций, занимаемых участниками в отношении поставленных задач. Повседневная практика показывает, что курс на социально-ориентированный национальный проект играет важную роль в формировании системы масштабного изменения ряда параметров!

Равным образом начало повседневной работы по формированию позиции требует от нас системного анализа позиций, занимаемых участниками в отношении поставленных задач. Таким образом, социально-экономическое развитие напрямую зависит от системы обучения кадров, соответствующей насущным потребностям. Повседневная практика показывает, что постоянное информационно-техническое обеспечение нашей деятельности играет важную роль в формировании дальнейших направлений... 
            ";
            string languageFrom = "ru";
            string languageTo = "en";
            Console.WriteLine(translate(input, languageFrom, languageTo));
            Console.WriteLine("");

            TranslateSubtitles();
            Console.ReadKey();

            //Работать в StringBuilder, но когда передовать в функцию переводчика, то преобразовать в String
        }

        public static String translate(String input, string from, string to)
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
            String res = "";

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
            String srtFile = GetSRTFile();
            Console.WriteLine(srtFile);
            WriteSRTFile(srtFile);
        }

        public static String GetSRTFile()
        {
            //D:\Projects\srtTranslator\Translater
            string path = "D:\\Projects\\srtTranslator\\Translater\\1.srt";
            String text = "";

            if (File.Exists(path))
            {
                text = File.ReadAllText(path);
               
            }
         
            return text;
        }
        
        public static void WriteSRTFile(String text)
        {
            string path = "D:\\Projects\\srtTranslator\\Translater\\2.srt";
            File.WriteAllText(path, text);
        }
    }
}



 