using HtmlAgilityPack;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace GksKatowiceBot.Helpers
{
    public class BaseGETMethod
    {


        public static IList<Attachment> GetCardsAttachmentsAktualnosci(ref List<IGrouping<string, string>> hrefList, bool newUser = false)
        {
            List<Attachment> list = new List<Attachment>();
            string urlAddress = "http://uml.lodz.pl/aktualnosci/";
            // string urlAddress = "http://www.orlenliga.pl/";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            var listTemp2 = new List<System.Linq.IGrouping<string, string>>();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.UTF8);
                }

                string data = readStream.ReadToEnd();

                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(data);

                string matchResultDivId = "articles-list";
                string xpath = String.Format("//div[@class='{0}']/div", matchResultDivId);
                var people = doc.DocumentNode.SelectNodes(xpath).Select(p => p.InnerHtml);
                string text = "";
                foreach (var person in people)
                {
                    text += person;
                }

                HtmlAgilityPack.HtmlDocument doc2 = new HtmlAgilityPack.HtmlDocument();

                doc2.LoadHtml(text);
                hrefList = doc2.DocumentNode.SelectNodes("//a")
                                  .Select(p => p.GetAttributeValue("href", "not found")).Where(p => p.Contains("artykul")).GroupBy(p => p.ToString())
                                  .ToList();

                var imgList = doc2.DocumentNode.SelectNodes("//img")
                                  .Select(p => p.GetAttributeValue("src", "not found"))
                                  .ToList();

                var titleList = doc2.DocumentNode.SelectNodes("//a")
                                   .Select(p => p.GetAttributeValue("title", "")).Where(p => p != "").GroupBy(p => p.ToString())
                                   .ToList();

                response.Close();
                readStream.Close();

                int index = 5;

                DataTable dt =  BaseDB.GetWiadomosci(); 
                //    GetWiadomosciPilka();

                if (newUser == true)
                {
                    index = hrefList.Count;
                    if (dt.Rows.Count == 0)
                    {
                        //    AddWiadomosc(hrefList);
                    }
                }

                else
                {
                    if (dt.Rows.Count > 0)
                    {
                        List<int> deleteList = new List<int>();
                        var listTemp = new List<System.Linq.IGrouping<string, string>>();
                        var imageListTemp = new List<string>();
                        var titleListTemp = new List<IGrouping<string,string>>();
                        index = hrefList.Count > 15 ? 15 : hrefList.Count;
                        for (int i = 0; i < hrefList.Count; i++)
                        {
                            if (dt.Rows[dt.Rows.Count - 1]["Wiadomosc1"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc2"].ToString() != hrefList[i].Key &&
                                dt.Rows[dt.Rows.Count - 1]["Wiadomosc3"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc4"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc5"].ToString() != hrefList[i].Key &&
                                dt.Rows[dt.Rows.Count - 1]["Wiadomosc6"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc7"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc8"].ToString() != hrefList[i].Key &&
                                dt.Rows[dt.Rows.Count - 1]["Wiadomosc9"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc10"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc11"].ToString() != hrefList[i].Key &&
                                dt.Rows[dt.Rows.Count - 1]["Wiadomosc12"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc13"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc14"].ToString() != hrefList[i].Key &&
                                dt.Rows[dt.Rows.Count - 1]["Wiadomosc15"].ToString() != hrefList[i].Key
                            )
                            {
                                listTemp.Add(hrefList[i]);
                                imageListTemp.Add("http://uml.lodz.pl/" + imgList[i]);
                                titleListTemp.Add(titleList[i]);
                            }
                            listTemp2.Add(hrefList[i]);
                        }
                        hrefList = listTemp;
                        index = hrefList.Count;
                        imgList = imageListTemp;
                        titleList = titleListTemp;
                        //   AddWiadomosc(listTemp2);
                    }
                    else
                    {
                        index = hrefList.Count>15?15 : hrefList.Count; ;
                        //   AddWiadomosc(hrefList);
                    }
                }

                for (int i = 0; i < index; i++)
                {
                    string link = "";
                    if (hrefList[i].Key.Contains("http"))
                    {
                        link = hrefList[i].Key;
                    }
                    else
                    {
                        link = "http://uml.lodz.pl" + hrefList[i].Key;
                        //link = "http://www.orlenliga.pl/" + hrefList[i].Key;
                    }

                    if (link.Contains("video"))
                    {
                        list.Add(GetHeroCard(
                        titleList[i].Key.Replace("&quot;", ""), "", "",
                        new CardImage(url: imgList[i]),
                        new CardAction(ActionTypes.OpenUrl, "Oglądaj video", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }
                    else
                        if (link.Contains("gallery"))
                    {
                        list.Add(GetHeroCard(
                        titleList[i].Key.Replace("&quot;", ""), "", "",
                        new CardImage(url: imgList[i]),
                        new CardAction(ActionTypes.OpenUrl, "Przeglądaj galerie", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }
                    else
                    {
                        list.Add(GetHeroCard(
                        titleList[i].Key.Replace("&quot;", ""), "", "",
                        new CardImage(url: "http://uml.lodz.pl/" + imgList[i]),
                        new CardAction(ActionTypes.OpenUrl, "Więcej", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }

                    //  list.Add(new Microsoft.Bot.Connector.VideoCard(titleList[i], "", "",null)
                }
            }
            if (listTemp2.Count > 0)
            {
                hrefList = listTemp2;
            }

            return list;

        }


        public static IList<Attachment> GetCardsAttachmentsWyszukaj(ref List<IGrouping<string, string>> hrefList, bool newUser = false,string wyszukaj="")
        {
            List<Attachment> list = new List<Attachment>();
            try
            {
                string urlAddress = "http://uml.lodz.pl/wyszukiwanie/?id=102&L=0&q=" + wyszukaj;
                // string urlAddress = "http://www.orlenliga.pl/";

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                var listTemp2 = new List<System.Linq.IGrouping<string, string>>();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Stream receiveStream = response.GetResponseStream();
                    StreamReader readStream = null;

                    if (response.CharacterSet == null)
                    {
                        readStream = new StreamReader(receiveStream);
                    }
                    else
                    {
                        readStream = new StreamReader(receiveStream, Encoding.UTF8);
                    }

                    string data = readStream.ReadToEnd();

                    HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(data);

                    string matchResultDivId = "articles-list";
                    string xpath = String.Format("//div[@class='{0}']/div", matchResultDivId);
                    var people = doc.DocumentNode.SelectNodes(xpath).Select(p => p.InnerHtml);
                    string text = "";
                    foreach (var person in people)
                    {
                        text += person;
                    }

                    HtmlAgilityPack.HtmlDocument doc2 = new HtmlAgilityPack.HtmlDocument();

                    doc2.LoadHtml(text);
                    hrefList = doc2.DocumentNode.SelectNodes("//a")
                                      .Select(p => p.GetAttributeValue("href", "not found")).GroupBy(p => p.ToString())
                                      .ToList();

                    var imgList = doc2.DocumentNode.SelectNodes("//img")
                                      .Select(p => p.GetAttributeValue("src", "not found"))
                                      .ToList();

                    var titleList = doc2.DocumentNode.SelectNodes("//h3//a").Select(p=>p.InnerHtml.Replace("<span class="+'"'+"search--results--highlight"+'"'+">","").Replace("</span>","").Replace("\n","").Replace("\t","").Replace(@"\ "," "))

                                       .ToList();

                    //var titleList2 = doc2.DocumentNode.SelectNodes("//time").Select(p => p.InnerHtml.Replace("<span class=" + '"' + "search--results--highlight" + '"' + ">", "").Replace("</span>", "").Replace("\n", "").Replace("\t", "").Replace(@"\ ", " "))

                    //                   .ToList();
                    var titleList2 = doc2.DocumentNode.SelectNodes("//time").Select(p => p.GetAttributeValue("datetime", "not found")).Where(p=> p!="")

                                       .ToList();

                    response.Close();
                    readStream.Close();

                    int index = 5;

                    DataTable dt = BaseDB.GetWiadomosci();
                    //    GetWiadomosciPilka();

                    if (newUser == true)
                    {
                        index = hrefList.Count;
                        if (dt.Rows.Count == 0)
                        {
                            //    AddWiadomosc(hrefList);
                        }
                    }

                    else
                    {
                        if (dt.Rows.Count > 0)
                        {
                            List<int> deleteList = new List<int>();
                            var listTemp = new List<System.Linq.IGrouping<string, string>>();
                            var imageListTemp = new List<string>();
                            var titleListTemp = new List<IGrouping<string, string>>();
                            index = imgList.Count > 15 ? 15 : imgList.Count;
                            for (int i = 0; i < index; i++)
                            {
                                if (dt.Rows[dt.Rows.Count - 1]["Wiadomosc1"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc2"].ToString() != hrefList[i].Key &&
                                    dt.Rows[dt.Rows.Count - 1]["Wiadomosc3"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc4"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc5"].ToString() != hrefList[i].Key &&
                                    dt.Rows[dt.Rows.Count - 1]["Wiadomosc6"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc7"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc8"].ToString() != hrefList[i].Key &&
                                    dt.Rows[dt.Rows.Count - 1]["Wiadomosc9"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc10"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc11"].ToString() != hrefList[i].Key &&
                                    dt.Rows[dt.Rows.Count - 1]["Wiadomosc12"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc13"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc14"].ToString() != hrefList[i].Key &&
                                    dt.Rows[dt.Rows.Count - 1]["Wiadomosc15"].ToString() != hrefList[i].Key
                                )
                                {
                                    listTemp.Add(hrefList[i]);
                                    imageListTemp.Add("http://uml.lodz.pl/" + imgList[i]);
                                    //titleListTemp.Add(titleList[i].Key);
                                }
                                listTemp2.Add(hrefList[i]);
                            }
                            hrefList = listTemp;
                            index = hrefList.Count;
                            imgList = imageListTemp;
                       //     titleList = titleListTemp;
                            //   AddWiadomosc(listTemp2);
                        }
                        else
                        {
                            index = hrefList.Count > 15 ? 15 : hrefList.Count; ;
                            //   AddWiadomosc(hrefList);
                        }
                    }

                    for (int i = 0; i < index; i++)
                    {
                        string link = "";
                        if (hrefList[i].Key.Contains("http"))
                        {
                            link = hrefList[i].Key;
                        }
                        else
                        {
                            link = "http://uml.lodz.pl" + hrefList[i].Key;
                            //link = "http://www.orlenliga.pl/" + hrefList[i].Key;
                        }

                        if (link.Contains("video"))
                        {
                            list.Add(GetHeroCard(
                            titleList[i].Replace("&quot;", ""), "", "",
                            new CardImage(url: imgList[i]),
                            new CardAction(ActionTypes.OpenUrl, "Oglądaj video", value: link),
                            new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                            );
                        }
                        else
                            if (link.Contains("gallery"))
                        {
                            list.Add(GetHeroCard(
                            titleList[i].Replace("&quot;", ""), "", "",
                            new CardImage(url: imgList[i]),
                            new CardAction(ActionTypes.OpenUrl, "Przeglądaj galerie", value: link),
                            new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                            );
                        }
                        else
                        {
                            list.Add(GetHeroCard(
                            titleList[i].Replace("&quot;", ""), "", "",
                            new CardImage(url: "http://uml.lodz.pl/" + imgList[i]),
                            new CardAction(ActionTypes.OpenUrl, "Więcej", value: link),
                            new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                            );
                        }

                        //  list.Add(new Microsoft.Bot.Connector.VideoCard(titleList[i], "", "",null)
                    }
                }
                if (listTemp2.Count > 0)
                {
                    hrefList = listTemp2;
                }

                return list;
            }
            catch
            {
                return list;
            }
        }

        public static IList<Attachment> GetCardsAttachmentsWydarzenia(ref List<IGrouping<string, string>> hrefList, bool newUser = false,string dataOd="",string dataDo="")
        {
            List<Attachment> list = new List<Attachment>();
          
            string urlAddress = "http://uml.lodz.pl/kalendarz/"+dataOd.Replace("-","/");

            if (dataDo != "")
            {
                urlAddress = @"http://uml.lodz.pl/kalendarz/?no_cache=1&tx_calendarize_calendar%5BcustomSearch%5D%5Bcategories%5D=&tx_calendarize_calendar%5BstartDate%5D="+dataOd+"&tx_calendarize_calendar%5BendDate%5D="+dataDo;
            }
            // string urlAddress = "http://www.orlenliga.pl/";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            var listTemp2 = new List<System.Linq.IGrouping<string, string>>();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.UTF8);
                }

                string data = readStream.ReadToEnd();

                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(data);

                string matchResultDivId = "articles-list articles-list--grid row";
                string xpath = String.Format("//div[@class='{0}']/div", matchResultDivId);
                var people = doc.DocumentNode.SelectNodes(xpath).Select(p => p.InnerHtml);
                string text = "";
                foreach (var person in people)
                {
                    text += person;
                }

                HtmlAgilityPack.HtmlDocument doc2 = new HtmlAgilityPack.HtmlDocument();

                doc2.LoadHtml(text);
                hrefList = doc2.DocumentNode.SelectNodes("//a")
                                  .Select(p => p.GetAttributeValue("href", "not found")).GroupBy(p => p.ToString())
                                  .ToList();

                var imgList = doc2.DocumentNode.SelectNodes("//img")
                                  .Select(p => p.GetAttributeValue("src", "not found"))
                                  .ToList();

                var titleList = doc2.DocumentNode.SelectNodes("//a")
                                   .Select(p => p.GetAttributeValue("title", "")).Where(p => p != "").GroupBy(p => p.ToString())
                                   .ToList();
                var titleList2 = doc2.DocumentNode.SelectNodes("//div[@class='article-item__location']")
                   .Select(p => p.InnerText.Replace("\n", " ").Replace("\t", " ").Replace(@"\ ", " "))
                   .ToList();
                var titleList3 = doc2.DocumentNode.SelectNodes("//p[@class='article-item__lead']")
   .Select(p => p.InnerText.Replace("\n", " ").Replace("\t", " ").Replace(@"\ ", " "))
   .ToList();
                response.Close();
                readStream.Close();

                int index = 5;

                DataTable dt = BaseDB.GetWiadomosci();
                //    GetWiadomosciPilka();

                if (newUser == true)
                {
                    index = titleList.Count>20?20: titleList.Count;
                    if (dt.Rows.Count == 0)
                    {
                        //    AddWiadomosc(hrefList);
                    }
                }

                else
                {
                    if (dt.Rows.Count > 0)
                    {
                        List<int> deleteList = new List<int>();
                        var listTemp = new List<System.Linq.IGrouping<string, string>>();
                        var imageListTemp = new List<string>();
                        var titleListTemp = new List<IGrouping<string, string>>();
                        index = titleList.Count > 20 ? 20 : titleList.Count;
                        for (int i = 0; i < index; i++)
                        {
                            if (dt.Rows[dt.Rows.Count - 1]["Wiadomosc1"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc2"].ToString() != hrefList[i].Key &&
                                dt.Rows[dt.Rows.Count - 1]["Wiadomosc3"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc4"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc5"].ToString() != hrefList[i].Key &&
                                dt.Rows[dt.Rows.Count - 1]["Wiadomosc6"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc7"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc8"].ToString() != hrefList[i].Key &&
                                dt.Rows[dt.Rows.Count - 1]["Wiadomosc9"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc10"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc11"].ToString() != hrefList[i].Key &&
                                dt.Rows[dt.Rows.Count - 1]["Wiadomosc12"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc13"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc14"].ToString() != hrefList[i].Key &&
                                dt.Rows[dt.Rows.Count - 1]["Wiadomosc15"].ToString() != hrefList[i].Key
                            )
                            {
                                listTemp.Add(hrefList[i]);
                                imageListTemp.Add(imgList[i]);
                                titleListTemp.Add(titleList[i]);
                            }
                            listTemp2.Add(hrefList[i]);
                        }
                        hrefList = listTemp;
                        index = hrefList.Count;
                        imgList = imageListTemp;
                        titleList = titleListTemp;
                        //   AddWiadomosc(listTemp2);
                    }
                    else
                    {
                        index = hrefList.Count > 15 ? 15 : hrefList.Count; ;
                        //   AddWiadomosc(hrefList);
                    }
                }

                for (int i = 0; i < index; i++)
                {
                    string link = "";
                    if (hrefList[i].Key.Contains("http"))
                    {
                        link = hrefList[i].Key;
                    }
                    else
                    {
                        link = "http://uml.lodz.pl" + hrefList[i].Key;
                        //link = "http://www.orlenliga.pl/" + hrefList[i].Key;
                    }

                    if (link.Contains("video"))
                    {
                        list.Add(GetHeroCard(
                        titleList[i].Key.Replace("&quot;", ""), "", "",
                        new CardImage(url: imgList[i]),
                        new CardAction(ActionTypes.OpenUrl, "Oglądaj video", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }
                    else
                        if (link.Contains("gallery"))
                    {
                        list.Add(GetHeroCard(
                        titleList[i].Key.Replace("&quot;", ""),titleList2[i].Replace("\n",""), "",
                        new CardImage(url: imgList[i]),
                        new CardAction(ActionTypes.OpenUrl, "Przeglądaj galerie", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }
                    else
                    {
                        list.Add(GetHeroCard(
                        titleList[i].Key.Replace("&quot;", ""), titleList2[i].Replace("\n", "").Replace("  ",""), titleList3[i].Replace("\n", "").Replace("  ", "").Replace("więcej",""),
                        new CardImage(url: "http://uml.lodz.pl/" + imgList[i]),
                        new CardAction(ActionTypes.OpenUrl, "Więcej", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }

                    //  list.Add(new Microsoft.Bot.Connector.VideoCard(titleList[i], "", "",null)
                }
            }
            if (listTemp2.Count > 0)
            {
                hrefList = listTemp2;
            }

            return list;

        }



        public static IList<Attachment> GetCardsAttachmentsWydarzenia1(ref List<IGrouping<string, string>> hrefList, bool newUser = false, string dataOd = "", string dataDo = "")
        {
            List<Attachment> list = new List<Attachment>();


            string urlAddress = "http://uml.lodz.pl/wydarzenia/";

            if (dataDo != "")
            {
                urlAddress = @"http://uml.lodz.pl/kalendarz/?no_cache=1&tx_calendarize_calendar%5BcustomSearch%5D%5Bcategories%5D=&tx_calendarize_calendar%5BstartDate%5D=" + dataOd + "&tx_calendarize_calendar%5BendDate%5D=" + dataDo;
            }
            // string urlAddress = "http://www.orlenliga.pl/";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            var listTemp2 = new List<System.Linq.IGrouping<string, string>>();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.UTF8);
                }

                string data = readStream.ReadToEnd();

                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(data);

                string matchResultDivId = "articles-list articles-list--grid row";
                string xpath = String.Format("//div[@class='{0}']/div", matchResultDivId);
                var people = doc.DocumentNode.SelectNodes(xpath).Select(p => p.InnerHtml);
                string text = "";
                foreach (var person in people)
                {
                    text += person;
                }

                HtmlAgilityPack.HtmlDocument doc2 = new HtmlAgilityPack.HtmlDocument();

                doc2.LoadHtml(text);
                hrefList = doc2.DocumentNode.SelectNodes("//a")
                                  .Select(p => p.GetAttributeValue("href", "not found")).GroupBy(p => p.ToString())
                                  .ToList();

                var imgList = doc2.DocumentNode.SelectNodes("//img")
                                  .Select(p => p.GetAttributeValue("src", "not found"))
                                  .ToList();

                var titleList = doc2.DocumentNode.SelectNodes("//a")
                                   .Select(p => p.GetAttributeValue("title", "")).Where(p => p != "").GroupBy(p => p.ToString())
                                   .ToList();
                var titleList2 = doc2.DocumentNode.SelectNodes("//div[@class='article-item__location']")
                   .Select(p => p.InnerText.Replace("\n", " ").Replace("\t", " ").Replace(@"\ ", " "))
                   .ToList();
                var titleList3 = doc2.DocumentNode.SelectNodes("//p[@class='article-item__lead']")
   .Select(p => p.InnerText.Replace("\n", " ").Replace("\t", " ").Replace(@"\ ", " "))
   .ToList();
                response.Close();
                readStream.Close();

                int index = 5;

                DataTable dt = new DataTable();
                //    GetWiadomosciPilka();

                if (newUser == true)
                {
                    index = titleList.Count > 15 ? 15 : titleList.Count;
                    if (dt.Rows.Count == 0)
                    {
                        //    AddWiadomosc(hrefList);
                    }
                }

                else
                {
                    if (dt.Rows.Count > 0)
                    {
                        List<int> deleteList = new List<int>();
                        var listTemp = new List<System.Linq.IGrouping<string, string>>();
                        var imageListTemp = new List<string>();
                        var titleListTemp = new List<IGrouping<string, string>>();

                        for (int i = 0; i < hrefList.Count; i++)
                        {
                            if (dt.Rows[dt.Rows.Count - 1]["Wiadomosc1"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc2"].ToString() != hrefList[i].Key &&
                                dt.Rows[dt.Rows.Count - 1]["Wiadomosc3"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc4"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc5"].ToString() != hrefList[i].Key
                            )
                            {
                                listTemp.Add(hrefList[i]);
                                imageListTemp.Add("http://www.gkskatowice.eu" + imgList[i]);
                                titleListTemp.Add(titleList[i]);
                            }
                            listTemp2.Add(hrefList[i]);
                        }
                        hrefList = listTemp;
                        index = hrefList.Count;
                        imgList = imageListTemp;
                        titleList = titleListTemp;
                        //   AddWiadomosc(listTemp2);
                    }
                    else
                    {
                        index = 5;
                        //   AddWiadomosc(hrefList);
                    }
                }

                for (int i = 0; i < index; i++)
                {
                    string link = "";
                    if (hrefList[i].Key.Contains("http"))
                    {
                        link = hrefList[i].Key;
                    }
                    else
                    {
                        link = "http://uml.lodz.pl" + hrefList[i].Key;
                        //link = "http://www.orlenliga.pl/" + hrefList[i].Key;
                    }

                    if (link.Contains("video"))
                    {
                        list.Add(GetHeroCard(
                        titleList[i].Key.Replace("&quot;", ""), "", "",
                        new CardImage(url: imgList[i]),
                        new CardAction(ActionTypes.OpenUrl, "Oglądaj video", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }
                    else
                        if (link.Contains("gallery"))
                    {
                        list.Add(GetHeroCard(
                        titleList[i].Key.Replace("&quot;", ""), titleList2[i].Replace("\n", ""), "",
                        new CardImage(url: imgList[i]),
                        new CardAction(ActionTypes.OpenUrl, "Przeglądaj galerie", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }
                    else
                    {
                        list.Add(GetHeroCard(
                        titleList[i].Key.Replace("&quot;", ""), titleList2[i].Replace("\n", "").Replace("  ", ""), titleList3[i].Replace("\n", "").Replace("  ", "").Replace("więcej", ""),
                        new CardImage(url: "http://uml.lodz.pl/" + imgList[i]),
                        new CardAction(ActionTypes.OpenUrl, "Więcej", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }

                    //  list.Add(new Microsoft.Bot.Connector.VideoCard(titleList[i], "", "",null)
                }
            }
            if (listTemp2.Count > 0)
            {
                hrefList = listTemp2;
            }

            return list;

        }
        private static Attachment GetHeroCard(string title, string subtitle, string text, CardImage cardImage, CardAction cardAction, CardAction cardAction2)
        {
            if (cardAction2 != null)
            {
                var heroCard = new HeroCard
                {
                    Title = title,
                    Subtitle = subtitle,
                    Text = text,
                    Images = new List<CardImage>() { cardImage },
                    Buttons = new List<CardAction>() { cardAction, cardAction2 },
                };

                return heroCard.ToAttachment();
            }
            else
            {
                var heroCard = new HeroCard
                {
                    Title = title,
                    Subtitle = subtitle,
                    Text = text,
                    Images = new List<CardImage>() { cardImage },
                    Buttons = new List<CardAction>() { cardAction },
                };

                return heroCard.ToAttachment();
            }
        }

  
        public static DataTable GetWiadomosci()
        {
            DataTable dt = new DataTable();

            try
            {
                SqlConnection sqlConnection1 = new SqlConnection("Server=tcp:plps.database.windows.net,1433;Initial Catalog=PLPS;Persist Security Info=False;User ID=tomasoft;Password=Tomason18,;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                SqlCommand cmd = new SqlCommand();

                cmd.CommandText = "SELECT * FROM [dbo].[Wiadomosci" + BaseDB.appName + "]";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;

                sqlConnection1.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                sqlConnection1.Close();
                return dt;
            }
            catch
            {
                BaseDB.AddToLog("Błąd pobierania wiadomości");
                return null;
            }
        }


        public static DataTable GetUser()
        {
            DataTable dt = new DataTable();

            try
            {
                SqlConnection sqlConnection1 = new SqlConnection("Server=tcp:plps.database.windows.net,1433;Initial Catalog=PLPS;Persist Security Info=False;User ID=tomasoft;Password=Tomason18,;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                SqlCommand cmd = new SqlCommand();

                cmd.CommandText = "SELECT * FROM [dbo].[User" + BaseDB.appName + "] where flgDeleted=0";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;

                sqlConnection1.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                sqlConnection1.Close();
                return dt;
            }
            catch
            {
                BaseDB.AddToLog("Błąd pobierania użytkowników");
                return null;
            }
        }


    }
}