using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TodoApi.Models;

namespace TodoApi.Controllers
{
    [Route("api/[controller]")] 
    [ApiController]
    public class TodoController : Controller
    {
        private readonly TodoContext _context;
        private readonly IConfiguration Configuration;

        public enum ResponseMode
        {
            Constant = 0,
            Variable = 1
        }

        private readonly ResponseMode responseMode = ResponseMode.Constant;
        private int responseDelay = 300;
        private bool createLongDelay = false;
        private int longDelayOnWhichMillisecond = 300; 

        private Stopwatch stopwatch = new Stopwatch();
        private long actualDelay = 0;

        public TodoController(TodoContext context, IConfiguration configuration)
        {
            _context = context;
            Configuration = configuration;

            if (!_context.TodoItems.Any())
            {
                _context.TodoItems.Add(new TodoItem { Name = "Item1" });
                _context.SaveChanges();
            }

            try
            {
                responseMode = (ResponseMode)Enum.Parse(typeof(ResponseMode), Configuration["ResponseMode"]);
                responseDelay = int.Parse(Configuration["ResponseDelay"]);
                string createLongDelayString = Configuration["Create60SecondDelayOn"];
                if ( !String.IsNullOrEmpty(createLongDelayString) )
                {
                    longDelayOnWhichMillisecond = int.Parse(createLongDelayString);
                    createLongDelay = true;
                }

            }
            catch (Exception)
            {
            }
            CauseResponseDelay();
        }

        // GET: api/Todo
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TodoItem>>> GetTodoItem()
        {
            return await _context.TodoItems.ToListAsync();
        }

        // GET: api/Todo/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TodoItem>> GetTodoItem(long id)
        {
            var todoItem = await _context.TodoItems.FindAsync(id);

            if (todoItem == null)
            {
                return NotFound();
            }

            return todoItem;
        }

        // PUT: api/Todo/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTodoItem(long id, TodoItem todoItem)
        {
            if (id != todoItem.Id)
            {
                return BadRequest();
            }

            _context.Entry(todoItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TodoItemExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Todo
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<TodoItem>> PostTodoItem(TodoItem todoItem)
        {
            _context.TodoItems.Add(todoItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTodoItem", new { id = todoItem.Id }, todoItem);
        }

        // DELETE: api/Todo/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<TodoItem>> DeleteTodoItem(long id)
        {
            var todoItem = await _context.TodoItems.FindAsync(id);
            if (todoItem == null)
            {
                return NotFound();
            }

            _context.TodoItems.Remove(todoItem);
            await _context.SaveChangesAsync();

            return todoItem;
        }

        private bool TodoItemExists(long id)
        {
            return _context.TodoItems.Any(e => e.Id == id);
        }

        private void CauseResponseDelay()
        {
            stopwatch.Start();
            if (createLongDelay && DateTime.Now.Millisecond == longDelayOnWhichMillisecond)
            {
                responseDelay = 60000;      // 60 seconds of delay
                while (stopwatch.ElapsedMilliseconds < responseDelay)
                {
                    int dummyInteger = 1;
                    dummyInteger += 1;
                }
            }
            else
            {
                if (responseMode == ResponseMode.Constant)
                {
                    while (stopwatch.ElapsedMilliseconds < responseDelay)
                    {
                        int dummyInteger = 1;
                        dummyInteger += 1;
                    }
                }
                else
                {
                    Random random = new Random();
                    responseDelay = random.Next(100, (int)responseDelay);
                    while (stopwatch.ElapsedMilliseconds < responseDelay)
                    {
                        // Do nothing
                    }
                }
            }
            stopwatch.Stop();
            actualDelay = stopwatch.ElapsedMilliseconds;
        }
    }
}
