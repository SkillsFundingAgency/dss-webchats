using Microsoft.Azure.Documents.Client;
using NCS.DSS.WebChat.Cosmos.Client;
using NUnit.Framework;

namespace NCS.DSS.WebChat.Tests.ServiceTests
{
    [TestFixture]
    public class DocumentDBClientTests
    {
        [Test]
        public void DocumentDBClientTests_ReturnsStatusCodeOK_WhenHttpResponseMessageOkIsCalledWithGuid()
        {
            var client = DocumentDBClient.CreateDocumentClient();

            Assert.IsInstanceOf<DocumentClient>(client);
            Assert.IsNotNull(client);
        }
    }
}
