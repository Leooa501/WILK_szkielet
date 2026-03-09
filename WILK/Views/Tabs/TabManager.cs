using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using WILK.Services;

namespace WILK.Views.Tabs
{
    public class TabManager : IDisposable
    {
        private readonly TabControl _tabControl;
        private readonly IEnterpriseDatabase _enterpriseDatabase;
        private readonly IFileProcessingService _fileProcessingService;
        private readonly IMainView _mainView;
        private readonly Dictionary<string, ITab> _tabs = new Dictionary<string, ITab>();
        private readonly Dictionary<string, Func<ITab>> _tabFactories = new Dictionary<string, Func<ITab>>();
        private ITab _currentTab;
        private bool _disposed = false;

        public event EventHandler<TabChangedEventArgs> TabChanged;

        public TabManager(TabControl tabControl, IEnterpriseDatabase enterpriseDatabase, IFileProcessingService fileProcessingService, IMainView mainView)
        {
            _tabControl = tabControl ?? throw new ArgumentNullException(nameof(tabControl));
            _enterpriseDatabase = enterpriseDatabase ?? throw new ArgumentNullException(nameof(enterpriseDatabase));
            _fileProcessingService = fileProcessingService ?? throw new ArgumentNullException(nameof(fileProcessingService));
            _mainView = mainView ?? throw new ArgumentNullException(nameof(mainView));

            _tabControl.SelectedIndexChanged += OnTabControlSelectedIndexChanged;
        }

        public void RegisterTab<T>(string tabName, Func<T> factory) where T : class, ITab
        {
            if (string.IsNullOrEmpty(tabName))
                throw new ArgumentException("Tab name cannot be null or empty", nameof(tabName));

            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            _tabFactories[tabName] = factory;
        }

        public void InitializeTab(string tabName)
        {
            if (!_tabFactories.ContainsKey(tabName))
                throw new ArgumentException($"Tab '{tabName}' is not registered", nameof(tabName));

            if (_tabs.ContainsKey(tabName) && _tabs[tabName].IsInitialized)
                return;

            var tab = _tabFactories[tabName]();
            tab.Initialize();
            _tabs[tabName] = tab;
            if (!_tabControl.TabPages.Contains(tab.TabPage))
            {
                _tabControl.TabPages.Add(tab.TabPage);
            }
        }

        public void InitializeAllTabs()
        {
            foreach (var tabName in _tabFactories.Keys)
            {
                InitializeTab(tabName);
            }

        }
        public void SelectInitialTab(string initialTabName = null)
        {
            ITab initialTab;

            if (!string.IsNullOrEmpty(initialTabName))
            {
                initialTab = GetTab<ITab>(initialTabName);
            }
            else
            {
                var selectedTabPage = _tabControl.SelectedTab;
                if (selectedTabPage == null) return;
                initialTab = _tabs.Values.FirstOrDefault(t => t.TabPage == selectedTabPage);
            }

            if (initialTab != null && _currentTab != initialTab)
            {
                _currentTab = initialTab;
                _currentTab.OnTabSelected();
            }
        }

        public T GetTab<T>(string tabName) where T : class, ITab
        {
            if (_tabs.TryGetValue(tabName, out var tab))
            {
                return tab as T;
            }
            return null;
        }

        public ITab GetCurrentTab()
        {
            return _currentTab;
        }

        public void SelectTab(string tabName)
        {
            if (!_tabs.TryGetValue(tabName, out var tab))
            {
                InitializeTab(tabName);
                tab = _tabs[tabName];
            }

            var tabPage = tab.TabPage;
            if (_tabControl.TabPages.Contains(tabPage))
            {
                _tabControl.SelectedTab = tabPage;
            }
        }

        public void RemoveTab(string tabName)
        {
            if (_tabs.TryGetValue(tabName, out var tab))
            {
                if (_currentTab == tab)
                {
                    _currentTab.OnTabDeselected();
                    _currentTab = null;
                }

                _tabControl.TabPages.Remove(tab.TabPage);
                tab.Dispose();
                _tabs.Remove(tabName);
            }
        }

        private void OnTabControlSelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedTab = _tabControl.SelectedTab;
            if (selectedTab == null) return;
            var newTab = _tabs.Values.FirstOrDefault(t => t.TabPage == selectedTab);
            if (newTab == null) return;
            if (_currentTab != newTab)
            {
                var oldTab = _currentTab;
                oldTab?.OnTabDeselected();

                _currentTab = newTab;
                _currentTab.OnTabSelected();

                TabChanged?.Invoke(this, new TabChangedEventArgs(oldTab, _currentTab));
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                if (_tabControl != null)
                {
                    _tabControl.SelectedIndexChanged -= OnTabControlSelectedIndexChanged;
                }

                foreach (var tab in _tabs.Values)
                {
                    tab?.Dispose();
                }
                _tabs.Clear();
                _tabFactories.Clear();
                
                _disposed = true;
            }
        }
    }

    public class TabChangedEventArgs : EventArgs
    {
        public ITab OldTab { get; }
        public ITab NewTab { get; }

        public TabChangedEventArgs(ITab oldTab, ITab newTab)
        {
            OldTab = oldTab;
            NewTab = newTab;
        }
    }
}