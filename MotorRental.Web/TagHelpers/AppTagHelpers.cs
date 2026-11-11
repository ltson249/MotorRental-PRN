using System.Globalization;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace MotorRental.Web.TagHelpers;

// <status-badge status="Available" />  ->  <span class="badge text-bg-success">San sang</span>
[HtmlTargetElement("status-badge")]
public class StatusBadgeTagHelper : TagHelper
{
    public string Status { get; set; } = "";

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var (css, text) = Status switch
        {
            "Available" => ("success", "Sẵn sàng"),
            "Rented" => ("warning", "Đang cho thuê"),
            "Maintenance" => ("secondary", "Bảo trì"),
            "Active" => ("primary", "Đang thuê"),
            "Completed" => ("success", "Hoàn tất"),
            "Cancelled" => ("danger", "Đã hủy"),
            "Admin" => ("danger", "Admin"),
            "Staff" => ("info", "Nhân viên"),
            _ => ("dark", Status)
        };
        output.TagName = "span";
        output.Attributes.SetAttribute("class", $"badge text-bg-{css}");
        output.Content.SetContent(text);
    }
}

// <money value="@x.Price" />  ->  100.000 d
[HtmlTargetElement("money")]
public class MoneyTagHelper : TagHelper
{
    public decimal Value { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        output.Content.SetContent(Value.ToString("N0", new CultureInfo("vi-VN")) + " ₫");
    }
}
