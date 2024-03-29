﻿using FreeCourse.Services.PhotoStock.Dtos;
using FreeCourse.Shared;
using FreeCourse.Shared.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FreeCourse.Services.PhotoStock.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhotosController : CustomBaseController
    {

        [HttpPost]
        public async Task<IActionResult> PhotoSave(IFormFile photo, CancellationToken cancellationToken)
        {
            if (photo != null && photo.Length > 0)
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/photos", photo.FileName);

                using var stream = new FileStream(path, FileMode.Create);
                await photo.CopyToAsync(stream,cancellationToken); // istek yarıda kesilirse kapanır.

                var returnPath = "photos/" + photo.FileName;

                PhotoDto photoDto = new()
                {
                    Url = returnPath
                };

                return CreateActionResultInstance(Response<PhotoDto>.Success(photoDto,StatusCodes.Status200OK));
            }

            return CreateActionResultInstance(Response<PhotoDto>.Fail("photo is empty",StatusCodes.Status400BadRequest));
        }
   
        public async Task<IActionResult> PhotoDelete(string photoUrl)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/photos", photoUrl);

            if (!System.IO.File.Exists(path))
            {
                return CreateActionResultInstance(Response<NoContent>.Fail("photo not found", StatusCodes.Status404NotFound));
            }

            await  Task.Run(() => System.IO.File.Delete(path));

            return CreateActionResultInstance(Response<NoContent>.Success(StatusCodes.Status204NoContent));
        }
    }
}
