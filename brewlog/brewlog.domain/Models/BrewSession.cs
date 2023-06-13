using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace brewlog.domain.Models
{
    public record BrewSession(DateTimeOffset CreatedAt, string SessionName);
}
