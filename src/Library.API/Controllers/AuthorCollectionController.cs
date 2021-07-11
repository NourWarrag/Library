using AutoMapper;
using Library.API.Entities;
using Library.API.Helpers;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.API.Controllers
{
    [Route("api/authorCollections")]
    public class AuthorCollectionController: Controller
    {
        private ILibraryRepository _Repo;

        public AuthorCollectionController(ILibraryRepository libraryRepository)
        {
            _Repo = libraryRepository;
        }
        [HttpPost]
        public IActionResult CreateAuthorCollection(
            [FromBody] IEnumerable<AuthorForCreationDto> authorCollection)
        {
            if (authorCollection == null)
                return BadRequest();
            var authorEntities = Mapper.Map<IEnumerable<Author>>(authorCollection);
            foreach(var author in authorEntities)
            {
                _Repo.AddAuthor(author);
            }
            if (!_Repo.Save())
            {
                throw new Exception("Creating of authors failed");
                // return StatusCode(500, "A problem has happened");
            }
            var authorsToReturn = Mapper.Map<IEnumerable<AuthorDto>>(authorEntities);
            var idsAsString = string.Join(",",
                authorEntities.Select(a => a.Id));
            return CreatedAtRoute("GetAuthorCollection", new { ids = idsAsString }, authorsToReturn);
        }
        [HttpGet("({ids})", Name = "GetAuthorCollection")]
        public IActionResult GetAuthorCollection(
            [ModelBinder(BinderType = typeof(ArrayModelBinder))]IEnumerable<Guid> ids)
        {
            if (ids == null)
                return BadRequest();
            var authorEntities = _Repo.GetAuthors(ids);
            if (ids.Count() != authorEntities.Count())
                return NotFound();
            var authorsToReturn = Mapper.Map<IEnumerable<AuthorDto>>(authorEntities);

            return Ok(authorsToReturn);
        }
    }
}
