using CCM.API.Data; using CCM.API.DTOs; using CCM.API.Models; using CCM.API.Services;
using Microsoft.AspNetCore.Authorization; using Microsoft.AspNetCore.Mvc; using Microsoft.EntityFrameworkCore;
namespace CCM.API.Controllers;
[ApiController][Route("api/requests")][Authorize]
public class CustomerRequestController : ControllerBase {
    private readonly AppDbContext _db; private readonly EmailService _email;
    private readonly SapService _sap; private readonly PdfService _pdf; private readonly IConfiguration _cfg;
    public CustomerRequestController(AppDbContext db,EmailService e,SapService s,PdfService p,IConfiguration c){_db=db;_email=e;_sap=s;_pdf=p;_cfg=c;}
    int Uid=>int.Parse(User.FindFirst("id")!.Value);
    string URole=>User.FindFirst("role")!.Value;
    string UName=>User.FindFirst("name")!.Value;
    [HttpGet] public async Task<IActionResult> GetAll() {
        var q=_db.CustomerRequests.Include(r=>r.Submitter).Include(r=>r.L1Approver).Include(r=>r.L2Approver).AsQueryable();
        if(URole=="requestor") q=q.Where(r=>r.SubmittedBy==Uid);
        var list=await q.OrderByDescending(r=>r.SubmittedAt).ToListAsync();
        return Ok(list.Select(r=>new{r.Id,r.RequestNo,r.CustomerName,r.CustomerType,r.ContactPerson,r.Email,r.Phone,r.GSTIN,r.PAN,r.PaymentTerms,r.CreditLimit,r.Industry,r.BillingAddress,r.Remarks,r.Status,r.GstFileName,r.SapCustomerId,r.SapCreatedAt,r.L1Comment,r.L2Comment,r.SubmittedAt,SubmittedBy=r.Submitter!=null?r.Submitter.FullName:"",L1By=r.L1Approver!=null?r.L1Approver.FullName:"",r.L1ApprovedAt,L2By=r.L2Approver!=null?r.L2Approver.FullName:"",r.L2ApprovedAt}));
    }
    [HttpGet("queue")] public async Task<IActionResult> GetQueue() {
        var f=URole=="teamlead"?"Pending L1":"Pending L2";
        return Ok(await _db.CustomerRequests.Include(r=>r.Submitter).Where(r=>r.Status==f).OrderBy(r=>r.SubmittedAt).ToListAsync());
    }
    [HttpGet("{id}")] public async Task<IActionResult> GetById(int id) {
        var r=await _db.CustomerRequests.Include(r=>r.Submitter).Include(r=>r.L1Approver).Include(r=>r.L2Approver).Include(r=>r.Timeline).ThenInclude(t=>t.ActionBy).FirstOrDefaultAsync(r=>r.Id==id);
        return r==null?NotFound():Ok(r);
    }
    [HttpPost] public async Task<IActionResult> Create([FromForm] CustomerRequestDto dto, IFormFile? gstFile) {
        var req=new CustomerRequest{RequestNo="CR-"+DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString()[^6..],CustomerName=dto.CustomerName,CustomerType=dto.CustomerType,ContactPerson=dto.ContactPerson,Email=dto.Email,Phone=dto.Phone,GSTIN=dto.GSTIN,PAN=dto.PAN,PaymentTerms=dto.PaymentTerms,CreditLimit=dto.CreditLimit,Industry=dto.Industry,BillingAddress=dto.BillingAddress,Remarks=dto.Remarks,SubmittedBy=Uid};
        if(gstFile is{Length:>0}){
            var ext=Path.GetExtension(gstFile.FileName).ToLower();
            if(!new[]{".pdf",".jpg",".jpeg",".png"}.Contains(ext)) return BadRequest(new{error="Only PDF/JPG/PNG allowed."});
            var up=_cfg["FileStorage:GstUploadPath"]!; Directory.CreateDirectory(up);
            var fn=req.RequestNo+"_"+Path.GetFileName(gstFile.FileName);
            using var s=System.IO.File.Create(Path.Combine(up,fn)); await gstFile.CopyToAsync(s);
            req.GstFileName=fn; req.GstFilePath=Path.Combine(up,fn);
        }
        _db.CustomerRequests.Add(req); await _db.SaveChangesAsync();
        _db.ApprovalTimelines.Add(new(){RequestId=req.Id,ActionById=Uid,Role="Requestor",Message="Request submitted by "+UName});
        await _db.SaveChangesAsync();
        var tl=await _db.Users.FirstOrDefaultAsync(u=>u.Role=="teamlead"&&u.IsActive);
        var me=await _db.Users.FindAsync(Uid);
        if(tl!=null) await _email.SendAsync(tl.Email,tl.FullName,"[Action Required] New Request "+req.RequestNo,"Request "+req.RequestNo+" for "+req.CustomerName+" needs L1 approval.",req.Id);
        if(me!=null) await _email.SendAsync(me.Email,me.FullName,"[Submitted] Request "+req.RequestNo,"Your request "+req.RequestNo+" has been submitted.",req.Id);
        return Ok(new{req.Id,req.RequestNo,req.Status});
    }
    [HttpPost("{id}/approve")] public async Task<IActionResult> Approve(int id,[FromBody] ApprovalDto dto) {
        var req=await _db.CustomerRequests.Include(r=>r.Submitter).FirstOrDefaultAsync(r=>r.Id==id);
        if(req==null) return NotFound();
        if(req.Status=="Pending L1"&&URole=="teamlead"){
            req.Status="Pending L2";req.L1ApprovedBy=Uid;req.L1ApprovedAt=DateTime.UtcNow;req.L1Comment=dto.Comment;
            _db.ApprovalTimelines.Add(new(){RequestId=id,ActionById=Uid,Role="Team Lead",Message="L1 Approved by "+UName+(dto.Comment!=null?" - "+dto.Comment:"")});
            var mgr=await _db.Users.FirstOrDefaultAsync(u=>u.Role=="manager"&&u.IsActive);
            if(mgr!=null) await _email.SendAsync(mgr.Email,mgr.FullName,"[Action Required] L2 Approval - "+req.RequestNo,req.RequestNo+" needs your final approval.",id);
            if(req.Submitter!=null) await _email.SendAsync(req.Submitter.Email,req.Submitter.FullName,"[Update] Request "+req.RequestNo+" passed L1","Your request passed L1 and is awaiting final approval.",id);
        } else if(req.Status=="Pending L2"&&URole=="manager"){
            req.Status="Processing SAP";req.L2ApprovedBy=Uid;req.L2ApprovedAt=DateTime.UtcNow;req.L2Comment=dto.Comment;
            _db.ApprovalTimelines.Add(new(){RequestId=id,ActionById=Uid,Role="Manager",Message="L2 Approved by "+UName+" - sending to SAP"});
            await _db.SaveChangesAsync(); _sap.CreateCustomerFireAndForget(req.Id);
            return Ok(new{req.Status});
        } else return BadRequest(new{error="Action not allowed."});
        await _db.SaveChangesAsync(); return Ok(new{req.Status});
    }
    [HttpPost("{id}/reject")] public async Task<IActionResult> Reject(int id,[FromBody] ApprovalDto dto) {
        var req=await _db.CustomerRequests.Include(r=>r.Submitter).FirstOrDefaultAsync(r=>r.Id==id);
        if(req==null) return NotFound();
        req.Status="Rejected";
        _db.ApprovalTimelines.Add(new(){RequestId=id,ActionById=Uid,Role=URole,Message="Rejected by "+UName+" - "+(dto.Comment??"No reason")});
        await _db.SaveChangesAsync();
        if(req.Submitter!=null) await _email.SendAsync(req.Submitter.Email,req.Submitter.FullName,"[Rejected] Request "+req.RequestNo,"Your request was rejected. Reason: "+(dto.Comment??"No reason provided"),id);
        return Ok(new{req.Status});
    }
    [HttpGet("{id}/pdf")] public async Task<IActionResult> GetPdf(int id) {
        var req=await _db.CustomerRequests.Include(r=>r.Submitter).Include(r=>r.L1Approver).Include(r=>r.L2Approver).Include(r=>r.Timeline).ThenInclude(t=>t.ActionBy).FirstOrDefaultAsync(r=>r.Id==id);
        if(req==null) return NotFound();
        return File(_pdf.GenerateRequestPdf(req),"application/pdf",req.RequestNo+".pdf");
    }
    [HttpGet("export")] public async Task<IActionResult> ExportCsv() {
        var list=await _db.CustomerRequests.Include(r=>r.Submitter).Include(r=>r.L1Approver).Include(r=>r.L2Approver).OrderByDescending(r=>r.SubmittedAt).ToListAsync();
        var sb=new System.Text.StringBuilder();
        sb.AppendLine("RequestNo,CustomerName,Type,Contact,Email,GSTIN,Status,SapId,SubmittedBy,SubmittedAt,L1By,L2By,GstFile");
        foreach(var r in list) sb.AppendLine($"\"{r.RequestNo}\",\"{r.CustomerName}\",\"{r.CustomerType}\",\"{r.ContactPerson}\",\"{r.Email}\",\"{r.GSTIN}\",\"{r.Status}\",\"{r.SapCustomerId}\",\"{r.Submitter?.FullName}\",\"{r.SubmittedAt:yyyy-MM-dd HH:mm}\",\"{r.L1Approver?.FullName}\",\"{r.L2Approver?.FullName}\",\"{r.GstFileName}\"");
        return File(System.Text.Encoding.UTF8.GetBytes(sb.ToString()),"text/csv","CCM_"+DateTime.Today.ToString("yyyyMMdd")+".csv");
    }
}