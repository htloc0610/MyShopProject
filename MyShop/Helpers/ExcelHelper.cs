using ClosedXML.Excel;
using MyShop.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace MyShop.Helpers;

/// <summary>
/// Helper class for Excel operations including template generation and data import.
/// </summary>
public static class ExcelHelper
{
    /// <summary>
    /// Creates an Excel template file for product import.
    /// </summary>
    /// <param name="filePath">Full path where the template will be saved</param>
    public static void CreateProductTemplate(string filePath)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Products");

        // Define column headers
        var headers = new[]
        {
            "ProductName",
            "Price",
            "CategoryName",
            "SKU",
            "Stock",
            "Description",
            "Image1",
            "Image2",
            "Image3"
        };

        // Add headers to first row
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightBlue;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }

        // Add sample data row
        worksheet.Cell(2, 1).Value = "Sản phẩm mẫu";
        worksheet.Cell(2, 2).Value = 100000;
        worksheet.Cell(2, 3).Value = "Electronics";
        worksheet.Cell(2, 4).Value = "SP001";
        worksheet.Cell(2, 5).Value = 10;
        worksheet.Cell(2, 6).Value = "Mô tả sản phẩm mẫu";
        worksheet.Cell(2, 7).Value = "https://via.placeholder.com/400";
        worksheet.Cell(2, 8).Value = "https://via.placeholder.com/400";
        worksheet.Cell(2, 9).Value = "https://via.placeholder.com/400";

        // Auto-fit columns
        worksheet.Columns().AdjustToContents();

        // Save workbook
        workbook.SaveAs(filePath);
    }

    /// <summary>
    /// Reads products from an Excel file and validates the structure.
    /// Returns products only if ALL rows are valid, otherwise returns errors and rejects the entire file.
    /// </summary>
    /// <param name="file">StorageFile representing the Excel file</param>
    /// <param name="categories">List of existing categories for mapping</param>
    /// <returns>Tuple containing list of products and validation errors</returns>
    public static async Task<(List<ProductImportDto> Products, List<string> Errors)> ReadProductsFromExcel(
        StorageFile file,
        List<Category> categories)
    {
        var products = new List<ProductImportDto>();
        var errors = new List<string>();

        try
        {
            // Read file content
            var stream = await file.OpenStreamForReadAsync();

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.FirstOrDefault();

            if (worksheet == null)
            {
                errors.Add("File Excel không chứa worksheet nào.");
                return (products, errors);
            }

            // Validate headers
            var expectedHeaders = new[] { "ProductName", "Price", "CategoryName", "SKU", "Stock", "Description" };
            var actualHeaders = new List<string>();

            for (int col = 1; col <= 6; col++)
            {
                var headerValue = worksheet.Cell(1, col).GetString();
                actualHeaders.Add(headerValue);
            }

            // Check if headers match
            for (int i = 0; i < expectedHeaders.Length; i++)
            {
                if (i >= actualHeaders.Count || !actualHeaders[i].Equals(expectedHeaders[i], StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add($"TIÊU ĐỀ CỘT SAI - Cột {i + 1}: Mong đợi '{expectedHeaders[i]}' nhưng nhận được '{(i < actualHeaders.Count ? actualHeaders[i] : "không có")}'");
                }
            }

            if (errors.Any())
            {
                errors.Insert(0, "FILE BỊ TỪ CHỐI - Tiêu đề cột không đúng định dạng.");
                return (products, errors);
            }

            // Read data rows (skip header row)
            var rowCount = worksheet.LastRowUsed()?.RowNumber() ?? 1;
            var tempProducts = new List<ProductImportDto>();
            var validationErrors = new List<string>();

            // First pass: Validate ALL rows
            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    var productName = worksheet.Cell(row, 1).GetString().Trim();
                    var priceText = worksheet.Cell(row, 2).GetString().Trim();
                    var categoryName = worksheet.Cell(row, 3).GetString().Trim();
                    var sku = worksheet.Cell(row, 4).GetString().Trim();
                    var stockText = worksheet.Cell(row, 5).GetString().Trim();
                    var description = worksheet.Cell(row, 6).GetString().Trim();

                    // Skip completely empty rows
                    if (string.IsNullOrWhiteSpace(productName) && 
                        string.IsNullOrWhiteSpace(priceText) && 
                        string.IsNullOrWhiteSpace(categoryName) &&
                        string.IsNullOrWhiteSpace(sku))
                        continue;

                    // Validate required fields
                    if (string.IsNullOrWhiteSpace(productName))
                    {
                        validationErrors.Add($"Dòng {row}: Tên sản phẩm không được để trống");
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(sku))
                    {
                        validationErrors.Add($"Dòng {row}: SKU không được để trống");
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(categoryName))
                    {
                        validationErrors.Add($"Dòng {row}: Loại sản phẩm không được để trống");
                        continue;
                    }

                    // Parse price
                    if (!decimal.TryParse(priceText, out var price) || price <= 0)
                    {
                        validationErrors.Add($"Dòng {row} ({productName}): Giá không hợp lệ ('{priceText}'). Giá phải là số dương.");
                        continue;
                    }

                    // Parse stock
                    if (!int.TryParse(stockText, out var stock) || stock < 0)
                    {
                        validationErrors.Add($"Dòng {row} ({productName}): Số lượng không hợp lệ ('{stockText}'). Số lượng phải là số không âm.");
                        continue;
                    }

                    // Find category by name
                    var category = categories.FirstOrDefault(c =>
                        c.Name.Equals(categoryName, StringComparison.OrdinalIgnoreCase));

                    if (category == null)
                    {
                        var availableCategories = string.Join(", ", categories.Select(c => $"'{c.Name}'"));
                        validationErrors.Add($"Dòng {row} ({productName}): Không tìm thấy loại sản phẩm '{categoryName}'. " +
                            $"Các loại có sẵn: {availableCategories}");
                        continue;
                    }

                    // Read image URLs (optional columns)
                    var imageUrls = new List<string>();
                    for (int col = 7; col <= 9; col++)
                    {
                        var imageUrl = worksheet.Cell(row, col).GetString().Trim();
                        if (!string.IsNullOrWhiteSpace(imageUrl))
                        {
                            imageUrls.Add(imageUrl);
                        }
                    }

                    // Create product DTO
                    var product = new ProductImportDto
                    {
                        Sku = sku,
                        Name = productName,
                        Price = price,
                        Stock = stock,
                        Description = description,
                        CategoryId = category.CategoryId,
                        CategoryName = categoryName,
                        ImageUrls = imageUrls
                    };

                    tempProducts.Add(product);
                }
                catch (Exception ex)
                {
                    validationErrors.Add($"Dòng {row}: Lỗi đọc dữ liệu - {ex.Message}");
                }
            }

            // Check if we have any validation errors
            if (validationErrors.Any())
            {
                errors.Add("FILE BỊ TỪ CHỐI - File chứa dữ liệu không hợp lệ.");
                errors.Add($"Tổng số lỗi: {validationErrors.Count}");
                errors.Add("");
                errors.Add("CHI TIẾT LỖI:");
                errors.AddRange(validationErrors);
                errors.Add("");
                errors.Add("Vui lòng sửa TẤT CẢ các lỗi và thử lại.");
                return (new List<ProductImportDto>(), errors);
            }

            if (tempProducts.Count == 0)
            {
                errors.Add("FILE BỊ TỪ CHỐI - File Excel không chứa dữ liệu hợp lệ nào.");
                errors.Add("Vui lòng kiểm tra lại file và đảm bảo có ít nhất 1 sản phẩm hợp lệ.");
                return (products, errors);
            }

            // All rows are valid, return the products
            products = tempProducts;
            
        }
        catch (Exception ex)
        {
            errors.Add($"FILE BỊ TỪ CHỐI - Lỗi đọc file Excel: {ex.Message}");
            errors.Add("Vui lòng kiểm tra file có đúng định dạng .xlsx không.");
        }

        return (products, errors);
    }
}
