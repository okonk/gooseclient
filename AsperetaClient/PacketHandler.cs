using System;
using System.Collections.Generic;

namespace AsperetaClient
{
    abstract class PacketParser
    {
        public abstract string Prefix { get; }

        public List<Action<object>> Observers = new List<Action<object>>();

        public abstract object Parse(string packet);

        public virtual void CallObservers(object obj)
        {
            for (int i = 0; i < Observers.Count; i++)
            {
                Observers[i].Invoke(obj);
            }
        }
    }

    class PacketHandler
    {
        private Dictionary<string, PacketParser> parsers = new Dictionary<string, PacketParser>();
        private Dictionary<Type, PacketParser> typeToParser = new Dictionary<Type, PacketParser>();

        public void Register<T>() where T : PacketParser, new()
        {
            var parser = new T();
            parsers[parser.Prefix] = parser;
            typeToParser[typeof(T)] = parser;
        }

        public void Listen<T>(Action<object> callback) where T : PacketParser, new()
        {
            typeToParser[typeof(T)].Observers.Add(callback);
        }

        public void Remove<T>(Action<object> callback) where T : PacketParser, new()
        {
            typeToParser[typeof(T)].Observers.Remove(callback);
        }

        public void Handle(string packet)
        {
            for (int i = 0; i < packet.Length; i++)
            {
                if (parsers.TryGetValue(packet.Substring(0, i), out PacketParser parser))
                {
                    var obj = parser.Parse(packet);
                    parser.CallObservers(obj);

                    return;
                }
            }

            Console.WriteLine($"Couldn't handle packet: {packet}");
        }
    }
}
