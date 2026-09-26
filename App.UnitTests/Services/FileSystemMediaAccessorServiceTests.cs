using App.Services;

namespace App.UnitTests.Services
{
    public class FileSystemMediaAccessorServiceTests
    {
        FileSystemMediaAccessorService sut;

        public FileSystemMediaAccessorServiceTests()
        {
            sut = new FileSystemMediaAccessorService();
        }


        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Exists_False_AssetDataRefIsNullOrWhitespace(string? assetDataRef)
        {
            Assert.False(sut.Exists(assetDataRef));
        }




    }
}
