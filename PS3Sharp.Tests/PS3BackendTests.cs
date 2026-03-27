using PS3Sharp.Types;

namespace PS3Sharp.Tests
{
    public class PS3BackendTests
    {
        [Fact]
        public void TMAPIClient_RequiresIPAddress()
        {
            Assert.Throws<ArgumentNullException>(() => new PS3Client(BackendType.TMAPI, null!));
        }

        [Fact]
        public void CCAPIClient_RequiresIPAddress()
        {
            Assert.Throws<ArgumentNullException>(() => new PS3Client(BackendType.CCAPI, null!));
        }

        [Fact]
        public void MAPIClient_RequiresIPAddress()
        {
            Assert.Throws<ArgumentNullException>(() => new PS3Client(BackendType.MAPI, null!));
        }

        [Fact]
        public void RPCS3Client_DefaultProcessName()
        {
            var client = new PS3Client();
            Assert.Equal(BackendType.RPCS3, client.ActiveBackendType);
        }

        [Fact]
        public void RPCS3Client_CustomProcessName()
        {
            var client = new PS3Client("rpcs3-custom");
            Assert.Equal(BackendType.RPCS3, client.ActiveBackendType);
        }

        [Fact]
        public void SelectBackend_SwitchesType()
        {
            var client = new PS3Client();
            Assert.Equal(BackendType.RPCS3, client.ActiveBackendType);

            client.SelectBackend(BackendType.TMAPI, ipAddress: "10.0.0.1");
            Assert.Equal(BackendType.TMAPI, client.ActiveBackendType);
        }
    }
}
