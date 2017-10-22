using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System.Web.Http;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Data.SqlClient;
using System.Data;
using Newtonsoft.Json.Linq;
using Parameters;
using GksKatowiceBot.Helpers;
using System.Json;

namespace GksKatowiceBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody] Activity activity)
        {
            try
            {
                if (activity.Type == ActivityTypes.Message)
                {

                    if (BaseDB.czyAdministrator(activity.From.Id) != null && (((activity.Text != null && activity.Text.IndexOf("!!!") == 0) || (activity.Attachments != null && activity.Attachments.Count > 0))))
                    {
                        WebClient client = new WebClient();

                        if (activity.Attachments != null)
                        {
                            //Uri uri = new Uri(activity.Attachments[0].ContentUrl);
                            string filename = activity.Attachments[0].ContentUrl.Substring(activity.Attachments[0].ContentUrl.Length - 4, 3).Replace(".", "");


                            //  WebClient client = new WebClient();
                            client.Credentials = new NetworkCredential("serwer1606926", "Tomason1910");
                            client.BaseAddress = "ftp://serwer1606926.home.pl/public_html/pub/";


                            byte[] data;
                            using (WebClient client2 = new WebClient())
                            {
                                data = client2.DownloadData(activity.Attachments[0].ContentUrl);
                            }
                            if (activity.Attachments[0].ContentType.Contains("image")) client.UploadData(filename + ".png", data); //since the baseaddress
                            else if (activity.Attachments[0].ContentType.Contains("video")) client.UploadData(filename + ".mp4", data);
                        }


                        CreateMessage(activity.Attachments, activity.Text == null ? "" : activity.Text.Replace("!!!", ""), activity.From.Id);

                    }
                    else
                    {
                        string komenda = "";
                        if (activity.ChannelData != null)
                        {
                            try
                            {
                                BaseDB.AddToLog("Przesylany Json " + activity.ChannelData.ToString());
                                dynamic stuff = JsonConvert.DeserializeObject(activity.ChannelData.ToString());
                                komenda = stuff.message.quick_reply.payload;
                                BaseDB.AddToLog("Komenda: " + komenda);
                            }
                            catch (Exception ex)
                            {
                                BaseDB.AddToLog("Bład rozkładania Jsona " + ex.ToString());
                            }
                        }

                        var toReply = activity.CreateReply(String.Empty);
                        var connectorNew = new ConnectorClient(new Uri(activity.ServiceUrl));
                        toReply.Type = ActivityTypes.Typing;
                        await connectorNew.Conversations.SendToConversationAsync(toReply);


                        MicrosoftAppCredentials.TrustServiceUrl(@"https://facebook.botframework.com", DateTime.MaxValue);
                        if (komenda == "DEVELOPER_DEFINED_PAYLOAD_Aktualnosci" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_Aktualnosci")
                        {
                           
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();

                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();
                            message.Attachments = BaseGETMethod.GetCardsAttachmentsAktualnosci(ref hrefList, true);
                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",

                                quick_replies = new dynamic[]
                            {
                                new
                                {
                                    content_type = "text",
                                    title = "Powiadomienia",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Powiadomienia",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                            //        image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Wydarzenia",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Wydarzenia",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Aktualnosci",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Aktualnosci",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Prześlij zdjęcie/wideo",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Przeslij",
                                //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },

                                                           }
                            });


                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;

                            //    message.Attachments = BaseGETMethod.GetCardsAttachments(ref hrefList, true);
                            message.Text = "Przesyłamy aktualności z życia miasta, możesz zarządzać powiadomieniami wybierając opcje powiadomienia :) ";
                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }
                        else if (komenda == "DEVELOPER_DEFINED_PAYLOAD_WlaczPowiadomienia" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_WlaczPowiadomienia")
                        {
                           
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();

                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();
                            BaseDB.ZmienPowiadomienia(userAccount.Id, 0);
                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",

                                quick_replies = new dynamic[]
                            {
                                new
                                {
                                    content_type = "text",
                                    title = "Powiadomienia",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Powiadomienia",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                            //        image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Wydarzenia",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Wydarzenia",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Aktualnosci",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Aktualnosci",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Prześlij zdjęcie/wideo",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Przeslij",
                                //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },

                                                           }
                            });


                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;

                            //    message.Attachments = BaseGETMethod.GetCardsAttachments(ref hrefList, true);
                            message.Text = "Powiadomienia zostały włączone, raz dziennie otrzymasz aktualności z Naszego miasta";
                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }
                        else if (komenda == "DEVELOPER_DEFINED_PAYLOAD_WylaczPowiadomienia" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_WylaczPowiadomienia")
                        {
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();

                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();
                            BaseDB.ZmienPowiadomienia(userAccount.Id, 1);
                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",

                                quick_replies = new dynamic[]
                            {
                                new
                                {
                                    content_type = "text",
                                    title = "Powiadomienia",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Powiadomienia",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                            //        image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Wydarzenia",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Wydarzenia",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Aktualnosci",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Aktualnosci",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Prześlij zdjęcie/wideo",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Przeslij",
                                //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },

                                                           }
                            });


                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;

                            //    message.Attachments = BaseGETMethod.GetCardsAttachments(ref hrefList, true);
                            message.Text = "Powiadomienia zostały wyłączone";
                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }
                        else if (komenda == "DEVELOPER_DEFINED_PAYLOAD_Wydarzenia" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_Wydarzenia")
                        {
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                            message.Attachments = BaseGETMethod.GetCardsAttachmentsWydarzenia1(ref hrefList, true, DateTime.Today.ToString("yyyy/MM/d"), DateTime.Today.ToString("yyyy/MM/d"));
                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",

                                quick_replies = new dynamic[]
                            {

                                new
                                {
                                    content_type = "text",
                                    title = "Dzisiaj",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_WydarzeniaDzisiaj",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                            //        image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Weekend",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_WydarzeniaWeekend",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                                                new
                                {
                                    content_type = "text",
                                    title = "Inny termin",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_WydarzeniaPozostale",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },

                                                           }
                            });

                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            //    message.Attachments = BaseGETMethod.GetCardsAttachments(ref hrefList, true);
                            message.Text = "Co ciekawego dzieje się w naszym mieście? ";
                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }
                        else if (komenda == "DEVELOPER_DEFINED_PAYLOAD_WydarzeniaWeekend" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_WydarzeniaWeekend")
                        {
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                         
                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",

                                quick_replies = new dynamic[]
                            {

                                new
                                {
                                    content_type = "text",
                                    title = "Piątek",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_WydarzeniaPiatek",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                            //        image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Sobota",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_WydarzeniaSobota",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                                                new
                                {
                                    content_type = "text",
                                    title = "Niedziela",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_WydarzeniaNiedziela",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },

                                                           }
                            });


                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            //    message.Attachments = BaseGETMethod.GetCardsAttachments(ref hrefList, true);
                            message.Text = "Wybierz dzień  :) ";
                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }
                        else if (komenda == "DEVELOPER_DEFINED_PAYLOAD_Powiadomienia" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_Powiadomienia")
                        {
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                            DateTime someDay = DateTime.Today;
                            var daysTillThursday = (int)DayOfWeek.Friday - (int)someDay.DayOfWeek;
                            var friday = someDay.AddDays(daysTillThursday);
                           // message.Attachments = BaseGETMethod.GetCardsAttachmentsWydarzenia(ref hrefList, true, friday.ToString("yyyy/MM/dd"));
                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",

                                quick_replies = new dynamic[]
 {

                                new
                                {
                                    content_type = "text",
                                    title = "Włącz powiadomienia",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_WlaczPowiadomienia",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                            //        image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Wyłącz powiadomienia",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_WylaczPowiadomienia",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                }
                            });

                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            //    message.Attachments = BaseGETMethod.GetCardsAttachments(ref hrefList, true);
                            message.Text = "Wybierz jedą z opcji";
                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }
                        else if (komenda == "DEVELOPER_DEFINED_PAYLOAD_WydarzeniaPiatek" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_WydarzeniaPiatek")
                        {
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                            DateTime someDay = DateTime.Today;
                            var daysTillThursday = (int)DayOfWeek.Friday - (int)someDay.DayOfWeek;
                            var friday = someDay.AddDays(daysTillThursday);
                            message.Attachments = BaseGETMethod.GetCardsAttachmentsWydarzenia(ref hrefList, true, friday.ToString("yyyy/MM/d"));
                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",

                                quick_replies = new dynamic[]
 {

                                new
                                {
                                    content_type = "text",
                                    title = "Piątek",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_WydarzeniaPiatek",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                            //        image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Sobota",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_WydarzeniaSobota",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                                                new
                                {
                                    content_type = "text",
                                    title = "Niedziela",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_WydarzeniaNiedziela",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },

                                }
                            });

                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            //    message.Attachments = BaseGETMethod.GetCardsAttachments(ref hrefList, true);
                            message.Text = "Sprawdź co ciekawego dzieje sie w Naszym mieście w piątek :) ";
                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }
                        else if (komenda == "DEVELOPER_DEFINED_PAYLOAD_WydarzeniaSobota" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_WydarzeniaSobota")
                        {
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                            DateTime someDay = DateTime.Today;
                            var daysTillThursday = (int)DayOfWeek.Saturday - (int)someDay.DayOfWeek;
                            var saturday = someDay.AddDays(daysTillThursday);
                            message.Attachments = BaseGETMethod.GetCardsAttachmentsWydarzenia(ref hrefList, true, saturday.ToString("yyyy/MM/d"));
                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",

                                quick_replies = new dynamic[]
   {
                                new
                                {
                                    content_type = "text",
                                    title = "Piątek",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_WydarzeniaPiatek",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                            //        image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Sobota",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_WydarzeniaSobota",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                                                new
                                {
                                    content_type = "text",
                                    title = "Niedziela",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_WydarzeniaNiedziela",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },

                                  }
                            });

                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            //    message.Attachments = BaseGETMethod.GetCardsAttachments(ref hrefList, true);
                            message.Text = "Sprawdź co ciekawego dzieje sie w Naszym mieście w sobotę :) ";
                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }
                        else if (komenda == "DEVELOPER_DEFINED_PAYLOAD_WydarzeniaNiedziela" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_WydarzeniaNiedziela")
                        {
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                            DateTime someDay = DateTime.Today;
                            var daysTillThursday = ((int)DayOfWeek.Sunday+7) - (int)someDay.DayOfWeek;
                            var saturday = someDay.AddDays(daysTillThursday);
                            message.Attachments = BaseGETMethod.GetCardsAttachmentsWydarzenia(ref hrefList, true, saturday.ToString("yyyy/MM/d"));
                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",

                                quick_replies = new dynamic[]
   {

                                new
                                {
                                    content_type = "text",
                                    title = "Piątek",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_WydarzeniaPiatek",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                            //        image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Sobota",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_WydarzeniaSobota",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                                                new
                                {
                                    content_type = "text",
                                    title = "Niedziela",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_WydarzeniaNiedziela",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },

                                  }
                            });

                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            //    message.Attachments = BaseGETMethod.GetCardsAttachments(ref hrefList, true);
                            message.Text = "Sprawdź co ciekawego dzieje sie w Naszym mieście w niedziele :) ";
                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }
                        else if (komenda == "DEVELOPER_DEFINED_PAYLOAD_WydarzeniaDzisiaj" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_WydarzeniaDzisiaj")
                        {
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                            message.Attachments = BaseGETMethod.GetCardsAttachmentsWydarzenia(ref hrefList, true,DateTime.Today.ToString("yyyy/MM/d"));
                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",

                                quick_replies = new dynamic[]
                            {

                                new
                                {
                                    content_type = "text",
                                    title = "Dzisiaj",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_WydarzeniaDzisiaj",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                            //        image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Weekend",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_WydarzeniaWeekend",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                                                new
                                {
                                    content_type = "text",
                                    title = "Inny termin",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_WydarzeniaPozostale",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },

                                                           }
                            });


                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            //    message.Attachments = BaseGETMethod.GetCardsAttachments(ref hrefList, true);
                            message.Text = "Co dzisiaj ciekawego dzieje się w Łodzi ??";
                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }
                        else if (komenda == "DEVELOPER_DEFINED_PAYLOAD_WydarzeniaPozostale" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_WydarzeniaPozostale")
                        {
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                           // message.Attachments = BaseGETMethod.GetCardsAttachmentsAktualnosci(ref hrefList, true);
                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",

                                quick_replies = new dynamic[]
                            {

                                new
                                {
                                    content_type = "text",
                                    title = "Dzisiaj",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_WydarzeniaDzisiaj",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                            //        image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Weekend",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_WydarzeniaWeekend",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                                                new
                                {
                                    content_type = "text",
                                    title = "Inny termin",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_WydarzeniaPozostale",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },

                                                           }
                            });


                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            //    message.Attachments = BaseGETMethod.GetCardsAttachments(ref hrefList, true);
                            message.Text = "Możesz wpisać konkretną datę, sprawdzę wydarzenia w tym terminie specjalnie dla Ciebie :) Datę wpisz w formacie Rok-Miesiąc-Dzień";
                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }
                        
     else if (komenda == "<GET_STARTED_PAYLOAD>" || activity.Text == "<GET_STARTED_PAYLOAD>" || activity.Text == "Rozpocznij" || activity.Text == "Get Started")
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            //               BaseDB.AddToLog("UserName: " + userStruct.userName + " User Id: " + userStruct.userId + " BOtId: " + userStruct.botId + " BotName: " + userStruct.botName + " url: " + userStruct.ServiceUrl);
                            BaseDB.AddUser(userStruct.userName, userStruct.userId, userStruct.botName, userStruct.botId, userStruct.ServiceUrl, 1);

                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();

                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",

                                quick_replies = new dynamic[]
                            {
                                new
                                {
                                    content_type = "text",
                                    title = "Powiadomienia",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Powiadomienia",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                            //        image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Wydarzenia",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Wydarzenia",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Aktualnosci",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Aktualnosci",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Prześlij zdjęcie/wideo",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Przeslij",
                                //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },
                                                           }
                            });


                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                            message.Text = "Witaj " + userAccount.Name.Substring(0, userAccount.Name.IndexOf(" ")) + " jak możemy Ci pomóc?";
                            // message.Attachments = GetCardsAttachments(ref hrefList, true);

                            await connector.Conversations.SendToConversationAsync((Activity)message);       // message.Attachments = GetCardsAttachments(ref hrefList, true);
                        }
                        else
                                if (activity.Text == "DEVELOPER_DEFINED_PAYLOAD_HELP")
                        {
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();

                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",

                                quick_replies = new dynamic[]
                            {
                                                               new
                                {
                                    content_type = "text",
                                    title = "Powiadomienia",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Powiadomienia",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                            //        image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Wydarzenia",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Wydarzenia",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Aktualnosci",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Aktualnosci",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Prześlij zdjęcie/wideo",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Przeslij",
                                //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },
                                                           }
                            });


                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                            message.Text = "Witaj " + userAccount.Name.Substring(0, userAccount.Name.IndexOf(" ")) + " jak możemy Ci pomóc?";
                            // message.Attachments = GetCardsAttachments(ref hrefList, true);

                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }

                        else
                                if (komenda== "DEVELOPER_DEFINED_PAYLOAD_Przeslij" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_Przeslij")
                        {
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();

                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",

                                quick_replies = new dynamic[]
                            {
                                                               new
                                {
                                    content_type = "text",
                                    title = "Powiadomienia",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Powiadomienia",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                            //        image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Wydarzenia",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Wydarzenia",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Aktualnosci",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Aktualnosci",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Prześlij zdjęcie/wideo",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Przeslij",
                                //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },
                                                           }
                            });


                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                            message.Text = "Jeśli chcesz się pochwalić swoją pracą prześlij nam ją na adres: @gmail.com - dzięki! Najlepsze opublikujemy ";
                            // message.Attachments = GetCardsAttachments(ref hrefList, true);

                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }

                        else
                        {
                            DateTime dt = new DateTime();
                            try
                            {
                                
                                dt = Convert.ToDateTime(activity.Text);
                            }
                            catch
                            {

                            }
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                           
                          
                            if (activity.Attachments.Count != 0)
                            {
                                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                                var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                                connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                                IMessageActivity message = Activity.CreateMessageActivity();
                                hrefList = new List<IGrouping<string, string>>();
                                // message.Attachments = BaseGETMethod.GetCardsAttachmentsWydarzenia(ref hrefList, true,dt.ToString("yyyy/MM/dd"));
                                message.ChannelData = JObject.FromObject(new
                                {
                                    notification_type = "REGULAR",

                                    quick_replies = new dynamic[]
                                {

                                new
                                {
                                    content_type = "text",
                                    title = "Dzisiaj",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_WydarzeniaDzisiaj",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                            //        image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Weekend",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_WydarzeniaWeekend",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                                                new
                                {
                                    content_type = "text",
                                    title = "Inny termin",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_WydarzeniaPozostale",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },

                                                               }
                                });


                                message.From = botAccount;
                                message.Recipient = userAccount;
                                message.Conversation = new ConversationAccount(id: conversationId.Id);
                                message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                                //    message.Attachments = BaseGETMethod.GetCardsAttachments(ref hrefList, true);
                                message.Text = "Dziękujemy za przesłane materiały, najlepsze wezmą udział w konkursie. O wyniku konkursu powiadomi Cie wirtualny asystent :)";
                                await connector.Conversations.SendToConversationAsync((Activity)message);
                            }



                           else  if ((int)dt.Year >=2000)
                            {
                                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                                var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                                connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                                IMessageActivity message = Activity.CreateMessageActivity();
                                hrefList = new List<IGrouping<string, string>>();
                                message.Attachments = BaseGETMethod.GetCardsAttachmentsWydarzenia(ref hrefList, true,dt.ToString("yyyy/MM/d"));
                                message.ChannelData = JObject.FromObject(new
                                {
                                    notification_type = "REGULAR",

                                    quick_replies = new dynamic[]
                                {

                                new
                                {
                                    content_type = "text",
                                    title = "Dzisiaj",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_WydarzeniaDzisiaj",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                            //        image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Weekend",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_WydarzeniaWeekend",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                                                new
                                {
                                    content_type = "text",
                                    title = "Inny termin",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_WydarzeniaPozostale",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },

                                                               }
                                });


                                message.From = botAccount;
                                message.Recipient = userAccount;
                                message.Conversation = new ConversationAccount(id: conversationId.Id);
                                message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                                //    message.Attachments = BaseGETMethod.GetCardsAttachments(ref hrefList, true);
                                message.Text = "W podanym przez Ciebie terminie możesz wziąć udział w wydarzeniach :)";
                                await connector.Conversations.SendToConversationAsync((Activity)message);

                            }

                            else
                            {
                                var lista = BaseGETMethod.GetCardsAttachmentsWyszukaj(ref hrefList, true, activity.Text);
                                if (lista.Count > 0)
                                {
                                    ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                    var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                                    var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                                    connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                    var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                                    IMessageActivity message = Activity.CreateMessageActivity();
                                    message.Attachments = BaseGETMethod.GetCardsAttachmentsWyszukaj(ref hrefList, true, activity.Text);


                                    message.From = botAccount;
                                    message.Recipient = userAccount;
                                    message.Conversation = new ConversationAccount(id: conversationId.Id);
                                    message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                                    //    message.Attachments = BaseGETMethod.GetCardsAttachments(ref hrefList, true);
                                    //message.Text = "W podanym przez Ciebie terminie możesz wziąść udział w takich wydarzeniach :)";
                                    await connector.Conversations.SendToConversationAsync((Activity)message);

                                    message.ChannelData = JObject.FromObject(new
                                    {
                                        notification_type = "REGULAR",

                                        quick_replies = new dynamic[]
                                  {

                                new
                                {
                                    content_type = "text",
                                    title = "Dzisiaj",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_WydarzeniaDzisiaj",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                            //        image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Weekend",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_WydarzeniaWeekend",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                                                new
                                {
                                    content_type = "text",
                                    title = "Inny termin",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_WydarzeniaPozostale",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },

                                                                 }
                                    });

                                    message.Text = "Zobacz pozostałe wydarzenia";
                                    message.Attachments = null;
                                    await connector.Conversations.SendToConversationAsync((Activity)message);
                                }

                                else
                                {
                                    ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                    var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                                    var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                                    connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                    var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                                    IMessageActivity message = Activity.CreateMessageActivity();

                                    message.ChannelData = JObject.FromObject(new
                                    {
                                        notification_type = "REGULAR",

                                        quick_replies = new dynamic[]
                                    {
                                new
                                {
                                    content_type = "text",
                                    title = "Powiadomienia",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Powiadomienia",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                            //        image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Wydarzenia",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Wydarzenia",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Aktualnosci",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Aktualnosci",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },                         new
                                {
                                    content_type = "text",
                                    title = "Prześlij zdjęcie/wideo",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Przeslij",
                                //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },
                                                                   }
                                    });

                                    message.From = botAccount;
                                    message.Recipient = userAccount;
                                    message.Conversation = new ConversationAccount(id: conversationId.Id);
                                    message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                                    hrefList = new List<IGrouping<string, string>>();
                                    //   message.Text = "Witaj " + userAccount.Name.Substring(0, userAccount.Name.IndexOf(" ")) + " jak możemy Ci pomóc?";
                                    // message.Attachments = BaseGETMethod.GetCardsAttachmentsGallery(ref hrefList, true);
                                    message.Text = "Niestety nie znalazłem pasujących wydarzeń. Może skorzystasz z moich podpowiedzi?";
                                    await connector.Conversations.SendToConversationAsync((Activity)message);
                                }
                            }
                        }
                    }
                }

                else
                {
                    HandleSystemMessage(activity);
                }
            }
            catch (Exception ex)
            {
                BaseDB.AddToLog("Wysylanie wiadomosci: " + ex.ToString());
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        public async static void CreateMessage(IList<Attachment> foto, string wiadomosc, string fromId)
        {
            try
            {
                BaseDB.AddToLog("Wywołanie metody CreateMessage");

                string uzytkownik = "";
                DataTable dt = BaseGETMethod.GetUser();

                try
                {
                    MicrosoftAppCredentials.TrustServiceUrl(@"https://facebook.botframework.com", DateTime.MaxValue);

                    IMessageActivity message = Activity.CreateMessageActivity();
                    message.ChannelData = JObject.FromObject(new
                    {
                        notification_type = "REGULAR",
                        quick_replies = new dynamic[]
                            {
                               new
                        {
                                    content_type = "text",
                                    title = "Piłka nożna",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Pilka_Nozna",
                                  //  image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },
                                new
                        {
                                    content_type = "text",
                                    title = "Siatkówka",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Siatkowka",
                                   // image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                },                                new
                        {
                                    content_type = "text",
                                    title = "Hokej",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Hokej",
                                   // image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },
                                                           }
                    });

                    message.AttachmentLayout = null;

                    if (foto != null && foto.Count > 0)
                    {
                        string filename = foto[0].ContentUrl.Substring(foto[0].ContentUrl.Length - 4, 3).Replace(".", "");

                        if (foto[0].ContentType.Contains("image")) foto[0].ContentUrl = "http://serwer1606926.home.pl/pub/" + filename + ".png";//since the baseaddress
                        else if (foto[0].ContentType.Contains("video")) foto[0].ContentUrl = "http://serwer1606926.home.pl/pub/" + filename + ".mp4";

                        //foto[0].ContentUrl = "http://serwer1606926.home.pl/pub/" + filename + ".png";

                        message.Attachments = foto;
                    }


                    //var list = new List<Attachment>();
                    //if (foto != null)
                    //{
                    //    for (int i = 0; i < foto.Count; i++)
                    //    {
                    //        list.Add(GetHeroCard(
                    //       foto[i].ContentUrl, "", "",
                    //       new CardImage(url: foto[i].ContentUrl),
                    //       new CardAction(ActionTypes.OpenUrl, "", value: ""),
                    //       new CardAction(ActionTypes.OpenUrl, "", value: "https://www.facebook.com/sharer/sharer.php?u=" + "")));
                    //    }
                    //}

                    message.Text = wiadomosc;
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        try
                        {
                            if (fromId != dt.Rows[i]["UserId"].ToString())
                            {

                                var userAccount = new ChannelAccount(name: dt.Rows[i]["UserName"].ToString(), id: dt.Rows[i]["UserId"].ToString());
                                uzytkownik = userAccount.Name;
                                var botAccount = new ChannelAccount(name: dt.Rows[i]["BotName"].ToString(), id: dt.Rows[i]["BotId"].ToString());
                                var connector = new ConnectorClient(new Uri(dt.Rows[i]["Url"].ToString()), "d2483171-4038-4fbe-b7a1-7d73bff7d046", "cUKwH06PFdwmLQoqpGYQLdJ");
                                var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                                message.From = botAccount;
                                message.Recipient = userAccount;
                                message.Conversation = new ConversationAccount(id: conversationId.Id, isGroup: false);
                                //await connector.Conversations.SendToConversationAsync((Activity)message).ConfigureAwait(false);

                                var returne = await connector.Conversations.SendToConversationAsync((Activity)message);
                            }
                        }
                        catch (Exception ex)
                        {
                            BaseDB.AddToLog("Błąd wysyłania wiadomości do: " + uzytkownik + " " + ex.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    BaseDB.AddToLog("Błąd wysyłania wiadomości do: " + uzytkownik + " " + ex.ToString());
                }
            }
            catch (Exception ex)
            {
                BaseDB.AddToLog("Błąd wysłania wiadomosci: " + ex.ToString());
            }
        }






        public static void CallToChildThread()
        {
            try
            {
                Thread.Sleep(5000);
            }

            catch (ThreadAbortException e)
            {
                Console.WriteLine("Thread Abort Exception");
            }
            finally
            {
                Console.WriteLine("Couldn't catch the Thread Exception");
            }
        }






        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                BaseDB.DeleteUser(message.From.Id);
            }
            else
                if (message.Type == ActivityTypes.ConversationUpdate)
            {
            }
            else
                    if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
            }
            else
                        if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else
                            if (message.Type == ActivityTypes.Ping)
            {
            }
            else
                                if (message.Type == ActivityTypes.Typing)
            {
            }
            return null;
        }







    }
}
