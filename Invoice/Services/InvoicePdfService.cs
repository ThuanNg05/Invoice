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
        string baseDir;
        if (RuntimeHelper.IsMSIX)
        {
            baseDir = Windows.ApplicationModel.Package.Current.InstalledLocation.Path;
        }
        else
        {
            baseDir = AppDomain.CurrentDomain.BaseDirectory;
        }

        string path = Path.Combine(baseDir, "Assets", "Logo.png");
        return File.Exists(path) ? path : null;
    }

    public async Task GenerateOfficialAsync(IEnumerable<TempInvoice> items, string customerName, string phoneNO, string invoiceCode, DateTime date, string filePath)
    {
        var logoPath = GetLogoPath();
        await Task.Run(() =>
        {
            Document.Create(container =>
            {
                for (int i = 1; i <= 2; i++)
                {
                    var title = (i == 1) ? "HÓA ĐƠN BÁN HÀNG" : "PHIẾU XUẤT KHO";
                    container.Page(page =>
                    {
                        SetupPage(page);
                        page.Header().Component(new InvoiceHeaderComponent(title, invoiceCode, customerName, phoneNO, date, logoPath, fontSize));
                        
                        page.Content().PaddingVertical(10).Element(c => {
                            if (i == 1) ComposeTable(c, items);
                            else Sub_ComposeTable(c, items);
                        });

                        SetupFooter(page);
                    });
                }
            })
            .GeneratePdf(filePath);
        });
    }

    public async Task GenerateTempAsync(IEnumerable<TempInvoice> items, string filePath)
    {
        await Task.Run(() =>
        {
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    SetupPage(page);
                    page.Header().Element(header =>
                    {
                        header.Column(col =>
                        {
                            col.Item().Text("PHIẾU TẠM TÍNH").FontSize(20).SemiBold().AlignCenter();
                            col.Item().AlignCenter().Text($"(Ngày: {DateTime.Now:dd/MM/yyyy HH:mm})").FontSize(10).Italic();
                            col.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                        });
                    });
                    page.Content().PaddingVertical(10).Element(c => ComposeTable(c, items));
                    SetupFooter(page);
                });
            }).GeneratePdf(filePath);
        });
    }

    private void SetupPage(PageDescriptor page)
    {
        page.Size(PageSizes.A4);
        page.Margin(0.5f, Unit.Centimetre);
        page.PageColor(Colors.White);
        page.DefaultTextStyle(x => x.FontSize(12).FontFamily(Fonts.Arial));
    }

    private void SetupFooter(PageDescriptor page)
    {
        page.Footer().AlignCenter().Text(x =>
        {
            x.Span("Trang ");
            x.CurrentPageNumber();
            x.Span(" / ");
            x.TotalPages();
        });
    }

    private void ComposeTable(IContainer container, IEnumerable<TempInvoice> items)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(30);
                columns.RelativeColumn(3);
                columns.ConstantColumn(80);
                columns.ConstantColumn(40);
                columns.ConstantColumn(90);
                columns.RelativeColumn(2);
            });

            table.Header(header =>
            {
                header.Cell().Element(HeaderStyle).Text("#").AlignCenter();
                header.Cell().Element(HeaderStyle).Text("Tên Sản Phẩm");
                header.Cell().Element(HeaderStyle).Text("Đơn giá").AlignCenter();
                header.Cell().Element(HeaderStyle).Text("SL").AlignCenter();
                header.Cell().Element(HeaderStyle).Text("Thành tiền").AlignCenter();
                header.Cell().Element(HeaderStyle).Text("Ghi chú");
            });

            int index = 1;
            foreach (var item in items)
            {
                table.Cell().Element(CellStyle).AlignCenter().Text(index++);
                table.Cell().Element(CellStyle).Text(item.ProductName);
                table.Cell().Element(CellStyle).AlignRight().Text(item.SellPrice.ToString("N0"));
                table.Cell().Element(CellStyle).AlignCenter().Text(item.Amount.ToString());
                table.Cell().Element(CellStyle).AlignRight().Text(item.LineTotal.ToString("N0"));
                table.Cell().Element(CellStyle).Text(item.Note ?? "");
            }

            table.Footer(footer =>
            {
                double grandTotal = items.Sum(x => x.LineTotal);
                int totalAmount = items.Sum(x => x.Amount);
                footer.Cell().ColumnSpan(3).Border(1).Padding(5).AlignRight().Text("Tổng Cộng:");
                footer.Cell().Border(1).Padding(5).AlignCenter().Text(totalAmount.ToString("N0")).Bold().FontSize(12);
                footer.Cell().Border(1).Padding(5).AlignRight().Text(grandTotal.ToString("N0")).Bold().FontSize(12);
                footer.Cell().Border(1);
                footer.Cell().ColumnSpan(6).Border(1).Padding(5).Text($"Bằng chữ: {StringHelper.NumberToTextVN(grandTotal)}").Italic().FontSize(14).FontFamily("Times New Roman");
            });
        });
    }

    private void Sub_ComposeTable(IContainer container, IEnumerable<TempInvoice> items)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(30);
                columns.RelativeColumn(3);
                columns.ConstantColumn(40);
                columns.RelativeColumn(2);
            });

            table.Header(header =>
            {
                header.Cell().Element(HeaderStyle).Text("#").AlignCenter();
                header.Cell().Element(HeaderStyle).Text("Tên Sản Phẩm");
                header.Cell().Element(HeaderStyle).Text("SL").AlignCenter();
                header.Cell().Element(HeaderStyle).Text("Ghi chú");
            });

            int index = 1;
            foreach (var item in items)
            {
                table.Cell().Element(CellStyle).AlignCenter().Text(index++);
                table.Cell().Element(CellStyle).Text(item.ProductName);
                table.Cell().Element(CellStyle).AlignCenter().Text(item.Amount.ToString());
                table.Cell().Element(CellStyle).Text(item.Note ?? "");
            }

            table.Footer(footer =>
            {
                double grandTotal = items.Sum(x => x.LineTotal);
                int totalAmount = items.Sum(x => x.Amount);
                footer.Cell().ColumnSpan(2).Border(1).Padding(5).AlignRight().Text("Tổng Cộng:");
                footer.Cell().Border(1).Padding(5).AlignCenter().Text(totalAmount.ToString("N0")).Bold().FontSize(12);
                footer.Cell().Border(1).Padding(5).AlignRight().Text(grandTotal.ToString("N0")).Bold().FontSize(12);
                footer.Cell().ColumnSpan(4).Border(1).Padding(5).Text($"Bằng chữ: {StringHelper.NumberToTextVN(grandTotal)}").Italic().FontSize(14).FontFamily("Times New Roman");
            });
        });
    }

    private IContainer HeaderStyle(IContainer c) => c.Border(1).Background(Colors.Grey.Lighten2).Padding(5).DefaultTextStyle(x => x.SemiBold());
    private IContainer CellStyle(IContainer c) => c.Border(1).Padding(2);
}

