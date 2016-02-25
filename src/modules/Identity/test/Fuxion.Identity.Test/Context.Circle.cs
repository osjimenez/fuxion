using Fuxion.Identity.Test.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test
{
    public class CircleList : List<Circle>
    {
        public CircleList()
        {
            Circle_1.Inclusions = new[] { Circle_2 };
            Circle_1.Exclusions = new Circle[] { };

            Circle_2.Inclusions = new Circle[] { };
            Circle_2.Exclusions = new[] { Circle_1 };

            AddRange(new[] { Circle_1, Circle_2 });
        }
        public const string CIRCLE_1 = nameof(CIRCLE_1);
        public Circle Circle_1 = new Circle
        {
            Id = CIRCLE_1,
            Name = "Circle 1",
        };
        public const string CIRCLE_2 = nameof(CIRCLE_2);
        public Circle Circle_2 = new Circle
        {
            Id = CIRCLE_2,
            Name = "Circle 2",
        };
    }
}
