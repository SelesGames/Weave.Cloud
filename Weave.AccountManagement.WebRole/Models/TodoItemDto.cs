using System.ComponentModel.DataAnnotations;

namespace Weave.AccountManagement.WebRole.Models
{
    public class TodoItemDto
    {
        /// <summary>
        /// Data transfer object for <see cref="TodoItem"/>
        /// </summary>
        public TodoItemDto() { }

        public TodoItemDto(TodoItem item)
        {
            TodoItemId = item.TodoItemId;
            Title = item.Title;
            IsDone = item.IsDone;
            TodoListId = item.TodoListId;
        }

        [Key]
        public int TodoItemId { get; set; }

        [Required]
        public string Title { get; set; }

        public bool IsDone { get; set; }

        public int TodoListId { get; set; }

        public TodoItem ToEntity()
        {
            return new TodoItem
            {
                TodoItemId = TodoItemId,
                Title = Title,
                IsDone = IsDone,
                TodoListId = TodoListId
            };
        }
    }
}
