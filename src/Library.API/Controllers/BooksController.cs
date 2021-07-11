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
    [Route("api/authors/{authorId}/books")]
    public class BooksController : Controller
    {
        private ILibraryRepository _Repo;

        public BooksController(ILibraryRepository libaryRepository)
        {
            _Repo = libaryRepository;
        }

        [HttpGet()]
        public IActionResult GetBooksForAuthor(Guid authorId)
        {
            if (!_Repo.AuthorExists(authorId))
            {
                return NotFound();
            }
            var booksForAuthorFromRepo = _Repo.GetBooksForAuthor(authorId);
            var BooksForAuthor = Mapper.Map<IEnumerable<BookDto>>(booksForAuthorFromRepo);
            return Ok(BooksForAuthor);
        }
        [HttpGet("{bookId}",Name ="GetBookForAuthor")]
        public IActionResult GetBookForAuthor(Guid authorId,Guid bookId)
        {
            if (!_Repo.AuthorExists(authorId))
            {
                return NotFound();
            }
            var bookForAuthorFromRepo = _Repo.GetBookForAuthor(authorId,bookId);
            if (bookForAuthorFromRepo == null)
            {
                return NotFound();
            }
            var BookForAuthor = Mapper.Map<BookDto>(bookForAuthorFromRepo);
            return Ok(BookForAuthor);
        }
        [HttpPost()]
        public IActionResult CreateBookForAuthor(Guid authorId,
            [FromBody] BookForCreationDto book)
        {
            if (book == null)
            {
                return BadRequest();
            }
            if(book.Description == book.Title)
            {
                ModelState.AddModelError(nameof(BookForCreationDto), 
                    "the provided description should be different from the title.");
            }
            if(!ModelState.IsValid)
            {
                return new UnprocessableEntityObjectResult(ModelState);
            }

            if (!_Repo.AuthorExists(authorId))
                return NotFound();
            var bookEntity = Mapper.Map<Book>(book);
            _Repo.AddBookForAuthor(authorId,bookEntity);
            if (!_Repo.Save())
            {
                throw new Exception("Creating of book for author failed");
                // return StatusCode(500, "A problem has happened");
            }
            var bookToReturn = Mapper.Map<BookDto>(bookEntity);
            return CreatedAtRoute("GetBookForAuthor", new { authorId = authorId,
                bookId = bookToReturn.Id }, bookToReturn);
        }
        [HttpDelete("{bookId}")]
        public IActionResult DeleteBookFrorAuthor(Guid authorId , Guid bookId)
        {
            if (!_Repo.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookForAuthorFromRepo = _Repo.GetBookForAuthor(authorId, bookId);
            if (bookForAuthorFromRepo == null)
                return NotFound();
            _Repo.DeleteBook(bookForAuthorFromRepo);
            if (!_Repo.Save())
                throw new Exception($"Deleting book{bookId} for author {authorId} failed");
            return NoContent();
        }
        [HttpPut("{bookId}")]
        public IActionResult UpdateBookForAuthor(Guid authorId,Guid bookId,
            [FromBody]BookForUpdateDto book)
        {
            if (book == null)
                return BadRequest();
            if (book.Description == book.Title)
            {
                ModelState.AddModelError(nameof(BookForUpdateDto),
                    "the provided description should be different from the title.");
            }
            if (!ModelState.IsValid)
            {
                return new UnprocessableEntityObjectResult(ModelState);
            }
            if (!_Repo.AuthorExists(authorId))
            {
                return NotFound();
            }
            var bookForAuthorFromRepo = _Repo.GetBookForAuthor(authorId, bookId);
            if (bookForAuthorFromRepo == null)
            {

                var bookEntity = Mapper.Map<Book>(book);
                bookEntity.Id = bookId;
                _Repo.AddBookForAuthor(authorId, bookEntity);
                if (!_Repo.Save())
                    throw new Exception($"upsertting book{bookId} for author {authorId} failed");
                var bookToReturn = Mapper.Map<BookDto>(bookEntity);
                return CreatedAtRoute("GetBookForAuthor", new
                {
                    authorId = authorId,
                    bookId = bookToReturn.Id
                }, bookToReturn);
            }



            Mapper.Map(book, bookForAuthorFromRepo);
           
            _Repo.UpdateBookForAuthor(bookForAuthorFromRepo);

            if (!_Repo.Save())
                throw new Exception($"update book{bookId} for author {authorId} failed");

            return NoContent();
        }
    }
}
