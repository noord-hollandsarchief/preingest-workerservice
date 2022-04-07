using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Noord.Hollands.Archief.Preingest.WorkerService.Entities.EventHub
{
    public class Settings
    {
        public string ChecksumType { get; set; }
        public string ChecksumValue { get; set; }
        public string Prewash { get; set; }
        /**
         * PolishHandler: polish xslt filename, use saxon as transformation (JAVA, very slow)
         * */
        public string Polish { get; set; }
        /**          
         * BuildOpexHandler: build option
         * **/        
        public string MergeRecordAndFile { get; set; }

        /**
         * IndexMetadataHandler: validation schema, extra xml 
         */
        public string SchemaToValidate { get; set; }
        public string RootNamesExtraXml { get; set; }
        public string IgnoreValidation { get; set; }
    }
}
