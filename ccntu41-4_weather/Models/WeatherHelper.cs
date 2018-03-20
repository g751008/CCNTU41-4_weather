using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI.DataVisualization.Charting;
using System.Xml;

namespace ccntu41_4_weather.Models
{
    public class WeatherHelper
    {
        //設定天氣預報類別
        public class WeatherClass
        {
            public String City = null; //城市
            public DateTime DT = new DateTime(); //日期
            public String DN = null; //日夜
            public String Climate = null;//氣候
            public Int32 MaxT = 0;//最高溫
            public Int32 MinT = 0;//最低溫
        }

        public static List<WeatherClass> WeatherList = new List<WeatherClass>();//設定天氣預報清單

        //新增天氣資料(城市、日期、日夜)
        private static void AddWeatherData(String City, DateTime DT, string DN)
        {
            if (WeatherList.Where(A => A.City.Equals(City) && A.DT.Equals(DT) && A.DN.Equals(DN)).Count().Equals(0))
            {
                WeatherList.Add(new WeatherHelper.WeatherClass
                {
                    City = City,
                    DT = DT,
                    DN = DN
                });
            }
        }

        //更新天氣資料(城市、日期、日夜、分類、資料)
        private static void UpdateWeatherData(String City, DateTime DT, string DN, String Code, String Data)
        {
            //取出欲更新的資料
            WeatherClass WeatherData = WeatherList.First(A => A.City.Equals(City) && A.DT.Equals(DT) && A.DN.Equals(DN));

            //該筆資料索引值
            Int32 WeatherIndex = WeatherList.IndexOf(WeatherData);

            //依分類設定更新的內容
            switch (Code)
            {
                case "Wx":
                    WeatherData.Climate = Data;
                    break;

                case "MaxT":
                    WeatherData.MaxT = Int32.Parse(Data);
                    break;

                case "MinT":
                    WeatherData.MinT = Int32.Parse(Data);
                    break;
            }

            //進行更新
            WeatherList[WeatherIndex] = WeatherData;
        }

        //天氣預報資料處理
        private static void WeatherDataProcess(XmlTextReader WeatherXML)
        {
            //天氣預報清單
            String City = null; //城市
            DateTime DT = new DateTime(); //日期
            String DN = null; //日夜
            Boolean IsMorning = true; //是否為白天(預設為白天)
            String Code = null; //分類

            //讀取XML內容
            while (WeatherXML.Read())
            {
                switch (WeatherXML.Name)
                {
                    case "locationName": //城市
                        City = WeatherXML.ReadString();
                        break;
                    case "startTime": //日期
                        DT = DateTime.Parse(WeatherXML.ReadString().Split('T')[0]);
                        if (IsMorning)
                        {
                            DN = "白天";
                            IsMorning = false;
                        }
                        else
                        {
                            DN = "晚上";
                            IsMorning = true;
                        }
                        AddWeatherData(City, DT, DN);
                        break;
                    case "elementName": //分類
                        Code = WeatherXML.ReadString();
                        break;
                    case "parameterName": //更新氣候資料
                        UpdateWeatherData(City, DT, DN, Code, WeatherXML.ReadString());
                        break;
                }
            }

            //關閉讀取XML內容
            WeatherXML.Close();
        }

        //建立天氣預報資料清單
        public static void BuildWeatherList()
        {
            //中央氣象局XML資料來源
            string Path = "http://opendata.cwb.gov.tw/govdownload?dataid=F-C0032-005&authorizationkey=rdec-key-123-45678-011121314";
            Uri URL = new Uri(Path);

            //天氣預報資料的傳送與接收
            using (WebClient WeatherClient = new WebClient())
            {
                try
                {
                    //讀取XML內容
                    using (XmlTextReader WeatherXML = new XmlTextReader(WeatherClient.OpenRead(URL)))
                    {
                        WeatherDataProcess(WeatherXML); //天氣預報資料處理
                        WeatherXML.Close(); //關閉讀取內容
                    }
                }
                catch (Exception)
                {
                    using (XmlTextReader WeatherXML = new XmlTextReader(WeatherClient.OpenRead(URL)))
                    {
                        WeatherDataProcess(WeatherXML);
                        WeatherXML.Close();
                    }
                }

                //釋放WebClient資源
                WeatherClient.Dispose();
            }
        }

