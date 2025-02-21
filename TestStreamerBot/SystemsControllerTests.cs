using Moq;

using StreamerBotLib.Enums;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Models;
using StreamerBotLib.Systems;

namespace TestStreamerBot
{
    public class SystemsControllerTests
    {
        private readonly SystemsController _controller;
        private readonly Mock<IDataManager> _mockDataManager;
        private readonly Mock<ActionSystem> _mockActionSystem;

        public SystemsControllerTests()
        {
            _mockDataManager = new Mock<IDataManager>();
            _mockActionSystem = new Mock<ActionSystem>();
            _controller = new SystemsController();
        }

        [Fact]
        public void TestAddUsers_ShouldAddUsers()
        {
            // Arrange
            var users = new List<LiveUser>
            {
                new LiveUser("User1", Platform.Twitch),
                new LiveUser("User2", Platform.Twitch)
            };

            // Act
            _controller.UserJoined(users);

            // Assert
            _mockActionSystem.Verify(x => x.UserJoined(It.IsAny<List<LiveUser>>(), It.IsAny<DateTime>()), Times.Once);
        }

        [Fact]
        public void NotifyBotStart_ShouldStartBot()
        {
            // Act
            _controller.NotifyBotStart();

            // Assert
            Assert.True(_controller.ChatBotStarted);
            _mockActionSystem.Verify(x => x.StartElapsedTimerThread(), Times.Once);
        }

        [Fact]
        public void NotifyBotStop_ShouldStopBot()
        {
            // Act
            _controller.NotifyBotStop();

            // Assert
            Assert.False(_controller.ChatBotStarted);
            _mockActionSystem.Verify(x => x.StopElapsedTimerThread(), Times.Once);
        }

        [Fact]
        public void Exit_ShouldJoinProcessMsgsThread()
        {
            // Arrange
            var processMsgsThread = new Thread(() => { });
            _controller.ProcessMsgs = processMsgsThread;

            // Act
            _controller.Exit();

            // Assert
            Assert.False(processMsgsThread.IsAlive);
            _mockDataManager.Verify(x => x.Exit(), Times.Once);
        }

        [Fact]
        public void AddNewFollowers_ShouldProcessFollowers()
        {
            // Arrange
            var followers = new List<Follow>
            {
                new Follow { FromUserName = "Follower1" },
                new Follow { FromUserName = "Follower2" }
            };

            // Act
            _controller.AddNewFollowers(followers);

            // Assert
            _mockDataManager.Verify(x => x.PostFollowers(It.IsAny<IEnumerable<Follow>>()), Times.Once);
        }

        [Fact]
        public void BeginGiveaway_ShouldStartGiveaway()
        {
            // Act
            _controller.BeginGiveaway();

            // Assert
            Assert.True(_controller.GiveawayStarted);
            Assert.Empty(_controller.GiveawayCollectionList);
        }

        [Fact]
        public void EndGiveaway_ShouldStopGiveaway()
        {
            // Act
            _controller.EndGiveaway();

            // Assert
            Assert.False(_controller.GiveawayStarted);
        }

        [Fact]
        public void PostGiveawayResult_ShouldPickWinner()
        {
            // Arrange
            var users = new List<LiveUser>
            {
                new LiveUser("User1", Platform.Twitch),
                new LiveUser("User2", Platform.Twitch)
            };
            _controller.GiveawayCollectionList.AddRange(users);

            // Act
            _controller.PostGiveawayResult();

            // Assert
            Assert.NotEmpty(_controller.GiveawayCollectionList);
        }

        [Fact]
        public void StreamOnline_ShouldStartStream()
        {
            // Arrange
            var currentTime = DateTime.Now;

            // Act
            var result = _controller.StreamOnline(currentTime);

            // Assert
            Assert.True(result);
            _mockActionSystem.Verify(x => x.StreamOnline(currentTime), Times.Once);
        }

        [Fact]
        public void StreamOffline_ShouldStopStream()
        {
            // Arrange
            var currentTime = DateTime.Now;

            // Act
            _controller.StreamOffline(currentTime);

            // Assert
            _mockActionSystem.Verify(x => x.StreamOffline(currentTime), Times.Once);
        }

        [Fact]
        public void SetCategory_ShouldUpdateCategory()
        {
            // Arrange
            var category = new CategoryData("Category1", "Description1");

            // Act
            _controller.SetCategory(category);

            // Assert
            Assert.Equal(category, _controller.CurrCategory);
            _mockActionSystem.Verify(x => x.SetCategory(category), Times.Once);
        }
    }
}
