using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.API.Models
{
    public class Like
    {
        public Guid LikerId { get; set; }
        public Guid LikeeId { get; set; }
        public User Liker { get; set; }
        public User Likee { get; set; }
    }
}
