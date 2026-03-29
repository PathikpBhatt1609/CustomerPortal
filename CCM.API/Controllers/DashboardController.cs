using CCM.API.Data; using Microsoft.AspNetCore.Authorization; using Microsoft.AspNetCore.Mvc; using Microsoft.EntityFrameworkCore;
namespace CCM.API.Controllers;
[ApiController][Route("api/dashboard")][Authorize]
public class DashboardController : ControllerBase {
    private readonly AppDbContext _db;
    public DashboardController(AppDbContext db){_db=db;}
    [HttpGet] public async Task<IActionResult> Get() {
        var all=await _db.CustomerRequests.ToListAsync();
        var l30=DateTime.UtcNow.AddDays(-30);
        return Ok(new{
            totalRequests=all.Count, pendingL1=all.Count(r=>r.Status=="Pending L1"),
            pendingL2=all.Count(r=>r.Status=="Pending L2"), approved=all.Count(r=>r.Status=="Approved & Created"),
            rejected=all.Count(r=>r.Status=="Rejected"), sapErrors=all.Count(r=>r.Status=="SAP Error"),
            last30DaysCount=all.Count(r=>r.SubmittedAt>=l30),
            byStatus=all.GroupBy(r=>r.Status).Select(g=>new{status=g.Key,count=g.Count()}),
            byType=all.GroupBy(r=>r.CustomerType).Select(g=>new{type=g.Key,count=g.Count()}),
            trend=all.Where(r=>r.SubmittedAt>=l30).GroupBy(r=>r.SubmittedAt.Date.ToString("dd MMM")).Select(g=>new{date=g.Key,count=g.Count()}).OrderBy(x=>x.date)
        });
    }
    [HttpGet("emails")] public async Task<IActionResult> GetEmails() => Ok(await _db.EmailLogs.OrderByDescending(e=>e.SentAt).Take(100).ToListAsync());
    [HttpGet("audit")] public async Task<IActionResult> GetAudit() {
        return Ok(await _db.ApprovalTimelines.Include(t=>t.ActionBy).Include(t=>t.Request).OrderByDescending(t=>t.ActionAt).Take(200)
            .Select(t=>new{t.Id,t.Role,t.Message,t.ActionAt,ActionBy=t.ActionBy==null?"System":t.ActionBy.FullName,RequestNo=t.Request==null?"":t.Request.RequestNo,Customer=t.Request==null?"":t.Request.CustomerName}).ToListAsync());
    }
}