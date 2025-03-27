using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Text.Json;

namespace TodoApi.Client
{
    public class TodoItemDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public bool IsComplete { get; set; }
    }

    class Program
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly string _apiUrl = "http://localhost:5077/api/Todo"; // Замените на ваш URL

        static async Task Main()
        {
            Console.WriteLine("=== Todo API Client ===");

            while (true)
            {
                Console.WriteLine("\nВыберите действие:");
                Console.WriteLine("1. Показать все задачи");
                Console.WriteLine("2. Добавить задачу");
                Console.WriteLine("3. Обновить задачу");
                Console.WriteLine("4. Удалить задачу");
                Console.WriteLine("5. Выход");

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        await GetAllTodos();
                        break;
                    case "2":
                        await CreateTodo();
                        break;
                    case "3":
                        await UpdateTodo();
                        break;
                    case "4":
                        await DeleteTodo();
                        break;
                    case "5":
                        return;
                    default:
                        Console.WriteLine("Неверный выбор!");
                        break;
                }
            }
        }

        static async Task GetAllTodos()
        {
            try
            {
                var response = await _httpClient.GetAsync(_apiUrl);
                response.EnsureSuccessStatusCode();

                var todos = await response.Content.ReadFromJsonAsync<TodoItemDTO[]>();
                var sortedTodos = todos.OrderBy(t => t.Id).ToList();
                Console.WriteLine("\nСписок задач:");
                foreach (var todo in sortedTodos)
                {
                    Console.WriteLine($"- ID: {todo.Id}, Название: {todo.Name}, Готово: {todo.IsComplete}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }

        static async Task CreateTodo()
        {
            Console.Write("Введите название задачи: ");
            var name = Console.ReadLine();

            Console.Write("Задача завершена? (y/n): ");
            var isComplete = Console.ReadLine().ToLower() == "y";

            var newTodo = new TodoItemDTO { Name = name, IsComplete = isComplete };

            try
            {
                var response = await _httpClient.PostAsJsonAsync(_apiUrl, newTodo);
                response.EnsureSuccessStatusCode();

                var createdTodo = await response.Content.ReadFromJsonAsync<TodoItemDTO>();
                Console.WriteLine($"\nЗадача создана: ID {createdTodo.Id}, Название: {createdTodo.Name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }

        static async Task UpdateTodo()
        {
            Console.Write("Введите ID задачи для обновления: ");
            if (!long.TryParse(Console.ReadLine(), out var id))
            {
                Console.WriteLine("Неверный ID!");
                return;
            }

            // Проверяем, существует ли задача
            var existingTodo = await GetTodoById(id);
            if (existingTodo == null)
            {
                Console.WriteLine($"Задача с ID {id} не найдена!");
                return; // Прерываем выполнение
            }

            Console.Write("Новое название задачи: ");
            var name = Console.ReadLine();

            Console.Write("Задача завершена? (y/n): ");
            var isComplete = Console.ReadLine().ToLower() == "y";

            var updatedTodo = new TodoItemDTO { Id = id, Name = name, IsComplete = isComplete };

            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{_apiUrl}/{id}", updatedTodo);
                response.EnsureSuccessStatusCode();
                Console.WriteLine("\nЗадача обновлена!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }
        
        static async Task<TodoItemDTO?> GetTodoById(long id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiUrl}/{id}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<TodoItemDTO>();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        static async Task DeleteTodo()
        {
            Console.Write("Введите ID задачи для удаления: ");
            if (!long.TryParse(Console.ReadLine(), out var id))
            {
                Console.WriteLine("Неверный ID!");
                return;
            }

            try
            {
                var response = await _httpClient.DeleteAsync($"{_apiUrl}/{id}");
                response.EnsureSuccessStatusCode();

                Console.WriteLine("\nЗадача удалена!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }
    }
}