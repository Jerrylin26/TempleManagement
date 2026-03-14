using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;
using TempleManagement.Models;
using TempleManagement.Models.DBManager;

namespace TempleManagement.Controllers
{
    public class AccountManagementController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }



        public async Task<IActionResult> SignUp(Admin info)
        {
            if (!ModelState.IsValid) //驗證 Post 來的資料

            {
                Debug.WriteLine($"ModelState: {JsonSerializer.Serialize(info)}");
                foreach (var key in ModelState.Keys)
                {
                    var errors = ModelState[key].Errors;
                    foreach (var error in errors)
                    {
                        Debug.WriteLine($"欄位: {key}, 錯誤: {error.ErrorMessage}");
                    }
                }

                return Json(new { success = false, message = "資料驗證失敗" });
            }

            Debug.WriteLine($"NewAccount_info_check:{JsonSerializer.Serialize(info)}");

            try
            {
                AccountManagement_DBManager dbManager = new AccountManagement_DBManager();
                Debug.WriteLine("AccountManagement_DBManager");
                await dbManager.newAccount(info);


                return Json(new { success = true, name = info.Name });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("catch error");
                // 可記錄錯誤 log
                return Json(new { success = false, message = ex.Message });
            }
        }


        public async Task<IActionResult> Login(Admin info)
        {
            if (!ModelState.IsValid) //驗證 Post 來的資料

            {
                Debug.WriteLine($"ModelState: {JsonSerializer.Serialize(info)}");
                foreach (var key in ModelState.Keys)
                {
                    var errors = ModelState[key].Errors;
                    foreach (var error in errors)
                    {
                        Debug.WriteLine($"欄位: {key}, 錯誤: {error.ErrorMessage}");
                    }
                }

                return Json(new { success = false, message = "資料驗證失敗" });
            }

            Debug.WriteLine($"Login_check:{JsonSerializer.Serialize(info)}");

            try
            {
                AccountManagement_DBManager dbManager = new AccountManagement_DBManager();
                Debug.WriteLine("AccountManagement_DBManager start");
                List<Admin> infos = await dbManager.getAccount(info);

                if (infos.Count == 1)
                {
                    return Json(new { success = true, name = infos[0].Name });
                }


                return Json(new { success = true, message = "錯誤輸入帳密" });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("catch error login");
                // 可記錄錯誤 log
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
