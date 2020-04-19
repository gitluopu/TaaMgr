using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LiveCharts.Wpf;
using System.ComponentModel;
using LiveCharts;
using LiveCharts.Configurations;
namespace PPs
{
    /// <summary>
    /// Interaction logic for PPsCtl.xaml
    /// </summary>

    public class MeasureModel
    {
        public long Cnt { get; set; }
        public long Value { get; set; }
    }
    public partial class PPsCtl : UserControl,INotifyPropertyChanged
    {
        private long m_cnt = 0;
        private const int m_xSize = 120;
        public PPsCtl()
        {
            InitializeComponent();
            var mapper = Mappers.Xy<MeasureModel>()
               .X(model => model.Cnt)   //use DateTime.Ticks as X
               .Y(model => model.Value);           //use the value property as Y

            //lets save the mapper globally.
            Charting.For<MeasureModel>(mapper);

            //the values property will store our values array
            m_ppsVisible = true;
            m_bpsVisible = false;
            m_ppsY = new ChartValues<MeasureModel>();
            m_bpsY = new ChartValues<MeasureModel>();

            SetAxisLimits(m_cnt);
            //The next code simulates data changes every 300 ms

            //m_linePps.Visibility = BooleanToVisibilityConverter.Equals
            DataContext = this;
        }

        private bool _m_ppsVisible;
        public bool m_ppsVisible
        {
            get { return _m_ppsVisible; }
            set
            {
                _m_ppsVisible = value;
                OnPropertyChanged("m_ppsVisible");
            }
        }

        private bool _m_bpsVisible;
        public bool m_bpsVisible
        {
            get { return _m_bpsVisible; }
            set
            {
                _m_bpsVisible = value;
                OnPropertyChanged("m_bpsVisible");
            }
        }

        public ChartValues<MeasureModel> m_ppsY { get; set; }
        public ChartValues<MeasureModel> m_bpsY { get; set; }
        private long _m_xMax;

        public long m_xMax
        {
            get { return _m_xMax; }
            set
            {
                _m_xMax = value;
                OnPropertyChanged("m_xMax");
            }
        }
        private long _m_xMin;
        public long m_xMin
        {
            get { return _m_xMin; }
            set
            {
                _m_xMin = value;
                OnPropertyChanged("m_xMin");
            }
        }


        public void Add(Tuple<long, long> val)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                long pps = val.Item1;
                long bps = val.Item2;
                m_cnt++;
                m_ppsY.Add(new MeasureModel
                {
                    Cnt = m_cnt,
                    Value = pps,
                });
                m_bpsY.Add(new MeasureModel
                {
                    Cnt = m_cnt,
                    Value = bps,
                });

                //更新纵坐标数据
                SetAxisLimits(m_cnt);
                if (m_ppsY.Count > m_xSize) m_ppsY.RemoveAt(0);
                if (m_bpsY.Count > m_xSize) m_bpsY.RemoveAt(0);
            });

        }

        public void Add(object sender, Tuple<long, long> e)
        {
            Add(e);
        }

        private void SetAxisLimits(long cnt)
        {
            if (cnt < m_xSize)
            {
                m_xMin = 0;
                m_xMax = m_xSize;
                return;
            }
            m_xMin = cnt - m_xSize;
            m_xMax = cnt;
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
