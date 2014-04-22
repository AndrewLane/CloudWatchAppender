using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
using log4net.Util;

namespace CloudWatchAppender.Model
{
    public class MetricDatum
    {
        Amazon.CloudWatch.Model.MetricDatum _datum = new Amazon.CloudWatch.Model.MetricDatum();
        PutMetricDataRequest _request = new PutMetricDataRequest();

        private double? _value;
        private double? _sum;
        private double? _max;
        private double? _min;
        private double? _sampleCount;

        private bool _statisticsMode;
        private bool _valueMode;

        private DateTimeOffset? _timestamp;

        public double Value
        {
            get
            {
                return _datum.Value;
            }
            set
            {
                if (_value.HasValue)
                    throw new MetricDatumFilledException("Value has been set already.");

                if (StatisticsMode)
                    throw new MetricDatumFilledException("Value cannot be set since we're in statistics mode.");

                _valueMode = true;
                _value = value;
                _datum.Value = value;
            }
        }

        public StandardUnit Unit
        {
            get { return _datum.Unit; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    if (!string.IsNullOrEmpty(_datum.Unit) && _datum.Unit != value)
                        throw new MetricDatumFilledException("Unit has been set already.");

                    _datum.Unit = value;

                    if (_datum.Unit != value)
                        LogLog.Warn(typeof(MetricDatum), string.Format("Unit {0} not supported. Using default.", value));
                }
            }
        }

        public string MetricName
        {
            get { return _datum.MetricName; }
            set
            {
                if (!string.IsNullOrEmpty(_datum.MetricName))
                    throw new MetricDatumFilledException("MetricName has been set already.");

                _datum.MetricName = value;
            }
        }

        public string NameSpace
        {
            get { return _request.Namespace; }
            set
            {
                if (!string.IsNullOrEmpty(_request.Namespace))
                    throw new MetricDatumFilledException("NameSpace has been set already.");

                _request.Namespace = value.Replace(".", "/");
            }
        }

        public double Maximum
        {
            get { return _datum.StatisticValues.Maximum; }
            set
            {
                if (_max.HasValue)
                    throw new MetricDatumFilledException("Value has been set already.");

                if (ValueMode)
                    throw new MetricDatumFilledException("Statistics cannot be set since we're in value mode.");

                _max = value;

                _statisticsMode = true;
                if (_datum.StatisticValues == null)
                    _datum.StatisticValues = new StatisticSet();

                _datum.StatisticValues.Maximum = value;
            }
        }

        public double Minimum
        {
            get { return _datum.StatisticValues.Minimum; }
            set
            {
                if (_min.HasValue)
                    throw new MetricDatumFilledException("Value has been set already.");

                if (ValueMode)
                    throw new MetricDatumFilledException("Statistics cannot be set since we're in value mode.");

                _min = value;

                _statisticsMode = true;
                if (_datum.StatisticValues == null)
                    _datum.StatisticValues = new StatisticSet();

                _datum.StatisticValues.Minimum = value;
            }
        }

        public double Sum
        {
            get { return _datum.StatisticValues.Sum; }
            set
            {
                if (_sum.HasValue)
                    throw new MetricDatumFilledException("Value has been set already.");

                if (ValueMode)
                    throw new MetricDatumFilledException("Statistics cannot be set since we're in value mode.");

                _sum = value;

                _statisticsMode = true;
                if (_datum.StatisticValues == null)
                    _datum.StatisticValues = new StatisticSet();

                _datum.StatisticValues.Sum = value;
            }
        }

        public double SampleCount
        {
            get { return _datum.StatisticValues.SampleCount; }
            set
            {
                if (_sampleCount.HasValue)
                    throw new MetricDatumFilledException("Value has been set already.");

                if (ValueMode)
                    throw new MetricDatumFilledException("Statistics cannot be set since we're in value mode.");

                _sampleCount = value;

                _statisticsMode = true;
                if (_datum.StatisticValues == null)
                    _datum.StatisticValues = new StatisticSet();

                _datum.StatisticValues.SampleCount = value;
            }
        }

        public DateTimeOffset? Timestamp
        {
            get
            {
                try
                {
                    return _datum.Timestamp;
                }
                catch (Exception)
                {
                    return null;
                }
            }
            set
            {
                if (_timestamp.HasValue)
                    throw new MetricDatumFilledException("Value has been set already.");

                _timestamp = value;

                _datum.Timestamp = value.Value.UtcDateTime;
            }
        }

        internal PutMetricDataRequest Request
        {
            get
            {
                if (!_request.MetricData.Any())
                    _request.MetricData.Add(_datum);
                return _request;
            }
        }

        internal Amazon.CloudWatch.Model.MetricDatum AWSDatum
        {
            get { return _datum; }
        }

        public List<Dimension> Dimensions
        {
            get { return AWSDatum.Dimensions; }
            set
            {
                if (AWSDatum.Dimensions.Count != 0)
                    throw new MetricDatumFilledException("Value has been set already.");

                AWSDatum.Dimensions = value;
            }
        }

        public bool StatisticsMode
        {
            get { return _statisticsMode; }
        }

        public bool ValueMode
        {
            get { return _valueMode; }
            set { _valueMode = value; }
        }

        public static readonly ISet<string> SupportedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                                                    {
                                                        "Value",
                                                        "Unit",
                                                        "Dimension",
                                                        "Dimensions",
                                                        "NameSpace",
                                                        "MetricName",
                                                        "Timestamp"
                                                    };

        public static readonly ISet<string> SupportedStatistics = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                                                    {
                                                        "Maximum",
                                                        "Minimum",
                                                        "SampleCount",
                                                        "Sum"
                                                    };

        public string Message { get; set; }

        public MetricDatum(string s)
        {
            Message = s;
        }

        public MetricDatum()
        {
        }

        public MetricDatum WithMetricName(string value)
        {
            _datum.MetricName = value;
            return this;
        }

        public MetricDatum WithUnit(StandardUnit value)
        {
            _datum.Unit = value;
            return this;
        }

        public MetricDatum WithValue(double value)
        {
            _datum.Value = value;
            return this;
        }

        public MetricDatum WithTimestamp(DateTime value)
        {
            _datum.Timestamp = value;
            return this;
        }

        public MetricDatum WithTimestamp(DateTimeOffset value)
        {
            _datum.Timestamp = value.UtcDateTime;
            return this;
        }

        public MetricDatum WithDimensions(IEnumerable<Dimension> value)
        {
            _datum.Dimensions = value.ToList();
            return this;
        }

        public MetricDatum WithStatisticValues(StatisticSet statisticSet)
        {
            _datum.StatisticValues = statisticSet;
            return this;
        }

        public override string ToString()
        {
            var s = new StringWriter();
            new MetricDatumRenderer().RenderObject(null, this, s);

            return "MetricDatum, NameSpace: " + NameSpace + ", " + s;
        }
    }

    public class MetricDatumFilledException : InvalidOperationException
    {
        public MetricDatumFilledException(string message)
            : base(message)
        {

        }
    }
}