using System.Text.Json;

using ExpensesTracker.API.Models;

using Microsoft.AspNetCore.Mvc;

namespace ExpensesTracker.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        [HttpPost("upload")]
        public async Task<ActionResult> UploadFile(IFormFile file)
        {
            BadRequestObjectResult? fileValidationResult = ValidateUploadedFile(file);

            if (fileValidationResult == null)
            {
                await ProcessFile(file);
                return Ok("File uploaded successfully.");
            }

            return fileValidationResult;
        }

        private BadRequestObjectResult? ValidateUploadedFile(IFormFile file)
        {
            // Check if file was uploaded
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            // Validate the file extension
            string fileExtension = Path.GetExtension(file.FileName);

            if (!fileExtension.Equals(".tsv", StringComparison.CurrentCultureIgnoreCase) || file.ContentType != "text/tab-separated-values")
            {
                return BadRequest("Invalid file type. Only .TSV files are allowed.");
            }

            // Validate the file size
            int maxFileSize = 5 * 1024 * 1024; // 5MB

            if (file.Length > maxFileSize)
            {
                return BadRequest("File size exceeds the maximum limit of 5MB.");
            }

            // Validate the file content contains \t
            using StreamReader reader = new(file.OpenReadStream());
            string? firstLine = reader.ReadLine();

            return string.IsNullOrEmpty(firstLine) || !firstLine.Contains('\t')
                ? BadRequest("Invalid file content. The file must be in .TSV format.")
                : null;
        }

        private static async Task ProcessFile(IFormFile file)
        {
            List<Transaction> transactions = [];
            using StreamReader reader = new(file.OpenReadStream());

            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                string[] values = line.Split('\t');
                Transaction transaction = new()
                {
                    AccountNumber = values[0],
                    Currency = values[1],
                    ValueDate = values[2],
                    BalanceBefore = values[3],
                    BalanceAfter = values[4],
                    BookDate = values[5],
                    Amount = values[6],
                    Description = values[7],
                };

                transactions.Add(transaction);
            }
            JsonSerializerOptions jsonSerializerOptions = new()
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            string json = JsonSerializer.Serialize(transactions, jsonSerializerOptions);

            string outputDirectory = Path.Combine(".", "out");
            _ = Directory.CreateDirectory(outputDirectory);

            string filePath = Path.Combine(outputDirectory, "transactions.json");

            await System.IO.File.WriteAllTextAsync(filePath, json);
        }
    }
}