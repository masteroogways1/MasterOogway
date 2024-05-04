using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TelegramFileBot
{
    class Program
    {
        // Dictionary to store processed file IDs
        static Dictionary<string, bool> processedFiles = new Dictionary<string, bool>();

        static async Task Main(string[] args)
        {
            string botToken = "6903825577:AAFrf18Q4H8GvqCiISe_mm2FxHG15GFqP3w";
            string apiUrl = $"https://api.telegram.org/bot{botToken}/";

            Console.WriteLine("Bot started. Listening for messages...");

            while (true)
            {
                await Task.Delay(1000);

                string updatesResponse = await SendRequest(apiUrl + "getUpdates");
                UpdateResponse updateResponse = JsonConvert.DeserializeObject<UpdateResponse>(updatesResponse);

                foreach (var update in updateResponse.result)
                {
                    if (update.message != null && update.message.document != null)
                    {
                        string fileId = update.message.document.file_id;

                        // Check if the file has been processed
                        if (!processedFiles.ContainsKey(fileId))
                        {
                            // If not processed, mark it as processed and send the message
                            processedFiles[fileId] = true;

                            string fileLink = $"http://yourfilehosting.com/{fileId}";
                            await SendMessage(apiUrl, update.message.chat.id, $"File uploaded! You can stream it from:\n{fileLink}");
                        }
                    }
                }
            }
        }

        static async Task<string> SendRequest(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
        }

        static async Task SendMessage(string apiUrl, long chatId, string text)
        {
            string requestBody = $"{{\"chat_id\":{chatId},\"text\":\"{text}\"}}";
            using (HttpClient client = new HttpClient())
            {
                StringContent content = new StringContent(requestBody, System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(apiUrl + "sendMessage", content);
                response.EnsureSuccessStatusCode();
            }
        }

        class UpdateResponse
        {
            public Update[] result { get; set; }
        }

        class Update
        {
            public Message message { get; set; }
        }

        class Message
        {
            public long message_id { get; set; }
            public Chat chat { get; set; }
            public Document document { get; set; }
        }

        class Document
        {
            public string file_id { get; set; }
            public string file_name { get; set; }
        }

        class Chat
        {
            public long id { get; set; }
        }
    }
}
