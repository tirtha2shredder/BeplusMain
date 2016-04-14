﻿using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Microsoft.WindowsAzure.Mobile.Service;
using beplusService.DataObjects;
using beplusService.Models;
using System.Collections.Generic;

namespace beplusService.Controllers
{
    public class BepOrganizationController : TableController<BepOrganization>
    {

        beplusContext context = new beplusContext();
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            DomainManager = new EntityDomainManager<BepOrganization>(context, Request, Services);
        }

        // GET tables/BepOrganization
        public IQueryable<BepOrganization> GetAllBepOrganization()
        {
            return Query(); 
        }

        // GET tables/BepOrganization/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<BepOrganization> GetBepOrganization(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/BepOrganization/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<BepOrganization> PatchBepOrganization(string id, Delta<BepOrganization> patch)
        {
             return UpdateAsync(id, patch);
        }

        // POST tables/BepOrganization
        public async Task<IHttpActionResult> PostBepOrganization(BepOrganization item)
        {
            BepOrganization current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/BepOrganization/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteBepOrganization(string id)
        {
             return DeleteAsync(id);
        }
        [Route("api/registerOrganization", Name = "RegisterNewOrganization")]
        public async Task<IHttpActionResult> RegisterNewOrganization(BepOrganization organization)
        {
            // Does the Organization data exist?
            var count = context.BepOrganizations.Where(x => x.Phone == organization.Phone).Count();
            if (count > 0)
            {
                return BadRequest("Phone number already registered!");
            }
            count = context.BepOrganizations.Where(x => x.Email == organization.Email).Count();
            if (count > 0)
            {
                return BadRequest("Email number already registered!");
            }
            //Provision to send out activation email. Until implemented, the activation status will be true for all registering parties
            organization.Activated = true;

            BepOrganization current = await InsertAsync(organization);
            return Ok("Organization registered successfully!");
        }
        [Route("api/activateOrganization", Name = "ActivateOrganization")]
        [HttpGet]
        public async Task<IHttpActionResult> ActivateOrganization(string Id)
        {
            var count = context.BepOrganizations.Where(x => (x.Id == Id && x.Activated == true)).Count();
            if (count == 1)
            {
                return BadRequest("You are already registered. Please login using our application.");
            }
            count = context.BepOrganizations.Where(x => (x.Id == Id && x.Activated == false)).Count();
            if (count == 0)
            {
                return BadRequest("An error has occured. Please try registering again.");
            }
            else
            {
                using (var db = new beplusContext())
                {
                    BepOrganization donor = db.BepOrganizations.SingleOrDefault(x => x.Id == Id);
                    donor.Activated = true;
                    db.SaveChanges();                    
                }
                return Ok("Your account has been activated! Please login using our app.");
            }
        }
        [Route("api/loginOrganization", Name = "LoginOrganization")]
        public IHttpActionResult LoginOrganization(LoginData logindata)
        {
            // Does the Organization data exist?
            List<BepOrganization> orglist;
            if (logindata.Phone != null)
                orglist = context.BepOrganizations.Where(x => (x.Phone == logindata.Phone && x.Password == logindata.Password)).ToList();
            else orglist = context.BepOrganizations.Where(x => (x.Email == logindata.Email && x.Password == logindata.Password)).ToList();
            int count = orglist.Count;
            if (count == 1)
            {
                var current = orglist[0];
                var result = Lookup(current.Id).Queryable.Select(x => new BepOrganizationDTO()
                {
                    Id = x.Id,
                    About = x.About,
                    Locality = x.Locality,
                    Name = x.Name,
                    Phone = x.Phone,
                    Email = x.Email,
                    Chairperson = x.Chairperson,
                    LocationLat = x.LocationLat,
                    LocationLong = x.LocationLong,
                    Activated = x.Activated,
                    Imgurl = x.Imgurl
                });
                return Ok(SingleResult<BepOrganizationDTO>.Create(result));
            }
            else return BadRequest("Invalid login credentials!");
        }
    }
}