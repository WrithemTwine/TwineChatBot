using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;

using ChatBot_Net5;
using ChatBot_Net5.Data;

using Xunit;

namespace TestProject1
{
    public class UnitTestStreamStats
    {
        private static DateTime TestStart = DateTime.Parse("04/18/2021 15:00:00");

        [Fact]
        public void TestAddEndStream()
        {
            DataManager dataManager = new();
            Statistics test = new(dataManager);

            test.StreamOnline();

            int chats = (int)(new Random().NextDouble()*100);
            int bits = (int)(new Random().NextDouble() * 500);

            DateTime nowstart = DateTime.Now;

            test.StartStreamOnline(nowstart);

            DataSource.StreamStatsRow[] teststart = (DataSource.StreamStatsRow[]) dataManager.StreamStats.Table.Select();
            DataSource.StreamStatsRow findstart = null;

            foreach(DataSource.StreamStatsRow d in teststart)
            {
                if(d.StreamStart==nowstart)
                {
                    findstart = d;
                }
            }

            Assert.NotNull(findstart);

            for (int i = 0; i < chats; i++)
            {
                test.AddTotalChats();
            }

            Thread.Sleep(4000);

            test.AddBits(bits);

            Thread.Sleep(60000);

            DateTime nowend = DateTime.Now;

            test.StreamOffline(nowend);

            DataSource.StreamStatsRow[] testend = (DataSource.StreamStatsRow[])dataManager.StreamStats.Table.Select();
            DataSource.StreamStatsRow findend = null;

            foreach (DataSource.StreamStatsRow d in testend)
            {
                if (d.StreamEnd == nowend)
                {
                    findend = d;
                }
            }

            Assert.NotNull(findend);
            Assert.Equal(bits, findend.Bits);
            Assert.Equal(chats, findend.TotalChats);
        }

        [Fact]
        public void AddSpecificStream1()
        {
            DataManager dataManager = new();
            Statistics test = new(dataManager);

            DateTime nowstart = TestStart;

            test.StartStreamOnline(nowstart);

            DataSource.StreamStatsRow[] teststart = (DataSource.StreamStatsRow[])dataManager.StreamStats.Table.Select();
            
            List<DataSource.StreamStatsRow> findstart = new();

            foreach (DataSource.StreamStatsRow d in teststart)
            {
                if (d.StreamStart == nowstart)
                {
                    findstart.Add(d);
                }
            }

            Assert.Single(findstart);
        }

        [Fact]
        public void AddSpecificStream2()
        {
            DataManager dataManager = new();
            Statistics test = new(dataManager);

            test.StreamOnline();

            int chats = (int)(new Random().NextDouble() * 100);
            int bits = (int)(new Random().NextDouble() * 500);

            DateTime nowstart = TestStart;

            test.StartStreamOnline(nowstart);

            DataSource.StreamStatsRow[] teststart = (DataSource.StreamStatsRow[])dataManager.StreamStats.Table.Select();
            List<DataSource.StreamStatsRow> findstart = new();

            foreach (DataSource.StreamStatsRow d in teststart)
            {
                if (d.StreamStart == nowstart)
                {
                    findstart.Add(d);
                }
            }

            Assert.Single(findstart);

            for (int i = 0; i < chats; i++)
            {
                test.AddTotalChats();
            }

            Thread.Sleep(4000);

            test.AddBits(bits);

            Thread.Sleep(60000);

            DateTime nowend = DateTime.Now;

            test.StreamOffline(nowend);

            DataSource.StreamStatsRow[] testend = (DataSource.StreamStatsRow[])dataManager.StreamStats.Table.Select();
            List<DataSource.StreamStatsRow> findend = new();

            foreach (DataSource.StreamStatsRow d in testend)
            {
                if (d.StreamEnd == nowend)
                {
                    findend.Add(d);
                }
            }

            Assert.NotNull(findend);
            Assert.Equal(bits, findend[0].Bits);
            Assert.Equal(chats, findend[0].TotalChats);
        }
    }
}
