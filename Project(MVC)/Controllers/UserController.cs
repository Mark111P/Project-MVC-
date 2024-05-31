using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Project_MVC_.Models;
using Project_MVC_.Models.DB;
using System.Globalization;

namespace Project_MVC_.Controllers
{
    public class UserController : Controller
    {
        public CrmContext db;

        public UserController(CrmContext context)
        {
            db = context;
        }

        [Authorize]
        public IActionResult Profile()
        {
            ViewData["Login"] = "Auth";

            User u = db.Users.First(i => i.Login == User.Claims.First().Value);

            return View("Profile", u);
        }

        [Authorize]
        public IActionResult ChangeProfile(User u)
        {
            db.Users.First(i => i.Login == u.Login).Name = u.Name;
            db.Users.First(i => i.Login == u.Login).Email = u.Email;
            db.Users.First(i => i.Login == u.Login).PhoneNumber = u.PhoneNumber;
            db.Users.First(i => i.Login == u.Login).Adress = u.Adress;
            db.SaveChanges();

            return RedirectToAction("Profile");
        }

        [Authorize]
        public IActionResult ChangePass(string pass, string newpass, string login)
        {
            if (db.Users.First(i => i.Login == login).Password == pass)
                db.Users.First(i => i.Login == login).Password = newpass;

            db.SaveChanges();

            return RedirectToAction("Profile");
        }

        [Authorize]
        public IActionResult Groups()
        {
            ViewData["Login"] = "Auth";

            User u = db.Users.First(i => i.Login == User.Claims.First().Value);

            List<Group> g = db.Groups.Where(i => i.Members.Select(j => j.UserId).Contains(u.Id)).ToList();
            List<Member> m = db.Members.Where(i => i.UserId == u.Id).ToList();
            List<Invite> inv = db.Invites.Where(i => i.UserId == u.Id).ToList();
            List<Group> allg = db.Groups.ToList();

            return View(new { groups = g, members = m, invites = inv, allgroups = allg});
        }

        [Authorize]
        public IActionResult AddGroup(string name)
        {
            User u = db.Users.First(i => i.Login == User.Claims.First().Value);

            var g = db.Groups.Add(new Group()
            {
                Name = name
            });
            db.SaveChanges();

            db.Members.Add(new Member()
            {
                UserId = u.Id,
                GroupId = g.Entity.Id,
                Role = "Admin",
                CanDo = string.Join('|', db.Rights.Select(i => i.Name))
            });
            db.SaveChanges();

            return RedirectToAction("Groups");
        }

        [Authorize]
        public IActionResult Group(int groupId) 
        {
            ViewData["Login"] = "Auth";

            Group g = db.Groups.First(i => i.Id == groupId);
            List<Member> m = db.Members.Where(i => i.GroupId == g.Id).ToList();
            List<User> u = db.Users.Where(i => m.Select(j => j.UserId).Contains(i.Id)).ToList();
            string l = User.Claims.First().Value;
            List<string> cd = db.Members.First(i => i.User.Login == l && i.GroupId == groupId).CanDo.Split('|').ToList();
            List<string> r = db.Rights.Select(i => i.Name).ToList();
            string userRole = db.Members.First(i => i.User.Login == User.Claims.First().Value && i.GroupId == groupId).Role;

            return View("Group", new { group = g, members = m, users = u, login = l, canDo = cd, rights = r, role = userRole });
        }

        [Authorize]
        public IActionResult InviteInGroup(string userLogin, int groupId) 
        {
            
            if (db.Users.Count(i => i.Login == userLogin) > 0 && db.Groups.Count(i => i.Id == groupId) > 0)
            {
                int userId = db.Users.First(i => i.Login == userLogin).Id;

                if (db.Invites.Count(i => i.UserId == userId && i.GroupId == groupId) == 0 &&
                    db.Members.Count(i => i.UserId == userId && i.GroupId == groupId) == 0)
                {
                    db.Invites.Add(new Invite()
                    {
                        UserId = userId,
                        GroupId = groupId
                    });
                    db.SaveChanges();
                }
            }

            return Group(groupId);
        }

        [Authorize]
        public IActionResult AcceptInvite(int inviteId)
        {
            Invite inv = db.Invites.First(i => i.Id == inviteId);
            db.Members.Add(new Member()
            {
                UserId = inv.UserId,
                GroupId = inv.GroupId,
                Role = "Member",
                CanDo = ""
            });
            db.Invites.Remove(inv);
            db.SaveChanges();

            return RedirectToAction("Groups");
        }

        [Authorize]
        public IActionResult DeclineInvite(int inviteId)
        {
            db.Invites.Remove(db.Invites.First(i => i.Id == inviteId));
            db.SaveChanges();

            return RedirectToAction("Groups");
        }

