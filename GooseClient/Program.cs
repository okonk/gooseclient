using System;

namespace GooseClient
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var client = new GameClient())
            {
                client.Run();
            }
        }
    }
}
