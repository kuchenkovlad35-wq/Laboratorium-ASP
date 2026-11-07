using System.Text;
using BookStore.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Web.Controllers;

public class BookController : Controller
{
    // ==========================================
    // Часть C
    // ==========================================

    // GET /Book/Hello -> обычный текст
    public IActionResult Hello() => Content("Witaj w BookStore!", "text/plain; charset=utf-8");

    // GET /Book/Status?code=418 -> любой HTTP-код статуса
    public IActionResult Status(int code = 200) => StatusCode(code);

    // GET /Book/Details/3 -> страница View
    public IActionResult Details(int id)
    {
        ViewBag.Id = id;
        return View();
    }

    // ==========================================
    // Часть D: 4 способа получения данных
    // ==========================================

    // 1. Query String: /Book/Echo?name=Ala&count=3
    // Данные берутся из параметров URL (?name=...&count=...)
    public IActionResult Echo([FromQuery] string name = "nieznajomy", [FromQuery] int count = 1) =>
        Content(string.Join("\n", Enumerable.Repeat($"Cześć, {name}!", Math.Clamp(count, 1, 20))));

    // 2. Route Parameter: /Book/Square/7
    // {id} берется прямо из сегмента адреса URL (из стандартного роутинга)
    public IActionResult Square(int id) => Content($"{id}^2 = {id * id}");

    // 3. Request Body (JSON): POST /Book/Import
    // [FromBody] говорит ASP.NET распарсить входящий JSON в объект BookDto
    [HttpPost]
    public IActionResult Import([FromBody] BookDto dto) =>
        Json(new { received = dto.Title, dto.Isbn, dto.Price });

    // 4. Raw Request: /Book/Inspect
    // Читаем сырой объект Request (метод, путь, все заголовки браузера)
    public IActionResult Inspect()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Method: {Request.Method}");
        sb.AppendLine($"Path: {Request.Path}{Request.QueryString}");
        foreach (var h in Request.Headers) sb.AppendLine($"{h.Key}: {h.Value}");
        return Content(sb.ToString(), "text/plain; charset=utf-8");
    }

    // ==========================================
    // Задания для самостоятельного выполнения
    // ==========================================

    // Задание 1: GET /Book/Sum?a=2&b=3
    public IActionResult Sum(int a, int b) => Content((a + b).ToString());

    // Задание 2: GET /Book/UserAgent
    public IActionResult UserAgent() => Content(Request.Headers.UserAgent.ToString());

    // Задание 3: GET /Book/Health -> JSON со статусом и текущим временем
    public IActionResult Health() => Json(new { status = "ok", time = DateTime.Now });
}