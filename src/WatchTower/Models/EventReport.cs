using System;
using System.Collections.Generic;

namespace WatchTower
{
    public class EventReport
    {
        public int EventId { get; set; }
        public string RuleName { get; set; } = string.Empty;

        public DateTime UtcTime { get; set; }
        //public DateTime TimeStamp { get; set; }
        public DateTime CreationUtcTime { get; set; }

        public EventName EventName { get; set; }

        //public string EventNameStr { get; set; }
       // public string ProcessGuid { get; set; }
        public string ProcessId { get; set; }
        public string Image { get; set; }
        public string TargetFilename { get; set; }
        public string User { get; set; }
        public string ComputerName { get; set; }
        public DateTime? FileTimeDateStamp { get; set; }
        public string Md5 { get; set; }
        public string Sha1 { get; set; }
        public string Sha256 { get; set; }
        public string ImpHash { get; set; }
        public string SsDeep { get; set; }
        public string TypeRefHash { get; set; }


        public double Entropy { get; set; }

        public string ZoneIdentifier { get; set; }

        public string PdbFileName { get; set; }
        public string Machine { get; set; }
        public string SubSystem { get; set; }

        public string MetaDataHeaderSignature { get; set; }
        public bool HasExportTable { get; set; }
        public bool HasImportTable { get; set; }
        public string Architecture { get; set; }
        public bool IsExecutableImage { get; set; }
        public bool IsDotNet { get; set; }
        public bool IsSigned { get; set; }

        public bool IsTrustedAuthenticodeSignature { get; set; }
        public bool HasValidAuthenticodeCertChain { get; set; }
        public string SigningAuthenticodeCertificateIssuer { get; set; }
        public string CertificateSubject { get; set; }

        public string InterestingStrings { get; set; }

        public DateTime CertificateNotValidBefore { get; set; }
        public DateTime CertificateNotValidAfter { get; set; }

        public IsoDiscInformation IsoDisc { get; set; }
        public LnkFileInformation LnkFile { get; set; }
    }
}
