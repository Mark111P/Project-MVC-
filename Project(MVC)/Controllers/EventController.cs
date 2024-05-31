using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project_MVC_.Models.DB;
using System.Text.RegularExpressions;

namespace Project_MVC_.Controllers
{
    public class EventController : Controller
    {
        public CrmContext db;
        
        public EventController(CrmContext context)
        {
            db = context;
        }

        [Authorize]
        public IActionResult GroupMeetings(int groupId)
        {
            ViewData["Login"] = "Auth";

            List<Meeting> meetings = db.Meetings.Where(i => i.GroupId == groupId).ToList();

            List<Meeting> upcomingMeetings = meetings.Where(i => i.Time > DateTime.Now).OrderBy(i => i.Time).ToList();
            List<Meeting> oldMeetings = meetings.Where(i => i.Time <= DateTime.Now).OrderByDescending(i => i.Time).ToList();

            string userRole = db.Members.First(i => i.GroupId == groupId && i.User.Login == User.Claims.First().Value).Role;

            return View("GroupMeetings", new { gId = groupId, upcoming = upcomingMeetings, old = oldMeetings, role = userRole });
        }

        [Authorize]
        public IActionResult DeleteMeeting(int meetingId, int groupId) 
        {
            db.Meetings.Remove(db.Meetings.First(i => i.Id == meetingId));
            db.SaveChanges();

            return GroupMeetings(groupId);
        }

        public IActionResult GroupDeals(int groupId)
        {
            ViewData["Login"] = "Auth";

            List<Deal> deals = db.Deals.Where(i => i.GroupId == groupId).ToList();

            List<Deal> upcomingDeals = deals.Where(i => i.Time > DateTime.Now).OrderBy(i => i.Time).ToList();
            List<Deal> oldDeals = deals.Where(i => i.Time <= DateTime.Now).OrderByDescending(i => i.Time).ToList();

            string userRole = db.Members.First(i => i.GroupId == groupId && i.User.Login == User.Claims.First().Value).Role;

            return View("GroupDeals", new { gId = groupId, upcoming = upcomingDeals, old = oldDeals, role = userRole });
        }

        [Authorize]
        public IActionResult DeleteDeal(int dealId, int groupId)
        {
            db.Deals.Remove(db.Deals.First(i => i.Id == dealId));
            db.SaveChanges();

            return GroupDeals(groupId);
        }
        [Authorize]
        public IActionResult CompleteDeal(int dealId, int groupId)
        {
            db.Deals.First(i => i.Id == dealId).Status = "Completed";
            db.SaveChanges();

            return GroupDeals(groupId);
        }

        [Authorize]
        public IActionResult UserEvents()
        {
            ViewData["Login"] = "Auth";

            List<int> groupIds = db.Members.Where(i => i.User.Login == User.Claims.First().Value).Select(i => i.GroupId).ToList();

            List<Meeting> meetings = db.Meetings.Where(i => groupIds.Contains(i.GroupId)).ToList();

            List<Meeting> upcomingMeetings = meetings.Where(i => i.Time > DateTime.Now).OrderBy(i => i.Time).ToList();
            List<Meeting> oldMeetings = meetings.Where(i => i.Time <= DateTime.Now).OrderByDescending(i => i.Time).ToList();

            List<Deal> deals = db.Deals.Where(i => groupIds.Contains(i.GroupId)).ToList();

            List<Deal> upcomingDeals = deals.Where(i => i.Time > DateTime.Now).OrderBy(i => i.Time).ToList();
            List<Deal> oldDeals = deals.Where(i => i.Time <= DateTime.Now).OrderByDescending(i => i.Time).ToList();

            return View(new { upcomingM = upcomingMeetings, oldM = oldMeetings, upcomingD = upcomingDeals, oldD = oldDeals });
        }
    }
}
