using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModel
{
    public abstract class BaseModel
    {
        ReactiveProperty<bool?> IsSelect { get; set; }
    }
}
