using System;
using NCS.DSS.WebChat.Cosmos.Helper;
using NSubstitute;
using NUnit.Framework;

namespace NCS.DSS.WebChat.Tests.ServiceTests
{
    [TestFixture]
    public class DocumentDBHelperTests
    {
        [Test]
        public void DocumentDBHelperTests_ReturnsURI_WhenCreateDocumentUriIsCalled()
        {
            var uri = DocumentDBHelper.CreateDocumentUri(Arg.Any<Guid>());

            Assert.IsInstanceOf<Uri>(uri);
            Assert.IsNotNull(uri);
        }
        
        [Test]
        public void DocumentDBHelperTests_ReturnsURI_WhenDocumentCollectionUriIsCalled()
        {
            var uri = DocumentDBHelper.CreateDocumentCollectionUri();

            Assert.IsInstanceOf<Uri>(uri);
            Assert.IsNotNull(uri);
        }

        [Test]
        public void DocumentDBHelperTests_ReturnsURI_WhenCustomerCreateDocumentUriIsCalled()
        {
            var uri = DocumentDBHelper.CreateCustomerDocumentUri(Arg.Any<Guid>());

            Assert.IsInstanceOf<Uri>(uri);
            Assert.IsNotNull(uri);
        }

        [Test]
        public void DocumentDBHelperTests_ReturnsURI_WhenCustomerDocumentCollectionUriIsCalled()
        {
            var uri = DocumentDBHelper.CreateCustomerDocumentCollectionUri();

            Assert.IsInstanceOf<Uri>(uri);
            Assert.IsNotNull(uri);
        }
    }
}