using StreamerBotLib.Models;
using StreamerBotLib.Models.Enums;
using StreamerBotLib.Systems;

namespace TestStreamerBot
{
    public class ManageStreamViewersTests
    {
        [Fact]
        public void EndStreamResetList_ShouldResetUserList()
        {
            // Arrange
            var manageStreamViewers = new ManageStreamViewers();
            var liveUser = new LiveUser("User1", Platform.Twitch);
            var manageStreamViewer = new ManageStreamViewer(liveUser, true, true, true, true);
            manageStreamViewers.AddUsersFirstJoinedChannel([liveUser]);

            // Act
            manageStreamViewers.EndStreamResetList();

            // Assert
            Assert.Empty(manageStreamViewers.GetCurrentActiveUsers(true));
        }

        [Fact]
        public void AddUsersFirstJoinedChannel_ShouldAddNewUsers()
        {
            // Arrange
            var manageStreamViewers = new ManageStreamViewers();
            var liveUser = new LiveUser("User1", Platform.Twitch);

            // Act
            var result = manageStreamViewers.AddUsersFirstJoinedChannel(new List<LiveUser> { liveUser });

            // Assert
            Assert.Single(result);
            Assert.Equal(liveUser, result.First());
        }

        [Fact]
        public void AddUsersFirstChatMessage_ShouldAddNewUsers()
        {
            // Arrange
            var manageStreamViewers = new ManageStreamViewers();
            var liveUser = new LiveUser("User1", Platform.Twitch);

            // Act
            var result = manageStreamViewers.AddUsersFirstChatMessage(new List<LiveUser> { liveUser });

            // Assert
            Assert.Single(result);
            Assert.Equal(liveUser, result.First());
        }

        [Fact]
        public void GetUsersLeft_ShouldReturnUsersWhoLeft()
        {
            // Arrange
            var manageStreamViewers = new ManageStreamViewers();
            var liveUser1 = new LiveUser("User1", Platform.Twitch);
            var liveUser2 = new LiveUser("User2", Platform.Twitch);
            manageStreamViewers.AddUsersFirstJoinedChannel(new List<LiveUser> { liveUser1, liveUser2 });

            // Act
            var currentActiveUsers = manageStreamViewers.GetCurrentActiveUsers(true);
            var result = manageStreamViewers.GetUsersLeft(new List<LiveUser> { liveUser1 });

            // Assert
            Assert.Equal(2, currentActiveUsers.Count);
            Assert.Single(result);
            Assert.Equal(liveUser2, result.First());
        }

        [Fact]
        public void GetCurrentActiveUsers_ShouldReturnActiveUsers()
        {
            // Arrange
            var manageStreamViewers = new ManageStreamViewers();
            var liveUser = new LiveUser("User1", Platform.Twitch);
            manageStreamViewers.AddUsersFirstJoinedChannel(new List<LiveUser> { liveUser });

            // Act
            var result = manageStreamViewers.GetCurrentActiveUsers(true);

            // Assert
            Assert.Single(result);
            Assert.Equal(liveUser, result.First());
        }
    }
}
