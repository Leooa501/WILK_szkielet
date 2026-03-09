namespace WILK.Views.Tabs
{
    public interface ITab
    {
        string TabName { get; }
        TabPage TabPage { get; }
        bool IsInitialized { get; }
        
        void Initialize();
        void OnTabSelected();
        void OnTabDeselected();
        void Dispose();
    }
}