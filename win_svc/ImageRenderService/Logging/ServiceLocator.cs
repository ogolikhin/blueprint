using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageRenderService.Logging
{   
    // http://msdn.microsoft.com/en-us/library/ff648968.aspx
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
    // By Design: there are classes that implement this base class, relying on an accessible constructor to be present.
    public class ServiceLocator<TService> where TService : class
    {
        protected ServiceLocator()
        {
            // hide
        }

        public static TService Current { get; private set; }

        public static void Init(TService controller)
        {
            Log.Assert(controller != null, "IService controller cannot be null");
            Log.Assert(Current == null, string.Format("{0} can be initialized only once during the current user session", typeof(TService).FullName));

            Current = controller;
        }

        public static void DisposeCurrent()
        {
            if (Current == null)
            {
                return;
            }

            var disposable = Current as IDisposable;
            Current = null;

            if (disposable != null)
            {
                disposable.Dispose();
            }
        }
    }
}
