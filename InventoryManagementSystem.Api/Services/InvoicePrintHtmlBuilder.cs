using System.Globalization;
using System.Net;
using System.Text;
using InventoryManagementSystem.Application.Features.PurchaseInvoices.Dtos;
using InventoryManagementSystem.Application.Features.SalesInvoices.Dtos;

namespace InventoryManagementSystem.Api.Services;

public static class InvoicePrintHtmlBuilder
{
    private const string BrandName = "Inventory Management System";

    public static string BuildSalesInvoice(SalesInvoiceDto invoice)
    {
        ArgumentNullException.ThrowIfNull(invoice);

        var rows = invoice.Items.Select(item => new InvoicePrintRow(
            item.ProductName,
            item.Quantity,
            item.UnitPrice,
            item.Total));

        return Build(
            title: "Sales Invoice",
            partyLabel: "Customer",
            partyValue: invoice.CustomerId?.ToString() ?? "Walk-in customer",
            invoice.InvoiceNumber,
            invoice.WarehouseName,
            invoice.InvoiceDateUtc,
            invoice.Status.ToString(),
            invoice.Total,
            priceHeader: "Unit Price",
            rows);
    }

    public static string BuildPurchaseInvoice(PurchaseInvoiceDto invoice)
    {
        ArgumentNullException.ThrowIfNull(invoice);

        var rows = invoice.Items.Select(item => new InvoicePrintRow(
            item.ProductName,
            item.Quantity,
            item.UnitCost,
            item.Total));

        return Build(
            title: "Purchase Invoice",
            partyLabel: "Supplier",
            partyValue: invoice.SupplierId?.ToString() ?? "Unassigned supplier",
            invoice.InvoiceNumber,
            invoice.WarehouseName,
            invoice.InvoiceDateUtc,
            invoice.Status.ToString(),
            invoice.Total,
            priceHeader: "Unit Cost",
            rows);
    }

