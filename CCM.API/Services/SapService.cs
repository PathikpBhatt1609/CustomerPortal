using CCM.API.Data; using CCM.API.Models; using Microsoft.EntityFrameworkCore; using System.Text; using System.Text.Json;
namespace CCM.API.Services;
public class SapService {
    private readonly IServiceScopeFactory _sf; private readonly ILogger<SapService> _log;
    public SapService(IServiceScopeFactory sf, ILogger<SapService> l){_sf=sf;_log=l;}
    public void CreateCustomerFireAndForget(int id) => Task.Run(()=>CreateCustomerAsync(id));
    public async Task CreateCustomerAsync(int reqId) {
        using var scope=_sf.CreateScope();
        var db=scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var email=scope.ServiceProvider.GetRequiredService<EmailService>();
        var req=await db.CustomerRequests.Include(r=>r.Submitter).FirstOrDefaultAsync(r=>r.Id==reqId);
        if(req==null) return;
        var cfg=await db.SapConfigs.FirstOrDefaultAsync();
        if(cfg==null||cfg.Mode=="mock") {
            await Task.Delay(2000);
            req.SapCustomerId="C"+new Random().Next(1000000,9999999);
            req.SapCreatedAt=DateTime.UtcNow; req.Status="Approved & Created"; req.SapRawResponse="{\"mock\":true}";
            db.ApprovalTimelines.Add(new(){RequestId=req.Id,Role="System",Message="SAP Customer created (Mock). ID: "+req.SapCustomerId});
        } else {
            try {
                using var http=new HttpClient{Timeout=TimeSpan.FromMilliseconds(cfg.TimeoutMs)};
                if(!string.IsNullOrEmpty(cfg.SapUsername)) http.DefaultRequestHeaders.Add("Authorization","Basic "+Convert.ToBase64String(Encoding.UTF8.GetBytes(cfg.SapUsername+":"+cfg.SapPassword)));
                if(!string.IsNullOrEmpty(cfg.ClientNo)) http.DefaultRequestHeaders.Add("sap-client",cfg.ClientNo);
                var payload=new{BusinessPartnerCategory="2",BusinessPartnerFullName=req.CustomerName,BusinessPartnerGrouping=cfg.AccountGrp};
                var res=await http.PostAsync(cfg.BaseUrl+cfg.ApiPath+"/A_BusinessPartner",new StringContent(JsonSerializer.Serialize(payload),Encoding.UTF8,"application/json"));
                var raw=await res.Content.ReadAsStringAsync(); req.SapRawResponse=raw;
                if(res.IsSuccessStatusCode){
                    req.SapCustomerId=JsonDocument.Parse(raw).RootElement.GetProperty("d").GetProperty("BusinessPartner").GetString();
                    req.SapCreatedAt=DateTime.UtcNow; req.Status="Approved & Created";
                    db.ApprovalTimelines.Add(new(){RequestId=req.Id,Role="SAP System",Message="SAP Customer created. ID: "+req.SapCustomerId});
                } else { req.Status="SAP Error"; db.ApprovalTimelines.Add(new(){RequestId=req.Id,Role="SAP System",Message="SAP Error HTTP "+(int)res.StatusCode}); }
            } catch(Exception ex){
                req.Status="SAP Error"; req.SapRawResponse=ex.Message; _log.LogError(ex,"SAP failed {Id}",reqId);
                db.ApprovalTimelines.Add(new(){RequestId=req.Id,Role="SAP System",Message="SAP Exception: "+ex.Message[..Math.Min(80,ex.Message.Length)]});
            }
        }
        await db.SaveChangesAsync();
        if(req.Submitter!=null){
            bool ok=req.Status=="Approved & Created";
            await email.SendAsync(req.Submitter.Email,req.Submitter.FullName,
                ok?"[Completed] "+req.CustomerName+" created in SAP":"[Error] SAP failed for "+req.RequestNo,
                ok?"SAP ID: "+req.SapCustomerId+"\n\nRegards,\nCCM System":"SAP creation failed. Please contact admin.\n\nRegards,\nCCM System",reqId);
        }
    }
}