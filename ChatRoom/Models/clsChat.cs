using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.WebSockets;

namespace ChatRoom.Models
{
    public class clsChat
    {
        public static List<ChatMod> ModList = new List<ChatMod>();
        public async Task websockets(AspNetWebSocketContext arg)
        {
            WebSocket socket = arg.WebSocket;
            while (true)
            {
                ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[1024]);
                WebSocketReceiveResult result = await socket.ReceiveAsync(buffer, CancellationToken.None);
                if (socket.State == WebSocketState.Open)
                {
                    string message = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);

                    if (message.Length > 0)
                    {
                        //Dictionary<string, string> data = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);

                        //// 用來儲存用戶端連結
                        //if (!ModList.Exists(p => p.userid == data["userid"]))
                        //{
                        //    ChatMod _mod = new ChatMod();
                        //    _mod.socketKey = arg.SecWebSocketKey;
                        //    _mod.userid = data["userid"];
                        //    _mod.webst = socket;
                        //    ModList.Add(_mod);
                        //}

                        // 接收到後開始回傳
                        //string toMessage = "回送資訊";
                        string toMessage = message;
                        buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(toMessage));
                        await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }
                else
                {
                    // 當 websocket 關閉時會到這
                    if (ModList.Exists(p => p.socketKey == arg.SecWebSocketKey))
                    {
                        ModList.Remove(ModList.FirstOrDefault(p => p.socketKey.Equals(arg.SecWebSocketKey)));
                    }
                    break;
                }
            }
        }
    }

    // 聊天實體模組 : 
    public class ChatMod
    {
        public string socketKey { get; set; }
        public string userid { get; set; }
        public WebSocket webst { get; set; }
    }
}