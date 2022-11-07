using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using System;

namespace Noord.Hollands.Archief.Preingest.WorkerService.Entities.CommandKey
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ValidationActionType
    {
        ContainerChecksumHandler,
        ExportingHandler,
        ReportingPdfHandler,
        ReportingDroidXmlHandler,
        ReportingPlanetsXmlHandler,
        ProfilesHandler,
        EncodingHandler,
        UnpackTarHandler,
        MetadataValidationHandler,
        NamingValidationHandler,
        GreenListHandler,
        ExcelCreatorHandler,
        ScanVirusValidationHandler,
        SidecarValidationHandler, 
        PrewashHandler,
        ShowBucketHandler,
        ClearBucketHandler,
        BuildOpexHandler,
        PolishHandler,
        UploadBucketHandler,
        FilesChecksumHandler,
        IndexMetadataHandler,
        PasswordDetectionHandler,
        ToPX2MDTOHandler,
        PronomPropsHandler,
        RelationshipHandler,
        FixityPropsHandler,
        BinaryFileObjectValidationHandler,
        BinaryFileMetadataMutationHandler,
        BuildNonMetadataOpexHandler
    }
    public interface IKey : IEquatable<IKey>
    {
        ValidationActionType Name { get; set; }
    }
}
