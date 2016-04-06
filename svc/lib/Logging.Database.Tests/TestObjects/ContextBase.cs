using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Logging.Database.TestObjects
{
    [TestClass]
    public abstract class ContextBase
    {
        [TestInitialize]
        public void Initialize()
        {
            this.Given();
            this.When();
        }

        [TestCleanup]
        public void Cleanup()
        {
            this.OnCleanup();
        }

        protected virtual void Given()
        {
        }

        protected virtual void When()
        {
        }

        protected virtual void OnCleanup()
        {
        }
    }
}