public class InvoiceHeaderComponent : IComponent
{
    private string Title { get; }
    private string InvoiceCode { get; }
    private string CustomerName { get; }
    private string PhoneNO { get; }
    private DateTime Date { get; }
    private string LogoPath { get; }
    private int FontSize { get; }

    public InvoiceHeaderComponent(string title, string invoiceCode, string customerName, string phoneNO, DateTime date, string logoPath, int fontSize)
    {
        Title = title;
        InvoiceCode = invoiceCode;
        CustomerName = customerName;
        PhoneNO = phoneNO;
        Date = date;
        LogoPath = logoPath;
        FontSize = fontSize;
    }

    public void Compose(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Row(row =>
            {
                if (!string.IsNullOrEmpty(LogoPath))
                    row.RelativeItem().AlignLeft().Height(74).Image(LogoPath).FitArea();
                else
                    row.RelativeItem().AlignLeft().Height(74);

                row.RelativeItem().PaddingRight(10).AlignRight().Column(info =>
                {
                    info.Item().Text(text =>
                    {
                        text.Span("Số HĐ: ").SemiBold().FontSize(FontSize);
                        text.Span(InvoiceCode).Bold().FontSize(FontSize);
                    });
                    info.Item().PaddingTop(5).Text(text =>
                    {
                        text.Span("Khách hàng: ").SemiBold().FontSize(FontSize);
                        text.Span(CustomerName).FontSize(FontSize);
                    });
                    info.Item().PaddingTop(5).Text(text =>
                    {
                        text.Span("SĐT: ").SemiBold().FontSize(FontSize);
                        text.Span(PhoneNO).FontSize(FontSize);
                    });
                    info.Item().PaddingTop(5).Text(text =>
                    {
                        text.Span("Ngày: ").SemiBold().FontSize(FontSize);
                        text.Span($"{Date:dd/MM/yyyy HH:mm}").FontSize(FontSize);
                    });
                });
            });

            col.Item().PaddingTop(15).Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("ĐC: 397 tổ 15, ấp Long Tân,\r\n xã Long Điền, tỉnh An Giang").FontSize(13).Italic().AlignCenter();
                });
                row.RelativeItem().PaddingRight(10).AlignRight().Column(c =>
                {
                    c.Item().PaddingRight(10).Text("ĐT: 0907.504.311 - 0344.627.378\r\n0907.504.105 - 0338.213.129").FontSize(13).Italic().AlignRight();
                });
            });

            col.Item().PaddingTop(10).AlignCenter().Text(Title)
                .FontFamily("Times New Roman").FontSize(22).Bold().FontColor(Colors.Black);
        });
    }
}