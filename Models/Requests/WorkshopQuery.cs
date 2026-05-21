using EXE201_Backend.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace EXE201_Backend.Models.Requests
{
    public class WorkshopQuery
    {
        public string? Query { get; internal set; }
    }
}
