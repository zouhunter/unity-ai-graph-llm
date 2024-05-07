using System;
namespace AIScripting
{
    public class InPortAttribute : Attribute
    {
        public int index { get; private set; }
        public int max { get; private set; }
        public InPortAttribute(int id = 0,int max = int.MaxValue)
        {
            this.index = id;
            this.max = max;
        }
    }

    public class OutPortAttribute : Attribute
    {
        public int index { get; private set; }
        public int max { get; private set; }

        public OutPortAttribute(int id = 0, int max = int.MaxValue)
        {
            this.index = id;
            this.max = max;
        }
    }
}