namespace ZeroMenu.Tabs
{
    public interface ITab
    {
        string Name { get; }
        void Draw();
    }
}