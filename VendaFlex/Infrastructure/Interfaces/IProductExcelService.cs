using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VendaFlex.Data.Entities;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;
using VendaFlex.Core.Utils;

namespace VendaFlex.Infrastructure.Interfaces
{
    public interface IProductExcelService
    {
        Task<byte[]> ExportProductsToExcelAsync(IEnumerable<Product> products);
        void CreateHeaders(IXLWorksheet worksheet);
        void FillProductRow(IXLWorksheet worksheet, int row, Product product);
        void ApplyFormatting(IXLWorksheet worksheet, int lastRow);
        void CreateInstructionsSheet(XLWorkbook workbook);
        Task CreateCategoriesReferenceSheet(XLWorkbook workbook);
        Task CreateSuppliersReferenceSheet(XLWorkbook workbook);
        Task<OperationResult> ImportProductsFromExcelAsync(Stream fileStream, int userId);
        Task<Product> ParseProductFromRow(IXLRow row);
        List<string> ValidateProduct(Product product, int rowNumber);
        void UpdateProduct(Product existing, Product updated, int userId);

    }
}
