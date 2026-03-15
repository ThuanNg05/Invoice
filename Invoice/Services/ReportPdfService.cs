using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Invoice.Services;

public class ReportPdfService
{
    public string GenerateFinancialReport(ReportData data, int year)
    {
        string fileName = $"BaoCao_TaiChinh_{year}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
        string filePath = Path.Combine(Path.GetTempPath(), fileName);

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                // Header
                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text($"BÁO CÁO TÀI CHÍNH NĂM {year}").FontSize(20).Bold().FontColor(Colors.Blue.Medium);
                        col.Item().Text($"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}");
                    });
                });

                // Content
                page.Content().PaddingVertical(10).Column(col =>
                {
                    // Summary Section
                    col.Item().Text("1. Tổng Quan").Bold().FontSize(14);
                    col.Item().PaddingBottom(10).Table(table =>
                    {
                        table.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); });
                        table.Cell().Border(1).Padding(5).Text("Tổng Doanh Thu");
                        table.Cell().Border(1).Padding(5).AlignRight().Text($"{data.TotalRevenue:N0} VND").Bold();
                        table.Cell().Border(1).Padding(5).Text("Tổng Lợi Nhuận (Ước tính)");
                        table.Cell().Border(1).Padding(5).AlignRight().Text($"{data.TotalProfit:N0} VND").Bold();
                    });

                    // Monthly Stats Table
                    col.Item().PaddingTop(10).Text("2. Chi Tiết Theo Tháng").Bold().FontSize(14);
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c => { c.ConstantColumn(50); c.RelativeColumn(); c.RelativeColumn(); });

                        table.Header(h =>
                        {
                            h.Cell().Border(1).Background(Colors.Grey.Lighten2).Padding(2).AlignCenter().Text("Tháng");
                            h.Cell().Border(1).Background(Colors.Grey.Lighten2).Padding(2).AlignCenter().Text("Số Đơn Hàng");
                            h.Cell().Border(1).Background(Colors.Grey.Lighten2).Padding(2).AlignCenter().Text("Doanh Thu");
                        });

                        foreach (var month in data.MonthlyStats)
                        {
                            table.Cell().Border(1).Padding(2).AlignCenter().Text(month.Month.ToString());
                            table.Cell().Border(1).Padding(2).AlignCenter().Text(month.OrderCount.ToString());
                            table.Cell().Border(1).Padding(2).AlignRight().Text($"{month.Revenue:N0}");
                        }
                    });

                    // Top Products
                    col.Item().PaddingTop(10).Text("3. Top 10 Sản Phẩm Bán Chạy").Bold().FontSize(14);
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c => { c.RelativeColumn(3); c.RelativeColumn(1); });
                        foreach (var p in data.TopProducts)
                        {
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(2).Text(p.ProductName);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(2).AlignRight().Text(p.TotalQuantity.ToString());
                        }
                    });
                });

                page.Footer().AlignCenter().Text(x => { x.CurrentPageNumber(); x.Span(" / "); x.TotalPages(); });
            });
        }).GeneratePdf(filePath);

        return filePath;
    }
}