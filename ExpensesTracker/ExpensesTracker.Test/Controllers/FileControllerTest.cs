using System.Text;

using ExpensesTracker.API.Controllers;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ExpensesTracker.Test.Controllers
{
    [TestClass]
    public class FileControllerTest
    {
        private const string FileContent = "Value1\tValue2\tValue3\tValue4\tValue5\tValue6\tValue7\tValue8";
        private readonly byte[] _fileContentBytes = Encoding.UTF8.GetBytes(FileContent);

        [TestMethod]
        public async Task UploadFileReturnsOkWhenAllConditionsAreMet()
        {
            // Arrange
            FileController controller = new();
            FormFile file = new(
                baseStream: new MemoryStream(_fileContentBytes),
                baseStreamOffset: 0,
                length: _fileContentBytes.Length,
                name: "file",
                fileName: "file.tsv")
            {
                Headers = new HeaderDictionary(),
                ContentType = "text/tab-separated-values"
            };

            // Act
            ActionResult result = await controller.UploadFile(file);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            Assert.AreEqual((result as OkObjectResult)?.Value, "File uploaded successfully.");
        }

        [TestMethod]
        public async Task UploadFileReturnsBadRequestWhenFileIsNull()
        {
            // Arrange
            FileController controller = new();

            // Act

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            ActionResult result = await controller.UploadFile(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.


            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            Assert.AreEqual((result as BadRequestObjectResult)?.Value, "No file uploaded.");
        }

        [TestMethod]
        public async Task UploadFileReturnsBadRequestWhenFileLengthIsZero()
        {
            // Arrange
            FileController controller = new();
            FormFile file = new(
                baseStream: new MemoryStream(),
                baseStreamOffset: 0,
                length: 0,
                name: "file",
                fileName: "file.tsv")
            {
                Headers = new HeaderDictionary(),
                ContentType = "text/tab-separated-values"
            };

            // Act
            ActionResult result = await controller.UploadFile(file);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            Assert.AreEqual((result as BadRequestObjectResult)?.Value, "No file uploaded.");
        }

        [TestMethod]
        public async Task UploadFileReturnsBadRequestWhenFileHasWrongFileType()
        {
            // Arrange
            FileController controller = new();
            FormFile file = new(
                baseStream: new MemoryStream(_fileContentBytes),
                baseStreamOffset: 0,
                length: _fileContentBytes.Length,
                name: "file",
                fileName: "file.txt")
            {
                Headers = new HeaderDictionary(),
                ContentType = "text/tab-separated-values"
            };

            // Act
            ActionResult result = await controller.UploadFile(file);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            Assert.AreEqual((result as BadRequestObjectResult)?.Value, "Invalid file type. Only .TSV files are allowed.");
        }

        [TestMethod]
        public async Task UploadFileReturnsBadRequestWhenFileHasWrongContentType()
        {
            // Arrange
            FileController controller = new();
            FormFile file = new(
                baseStream: new MemoryStream(_fileContentBytes),
                baseStreamOffset: 0,
                length: _fileContentBytes.Length,
                name: "file",
                fileName: "file.tsv")
            {
                Headers = new HeaderDictionary(),
                // Provide a wrong content type
                ContentType = "application/json"
            };

            // Act
            ActionResult result = await controller.UploadFile(file);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            Assert.AreEqual((result as BadRequestObjectResult)?.Value, "Invalid file type. Only .TSV files are allowed.");
        }

        [TestMethod]
        public async Task UploadFileReturnsBadRequestWhenFileSizeIsTooBig()
        {
            // Arrange
            FileController controller = new();
            int fiveMBPlusOneByte = (5 * 1024 * 1024) + 1;
            byte[] fileContentBytes = new byte[fiveMBPlusOneByte];

            // Ensure the byte array contains at least one "\t"
            fileContentBytes[0] = (byte)'\t';

            FormFile file = new(
                baseStream: new MemoryStream(fileContentBytes),
                baseStreamOffset: 0,
                length: fiveMBPlusOneByte,
                name: "file",
                fileName: "file.tsv")
            {
                Headers = new HeaderDictionary(),
                ContentType = "text/tab-separated-values"
            };

            // Act
            ActionResult result = await controller.UploadFile(file);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            Assert.AreEqual((result as BadRequestObjectResult)?.Value, "File size exceeds the maximum limit of 5MB.");
        }
    }
}