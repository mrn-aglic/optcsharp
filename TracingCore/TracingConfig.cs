using Newtonsoft.Json;

namespace TracingCore
{
    public class InstrumentationConfig
    {
        public string ReturnVarTemplate { get; set; }
        public PropertyInstrumentationConfig Property { get; set; } = new PropertyInstrumentationConfig();
    }
    
    public class PropertyInstrumentationConfig
    {
        public string BackupNamePrefix { get; set; }
    }
}