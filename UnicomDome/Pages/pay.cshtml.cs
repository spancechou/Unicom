using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PuppeteerSharp;

namespace UnicomDome.Pages
{
    public class payModel : PageModel
    {
        public async Task OnGetAsync()
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

            await page.GoToAsync("https://upay.10010.com/jf_wxgz");
            await page.EvaluateFunctionHandleAsync("() =>{Object.defineProperty(navigator, 'webdriver', {get: () => false});}");
            await page.EvaluateFunctionOnNewDocumentAsync("() =>{ window.navigator.chrome = { runtime: {},  }; }");
            await page.EvaluateFunctionOnNewDocumentAsync("() =>{ Object.defineProperty(navigator, 'languages', { get: () => ['en-US', 'en'] }); }");
            await page.EvaluateFunctionOnNewDocumentAsync("() =>{ Object.defineProperty(navigator, 'plugins', { get: () => [1, 2, 3, 4, 5,6], }); }");
            Console.WriteLine(await page.EvaluateExpressionAsync("navigator.webdriver"));
            //填写手机号码
            string phone = "13113075869";
            await page.ClickAsync("#number");
            await page.TypeAsync("#number", phone);
            Thread.Sleep(1000);
            //点击充值按钮
            await page.ClickAsync("#cardlist > section > div.mobile-cardlist.cardlista > a:nth-child(1)");
            ElementHandle slideBtn = null;
            try
            {
                slideBtn = await page.WaitForSelectorAsync("#nc_1_n1t", new WaitForSelectorOptions { Timeout = 3 * 1000 });
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }
            if (slideBtn != null)
            {
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
            }

            var channel = await page.WaitForSelectorAsync("[channelcode='alipaywap']");

            await channel.ClickAsync();

            var submit = await page.WaitForSelectorAsync("body > div.mask.confirmPay > section > div.btnPd > button");

            await submit.ClickAsync();

        }
    }
}
