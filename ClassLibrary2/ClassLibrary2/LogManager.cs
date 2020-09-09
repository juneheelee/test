using System;
using System.Configuration;
using System.IO;
using NLog;
using NLog.Targets;

namespace ClassLibrary2
{
    public class LogManager
    {
        private static readonly Lazy<LogManager> m_instance = new Lazy<LogManager>(
            () => new LogManager());

        private LOGLEVEL e_logLevel;
        private FileTarget m_target;
        private Logger m_logger;
        private Configuration m_config;
        private string m_projectName;
        private string m_projectVersion;
        private string m_configPath;
        public enum LOGLEVEL
        {
            NONE = 0,
            TRACE,
            DEBUG,
            INFO,
            WARN,
            ERROR,
            FATAL
        }

        private LogManager()
        {
            e_logLevel = LOGLEVEL.NONE;

            m_projectName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
            m_projectVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            m_configPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), m_projectName, m_projectVersion);

            m_target = new FileTarget();
            m_target.Layout = "${processid} ${longdate} [${level}]   ${message}${exception:format=ToString}";
            m_target.FileName = "${basedir}/logs/${shortdate} log.txt";
            m_target.ArchiveAboveSize = 10000 * 1024; // archive files greater than 10 MB
            m_target.ConcurrentWrites = true;

            NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(m_target, LogLevel.Debug);
            m_logger = NLog.LogManager.GetLogger("CavuLogger");

            SetConfig();
        }
        public static LogManager Instance
        {
            get
            {
                return m_instance.Value;
            }
        }

        public NLog.LogLevel GetLogLevel(LogManager.LOGLEVEL level)
        {
            switch (level)
            {
                case LOGLEVEL.TRACE:
                    return NLog.LogLevel.Trace;
                case LOGLEVEL.DEBUG:
                    return NLog.LogLevel.Debug;
                case LOGLEVEL.INFO:
                    return NLog.LogLevel.Info;
                case LOGLEVEL.WARN:
                    return NLog.LogLevel.Warn;
                case LOGLEVEL.ERROR:
                    return NLog.LogLevel.Error;
                case LOGLEVEL.FATAL:
                    return NLog.LogLevel.Fatal;
            }
            return NLog.LogLevel.Warn;
        }
        private void SetConfig()
        {
            try
            {
                string p1 = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                Console.WriteLine(p1);
                string path = Path.Combine(p1, "App.config");
                if (File.Exists(path) == false)
                {
                    throw new FileNotFoundException("App.config file not found");
                }

                m_config = this.OpenConfiguration(path);
                string value = m_config.AppSettings.Settings["LogLevel"].Value;

                LOGLEVEL level;
                if (Enum.TryParse<LOGLEVEL>(value.ToUpper(), out level))
                {
                    if (e_logLevel.Equals(level) == false)
                    {
                        NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(m_target, GetLogLevel(level));
                        e_logLevel = level;
                    }
                }
                else
                {
                    throw new Exception("Fail to Set Log Level");
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        private Configuration OpenConfiguration(string configFile)
        {
            Configuration config = null;

            ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
            fileMap.ExeConfigFilename = configFile;
            config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);

            return config;
        }

        public void LogWrite(string msg, LOGLEVEL level = LOGLEVEL.INFO)
        {
            switch (level)
            {
                case LOGLEVEL.NONE:
                    break;
                case LOGLEVEL.TRACE:
                    m_logger.Trace(msg);
                    break;
                case LOGLEVEL.DEBUG:
                    m_logger.Debug(msg);
                    break;
                case LOGLEVEL.INFO:
                    m_logger.Info(msg);
                    break;
                case LOGLEVEL.WARN:
                    m_logger.Warn(msg);
                    break;
                case LOGLEVEL.ERROR:
                    m_logger.Error(msg);
                    break;
                case LOGLEVEL.FATAL:
                    m_logger.Fatal(msg);
                    break;
                default:
                    break;
            }
        }
    }
}
