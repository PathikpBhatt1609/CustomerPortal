using CCM.API.Models; using QuestPDF.Fluent; using QuestPDF.Helpers; using QuestPDF.Infrastructure;
namespace CCM.API.Services;
public class PdfService {
    public byte[] GenerateRequestPdf(CustomerRequest req) {
        return Document.Create(c => { c.Page(page => {
            page.Size(PageSizes.A4); page.Margin(40); page.DefaultTextStyle(x=>x.FontSize(10));
            page.Header().Column(col => {
                col.Item().Row(row => { row.RelativeItem().Text("Customer Creation Request").Bold().FontSize(18).FontColor("#1565c0"); row.ConstantItem(120).AlignRight().Text(req.RequestNo).Bold().FontSize(13); });
                col.Item().PaddingTop(4).LineHorizontal(1).LineColor("#1565c0");
            });
            page.Content().PaddingTop(16).Column(col => {
                col.Item().Text("Customer Information").Bold().FontSize(12).FontColor("#1565c0");
                col.Item().PaddingTop(8).Table(t => {
                    t.ColumnsDefinition(c=>{c.RelativeColumn(2);c.RelativeColumn(3);c.RelativeColumn(2);c.RelativeColumn(3);});
                    void R(string k1,string v1,string k2="",string v2=""){t.Cell().Background("#e3f2fd").Padding(5).Text(k1).Bold();t.Cell().Padding(5).Text(v1??"");t.Cell().Background("#e3f2fd").Padding(5).Text(k2).Bold();t.Cell().Padding(5).Text(v2??"");}
                    R("Customer",req.CustomerName,"Type",req.CustomerType);
                    R("Contact",req.ContactPerson,"Email",req.Email);
                    R("Phone",req.Phone??"","GSTIN",req.GSTIN??"");
                    R("PAN",req.PAN??"","Industry",req.Industry??"");
                    R("Payment",req.PaymentTerms??"","Credit",req.CreditLimit.HasValue?"Rs."+req.CreditLimit:"");
                    R("Status",req.Status,"SAP ID",req.SapCustomerId??"");
                });
                col.Item().PaddingTop(10).Text("Billing Address").Bold().FontSize(11).FontColor("#1565c0");
                col.Item().PaddingTop(4).Background("#f5f9ff").Padding(8).Text(req.BillingAddress);
                if(!string.IsNullOrEmpty(req.Remarks)){col.Item().PaddingTop(8).Text("Remarks").Bold().FontSize(11).FontColor("#1565c0");col.Item().PaddingTop(4).Text(req.Remarks);}
                col.Item().PaddingTop(14).Text("Approval Timeline").Bold().FontSize(12).FontColor("#1565c0");
                col.Item().PaddingTop(6).Table(t=>{
                    t.ColumnsDefinition(c=>{c.ConstantColumn(140);c.RelativeColumn(2);c.RelativeColumn(3);});
                    t.Header(h=>{h.Cell().Background("#1565c0").Padding(5).Text("Time").FontColor(Colors.White).Bold();h.Cell().Background("#1565c0").Padding(5).Text("Role").FontColor(Colors.White).Bold();h.Cell().Background("#1565c0").Padding(5).Text("Action").FontColor(Colors.White).Bold();});
                    foreach(var tl in req.Timeline.OrderBy(x=>x.ActionAt)){t.Cell().BorderBottom(1).BorderColor("#eee").Padding(5).Text(tl.ActionAt.ToString("dd MMM yyyy HH:mm"));t.Cell().BorderBottom(1).BorderColor("#eee").Padding(5).Text(tl.Role);t.Cell().BorderBottom(1).BorderColor("#eee").Padding(5).Text(tl.Message);}
                });
            });
            page.Footer().AlignCenter().Text(t=>{t.Span("Generated: ").FontSize(8).FontColor(Colors.Grey.Medium);t.Span(DateTime.Now.ToString("dd MMM yyyy HH:mm")).FontSize(8).FontColor(Colors.Grey.Medium);t.Span("  |  CCM System").FontSize(8).FontColor(Colors.Grey.Medium);});
        }); }).GeneratePdf();
    }
}