        //取得天氣預報序列(城市、氣溫)
        private static Series GetWeatherSeries(String City, String Temperature)
        {
            //宣告WeatherSeries用來存放天氣資料
            Series WeatherSeries = new Series(Temperature);

            //設定線條顏色
            if (Temperature.Equals("最高溫"))
            {
                WeatherSeries.Color = System.Drawing.Color.Crimson;
            }
            else if (Temperature.Equals("最低溫"))
            {
                WeatherSeries.Color = System.Drawing.Color.DodgerBlue;
            }

            //設定字型
            WeatherSeries.Font = new System.Drawing.Font("微軟正黑體", 12);

            //設定曲線
            WeatherSeries.ChartType = SeriesChartType.Spline;

            //設定線的寬度
            WeatherSeries.BorderWidth = 3;

            //設定圓形標示
            WeatherSeries.MarkerStyle = MarkerStyle.Circle;

            //設定標示大小
            WeatherSeries.MarkerSize = 10;

            //將預報內容新增到WeatherSeries
            foreach (WeatherClass X in WeatherList.Where(A => A.City.Equals(City)))
            {
                if (Temperature.Equals("最高溫"))
                {
                    WeatherSeries.Points.AddXY(X.DT.ToString("yyyy/MM/dd") + "\r\n" + X.DN, X.MaxT);
                }
                else if (Temperature.Equals("最低溫"))
                {
                    WeatherSeries.Points.AddXY(X.DT.ToString("yyyy/MM/dd") + "\r\n" + X.DN, X.MinT);
                }
            }

            //將數值顯示在曲線上
            WeatherSeries.IsValueShownAsLabel = true;

            return WeatherSeries;
        }

        //取得天氣圖區域
        private static ChartArea GetWeatherChartArea()
        {
            //用來存放圖表區域
            ChartArea WeatherArea = new ChartArea();

            //隱藏X軸標示線
            WeatherArea.AxisX.MajorGrid.Enabled = false;

            //設定X軸區間(設定1表示每1天都顯示)
            WeatherArea.AxisX.Interval = 1;

            //設定X軸的字體
            WeatherArea.AxisX.LabelStyle.Font = new System.Drawing.Font("微軟正黑體", 12);

            //設定字體大小至少為12
            WeatherArea.AxisX.LabelAutoFitMinFontSize = 12;

            //設定Y軸的最大值最小值
            WeatherArea.AxisY.Maximum = WeatherList.Select(A => A.MaxT).Max() + 3;
            WeatherArea.AxisY.Minimum = WeatherList.Select(A => A.MinT).Min() - 3;

            //設定Y軸的區間為2
            WeatherArea.AxisY.Interval = 2;

            //設定Y軸標題文字
            WeatherArea.AxisY.Title = "溫度°C";

            //設定Y軸的字體
            WeatherArea.AxisY.LabelStyle.Font = new System.Drawing.Font("微軟正黑體", 12);

            //設定Y軸標題文字的對齊方式為由左到右
            WeatherArea.AxisY.TitleAlignment = System.Drawing.StringAlignment.Far;

            //設定Y軸標題文字的文字顯示方式為水平
            WeatherArea.AxisY.TextOrientation = TextOrientation.Horizontal;

            //回傳結果
            return WeatherArea;
        }

        //取得標題和天氣氣候字串(城市)
        private static string[] GetWeatherForecast(string City)
        {
            //宣告WeatherForecast存放標題和兩天的天氣字串
            string[] WeatherForecast = new String[4];

            //主標題
            WeatherForecast[0] = City + " 一周天氣預報";

            //版權宣告
            WeatherForecast[1] = "資料來源：政府資料開放平台(中央氣象局)，網址:https://data.gov.tw/dataset/9219";

            //時間點
            DateTime DT = DateTime.Now;

            //今天的天氣預測
            WeatherForecast[3] = DT.ToString("yyyy/MM/dd") + " 氣候，白天：";
            WeatherForecast[3] += WeatherHelper.WeatherList.Where(A => A.City.Equals(City) && A.DT.Date.Equals(DT.Date) && A.DN.Equals("白天")).Select(A => A.Climate).First().ToString();
            WeatherForecast[3] += "，晚上：" + WeatherHelper.WeatherList.Where(A => A.City.Equals(City) && A.DT.Date.Equals(DT.Date) && A.DN.Equals("晚上")).Select(A => A.Climate).First() + "。";


            //明天的天氣預測
            DT = DT.AddDays(1);
            WeatherForecast[2] = DT.ToString("yyyy/MM/dd") + " 氣候，白天：";
            WeatherForecast[2] += WeatherHelper.WeatherList.Where(A => A.City.Equals(City) && A.DT.Date.Equals(DT.Date) && A.DN.Equals("白天")).Select(A => A.Climate).First().ToString();
            WeatherForecast[2] += "，晚上：" + WeatherHelper.WeatherList.Where(A => A.City.Equals(City) && A.DT.Date.Equals(DT.Date) && A.DN.Equals("晚上")).Select(A => A.Climate).First().ToString() + "。";

            //回傳結果
            return WeatherForecast;
        }

