using System;
using System.Collections.Generic;

namespace AsperetaClient
{
    abstract class PacketHandler
    {
        public abstract string Prefix { get; }

        public List<Action<object>> Observers = new List<Action<object>>();

        public abstract object Parse(PacketParser p);

        public virtual void CallObservers(object obj)
        {
            for (int i = 0; i < Observers.Count; i++)
            {
                Observers[i].Invoke(obj);
            }
        }
    }

    class PacketManager
    {
        private Dictionary<string, PacketHandler> handlers = new Dictionary<string, PacketHandler>();
        private Dictionary<Type, PacketHandler> typeToHandler = new Dictionary<Type, PacketHandler>();

        public void Register<T>() where T : PacketHandler, new()
        {
            var handler = new T();
            handlers[handler.Prefix] = handler;
            typeToHandler[typeof(T)] = handler;
        }

        public void Listen<T>(Action<object> callback) where T : PacketHandler, new()
        {
            typeToHandler[typeof(T)].Observers.Add(callback);
        }

        public void Remove<T>(Action<object> callback) where T : PacketHandler, new()
        {
            typeToHandler[typeof(T)].Observers.Remove(callback);
        }

        public void Handle(string packet)
        {
            for (int i = 0; i < packet.Length; i++)
            {
                if (handlers.TryGetValue(packet.Substring(0, i + 1), out PacketHandler handler))
                {
                    var obj = handler.Parse(new PacketParser(packet, handler.Prefix));
                    handler.CallObservers(obj);

                    return;
                }
            }

            Console.WriteLine($"Can't handle packet: {packet}");
        }
    }

    class PacketParser
    {
        private string packet;
        private string prefix;
        private int index;

        public char Delimeter { get; set; } = ',';

        public PacketParser(string packet, string prefix)
        {
            this.packet = packet;
            this.prefix = prefix;
            this.index = prefix.Length;
        }

        public string GetRemaining()
        {
            if (index >= packet.Length)
                throw new InvalidOperationException($"Index {index} is out of bounds for packet {prefix}");

            return packet.Substring(index);
        }

        private string GetNextToken()
        {
            if (index >= packet.Length)
                throw new InvalidOperationException($"Index {index} is out of bounds for packet {prefix}");

            string strValue = null;

            for (int i = index; i < packet.Length; i++)
            {
                if (packet[i] == Delimeter)
                {
                    strValue = packet.Substring(index, i - index);
                    index = i + 1;
                    break;
                }
            }

            if (strValue == null)
            {
                strValue = packet.Substring(index);
                index = packet.Length;
            }

            //Console.WriteLine(strValue);

            return strValue;
        }

        public int GetInt32()
        {
            return Convert.ToInt32(GetNextToken());
        }

        public long GetInt64()
        {
            return Convert.ToInt64(GetNextToken());
        }
        public string GetString()
        {
            return GetNextToken();
        }

        public string GetSubstring(int length)
        {
            if (index + length >= packet.Length)
                throw new InvalidOperationException($"Substring is out of bounds for packet {prefix}");

            string result = packet.Substring(index, length);
            index += length;
            return result;
        }

        public char Peek()
        {
            return packet[index];
        }

        public int LengthRemaining()
        {
            if (index >= packet.Length)
                return 0;
            
            return packet.Length - index;
        }
    }
}
