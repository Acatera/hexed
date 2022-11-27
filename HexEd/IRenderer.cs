namespace HexEd
{
    public interface IRenderer
    {
        void BeginDraw();
        void Clear();
        void EndDraw();
        void PrintAt(int x, int y, string text);
        void PrintAt(int x, int y, string text, ConsoleColor foreColor, ConsoleColor backColor = ConsoleColor.Black);
    }
}