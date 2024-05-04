using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TelegramFileBot
{
    class Program
    {
        // Dictionary to store processed file IDs and their corresponding temporary URLs
        static Dictionary<string, string> fileUrls = new Dictionary<string, string>();

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
                    if (update.message != null)
                    {
                        if (update.message.document != null)
                        {
                            // Handle file uploads
                            string fileId = update.message.document.file_id;

                            // Check if the file has been processed
                            if (!fileUrls.ContainsKey(fileId))
                            {
                                // If not processed, generate a temporary URL for the file
                                string fileUrl = GenerateTemporaryUrl(fileId);

                                // Store the file URL
                                fileUrls[fileId] = fileUrl;

                                // Respond with the streaming link
                                await SendMessage(apiUrl, update.message.chat.id, $"File uploaded! You can stream it from:\n{fileUrl}");
                            }
                        }
                        else if (!string.IsNullOrWhiteSpace(update.message.text))
                        {
                            // Handle text messages
                            // Respond with a message prompting the user to send a file
                            await SendMessage(apiUrl, update.message.chat.id, "Send the file, child. Destiny can't be changed.");
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

        static string GenerateTemporaryUrl(string fileId)
        {
            // In this example, we construct a simple temporary URL with a placeholder for the file ID
            return $"http://example.com/{fileId}";
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
            public string text { get; set; }
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
