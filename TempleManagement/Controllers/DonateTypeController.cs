using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;
using TempleManagement.Exceptions;
using TempleManagement.Models;
using TempleManagement.Models.DBManager;

namespace TempleManagement.Controllers
{
    public class DonateTypeController : Controller
    {
        public async Task<IActionResult> Index()
        {
            DonateType_DBManager dbmanager = new DonateType_DBManager();
            List<DonateType> infos = await dbmanager.get_donatetype();

            return View(infos);
        }

        public async Task<IActionResult> Modify_page()
        {
            DonateType_DBManager dbmanager = new DonateType_DBManager();
            List<DonateType> infos = await dbmanager.get_donatetype();
            Debug.WriteLine("----------modify_button-------------");
            Debug.WriteLine(JsonSerializer.Serialize(infos));

            return Json(new {success=true, infos=infos, message="請修改!"});
        }

        public async Task<IActionResult> Modify_type(List<DonateType> donateTypes)
        {
            if (!ModelState.IsValid) //驗證 Post 來的資料

            {
                Debug.WriteLine($"ModelState: {JsonSerializer.Serialize(donateTypes)}");
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

            DonateType_DBManager dbmanager = new DonateType_DBManager();
            
            Debug.WriteLine("----------Modify_type-------------");
            Debug.WriteLine(JsonSerializer.Serialize(donateTypes));

            try
            {
                await dbmanager.modify_donatetype(donateTypes);
            }
            catch (NeedNameChineseException )
            {
                return Json(new { success = false, message = "請填寫 [類別] ! 否則請刪除!" });
            }
            catch(NeedPriceException)
            {
                return Json(new { success = false, message = "請填寫 [價格] ! 否則請刪除" });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "其他錯誤!" });
            }



            return Json(new { success = true,  message = "修改成功!" });
        }
    }
}