        [Authorize]
        public IActionResult ChangeCanDo(int groupId, string login, string role, string rights = "")
        {
            db.Members.First(i => i.GroupId == groupId && i.User.Login == login).Role = role;
            db.Members.First(i => i.GroupId == groupId && i.User.Login == login).CanDo = rights;
            db.SaveChanges();

            return Group(groupId);
        }

        [Authorize]
        public List<int> GetTimes(string userLogin, int day, int month, int year)
        {
            List<int> t = new List<int>();

            User u = db.Users.First(i => i.Login == userLogin);
            foreach (var group in db.Groups.Where(i => i.Members.Count(j => j.UserId == u.Id) > 0).ToList())
            {
                foreach (var meet in db.Meetings.Where(i => i.GroupId == group.Id))
                {
                    if (meet.Time.Year == year && meet.Time.Month == month && meet.Time.Day == day)
                    {
                        t.Add(meet.Time.Hour);
                    }
                }
                foreach (var deal in db.Deals.Where(i => i.GroupId == group.Id))
                {
                    if (deal.Time.Year == year && deal.Time.Month == month && deal.Time.Day == day)
                    {
                        t.Add(deal.Time.Hour);
                    }
                }
            }
            return t.Distinct().ToList();
        }

        [Authorize]
        public List<LoginTime> GetAllTimes(List<string> logins, int day, int month, int year)
        {
            List<LoginTime> t = new List<LoginTime>();

            foreach (var log in logins)
            {
                t.Add(new LoginTime()
                {
                    NameLogin = db.Users.First(i => i.Login == log).Name + "(" + log + ")",
                    Times = GetTimes(log, day, month, year)
                });
            }

            return t;
        }

        [Authorize]
        public IActionResult AddEvent(int groupId, int day = -1, int month = -1, int year = -1)
        {
            List<string> r = db.Rights.Select(i => i.Name).ToList();
            foreach (Member x in db.Members)
            {
                if (x.Role == "Admin")
                {
                    x.CanDo = string.Join('|', r);
                }
            }
            db.SaveChanges();


            ViewData["Login"] = "Auth";

            if (day == -1) day = DateTime.Now.Day;
            if (month == -1) month = DateTime.Now.Month;
            if (year == -1) year = DateTime.Now.Year;

            List<Member> members = db.Members.Where(i => i.GroupId == groupId).ToList();
            List<string> logins = members.Select(i => db.Users.First(j => j.Id == i.UserId).Login).ToList();

            return View(new { times = GetAllTimes(logins, day, month, year), date = new DateTime(year, month, day), gId = groupId });
        }

        [Authorize]
        public IActionResult AddMeeting (int groupId, string name, string description, string place, string time)
        {
            db.Meetings.Add(new Meeting()
            {
                Name = name,
                Description = description,
                Place = place,
                Time = DateTime.ParseExact(time, "HH:mm dd.MM.yyyy", new CultureInfo("uk-UA")),
                GroupId = groupId
            });
            db.SaveChanges();

            return Group(groupId);
        }

        [Authorize]
        public IActionResult AddDeal(int groupId, string name, string description, int price, string time)
        {
            db.Deals.Add(new Deal()
            {
                Name = name,
                Description = description,
                Price = price,
                Time = DateTime.ParseExact(time, "HH:mm dd.MM.yyyy", new CultureInfo("uk-UA")),
                GroupId = groupId,
                Status = "Planned"
            });
            db.SaveChanges();

            return Group(groupId);
        }

        [Authorize]
        public IActionResult ChangeGroupName(int groupId, string newGroupName) 
        { 
            db.Groups.First(i => i.Id == groupId).Name = newGroupName;
            db.SaveChanges();

            return Group(groupId);
        }

        [Authorize]
        public IActionResult KickUser(int kickMemberId)
        {
            int groupId = db.Members.First(i => i.Id == kickMemberId).GroupId;

            db.Members.Remove(db.Members.First(i => i.Id == kickMemberId));
            db.SaveChanges();

            return Group(groupId);
        }

        [Authorize]
        public IActionResult DeleteGroup(int groupId)
        {
            db.Groups.Remove(db.Groups.First(i => i.Id == groupId));
            db.SaveChanges();

            return RedirectToAction("Groups");
        }

        [Authorize]
        public IActionResult LeaveGroup(int groupId)
        {
            db.Members.Remove(db.Members.First(i => i.GroupId == groupId && i.User.Login == User.Claims.First().Value));
            db.SaveChanges();

            return RedirectToAction("Groups");
        }
    }
}
