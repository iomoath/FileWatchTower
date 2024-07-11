using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DiscUtils.Iso9660;
using PEFile;
using PeNet;
using PeNet.Header.Pe;
using Serilog;
using ShellLink;
using SsdeepNET;
using static System.Collections.Specialized.BitVector32;

namespace WatchTower
{
    public class PostProcessor
    {
        #region Members

        private readonly IConfiguration _configuration;
        private readonly IMonitoringService _monitoringService;
        private volatile bool _running;
        private Queue<JobData> _rawEventDetailsJobQueue;
        private readonly object _rawEventDetailsJobQueueLock = new object();
        private ConcurrentBag<Task> _tasks;
        private IEventReporter _logFileReporter;
        private IEventReporter _winEventLogReporter;
        private IEventReporter _apiReporter;
        private IEventReporter[] _eventReporters;

        private readonly HashSet<string> _discImageFileExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".iso", ".img", ".vhdx", ".vhd", ".vmdk", ".daa", ".cdi" , ".udf", ".mdf", ".nrg", ".bin", ".isoz"
        };

        private readonly HashSet<string> _shortcutFileExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".lnk", ".url", ".scf", ".pif", ".lnkz"
        };


        private readonly HashSet<string> _hashesNeeded;


        private class JobData
        {
            public RawEventDetails RawEventDetails { get; set; }
        }

        #endregion

        #region Public

        public PostProcessor(IMonitoringService monitoringService, IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _monitoringService = monitoringService;
            _monitoringService.NewFileIoEventAvailable += OnNewFileIOEventAvailable;
            _hashesNeeded = _configuration.HashAlgorithms?.Trim().Replace(" ", "").ToLowerInvariant().Split(',').ToHashSet();
        }

        public void Start()
        {
            _running = true;
            Init();
        }

        public async Task Stop()
        {
            _running = false;
            ClearRawEventJobQueue();

            await Task.Run(() =>
            {
                foreach (var task in _tasks)
                {
                    try
                    {
                        task?.Wait();
                    }
                    catch (AggregateException aex)
                    {
                        if (aex.InnerException != null)
                        {
                            //Log.Error(aex.InnerException, aex.InnerException.Message);
                        }

                        aex.Handle(x => true);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e, e.Message);
                    }
                }
            });
        }

        public void Wait()
        {
            Task.Run(() =>
            {
                while (_running)
                {
                    Thread.Sleep(50);
                }
            }).Wait();
        }

        public async Task WaitAsync()
        {
            while (_running)
            {
                await Task.Delay(50);
            }
        }

        #endregion

        #region Init

        private void Init()
        {
            _tasks = new ConcurrentBag<Task>();
            _rawEventDetailsJobQueue = new Queue<JobData>();


            if (_configuration.WriteToWinEventLogs)
            {
                _winEventLogReporter = new EventLogReporter();
                _winEventLogReporter.LogFormat = _configuration.WinEventLogOutputFormat;
            }

            if (!string.IsNullOrWhiteSpace(_configuration.LogDirectoryPath))
            {
                LogFileWriter.Instance.SetDirectory(_configuration.LogDirectoryPath);
                _logFileReporter = new LogFileEventReporter();
                _logFileReporter.LogFormat = _configuration.LogFileOutputFormat;

            }

            if (!string.IsNullOrWhiteSpace(_configuration.ApiLogEndpointUrl) && Uri.IsWellFormedUriString(_configuration.ApiLogEndpointUrl, UriKind.Absolute))
            {
                _apiReporter = new ApiEventReporter();
                _apiReporter.LogFormat = _configuration.ApiLogOutputFormat;

            }


            _eventReporters = new[] { _winEventLogReporter, _logFileReporter, _apiReporter };


            CreateJobConsumers();
        }

        private void CreateJobConsumers()
        {
            for (var i = 0; i < GlobalConfigs.MaxPostProcessorWorkerThreads; i++)
            {
                var task = new Task<Task>(RawEventJobConsumer, TaskCreationOptions.LongRunning);
                task.Start();
                _tasks.Add(task);
            }
        }

        #endregion


        #region Job Queue

        private void EnqueueRawEventJob(JobData e)
        {
            lock (_rawEventDetailsJobQueueLock)
            {
                if (!_running || e.RawEventDetails == null)
                    return;

                _rawEventDetailsJobQueue.Enqueue(e);
            }
        }

        private JobData DequeueRawEventJob()
        {
            lock (_rawEventDetailsJobQueueLock)
            {
                if (_rawEventDetailsJobQueue == null || _rawEventDetailsJobQueue.Count == 0)
                {
                    return null;
                }

                return _rawEventDetailsJobQueue.Dequeue();
            }
        }

        private void ClearRawEventJobQueue()
        {
            lock (_rawEventDetailsJobQueueLock)
            {
                _rawEventDetailsJobQueue?.Clear();
            }
        }

        #endregion


        #region Job Consumer

        private async Task RawEventJobConsumer()
        {
            while (_running)
            {
                await Task.Delay(5);

                try
                {
                    var jobData = DequeueRawEventJob();

                    if (jobData == null)
                        continue;

                    await ProcessRawEvent(jobData);
                }
                catch (Exception e)
                {
                    Log.Error(e, e.Message);
                }
            }
        }


        #endregion

        #region Processing

        private async Task ProcessRawEvent(JobData jobData)
        {
            var rawEvent = jobData.RawEventDetails;
            if (string.IsNullOrEmpty(rawEvent.TargetFilename))
                return;

            try
            {
                var fileInfo = GetFileInfo(rawEvent);

                if (fileInfo == null || fileInfo.FileSize > _configuration.MaxTargetFileSize)
                    return;

                var eventReport = CreateEventReport(fileInfo, jobData);

                // Send to available Reporters
                await ReportEvent(eventReport);
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
            }

        }

        private async Task ReportEvent(EventReport eventReport)
        {
            var tasks = _eventReporters
                .Where(reporter => reporter != null)
                .Select(reporter => Task.Run(() => reporter.Report(eventReport)))
                .ToArray();

            await Task.WhenAll(tasks);
        }

        #endregion

        #region Report Building

        private EventReport CreateEventReport(FileInformation fileInfo, JobData jobData)
        {
            var eventReport = new EventReport
            {
                EventId = jobData.RawEventDetails.EventId,
                IsoDisc = GetIsoDiscInformation(Helpers.FromBytesToMemoryStream(fileInfo.Bytes), jobData.RawEventDetails.TargetFilename),
                LnkFile = GetLnkFileInformation(fileInfo.Bytes, jobData.RawEventDetails.TargetFilename),
                Entropy = new DataEntropyCalculatorUtf8(fileInfo.Bytes).Entropy,
                ZoneIdentifier = NtfsZoneIdentifierParser.ZoneInformation(jobData.RawEventDetails.TargetFilename),
                UtcTime = jobData.RawEventDetails.UtcTime,
                //TimeStamp = jobData.RawEventDetails.TimeStamp,
                EventName = jobData.RawEventDetails.EventName,
                //EventNameStr = jobData.RawEventDetails.EventNameStr,
                //ProcessGuid = jobData.RawEventDetails.ProcessGuid,
                ProcessId = jobData.RawEventDetails.ProcessId,
                Image = jobData.RawEventDetails.Image,
                TargetFilename = jobData.RawEventDetails.TargetFilename,
                User = jobData.RawEventDetails.User,
                ComputerName = jobData.RawEventDetails.ComputerName,
                CreationUtcTime = jobData.RawEventDetails.CreationUtcTime,
                RuleName = jobData.RawEventDetails.RuleName,
            };

            eventReport.InterestingStrings = string.Join(",", fileInfo.Strings.ToArray());

            eventReport = GetHashes(fileInfo, eventReport);

            if (fileInfo.PeFile == null) 
                return eventReport;

            eventReport.HasExportTable = fileInfo.PeFile.ExportedFunctions != null;
            eventReport.HasImportTable = fileInfo.PeFile.ImportedFunctions != null;
            eventReport.IsDotNet = fileInfo.PeFile.IsDotNet;
            eventReport.IsSigned = fileInfo.PeFile.HasValidAuthenticodeSignature;
            eventReport.Machine = fileInfo.PeFile.ImageNtHeaders?.FileHeader.Machine.ToString();
            eventReport.SubSystem = fileInfo.PeFile.ImageNtHeaders?.OptionalHeader.Subsystem.ToString();
            eventReport.IsExecutableImage = IsExecutable(fileInfo.PeFile);
            eventReport.PdbFileName = GetPdbFileName(fileInfo.PeFile);
            eventReport.Architecture = GetFileArchitecture(fileInfo.PeFile);
            eventReport.MetaDataHeaderSignature = fileInfo.PeFile.MetaDataHdr?.Signature.ToString();

            eventReport = GetFileSignatureInformation(fileInfo.PeFile, eventReport);

            if (fileInfo.PeFile.ImageNtHeaders?.FileHeader != null)
            {
                eventReport.FileTimeDateStamp = Helpers.UnixTimeStampToDateTime(fileInfo.PeFile.ImageNtHeaders.FileHeader.TimeDateStamp);
            }

            return eventReport;
        }

        private FileInformation GetFileInfo(RawEventDetails rawEvent)
        {
            var peFile = PeFileHelper.GetPeFile(rawEvent.TargetFilename);
            var fileInfo = new FileInformation { PeFile = peFile };

            if (peFile == null)
            {
                fileInfo.Bytes = Helpers.ReadFile(rawEvent.TargetFilename);
                if (fileInfo.Bytes == null)
                {
                    // File read error, deleted/moved or locked
                    return null;
                }
            }

            fileInfo.Strings = GetPeInterestingStrings(peFile);

            return fileInfo;
        }

        private List<string> GetPeInterestingStrings(PeFile peFile)
        {
            // thanks to @horsicq - https://github.com/horsicq/Detect-It-Easy/ for signatures

            var list = new HashSet<string>();

            if (peFile == null || peFile.ImageSectionHeaders == null)
                return list.ToList();

            foreach (var section in peFile.ImageSectionHeaders)
            {
                if (!section.Name.Contains(".text") && !section.Name.Contains(".rdata"))
                    continue;


                var sectionData = peFile.RawFile.AsSpan((int)section.PointerToRawData, (int)section.SizeOfRawData).ToArray();
                var strings = ExtractReadableStrings(sectionData, 10);

                foreach (var str in strings)
                {
                    if (str.Contains("Go build ID:"))
                    {
                        list.Add("Go Compiler");
                    }

                    if (peFile.IsDotNet && str.Contains("<Eazfuscator CI>.") || str.Contains("<Eazfuscator>."))
                    {
                        list.Add("Eazfuscator");
                    }

                    // add more .. move to a separate class, refactor..
                }
            }

            return list.ToList();
        }

        private static IEnumerable<string> ExtractReadableStrings(byte[] data, int minimumLength)
        {
            var result = new List<string>();
            var currentString = new StringBuilder();

            foreach (var b in data)
            {
                if (b >= 32 && b <= 126) // ASCII printable characters
                {
                    currentString.Append((char)b);
                }
                else
                {
                    if (currentString.Length >= minimumLength)
                    {
                        result.Add(currentString.ToString());
                    }
                    currentString.Clear();
                }
            }

            if (currentString.Length >= minimumLength)
            {
                result.Add(currentString.ToString());
            }

            return result;
        }


        private EventReport GetHashes(FileInformation fi, EventReport e)
        {
            if (_hashesNeeded == null || _hashesNeeded.Count == 0)
                return null;


            if (_hashesNeeded.Contains("md5"))
                e.Md5 = fi.PeFile?.Md5 ?? Helpers.ComputeFileHash(e.TargetFilename, MD5.Create());

            if (_hashesNeeded.Contains("sha1"))
                e.Sha1 = fi.PeFile?.Sha1 ?? Helpers.ComputeFileHash(e.TargetFilename, SHA1.Create());

            if (_hashesNeeded.Contains("sha256"))
                e.Sha256 = fi.PeFile?.Sha256 ?? Helpers.ComputeFileHash(e.TargetFilename, SHA256.Create());

            // possible null value, input is not a PE
            if (_hashesNeeded.Contains("imphash"))
                e.ImpHash = fi.PeFile?.ImpHash;

            // possible null value, input is not a PE
            if (_hashesNeeded.Contains("ssdeep") && fi.IsPeFile)
            {
                e.SsDeep = new FuzzyHash().ComputeHash(fi.Bytes);
            }

            // possible null value, input is not a PE
            if (_hashesNeeded.Contains("typeref") && fi.IsPeFile)
                e.TypeRefHash = fi.PeFile?.TypeRefHash;

            return e;
        }


        private EventReport GetFileSignatureInformation(PeFile pe, EventReport e)
        {
            if (!pe.HasValidAuthenticodeSignature)
                return e;


            e.IsTrustedAuthenticodeSignature = pe.IsTrustedAuthenticodeSignature;
            e.HasValidAuthenticodeCertChain = pe.HasValidAuthenticodeCertChain(_configuration.CheckRevocation);
            e.SigningAuthenticodeCertificateIssuer = pe.SigningAuthenticodeCertificate?.Issuer;

            if (pe.AuthenticodeInfo?.SigningCertificate != null)
            {
                e.CertificateSubject = pe.AuthenticodeInfo.SigningCertificate.Subject;
                e.CertificateNotValidBefore = pe.AuthenticodeInfo.SigningCertificate.NotBefore;
                e.CertificateNotValidAfter = pe.AuthenticodeInfo.SigningCertificate.NotAfter;
            }

            return e;
        }

        private LnkFileInformation GetLnkFileInformation(byte[] fileBytes, string filePath)
        {
            if (!IsShortcutFile(filePath))
                return null;

            try
            {
                var s = Shortcut.FromByteArray(fileBytes);
                return new LnkFileInformation
                {
                    Path = s.LinkTargetIDList?.Path,
                    CommandLineArguments = s.StringData?.CommandLineArguments?.NormalizeLineEndings("")
                };
            }
            catch (Exception)
            {
                return null;
            }
        }

        private bool IsShortcutFile(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return _shortcutFileExtensions.Contains(extension);
        }

        public IsoDiscInformation GetIsoDiscInformation(MemoryStream stream, string filePath)
        {
            if (!IsDisImage(filePath))
                return null;

            try
            {

                CDReader cd = new CDReader(stream, true);
                return new IsoDiscInformation
                {
                    VolumeLabel = cd.VolumeLabel,
                    ActiveVariant = cd.ActiveVariant.ToString(),
                    FileCount = cd.Root.GetFiles().Length
                };
            }
            catch
            {
                return null;
            }
        }

        private bool IsDisImage(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return _discImageFileExtensions.Contains(extension);
        }

        private string GetFileArchitecture(PeFile pe)
        {
            if (pe.Is32Bit)
                return "32Bit";
            
            if (pe.Is64Bit)
                return "64Bit";

            return null;
        }

        private bool IsExecutable(PeFile pe)
        {
            if (pe.ImageNtHeaders != null)
            {
                return (pe.ImageNtHeaders.FileHeader.Characteristics & FileCharacteristicsType.ExecutableImage) != 0;
            }

            return pe.IsDll || pe.IsExe;
        }

        private string GetPdbFileName(PeFile pe)
        {
            if (pe.ImageDebugDirectory != null && pe.ImageDebugDirectory.Length > 0)
            {
                var a = pe.ImageDebugDirectory[0];
                return a.CvInfoPdb70?.PdbFileName;
            }

            return null;
        }

        
        #endregion



        #region Event Handlers

        private void OnNewFileIOEventAvailable(object sender, NewFileIOEventAvailableEventArgs e)
        {
            EnqueueRawEventJob(new JobData { RawEventDetails = e.RawEventDetails });
        }

        #endregion

    }
}
