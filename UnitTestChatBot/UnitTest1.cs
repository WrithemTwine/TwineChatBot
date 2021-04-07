using ChatBot_Net5.Data;

using NUnit.Framework;

namespace UnitTestChatBot
{
    public class Tests
    {
        public DataManager datamanager { get; set; }

        [SetUp]
        public void Setup()
        {
            datamanager = new();
        }

        [Test]
        public void Test1()
        {
            
            Assert.Pass();
        }
    }
}