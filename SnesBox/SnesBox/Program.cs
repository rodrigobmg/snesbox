
namespace SnesBox
{
#if WINDOWS || XBOX
    static class Program
    {
        static void Main(string[] args)
        {
            using (var game = new SnesBoxGame() { CartridgePath = args[0] })
            {
                game.Run();
            }
        }

    }
#endif
}

