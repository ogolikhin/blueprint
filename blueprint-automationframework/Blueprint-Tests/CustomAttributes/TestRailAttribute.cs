using System;
using NUnit.Framework;

namespace CustomAttributes
{
    /// <summary>
    /// Lets you set the TestRail ID for a test.
    /// The NUnit XML will contain a property tag for tests with this attribute similar to the following:
    /// &lt;property name="TestRail" value="96093" /&gt;
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class TestRailAttribute : PropertyAttribute
    {
        public int TestRailId { get; }

        public TestRailAttribute(int testrailId) : base(testrailId)
        {
            TestRailId = testrailId;
        }
    }
}
