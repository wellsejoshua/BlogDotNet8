using BlogDotNet8.Models;
using System.Collections.Generic;

namespace BlogDotNet8.ViewModels
{
    public class PostDetailViewModel
    {
        public Post Post { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
    }
}
