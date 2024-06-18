﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Excel.Models;
using RabbitMQ.Excel.Services;

namespace RabbitMQ.Excel.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppDbContext _context;
        private readonly RabbitMQPublisher _rabbitMQPublisher;

        public ProductController(UserManager<IdentityUser> userManager, AppDbContext context, RabbitMQPublisher rabbitMQPublisher)
        {
            _userManager = userManager;
            _context = context;
            _rabbitMQPublisher = rabbitMQPublisher;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> CreatedProductExcel()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            var fileName = Guid.NewGuid().ToString();
            UserFile userFile = new()
            {
                UserId = user.Id,
                FileName = fileName,
                FileStatus = FileStatus.Creating
            };
            await _context.UserFiles.AddAsync(userFile);
            await _context.SaveChangesAsync();

            _rabbitMQPublisher.Publish(new()
            {
                FileId = userFile.Id
            });

            return RedirectToAction(nameof(Files));
        }

        public async Task<IActionResult> Files()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            return View(_context.UserFiles.Where(x => x.UserId == user.Id));
        }
    }
}