        //取得天氣預報圖形標題
        private static Title[] GetWeatherTitle(string[] WeatherForecast)
        {
            //宣告WeatherTitle用來存放圖形標題陣列
            Title[] WeatherTitle = new Title[4];

            //主標題(標題、位置、字體、顏色)
            WeatherTitle[0] = new Title(WeatherForecast[0], Docking.Top, new System.Drawing.Font("微軟正黑體", 24, System.Drawing.FontStyle.Bold), System.Drawing.Color.MediumBlue);

            //版權宣告
            WeatherTitle[1] = new Title(WeatherForecast[1], Docking.Bottom, new System.Drawing.Font("微軟正黑體", 10, System.Drawing.FontStyle.Bold), System.Drawing.Color.Black);

            //明天的天氣
            WeatherTitle[2] = new Title(WeatherForecast[2], Docking.Bottom, new System.Drawing.Font("微軟正黑體", 14, System.Drawing.FontStyle.Bold), System.Drawing.Color.Blue);
            
            //今天的天氣
            WeatherTitle[3] = new Title(WeatherForecast[3], Docking.Bottom, new System.Drawing.Font("微軟正黑體", 14, System.Drawing.FontStyle.Bold), System.Drawing.Color.Blue);

            //回傳結果
            return WeatherTitle;
        }

        //取得天氣預報圖
        private static Chart GetWeatherChart(Title[] WeatherTitle, Series[] WeatherSeries, ChartArea WeatherArea)
        {
            //宣告WeatherChart為天氣預報圖
            Chart WeatherChart = new Chart();

            //設定天氣預報圖寬長
            WeatherChart.Width = 1024;
            WeatherChart.Height = 768;

            //設定天氣預報圖底色與外觀
            WeatherChart.BackColor = System.Drawing.Color.Honeydew;
            WeatherChart.BorderSkin.SkinStyle = BorderSkinStyle.Raised;

            //新增最高溫最低溫資料，並新增天氣圖的樣式
            WeatherChart.Series.Add(WeatherSeries[0]); //最高溫
            WeatherChart.Series.Add(WeatherSeries[1]); //最低溫
            WeatherChart.ChartAreas.Add(WeatherArea); //曲線圖

            //主標題、版權宣示、明天和今天的天氣氣候
            WeatherChart.Titles.Add(WeatherTitle[0]);
            WeatherChart.Titles.Add(WeatherTitle[1]);
            WeatherChart.Titles.Add(WeatherTitle[2]);
            WeatherChart.Titles.Add(WeatherTitle[3]);

            //設定今天和明天的天氣氣候標題樣式
            for (int x = 2; x <= 3; x++)
            {
                WeatherChart.Titles[x].BackColor = System.Drawing.Color.White;
                WeatherChart.Titles[x].BorderColor = System.Drawing.Color.LightSeaGreen;
                WeatherChart.Titles[x].BorderDashStyle = ChartDashStyle.Solid;
                WeatherChart.Titles[x].BorderWidth = 1;
                WeatherChart.Titles[x].Alignment = System.Drawing.ContentAlignment.BottomLeft;
            }

            //天氣圖的圖例(最高溫和最低溫)
            WeatherChart.Legends.Add(new Legend("最高溫"));
            WeatherChart.Legends.Add(new Legend("最低溫"));
            WeatherChart.Legends[0].Docking = Docking.Top;
            WeatherChart.Legends[0].Font = new System.Drawing.Font("微軟正黑體", 12);
            WeatherChart.Legends[0].BorderDashStyle = ChartDashStyle.Solid;
            WeatherChart.Legends[0].BorderWidth = 1;
            WeatherChart.Legends[0].BorderColor = System.Drawing.Color.LightSeaGreen;
            WeatherChart.IsMapEnabled = true;

            //回傳結果
            return WeatherChart;
        }

        //建立天氣預報圖
        public static MemoryStream BuildWeatherImage(string City)
        {
            //取得標題和天氣氣候字串(城市)
            string[] WeatherForecast = GetWeatherForecast(City);

            //取得天氣預報圖形標題
            Title[] WeatherTitle = GetWeatherTitle(WeatherForecast);

            //取得天氣預報資料
            Series[] WeatherSeries = new Series[2] { GetWeatherSeries(City, "最高溫"), GetWeatherSeries(City, "最低溫") };

            //取得天氣預報區域
            ChartArea WeatherArea = GetWeatherChartArea();

            //取得天氣預報圖
            Chart WeatherChart = GetWeatherChart(WeatherTitle, WeatherSeries, WeatherArea);

            //設定WeatherImage用來存放資料到記憶體
            MemoryStream WeatherImage = new MemoryStream();

            //設定圖檔格式
            WeatherChart.ImageType = ChartImageType.Png;

            //將天氣圖放入記憶體
            WeatherChart.SaveImage(WeatherImage);
            WeatherImage.Position = 0;

            //回傳結果
            return WeatherImage;
        }
    }
}