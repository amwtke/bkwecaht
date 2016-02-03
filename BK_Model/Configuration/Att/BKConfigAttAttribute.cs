using System;

namespace BK.Model.Configuration.Att
{
    public class BKConfigAttribute : Attribute
    {
        string moduleName, functionName;
        public BKConfigAttribute(string module, string func)
        {
            moduleName = module;
            functionName = func;
        }
        public BKConfigAttribute(string module)
        {
            moduleName = module;
        }
        public string Module
        {
            get { return moduleName; }
            set { moduleName = value; }
        }

        public string Function
        {
            get { return functionName;}
            set { functionName = value; }
        }
    }

    public class BKKeyAttribute : Attribute
    {
        public string Key { get; set; }
        public BKKeyAttribute(string key)
        {
            Key = key;
        }
    }
}