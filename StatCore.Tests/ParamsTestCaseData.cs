using NUnit.Framework;

namespace StatCore.Tests
{
    public static class ParamsTestCaseData
    {
        public static TestCaseData Create<T>(params T[] items)
        {
            return new TestCaseData(items);
        }
    }
}