using System;

namespace WatchTower
{
    public class FileSignatureInformation
    {
        public bool IsTrustedAuthenticodeSignature { get; set; }
        public bool HasValidAuthenticodeCertChain { get; set; }
        public string SigningAuthenticodeCertificateIssuer { get; set; }
        public string CertificateSubject { get; set; }
        public DateTime CertificateNotValidBefore { get; set; }
        public DateTime CertificateNotValidAfter { get; set; }
    }
}
