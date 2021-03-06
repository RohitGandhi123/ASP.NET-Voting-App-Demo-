﻿using OnlineVotingSystem.Models;
using OnlineVotingSystem.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OnlineVotingSystem.Controllers
{
    public class VotingPanelController : Controller
    {
        OnlineVotingSystemEntities db = new OnlineVotingSystemEntities();

        // GET: VotingPanel       
        public ActionResult Index()
        {
            try
            {
                Guid urlId = new Guid(Url.RequestContext.RouteData.Values["id"].ToString());
                TempData["UrlID"] = urlId;
                var voter = db.Voters.Where(u => u.UniqueIdentification == urlId).SingleOrDefault();
                if (voter == null)
                {
                    if (Session["userEmail"] != null && Session["userType"] != null)
                    {
                        var positionWiseCandidateList = (from cp in db.CandidatePositions
                                                         join c in db.Candidates
                                                         on cp.CandidateId equals c.CandidateId
                                                         join p in db.Positions
                                                         on cp.PositionId equals p.PositionId
                                                         select new VoteCastingPanelViewModel
                                                         {
                                                             Position = p,
                                                             Candidate = c
                                                         });

                        return View(positionWiseCandidateList);
                    }
                    else
                    {
                        return View("Error");
                    }
                }
                else
                {
                    if (voter.IsCommittedVote == false)
                    {
                        if (voter.IsSelected == true)
                        {
                            var positionWiseCandidateList = (from cp in db.CandidatePositions
                                                             join c in db.Candidates
                                                             on cp.CandidateId equals c.CandidateId
                                                             join p in db.Positions
                                                             on cp.PositionId equals p.PositionId
                                                             select new VoteCastingPanelViewModel
                                                             {
                                                                 Position = p,
                                                                 Candidate = c
                                                             });

                            return View(positionWiseCandidateList);
                        }
                        else
                        {
                            return View("endtime");
                        }
                    }
                    else
                    {
                        return View("Commit");
                    }
                }
            }
            catch (Exception ex)
            {
                if (Session["userEmail"] != null && Session["userType"] != null)
                {
                    var positionWiseCandidateList = (from cp in db.CandidatePositions
                                                     join c in db.Candidates
                                                     on cp.CandidateId equals c.CandidateId
                                                     join p in db.Positions
                                                     on cp.PositionId equals p.PositionId
                                                     select new VoteCastingPanelViewModel
                                                     {
                                                         Position = p,
                                                         Candidate = c
                                                     });

                    return View(positionWiseCandidateList);
                }
                else
                {
                    return View("Error");
                }
            }
        }

        [HttpPost]
        public ActionResult VoteCast(List<VoteCastingInformation> result)
        {
            Guid urlId = (Guid)TempData["UrlID"];
            var voter = db.Voters.Where(u => u.UniqueIdentification == urlId).SingleOrDefault();
            if (voter != null)
            {
                int voterId = voter.VoterId;

                foreach (VoteCastingInformation vc in result)
                {
                    vc.VoterId = voterId;
                    db.VoteCastingInformations.Add(vc);
                }

                //db.SaveChanges();

                Voter vote = (from v in db.Voters
                              where v.VoterId == voterId
                              select v).SingleOrDefault();

                vote.IsCommittedVote = true;

                db.SaveChanges();
            }

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }
    }
}