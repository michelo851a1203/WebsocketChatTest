using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
                        Dictionary<string, string> data = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);

                        // 用來儲存用戶端連結
                        if (!ModList.Exists(p => p.userName == data["userName"]))
                        {
                            ChatMod _mod = new ChatMod();
                            _mod.socketKey = arg.SecWebSocketKey;
                            _mod.userName = data["userName"];
                            _mod.webst = socket;
                            ModList.Add(_mod);
                        }

                        // 存取紀錄 :
                        //DBInsert(data["userName"], data["title"]);
                        // 接收到後開始回傳

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

        public string DBInsert(string Name,string title)
        {

            string _result = string.Empty;
            using (clsDB db = new clsDB())
            {
                string[] _col = {
                    "Name",
                    "title",
                    "createDate",
                };
                string _parmName = string.Empty;
                string _parmValue = string.Empty;
                foreach (string item in _col)
                {
                    _parmName += $"{item},";
                    _parmValue += $"@{item},";
                }

                string table = "table";
                string _sql = $" insert into {table} (id,{_parmName.TrimEnd(',')}) output inserted.[id] ";
                _sql += $" select ISNULL(MAX([id]),0) + 1,{_parmValue.TrimEnd(',')} from {table} ";

                List<SqlParameter> _par = new List<SqlParameter>();
                _par.Add(new SqlParameter("@Name ", Name));
                _par.Add(new SqlParameter("@title ", title));
                _par.Add(new SqlParameter("@createDate ", DateTime.Now));

                _result = db.getResult(_sql, _par.ToArray());
            }
            return _result;
        }

    }

    // 聊天實體模組 : 
    public class ChatMod
    {
        public string socketKey { get; set; }
        public string title { get; set; }
        public string userName { get; set; }
        public WebSocket webst { get; set; }
    }
}