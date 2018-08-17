using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PYAC.Events
{

    public class CookInfoEvent : PubSubEvent<Cook>
    {
    }

    public class Cook
    {
        public int batch_ID { get; set; }
        public string recipe_name { get; set; }

        public Cook(int batch_ID, string recipe_name)
        {
            this.batch_ID = batch_ID;
            this.recipe_name = recipe_name;
        }
    }


}