    private static string Build(
        string title,
        string partyLabel,
        string partyValue,
        string invoiceNumber,
        string warehouseName,
        DateTimeOffset invoiceDateUtc,
        string status,
        decimal total,
        string priceHeader,
        IEnumerable<InvoicePrintRow> rows)
    {
        var rowList = rows.ToList();
        var statusClass = status.Equals("Posted", StringComparison.OrdinalIgnoreCase)
            ? "status status-posted"
            : "status status-draft";
        var printedAtUtc = DateTimeOffset.UtcNow;

        var html = new StringBuilder();
        html.AppendLine("<!doctype html>");
        html.AppendLine("<html lang=\"en\">");
        html.AppendLine("<head>");
        html.AppendLine("<meta charset=\"utf-8\">");
        html.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">");
        html.AppendLine($"<title>{Encode(title)} #{Encode(invoiceNumber)}</title>");
        html.AppendLine("<style>");
        html.AppendLine("""
            :root {
                --ink: #172033;
                --muted: #667085;
                --line: #d8dee8;
                --soft: #f6f8fb;
                --panel: #ffffff;
                --accent: #14532d;
                --warning: #92400e;
            }

            * { box-sizing: border-box; }

            body {
                margin: 0;
                color: var(--ink);
                background: #eef1f5;
                font-family: Arial, Helvetica, sans-serif;
                font-size: 14px;
                line-height: 1.45;
            }

            .print-shell {
                width: min(960px, 100%);
                margin: 28px auto;
                padding: 0 20px;
            }

            .toolbar {
                display: flex;
                justify-content: flex-end;
                margin-bottom: 12px;
            }

            button {
                border: 0;
                background: #172033;
                color: #fff;
                padding: 10px 18px;
                font-weight: 700;
                cursor: pointer;
            }

            .invoice {
                min-height: 1120px;
                padding: 44px;
                background: var(--panel);
                box-shadow: 0 18px 45px rgba(23, 32, 51, 0.12);
            }

            .topbar {
                display: flex;
                align-items: flex-start;
                justify-content: space-between;
                gap: 32px;
                padding-bottom: 28px;
                border-bottom: 1px solid var(--line);
            }

            .brand {
                font-size: 13px;
                color: var(--muted);
                text-transform: uppercase;
                font-weight: 700;
            }

            h1 {
                margin: 8px 0 0;
                font-size: 36px;
                line-height: 1.05;
                letter-spacing: 0;
            }

            .invoice-number {
                min-width: 240px;
                text-align: right;
            }

            .label {
                color: var(--muted);
                font-size: 11px;
                font-weight: 700;
                text-transform: uppercase;
            }

            .value {
                margin-top: 4px;
                font-weight: 700;
                overflow-wrap: anywhere;
            }

            .status {
                display: inline-flex;
                margin-top: 12px;
                padding: 5px 10px;
                border: 1px solid currentColor;
                font-size: 11px;
                font-weight: 700;
                text-transform: uppercase;
            }

            .status-posted { color: var(--accent); }
            .status-draft { color: var(--warning); }

            .details {
                display: grid;
                grid-template-columns: repeat(4, minmax(0, 1fr));
                gap: 14px;
                margin: 30px 0 34px;
            }

            .detail {
                min-height: 82px;
                padding: 14px;
                border: 1px solid var(--line);
                background: var(--soft);
            }

            table {
                width: 100%;
                border-collapse: collapse;
                table-layout: fixed;
            }

            thead {
                display: table-header-group;
                background: #172033;
                color: #fff;
            }

            th, td {
                padding: 13px 12px;
                border-bottom: 1px solid var(--line);
                vertical-align: top;
            }

            th {
                font-size: 11px;
                font-weight: 700;
                text-align: left;
                text-transform: uppercase;
            }

            tbody tr:nth-child(even) { background: #fbfcfe; }
            .col-index { width: 58px; }
            .col-qty { width: 110px; }
            .col-money { width: 145px; }
            .number { text-align: right; }
            .product { font-weight: 700; overflow-wrap: anywhere; }

            .empty-row td {
                padding: 28px 12px;
                color: var(--muted);
                text-align: center;
            }

            .summary {
                display: flex;
                justify-content: flex-end;
                margin-top: 28px;
            }

            .summary-card {
                width: min(340px, 100%);
                border: 1px solid var(--line);
            }

            .summary-row {
                display: flex;
                justify-content: space-between;
                gap: 20px;
                padding: 13px 16px;
                border-bottom: 1px solid var(--line);
            }

            .summary-row:last-child {
                border-bottom: 0;
                background: #172033;
                color: #fff;
                font-size: 18px;
                font-weight: 700;
            }

            .footer {
                display: grid;
                grid-template-columns: 1fr 1fr;
                gap: 48px;
                margin-top: 72px;
                color: var(--muted);
            }

            .signature-line {
                margin-top: 42px;
                border-top: 1px solid var(--line);
                padding-top: 8px;
                color: var(--ink);
                font-weight: 700;
            }

            @page { size: A4; margin: 14mm; }

            @media (max-width: 760px) {
                .print-shell { margin: 0; padding: 0; }
                .invoice { min-height: auto; padding: 24px; box-shadow: none; }
                .topbar { display: block; }
                .invoice-number { margin-top: 22px; text-align: left; }
                .details { grid-template-columns: repeat(2, minmax(0, 1fr)); }
                .col-money { width: 120px; }
                .footer { grid-template-columns: 1fr; gap: 24px; }
            }

            @media print {
                body { background: #fff; }
                .print-shell { width: 100%; margin: 0; padding: 0; }
                .toolbar { display: none; }
                .invoice { min-height: auto; padding: 0; box-shadow: none; }
                .detail { background: #fff; }
                tbody tr:nth-child(even) { background: #fff; }
            }
            """);
        html.AppendLine("</style>");
        html.AppendLine("</head>");
        html.AppendLine("<body>");
        html.AppendLine("<div class=\"print-shell\">");
        html.AppendLine("<div class=\"toolbar\"><button type=\"button\" onclick=\"window.print()\">Print Invoice</button></div>");
        html.AppendLine("<main class=\"invoice\">");
        html.AppendLine("<section class=\"topbar\">");
        html.AppendLine("<div>");
        html.AppendLine($"<div class=\"brand\">{Encode(BrandName)}</div>");
        html.AppendLine($"<h1>{Encode(title)}</h1>");
        html.AppendLine($"<div class=\"{statusClass}\">{Encode(status)}</div>");
        html.AppendLine("</div>");
        html.AppendLine("<div class=\"invoice-number\">");
        html.AppendLine("<div class=\"label\">Invoice No.</div>");
        html.AppendLine($"<div class=\"value\">{Encode(invoiceNumber)}</div>");
        html.AppendLine("</div>");
        html.AppendLine("</section>");
        html.AppendLine("<section class=\"details\">");
        AppendDetail(html, "Invoice Date", invoiceDateUtc.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture));
        AppendDetail(html, "Warehouse", warehouseName);
        AppendDetail(html, partyLabel, partyValue);
        AppendDetail(html, "Printed At", printedAtUtc.ToString("yyyy-MM-dd HH:mm 'UTC'", CultureInfo.InvariantCulture));
        html.AppendLine("</section>");
        html.AppendLine("<table aria-label=\"Invoice items\">");
        html.AppendLine("<thead><tr>");
        html.AppendLine("<th class=\"col-index\">#</th>");
        html.AppendLine("<th>Product</th>");
        html.AppendLine("<th class=\"col-qty number\">Qty</th>");
        html.AppendLine($"<th class=\"col-money number\">{Encode(priceHeader)}</th>");
        html.AppendLine("<th class=\"col-money number\">Amount</th>");
        html.AppendLine("</tr></thead>");
        html.AppendLine("<tbody>");

        if (rowList.Count == 0)
        {
            html.AppendLine("<tr class=\"empty-row\"><td colspan=\"5\">No invoice items were added.</td></tr>");
        }
        else
        {
            for (var index = 0; index < rowList.Count; index++)
            {
                var row = rowList[index];
                html.AppendLine("<tr>");
                html.AppendLine($"<td>{(index + 1).ToString(CultureInfo.InvariantCulture)}</td>");
                html.AppendLine($"<td class=\"product\">{Encode(row.ProductName)}</td>");
                html.AppendLine($"<td class=\"number\">{row.Quantity.ToString(CultureInfo.InvariantCulture)}</td>");
                html.AppendLine($"<td class=\"number\">{FormatMoney(row.UnitPrice)}</td>");
                html.AppendLine($"<td class=\"number\">{FormatMoney(row.Total)}</td>");
                html.AppendLine("</tr>");
            }
        }

        html.AppendLine("</tbody>");
        html.AppendLine("</table>");
        html.AppendLine("<section class=\"summary\" aria-label=\"Invoice summary\">");
        html.AppendLine("<div class=\"summary-card\">");
        html.AppendLine($"<div class=\"summary-row\"><span>Subtotal</span><strong>{FormatMoney(total)}</strong></div>");
        html.AppendLine("<div class=\"summary-row\"><span>Adjustments</span><strong>0.00</strong></div>");
        html.AppendLine($"<div class=\"summary-row\"><span>Total</span><span>{FormatMoney(total)}</span></div>");
        html.AppendLine("</div>");
        html.AppendLine("</section>");
        html.AppendLine("<section class=\"footer\">");
        html.AppendLine("<div>");
        html.AppendLine("<div class=\"label\">Notes</div>");
        html.AppendLine("<p>Generated from the inventory system. Verify invoice status before fulfillment or accounting settlement.</p>");
        html.AppendLine("</div>");
        html.AppendLine("<div>");
        html.AppendLine("<div class=\"signature-line\">Authorized Signature</div>");
        html.AppendLine("</div>");
        html.AppendLine("</section>");
        html.AppendLine("</main>");
        html.AppendLine("</div>");
        html.AppendLine("</body>");
        html.AppendLine("</html>");

        return html.ToString();
    }

    private static void AppendDetail(StringBuilder html, string label, string value)
    {
        html.AppendLine("<div class=\"detail\">");
        html.AppendLine($"<div class=\"label\">{Encode(label)}</div>");
        html.AppendLine($"<div class=\"value\">{Encode(value)}</div>");
        html.AppendLine("</div>");
    }

    private static string FormatMoney(decimal value) => value.ToString("N2", CultureInfo.InvariantCulture);

    private static string Encode(string value) => WebUtility.HtmlEncode(value);

    private sealed record InvoicePrintRow(string ProductName, int Quantity, decimal UnitPrice, decimal Total);
}
