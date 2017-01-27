﻿using System;
using System.Collections.Generic;
using System.Linq;
using Saturn72.Core.Domain.Logging;
using Saturn72.Core.Logging;
using Saturn72.Extensions;

namespace Saturn72.Core.Services.Impl.Logging
{
    public class DefaultLogger : ILogger
    {
        #region Fields

        private readonly ILogRecordRepository _logRecordRespository;

        #endregion

        #region ctor

        public DefaultLogger(ILogRecordRepository logRecordRespository)
        {
            _logRecordRespository = logRecordRespository;
        }

        #endregion

        public LogLevel[] SupportedLogLevels => LogLevel.AllSystemLogLevels.ToArray();

        public void DeleteLogRecord(LogRecordModel logRecord)
        {
            throw new NotImplementedException();
        }


        public IEnumerable<LogRecordModel> GetAllLogRecords()
        {
            return _logRecordRespository.GetAllLogRecords();
        }

        public LogRecordModel GetLogById(long logRecordId)
        {
            Guard.GreaterThan(logRecordId, (long) 0);
            return _logRecordRespository.GetLogRecordById(logRecordId);
        }

        public LogRecordModel InsertLog(LogLevel logLevel, string shortMessage, string fullMessage = "",
            Guid contextId = new Guid())
        {
            Guard.NotNull(logLevel);
            Guard.HasValue(shortMessage);
            var logRecord = new LogRecordModel
            {
                LogLevel = logLevel,
                ShortMessage = shortMessage,
                FullMessage = fullMessage,
                ContextId = contextId
            };

            return _logRecordRespository.AddLogRecord(logRecord);
        }
    }
}