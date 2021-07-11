using AutoMapper;
using Library.API.Entities;
using Library.API.Helpers;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.API.Controllers
{
    [Route("api/authors")]
    public class AuthersController : Controller
    {
        private ILibraryRepository _Repo;
        private IUrlHelper _urlHelper;
        private IPropertyMappingService _propertyMappingService;
        private ITypeHelperService _typeHelperService;
        const int maxAuthorPageSize = 20;
        public AuthersController(ILibraryRepository libraryRepository,
            IUrlHelper urlHelper,
            IPropertyMappingService propertyMappingService,
            ITypeHelperService typeHelperService)
        {
            _Repo = libraryRepository;
            _urlHelper = urlHelper;
            _propertyMappingService = propertyMappingService;
            _typeHelperService = typeHelperService;
        }
        [HttpGet(Name ="GetAuthors")]
        public IActionResult GetAuthers(AuthorsResourceParameters authorsResourceParameters)
        {    
            if(!_propertyMappingService.ValidMappingExistsFor<AuthorDto,Author>(
                authorsResourceParameters.OrderBy))
            {
                return BadRequest();
            }
            if (!_typeHelperService.TypeHasProperties<AuthorDto>(authorsResourceParameters.Fields))
            {
                return BadRequest();
            }
            var authorsFromRepo = _Repo.GetAuthors(authorsResourceParameters);
            var previousPageLink = authorsFromRepo.HasPrevios ?
                CreateAuthorResourceUri(authorsResourceParameters,
                ResourseUriType.PreviousPage) : null;

            var nextPageLink = authorsFromRepo.HasNext ?
                CreateAuthorResourceUri(authorsResourceParameters,
                ResourseUriType.NextPage) : null;

            var paginationMetadata = new
            {
                totalCount = authorsFromRepo.TotalCount,
                pageSize = authorsFromRepo.PageSize,
                currentPage = authorsFromRepo.CurrentPage,
                totalPages = authorsFromRepo.TotalPages,
                previousPageLink = previousPageLink,
                nextPageLink = nextPageLink
            };

            Response.Headers.Add("X-Pagination",
                Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetadata));

            var authors = Mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo);
          
            return Ok(authors.ShapeData(authorsResourceParameters.Fields));
        }
        private  string CreateAuthorResourceUri(
            AuthorsResourceParameters authorsResourceParameters,
            ResourseUriType type)
        {
            switch (type)
            {
                case ResourseUriType.PreviousPage:
                    return _urlHelper.Link("GetAuthors",
                        new
                        {
                            fields = authorsResourceParameters.Fields,
                            orderBy = authorsResourceParameters.OrderBy,
                            searchQuery = authorsResourceParameters.SearchQuery,
                            genre = authorsResourceParameters.Genre,
                            pageNumber = authorsResourceParameters.PageNumber - 1,
                            pageSize = authorsResourceParameters.PageSize
                        });
                case ResourseUriType.NextPage:
                    return _urlHelper.Link("GetAuthors",
                        new
                        {
                            fields = authorsResourceParameters.Fields,
                            orderBy = authorsResourceParameters.OrderBy,
                            searchQuery = authorsResourceParameters.SearchQuery,
                            genre = authorsResourceParameters.Genre,
                            pageNumber = authorsResourceParameters.PageNumber + 1,
                            pageSiza = authorsResourceParameters.PageSize
                        });
                default:
                    return _urlHelper.Link("GetAuthors",
                        new
                        {
                            fields = authorsResourceParameters.Fields,
                            orderBy = authorsResourceParameters.OrderBy,
                            searchQuery = authorsResourceParameters.SearchQuery,
                            genre = authorsResourceParameters.Genre,
                            pageNumber = authorsResourceParameters.PageNumber,
                            pageSiza = authorsResourceParameters.PageSize
                        });
            }
        }
        [HttpGet("{id}", Name = "GetAuthor")]
        public IActionResult GetAuthor(Guid id, [FromQuery] string fields)
        {
            if (!_typeHelperService.TypeHasProperties<AuthorDto>
              (fields))
            {
                return BadRequest();
            }

            var authorFromRepo = _Repo.GetAuthor(id);

            if (authorFromRepo == null)
            {
                return NotFound();
            }

            var author = Mapper.Map<AuthorDto>(authorFromRepo);
            return Ok(author.ShapeData(fields));
        }
        [HttpPost(Name = "create_author")]
        public IActionResult CreateAuthor([FromBody] AuthorForCreationDto author)
        {
            if (author == null)
                return BadRequest();
            var authorEntity = Mapper.Map<Author>(author);         
            _Repo.AddAuthor(authorEntity);
           if(!_Repo.Save())
            {
                throw new Exception("Creating of author failed");
               // return StatusCode(500, "A problem has happened");
            }
            var authorToReturn = Mapper.Map<AuthorDto>(authorEntity);
            return CreatedAtRoute("GetAuthor", new { id = authorToReturn.Id }, authorToReturn);
        }
        [HttpPost("{id}")]
        public IActionResult BlockAuthorCreation(Guid id)
        {
            if (_Repo.AuthorExists(id))
            {
                return new StatusCodeResult(StatusCodes.Status409Conflict);
            }
           

            return NotFound();
        }
        [HttpDelete("{id}")]
        public IActionResult DeleteAuthor(Guid id)
        {
            var  authorFromRepo = _Repo.GetAuthor(id);
            if (authorFromRepo==null)
            {
                return NotFound();
            }

            _Repo.DeleteAuthor(authorFromRepo);
            if (!_Repo.Save())
                throw new Exception($"Deleting author {id} failed");
            return NoContent();
        }
    }
}
