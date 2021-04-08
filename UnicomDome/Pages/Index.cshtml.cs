using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Flurl.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PuppeteerSharp;
using PuppeteerSharp.Mobile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UnicomDome.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            try
            {
                
                
                
                string phone = "13113075869";
                await "https://upay.10010.com/npfwap/NpfMob/usernumber/get4gInfo"
                         .WithHeaders(new { Referer = "https://upay.10010.com/jf_wxgz" })
                         .WithCookies(out var cookies)
                         .PostUrlEncodedAsync(new { phoneNo = phone });

                var offeRateOrder = await "https://upay.10010.com/npfwap/NpfMobAppQuery/feeSearch/OfferateOrder"
                                        .WithHeaders(new { Referer = "https://upay.10010.com/jf_wxgz" })
                                        .WithCookies(cookies).PostAsync()
                                        .ReceiveString();
                var needCodeResponse = await "https://upay.10010.com/npfwap/NpfMob/needCode?channelType=307&channelKey=wxgz"
                                .WithHeaders(new { Referer = "https://upay.10010.com/jf_wxgz" })
                                .WithCookies(cookies)
                                .GetAsync();

                for (int i = 0; i < needCodeResponse.Cookies.Count; i++)
                {
                    cookies.AddOrReplace(needCodeResponse.Cookies[i]);
                }
                var nedCode =await needCodeResponse.GetStringAsync();

                var aliToken = ""; 
                var aliSessionId = "";
                var aliSig = "";
                if (nedCode.Trim() == "yes")
                {
                    var version = await new BrowserFetcher().DownloadAsync(615489);
                    var browser = await Puppeteer.LaunchAsync(new LaunchOptions
                    {
                        Headless = false,
                        Args = new string[]
                        {
                        "--no-sandbox",
                        "--disable-setuid-sandbox"
                        },
                        ExecutablePath = version.ExecutablePath,
                        Devtools = true
                    });
                    var page = await browser.NewPageAsync();
                    await page.GoToAsync("https://localhost:5001/VerificationCode/index");
                    await page.EvaluateFunctionHandleAsync("() =>{Object.defineProperty(navigator, 'webdriver', {get: () => false});}");
                    await page.EvaluateFunctionOnNewDocumentAsync("() =>{ window.navigator.chrome = { runtime: {},  }; }");
                    await page.EvaluateFunctionOnNewDocumentAsync("() =>{ Object.defineProperty(navigator, 'languages', { get: () => ['en-US', 'en'] }); }");
                    await page.EvaluateFunctionOnNewDocumentAsync("() =>{ Object.defineProperty(navigator, 'plugins', { get: () => [1, 2, 3, 4, 5,6], }); }");
                    Console.WriteLine(await page.EvaluateExpressionAsync("navigator.webdriver"));
                    var slideBtn = await page.WaitForSelectorAsync("#nc_1_n1t", new WaitForSelectorOptions { Timeout = 3 * 1000 });
                    var rect = await slideBtn.BoundingBoxAsync();
                    var left = rect.X + 10;
                    var top = rect.Y + 10;
                    var mouse = page.Mouse;

                    await mouse.MoveAsync(left, top);
                    await page.Touchscreen.TapAsync(left, top);
                    await mouse.DownAsync();

                    var startTime = DateTime.Now;

                    await mouse.MoveAsync(left + 800, top, new PuppeteerSharp.Input.MoveOptions { Steps = 30 });
                    await page.Touchscreen.TapAsync(left + 800, top);

                    Console.WriteLine(DateTime.Now - startTime);
                    await mouse.UpAsync();

                    var success = await page.WaitForSelectorAsync(".yes", new WaitForSelectorOptions { Timeout = 3000 });


                    string content = await page.GetContentAsync();
                    
                    var parser = new HtmlParser();
                    var document = await parser.ParseDocumentAsync(content);

                    aliToken = (document.GetElementById("aliToken") as IHtmlInputElement).GetAttribute("sms");
                    aliSessionId = (document.GetElementById("aliSessionId") as IHtmlInputElement).GetAttribute("sms");
                    aliSig = (document.GetElementById("aliSig") as IHtmlInputElement).GetAttribute("sms");
                }

                var response = await "https://upay.10010.com/npfwap/NpfMob/needdubbo/needDubboCheck?phoneNo=13113075869&amountMoney=200&channelKey=wxgz"
                                    .WithHeaders(new { Referer = "https://upay.10010.com/jf_wxgz" })
                                    .WithCookies(cookies)
                                    .GetStringAsync();

                 var dubbocheck = JsonConvert.DeserializeObject<NeedDubboCheckInput>(response);

                var action = await "https://upay.10010.com/npfwap/NpfMob/mobWapBankCharge/bankChargePayChannel.action"
                                    .WithHeaders(new { Referer = "https://upay.10010.com/jf_wxgz" })
                                    .WithCookies(cookies)
                                    .PostUrlEncodedAsync(new
                                    {
                                        phoneNo = phone,
                                        areaCode = "",
                                        provinceCode = "051",
                                        cityCode = "580",
                                        bussineTypeInput = "0",
                                        payAmount = "200",
                                        numberType = "",
                                        channelType = "307",
                                        channelKey = "wxgz",
                                        bussineType = "06",
                                        netAccount = "",
                                        browserVersion = "",
                                        ticketNew = "ticket",
                                        token = aliToken,
                                        sessionid = aliSessionId,
                                        sig = aliSig,
                                        joinSign = "",
                                        username = "",
                                        state = dubbocheck.Secstate
                                    }).ReceiveString();

                var chargeCheck = await "https://upay.10010.com/npfwap/NpfMob/mobWapBankCharge/wapBankChargeCheck.action"
                                        .WithHeaders(new { Referer = "https://upay.10010.com/jf_wxgz" })
                                        .WithCookies(cookies)
                                        .PostUrlEncodedAsync(new
                                        {
                                            phoneNo = phone,
                                            areaCode = "",
                                            provinceCode = "051",
                                            cityCode = "580",
                                            bussineTypeInput = "0",
                                            payAmount = 200,
                                            cardValue = "20000",
                                            cardValueCode = "10",
                                            userChooseMode = "0",
                                            reserved1 = false,
                                            numberType = "",
                                            channelType = 307,
                                            channelKey = "wxgz",
                                            bussineType = "06",
                                            netAccount = "",
                                            payMethod = "",
                                            internetThingsNumberFlag = "0",
                                            pointNumber = "",
                                            browserVersion = "",
                                            activityType = "",
                                            offerate = "1",
                                            offerateId = "",
                                            orgCode = "03",
                                            channelCode = "alipaywap",
                                            emergencyContact = "",
                                            ticketNo = "",
                                            reserved2 = "",
                                            ticketNew = "ticket",
                                            numberAttribution = "",
                                            urlSign = "",
                                            msgTimeStamp = "",
                                            serviceNo = "",
                                            natCode = "",
                                            saleChannel = "null",
                                            deviceId = "null",
                                            model = "null",
                                            vipCode = "",
                                            joinSign = "",
                                            presentActivityId = "",
                                            sinoUnionShortAddr = "",
                                            token = aliToken,
                                            sessionid = aliSessionId,
                                            sig = aliSig,
                                            state = dubbocheck.Secstate
                                        }).ReceiveJson<NeedDubboCheckInput>();

                var result = await "https://upay.10010.com/npfwap/NpfMob/mobWapBankCharge/wapBankChargeSubmit.action"
                                .WithHeaders(new { Referer = "https://upay.10010.com/jf_wxgz" })
                                .WithCookies(cookies)
                                .PostUrlEncodedAsync(new
                                {
                                    state = chargeCheck.Secstate,
                                    phoneNo = phone,
                                    provinceCode = "051",
                                    browserVersion = "",
                                    channelKey = "wxgz",
                                    ticketNew = "ticket"
                                }).ReceiveString();

            }
            catch (Exception ex)
            {

            }
            //await page.EmulateAsync(Puppeteer.Devices[DeviceDescriptorName.IPhone8Plus]);
            //await page.GoToAsync("https://upay.10010.com/jf_wxgz");
            //await page.SetCookieAsync(new CookieParam { Name = "key", Value = "value", SameSite = SameSite.None, Secure = true });
            //string phone = "13113075869";
            //await page.ClickAsync("#number");
            //await page.TypeAsync("#number", phone);
            //Thread.Sleep(3000);
            //await page.ClickAsync("#cardlist > section > div.mobile-cardlist.cardlista > a:nth-child(1)");
            //await browser.CloseAsync();
            //await browser.DisposeAsync();
            //await page.EvaluateExpressionAsync($"document.getElementById('number').value={phone}");
            //var input = await page.("#number", "13113075869");
            //await input.EvaluateFunctionAsync("#number", "131125836985");
            //await page.ScreenshotAsync(outputFile);

        }
    }
}
