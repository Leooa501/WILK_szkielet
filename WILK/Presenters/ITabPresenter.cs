using System;
using WILK.Services;

namespace WILK.Presenters
{
    /// <summary>
    /// Base interface for tab presenters managing tab lifecycle
    /// </summary>
    public interface ITabPresenter : IDisposable
    {
        void Initialize();
        void OnTabActivated();
        void OnTabDeactivated();
    }

    /// <summary>
    /// Base implementation providing common functionality for tab presenters
    /// </summary>
    public abstract class BaseTabPresenter : ITabPresenter
    {
        protected readonly IEnterpriseDatabase _enterpriseDatabase;
        private bool _disposed = false;

        protected BaseTabPresenter(IEnterpriseDatabase enterpriseDatabase)
        {
            _enterpriseDatabase = enterpriseDatabase ?? throw new ArgumentNullException(nameof(enterpriseDatabase));
        }

        public abstract void Initialize();
        
        public virtual void OnTabActivated()
        {
            // Override in derived classes if needed
        }

        public virtual void OnTabDeactivated()
        {
            // Override in derived classes if needed
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                // Override in derived classes to clean up resources
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}