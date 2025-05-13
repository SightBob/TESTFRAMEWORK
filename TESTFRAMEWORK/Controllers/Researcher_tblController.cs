using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TESTFRAMEWORK.Models;

namespace TESTFRAMEWORK.Controllers
{
    public class Researcher_tblController : Controller
    {
        private Research_DBEntities1 db = new Research_DBEntities1();

        // GET: Researcher_tbl
        public ActionResult Index()
        {
            var researcher_tbl = db.Researcher_tbl.Include(r => r.department).Include(r => r.division).Include(r => r.TypeResearch1).Include(r => r.work_groups);
            return View(researcher_tbl.ToList());
        }

        // GET: Researcher_tbl/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Researcher_tbl researcher_tbl = db.Researcher_tbl.Find(id);
            if (researcher_tbl == null)
            {
                return HttpNotFound();
            }
            return View(researcher_tbl);
        }

        // GET: Researcher_tbl/Create
        public ActionResult Create()
        {
            ViewBag.department_id = new SelectList(db.departments, "id", "name");
            ViewBag.division_id = new SelectList(db.divisions, "id", "name");
            ViewBag.TypeResearch = new SelectList(db.TypeResearches, "id", "type_name");
            ViewBag.work_group_id = new SelectList(db.work_groups, "id", "name");
            return View();
        }

        // POST: Researcher_tbl/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ResearcherNumber,Name,work_group_id,department_id,division_id,TypeResearch,title,OtherInfo")] Researcher_tbl researcher_tbl)
        {
            if (ModelState.IsValid)
            {
                db.Researcher_tbl.Add(researcher_tbl);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.department_id = new SelectList(db.departments, "id", "name", researcher_tbl.department_id);
            ViewBag.division_id = new SelectList(db.divisions, "id", "name", researcher_tbl.division_id);
            ViewBag.TypeResearch = new SelectList(db.TypeResearches, "id", "type_name", researcher_tbl.TypeResearch);
            ViewBag.work_group_id = new SelectList(db.work_groups, "id", "name", researcher_tbl.work_group_id);
            return View(researcher_tbl);
        }

        // GET: Researcher_tbl/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Researcher_tbl researcher_tbl = db.Researcher_tbl.Find(id);
            if (researcher_tbl == null)
            {
                return HttpNotFound();
            }
            ViewBag.department_id = new SelectList(db.departments, "id", "name", researcher_tbl.department_id);
            ViewBag.division_id = new SelectList(db.divisions, "id", "name", researcher_tbl.division_id);
            ViewBag.TypeResearch = new SelectList(db.TypeResearches, "id", "type_name", researcher_tbl.TypeResearch);
            ViewBag.work_group_id = new SelectList(db.work_groups, "id", "name", researcher_tbl.work_group_id);
            return View(researcher_tbl);
        }

        // POST: Researcher_tbl/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ResearcherNumber,Name,work_group_id,department_id,division_id,TypeResearch,title,OtherInfo")] Researcher_tbl researcher_tbl)
        {
            if (ModelState.IsValid)
            {
                db.Entry(researcher_tbl).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.department_id = new SelectList(db.departments, "id", "name", researcher_tbl.department_id);
            ViewBag.division_id = new SelectList(db.divisions, "id", "name", researcher_tbl.division_id);
            ViewBag.TypeResearch = new SelectList(db.TypeResearches, "id", "type_name", researcher_tbl.TypeResearch);
            ViewBag.work_group_id = new SelectList(db.work_groups, "id", "name", researcher_tbl.work_group_id);
            return View(researcher_tbl);
        }

        // GET: Researcher_tbl/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Researcher_tbl researcher_tbl = db.Researcher_tbl.Find(id);
            if (researcher_tbl == null)
            {
                return HttpNotFound();
            }
            return View(researcher_tbl);
        }

        // POST: Researcher_tbl/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            Researcher_tbl researcher_tbl = db.Researcher_tbl.Find(id);
            db.Researcher_tbl.Remove(researcher_tbl);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
