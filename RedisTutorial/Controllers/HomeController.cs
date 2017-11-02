using RedisTutorial.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RedisTutorial.Redis;

namespace RedisTutorial.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(Human item)
        {
            if (ModelState.IsValid)
            {
                item.Password = Encryption.Crypt(item.Password);
                RedisAccess.Set<Human>(item);
                return View("Index");
            }
            else
            {
                return View(item);
            }
        }

        [HttpGet]
        public ActionResult Login() 
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(Human item)
        {
            if (!string.IsNullOrWhiteSpace(item.Email) && !string.IsNullOrWhiteSpace(item.Password))
            {
                var registeredUser = RedisAccess.Get<Human>();
                registeredUser.ForEach(x => x.Password = Encryption.Derypt(x.Password));
                if (registeredUser.Any(x => x.Email == item.Email && x.Password == item.Password))
                    return View("LoginSuccess",registeredUser.First(x => x.Email == item.Email && x.Password == item.Password));
                else
                    return View();
            }
            else
            {
                return View(item);
            }

        }

        [HttpGet]
        public ActionResult RegisterHash()
        {
            return View();
        }

        [HttpPost]
        public ActionResult RegisterHash(Human item)
        {
            if (ModelState.IsValid)
            {
                item.Password = Encryption.Crypt(item.Password);
                RedisAccess.HSet<Human>(item);
                return View("Index");
            }
            else
            {
                return View(item);
            }
        }

        [HttpGet]
        public ActionResult LoginHash()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LoginHash(Human item)
        {
            if (!string.IsNullOrWhiteSpace(item.Email) && !string.IsNullOrWhiteSpace(item.Password))
            {
                var registeredUser = RedisAccess.HGet();
                registeredUser.ForEach(x => x.Password = Encryption.Derypt(x.Password));
                if (registeredUser.Any(x => x.Email == item.Email && x.Password == item.Password))
                    return View("LoginSuccess", registeredUser.First(x => x.Email == item.Email && x.Password == item.Password));
                else
                    return View();
            }
            else
            {
                return View(item);
            }

        }

        [HttpGet]
        public ActionResult LoginSuccess(Human item)
        {
            return View(item);
        }
    }
}