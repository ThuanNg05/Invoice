using Invoice.Core.Models;
using Invoice.Helpers;
using QuestPDF.Companion;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Invoice.Services;

public class InvoicePdfService
{
    private readonly int fontSize = 14;
    private string GetLogoPath()
    {
        // This gets the path where the app is running (e.g., bin\Debug\net8.0-windows...)
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string path = Path.Combine(baseDir, "Assets", "Logo.png");

        if (File.Exists(path)) return path;
        return null; // Return null if file not found
    }

    // ==========================================
    // VERSION 1: FINAL INVOICE (Hóa Đơn)
    // ==========================================
    public void GenerateOfficial(IEnumerable<TempInvoice> items, string customerName, string phoneNO, string invoiceCode, DateTime date, string filePath)
    {
        Document.Create(container =>
        {
            for (int i = 1; i <= 2; i++)
            {

                container.Page(page =>
                {
                    SetupPage(page);

                    // Header
                    page.Header().Column(col =>
                    {
                        // ====================================================
                        // 1. HÀNG ĐẦU TIÊN: LOGO (Trái) | THÔNG TIN HĐ (Phải)
                        // ====================================================
                        string titleText = (i == 1) ? "HÓA ĐƠN BÁN HÀNG" : "PHIẾU XUẤT KHO";

                        col.Item().Row(row =>
                        {
                            row.RelativeItem().AlignLeft().Height(74).Image(GetLogoPath()).FitArea();


                            // --- CỘT PHẢI (50%): THÔNG TIN HOÁ ĐƠN ---
                            row.RelativeItem().PaddingRight(10).AlignRight().Column(info =>
                            {
                                // Số HĐ (Màu đỏ)
                                info.Item().Text(text =>
                                {
                                    text.Span("Số HĐ: ").SemiBold().FontSize(fontSize);
                                    text.Span(invoiceCode).Bold().FontSize(fontSize);
                                });

                                // Khách hàng
                                info.Item().PaddingTop(5).Text(text =>
                                {
                                    text.Span("Khách hàng: ").SemiBold().FontSize(fontSize);
                                    text.Span(customerName).FontSize(fontSize);
                                });

                                // SĐT
                                info.Item().PaddingTop(5).Text(text =>
                                {
                                    text.Span("SĐT: ").SemiBold().FontSize(fontSize);
                                    text.Span(phoneNO).FontSize(fontSize);
                                });


                                // Ngày tháng
                                info.Item().PaddingTop(5).Text(text =>
                                {
                                    text.Span("Ngày: ").SemiBold().FontSize(fontSize);
                                    text.Span($"{date:dd/MM/yyyy HH:mm}").FontSize(fontSize);
                                });
                            });
                        });

                        // ====================================================
                        // 2. HÀNG THỨ HAI: ĐỊA CHỈ (Trái) | SĐT CỬA HÀNG (Phải)
                        // ====================================================
                        col.Item().PaddingTop(15).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                // Trái: Địa chỉ cửa hàng
                                c.Item().Text("ĐC: 397 tổ 15, ấp Long Tân,\r\n xã Long Điền, tỉnh An Giang").FontSize(13).Italic().AlignCenter();
                            });

                            row.RelativeItem().PaddingRight(10).AlignRight().Column(c =>
                            {
                                c.Item().PaddingRight(10).Text("ĐT: 0907.504.311 - 0344.627.378\r\n0907.504.105 - 0338.213.129").FontSize(13).Italic().AlignRight();
                            });
                        });


                        col.Item().PaddingTop(10).PaddingBottom(0).AlignCenter().Text(titleText)
                            .FontFamily("Times New Roman").FontSize(22).Bold().FontColor(Colors.Black);
                    });

                    // 2. Content                    
                    page.Content().PaddingVertical(10).Element(c => {
                        if (i == 1)
                        {
                            ComposeTable(c, items);
                        }
                        else
                        {
                            Sub_ComposeTable(c, items);
                        }
                    });

                    // 3. Footer
                    SetupFooter(page);
                });
            }
        })
        .GeneratePdf(filePath);
        //.ShowInCompanion();
    }

    // ==========================================
    // VERSION 2: TEMP INVOICE (Phiếu Tạm)
    // ==========================================
    public void GenerateTemp(IEnumerable<TempInvoice> items, string filePath)
    {
        Document.Create(container =>
        {
            container.Page(page =>
            {
                SetupPage(page);

                // 1. Simple Header
                page.Header().Element(header =>
                {
                    header.Column(col =>
                    {
                        col.Item().Text("PHIẾU TẠM TÍNH")
                            .FontSize(20).SemiBold().AlignCenter();

                        col.Item().AlignCenter().Text($"(Ngày: {DateTime.Now:dd/MM/yyyy HH:mm})").FontSize(10).Italic();

                        col.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    });
                });

                // 2. Content
                page.Content().PaddingVertical(10).Element(c => ComposeTable(c, items));

                // 3. Footer
                SetupFooter(page);
            });
        }).GeneratePdf(filePath);
    }

    // --- Shared Helper Methods ---

    private void SetupPage(PageDescriptor page)
    {
        page.Size(PageSizes.A4);
        page.Margin((float)0.5, Unit.Centimetre);
        page.PageColor(Colors.White);
        page.DefaultTextStyle(x => x.FontSize(12).FontFamily(Fonts.Arial));
    }

    private void SetupFooter(PageDescriptor page)
    {
        page.Footer()
            .AlignCenter()
            .Text(x =>
            {
                x.Span("Trang ");
                x.CurrentPageNumber();
                x.Span(" / ");
                x.TotalPages();
            });
    }

    // Main table
    private void ComposeTable(IContainer container, IEnumerable<TempInvoice> items)
    {
        container.Table(table =>
        {
            // Define Column Widths
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(30);  // STT
                columns.ConstantColumn(60);  // Mã SP
                columns.RelativeColumn();   // Tên SP
                columns.ConstantColumn(70);  // Đơn giá
                columns.ConstantColumn(40);  // SL
                columns.ConstantColumn(80);  // Thành tiền
                columns.RelativeColumn(1);   // Ghi chú
            });

            // Header Style
            IContainer HeaderStyle(IContainer c) => c.Border(1).Background(Colors.Grey.Lighten2).Padding(5).DefaultTextStyle(x => x.SemiBold());

            table.Header(header =>
            {
                header.Cell().Element(HeaderStyle).PaddingBottom(1).PaddingTop(1).AlignCenter().Text("#");
                header.Cell().Element(HeaderStyle).PaddingBottom(1).PaddingTop(1).AlignMiddle().AlignCenter().Text("Mã SP");
                header.Cell().Element(HeaderStyle).PaddingBottom(1).PaddingTop(1).AlignMiddle().AlignCenter().Text("Tên Sản Phẩm");
                header.Cell().Element(HeaderStyle).PaddingBottom(1).PaddingTop(1).AlignMiddle().AlignCenter().Text("Đơn giá");
                header.Cell().Element(HeaderStyle).PaddingBottom(1).PaddingTop(1).AlignMiddle().AlignCenter().Text("SL");
                header.Cell().Element(HeaderStyle).PaddingBottom(1).PaddingTop(1).AlignMiddle().AlignCenter().Text("Thành tiền");
                header.Cell().Element(HeaderStyle).PaddingBottom(1).PaddingTop(1).AlignMiddle().AlignCenter().Text("Ghi chú");
            });

            // Data Rows
            IContainer CellStyle(IContainer c) => c.Border(1).Padding(2);

            int index = 1;
            foreach (var item in items)
            {
                table.Cell().Element(CellStyle).AlignCenter().Text(index++);
                table.Cell().Element(CellStyle).Text(item.ProductID);
                table.Cell().Element(CellStyle).Text(item.ProductName);
                table.Cell().Element(CellStyle).AlignRight().Text(item.SellPrice.ToString("N0"));
                table.Cell().Element(CellStyle).AlignCenter().Text(item.Amount.ToString());
                table.Cell().Element(CellStyle).AlignRight().Text(item.LineTotal.ToString("N0"));
                table.Cell().Element(CellStyle).Text(item.Note ?? "");
            }

            // Grand Total
            table.Footer(footer =>
            {
                double grandTotal = items.Sum(x => x.LineTotal);
                int totalAmount = items.Sum(x => x.Amount);
                footer.Cell().ColumnSpan(4).Border(1).Padding(5).AlignRight().Text("Tổng Cộng:");
                footer.Cell().Border(1).Padding(5).AlignCenter().Text(totalAmount.ToString("N0")).Bold().FontSize(12);
                footer.Cell().Border(1).Padding(2).AlignRight().AlignMiddle().Text(grandTotal.ToString("N0")).Bold().FontSize(12);
                footer.Cell().Border(1);
                footer.Cell().ColumnSpan(7).Border(1).Padding(5).AlignLeft().Text($"Bằng chữ: {StringHelper.NumberToTextVN(grandTotal)}").Italic().FontSize(14).FontFamily("Times New Roman");
                footer.Cell().Border(1).Background(Colors.White);
            });
        });
    }

    // Sub table
    private void Sub_ComposeTable(IContainer container, IEnumerable<TempInvoice> items)
    {
        container.Table(table =>
        {
            // Define Column Widths
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(30);  // STT
                columns.ConstantColumn(60);  // Mã SP
                columns.RelativeColumn();   // Tên SP                
                columns.ConstantColumn(40);  // SL                
                columns.RelativeColumn(1);   // Ghi chú
            });

            // Header Style
            IContainer HeaderStyle(IContainer c) => c.Border(1).Background(Colors.Grey.Lighten2).Padding(5).DefaultTextStyle(x => x.SemiBold());

            table.Header(header =>
            {
                header.Cell().Element(HeaderStyle).PaddingBottom(1).PaddingTop(1).AlignCenter().Text("#");
                header.Cell().Element(HeaderStyle).PaddingBottom(1).PaddingTop(1).AlignMiddle().AlignCenter().Text("Mã SP");
                header.Cell().Element(HeaderStyle).PaddingBottom(1).PaddingTop(1).AlignMiddle().AlignCenter().Text("Tên Sản Phẩm");
                header.Cell().Element(HeaderStyle).PaddingBottom(1).PaddingTop(1).AlignMiddle().AlignCenter().Text("SL");
                header.Cell().Element(HeaderStyle).PaddingBottom(1).PaddingTop(1).AlignMiddle().AlignCenter().Text("Ghi chú");
            });

            // Data Rows
            IContainer CellStyle(IContainer c) => c.Border(1).Padding(2);

            int index = 1;
            foreach (var item in items)
            {
                table.Cell().Element(CellStyle).AlignCenter().Text(index++.ToString());
                table.Cell().Element(CellStyle).Text(item.ProductID);
                table.Cell().Element(CellStyle).Text(item.ProductName);
                table.Cell().Element(CellStyle).AlignCenter().Text(item.Amount.ToString());
                table.Cell().Element(CellStyle).Text(item.Note ?? "");
            }

            // Grand Total
            table.Footer(footer =>
            {
                double grandTotal = items.Sum(x => x.LineTotal);
                int totalAmount = items.Sum(x => x.Amount);
                footer.Cell().ColumnSpan(3).Border(1).Padding(5).AlignRight().Text("Tổng Cộng:");
                footer.Cell().Border(1).Padding(5).AlignCenter().Text(totalAmount.ToString("N0")).Bold().FontSize(12);
                footer.Cell().Border(1).Padding(5).AlignRight().Text(grandTotal.ToString("N0")).Bold().FontSize(12);
                footer.Cell().ColumnSpan(5).Border(1).Padding(5).AlignLeft().Text($"Bằng chữ: {StringHelper.NumberToTextVN(grandTotal)}").Italic().FontSize(14).FontFamily("Times New Roman");
                footer.Cell().Border(1).Background(Colors.White);
            });
        });
    }
}