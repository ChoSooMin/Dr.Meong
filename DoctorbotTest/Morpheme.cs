using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using agi = HtmlAgilityPack;
using System.Net;
using System.Web;

using System.IO;
using System.Text.RegularExpressions;
using System.Xml.XPath;
using System.Text;

namespace DoctorbotTest
{
    public class Morpheme
    {
        public static string GetOriginalForm(string finalWord) // 단어의 원형을 찾는 함수
        {
            string baseUri = "http://krdic.naver.com/search.nhn?kind=keyword&query="; // 기본 uri
            string findWord = finalWord.Replace(".", ""); // 반복한다. 중 마지막 점(.) 삭제
            string uri = baseUri + findWord;

            // Console.WriteLine(uri); // uri 맞는지 확인

            Uri targetUri = new Uri(uri); // Uri 객체 생성
            HttpWebRequest webRequest = HttpWebRequest.Create(targetUri) as HttpWebRequest; // Request로 페이지를 가져오는건가?

            using (HttpWebResponse webResponse = webRequest.GetResponse() as HttpWebResponse) // Response로 페이지 받아오는 건강?
            using (Stream webResponseStream = webResponse.GetResponseStream()) // 여기부터는 잘 모르겠음,,,
            {
                agi.HtmlDocument s = new agi.HtmlDocument();
                Encoding targetEncoding = Encoding.UTF8;

                s.Load(webResponseStream, targetEncoding, true);
                IXPathNavigable nav = s;

                string word = WebUtility.HtmlDecode(nav.CreateNavigator().SelectSingleNode("//*[@id='content']/div[2]/ul/li/div/a/strong").ToString()); // Xpath 사용하여 해당 노드를 가져온 후, ToString()으로 문자열만 가져온다.

                return word;
            }
        }

        public static string TransformSentence(string originalSentence, string morpheme)
        {
            var sentence = originalSentence;

            char delimiterChars = ' '; // 공백을 기준으로 분할한다.

            string[] words = sentence.Split(delimiterChars); // 분할된 문장들이 배열로 들어감 (ex. 심한, 설사를, 반복한다.)
            string finalWord = words[words.Length - 1]; // 분할된 문장 중 맨 마지막 문장만 가져온다. (ex. 반복한다.)

            string originalForm = GetOriginalForm(finalWord); // GetOriginalForm 함수를 사용하여 단어의 원형을 찾아온다.

            string andForm = "";

            // 
            char end2 = originalForm.ElementAt(originalForm.Length - 2);
            string end = end2 + "";

            if (morpheme.Equals("면")) {
                if (end.Equals("있") || end.Equals("없") || end.Equals("않")) // 있다 & 않다 & 없다 처리
                {
                    andForm = originalForm.Replace("다", "으" + morpheme);
                }
                else if (originalForm.Equals("난다"))
                {
                    andForm = "나면";
                }
                else
                {
                    andForm = originalForm.Replace("다", morpheme);
                }
            }
            else
            {
                if (originalForm.Equals("난다") && morpheme.Equals("고, "))
                {
                    andForm = "나고";
                }
                else
                {
                    andForm = originalForm.Replace("다", morpheme);
                }
            }
            
            //
            if (originalForm.Equals("이상하다") && morpheme.Equals("는 경우에, \n"))
            {
                andForm = "이상한 경우에, \n";
            }

            // 난다 처리
            if (originalForm.Equals("난다") && morpheme.Equals("는 경우에, \n"))
            {
                andForm = "나는 경우에, \n";
            }
            

            // 바꾼 단어를 다시 문장으로
            string word = "";
            for (int i = 0; i < words.Length - 1; i++)
            {
                word += words[i] + " ";
            }
            word += andForm;

            return word;
        }
    }
}