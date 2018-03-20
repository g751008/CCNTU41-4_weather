using ccntu41_4_weather.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace ccntu41_4_weather.Controllers
{
    public class WeatherController:Controller
    {
        public ActionResult Index()
        {
            //建立天氣預報資料清單
            WeatherHelper.BuildWeatherList();

            //從資料清單取得主要縣市的名單
            ViewData["City"] = WeatherHelper.WeatherList.Select(A => A.City).Distinct().ToList();

            return View();
        }

        //各縣市天氣圖
        [HttpGet]
        public ActionResult Chart(string City)
        {
            //建立並回傳縣市天氣圖。不需要新增部分檢視頁面。
            return new FileStreamResult(WeatherHelper.BuildWeatherImage(City), "image/png");
        }
    }
}