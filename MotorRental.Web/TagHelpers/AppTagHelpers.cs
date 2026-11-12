using System.Globalization;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace MotorRental.Web.TagHelpers;

// <status-badge status="Available" /> -> Sleek modern badge with dot and icon
[HtmlTargetElement("status-badge")]
public class StatusBadgeTagHelper : TagHelper
{
    public string Status { get; set; } = "";

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var (bgClass, icon, text) = Status switch
        {
            "Available" => ("badge-status-available", "bi-check-circle-fill", "Sẵn sàng"),
            "Rented" => ("badge-status-rented", "bi-key-fill", "Đang thuê"),
            "Maintenance" => ("badge-status-maint", "bi-tools", "Bảo dưỡng"),
            "Active" => ("badge-status-rented", "bi-hourglass-split", "Đang thuê"),
            "Completed" => ("badge-status-available", "bi-check2-all", "Đã hoàn tất"),
            "Cancelled" => ("badge-status-cancel", "bi-x-circle-fill", "Đã hủy"),
            "InProgress" => ("badge-status-maint", "bi-wrench-adjustable", "Đang sửa chữa"),
            "Admin" => ("badge-role-admin", "bi-shield-shaded", "Quản trị viên"),
            "Staff" => ("badge-role-staff", "bi-person-badge", "Nhân viên"),
            _ => ("bg-secondary text-white", "bi-info-circle", Status)
        };

        output.TagName = "span";
        output.Attributes.SetAttribute("class", $"badge rounded-pill fw-medium px-2 py-1 {bgClass} d-inline-flex align-items-center gap-1");
        output.Content.SetHtmlContent($"<i class=\"bi {icon}\"></i> {text}");
    }
}

// <category-badge category="Xe tay ga" />
[HtmlTargetElement("category-badge")]
public class CategoryBadgeTagHelper : TagHelper
{
    public string? Category { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var cat = Category ?? "Xe số";
        var icon = cat switch
        {
            "Xe tay ga" => "bi-scooter",
            "Xe côn tay" => "bi-lightning-charge-fill",
            "Xe điện" => "bi-battery-charging",
            _ => "bi-bicycle"
        };

        output.TagName = "span";
        output.Attributes.SetAttribute("class", "badge bg-light text-dark border px-2 py-1");
        output.Content.SetHtmlContent($"<i class=\"bi {icon} text-primary me-1\"></i>{cat}");
    }
}

// <money value="@x.Price" /> -> 100.000 ₫
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
