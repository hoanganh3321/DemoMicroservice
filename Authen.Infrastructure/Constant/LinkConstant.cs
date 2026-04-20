using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authen.Infrastructure.Constant
{
    public class LinkConstant
    {
        public static readonly string frontendBaseUri = $"https://localhost:3000";
        public static readonly string backendBaseUri = $"https://localhost:7206";


        public static UriBuilder UriBuilder(string controller, string path, string userId, string token)
        {
            // Nếu là link xác nhận email -> điều hướng tới FE route /confirm-email
            if (controller == "User" && path == "confirm-email")
            {
                var fe = new UriBuilder(frontendBaseUri)
                {
                    Path = "confirm-email",
                    Query = $"userId={userId}&token={Uri.EscapeDataString(token)}"
                };
                return fe;
            }

            // Nếu là link đặt lại mật khẩu -> điều hướng tới FE route /reset-password
            if (controller == "User" && path == "reset-password")
            {
                var fe = new UriBuilder(frontendBaseUri)
                {
                    Path = "reset-password",
                    Query = $"userId={userId}&token={Uri.EscapeDataString(token)}"
                };
                return fe;
            }

            // Mặc định: trỏ tới BE API (giữ nguyên hành vi cũ cho các luồng khác như reset-password)
            var be = new UriBuilder(backendBaseUri)
            {
                Path = $"api/{controller}/{path}",
                Query = $"userId={userId}&token={Uri.EscapeDataString(token)}"
            };
            return be;
        }
    }
}
